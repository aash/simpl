#region Honorbuddy

#endregion

#region System

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Navigation;
using Bots.DungeonBuddy.Helpers;
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
        public class CacheInternal
        {
            protected Dictionary<WoWGuid, ProxyCacheEntry> _cache = new Dictionary<WoWGuid, ProxyCacheEntry>();

            public class ProxyCacheEntry : DynamicObject
            {
                private readonly Dictionary<String, object> _values = new Dictionary<string, object>();

                public override bool TryGetMember(
                    GetMemberBinder binder, out object result)
                {
                    // Converting the property name to lowercase 
                    // so that property names become case-insensitive. 
                    var name = binder.Name.ToLower();

                    // If the property name is found in a dictionary, 
                    // set the result parameter to the property value and return true. 
                    // Otherwise, return false. 
                    if (!_values.ContainsKey(name)) _values[name] = new CacheValue();
                    return _values.TryGetValue(name, out result);
                }

                public class CacheValue
                {
                    public CacheValue()
                    {
                        Ite = -1;
                    }

                    public int Ite { get; set; }
                    public object Value { get; set; }
                }
            }
        }

        public class ActionProxy : DynamicObject
        {
            private readonly Dictionary<string, object> itemsByName = new Dictionary<string, object>();
            public ActionImpl Selector = new ActionImpl("base");

            

            public ActionImpl this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new ActionImpl(name));
                    return (ActionImpl) itemsByName[name];
                }
                set { itemsByName[name] = value; }
            }

            public void Reset()
            {
                Selector.Children.Clear();
                itemsByName.Clear();
            }

            public override bool TryGetMember(
                GetMemberBinder binder, out object result)
            {
                // Converting the property name to lowercase 
                // so that property names become case-insensitive. 
                var name = binder.Name.ToLower();

                // If the property name is found in a dictionary, 
                // set the result parameter to the property value and return true. 
                // Otherwise, return false. 
                return itemsByName.TryGetValue(name, out result);
            }

            // If you try to set a value of a property that is 
            // not defined in the class, this method is called. 
            public override bool TrySetMember(
                SetMemberBinder binder, object value)
            {
                // Converting the property name to lowercase 
                // so that property names become case-insensitive.
                itemsByName[binder.Name.ToLower()] = value;

                // You can always add a value to a dictionary, 
                // so this method always returns true. 
                return true;
            }

            public static ActionProxy operator +(ActionProxy h1, Composite val)
            {
                h1.Selector.AddChild(val);
                return h1;
            }

            public class ActionImpl : PrioritySelector
            {
                public static String oocapl = "out_of_combat";
                public PrioritySelector Content = new PrioritySelector();
                private PrioritySelector End = new PrioritySelector();
                private PrioritySelector Enter = new PrioritySelector();
                private String name;

                public ActionImpl(String name)
                {
                    this.name = name;
                    AddChild(new Action(delegate
                    {
                        //Logging.Write(DateTime.Now + ": Entering " + name+ " "+Children.Count);
                        Lua.DoString("_G[\"apl\"] = \"" + name.Replace("_", " ") + "\";");
                        if (name.Equals(oocapl)) ooc = DateTime.Now;
                        return RunStatus.Failure;
                    }));
                    //AddChild(Content);
                    //AddChild(new Action(delegate {  }));
                }

                public static ActionImpl operator +(ActionImpl h1, Composite val)
                {
                    h1.AddChild(val);
                    return h1;
                }
            }
        }

        public class BuffProxy : Proxy
        {
            public static int cShots;
            private readonly Dictionary<int, BuffInternal> itemsById = new Dictionary<int, BuffInternal>();
            private readonly Dictionary<string, BuffInternal> itemsByName = new Dictionary<string, BuffInternal>();

            public BuffProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public bool Pre_Steady_Focus
            {
                get { return cShots == 1; }
            }

            public BuffInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new BuffInternal(name));
                    return itemsByName[name];
                }
            }

            public BuffInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new BuffInternal(id));
                    return itemsById[id];
                }
            }

            public class BuffInternal : CacheInternal
            {
                private readonly string _name;
                private readonly int spellid;
                private double _remains;
                private int _stack;
                private bool _up;
                private int lastIteRemains = -1;
                private int lastIteStack = -1;
                private int lastIteUp = -1;

                public BuffInternal(string name)
                {
                    _name = name;
                }

                public BuffInternal(int spellid)
                {
                    this.spellid = spellid;
                }

                public bool down
                {
                    get { return !up; }
                }

                public bool up
                {
                    get
                    {
                        if (lastIteUp + iterationCache < iterationCounter || lastIteUp > iterationCounter)
                        {
                            _up =
                                spellid == 0
                                    ? !Lua.GetReturnVal<bool>(
                                        "name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitAura(\"player\", \"" +
                                        _name + "\"); return name == nil", 0)
                                    : !Lua.GetReturnVal<bool>(
                                        "name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitAura(\"player\", \"" +
                                        spellid + "\"); return name == nil", 0);
                            lastIteUp = iterationCounter;
                        }
                        return _up;
                    }
                }

                public double remains
                {
                    get
                    {
                        if (lastIteRemains + iterationCache < iterationCounter || lastIteRemains > iterationCounter)
                        {
                            _remains = spellid == 0
                                ? GetAuraTimeLeft(StyxWoW.Me.ToUnit(), _name).TotalSeconds
                                : GetAuraTimeLeft(StyxWoW.Me.ToUnit(), spellid).TotalSeconds;
                            lastIteRemains = iterationCounter;
                        }
                        return _remains;
                    }
                }

                public int Stack
                {
                    get
                    {
                        if (lastIteStack + iterationCache < iterationCounter || lastIteStack > iterationCounter)
                        {
                            _stack = spellid == 0
                                ? GetAuraStacks(StyxWoW.Me.ToUnit(), _name)
                                : GetAuraStacks(StyxWoW.Me.ToUnit(), spellid);
                            lastIteStack = iterationCounter;
                        }
                        return _stack;
                    }
                }

                public bool react
                {
                    get { return remains > 0.1; }
                }
            }
        }

        public class CooldownProxy : Proxy
        {
            private readonly Dictionary<int, CooldownInternal> itemsById = new Dictionary<int, CooldownInternal>();

            private readonly Dictionary<string, CooldownInternal> itemsByName =
                new Dictionary<string, CooldownInternal>();

            public CooldownProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public CooldownInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new CooldownInternal(name));
                    return itemsByName[name];
                }
            }

            public CooldownInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new CooldownInternal(id));
                    return itemsById[id];
                }
            }

            public class CooldownInternal : CacheInternal
            {
                private readonly string _name;
                private readonly int spellid;
                private double _remains;
                private bool _up;
                private int lastIteRemains = -1;
                private int lastIteUp = -1;

                public CooldownInternal(string name)
                {
                    _name = name;
                }

                public CooldownInternal(int spellid)
                {
                    this.spellid = spellid;
                }

                public bool Down
                {
                    get { return !up; }
                }

                public bool up
                {
                    get
                    {
                        return remains < 1.5;
                        _up =
                            spellid == 0
                                ? !GetSpellOnCooldown(_name)
                                : !GetSpellOnCooldownId(spellid);
                        lastIteUp = iterationCounter;
                        return _up;
                    }
                }

                public double remains
                {
                    get
                    {
                        _remains = spellid == 0
                            ? GetSpellCooldown(_name).TotalSeconds
                            : GetSpellCooldownId(spellid).TotalSeconds;
                        lastIteRemains = iterationCounter;
                        return _remains;
                    }
                }

                public bool React
                {
                    get { return up; }
                }
            }
        }

        public class SpellProxy : Proxy
        {
            private readonly Dictionary<int, SpellInternal> itemsById = new Dictionary<int, SpellInternal>();
            private readonly Dictionary<string, SpellInternal> itemsByName = new Dictionary<string, SpellInternal>();

            public SpellProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public SpellInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new SpellInternal(name));
                    return itemsByName[name];
                }
            }

            public SpellInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new SpellInternal(id));
                    return itemsById[id];
                }
            }

            public void Reset()
            {
                itemsByName.Clear();
                itemsById.Clear();
            }

            public class SpellInternal : CacheInternal
            {
                private readonly bool _hasMe;
                private readonly string _name;
                private readonly int spellid;
                private double _castime;
                private double _channeltime;
                private int _charges;
                private double _rechargeTime;
                private int lastIteCasttime = -1;
                private int lastIteChanneltime = -1;
                private int lastIteCharges = -1;
                private int lastIteRecharge = -1;

                public SpellInternal(string name)
                {
                    _name = name;
                    _hasMe = SpellManager.HasSpell(name);
                    Logging.Write(_hasMe ? Colors.Green : Colors.Red, (_hasMe ? "Found " : "Missing ") + name);
                }

                public SpellInternal(int spellid)
                {
                    this.spellid = spellid;
                    _hasMe = SpellManager.HasSpell(spellid);
                }

                public double Range
                {
                    get
                    {

                        var spell = spellid == 0
                            ? GetSpell(_name)
                            : GetSpell(spellid);

                        return spell.HasRange ? spell.MaxRange : Me.CombatReach+5;
                    }
                }

                public int Charges
                {
                    get
                    {
                        if (!_hasMe) return 0;
                        if (lastIteCharges + iterationCache < iterationCounter || lastIteCharges > iterationCounter)
                        {
                            _charges = spellid == 0
                                ? Lua.GetReturnVal<int>(
                                    "currentCharges,_,_,_,_ = GetSpellCharges(\"" + _name + "\"); return currentCharges",
                                    0)
                                : Lua.GetReturnVal<int>(
                                    "currentCharges,_,_,_,_ = GetSpellCharges(" + spellid + "); return currentCharges",
                                    0);
                            lastIteCharges = iterationCounter;
                        }
                        return _charges;
                    }
                }

                public double CastTime
                {
                    get
                    {
                        if (!_hasMe) return 0;
                        if (lastIteCasttime + iterationCache < iterationCounter || lastIteCasttime > iterationCounter)
                        {
                            _castime = spellid == 0
                                ? (double) GetSpell(_name).CastTime/1000
                                : (double) GetSpell(spellid).CastTime/1000;
                            lastIteCasttime = iterationCounter;
                        }
                        return _castime;
                    }
                }

                public double ChannelTime
                {
                    get
                    {
                        if (!_hasMe) return 0;
                        if (lastIteChanneltime + iterationCache < iterationCounter ||
                            lastIteChanneltime > iterationCounter)
                        {
                            _channeltime = spellid == 0
                                ? (double) GetSpell(_name).BaseDuration/1000
                                : (double) GetSpell(spellid).BaseDuration/1000;
                            lastIteChanneltime = iterationCounter;
                        }
                        return _channeltime;
                    }
                }

                public double RechargeTime
                {
                    get
                    {
                        if (!_hasMe) return 0;
                        if (lastIteRecharge + iterationCache < iterationCounter || lastIteRecharge > iterationCounter)
                        {
                            _rechargeTime = spellid == 0
                                ? Lua.GetReturnVal<double>(
                                    "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                                    _name +
                                    "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return cd",
                                    0)
                                : Lua.GetReturnVal<double>(
                                    "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                                    spellid +
                                    "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return cd",
                                    0);
                            lastIteRecharge = iterationCounter;
                        }
                        return _rechargeTime;
                    }
                }
            }
        }

        public class TargetProxy : Proxy
        {
            public HealthProxy health;

            public TargetProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
                health = new HealthProxy(() => GetUnit(), simc);
            }

            public double time_to_die
            {
                get { return health.TimeToDie; }
            }

            public bool melee_range
            {
                get { return GetUnit().IsWithinMeleeRange; }
            }

            public bool facing
            {
                get { return Me.IsFacing(GetUnit()); }
            }

            public WoWPoint Location
            {
                get { return GetUnit().Location; }
            }
        }

        public class DebuffProxy : Proxy
        {
            private readonly Dictionary<int, DebuffInternal> DebuffsById = new Dictionary<int, DebuffInternal>();
            private readonly Dictionary<string, DebuffInternal> DebuffsByName = new Dictionary<string, DebuffInternal>();

            public DebuffProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public DebuffInternal this[String name]
            {
                get
                {
                    if (!DebuffsByName.ContainsKey(name)) DebuffsByName.Add(name, new DebuffInternal(name, this));
                    return DebuffsByName[name];
                }
            }

            public DebuffInternal this[int id]
            {
                get
                {
                    if (!DebuffsById.ContainsKey(id)) DebuffsById.Add(id, new DebuffInternal(id, this));
                    return DebuffsById[id];
                }
            }

            public class DebuffInternal : CacheInternal
            {
                private readonly string _name;
                private readonly DebuffProxy _owner;
                private readonly int spellid;

                public DebuffInternal(string name, DebuffProxy owner)
                {
                    _name = name;
                    _owner = owner;
                }

                public DebuffInternal(int spellid, DebuffProxy owner)
                {
                    this.spellid = spellid;
                    _owner = owner;
                }

                public bool down
                {
                    get { return !Up; }
                }

                public bool Up
                {
                    get
                    {
                        var guid = _owner.GetUnit().Guid;
                        if (!_cache.ContainsKey(guid)) _cache[guid] = new ProxyCacheEntry();
                        dynamic cach = _cache[guid];
                        if (cach.Up.Ite + iterationCache < iterationCounter || cach.Up.Ite > iterationCounter)
                        {
                            cach.Up.Value =
                                spellid == 0
                                    ? GetAuraUp(_owner.GetUnit(), _name, true)
                                    : GetAuraUp(_owner.GetUnit(), spellid, true);
                            cach.Up.Ite = iterationCounter;
                        }
                        return cach.Up.Value;
                    }
                }

                public double remains
                {
                    get
                    {
                        var guid = _owner.GetUnit().Guid;
                        if (!_cache.ContainsKey(guid)) _cache[guid] = new ProxyCacheEntry();
                        dynamic cach = _cache[guid];
                        if (cach.remains.Ite + iterationCache < iterationCounter || cach.remains.Ite > iterationCounter)
                        {
                            cach.remains.Value = spellid == 0
                                ? GetAuraTimeLeft(_owner.GetUnit(), _name, true).TotalSeconds
                                : GetAuraTimeLeft(_owner.GetUnit(), spellid, true).TotalSeconds;
                            cach.remains.Ite = iterationCounter;
                        }
                        return cach.remains.Value;
                    }
                }

                public int Stack
                {
                    get
                    {
                        var guid = _owner.GetUnit().Guid;
                        if (!_cache.ContainsKey(guid)) _cache[guid] = new ProxyCacheEntry();
                        dynamic cach = _cache[guid];
                        if (cach.stack.Ite + iterationCache < iterationCounter || cach.stack.Ite > iterationCounter)
                        {
                            cach.stack.Value = spellid == 0
                                ? GetAuraStacks(_owner.GetUnit(), _name, true)
                                : GetAuraStacks(_owner.GetUnit(), spellid, true);
                            cach.stack.Ite = iterationCounter;
                        }
                        return cach.stack.Value;
                    }
                }

                public bool Ticking
                {
                    get { return Up; }
                }
            }
        }

        public class TalentProxy : Proxy
        {
            private readonly Dictionary<String, Talent> _talents = new Dictionary<string, Talent>();

            public TalentProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public Talent this[String name]
            {
                get
                {
                    if (!_talents.ContainsKey(name)) _talents.Add(name, new Talent(name));
                    return _talents[name];
                }
            }

            public void Reset()
            {
                _talents.Clear();
            }

            public class Talent
            {
                private readonly string _name;
                private bool? _enabled;

                public Talent(string name)
                {
                    _name = name;
                }

                public bool enabled
                {
                    get
                    {
                        if (_enabled == null) _enabled = StyxWoW.Me.GetLearnedTalents().Count(a => a.Name == _name) > 0;
                        return _enabled.Value;
                    }
                }
            }
        }

        /*public static class WoWUnit
        {
            {
                Health = new HealthProxy(del, simc);
            }

            public HealthProxy Health;// = new HealthProxy(GetUnit, simc);
            public int TimeToDie = 3600;
        }*/

        public abstract class Proxy
        {
            public delegate WoWUnit GetUnitDelegate();

            protected GetUnitDelegate GetUnit;
            protected int lastIte = 0;
            protected SimcraftImpl simc;

            public Proxy(GetUnitDelegate del, SimcraftImpl simc)
            {
                GetUnit = del;
                this.simc = simc;
            }
        }

        public abstract class ResourceProxy : Proxy
        {
            private double _crt;
            private double _max;
            private double _pct;
            private int lastIteCrt = -1;
            private int lastIteMax = -1;
            private int lastItePct = -1;

            public ResourceProxy(GetUnitDelegate del, SimcraftImpl simc) : base(del, simc)
            {
            }

            public double time_to_max
            {
                get { return deficit/Regen; }
            }

            public double pct
            {
                get
                {
                    if (lastItePct + iterationCache < iterationCounter || lastItePct > iterationCounter)
                    {
                        _pct = GetPercent;
                        lastItePct = iterationCounter;
                    }
                    return _pct;
                }
            }

            public double Current
            {
                get
                {
                    if (lastIteCrt + iterationCache < iterationCounter || lastIteCrt > iterationCounter)
                    {
                        _crt = GetCurrent;
                        lastIteCrt = iterationCounter;
                    }
                    return _crt;
                }
            }

            public double Max
            {
                get
                {
                    if (lastIteMax + iterationCache < iterationCounter || lastIteMax > iterationCounter)
                    {
                        _max = GetMax;
                        lastIteMax = iterationCounter;
                    }
                    return _max;
                }
            }

            public abstract double GetPercent { get; }
            public abstract int GetMax { get; }
            public abstract int GetCurrent { get; }

            public int deficit
            {
                get { return GetUnit() != null ? (int) (Max - Current) : 0; }
            }

            public double Regen { get; private set; }

            public double Cast_Regen(double seconds)
            {
                return Regen*seconds;
            }

            public static bool operator <(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.Current < val;
            }

            public static bool operator >(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.Current > val;
            }

            public static bool operator <=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.Current <= val;
            }

            public static bool operator >=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.Current >= val;
            }

            public static bool operator !=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.Current != val : false;
            }

            public static bool operator ==(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.Current == val : false;
            }

            public static double operator -(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val - h1.Current : 0;
            }

            public static double operator -(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.Current - val : 0;
            }

            public static double operator +(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val + h1.Current : 0;
            }

            public static double operator +(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? val + h1.Current : 0;
            }

            public static double operator *(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val*h1.Current : 0;
            }

            public static double operator *(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? val*h1.Current : 0;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ResourceProxy)) return false;
                return ((ResourceProxy) obj).Current == Current;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public void Pulse()
            {
                Regen = Lua.GetReturnVal<double>(
                    "inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen;", 0);
            }
        }

        public class HealthProxy : ResourceProxy
        {
            public static int AVG_DPS = 22000;

            public HealthProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override double GetPercent
            {
                get { return GetUnit().HealthPercent; }
            }

            public override int GetCurrent
            {
                get { return GetUnit() != null ? (int) (GetUnit().CurrentHealth) : 0; }
            }

            public override int GetMax
            {
                get { return GetUnit() != null ? (int) (GetUnit().MaxHealth) : 0; }
            }

            public double TimeToDie
            {
                get
                {
                    var g = (int) StyxWoW.Me.GroupInfo.GroupSize;
                    if (g > 0)
                        return GetCurrent/(StyxWoW.Me.GroupInfo.GroupSize*0.6*AVG_DPS);
                    return GetCurrent/(AVG_DPS);
                }
            }
        }

        public class ComboPointProxy : ResourceProxy
        {
            public ComboPointProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override double GetPercent
            {
                get { return 0; }
            }

            public override int GetCurrent
            {
                get { return GetUnit() != null ? StyxWoW.Me.ComboPoints : 0; }
            }

            public override int GetMax
            {
                get { return 5; }
            }
        }

        public class ChiProxy : ResourceProxy
        {
            public ChiProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override int GetMax
            {
                get { return (int) GetUnit().MaxChi; }
            }

            public override int GetCurrent
            {
                get { return (int) GetUnit().CurrentChi; }
            }

            public override double GetPercent
            {
                get { return 0; }
            }
        }

        public class EnergyProxy : ResourceProxy
        {
            public EnergyProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override double GetPercent
            {
                get { return StyxWoW.Me.EnergyPercent; }
            }

            public override int GetMax
            {
                get { return GetUnit() != null ? (int) (GetUnit().MaxEnergy) : 0; }
            }

            public override int GetCurrent
            {
                get { return GetUnit() != null ? (int) (GetUnit().CurrentEnergy) : 0; }
            }
        }

        public class FocusProxy : ResourceProxy
        {
            public FocusProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override double GetPercent
            {
                get { return StyxWoW.Me.FocusPercent; }
            }

            public override int GetMax
            {
                get { return GetUnit() != null ? (int) (GetUnit().MaxFocus) : 0; }
            }

            public override int GetCurrent
            {
                get { return GetUnit() != null ? (int) (GetUnit().CurrentFocus) : 0; }
            }
        }

        public class RageProxy : ResourceProxy
        {
            public RageProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override double GetPercent
            {
                get { return StyxWoW.Me.RagePercent; }
            }

            public override int GetMax
            {
                get { return GetUnit() != null ? (int) (GetUnit().MaxRage) : 0; }
            }

            public override int GetCurrent
            {
                get { return GetUnit() != null ? (int) (GetUnit().CurrentRage) : 0; }
            }
        }
    }
}