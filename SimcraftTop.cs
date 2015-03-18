//This BotBase was created by Apoc, I take no credit for anything within this fullExpression
//I just changed "!StyxWoW.Me.CurrentTarget.IsFriendly" to "!StyxWoW.Me.CurrentTarget.IsHostile"
//For the purpose of allowing RaidBot to work within Arenas

using System;
using System.CodeDom;
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
using Honorbuddy;
using JetBrains.Annotations;
using Styx.CommonBot.Profiles.Quest.Order;


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
        public Dictionary<string, bool> hotkeyVariables = new Dictionary<string, bool>();
        public static bool Superlog = false;
        public dynamic totem
        {
            get { return pet; }
        }


        public uint[] CobraReset = new uint[] { 131894, 13813, 3044, 2643, 19434, 37506, 80012, 53351, 109259, 117050, 120360, 3674 };
            
            
            /*"[A Murder of Crows]" + "[Explosive Trap]" + "[Multi-Shot]" + "[Arcane Shot]" +
                                   "[Aimed Shot]" + "[Scatter Shot]" + "[Chimera Shot]" + "[Kill Shot]" + "[Powershot]" +
                                   "[Glaive Toss]" + "[Barrage]" + "[Black Arrow]";*/

        public ComboPointProxy combo_points;
        public WoWContext Context = WoWContext.PvE;
        public dynamic cooldown; // = new CooldownProxy();
        public dynamic debuff; // = new DebuffProxy();
        public EnergyProxy energy; // = new EnergyProxy(() => StyxWoW.Me.ToUnit(), this);
        public FocusProxy focus; // = new FocusProxy(() => StyxWoW.Me.ToUnit(), this);
        public HealthProxy health; // = new HealthProxy(() => StyxWoW.Me.ToUnit(), this);
        public Stopwatch IterationTimer = new Stopwatch();
        public double iterationTotal;
        public spell_data_t LastSpellCast;
        //public SpellOverride OverrideSpell = new SpellOverride();
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
        public dynamic stat;
       
        public ProxyCacheEntry MainCache;
       

        public AuraProxy PlayerAuras = new AuraProxy("player");
        public AuraProxy PetAuras = new AuraProxy("pet");


        public GetUnitDelegate Target1
        {
            get { return () => Me.CurrentTarget; }
        }


        public String FindDatabase()
        {
            Write("Looking for db.dbc in " + Directory.GetCurrentDirectory()+ " and all its Subdirectories");
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(),
                "db.dbc",
                SearchOption.AllDirectories);
            if (files.Length == 0) throw new FileNotFoundException("Couldnt find Database", "db.dbc");
            return files.First();
        }

        public static String SimcraftProfilePath = @"Simcraft Profiles/";


        public static Database dbc;

        public SimcraftImpl()
        {
  

            //Write("go!");
            try
            {
                dbc = Serializer.DeSerializeObject(FindDatabase());
            }
            catch (Exception e)
            {
                Write(e.ToString());
            }

            spell_data_t[] a = new spell_data_t[dbc.Spells.Values.Count];
            dbc.Spells.Values.CopyTo(a, 0);

            foreach (var v in a)
            {
                dbc.Spells[v.id, v.token] = v;
            }

            Write("Count "+dbc.Spells.Count);

            Directory.CreateDirectory(SimcraftProfilePath);

            try
            {

                RoutineManager.Current = null;

                SimcNames.Populate();
                MainCache = new ProxyCacheEntry();
                inst = this;
                active_dot = new ActiveDot();
                //trinket = new TrinketProxy(() => StyxWoW.Me.ToUnit(), this);
                health = new HealthProxy(() => StyxWoW.Me.ToUnit());
                energy = new EnergyProxy(() => StyxWoW.Me.ToUnit());
                focus = new FocusProxy(() => StyxWoW.Me.ToUnit());
                chi = new ChiProxy(() => StyxWoW.Me.ToUnit());
                rage = new RageProxy(() => StyxWoW.Me.ToUnit());
                buff = new BuffProxy(() => StyxWoW.Me.ToUnit(), PlayerAuras);
                debuff = new DebuffProxy(() => conditionUnit);
                talent = new TalentProxy(() => StyxWoW.Me.ToUnit());
                cooldown = new CooldownProxy(() => StyxWoW.Me.ToUnit());
                spell = new SpellProxy(() => StyxWoW.Me.ToUnit());
                combo_points = new ComboPointProxy(() => StyxWoW.Me.ToUnit());

                target = new TargetProxy(() => conditionUnit);
                runic_power = new RunicPowerProxy(() => StyxWoW.Me.ToUnit());
                disease = new DiseaseProxy(this);
                blood = new RuneProxy(() => StyxWoW.Me.ToUnit(), RuneType.Blood);
                unholy = new RuneProxy(() => StyxWoW.Me.ToUnit(), RuneType.Unholy);
                frost = new RuneProxy(() => StyxWoW.Me.ToUnit(), RuneType.Frost);
                death = new RuneProxy(() => StyxWoW.Me.ToUnit(), RuneType.Death);
                glyph = new GlyphProxy(() => StyxWoW.Me.ToUnit());
                set_bonus = new SetBonusProxy();
                prev_gcd = new PrevGcdProxy();
                prev = new PrevGcdProxy();
                pet = new PetProxy("def");
                eclipse_energy = new EclipseProxy(() => StyxWoW.Me.ToUnit());
                mana = new ManaProxy(() => StyxWoW.Me.ToUnit());
                holy_power = new HolyPowerProxy(() => StyxWoW.Me.ToUnit());
                seal = new SealProxy();
                actions = new ActionProxy();
                stat = new StatProxy();
                obliterate = new ObliterateProxy();

                MainCache["gcd"].SetRetrievalDelegate(() =>
                {
                    var rem = (Decimal)SpellManager.GlobalCooldownLeft.TotalSeconds;
                    var g = BaseGcd();
                    g = g / ((100+spell_haste)/ 100);
                    return new Gcd((Decimal)_conditionSpell.gcd, Math.Max(g, 1), rem);
                });

                MainCache["GlobalCooldown"].SetRetrievalDelegate(() =>
                {
                    return SpellManager.GlobalCooldownLeft.TotalMilliseconds > 1000/15;
                });

                MainCache["WoWTime"].SetRetrievalDelegate(() =>
                {
                    return Lua.GetReturnVal<double>("return GetTime()",0);
                });


                var s = new spell_data_t();
                s.name = "Storm, Earth, and Fire";
                s.token = "storm_earth_and_fire";
                s.id = 138130;
                dbc.Spells.Add(138130, s);



            }
            catch (Exception e)
            {
                Write(e.ToString());
            }
        }

        public Decimal BaseGcd()
        {
            switch (_class)
            {
               //hunters, rogues,  [Cat Form] druids, monks and death knights,
                case WoWClass.Hunter:
                case WoWClass.Rogue:
                case WoWClass.Druid:
                case WoWClass.Monk:
                case WoWClass.DeathKnight:
                    if (_spec != WoWSpec.DruidBalance && _spec != WoWSpec.DruidGuardian && _spec != WoWSpec.DruidRestoration)
                        return 1;
                    break;
            }
            return (Decimal)1.5;
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
            inst.actions.Reset();

            if (current_action_list != null ) current_action_list.Unload();


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

            dbc = Serializer.DeSerializeObject("db.dbc");
            

            spell_data_t[] a = new spell_data_t[dbc.Spells.Values.Count];
            dbc.Spells.Values.CopyTo(a, 0);

            foreach (var v in a)
            {
                dbc.Spells[v.id, v.token] = v;
            }


            int c = 0;
            String akk = "";

            SimcNames.Populate();


            //var u = new SimcNames.SpecList { new SimcNames.SpecPair(WoWSpec.None, 1), };
            
            /*foreach (var s in SimcNames.spells.Keys)
            {
              if (SimcNames.spells[s].Count == 0)  Console.WriteLine("spell::"+s);
            }

            foreach (var s in SimcNames.debuffs.Keys)
            {
                if (SimcNames.debuffs[s].Count == 0) Console.WriteLine("debuff::" + s);
            }

            foreach (var s in SimcNames.buffs.Keys)
            {
                if (SimcNames.buffs[s].Count == 0) Console.WriteLine("buff::" + s);
            }*/


            //var attr = dbc.Spells[SimcNames.spells["sinister_strike"].First().V2].attr1;


            foreach (var s in dbc.Spells.Values)
            {
                if (s.IsChanneled() && SimcNames.spells.ContainsKey(s.token)) Console.WriteLine(s.name);
            }

            Console.ReadKey();

            /*String content = "";
            foreach (var s in SimcNames.spells.Keys)
            {
                if (!dbc.Spells.RelationContainsKey(s))
                {
                    content += "spells[\"" + s + "\"]=new SpecList();" + Environment.NewLine;
                    continue;
                }
                string arr = "";
                
                foreach (var e in GetAllSpells(s))
                {
                    arr += "new SpecPair(WoWSpec.None, "+e.id+"),";

                }
                content += "spells[\"" + s + "\"]=new SpecList{" +arr+ "};" + Environment.NewLine;
            }

            foreach (var s in SimcNames.buffs.Keys)
            {
                if (!dbc.Spells.RelationContainsKey(s))
                {
                    content += "buffs[\"" + s + "\"]=new SpecList();" + Environment.NewLine;
                    continue;
                }
                string arr = "";

                foreach (var e in GetAllSpells(s))
                {
                    arr += "new SpecPair(WoWSpec.None, " + e.id + "),";

                }
                content += "buffs[\"" + s + "\"]=new SpecList{" + arr + "};" + Environment.NewLine;
            }

            foreach (var s in SimcNames.debuffs.Keys)
            {
                if (!dbc.Spells.RelationContainsKey(s))
                {
                    content += "debuffs[\"" + s + "\"]=new SpecList();" + Environment.NewLine;
                    continue;
                }
                string arr = "";

                foreach (var e in GetAllSpells(s))
                {
                    arr += "new SpecPair(WoWSpec.None, " + e.id + "),";

                }
                content += "debuffs[\"" + s + "\"]=new SpecList{" + arr + "};" + Environment.NewLine;
            }

            File.WriteAllText("a.txt",content);

            Console.WriteLine(c);

            //Console.WriteLine(FindBuff("steady_focus").tooltip);

            //Console.WriteLine(e.Spells[lid].name + " " + e.Spells[lid].desc);*/

        }


        public static List<spell_data_t> GetAllSpells(String token)
        {
            var ret = new List<spell_data_t>();
            foreach (var s in dbc.Spells[token])
            {
                ret.Add(dbc.Spells[s]);
            }
            return ret;
        }

        public static List<Effect> GetAllSpellEffects(spell_data_t t)
        {
            var ret = new List<Effect>();
            foreach (var ef in t.effects)
            {
                ret.Add(dbc.Effects[ef]);
            }
            return ret;
        }  


        public static spell_data_t FindDebuff(String token)
        {
            if (SimcNames.debuffs.ContainsKey(token))
            {
                var simcSpell = SimcNames.debuffs[token];

                if (simcSpell.Count == 1)
                {
                    return dbc.Spells[simcSpell[0].V2];
                }

                foreach (var s in simcSpell)
                {
                    if (s.V1 == WoWSpec.None || s.V1 == Me.Specialization)
                    {
                        return dbc.Spells[s.V2];
                    }
                }
            }
           
            var spells = GetAllSpells(token);
            uint lid = uint.MaxValue;
            bool casdam = false;
            if (spells.Count == 1) return dbc.Spells[spells.First().id];
            foreach (var spell in spells)
            {
                if (spell.tooltip.Length < 2) continue;

                bool hasAura = false;
                bool hasDamage = false;

                foreach (var ef in GetAllSpellEffects(spell))
                {
                    if (ef.type.Equals("E_APPLY_AURA"))
                    {
                        hasAura = true;
                        if (ef.sub_type.Contains("DAMAGE"))
                            hasDamage = true;
                    }
                }
                if (!hasAura) continue;
                if (!casdam && hasDamage)
                {
                    lid = spell.id;
                    casdam = true;
                }
                if ((casdam && hasDamage) || (!casdam && !hasDamage) && spell.id < lid)
                {
                    lid = spell.id;
                }
            }

            if (lid < uint.MaxValue)
                return dbc.Spells[lid];

            throw new Exception("No Debuff named " + token + " found");
        }

        public static spell_data_t FindBuff(String token)
        {

            if (SimcNames.buffs.ContainsKey(token))
            {
                var simcSpell = SimcNames.buffs[token];

                if (simcSpell.Count == 1)
                {
                    return dbc.Spells[simcSpell[0].V2];
                }

                foreach (var s in simcSpell)
                {
                    if (s.V1 == WoWSpec.None || s.V1 == Me.Specialization)
                    {
                        return dbc.Spells[s.V2];
                    }
                }
            }

            var spells = GetAllSpells(token);
            uint lid = uint.MaxValue;
            if (spells.Count == 1) return dbc.Spells[spells.First().id];
            foreach (var spell in spells)
            {
                if (spell.tooltip.Length < 2) continue;

                bool hasAura = false;
                foreach (var ef in GetAllSpellEffects(spell))
                {
                    if (ef.type.Equals("E_APPLY_AURA") && !ef.sub_type.Contains("DAMAGE"))
                    {
                        hasAura = true;
                    }
                }
                if (!hasAura) continue;
                if (spell.id < lid) lid = spell.id;
            }

            if (lid < uint.MaxValue)
                return dbc.Spells[lid];

            throw new Exception("No Buff named "+token+" found");
        }

        public Composite CombatIteration()
        {
            return new Action(delegate
            {
                //if (iterationCounter%7 == 0)
                /*if (console != null %% console.Va)console.BeginInvoke((System.Action)(() =>
                {

                        console.reset();

                }));*/
                
                //console.BeginInvoke(new Delegate(console,"reset"));
                //console.reset();
                //Write(Me.Pet.);

                combatIterationCount++;

                IterationTimer.Restart();

                if (SpellIsTargeting)
                {
                    SimcraftImpl.Write("Wanting to click: " + clickUnit);
                    //Write("Clicking: " + clickUnit.Location + " " + iterationCounter);
                    SpellManager.ClickRemoteLocation(clickUnit.Location);
                    clickUnit = null;
                }

                //Write("u:{0} b:{1} f:{2}", unholy.current, death.current, frost.current);
                
                //OverrideSpell.Pulse();
                return RunStatus.Failure;
            });
        }

        

        public Composite IterationEnd()
        {
            return new Action(delegate
            {
                IterationTimer.Stop();
                iterationTotal += IterationTimer.ElapsedMilliseconds;
                //if (IterationTimer.ElapsedMilliseconds > 1) Write(DateTime.Now.Millisecond+": "+IterationTimer.ElapsedMilliseconds+"");
                if (combatIterationCount % 150 == 0) SimcraftImpl.Write("avgIte: " + (iterationTotal / combatIterationCount));
                //Write(""+IterationTimer.ElapsedMilliseconds);
            });
        }


        static Regex token = new Regex("[^a-z_ 0-9]");

        private static String Tokenize(String s)
        {
            s = s.ToLower();
            s = token.Replace(s, "");
            s = s.Replace(' ', '_');
            return s;
        }



        private void COMBATLOG_EVENT_UNFILTERED(object sender, LuaEventArgs args)
        {
            /*var s = "";
            foreach (var arg in args.Args)
            {
                s += arg.ToString() + " ";
            }*/
            Logging.Write(args.EventName);
        }


        private void UNIT_SPELLCAST_SUCCEEDED(object sender, LuaEventArgs args)
        {
            //Logging.Write(args.EventName);
            //Write("bc: "+pet.buff.beast_cleave.up);
            //Write("ff: " + buff.frenzy.stack);

            //(Write("rav"+talent.ravager.enabled);
            //Write("ava:"+talent.avatar.enabled);

            /* var sw = new Stopwatch();
            sw.Restart();
            

            string cc = "local can_cast = {};";

            int i = 0;

            foreach (var sp in current_action_list.spells)
            {
                cc += "u, m = IsUsableSpell(\""+"Kill Command"+"\"); can_cast["+i+"] = u;";
                i++;
            }
            cc += "return unpack(can_cast);";

            Lua.GetReturnValues(cc);

            Logging.Write(""+sw.ElapsedTicks+" checked spells "+i);*/

            if (args.Args[0].ToString().Equals("player"))
            {

                uint spellid = uint.Parse(args.Args[4].ToString());

                if (!dbc.Spells.ContainsKey(spellid)) return;

                LastSpellCast = dbc.Spells[spellid];

                prev.spell = LastSpellCast;

                LogDebug("Cast: " + LastSpellCast.name);

                if (LastSpellCast.id == 78203)
                {
                    var s = new Stopwatch();
                    s.Start();
                    apparitions.Add(s);
                }

                if (LastSpellCast.id == 8092)
                {
                    if (conditionUnit != null)
                        conditionUnit.ApplyMindHarvest();
                }

                if (LastSpellCast.id == 20271)
                {
                    last_judgment_target = conditionUnit;
                }

                if (LastSpellCast.gcd > 0)
                    prev_gcd.spell = LastSpellCast;

                if (LastSpellCast.name.Contains("Seal of"))
                {
                    seal.active = LastSpellCast.token.Split('_')[2];
                }


                if (talent.steady_focus.enabled)
                {
                    if (LastSpellCast.id == 77767)
                    {
                        BuffProxy.cShots++;
                        if (BuffProxy.cShots == 2)
                        {
                            BuffProxy.cShots = 0;
                            SimcraftImpl.Write(DateTime.Now + ": Steady Shots!");
                        }
                    }
                    if (CobraReset.Contains(LastSpellCast.id) && BuffProxy.cShots > 0)
                    {
                        BuffProxy.cShots = 0;
                    }
                }
            }
        }



        public override void Initialize()
        {
           
            var x = SimCSettings.currentSettings;
        }

        public WoWSpec Specialisation = WoWSpec.None;

        public void ContextChange(object sender, LuaEventArgs args)
        {

            talent.Reset();
            glyph.Reset();
            spell.Reset();
            line_cds.Clear();
            PlayerAuras.Reset();
            PetAuras.Reset();

            _class = StyxWoW.Me.Class;
            //_spec = Me.Specialization;

            var oldctx = Context;
            Context = (StyxWoW.Me.CurrentMap.IsArena || StyxWoW.Me.CurrentMap.IsBattleground)
                ? WoWContext.PvP
                : WoWContext.PvE;
            if (Context != oldctx) SimcraftImpl.Write("Switching to " + Context + " Mode");

            ooc = DateTime.Now;
            RebuildBehaviors();
        }

        public static ProfileSelector SelectorWindow = new ProfileSelector();
        public static DevConsole console = new DevConsole();

        public WoWSpec _spec = WoWSpec.None;


        public override void Start()
        {

            try
            {
                Specialisation = WoWSpec.None;
                NamedComposite.Sequence = "";
                TreeRoot.TicksPerSecond = 8;
                ProfileManager.LoadEmpty();
                //_oldRoutine = RoutineManager.Current; //Save it so we can restore it later
                RoutineManager.Current = new ProxyRoutine();

                Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", UNIT_SPELLCAST_SUCCEEDED);               
                //Lua.Events.AttachEvent("COMBATLOG_EVENT_UNFILTERED", COMBATLOG_EVENT_UNFILTERED);

                Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
                Lua.Events.AttachEvent("PLAYER_LOGOUT", ContextChange);
                Lua.Events.AttachEvent("PLAYER_TALENT_UPDATE", ContextChange);
                Lua.Events.AttachEvent("UNIT_AURA", PlayerAuras.UNIT_AURA);
                Lua.Events.AttachEvent("UNIT_AURA", PetAuras.UNIT_AURA);
                ContextChange(null, null);

                RegisterHotkeys();
            }
            catch (Exception e)
            {
                SimcraftImpl.Write(e.ToString());
            }
        }

        public override void Stop()
        {
            //SimcraftImpl.Write(NamedComposite.Sequence);
            NameCount = 'A';
            TreeRoot.ResetTicksPerSecond();
            //RoutineManager.Current = _oldRoutine;
            UnregisterHotkeys();
            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", UNIT_SPELLCAST_SUCCEEDED);
            Lua.Events.DetachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
            Lua.Events.DetachEvent("PLAYER_LOGOUT", ContextChange);
            Lua.Events.DetachEvent("PLAYER_TALENT_UPDATE", ContextChange);

            Lua.Events.DetachEvent("UNIT_AURA", PlayerAuras.UNIT_AURA);
            Lua.Events.DetachEvent("UNIT_AURA", PetAuras.UNIT_AURA);


        }

        public class AuraProxy : Proxy
        {
            private String Unit;

            private Stopwatch iterationTimer = new Stopwatch();

            public struct Aura
            {
                public uint Spell;
                public Decimal Duration;
                public int Stack;
                public Decimal ExpirationTime;
                public String Caster;
                public String DebuffType;

                private SimcraftImpl simc
                {
                    get { return SimcraftImpl.inst; }
                }

                public TimeSpan TimeLeft()
                {
                    var time = simc.GetTime();
                    var sLeft = ExpirationTime - time;
                    sLeft *= 1000;
                    if (sLeft < 0) return TimeSpan.Zero;
                    return new TimeSpan(0, 0, 0, 0, (int)sLeft);
                }

            }


            public bool GetAuraUp(uint id)
            {
                return auras.ContainsKey(id);
            }

            public TimeSpan GetAuraTimeLeft(uint id)
            {
                if (!auras.ContainsKey(id)) return TimeSpan.Zero;
                return auras[id].TimeLeft();
            }

            public Decimal GetAuraDuration(uint id)
            {
                if (!auras.ContainsKey(id)) return 0;
                return auras[id].Duration;
            }

            public Decimal GetAuraStacks(uint id)
            {
                if (!auras.ContainsKey(id)) return 0;
                return auras[id].Stack;
            }


            public AuraProxy(String unit)
            {
                Unit = unit;
            }

            private SimcraftImpl simc
            {
                get { return SimcraftImpl.inst; }
            }

            private int lastIteCount = -1;

            Dictionary<uint, Aura> auras = new Dictionary<uint, Aura>();

            public void Reset()
            {
                auras.Clear();
            }

            public void UNIT_AURA(object sender, LuaEventArgs args)
            {             
                iterationTimer.Restart();
                //if (lastIteCount == iterationCounter) return;
                //lastIteCount = iterationCounter;
                String u = args.Args[0].ToString();

                if (!u.Equals(Unit)) return;

                auras.Clear();

                int i = 1;

                try
                {
                    var lt =
                        Lua.GetReturnValues(
                            "auras={} for i=1,40 do local u = \"\" local p = \",\" local a,b,c,d,e,f,g,h,j,k,l = UnitAura(\""+Unit+"\",i);" +
                            "a=a and a or u;b=b and b or u;c=c and c or u;d=d and d or u;e=e and e or u;f=f and f or u;g=g and g or u;h=h and h or u;j=j and j or u;k=k and k or u;l=l and l or u; " +
                            "z = d..p..e..p..f..p..g..p..h..p..l; auras[i] = z;" +
                            "end return unpack(auras)");

                    foreach (var ar in lt)
                    {
                       

                        var ars = ar.Split(',');

                        if (ars[5].Length < 2) continue;

                        var a = new Aura
                        {
                            Caster = ars[4].ToString(),
                            Duration = Convert.ToDecimal(ars[2].ToString()),
                            ExpirationTime = Convert.ToDecimal(ars[3].ToString()),
                            Stack = Convert.ToInt32(ars[0].ToString()),
                            
                        };

                        var sid = Convert.ToUInt32(ars[5]);

                        /*if (!dbc.Spells.ContainsKey(sid))
                        {
                            //Logging.Write("No Spell with ID "+sid);
                            continue;
                        }*/

                        a.Spell = sid;

                        auras[sid] = a;

                        //Logging.Write("id({0}) stack({1}) duration({2}) expiration({3}) caster({4})", sid,
                        //    a.Stack, a.Duration, simc.GetTime(), a.TimeLeft());
                    }

                    //Logging.Write(args.FireTimeStamp.ToString() + ": auraite=" + iterationTimer.ElapsedTicks + " checked auras " + auras.Count);

                }
                catch (Exception e)
                {
                    Logging.Write(e.ToString());
                }




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
                        LuaDoString("print('Execution Paused!')");
                        // Make the bot use less resources while paused.
                        TreeRoot.TicksPerSecond = 5;
                    }
                    else
                    {
                        LuaDoString("print('Execution Resumed!')");
                        // Kick it back into overdrive!
                        TreeRoot.TicksPerSecond = 8;
                    }
                });
            HotkeysManager.Register("Simcraft AOE",
                SimCSettings.currentSettings.Aoe.key,
                SimCSettings.currentSettings.Aoe.mod,
                hk =>
                {
                    _aoeEnabled = !_aoeEnabled; 
                    LuaDoString("if _G[\"kane_aoe\"] == true then _G[\"kane_aoe\"] = nil; print('AOE enabled'); else _G[\"kane_aoe\"] = true; print('AOE disabled') end");
                });
            HotkeysManager.Register("Simcraft Cooldowns",
                SimCSettings.currentSettings.Cooldowns.key,
                SimCSettings.currentSettings.Cooldowns.mod,
                hk =>
                {
                    _cdsEnabled = !_cdsEnabled; 
                    LuaDoString("if _G[\"kane_cds\"] == true then _G[\"kane_cds\"] = nil; print('Cds enabled') else _G[\"kane_cds\"] = true; print('Cds disabled') end");


                });
        }

        public static void UnregisterHotkeys()
        {
            HotkeysManager.Unregister("Simcraft Pause");
            HotkeysManager.Unregister("Simcraft Cooldowns");
            HotkeysManager.Unregister("Simcraft AOE");
        }



        public void RebuildBehaviors()
        {
            Composite simcRoot = null;
            NameCount = 'A';

            simcRoot = new PrioritySelector(
                new Action(context =>
                {
                    iterationCounter++;
                    return RunStatus.Failure;
                }),
                new Decorator(ret => IsPaused,
                    new Action(ret => RunStatus.Success)),
                CallActionList(ActionProxy.ActionImpl.oocapl, ret => !StyxWoW.Me.Combat),
                new Decorator(ret => StyxWoW.Me.Combat,
                    new LockSelector(
                        new Decorator(
                            ret => StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.Combat,
                            new PrioritySelector(CombatIteration(), actions.Selector, IterationEnd())))));

            TreeHooks.Instance.ReplaceHook(SimcraftHookName, simcRoot);

            if (Me.Specialization != Specialisation)
            {
                SelectorWindow.ShowDialog();
            }

            Specialisation = Me.Specialization;

        }

        /*public Composite CastOverride()
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
                        if (
         * (OverrideSpell.Spell, OverrideSpell.Target, 3, "SpellOverride"))
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
                _overrideString = LuaGet<String>(set_spelloverride_lua, 0);
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
                LuaDoString(reset_spelloverride_lua);
            }
        }*/



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
        public static bool IsChanneled(this spell_data_t t)
        {
            return test_flag(t.attr1, 2);
        }

        private static bool test_flag(uint attr, uint num)
        {
            uint s = 0x0000000F;

            attr = attr >> (int)(num / 4) * 4;
            attr = attr & s;
            num = num % 4;
            num = (uint)1 << (int)num;

            return attr > 0;
        }


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

        public static string ToLiteral(this string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
  
    }



    #region enums

    #endregion
}

