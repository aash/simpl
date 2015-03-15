#region Honorbuddy

#endregion

#region System

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using CommonBehaviors.Actions;
using Simcraft.APL;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        public delegate String RetrieveSpellDelegate();

        private const int apparition_flight_time = 5000;
        public static String logf;
        private static readonly Mutex mut = new Mutex();
        private readonly List<Stopwatch> apparitions = new List<Stopwatch>();
        private readonly Dictionary<char, Stopwatch> line_cds = new Dictionary<char, Stopwatch>();
        private readonly Stopwatch SoulShardTimer = new Stopwatch();
        //<summary>_conditonSpell is the true name of the spell, non tokenized</summary>
        private spell_data_t _conditionSpell;
        private Mutex a = new Mutex();
        public char conditionName = 'A';
        public WoWUnit CycleTarget;
        public char NameCount = Convert.ToChar(65);
        private int o_soul_shard;
        public WoWItem Potion;
        public bool pyro_chain;
        private Regex shorthand = new Regex("[^a-z_]");
        public WoWUnit clickUnit { get; set; }

        public static int combatIterationCount = 0;

        public static MagicValueType SpellIsTargeting
        {
            get
            {
                return new MagicValueType(LuaGet<Boolean>(
                    "return SpellIsTargeting()", 0));
            }
        }


        public ResourceProxy main_resource
        {
            get
            {
                switch (_class)
                {
                    case WoWClass.Hunter:
                        return focus;
                    case WoWClass.Rogue:
                        return energy;
                    case WoWClass.Warrior:
                        return rage;
                    case WoWClass.Monk:
                        if (_spec == WoWSpec.MonkMistweaver) return mana;
                        return energy;
                    case WoWClass.Druid:
                        if (_spec == WoWSpec.DruidFeral) return energy;
                        if (_spec == WoWSpec.DruidGuardian) return rage;
                        return mana;
                         case WoWClass.DeathKnight:
                        return runic_power;
                        
                    default:
                        return mana;
                }
            }
        }

        public MagicValueType PotionCooldown
        {
            get { return new MagicValueType(Potion.Cooldown); }
        }

        public void toggle_hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            hotkeyVariables[name] = !hotkeyVariables[name];
            LuaDoString("_G[\"sc_" + name + "\"] = " + hotkeyVariables[name] + "; print('" + name + " => " +
                        hotkeyVariables[name] + "');");
            //true then _G[\""+name+"\"] = nil; print('Cds enabled') else _G[\""+name+"\"] = true; print('Cds disabled') end");
            //Write("Toggled " + name + " to :" + hotkeyVariables[name]);
        }

        public bool hkvar(String name)
        {
            if (!hotkeyVariables.ContainsKey(name)) hotkeyVariables[name] = false;
            return hotkeyVariables[name];
        }

        public static spell_data_t DBGetSpell(String name)
        {
            name = Tokenize(name);

            //Write("tttt: "+name);

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

        public void start_line_cd()
        {
            if (!line_cds.ContainsKey(conditionName))
                line_cds[conditionName] = new Stopwatch();
            line_cds[conditionName].Restart();
        }

        public Composite StartPyroChain(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator(_ret =>
            {
                var r = d(_ret) && moving;
                //LogDebug("start_pyro_chain" + " " + reason + " => " + r);
                return r;
            }, new Action(delegate
            {
                pyro_chain = true;
                return RunStatus.Failure;
            }));
        }

        public Composite StopPyroChain(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator(_ret =>
            {
                var r = d(_ret) && moving;
                //LogDebug("stop_pyro_chain" + " " + reason + " => " + r);
                return r;
            }, new Action(delegate
            {
                pyro_chain = false;
                return RunStatus.Failure;
            }));
        }

        public Composite Wait(CanRunDecoratorDelegate del = null, String reason = "")
        {
            var d = del ?? (ret => true);
            return new Decorator(_ret =>
            {
                var r = d(_ret) && moving;
                //LogDebug("wait" + " " + reason + " => " + r);
                return r;
            }, new ActionAlwaysSucceed());
        }


        public static bool CanCast(String name, WoWUnit target)
        {
            var a = SpellManager.CanCast(name, target);
            LogDebug("CanCast: "+name+" => "+a);
            return a;
        }

        public Composite CycleTargets(String _spell, UnitCriteriaDelegate criteria, String Reason = "")
        {
            NameCount++;

            //Write("looking for spell: " + _spell);
            var actualSpell = LearnedSpellFromToken(_spell);

            if (actualSpell == null)
            {
                Write("Couldnt find " + _spell + " in our spellbook or in the Simcraft table");
                return new ActionAlwaysFail();
            }

            return new NamedComposite("" + NameCount, _spell,
                new Action(delegate
                {
                    conditionName = NameCount;
                    _conditionSpell = actualSpell;
                    foreach (var w in actives)
                    {
                        conditionUnit = w;
                        if (criteria(w) && (CanCast(actualSpell.name, w)))
                        {
                            CycleTarget = w;
                            return RunStatus.Failure;
                        }                       
                    }
                    CycleTarget = null;
                    return RunStatus.Failure;
                }),
                new Decorator(ret => CycleTarget != null,
                    new Action(delegate
                    {
                        if (CastSpell(actualSpell, CycleTarget, 3, Reason))
                        {
                            //LogDebug(_spell + " " + Reason + " => SUCCESS!");
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite Cast(String _spell, CanRunDecoratorDelegate del, GetUnitDelegate getTarget, String Reason)
        {
            NameCount++;

            WoWUnit _target;
          
            var actualSpell = LearnedSpellFromToken(_spell);

            if (actualSpell == null)
            {
                Write("Couldnt find " + _spell + " in our spellbook or in the Simcraft table");
                return new ActionAlwaysFail();
            }



            return new NamedComposite("" + NameCount, actualSpell.name,
                new Action(delegate
                {

                    
                    _target = getTarget();
                    //if (_target != null && _target != Target1()) Write("Checking diff target");

                    conditionName = NameCount;
                    _conditionSpell = actualSpell;
                    
                    conditionUnit = _target;
                    if (conditionUnit == null || conditionUnit == default(WoWUnit)) conditionUnit = Me.CurrentTarget;

                    return RunStatus.Failure;
                }),
                new Decorator(_ret =>
                {
                    if (actualSpell.gcd > 0 && SpellManager.GlobalCooldown)
                    {
                        LogDebug("GCD Skip: "+actualSpell.name);
                        return false;
                    }
                    var d = del ?? (ret => true);
                    try
                    {
                        LogDebug(actualSpell.name + " if=" + Reason);
                        var r = d(_ret);
                        LogDebug(actualSpell.name + " => " + r);
                        if (!r) return false;
                        return (CanCast(actualSpell.name, conditionUnit));
                    }
                    catch (Exception e)
                    {
                        //Write(conditionName + " " + _spell + " " + e.ToString());
                        return false;
                    }
                },
                    new Action(delegate
                    {
                        clickUnit = conditionUnit;
                        //if (SpellManager.CanCast(spell.name, conditionUnit))
                        if (CastSpell(actualSpell, conditionUnit, 3, Reason))
                        {
                            //if (actualSpell.name.Contains("Fire")) Write("fire:"+conditionUnit.Guid.GetFriendlyString());
                            //LogDebug(_spell +  " => SUCCESS!");
                            start_line_cd();
                            return RunStatus.Success;
                        }
                        else
                        {
                            LogDebug(_spell +  " => FAIL!");
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite Cast(String _spell, CanRunDecoratorDelegate del, String r)
        {
            return Cast(_spell, del, () => Target1(), r);
        }

        public Composite Cast(String _spell, GetUnitDelegate getTarget, String Reason)
        {
            return Cast(_spell, null, getTarget, Reason);
        }

        public Composite Cast(String _spell, String r)
        {
            return Cast(_spell, null, () => Target1(), r);
        }

        public bool IsPlayerSpell(uint spell)
        {
            return LuaGet<bool>("return IsPlayerSpell(" + spell + ");", 0)
                ;
        }

        public spell_data_t LearnedSpellFromToken(String token)
        {
            if (SimcNames.spells.ContainsKey(token))
            {
                var s = SimcNames.spells[token];

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
            //throw new Exception("Token: "+token+" missing from SpellDB");   
        }

        public Composite Cast(String _spell)
        {
            return Cast(_spell, null, null, "");
        }

        private static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public MagicValueType GetTime()
        {
            return MainCache["WoWTime"].GetValue();
        }

        public Composite UseItem(int id, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var item = Me.Inventory.Equipped.PhysicalItems.FirstOrDefault(it => it.ItemInfo.Id == id);

                    if (item == default(WoWItem)) return RunStatus.Failure;
                    ;
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

                    if (item == default(WoWItem)) return RunStatus.Failure;
                    ;
                    if (item.Cooldown <= 0) item.Use();
                    return RunStatus.Failure;
                }));
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
                            //Write(DateTime.Now+": Pooling Energy for " + spell + " " + Energy.current + "/" + energy);
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

        public static void LogDebug(String stuff)
        {
            if (Superlog) Write( LogLevel.Diagnostic, stuff);
        }

        public static void Write(LogLevel l, String format, params object[] pars)
        {
            try
            {
                Logging.Write(l, "<" + DateTime.Now.ToShortTimeString() + ">:" + format, pars);
            }
            catch (Exception e)
            {
                Logging.WriteException(e);
            }
        }

        public static void Write(String format, params object[] pars)
        {
            Write(LogLevel.Normal, format, pars);
        }

        public class NamedComposite : PrioritySelector
        {
            public static String Sequence = "";
            public String Name;

            public NamedComposite(String name, String spell, params Composite[] children)
                : base(children)
            {
                Name = name;
                //Write(name + " = " + spell);
            }

            public override RunStatus Tick(object context)
            {
                var x = base.Tick(context);
                if (x == RunStatus.Success) Sequence += Name;
                return x;
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

            //if (SpellManager.CanCast(spell.name, u))

                if (SpellManager.Cast(spell.name, u))
                {
                    LogDebug(spell.name + " Success!");
                    return true;
                }
                else
                {
                    LogDebug(spell.name + " failed due to Cast");
                    return false;
                }

            LogDebug(spell.name+" failed due to CanCast?");
            return false;
        }

        #endregion [Spell / Logging]

        #region [Surrounding Unit SimcraftImpl]

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

        public GetUnitDelegate TargetSelf
        {
            get { return () => Me; }
        }

        public GetUnitDelegate Targetself
        {
            get
            {
                
                return () => Me;
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
            return
                UnfriendlyUnits.Where(
                    u =>
                        u.Location.Distance(StyxWoW.Me.CurrentTarget.Location) <
                        dist + StyxWoW.Me.CurrentTarget.CombatReach);
        }

        public static IEnumerable<WoWUnit> UnfriendlyUnitsInSpellRange(spell_data_t s)
        {
            return
                UnfriendlyUnits.Where(
                    u =>
                        u.Location.Distance(StyxWoW.Me.CurrentTarget.Location) <
                        s.max_range + StyxWoW.Me.CurrentTarget.CombatReach);
        }

        public static IEnumerable<WoWUnit> UnfriendlyUnitsNearMe(float dist)
        {
            return
                UnfriendlyUnits.Where(
                    ret => ret.Distance < dist + +StyxWoW.Me.CurrentTarget.CombatReach + Me.CombatReach);
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

        #region Spell CD SimcraftImpl

        public static TimeSpan GetSpellCooldown(spell_data_t spell)
        {
            var s = GetSpell(spell);
            if (s == default(WoWSpell)) return TimeSpan.MaxValue;
            return s.CooldownTimeLeft;
        }

        public static WoWSpell GetSpell(spell_data_t spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell((int) spell.id, out results))
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

        public class CachePair
        {
            public int Iteration;
            public object Value;
        }

        static Dictionary<string, CachePair> luaValues = new Dictionary<string, CachePair>();

        public static T LuaGet<T>(String lua, uint a)
        {
            

            if (!luaValues.ContainsKey(lua))
                luaValues[lua] = new CachePair {Iteration = -1};

            var lv = luaValues[lua];

            if (lv.Iteration < iterationCounter || lv.Iteration > iterationCounter){
                lv.Value = Lua.GetReturnVal<T>(lua, a);
                lv.Iteration = iterationCounter;
                luaCount++;
            if (luaCount%100 == 0)
                Write("luaavg:" + luaCount/inst.time);
                LogDebug(lua+" => "+lv.Value);
            }

            return (T)lv.Value;
        }

        public static void LuaDoString(String lua)
        {
            luaCount++;
            if (luaCount%1000 == 0)
                Write("luaavg:" + luaCount/inst.time);
            Lua.DoString(lua);
        }

        #endregion Spell CD

        #region BuffProxy SimcraftImpl

        public static int GetAuraStacks(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            var wantedAura = GetAura(unit, aura, fromMyAura);
            if (wantedAura == null || !wantedAura.IsActive) return 0;
            return (int) wantedAura.StackCount;
        }

        public static List<String> Mismatches = new List<string>();

        public static WoWAura GetAura(WoWUnit unit, spell_data_t aura, bool fromMyAura = false)
        {
            //if (aura == null) return null;
            if (unit != null)
            {
                
                LogDebug("Looking for "+aura.name);
                WoWAura wantedAura = null;
                var mismatch = 0;
                foreach (var a in unit.GetAllAuras())
                {
                    LogDebug(a.Name+" "+a.IsActive+" "+a.TimeLeft.TotalSeconds+" "+Me.HasAura((int)a.SpellId));
                    if (a.SpellId == aura.id &&
                        (!fromMyAura || a.CreatorGuid == Me.Guid || (Me.Pet != null && a.CreatorGuid == Me.Pet.Guid)))
                        return a;
                    if (a.Name.Equals(aura.name) && a.IsActive)
                    {
                        mismatch = a.SpellId;
                    }
                }
                if (mismatch > 0 &&
                    !Mismatches.Contains("We couldnt find " + aura.name + " by Id but there is an Aura with its name: " +
                                         aura.id + " / " + mismatch))
                {
                    Mismatches.Add("We couldnt find " + aura.name + " by Id but there is an Aura with its name: " +
                                   aura.id + " / " + mismatch);
                    Write("We couldnt find " + aura.name + " by Id but there is an Aura with its name: " +
                                  aura.id + " / " + mismatch);
                }
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