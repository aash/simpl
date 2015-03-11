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
using System.Threading;
using System.Windows.Markup;
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
using Simcraft.APL;

#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        public delegate String RetrieveSpellDelegate();

        //<summary>_conditonSpell is the true name of the spell, non tokenized</summary>
        private spell_data_t _conditionSpell;

        public WoWUnit CycleTarget;
        public char NameCount = Convert.ToChar(65);

        private List<Stopwatch> apparitions = new List<Stopwatch>();
        private const int apparition_flight_time = 5000;

        public void toggle_hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            hotkeyVariables[name] = !hotkeyVariables[name];
            LuaDoString("_G[\"sc_" + name + "\"] = " + hotkeyVariables[name] + "; print('" + name + " => " + hotkeyVariables[name] + "');");//true then _G[\""+name+"\"] = nil; print('Cds enabled') else _G[\""+name+"\"] = true; print('Cds disabled') end");
            //Logging.Write("Toggled " + name + " to :" + hotkeyVariables[name]);
        }

        public bool hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            return hotkeyVariables[name];
        }




        public MagicValueType incanters_flow_dir
        {
            get { return new MagicValueType(buff.incanters_flow.dir); }
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

        public MagicValueType desired_targets
        {
            get { return new MagicValueType(1); }
        }

        public MagicValueType mind_harvest
        {
            get { return new MagicValueType(conditionUnit.MindHarvest() ? 1 : 0); }
        }
        public MagicValueType moving
        {
            get { return new MagicValueType(Me.IsMoving); }
        }
        public MagicValueType shadowy_apparitions_in_flight
        {
            get
            {
                var naps = apparitions.Where(ret => ret.ElapsedMilliseconds >= apparition_flight_time);
                apparitions.RemoveAll(ret => naps.Contains(ret));
                return new MagicValueType(apparitions.Count());
            }
        }

        public WoWUnit current_target
        {
            get
            {
                return conditionUnit;

            }
        }

        public MagicValueType miss_react
        {

            get { return new MagicValueType(true); }
        }

        public MagicValueType cooldown_react
        {
            get
            {
                if (_conditionSpell.name == "Mind Blast")
                {
                    return buff.shadowy_insight.up;
                }
                return new MagicValueType(false);
            }
        }

        public MagicValueType natural_shadow_word_death_range
        {
            get { return target.health.pct < 20; }
        }

        public MagicValueType active_enemies
        {
            get { return new MagicValueType(actives.Count()); }
        }
        public MagicValueType demonic_fury
        {
            get
            {
                var be = LuaGet<int>("UnitPower(\"player\", SPELL_POWER_DEMONIC_FURY)", 0);
                return new MagicValueType(be);
            }
        }

        public MagicValueType shadow_orb
        {
            get
            {
                var be = LuaGet<int>("ret,_,_ = UnitPower(\"player\", SPELL_POWER_SHADOW_ORBS); return ret;", 0);
                return new MagicValueType(be);
            }
        }

        public MagicValueType distance
        {
            get { return new MagicValueType(conditionUnit.Distance); }
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


        public MagicValueType soul_shard
        {
            get
            {
                int ss = (int)Me.CurrentSoulShards;
                if (ss > o_soul_shard)
                {
                    SoulShardTimer.Restart();
                }
                o_soul_shard = ss;
                return new MagicValueType(ss);
            }
        }

        public static spell_data_t DBGetSpell(String name)
        {
            name = Tokenize(name);

            //Logging.Write("tttt: "+name);

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

        public MagicValueType charges_fractional
        {
            get
            {
                var sp = spell[_conditionSpell].charges_fractional;
                return sp;
            }
        }

        public MagicValueType travel_time
        {
            get
            {
                return new MagicValueType(conditionUnit.Distance - conditionUnit.CombatReach / _conditionSpell.speed);
            }
        }

        public MagicValueType spell_haste
        {
            get { return stat.spell_haste; }
        }

        public MagicValueType mastery_value
        {
            get { return stat.mastery_value; }
        }

        public MagicValueType shard_react
        {
            get
            {
                int ss = soul_shard;
                return new MagicValueType(SoulShardTimer.IsRunning && SoulShardTimer.ElapsedMilliseconds < 2000);
            }
        }

        public MagicValueType burning_ember
        {
            get
            {
                var be = LuaGet<int>("UnitPower(\"player\", SPELL_POWER_BURNING_EMBERS, true)", 0);
                return new MagicValueType((double)be / 10);
            }
        }

        public MagicValueType tick_time
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

        public MagicValueType eclipse_change
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

                var dir = LuaGet<String>("direction = GetEclipseDirection(); return direction;", 0);
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

                return new MagicValueType(eclipse_ch);
            }
        }

        public MagicValueType lunar_max
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

                var dir = LuaGet<String>("direction = GetEclipseDirection(); return direction;", 0);
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

                return new MagicValueType(time_to_next_lunar);
            }
        }
        public MagicValueType solar_max
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

                var dir = LuaGet<String>("direction = GetEclipseDirection(); return direction;", 0);
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

                return new MagicValueType(time_to_next_solar);
            }
        }


        public MagicValueType time_to_die
        {
            get { return target.time_to_die; }
        }

        public MagicValueType position_front
        {
            get { return new MagicValueType(!Me.IsBehind(conditionUnit)); }
        }

        public WoWUnit clickUnit { get; set; }

        public MagicValueType anticipation_charges
        {
            get { return new MagicValueType(buff.anticipation.stack); }
        }

        public MagicValueType primary_target
        {
            get { return new MagicValueType(Target1() == conditionUnit); }
        }

        public MagicValueType persistent_multiplier
        {
            get { return new MagicValueType(0); }
        }

        public MagicValueType remains
        {
            get { return debuff[_conditionSpell].remains; }
        }

        public MagicValueType duration
        {
            get { return spell[_conditionSpell].duration; }
        }

        public MagicValueType level { get { return new MagicValueType(Me.Level); } }

        public MagicValueType melee_range
        {
            get { return new MagicValueType(Me.IsWithinMeleeRangeOf(conditionUnit)); }
        }

        public MagicValueType facing
        {
            get { return target.facing; }
        }

        public MagicValueType execute_time
        {

            get
            {
                var ex = spell[_conditionSpell].execute_time;
                return ex;
            }
        }

        public MagicValueType enemies
        {
            get { return new MagicValueType(1000); }
        }

        public double cast_time
        {
            get { return spell[_conditionSpell].cast_time; }
        }

        public MagicValueType gcd
        {
            get { return MainCache["gcd"].GetValue(); }
        }

        public static MagicValueType SpellIsTargeting
        {
            get
            {
                return new MagicValueType(LuaGet<Boolean>(
                    "return SpellIsTargeting()", 0));
            }
        }

        public MagicValueType damage_enabled
        {
            get { return new MagicValueType(!_damageEnabled); }
        }

        public MagicValueType aoe_enabled
        {
            get { return new MagicValueType(!_aoeEnabled); }
        }

        public MagicValueType cooldowns_enabled
        {
            get { return new MagicValueType(!_cdsEnabled); }
        }

        public MagicValueType ticking
        {
            get { return debuff[_conditionSpell].ticking; }
        }

        public MagicValueType channel_time
        {
            get { return spell[_conditionSpell].channel_time; }
        }

        public MagicValueType recharge_time
        {
            get { return spell[_conditionSpell].recharge_time; }
        }

        public MagicValueType charges
        {
            get
            {
                return spell[_conditionSpell].charges;
            }
        }

        public MagicValueType cast_regen
        {
            get
            {
                var cTime = spell[_conditionSpell].cast_time;
                return focus.cast_regen(cTime > 0 ? cTime : gcd.nat);
            }
        }

        public MagicValueType sync(String spell)
        {
            return new MagicValueType(true);
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

            spell_data_t actualSpell = DBGetClassSpell(_spell);

            return new NamedComposite("" + NameCount, _spell,
                new Action(delegate
                {
                    conditionName = NameCount;
                    _conditionSpell = actualSpell;
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
                        if (CastSpell(actualSpell, CycleTarget, 3, Reason))
                        {
                            LogDebug(_spell + " " + Reason + " => SUCCESS!");
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite Cast(String spell, CanRunDecoratorDelegate del, GetUnitDelegate getTarget, String Reason)
        {
            NameCount++;

            WoWUnit _target;
            
            spell_data_t actualSpell = LearnedSpellFromToken(spell);

            if (actualSpell == null)
            {
                Logging.Write("Couldnt find " + spell + " in our spellbook or in the Simcraft table");
                return new ActionAlwaysFail();
            }

            return new NamedComposite("" + NameCount, actualSpell.name,
                new Action(delegate
                {
                    _target = getTarget();
                    //if (_target != null && _target != Target1()) Logging.Write("Checking diff target");

                    conditionName = NameCount;
                    _conditionSpell = actualSpell;
                    conditionUnit = _target;
                    if (conditionUnit == null || conditionUnit == default(WoWUnit)) conditionUnit = Me.CurrentTarget;
                    
                    return RunStatus.Failure;
                }),
                new Decorator((_ret) =>
                {
                    var d = del ?? (ret => true);
                    try
                    {
                        if (SimcraftImpl.Superlog) LogDebug(actualSpell.token + " if=" + Reason);
                        var r = d(_ret);
                        if (SimcraftImpl.Superlog) LogDebug(actualSpell.token + " => " + r);
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
                        if (CastSpell(actualSpell, conditionUnit, 3, Reason))
                        {
                            //if (actualSpell.name.Contains("Fire")) Logging.Write("fire:"+conditionUnit.Guid.GetFriendlyString());
                            LogDebug(spell + " " + Reason + " => SUCCESS!");
                            start_line_cd();
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite Cast(String spell, CanRunDecoratorDelegate del, String r)
        {
            return Cast(spell, del, () => Target1(), r);
        }

        public Composite Cast(String spell, GetUnitDelegate getTarget, String Reason)
        {
            return Cast(spell, null, getTarget, Reason);
        }

        public Composite Cast(String spell, String r)
        {
            return Cast(spell, null, () => Target1(), r);
        }

        public bool IsPlayerSpell(uint spell)
        {
            return Lua.GetReturnVal<bool>("return IsPlayerSpell(" + spell + ");", 0)
            ;
        }

        public spell_data_t LearnedSpellFromToken(String token)
        {

            if (SimcNames.spells.ContainsKey(token))
            {
                var s = SimcNames.spells[token];

                if (s.Count == 0) return null;
                if (s.Count == 1) return dbc.Spells[s[0].V2];
                foreach (var kv in s)
                {
                    if (kv.V1 == WoWSpec.None || kv.V1 == Me.Specialization)
                    {
                        return dbc.Spells[kv.V2];
                    }
                }
            }


            return null;
            /*var spellids = dbc.Spells[token];

            uint sp;

            if (spellids.Count == 1)
                sp = spellids.First();
            else
                sp = spellids.FirstOrDefault(IsPlayerSpell);

            if (!dbc.Spells.ContainsKey(sp))
            {
                //Logging.Write("Couldnt find " + token + " in " + spellids.Count+" trying SimcraftTable");

                if (!SimcNames.spells.ContainsKey(token)) return null;

                var s = SimcNames.spells[token];
               
                if (s.Count == 0) return null;
                if (s.Count == 1) return dbc.Spells[s[0].V2];
                foreach (var kv in s)
                {
                    if (kv.V1 == WoWSpec.None || kv.V1 == Me.Specialization)
                    {
                        if (s.Count == 1) return dbc.Spells[kv.V2];
                    }
                }

                return null;
            }
            //Logging.Write("Found "+token+" at "+sp);
            return  dbc.Spells[(uint)sp];*/
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

        public WoWItem Potion = null;

        public MagicValueType PotionCooldown
        {
            get
            {
                return new MagicValueType(Potion.Cooldown);
            }
        }

        public Composite UsePotion(String name, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            var n = UppercaseFirst(name.Split('_')[0]) + " " + UppercaseFirst(name.Split('_')[1]);
            var item = Me.BagItems.FirstOrDefault(ret => ret.Name.Contains(n) && ret.Name.Contains("Potion"));
            if (item == default(WoWItem)) return new ActionAlwaysFail();

            return new PrioritySelector(new Action(context =>
            {
                Potion = item;
                return RunStatus.Failure;

            }), new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    if (item.Cooldown <= 0) LuaDoString("UseItemByName(\"" + item.Name + "\")");
                    return RunStatus.Failure;
                })));
        }

        public Composite UsePotion(String name, String Reason = "")
        {
            CanRunDecoratorDelegate del = null;
            return UsePotion(name, null, Reason);
        }


        public int ticks_remain
        {
            get { return debuff[_conditionSpell].ticks_remain; }
        }

        public Composite PoolResource(String spell, CanRunDecoratorDelegate pool = null,
            CanRunDecoratorDelegate del = null, WoWUnit target = null, String Reason = "")
        {
            return null;
            /*return new Decorator(del ?? (ret => true),
                new PrioritySelector(
                    new Decorator(ret => pool(null),
                        new Action(delegate
                        {
                            LuaDoString("_G[\"kane_spd\"] = \"" + "Pooling: " + spell + "\";");
                            //Logging.Write(DateTime.Now+": Pooling Energy for " + spell + " " + Energy.current + "/" + energy);
                            return RunStatus.Success;
                        })),
                    Cast(spell, del, target, Reason)
                    )
                );*/
        }

        public SpellState IsUseableSpell(String name)
        {
            var i =
                LuaGet<int>(
                    "usable, nomana = IsUsableSpell(\"" + name +
                    "\");  if (not usable) then if (not nomana) then return 1; else return 2; end else return 0;end", 0);
            if (i == 1) return SpellState.CanNotCast;
            if (i == 2) return SpellState.NoMana;
            return SpellState.CanCast;
        }


        public static String logf = null;

        Mutex a = new Mutex();

        private static Mutex mut = new Mutex();

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
                    if (mut.WaitOne(1000))
                    {
                        File.AppendAllText(SimcraftLogfile,
                            "<" + DateTime.Now.ToShortTimeString() + ">:" + format + Environment.NewLine);
                        mut.ReleaseMutex();
                    }
                    //sem.Release();
                }
                if (l != LogLevel.Diagnostic) Logging.Write("<" + DateTime.Now.ToShortTimeString() + ">:" + format);
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
            }
        }

        #region [Spell / Logging]

        public bool SpecialRequirementsCheck(spell_data_t spell)
        {
            if (spell.name.Equals("Plague Leech"))
            {
                return debuff.frost_fever.up && debuff.blood_plague.up;
            }
            if (spell.name.Equals("Blood Tap"))
            {
                return buff.blood_charge.stack >= 5;
            }
            return true;
        }

        public bool CastSpell(spell_data_t spell, WoWUnit u, int LC, String Reason)
        {
            if (!ValidUnit(u) || u == null)
                return false;

            if (!SpecialRequirementsCheck(spell)) return false;

            if (SpellManager.CanCast((int)spell.id, u))

                if (SpellManager.Cast((int)spell.id, u))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            return false;
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


        #region [Surrounding Unit Helpers]

        private static int _unfriendlyCache;
        private static List<WoWUnit> unfriendlyCache = new List<WoWUnit>();

        public GetUnitDelegate Target2
        {
            get
            {
                return () =>
                {
                    var t3 = actives.Where(ret => ret.Guid != Target1().Guid).ToArray();
                    if (t3.Length == 0) return null;
                    return t3[0];
                };
            }
        }

        public GetUnitDelegate Target3
        {
            get
            {
                return () =>
                {
                    var t3 = actives.Where(ret => ret.Guid != Target1().Guid).ToArray();
                    if (t3.Length < 2) return null;
                    return t3[1];
                };
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

        public static TimeSpan GetSpellCooldown(spell_data_t spell)
        {
            var s = GetSpell(spell);
            if (s == default(WoWSpell)) return TimeSpan.MaxValue;
            return s.CooldownTimeLeft;
        }

        public static WoWSpell GetSpell(spell_data_t spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell((int)spell.id, out results))
            {
                if (results.Override != null)
                    return results.Override;
                return results.Original;
            }

            return default(WoWSpell);
        }

        public static WoWSpell GetSpellByName(String Name)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(Name, out results))
            {
                if (results.Override != null)
                    return results.Override;
                return results.Original;
            }

            return default(WoWSpell);
        }

        public static bool GetSpellOnCooldown(spell_data_t spell)
        {
            var s = GetSpell(spell);
            if (s == default(WoWSpell)) return false;
            return s.Cooldown;
        }

        private static int luaCount = 1;

        public static T LuaGet<T>(String lua, uint a)
        {
            luaCount++;
            if (luaCount % 1000 == 0)
                Logging.Write("luaavg:" + luaCount / SimcraftImpl.inst.time);
            //Logging.Write(lua);
            return Lua.GetReturnVal<T>(lua, a);

        }

        public static void LuaDoString(String lua)
        {
            luaCount++;
            if (luaCount % 1000 == 0)
                Logging.Write("luaavg:" + luaCount / SimcraftImpl.inst.time);
            Lua.DoString(lua);
        }

        #endregion Spell CD

        #region BuffProxy Helpers

        public static int GetAuraStacks(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            var wantedAura = GetAura(unit, aura, fromMyAura);
            if (wantedAura == null || !wantedAura.IsActive) return 0;
            return (int)wantedAura.StackCount;
        }

        public static WoWAura GetAura(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            //if (aura == null) return null;
            if (unit != null)
            {
                WoWAura wantedAura = null;
                int mismatch = 0;
                foreach (var a in unit.GetAllAuras())
                {
                    //Logging.Write(a.CreatorGuid.GetFriendlyString()+" ");
                    if (a.SpellId == aura.id && (!fromMyAura || a.CreatorGuid == Me.Guid || (Me.Pet != null && a.CreatorGuid == Me.Pet.Guid))) return a;
                    if (a.Name.Equals(aura.name) && a.IsActive)
                    {
                        mismatch = a.SpellId;
                    }
                }
                if (mismatch > 0) Logging.Write("We couldnt find " + aura.name + " by Id but there is an Aura with its name: " + aura.id + " / " + mismatch);
            }
            return null;
        }

        public static TimeSpan GetAuraTimeLeft(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            var wantedAura = GetAura(unit, aura, fromMyAura);
            if (wantedAura == null || !wantedAura.IsActive) return TimeSpan.Zero;
            return wantedAura.TimeLeft;
        }


        public static bool CancelAura(spell_data_t aura, bool fromMyAura = false)
        {
            var wantedAura = GetAura(Me, aura, fromMyAura);
            if (wantedAura == null || !wantedAura.IsActive) return false;
            wantedAura.TryCancelAura();
            return true;
        }


        public static bool GetAuraUp(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            var wantedAura = GetAura(unit, aura, fromMyAura);
            if (wantedAura == null || !wantedAura.IsActive) return false;
            return true;
        }

        public static bool GetAuraUp(WoWUnit unit, spell_data_t[] auras, bool fromMyAura = false)
        {
            foreach (var aura in auras)
            {
                var wantedAura = GetAura(unit, aura, fromMyAura);
                if (wantedAura != null && wantedAura.IsActive) return true;

            }
            return true;
        }

        public static TimeSpan GetAuraTimeLeft(WoWUnit unit, spell_data_t[] auras, bool fromMyAura = false)
        {
            foreach (var aura in auras)
            {
                var wantedAura = GetAura(unit, aura, fromMyAura);
                if (wantedAura != null && wantedAura.IsActive) return wantedAura.TimeLeft;

            }
            return TimeSpan.Zero;
        }


        #endregion
    }
}