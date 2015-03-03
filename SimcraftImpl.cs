#region Honorbuddy

#endregion

#region System

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Bots.DungeonBuddy.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Loaders;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
using System.Windows.Media;
#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        public delegate String RetrieveSpellDelegate();

        //<summary>_conditonSpell is the true name of the spell, non tokenized</summary>
        private String _conditionSpell;

        public WoWUnit CycleTarget;
        public char NameCount = Convert.ToChar(65);

        private List<Stopwatch> apparitions = new List<Stopwatch>();
        private const int apparition_flight_time = 5000; 

        public void toggle_hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            hotkeyVariables[name] = !hotkeyVariables[name];
            Lua.DoString("_G[\"sc_" + name + "\"] = " + hotkeyVariables[name] + "; print('" + name + " => " + hotkeyVariables[name] + "');");//true then _G[\""+name+"\"] = nil; print('Cds enabled') else _G[\""+name+"\"] = true; print('Cds disabled') end");
            //Logging.Write("Toggled " + name + " to :" + hotkeyVariables[name]);
        }

        public bool hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            return hotkeyVariables[name];
        }

        public int incanters_flow_dir
        {
            get { return buff.incanters_flow.dir; }
        }

        public WoWUnit prismatic_crystal
        {
            get
            {
                try
                {
                    var m = Me.Minions.FirstOrDefault(ret => ret.Name.Equals("Prismatic Crystal"));
                    if (m == default(WoWUnit)) return Me;
                    return m;
                }
                catch (Exception e)
                {
                    Write(e.ToString());
                    return Me;
                }

            }
        }

        public int desired_targets
        {
            get { return 1; }
        }

        public int mind_harvest
        {
            get { return conditionUnit.MindHarvest() ? 1 : 0; }
        }
        public bool moving
        {
            get { return Me.IsMoving; }
        }
        public int shadowy_apparitions_in_flight
        {
            get
            {
                var naps = apparitions.Where(ret => ret.ElapsedMilliseconds >= apparition_flight_time);
                apparitions.RemoveAll(ret => naps.Contains(ret));
                return apparitions.Count();
            }
        }

        public WoWUnit current_target
        {
            get
            {
                return conditionUnit;

            }
        }

        public bool miss_react
        {

            get { return true; }
        }

        public bool cooldown_react
        {
            get
            {
                if (_conditionSpell == "Mind Blast")
                {
                    return buff.shadowy_insight.up;
                }
                return false;
            }
        }

        public bool natural_shadow_word_death_range
        {
            get { return target.health.pct < 20; }
        }

        public int active_enemies
        {
            get { return actives.Count(); }
        }
        public int demonic_fury
        {
            get
            {
                var be = Lua.GetReturnVal<int>("UnitPower(\"player\", SPELL_POWER_DEMONIC_FURY)", 0);
                return be;
            }
        }

        public int shadow_orb
        {
            get
            {
                var be = Lua.GetReturnVal<int>("UnitPower(\"player\", SPELL_POWER_SHADOW_ORBS)", 0);
                return be;
            }
        }

        public double distance
        {
            get { return conditionUnit.Distance; }
        }
        private int o_soul_shard = 0;
        private Stopwatch SoulShardTimer = new Stopwatch();

        public MagicValueType ptr
        {
            get
            {
                return new MagicValueType(false);
            }
        }

        public bool active
        {
            get { return pet.children[Tokenize(_conditionSpell)].active; }
        }

        public int soul_shard
        {
            get
            {
                int ss = (int)Me.CurrentSoulShards;
                if (ss > o_soul_shard)
                {
                    SoulShardTimer.Restart();
                }
                o_soul_shard = ss;
                return ss;
            }
        }

        public static spell_data_t DBGetSpell(String name)
        {
            name = Tokenize(name);

            if (dbc.Spells.RelationContainsKey(name))
            {
                return dbc.Spells[dbc.Spells[name].Min()];
            }
            throw new MissingMemberException("DBGetSpell", name);
        }

        public static bool DBHasSpell(String name)
        {
            var n = Tokenize(name);
            //if (n == "invoke_xuen") n = "invoke_xuen_the_white_tiger";
            return dbc.Spells.RelationContainsKey(n);
        }

        public static spell_data_t DBGetClassSpell(String name)
        {
            name = Tokenize(name);

            if (dbc.ClassSpells.ContainsKey(name))
            {
                return dbc.Spells[dbc.ClassSpells[name]];
            }
            if (dbc.Spells.RelationContainsKey(name))
            {
                return dbc.Spells[dbc.Spells[name].Min()];
            }
            throw new MissingMemberException("DBGetClassSpell", name);
        }

        public static bool DBHasClassSpell(String name)
        {
            var n = Tokenize(name);

            return dbc.ClassSpells.ContainsKey(n);
        }

        public static spell_data_t DBGetTalentSpell(String name)
        {
            //var n = Tokenize(name);
            return DBGetClassSpell(name);
        }



        public bool in_flight_to_target = false;
        public bool in_flight = false;

        public double charges_fractional
        {
            get
            {
                var sp = spell[DBGetSpell(_conditionSpell)].charges_fractional;
                return sp;
            }
        }

        public double travel_time
        {
            get
            {
                return (conditionUnit.Distance - conditionUnit.CombatReach) / DBGetSpell(_conditionSpell).speed;
            }
        }

        public double spell_haste
        {
            get { return stat.spell_haste; }
        }

        public double mastery_value
        {
            get { return stat.mastery_value; }
        }

        public bool shard_react
        {
            get
            {
                int ss = soul_shard;
                return (SoulShardTimer.IsRunning && SoulShardTimer.ElapsedMilliseconds < 2000);
            }
        }

        public double burning_ember
        {
            get
            {
                var be = Lua.GetReturnVal<int>("UnitPower(\"player\", SPELL_POWER_BURNING_EMBERS, true)", 0);
                return ((double)be) / 10;
            }
        }

        public double tick_time
        {
            get
            {
                return debuff[_conditionSpell].tick_time;
            }
        }

        public IEnumerable<WoWUnit> actives
        {
            get
            {
                var _class = StyxWoW.Me.Class;
                var ranged = (_class == WoWClass.Mage || _class == WoWClass.Hunter || _class == WoWClass.Priest ||
                              _class == WoWClass.Warlock ||
                              (_class == WoWClass.Druid && StyxWoW.Me.Specialization == WoWSpec.DruidBalance));

                if (ranged) return UnfriendlyUnitsNearTarget(8f);
                return UnfriendlyUnitsNearMe(5f);
            }
        }

        public double eclipse_change
        {
            get
            {
                double eclipse_amount = eclipse_energy.current;
                double time_to_next_lunar, time_to_next_solar, eclipse_ch;
                double M_PI = Math.PI;

                double phi_lunar = Math.Asin(100.0 / 105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2 * M_PI / 40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = Lua.GetReturnVal<String>("direction = GetEclipseDirection(); return direction;", 0);
                double phi;

                if (dir.Equals("moon"))
                    phi = 2 * M_PI - Math.Asin(eclipse_amount / 105) + M_PI;
                else
                    phi = 2 * M_PI + Math.Asin(eclipse_amount / 105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi % (2 * M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;


                eclipse_ch = (M_PI - (phi % M_PI)) / omega / 1000;

                //Logging.Write("energy {3} change: {0} next_solar: {1} next_lunar: {2} phi: {4}",eclipse_ch,time_to_next_solar,time_to_next_lunar,eclipse_energy.current,phi);

                return eclipse_ch;
            }
        }

        public double lunar_max
        {
            get
            {
                double eclipse_amount = eclipse_energy.current;
                double time_to_next_lunar, time_to_next_solar, eclipse_ch;
                double M_PI = Math.PI;

                double phi_lunar = Math.Asin(100.0 / 105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2 * M_PI / 40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = Lua.GetReturnVal<String>("direction = GetEclipseDirection(); return direction;", 0);
                double phi;

                if (dir.Equals("moon"))
                    phi = 2 * M_PI - Math.Asin(eclipse_amount / 105) + M_PI;
                else
                    phi = 2 * M_PI + Math.Asin(eclipse_amount / 105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi % (2 * M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;


                eclipse_ch = (M_PI - (phi % M_PI)) / omega / 1000;

                return time_to_next_lunar;
            }
        }
        public double solar_max
        {
            get
            {
                double eclipse_amount = eclipse_energy.current;
                double time_to_next_lunar, time_to_next_solar, eclipse_ch;
                double M_PI = Math.PI;

                double phi_lunar = Math.Asin(100.0 / 105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2 * M_PI / 40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = Lua.GetReturnVal<String>("direction = GetEclipseDirection(); return direction;", 0);
                double phi;

                if (dir.Equals("moon"))
                    phi = 2 * M_PI - Math.Asin(eclipse_amount / 105) + M_PI;
                else
                    phi = 2 * M_PI + Math.Asin(eclipse_amount / 105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi % (2 * M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2 * M_PI) % (2 * M_PI)) / omega / 1000;


                eclipse_ch = (M_PI - (phi % M_PI)) / omega / 1000;

                return time_to_next_solar;
            }
        }


        public double time_to_die
        {
            get { return target.time_to_die; }
        }

        public bool position_front
        {
            get { return !Me.IsBehind(conditionUnit); }
        }

        public WoWUnit clickUnit { get; set; }

        public int anticipation_charges
        {
            get { return buff["Anticipation"].stack; }
        }

        public MagicValueType remains
        {
            get { return debuff[_conditionSpell].remains; }
        }

        public double duration
        {
            get { return spell[_conditionSpell].duration; }
        }

        public int level { get { return Me.Level; } }

        public bool melee_range
        {
            get { return Me.IsWithinMeleeRangeOf(conditionUnit); }
        }

        public bool facing
        {
            get { return target.facing; }
        }

        public double execute_time
        {
            get { return spell[_conditionSpell].execute_time; }
        }

        public double cast_time
        {
            get { return execute_time; }
        }

        public static bool SpellIsTargeting
        {
            get
            {
                return Lua.GetReturnVal<Boolean>(
                    "return SpellIsTargeting()", 0);
            }
        }

        public bool damage_enabled
        {
            get { return !_damageEnabled; }
        }

        public bool aoe_enabled
        {
            get { return !_aoeEnabled; }
        }

        public bool cooldowns_enabled
        {
            get { return !_cdsEnabled; }
        }

        public bool ticking
        {
            get { return debuff[_conditionSpell].ticking; }
        }

        public double channel_time
        {
            get { return spell[_conditionSpell].channel_time; }
        }

        public double recharge_time
        {
            get { return spell[_conditionSpell].recharge_time; }
        }

        public int charges
        {
            get
            {
                return spell[_conditionSpell].charges;
            }
        }

        public double cast_regen
        {
            get
            {
                var cTime = spell[_conditionSpell].cast_time;
                return focus.cast_regen(cTime > 0 ? cTime : gcd.nat);
            }
        }

        public bool sync(String spell)
        {
            return true;
        }

        public Composite CallActionList(String name, CanRunDecoratorDelegate del, String Reason)
        {
            return new Decorator(del ?? (ret => true), actions[name]);
        }

        public Composite CallActionList(String name, CanRunDecoratorDelegate del = null)
        {
            return new Decorator(del ?? (ret => true), actions[name]);
        }

        public Composite CallActionList(String name, String Reason)
        {
            CanRunDecoratorDelegate del = null;
            return new Decorator(del ?? (ret => true), actions[name]);
        }


        public char conditionName = 'A';
        private Dictionary<char, Stopwatch> line_cds = new Dictionary<char, Stopwatch>();

        public bool line_cd(double seconds)
        {
            if (!line_cds.ContainsKey(conditionName))
                line_cds[conditionName] = new Stopwatch();

            var b = !line_cds[conditionName].IsRunning || line_cds[conditionName].ElapsedMilliseconds / 1000 > seconds;
            //line_cds[conditionName].Restart();
            return b;
        }

        public void start_line_cd()
        {
            if (!line_cds.ContainsKey(conditionName))
                line_cds[conditionName] = new Stopwatch();
            line_cds[conditionName].Restart();
        }

        private Regex shorthand = new Regex("[^a-z_]");

        public bool pyro_chain = false;

        public Composite StartPyroChain(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator((_ret) =>
            {

                var r = d(_ret) && moving;
                LogDebug("start_pyro_chain" + " " + reason + " => " + r);
                return r;
            }, new Action(delegate(object context) { pyro_chain = true; return RunStatus.Failure; }));
        }

        public Composite StopPyroChain(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator((_ret) =>
            {

                var r = d(_ret) && moving;
                LogDebug("stop_pyro_chain" + " " + reason + " => " + r);
                return r;
            }, new Action(delegate(object context) { pyro_chain = false; return RunStatus.Failure; }));
        }

        public Composite Wait(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator((_ret) =>
            {

                var r = d(_ret) && moving;
                LogDebug("wait" + " " + reason + " => " + r);
                return r;
            }, new ActionAlwaysSucceed());
        }

        public Composite CycleTargets(String _spell, UnitCriteriaDelegate criteria, String Reason = "")
        {
            NameCount++;
            if (_spell.Contains(":")) _spell = _spell.Split(':')[1];
            _spell = this.spell.ResolveName(_spell).name;

            return new NamedComposite("" + NameCount, _spell,
                new Action(delegate
                {
                    conditionName = NameCount;
                    _conditionSpell = _spell;
                    foreach (var w in actives)
                    {
                        conditionUnit = w;
                        if (criteria(w))
                        {
                            CycleTarget = w;
                            return RunStatus.Failure;
                        }
                        ;
                    }
                    CycleTarget = null;
                    return RunStatus.Failure;
                }),
                new Decorator(ret => CycleTarget != null,
                    new Action(delegate
                    {
                        if (CastSpell(_spell, CycleTarget, 3, Reason))
                        {
                            LogDebug(_spell + " " + Reason + " => SUCCESS!");
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite MovingCast(String spell, CanRunDecoratorDelegate del = null, WoWUnit _target = null,
            String Reason = "")
        {
            NameCount++;
            if (spell.Contains(":")) spell = spell.Split(':')[1];

            //if (spell.Equals("wait")) return new Decorator(del, new ActionAlwaysSucceed());

            spell = this.spell.ResolveName(spell).name;
            //Logging.Write(this.spell.ResolveName(spell));
            return new NamedComposite("" + NameCount, spell,
                new Action(delegate
                {
                    //Logging.Write(spell + " on " +NameCount);
                    conditionName = NameCount;
                    _conditionSpell = spell;
                    if (_target == null || _target == default(WoWUnit)) conditionUnit = Me.CurrentTarget;
                    else conditionUnit = _target;
                    return RunStatus.Failure;
                }),
                new Decorator((_ret) =>
                {
                    var d = del ?? (ret => true);
                    try
                    {
                        var r = d(_ret) && moving;
                        LogDebug(spell + " " + Reason + " => " + r);
                        return r;
                    }
                    catch (Exception e)
                    {
                        SimcraftImpl.Write(e.ToString());
                        return false;
                    }
                },
                    new Action(delegate
                    {

                        clickUnit = conditionUnit;
                        if (CastSpell(spell, conditionUnit is ClusterUnit ? Me.CurrentTarget : conditionUnit, 3, Reason))
                        {
                            LogDebug(spell + " " + Reason + " => SUCCESS!");
                            start_line_cd();
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }



        public Composite Cast(String spell, CanRunDecoratorDelegate del = null, WoWUnit _target = null,
            String Reason = "")
        {
            NameCount++;
            if (spell.Contains(":")) spell = spell.Split(':')[1];

            //if (spell.Equals("wait")) return new Decorator(del, new ActionAlwaysSucceed());

            spell = this.spell.ResolveName(spell).name;
            //Logging.Write(this.spell.ResolveName(spell));
            return new NamedComposite("" + NameCount, spell,
                new Action(delegate
                {
                    //Logging.Write(spell + " on " +NameCount);
                    conditionName = NameCount;
                    _conditionSpell = spell;
                    if (_target == null || _target == default(WoWUnit)) conditionUnit = Me.CurrentTarget;
                    else conditionUnit = _target;
                    return RunStatus.Failure;
                }),
                new Decorator((_ret) =>
                {
                    var d = del ?? (ret => true);
                    try
                    {
                        var r = d(_ret);
                        LogDebug(spell + " " + Reason + " => " + r);
                        return r;
                    }
                    catch (Exception e)
                    {
                        SimcraftImpl.Write(conditionName + " " + spell + " " + e.ToString());
                        return false;
                    }
                },
                    new Action(delegate
                    {

                        clickUnit = conditionUnit;
                        if (CastSpell(spell, conditionUnit is ClusterUnit ? Me.CurrentTarget : conditionUnit, 3, Reason))
                        {
                            LogDebug(spell + " " + Reason + " => SUCCESS!");
                            start_line_cd();
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }


        public Composite MovingCast(String spell, String r)
        {
            return MovingCast(spell, null, null, r);
        }

        public Composite MovingCast(String spell, CanRunDecoratorDelegate del, String r)
        {
            return MovingCast(spell, del, null, r);
        }

        public Composite MovingCast(String spell, WoWUnit _target)
        {
            return MovingCast(spell, null, _target);
        }

        public Composite MovingCast(String spell)
        {
            return MovingCast(spell, null, null);
        }

        public Composite Cast(String spell, String r)
        {
            return Cast(spell, null, null, r);
        }

        public Composite Cast(String spell, CanRunDecoratorDelegate del, String r)
        {
            return Cast(spell, del, null, r);
        }

        public Composite Cast(String spell, WoWUnit _target, String Reason = "")
        {
            return Cast(spell, null, _target, Reason);
        }



        public Composite Cast(String spell)
        {
            return Cast(spell, null, null, "");
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public Composite UseItem(int id, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var item = Me.Inventory.Equipped.PhysicalItems.FirstOrDefault(it => it.ItemInfo.Id == id);

                    if (item == default(WoWItem)) return RunStatus.Failure; ;
                    if (item.Cooldown <= 0) item.Use();
                    return RunStatus.Failure;
                }));
        }

        public Composite UseItem(int id, String Reason = "")
        {
            CanRunDecoratorDelegate del = null;
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var item = Me.Inventory.Equipped.PhysicalItems.FirstOrDefault(it => it.ItemInfo.Id == id);

                    if (item == default(WoWItem)) return RunStatus.Failure; ;
                    if (item.Cooldown <= 0) item.Use();
                    return RunStatus.Failure;
                }));
        }

        public String PotionName = "";

        public MagicValueType PotionCooldown
        {
            get
            {
                var item = Me.BagItems.FirstOrDefault(ret => ret.Name.Equals(PotionName));
                //Logging.Write(item.ToString() + " " + item.CooldownTimeLeft.TotalSeconds);
                if (item == default(WoWItem)) return new MagicValueType(Decimal.MaxValue);
                return new MagicValueType(item.Cooldown);
            }

        }

        public Composite UsePotion(String name, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var n = UppercaseFirst(name.Split('_')[0]) + " " + UppercaseFirst(name.Split('_')[1]);
                    var item = Me.BagItems.FirstOrDefault(ret => ret.Name.Contains(n) && ret.Name.Contains("Potion"));
                    //Logging.Write(item.ToString() + " " + item.CooldownTimeLeft.TotalSeconds);
                    if (item == default(WoWItem)) return RunStatus.Failure; ;
                    PotionName = item.Name;
                    if (item.Cooldown <= 0) Lua.DoString("UseItemByName(\""+item.Name+"\")");
                    return RunStatus.Failure;
                }));
        }

        public Composite UsePotion(String name, String Reason = "")
        {
            CanRunDecoratorDelegate del = null;
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var n = UppercaseFirst(name.Split('_')[0]) + " " + UppercaseFirst(name.Split('_')[1]);
                    var item = Me.BagItems.FirstOrDefault(ret => ret.Name.Contains(n) && ret.Name.Contains("Potion"));
                    //Logging.Write(item.ToString() + " " + item.CooldownTimeLeft.TotalSeconds);
                    if (item == default(WoWItem)) return RunStatus.Failure; ;
                    PotionName = item.Name;
                    if (item.Cooldown <= 0) item.Use();
                    return RunStatus.Failure;
                }));
        }


        public int ticks_remain
        {
            get { return debuff[_conditionSpell].ticks_remain; }
        }

        public Composite PoolResource(String spell, CanRunDecoratorDelegate pool = null,
            CanRunDecoratorDelegate del = null, WoWUnit target = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new PrioritySelector(
                    new Decorator(ret => pool(null),
                        new Action(delegate
                        {
                            Lua.DoString("_G[\"kane_spd\"] = \"" + "Pooling: " + spell + "\";");
                            //Logging.Write(DateTime.Now+": Pooling Energy for " + spell + " " + Energy.current + "/" + energy);
                            return RunStatus.Success;
                        })),
                    Cast(spell, del, target, Reason)
                    )
                );
        }

        public SpellState IsUseableSpell(String name)
        {
            var i =
                Lua.GetReturnVal<int>(
                    "usable, nomana = IsUsableSpell(\"" + name +
                    "\");  if (not usable) then if (not nomana) then return 1; else return 2; end else return 0;end", 0);
            if (i == 1) return SpellState.CanNotCast;
            if (i == 2) return SpellState.NoMana;
            return SpellState.CanCast;
        }

        public static void UpdateLimiters()
        {
            _damageEnabled = Lua.GetReturnVal<Boolean>(
                "return _G[\"kane_damage\"] == true", 0);
            _aoeEnabled = Lua.GetReturnVal<Boolean>(
                "return _G[\"kane_aoe\"] == true", 0);
            _cdsEnabled = Lua.GetReturnVal<Boolean>(
                "return _G[\"kane_cds\"] == true", 0);
        }

        public static String logf = null;

        public static void Write(String format = "", Color c = default(Color), LogLevel l = LogLevel.Normal, params object[] pars)
        {
            try
            {
                Console.WriteLine(format);
                if (c == default(Color)) c = Colors.White;
                if (pars == null) pars = new object[0];
                if (logf == null) logf = RandomString(10);
                if (Superlog)
                {
                    File.AppendAllText(SimcraftLogfile, "<" + DateTime.Now.ToShortTimeString() + ">:" + format + Environment.NewLine);
                    /*if (Directory.Exists(@"Bots\Simcraft\Trunk\"))
                        File.AppendAllText(@"Bots\Simcraft\Trunk\Logs\" + logf + ".log", "<" + DateTime.Now.ToShortTimeString() + ">:" + format + Environment.NewLine);
                    else
                        File.AppendAllText(@"Bots\Simcraft\Logs\" + logf + ".log", "<" + DateTime.Now.ToShortTimeString() + ">:" + format + Environment.NewLine);*/

                }
                if (l != LogLevel.Diagnostic) Logging.Write("<" + DateTime.Now.ToShortTimeString() + ">:" + format);
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
            }
        }

        #region [Spell / Logging]

        public bool SpecialRequirementsCheck(String spell)
        {
            if (spell.Equals("Plague Leech"))
            {
                return debuff.frost_fever.up && debuff.blood_plague.up;
            }
            if (spell.Equals("Blood Tap"))
            {
                return buff.blood_charge.stack >= 5;
            }
            return true;
        }

        public bool CastSpell(string spellName, WoWUnit u, int LC, String Reason)
        {
            if (!ValidUnit(u) || u == null)
                return false;

            if (!SpecialRequirementsCheck(spellName)) return false;

            //Logging.Write("{7}: CR: {0} / {1} - BR {2} / {3} - MR {4} - Combat {5} {6} - CanCast: {8} / {9}", Me.CombatReach, Me.CurrentTarget.CombatReach, Me.BoundingRadius, Me.CurrentTarget.BoundingRadius, Me.CurrentTarget.IsWithinMeleeRange, Me.IsActuallyInCombat, Me.Combat, DateTime.Now, SpellManager.CanCast(spellName, u), spellName);


            if (SpellManager.CanCast(spellName, u) && GetSpell(spellName).ActualMinRange(u) < u.Distance)

                if (SpellManager.Cast(spellName, u))
                {
                    //if (Reason.Length > 1) SimcraftImpl.Write("Casting " + spellName + " to " + Reason);
                    return true;
                }
                else
                {
                    return false;
                }

            return false;
        }

        public bool CanCast(string spellname)
        {
            return Lua.GetReturnVal<bool>("usable, nomana = IsUsableSpell(\"" + spellname + "\"); return usable;", 0);
        }

        #endregion [Spell / Logging]



        public class NamedComposite : PrioritySelector
        {
            public static String Sequence = "";
            public String Name;

            public NamedComposite(String name, String spell, params Composite[] children)
                : base(children)
            {
                Name = name;
                //Logging.Write(name + " = " + spell);
            }

            public override RunStatus Tick(object context)
            {
                var x = base.Tick(context);
                if (x == RunStatus.Success) Sequence += Name;
                return x;
            }
        }

        public class ClusterUnit : WoWUnit
        {
            private readonly double radius;
            private readonly double range;

            public ClusterUnit(double range, double radius)
                : base(IntPtr.Zero)
            {
                this.range = range;
                this.radius = radius;
            }

            public override WoWPoint Location
            {
                get { return ideal_location(); }
            }

            // Find the points where the two circles intersect.
            private bool DoCirclesIntersect(
                double cx0, double cy0, double radius0,
                double cx1, double cy1, double radius1
                )
            {
                var dx = cx0 - cx1;
                var dy = cy0 - cy1;
                var dist = Math.Sqrt(dx * dx + dy * dy);

                return (dist <= radius0 + radius1);
            }

            public WoWPoint ideal_location()
            {
                var unfs = UnfriendlyUnitsNearMe((float)(range));

                return StyxWoW.Me.CurrentTarget.Location;
            }

            public class Circle
            {
                public WoWPoint Location;
                public double Radius;
                public bool MT;
                public Circle(double rd, WoWPoint loc, bool mt)
                {
                    MT = mt;
                    Radius = rd;
                    Location = loc;
                }
            }
        }

        #region [TargetU Checks]

        private static int _unfriendlyCache;
        private static List<WoWUnit> unfriendlyCache = new List<WoWUnit>();

        public WoWUnit Target2
        {
            get
            {
                try
                {
                    var t3 = actives.Where(ret => ret != Me.CurrentTarget).ToArray();
                    //Logging.Write(t3.Count()+"");
                    return t3[0];
                }
                catch
                {
                    return null;
                }
            }
        }

        public WoWUnit TargetSelf
        {
            get { return Me; }
        }

        public WoWUnit Targetself
        {
            get { return Me; }
        }

        public WoWUnit Target3
        {
            get
            {
                try
                {
                    var t3 = actives.Where(ret => ret != Me.CurrentTarget).ToArray();
                    return t3[1];
                }
                catch
                {
                    return null;
                }
            }
        }

        public static List<WoWUnit> UnfriendlyUnits
        {
            get
            {
                if (_unfriendlyCache == iterationCounter) return unfriendlyCache;
                _unfriendlyCache = iterationCounter;
                unfriendlyCache = ObjectManager.GetObjectsOfType<WoWUnit>().Where(u => !u.IsDead
                                                                                                   && u.CanSelect
                                                                                                   && u.Attackable
                                                                                                   && !u.IsFriendly
                    //&& u.Distance <= 10
                    ).ToList();
                return unfriendlyCache;
            }
        }

        public static IEnumerable<WoWUnit> UnfriendlyUnitsNearTarget(float dist)
        {
            return UnfriendlyUnits.Where(u => u.Location.Distance(StyxWoW.Me.CurrentTarget.Location) < dist + StyxWoW.Me.CurrentTarget.CombatReach);
        }

        public static IEnumerable<WoWUnit> UnfriendlyUnitsNearMe(float dist)
        {
            return UnfriendlyUnits.Where(ret => ret.Distance < dist + +StyxWoW.Me.CurrentTarget.CombatReach + Me.CombatReach);
        }

        public static bool ValidUnit(WoWUnit u)
        {
            if (u == null || !u.IsValid)
            {
                return false;
            }
            if (!u.CanSelect)
            {
                return false;
            }
            return true;
        }

        #endregion [TargetU Checks]

        #region Spell CD Helpers

        public static TimeSpan GetSpellCooldown(string spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                {
                    return results.Override.CooldownTimeLeft;
                }
                return results.Original.CooldownTimeLeft;
            }


            return TimeSpan.MaxValue;
        }

        public static WoWSpell GetSpell(int spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override;
                return results.Original;
            }


            return default(WoWSpell);
        }

        public static WoWSpell GetSpell(string spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override;
                return results.Original;
            }


            return default(WoWSpell);
        }

        public static bool GetSpellOnCooldown(String spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                {
                    return results.Override.Cooldown;
                }
                return results.Original.Cooldown;
            }
            return false;

            //return !Lua.GetReturnVal<bool>("start, duration, enabled = GetSpellCooldown(\"" + spell + "\"); return duration == 0", 0);
        }

        public static bool GetSpellOnCooldownId(int spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override.Cooldown;
                return results.Original.Cooldown;
            }
            return false;
            //return !Lua.GetReturnVal<bool>("start, duration, enabled = GetSpellCooldown(\"" + spell + "\"); return duration == 0", 0);
        }

        public static TimeSpan GetSpellCooldownId(int spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override.CooldownTimeLeft;
                return results.Original.CooldownTimeLeft;
            }
            return TimeSpan.MaxValue;
        }

        #endregion Spell CD

        #region BuffProxy Helpers

        public static int GetAuraStacks(WoWUnit unit, string auraName, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.Name == auraName && a.Duration > 0 && (!fromMyAura || a.CreatorGuid == Me.Guid));
                return wantedAura != null ? (int)wantedAura.StackCount : 0;
            }
            return 0;
        }


        public static WoWAura GetAura(WoWUnit unit, string auraName, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.Name == auraName && a.Duration > 0 && (!fromMyAura || a.CreatorGuid == Me.Guid));
                return wantedAura != null ? wantedAura : null;
            }
            return null;
        }

        public static TimeSpan GetAuraTimeLeft(WoWUnit unit, string auraName, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.Name == auraName && a.Duration > 0 && (!fromMyAura || a.CreatorGuid == Me.Guid));
                return wantedAura != null ? wantedAura.TimeLeft : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }

        public static bool CancelAura(string auraName, bool fromMyAura = false)
        {
            var unit = Me;
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.Name == auraName && (!fromMyAura || a.CreatorGuid == Me.Guid));
                if (wantedAura != null) wantedAura.TryCancelAura();
                return true;
            }
            return false;
        }

        public Composite CancelAura(String auraName, CanRunDecoratorDelegate del)
        {
            return new Decorator(del, new Action(delegate { CancelAura(auraName); }));
        }

        public static int GetAuraStacks(WoWUnit unit, int spellid, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.SpellId == spellid && a.Duration > 0 && (!fromMyAura || a.CreatorGuid == Me.Guid));
                return wantedAura != null ? (int)wantedAura.StackCount : 0;
            }
            return 0;
        }

        public static TimeSpan GetAuraTimeLeft(WoWUnit unit, int spellid, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.SpellId == spellid && a.Duration > 0 && (!fromMyAura || a.CreatorGuid == Me.Guid));
                return wantedAura != null ? wantedAura.TimeLeft : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }


        public static bool GetAuraUp(WoWUnit unit, int spellid, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a => a.SpellId == spellid && (!fromMyAura || a.CreatorGuid == Me.Guid));
                if (wantedAura == null) return false;
                return wantedAura.IsActive;
            }
            return false;
        }

        public static bool GetAuraUp(WoWUnit unit, string auraName, bool fromMyAura = false)
        {
            if (unit != null)
            {
                var wantedAura =
                    unit.GetAllAuras()
                        .FirstOrDefault(
                            a =>
                                a.Name.ToLower().Equals(auraName.ToLower()) && (!fromMyAura || a.CreatorGuid == Me.Guid));
                if (wantedAura == null) return false;
                return wantedAura.IsActive;
            }
            return false;
        }

        #endregion
    }
}