//This BotBase was created by Apoc, I take no credit for anything within this code
//I just changed "!StyxWoW.Me.CurrentTarget.IsFriendly" to "!StyxWoW.Me.CurrentTarget.IsHostile"
//For the purpose of allowing RaidBot to work within Arenas

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Profiles;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Simcraft.APL;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace Simcraft
{
    public partial class SimcraftImpl : BotBase
    {


        public double floor(double a)
        {
            return Math.Floor(a);
        }

        public double ceil(double a)
        {
            return Math.Ceiling(a);
        }

        public static SimcraftImpl inst;


        public delegate bool UnitCriteriaDelegate(WoWUnit u);

        private const string SimcraftHookName = "Simcraft.Root";
        private static Boolean _aoeEnabled;
        private static Boolean _damageEnabled;
        private static Boolean _cdsEnabled;
        public static int iterationCounter;
        public static int iterationCache = 1;
        public static DateTime ooc;
        public WoWClass _class = StyxWoW.Me.Class;
        public Stopwatch _clickTimer = new Stopwatch();
        public CombatRoutine _oldRoutine;
        public Composite _root;
        public dynamic actions;
        public dynamic buff; // = new BuffProxy();
        public ChiProxy chi; // = new ChiProxy(() => StyxWoW.Me.ToUnit(),this);
        public dynamic seal;
        public static ActionPrioriyList current_action_list = null;


        public dynamic totem
        {
            get { return pet; }
        }


        public String CobraReset = "[A Murder of Crows]" + "[Explosive Trap]" + "[Multi-Shot]" + "[Arcane Shot]" +
                                   "[Aimed Shot]" + "[Scatter Shot]" + "[Chimera Shot]" + "[Kill Shot]" + "[Powershot]" +
                                   "[Glaive Toss]" + "[Barrage]" + "[Black Arrow]";

        public ComboPointProxy combo_points;
        public WoWContext Context = WoWContext.PvE;
        public dynamic cooldown; // = new CooldownProxy();
        public dynamic debuff; // = new DebuffProxy();
        public EnergyProxy energy; // = new EnergyProxy(() => StyxWoW.Me.ToUnit(), this);
        public FocusProxy focus; // = new FocusProxy(() => StyxWoW.Me.ToUnit(), this);
        public HealthProxy health; // = new HealthProxy(() => StyxWoW.Me.ToUnit(), this);
        public Stopwatch IterationTimer = new Stopwatch();
        public double iterationTotal;
        public String LastSpellCast = "";
        public SpellOverride OverrideSpell = new SpellOverride();
        public RageProxy rage; // = new RageProxy(() => StyxWoW.Me.ToUnit(), this);
        public dynamic spell; // = new SpellProxy();
        public dynamic talent; // = new TalentProxy();
        public TargetProxy target;
        public RunicPowerProxy runic_power;
        public DiseaseProxy disease;
        public RuneProxy blood;// = new RuneProxy(null,null,RuneType.Blood);
        public RuneProxy unholy;
        public RuneProxy death;
        public RuneProxy frost;
        public dynamic glyph;
        public RaidEventProxy.FakeEvent movement = new RaidEventProxy.FakeEvent();
        public RaidEventProxy.FakeEvent adds = new RaidEventProxy.FakeEvent();
        public RaidEventProxy raid_event = new RaidEventProxy();
        public dynamic prev_gcd;
        public dynamic prev;
        public ItemProxy trinket1 = new ItemProxy(WoWEquipSlot.Trinket1);
        public ItemProxy trinket2 = new ItemProxy(WoWEquipSlot.Trinket2);
        public dynamic  set_bonus;
        public dynamic pet;
        public EclipseProxy eclipse_energy;
        public ObliterateProxy obliterate;
        public ManaProxy mana;
        public HolyPowerProxy holy_power;
        public WoWUnit last_judgment_target = null;
        public dynamic active_dot;
        public StatProxy stat;

        public WoWUnit Target1
        {
            get
            {
                try
                {
                    return Me.CurrentTarget;

                }
                catch
                {
                    return null;
                }
            }
        }


        public String FindDatabase()
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(),
                "db.dbc",
                SearchOption.AllDirectories);
            return files.First();
        }

        public static String SimcraftProfilePath = @"Simcraft Profiles/";
        public static String SimcraftLogPath = @"Logs/Simcraft/";
        public static String SimcraftLogfile;//

        public static Database dbc;

        public SimcraftImpl()
        {
            SimcraftLogfile = SimcraftLogPath + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + " - " + DateTime.Now.Hour + "-" + DateTime.Now.Minute + " " + Tokenize(Me.Name) + ".log";

            //Logging.Write("go!");
            try
            {
                dbc = Serializer.DeSerializeObject(FindDatabase());
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
            }

            spell_data_t[] a = new spell_data_t[dbc.Spells.Values.Count];
            dbc.Spells.Values.CopyTo(a, 0);

            foreach (var v in a)
            {
                dbc.Spells[v.id, v.token] = v;
            }

            Logging.Write("Count "+dbc.Spells.Count);

            Directory.CreateDirectory(SimcraftProfilePath);
            Directory.CreateDirectory(SimcraftLogPath);
            inst = this;
            active_dot = new ActiveDot();
            //trinket = new TrinketProxy(() => StyxWoW.Me.ToUnit(), this);
            health = new HealthProxy(() => StyxWoW.Me.ToUnit(), this);
            energy = new EnergyProxy(() => StyxWoW.Me.ToUnit(), this);
            focus = new FocusProxy(() => StyxWoW.Me.ToUnit(), this);
            chi = new ChiProxy(() => StyxWoW.Me.ToUnit(), this);
            rage = new RageProxy(() => StyxWoW.Me.ToUnit(), this);
            buff = new BuffProxy(() => StyxWoW.Me.ToUnit(), this);
            debuff = new DebuffProxy(() => conditionUnit, this);
            talent = new TalentProxy(() => StyxWoW.Me.ToUnit(), this);
            cooldown = new CooldownProxy(() => StyxWoW.Me.ToUnit(), this);
            spell = new SpellProxy(() => StyxWoW.Me.ToUnit(), this);
            combo_points = new ComboPointProxy(() => StyxWoW.Me.ToUnit(), this);
            
            target = new TargetProxy(() => conditionUnit, this);
            runic_power = new RunicPowerProxy(() => StyxWoW.Me.ToUnit(), this);
            disease = new DiseaseProxy(this);
            blood = new RuneProxy(() => StyxWoW.Me.ToUnit(), this, RuneType.Blood);
            unholy = new RuneProxy(() => StyxWoW.Me.ToUnit(), this, RuneType.Unholy);
            frost = new RuneProxy(() => StyxWoW.Me.ToUnit(), this, RuneType.Frost);
            death = new RuneProxy(() => StyxWoW.Me.ToUnit(), this, RuneType.Death);
            glyph = new GlyphProxy(() => StyxWoW.Me.ToUnit(), this);
            set_bonus = new SetBonusProxy();
            prev_gcd = new PrevGcdProxy();
            prev = new PrevGcdProxy();
            pet = new PetProxy("def");
            eclipse_energy = new EclipseProxy(() => StyxWoW.Me.ToUnit(), this);
            mana = new ManaProxy(() => StyxWoW.Me.ToUnit(), this);
            holy_power = new HolyPowerProxy(() => StyxWoW.Me.ToUnit(), this);
            seal = new SealProxy();
            actions = new ActionProxy();
            stat = new StatProxy();
            obliterate = new ObliterateProxy();

            //Logging.Write("eyo");

            dbc.ClassSpells["invoke_xuen"] = DBGetClassSpell("invoke_xuen_the_white_tiger").id;//dbc.Spells["invoke_xuen_the_white_tiger"].id;
            dbc.ClassSpells["tigereye_brew_use"] = DBGetClassSpell("tigereye_brew").id;
            dbc.ClassSpells["combo_breaker_bok"] = DBGetClassSpell("combo_breaker_blackout_kick").id;
            dbc.ClassSpells["combo_breaker_tp"] = DBGetClassSpell("tigereye_brew").id;
            dbc.ClassSpells["combo_breaker_ce"] = DBGetClassSpell("combo_breaker_tiger_palm").id;
            dbc.ClassSpells["storm_earth_and_fire_target"] = DBGetClassSpell("storm_earth_and_fire").id;

            dbc.ClassSpells["service_pet"] = DBGetSpell("grimoire_of_service").id;


            var arch = DBGetSpell("Archmage's Incandescence");
            var garch = DBGetSpell("Archmage's Greater Incandescence");
            dbc.Spells[arch.id, arch.token + "_agi"] = arch;
            dbc.Spells[arch.id, arch.token + "_str"] = arch;
            dbc.Spells[arch.id, arch.token + "_int"] = arch;
            dbc.Spells[garch.id, garch.token + "_agi"] = garch;
            dbc.Spells[garch.id, garch.token + "_str"] = garch;
            dbc.Spells[garch.id, garch.token + "_int"] = garch;
           

        }

        public override Form ConfigurationForm
        {
            get { return new ConfigWindow(); }
        }

        public override string Name
        {
            get { return "Simcraft Impl"; }
        }

        public override Composite Root
        {
            get
            {
                if (_root == null)
                    _root = new HookExecutor(SimcraftHookName);
                return _root;
            }
        }

       

        public override PulseFlags PulseFlags
        {
            get { return PulseFlags.Objects | PulseFlags.Lua; }
        }

        public static bool IsPaused { get; set; }

        public WoWClass Class
        {
            get { return StyxWoW.Me.Class; }
        }

        public static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        public dynamic action
        {
            get { return spell; }
        }

        public dynamic dot
        {
            get { return debuff; }
        }

        public WoWUnit conditionUnit { get; set; }

        public double time
        {
            get { return (DateTime.Now - ooc).TotalSeconds; }
        }


        private static Random Randomizer = new Random((int)DateTime.Now.Ticks);

        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Randomizer.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }


        public static Dictionary<String, ActionPrioriyList> apls = new Dictionary<String, ActionPrioriyList>(); 

        public static void GenerateApls(String folder)
        {
            apls.Clear();
            SimcraftImpl.inst.actions.Reset();
            SimcraftImpl.Write("Compiling Action Lists");

            foreach (var filename in Directory.GetFiles(folder))
            {
             
                if (!filename.EndsWith(".simc")) continue;
                String contents = File.ReadAllText(filename);
                var currentApl = ActionPrioriyList.FromString(contents);
                if (currentApl.Class != Me.Class) continue;
                
                var code = currentApl.ToCode();
                var classname = RandomString(10);
                code = code.Replace("public class SimcraftRota", "public class "+classname);

                var old = Superlog;
                Superlog = true;
                SimcraftImpl.Write(code, Colors.White, LogLevel.Diagnostic);
                //Console.WriteLine(code);
                Superlog = old;
                Assembly asm = RuntimeCodeCompiler.CompileCode(code);

                Behavior attributes =
                    (Behavior)asm.GetTypes()[0].GetMembers()[0].GetCustomAttributes(typeof(Behavior), true).First();

                currentApl.Assembly = asm;

                SimcraftImpl.Write("New Apl: " + currentApl.Name);

                apls[currentApl.ToString()] = currentApl;
            }
        }

        


        public static void Main()
        {


            //Logging.Write("go!");
            /*try
            {
                dbc = Serializer.DeSerializeObject(FindDatabase());
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
            }*/
            var e = Serializer.DeSerializeObject("db.dbc");
            
            spell_data_t[] a = new spell_data_t[e.Spells.Values.Count];
            e.Spells.Values.CopyTo(a, 0);

            foreach (var v in a)
            {
                e.Spells[v.id, v.token] = v;
                int count = 0;
                foreach (var eid in v.effects)
                {
                    if (e.Effects[eid].sub_type.Contains("PERIODIC") && e.Effects[eid].type.Contains("E_APPLY_AURA"))
                        count++;
                }
                if (count > 1) Console.WriteLine(v.name+" has multiple periodic");
                
            }



            Console.WriteLine("Count " + e.Spells.Count);

            Console.ReadKey();

        }

        public Composite CombatIteration()
        {
            return new Action(delegate
            {

                //Logging.Write(Me.Pet.);

                IterationTimer.Restart();

                if (SpellIsTargeting)
                {
                    SimcraftImpl.Write("Wanting to click: " + clickUnit);
                    //Logging.Write("Clicking: " + clickUnit.Location + " " + iterationCounter);
                    SpellManager.ClickRemoteLocation(clickUnit.Location);
                    clickUnit = null;
                }

                //Logging.Write("u:{0} b:{1} f:{2}", unholy.current, death.current, frost.current);
                iterationCounter++;
                UpdateLimiters();
                energy.Pulse();
                focus.Pulse();
                OverrideSpell.Pulse();
                return RunStatus.Failure;
            });
        }

        

        public Composite IterationEnd()
        {
            return new Action(delegate
            {
                IterationTimer.Stop();
                iterationTotal += IterationTimer.ElapsedMilliseconds;
                if (iterationCounter % 150 == 0) SimcraftImpl.Write("avgIte: " + (iterationTotal / iterationCounter));
                //Logging.Write(""+IterationTimer.ElapsedMilliseconds);
            });
        }

        public MagicValueType gcd
        {
            get
            {
                //if (!dbc.ClassSpells.ContainsKey(Tokenize(_conditionSpell))) return new MagicValueType(1.5);
                //return new MagicValueType(dbc.ClassSpells[Tokenize(_conditionSpell)].Gcd);
                return spell[_conditionSpell].gcd;
            }
        }
        static Regex token = new Regex("[^a-z_ 0-9]");

        private static String Tokenize(String s)
        {
            s = s.ToLower();
            s = token.Replace(s, "");
            s = s.Replace(' ', '_');
            return s;
        }

        private String[] weird_gcd_fuckers = new[] {"Ravager"};

        private void UNIT_SPELLCAST_SUCCEEDED(object sender, LuaEventArgs args)
        {
            if (args.Args[0].ToString().Equals("player"))
            {

                Logging.Write("Renddur: "+debuff.rupture.ticks_remain);

                //Logging.Write(""+DBGetSpell("Whirlwind").Gcd);
                //SimcraftImpl.Write(blood.frac + " " + frost.frac + " " + unholy.frac + " " + death.frac+ " " +blood.current + " " + frost.current + " " + unholy.current + " " + death.current+ " "+disease.ticking+ " "+disease.max_remains+ " "+disease.min_remains);

                LastSpellCast = args.Args[1].ToString();

                if (LastSpellCast.Equals("Shadowy Apparition"))
                {
                    var s = new Stopwatch();
                    s.Start();
                    apparitions.Add(s);
                }

                if (LastSpellCast.Equals("Mind Blast"))
                {
                    if (conditionUnit != null)
                        conditionUnit.ApplyMindHarvest();
                }

                if (LastSpellCast.Contains("Seal of"))
                {
                    seal.active = Tokenize(LastSpellCast).Split('_')[2];
                    //Logging.Write(seal.active);
                }

                if (LastSpellCast.Contains("Judgment"))
                {
                    last_judgment_target = conditionUnit;
                }
                var spell = DBGetClassSpell(LastSpellCast);
                if (spell.gcd > 0 || weird_gcd_fuckers.Contains(spell.name))
                {
                    Logging.Write(spell.name);
                    prev_gcd.Id = spell.id;
                    //Logging.Write(spell.Name + " using Gcd Cast");
                }       
                else
                {
                    //Logging.Write(spell.Name + " without Gcd Cast");
                }
                prev.id = spell.id;
                //Lua.DoString("_G[\"kane_spd\"] = \"" + LastSpellCast + "\";");
                if (LastSpellCast.Equals(OverrideSpell.Spell))
                {
                    SimcraftImpl.Write("Override Spell Cast, disabling");
                    OverrideSpell.Enabled = false;
                }
                if (Me.Specialization == WoWSpec.HunterSurvival && !talent[DBGetTalentSpell("Focusing Shot").name].enabled)
                {
                    if (LastSpellCast.Equals(DBGetSpell("Cobra Shot").name) || LastSpellCast.Equals(DBGetTalentSpell("Focusing Shot").name))
                    {
                        BuffProxy.cShots++;
                        if (BuffProxy.cShots == 2)
                        {
                            BuffProxy.cShots = 0;
                            SimcraftImpl.Write(DateTime.Now + ": Steady Shots!");
                        }
                    }
                    if (CobraReset.Contains(LastSpellCast) && BuffProxy.cShots > 0)
                    {
                        BuffProxy.cShots = 0;
                    }                   
                }
            }
        }

        public void LogDebug(String stuff)
        {
            Write(stuff,Colors.Violet,LogLevel.Diagnostic);
        }

        public override void Initialize()
        {
           
            var x = SimCSettings.currentSettings;
        }

        public WoWSpec Specialisation = WoWSpec.None;

        public void ContextChange(object sender, LuaEventArgs args)
        {

            //SimcraftImpl.Write(args != null ? args.EventName : "CTX");
            talent.Reset();
            glyph.Reset();
            spell.Reset();
            spell.Reset();
            line_cds.Clear();
            actions.Reset();
            _class = StyxWoW.Me.Class;

            var oldctx = Context;
            Context = (StyxWoW.Me.CurrentMap.IsArena || StyxWoW.Me.CurrentMap.IsBattleground)
                ? WoWContext.PvP
                : WoWContext.PvE;
            if (Context != oldctx) SimcraftImpl.Write("Switching to " + Context + " Mode");

            ooc = DateTime.Now;
            RebuildBehaviors();
        }

        public static ProfileSelector SelectorWindow = new ProfileSelector();

        public override void Start()
        {
            try
            {
                Specialisation = WoWSpec.None;
                NamedComposite.Sequence = "";
                TreeRoot.TicksPerSecond = 8;
                ProfileManager.LoadEmpty();
                _oldRoutine = RoutineManager.Current; //Save it so we can restore it later
                RoutineManager.Current = new ProxyRoutine();

                Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", UNIT_SPELLCAST_SUCCEEDED);
                Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
                Lua.Events.AttachEvent("PLAYER_LOGOUT", ContextChange);
                Lua.Events.AttachEvent("PLAYER_TALENT_UPDATE", ContextChange);

                ContextChange(null, null);

                RegisterHotkeys();
            }
            catch (Exception e)
            {
                SimcraftImpl.Write(e.ToString());
            }
        }

        public static void RegisterHotkeys()
        {
            HotkeysManager.Register("Simcraft Pause",
                SimCSettings.currentSettings.Execution.key,
                SimCSettings.currentSettings.Execution.mod,
                hk =>
                {
                    IsPaused = !IsPaused;
                    if (IsPaused)
                    {
                        Lua.DoString("print('Execution Paused!')");
                        // Make the bot use less resources while paused.
                        TreeRoot.TicksPerSecond = 5;
                    }
                    else
                    {
                        Lua.DoString("print('Execution Resumed!')");
                        // Kick it back into overdrive!
                        TreeRoot.TicksPerSecond = 8;
                    }
                });
            HotkeysManager.Register("Simcraft AOE",
                SimCSettings.currentSettings.Aoe.key,
                SimCSettings.currentSettings.Aoe.mod,
                hk =>
                {
                    
                    Lua.DoString("if _G[\"kane_aoe\"] == true then _G[\"kane_aoe\"] = nil; print('AOE enabled'); else _G[\"kane_aoe\"] = true; print('AOE disabled') end");
                });
            HotkeysManager.Register("Simcraft Cooldowns",
                SimCSettings.currentSettings.Cooldowns.key,
                SimCSettings.currentSettings.Cooldowns.mod,
                hk =>
                {
                    Lua.DoString("if _G[\"kane_cds\"] == true then _G[\"kane_cds\"] = nil; print('Cds enabled') else _G[\"kane_cds\"] = true; print('Cds disabled') end");


                });
        }

        public static void UnregisterHotkeys()
        {
            HotkeysManager.Unregister("Simcraft Pause");
            HotkeysManager.Unregister("Simcraft Cooldowns");
            HotkeysManager.Unregister("Simcraft AOE");
        }

        public override void Stop()
        {
            //SimcraftImpl.Write(NamedComposite.Sequence);
            NameCount = 'A';
            TreeRoot.ResetTicksPerSecond();
            RoutineManager.Current = _oldRoutine;
            UnregisterHotkeys();
            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", UNIT_SPELLCAST_SUCCEEDED);
            Lua.Events.DetachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
            Lua.Events.DetachEvent("PLAYER_LOGOUT", ContextChange);          
            Lua.Events.DetachEvent("PLAYER_TALENT_UPDATE", ContextChange);


        }

        public void RebuildBehaviors()
        {
            Composite simcRoot = null;
            NameCount = 'A';

            simcRoot = new PrioritySelector(
                new Decorator(ret => IsPaused,
                    new Action(ret => RunStatus.Success)),
                CallActionList(ActionProxy.ActionImpl.oocapl, ret => !StyxWoW.Me.Combat),
                new Decorator(ret => StyxWoW.Me.Combat,
                    new LockSelector(
                        new Decorator(
                            ret => StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.Combat,
                            new PrioritySelector(CombatIteration(), CastOverride(), actions.Selector, IterationEnd())))));

            TreeHooks.Instance.ReplaceHook(SimcraftHookName, simcRoot);

            //SimcraftImpl.Write(Specialisation+" "+Me.Specialization);

            if (Me.Specialization != Specialisation)
            {
                SelectorWindow.ShowDialog();
            }

            Specialisation = Me.Specialization;

        }

        public Composite CastOverride()
        {
            return new PrioritySelector(
                new Action(delegate
                {
                    _conditionSpell = OverrideSpell.Spell;
                    return RunStatus.Failure;
                }),
                new Decorator(ret => OverrideSpell.Enabled,
                    new Action(delegate
                    {
                        if (CastSpell(OverrideSpell.Spell, OverrideSpell.Target, 3, "SpellOverride"))
                            return RunStatus.Success;
                        return RunStatus.Failure;
                    })));
        }



        public class SpellOverride
        {
            public enum OverrideTarget
            {
                Target,
                Focus
            }

            public enum OverrideType
            {
                Location,
                Target
            }

            private readonly String reset_spelloverride_lua = "_G[\"spelloverride\"] = nil";
            private readonly String set_spelloverride_lua = "return _G[\"spelloverride\"]";
            private String _overrideString;
            public bool Enabled;
            public WoWUnit Target;
            public OverrideTarget TargetId;
            public OverrideType TargetType;
            public String Spell { get; private set; }

            public bool IsLoc
            {
                get { return TargetType == OverrideType.Location; }
            }

            public void Pulse()
            {
                _overrideString = Lua.GetReturnVal<String>(set_spelloverride_lua, 0);
                if (_overrideString == null) return;
                //if (_overrideString.Length < 8) return;
                Spell = _overrideString.Split(':')[1];
                var i = Convert.ToInt32(_overrideString.Split(':')[0]);
                var c = Convert.ToInt32(_overrideString.Split(':')[2]);
                if (c == 0) TargetId = OverrideTarget.Target;
                else TargetId = OverrideTarget.Focus;

                        if (TargetId == OverrideTarget.Target) Target = Me.CurrentTarget;
                        else Target = Me.FocusedUnit;

                Enabled = true;
                SimcraftImpl.Write("Spell Override: {0} via {1} on {2}", default(Color), LogLevel.Normal, Spell, TargetType, TargetId);
                Lua.DoString(reset_spelloverride_lua);
            }
        }


        public static bool Superlog = false;

        #region Nested types

        private class ProxyRoutine : CombatRoutine
        {
            public override string Name
            {
                get { return "Nullroutine"; }
            }

            public override WoWClass Class
            {
                get { return StyxWoW.Me.Class; }
            }
        }

        private class LockSelector : PrioritySelector
        {
            public LockSelector(params Composite[] children)
                : base(children)
            {
            }

            public override RunStatus Tick(object context)
            {
                try
                {
                    using (StyxWoW.Memory.AcquireFrame())
                    {
                        return base.Tick(context);
                    }
                }
                catch (Exception e)
                {
                    Logging.WriteException(e);
                }
                return RunStatus.Success;
            }
        }

        #endregion
    }


    public static class exts
    {

        

        private static List<WoWGuid> HarvestTarget = new List<WoWGuid>();

        public static bool MindHarvest(this WoWUnit me)
        {
            if (HarvestTarget.Contains(me.Guid)) return false;
            return true;
        }

        public static void ApplyMindHarvest(this WoWUnit me)
        {
            HarvestTarget.Add(me.Guid);
        }

    
  
    }

    #region enums

    #endregion
}

