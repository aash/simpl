//This BotBase was created by Apoc, I take no credit for anything within this code
//I just changed "!StyxWoW.Me.CurrentTarget.IsFriendly" to "!StyxWoW.Me.CurrentTarget.IsHostile"
//For the purpose of allowing RaidBot to work within Arenas

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace Simcraft
{
    public partial class SimcraftImpl : BotBase
    {
        public delegate bool UnitCriteriaDelegate(WoWUnit u);

        private const string SimcraftHookName = "Simcraft.Root";
        private static Boolean _aoeEnabled;
        private static Boolean _damageEnabled;
        private static Boolean _cdsEnabled;
        public static int iterationCounter;
        public static int iterationCache = 1;
        public static DateTime ooc;
        private WoWClass _class = StyxWoW.Me.Class;
        private Stopwatch _clickTimer = new Stopwatch();
        private CombatRoutine _oldRoutine;
        private Composite _root;
        public dynamic actions;
        public BuffProxy buff; // = new BuffProxy();
        public ChiProxy chi; // = new ChiProxy(() => StyxWoW.Me.ToUnit(),this);

        public String CobraReset = "[A Murder of Crows]" + "[Explosive Trap]" + "[Multi-Shot]" + "[Arcane Shot]" +
                                   "[Aimed Shot]" + "[Scatter Shot]" + "[Chimera Shot]" + "[Kill Shot]" + "[Powershot]" +
                                   "[Glaive Toss]" + "[Barrage]" + "[Black Arrow]";

        public ComboPointProxy combo_points;
        public WoWContext Context = WoWContext.PvE;
        public CooldownProxy cooldown; // = new CooldownProxy();
        public DebuffProxy debuff; // = new DebuffProxy();
        public EnergyProxy energy; // = new EnergyProxy(() => StyxWoW.Me.ToUnit(), this);
        public FocusProxy focus; // = new FocusProxy(() => StyxWoW.Me.ToUnit(), this);
        //public static WoWUnit MeU { get { return StyxWoW.Me.ToUnit(); } }
        //public static WoWUnit TargetU { get { return StyxWoW.Me.CurrentTarget; } }

        public HealthProxy health; // = new HealthProxy(() => StyxWoW.Me.ToUnit(), this);
        public Stopwatch IterationTimer = new Stopwatch();
        public double iterationTotal;
        public String LastSpellCast = "";
        public SpellOverride OverrideSpell = new SpellOverride();
        public RageProxy rage; // = new RageProxy(() => StyxWoW.Me.ToUnit(), this);
        public SpellProxy spell; // = new SpellProxy();
        public TalentProxy talent; // = new TalentProxy();
        public TargetProxy target;

        public SimcraftImpl()
        {
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
            actions = new ActionProxy();
            target = new TargetProxy(() => conditionUnit, this);
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

        public SpellProxy action
        {
            get { return spell; }
        }

        public DebuffProxy dot
        {
            get { return debuff; }
        }

        public WoWUnit conditionUnit { get; set; }

        public double Time
        {
            get { return (DateTime.Now - ooc).TotalSeconds; }
        }

        public static void Main()
        {
        }

        public Composite CombatIteration()
        {
            return new Action(delegate
            {
                IterationTimer.Restart();
                if (SpellIsTargeting && Context == WoWContext.PvE)
                {
                    Logging.Write("Clicking: " + clickUnit.Location + " " + iterationCounter);
                    SpellManager.ClickRemoteLocation(clickUnit.Location);
                    clickUnit = null;
                }

                


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
                if (iterationCounter%150 == 0) Logging.Write("avgIte: " + (iterationTotal/iterationCounter));
                //Logging.Write(""+IterationTimer.ElapsedMilliseconds);
            });
        }

        private void CombatEvent(object sender, LuaEventArgs args)
        {
            //Logging.Write(args.Args[0]+" casts "+args.Args[1]);

            if (args.Args[0].ToString().Equals("player"))
            {
                LastSpellCast = args.Args[1].ToString();
                //Lua.DoString("_G[\"kane_spd\"] = \"" + LastSpellCast + "\";");
                if (LastSpellCast.Equals(OverrideSpell.Spell))
                {
                    Logging.Write("Override Spell Cast, disabling");
                    OverrideSpell.Enabled = false;
                }
                if (talent[Focusing_Shot].enabled) return;

                if (LastSpellCast.Equals(Cobra_Shot) || LastSpellCast.Equals(Focusing_Shot))
                {
                    BuffProxy.cShots++;
                    if (BuffProxy.cShots == 2)
                    {
                        BuffProxy.cShots = 0;
                        Logging.Write(DateTime.Now + ": Steady Shots!");
                    }
                }
                if (CobraReset.Contains(LastSpellCast) && BuffProxy.cShots > 0)
                {
                    BuffProxy.cShots = 0;
                }
            }
        }

        public override void Initialize()
        {
            var x = SimCSettings.currentSettings;
            Logging.Write("===============================");
            Logging.Write("LETS HOPE WE DIDNT GET ANY COMPILE ERRORS");
            Logging.Write("===============================");
        }

        private void ContextChange(object sender, LuaEventArgs args)
        {
            talent.Reset();
            spell.Reset();
            _class = StyxWoW.Me.Class;
            var oldctx = Context;
            Context = (StyxWoW.Me.CurrentMap.IsArena || StyxWoW.Me.CurrentMap.IsBattleground)
                ? WoWContext.PvP
                : WoWContext.PvE;
            if (Context != oldctx) Logging.Write("Switching to " + Context + " Mode");
            ooc = DateTime.Now;
            RebuildBehaviors();
        }

        public override void Start()
        {
            try
            {
                NamedComposite.Sequence = "";
                TreeRoot.TicksPerSecond = 8;
                ProfileManager.LoadEmpty();
                _oldRoutine = RoutineManager.Current; //Save it so we can restore it later
                RoutineManager.Current = new ProxyRoutine();

                Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", CombatEvent);
                Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
                Lua.Events.AttachEvent("PLAYER_ENTERING_WORLD", ContextChange);

                ContextChange(null, null);

                RegisterHotkeys();

                StyxWoW.Overlay.Dispatcher.InvokeShutdown();
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
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
                    Lua.DoString("if _G[\"kane_aoe\"] == true then _G[\"kane_aoe\"] = nil; else _G[\"kane_aoe\"] = true; end");
                });
            HotkeysManager.Register("Simcraft Cooldowns",
                SimCSettings.currentSettings.Cooldowns.key,
                SimCSettings.currentSettings.Cooldowns.mod,
                hk =>
                {
                    Lua.DoString("if _G[\"kane_cds\"] == true then _G[\"kane_cds\"] = nil; else _G[\"kane_cds\"] = true; end");
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
            Logging.Write(NamedComposite.Sequence);
            NameCount = 'A';
            TreeRoot.ResetTicksPerSecond();
            RoutineManager.Current = _oldRoutine;
            UnregisterHotkeys();
            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", CombatEvent);
            Lua.Events.DetachEvent("CHARACTER_POINTS_CHANGED", ContextChange);
            Lua.Events.DetachEvent("PLAYER_ENTERING_WORLD", ContextChange);
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
            actions.Reset();

            var methods = typeof (SimcraftImpl).GetMethods()
                .Where(m => m.GetCustomAttributes(typeof (Behavior), false).Length > 0)
                .ToArray();

            foreach (var attr in methods)
            {
                var att = (Behavior) attr.GetCustomAttribute(typeof (Behavior));
                if (att.Match(Me.Class, Me.Specialization, Context))
                {
                    Logging.Write("Detected: " + att);
                    
                    attr.Invoke(this, null);
                }
            }
            CallActionList(ActionProxy.ActionImpl.oocapl);
            /*actions.out_of_combat += new Action(delegate
            {
                Logging.Write("CR: "+Me.CombatReach);
            });*/
        }

        public Composite CastOverride()
        {
            return new PrioritySelector(
                new Action(delegate
                {
                    _conditionSpell = OverrideSpell.Spell;
                    return RunStatus.Failure;
                }),
                new Decorator(ret => OverrideSpell.Enabled && !OverrideSpell.IsLoc,
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
                switch (TargetType)
                {
                    case OverrideType.Target:
                        if (TargetId == OverrideTarget.Target) Target = Me.CurrentTarget;
                        else Target = Me.FocusedUnit;
                        break;
                    case OverrideType.Location:
                        if (TargetId == OverrideTarget.Target) Target = Me.CurrentTarget;
                        else Target = Me.FocusedUnit;
                        break;
                }
                Enabled = true;
                Logging.Write("Spell Override: {0} via {1} on {2}", Spell, TargetType, TargetId);
                Lua.DoString(reset_spelloverride_lua);
            }
        }

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

    #region enums

    public enum WoWContext
    {
        PvP,
        PvE
    }

    public class Behavior : Attribute
    {
        private readonly WoWClass _class;
        private readonly WoWContext _context;
        private readonly WoWSpec _spec;

        public Behavior(WoWClass cl, WoWSpec sp, WoWContext cont)
        {
            _class = cl;
            _spec = sp;
            _context = cont;
        }

        public bool Match(WoWClass cl, WoWSpec sp, WoWContext cont)
        {
            return (cl == _class && sp == _spec && cont == _context);
        }

        public override String ToString()
        {
            return _spec + " " + _class + " in " + _context;
        }
    }

    public enum SpellState
    {
        NoMana,
        CanCast,
        CanNotCast
    }

    #endregion
}