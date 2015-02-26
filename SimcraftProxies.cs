#region Honorbuddy

#endregion

#region System

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Navigation;
using Bots.DungeonBuddy.Helpers;
using JetBrains.Annotations;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

#endregion

namespace Simcraft
{

    public partial class SimcraftImpl
    {

        public class SetBonusProxy : DynamicObject
        {

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = false;
                try
                {
                    var n = binder.Name;
                    var splits = n.Split('_');
                    var set = dbc.Sets[splits[0]];

                    int count = 0;
                    foreach (var it in Me.Inventory.Equipped.Items)
                    {
                        if (it == null) continue;
                        
                        var i = (int) it.ItemInfo.Id;
                                       
                        if (set.Contains(i)) count++;
                    }
                    //Logging.Write(splits[1]+" "+count);
                    result = (Convert.ToInt32(splits[1][0]) <= count);
                    //SimcraftImpl.Write(""+result);
                }
                catch (Exception e)
                {
                    //SimcraftImpl.Write(e.ToString());
                    result = false;
                    return true;
                }
                return true;
            }
        }

        public class DiseaseProxy
        {

            public bool max_ticking
            {
                get
                {
                    bool default_value = true;
                    bool ret = default_value;

                    var val =  simc.debuff["Blood Plague"].ticking;
                    if (val && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Frost Fever"].ticking;
                    if (val && !!simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Necrotic Plague"].ticking;
                    if (val && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = false;

                    return ret;
                }
            }

            public bool min_ticking
            {
                get
                {
                    bool default_value = true;
                    bool ret = default_value;
                    var val = simc.debuff["Blood Plague"].ticking;
                    if (!val && !simc.talent.necrotic_plague.enabled)
                        return val;
                    val = simc.debuff["Frost Fever"].ticking;
                    if (!val && !simc.talent.necrotic_plague.enabled)
                        return val;
                    val = simc.debuff["Necrotic Plague"].ticking;
                    if (!val && simc.talent.necrotic_plague.enabled)
                        return val;

                    if (ret == default_value)
                        ret = false;

                    return ret;
                }
            }

            /*
            if ( np_expr )
            {
              double val = np_expr -> eval();
              if ( type == TYPE_NONE && val != 0 )
                return val;
              else if ( type == TYPE_MIN && val < ret )
                ret = val;
              else if ( type == TYPE_MAX && val > ret )
                ret = val;
            }
            */
            public double min_remains
            {
                get
                {
                    var default_value = double.MaxValue;
                    var ret = default_value;
           
                    var val = simc.debuff["Blood Plague"].remains;
                    if (val < ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Frost Fever"].remains;
                    if (val < ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Necrotic Plague"].remains;
                    if (val < ret && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = 0;

                    return ret;
                }
            }

            public double max_remains
            {
                get
                {
                    var default_value = double.MinValue;
                    var ret = default_value;

                    var val = simc.debuff["Blood Plague"].remains;
                    if (val > ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Frost Fever"].remains;
                    if (val > ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff["Necrotic Plague"].remains;
                    if (val > ret && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = 0;

                    return ret;
                }
            }

            public bool ticking
            {
                get
                {
                    var val = simc.debuff["Blood Plague"].ticking;
                    if (val)
                        return val;
                    val = simc.debuff["Frost Fever"].ticking;
                    if (val)
                        return val;
                    val = simc.debuff["Necrotic Plague"].ticking;
                    if (val)
                        return val;

                    return false;
                }           
            }

            private SimcraftImpl simc;

            public DiseaseProxy(SimcraftImpl simc)
            {
                this.simc = simc;
            }

        }

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

        public class StatProxy
        {


            public double crit
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.CritMelee); }
            }

            public double haste
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.HasteMelee); }
            }

            public double mastery
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.Mastery); }
            }

            public double bonusarmor
            {
                //unfi
                get { return Me.GetCombatRating(WoWPlayerCombatRating.CritMelee); }
            }


            public double multistrike
            {
                //Unfi
                get { return Lua.GetReturnVal<int>("return GetCombatRating(CR_MULTISTRIKE);", 0); }
            }

            public double spellpower
            {
                get { return Lua.GetReturnVal<int>("spellDmg = GetSpellBonusDamage(3); return spellDmg;",0); }
            }

            public double dodge
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.Dodge); }
            }

            public double parry
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.Parry); }
            }

            public double block
            {
                get { return Me.GetCombatRating(WoWPlayerCombatRating.Block); }
            }


            public double spell_haste
            {
                get
                {
                    return Lua.GetReturnVal<double>("return UnitSpellHaste(\"player\")", 0);
                }
            }

            public double multistrike_pct
            {
                get
                {
                    return Lua.GetReturnVal<double>("return GetMultistrike()", 0);
                }
            }

            public double mastery_value
            {
                get { return Lua.GetReturnVal<double>("mastery, coefficient = GetMasteryEffect(); return mastery", 0); }
            }


        }

        public class ActionProxy : DynamicObject
        {
            private readonly Dictionary<string, object> itemsByName = new Dictionary<string, object>();
            public ActionImpl Selector = new ActionImpl("base");

            public override string ToString()
            {
                var str = "";
                foreach (var item in itemsByName)
                {
                    str += item.ToString();
                }
                return str;
            }

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
                 
                if (!itemsByName.ContainsKey(name))
                    itemsByName[name] = new ActionImpl(name);

                result = itemsByName[name];

                return true;
            }

            public static ActionProxy operator +(ActionProxy h1, Composite val)
            {
                h1.Selector.AddChild(val);
                return h1;
            }

            public class ActionImpl : PrioritySelector
            {
                public override RunStatus Tick(object context)
                {
                    try
                    {
                        return base.Tick(context);
                    }
                    catch (Exception e)
                    {
                        Write(e.ToString());
                    }
                    return RunStatus.Failure;
                    
                }

                public static String oocapl = "out_of_combat";
                public static String current_apl = oocapl;
                private String name;
                private String oldapl;

                private PrioritySelector head = new PrioritySelector();
                private PrioritySelector body = new PrioritySelector();
                private PrioritySelector foot = new PrioritySelector();

                public ActionImpl(String name)
                {
                    this.name = name;
                    head.AddChild(new Action(delegate
                    {                    
                        oldapl = current_apl;
                        current_apl = name;
                        Lua.DoString("_G[\"apl\"] = \"" + current_apl + "\";");

                        if (name.Equals(oocapl)) ooc = DateTime.Now;
                        return RunStatus.Failure;
                    }));

                    foot.AddChild(new Action(delegate
                    {
                        Lua.DoString("_G[\"apl\"] = \"" + oldapl + "\";");
                        current_apl = oldapl;
                        
                        //SimcraftImpl.Write(oldapl);
                        return RunStatus.Failure;
                    }));

                    AddChild(head);
                    AddChild(body);
                    AddChild(foot);
                    //AddChild(new Action(delegate {  }));
                }

                public void Add(Composite h1)
                {
                    body.AddChild(h1);
                }

                public static ActionImpl operator +(ActionImpl h1, Composite val)
                {
                    h1.body.AddChild(val);
                    return h1;
                }
            }
        }



   

        public class BuffProxy : Proxy
        {

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.Spells);
                _resolvePriority.Add(dbc.ClassSpells);
            }

            public override CacheInternal NewInternal(string name)
            {
                return new BuffInternal(name);
            }

            public override CacheInternal NewInternal(int name)
            {
                 return  new BuffInternal(name);
            }

            public static int cShots;

            public BuffProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
                overrides["pre_steady_focus"] = new PreSteadyFocus();
                overrides["bloodlust"] = new Bloodlust(simc);
                overrides["potion"] = new Potion(simc);
                overrides["anytrinket"] = new AnyTrinket(simc);
               
            }

            public IncantersFlow incanters_flow = new IncantersFlow("incanters_flow");

            public class IncantersFlow : BuffInternal
            {
                private int oldstack = 0;
                public int dir = 1;
                public IncantersFlow(string name) : base(name)
                {
                }

                public override int stack
                {
                    get
                    {
                        var bstack = base.stack;
                        if (oldstack < bstack) dir = 1;
                            else if (oldstack > bstack)  dir = -1;
                        oldstack = bstack;
                        return bstack;
                    }
                }
            }

            public class PreSteadyFocus : BuffInternal
            {
                public PreSteadyFocus() : base("pre_steady_focus")
                {
                }

                public override MagicValueType up
                {
                    get { return new MagicValueType(cShots == 1); }
                }
            }

            public class Potion : BuffInternal
            {
                private SimcraftImpl simc;

                public Potion(SimcraftImpl simc)
                    : base("potion")
                {
                    this.simc = simc;
                }

                public override MagicValueType up
                {
                    get { return new MagicValueType(simc.buff[simc.PotionName].up); }
                } 
            }

            public class AnyTrinket : BuffInternal
            {
                private SimcraftImpl simc;

                public AnyTrinket(SimcraftImpl simc)
                    : base("anytrinket")
                {
                    this.simc = simc;
                }

                public List<String> get_procs
                {
                    get
                    {
                        List<String> procs = new List<string>();

                        var t1 = Me.Inventory.GetItemBySlot((uint)WoWEquipSlot.Trinket1);
                        var t2 = Me.Inventory.GetItemBySlot((uint)WoWEquipSlot.Trinket2);

                        if (dbc.ItemProcs.ContainsKey((int)t2.ItemInfo.Id))
                        {
                            procs.Add(dbc.ItemProcs[(int)t2.ItemInfo.Id]);
                        }
                        if (dbc.ItemProcs.ContainsKey((int)t1.ItemInfo.Id))
                        {
                            procs.Add(dbc.ItemProcs[(int)t1.ItemInfo.Id]);
                        }
                        return procs;
                    }
                } 

                public override MagicValueType up
                {
                    get
                    {
                        foreach (var proc in get_procs)
                        {
                            if (simc.buff[DBGetSpell(proc)].up) return up;
                        }
                        return new MagicValueType(false); 
                    }
                }

                public override MagicValueType remains
                {
                    get
                    {
                        double dur = Double.MinValue;
                        foreach (var proc in get_procs)
                        {
                            var r = simc.buff[DBGetSpell(proc)].remains;
                            if (dur < r) dur = r;
                        }
                        return new MagicValueType(dur);
                    }
                }

                public override int stack
                {
                    get
                    {
                        int dur = int.MinValue;
                        foreach (var proc in get_procs)
                        {
                            var r = simc.buff[DBGetSpell(proc)].Stack;
                            if (dur < r) dur = r;
                        }
                        return dur;
                    }


                }

            }



            public class Bloodlust : BuffInternal
            {
                private SimcraftImpl simc;

                public Bloodlust(SimcraftImpl simc)
                    : base("bloodlust")
                {
                    this.simc = simc;
                }

                public override MagicValueType up
                {
                    get { return new MagicValueType(simc.buff["Heroism"].up || simc.buff["Bloodlust"].up || simc.buff["Time_Warp"].up); }
                }

                public override MagicValueType remains
                {
                    get
                    {
                        return new MagicValueType(simc.buff["Heroism"].up
                            ? simc.buff["Heroism"].remains
                            : simc.buff["Bloodlust"].up ? simc.buff["Bloodlust"].remains : simc.buff["Time_Warp"].up ? simc.buff["Time_Warp"].remains : 0.0);
                    }
                }

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
                    return (BuffInternal)itemsByName[name];
                }
            }

            public BuffInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new BuffInternal(id));
                    return (BuffInternal)itemsById[id];
                }
            }

            public class BuffInternal : CacheInternal
            {
                private readonly string _name;
                private readonly int spellid;
                private MagicValueType _remains;
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

                public virtual MagicValueType down
                {
                    get { return !up; }
                }

                public virtual MagicValueType up
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
                        return new MagicValueType(_up);
                    }
                }

                public virtual MagicValueType remains
                {
                    get
                    {
                        if (lastIteRemains + iterationCache < iterationCounter || lastIteRemains > iterationCounter)
                        {
                            _remains = new MagicValueType(spellid == 0
                                ? GetAuraTimeLeft(StyxWoW.Me.ToUnit(), _name).TotalSeconds
                                : GetAuraTimeLeft(StyxWoW.Me.ToUnit(), spellid).TotalSeconds);
                            lastIteRemains = iterationCounter;
                        }
                        return _remains;
                    }
                }

                public virtual int stack
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

                public virtual MagicValueType react
                {
                    get { return new MagicValueType(remains > (Decimal)0.1); }
                }
            }
        }

        public class ObliterateProxy
        {
            public MagicValueType ready_in
            {
                get
                {
                    var t =
                        Lua.GetReturnVal<double>(
                            "start, duration, enabled = GetSpellCooldown('Obliterate'); t =GetTime()-start; if t < 1 then return 1-t else return 10-t end",
                            0);
                    if (t < 0) return new MagicValueType(0);
                    return new MagicValueType(t);
                }
            }
        }


        public class ItemProxy
        {

            private MagicValueType _remains;

            public ItemProxy cooldown
            {
                get { return this; }
            }

            private WoWEquipSlot slot;

            public ItemProxy(WoWEquipSlot slot)
            {
                this.slot = slot;
            }

            public MagicValueType Down
            {
                get { return !up; }
            }

            public virtual MagicValueType up
            {
                get
                {
                    return remains < 1.5;
                }
            }

            public virtual MagicValueType remains
            {
                get
                {
                    _remains = new MagicValueType(Me.Inventory.GetItemBySlot((uint) slot).CooldownTimeLeft.TotalSeconds);
                    return _remains;
                }
            }

            public virtual MagicValueType React
            {
                get { return up; }
            }
        }



        public class CooldownProxy : Proxy
        {

            public override CacheInternal NewInternal(string name)
            {
                return new CooldownInternal(name);
            }

            public override CacheInternal NewInternal(int name)
            {
                return new CooldownInternal(name);
            }

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.ClassSpells);
                _resolvePriority.Add(dbc.Spells);
                

            }

            public CooldownProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
                overrides["potion"] = new BuffProxy.Potion(simc);
            }

            public class Potion : CooldownInternal
            {
                private SimcraftImpl simc;

                public Potion(SimcraftImpl simc)
                    : base("potion")
                {
                    this.simc = simc;
                }

                public override MagicValueType up
                {
                    get { return !(simc.PotionCooldown > 0); }
                }
            }


            public CooldownInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new CooldownInternal(name));
                    return (CooldownInternal)itemsByName[name];
                }
            }

            public CooldownInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new CooldownInternal(id));
                    return (CooldownInternal)itemsById[id];
                }
            }

            public class CooldownInternal : CacheInternal
            {
                private readonly string _name;
                private readonly int spellid;
                private MagicValueType _remains;
                private int lastIteRemains = -1;

                public CooldownInternal(string name)
                {
                    _name = name;
                }

                public CooldownInternal(int spellid)
                {
                    this.spellid = spellid;
                }

                public MagicValueType Down
                {
                    get { return !up; }
                }

                public virtual MagicValueType up
                {
                    get
                    {
                        return remains < 1.5;
                    }
                }

                public virtual MagicValueType remains
                {
                    get
                    {
                        _remains = new MagicValueType(spellid == 0
                            ? GetSpellCooldown(_name).TotalSeconds
                            : GetSpellCooldownId(spellid).TotalSeconds);
                        lastIteRemains = iterationCounter;
                        return _remains;
                    }
                }

                public virtual MagicValueType React
                {
                    get { return up; }
                }
            }
        }

        public class PetProxy : DynamicObject
        {
            public PetCooldownProxy cooldown = new PetCooldownProxy();
            public PetBuffProxy buff = new PetBuffProxy();

            public Dictionary<String, PetProxy> children = new Dictionary<String, PetProxy>(); 

            private String name = "def";

            public class ElementalTotem
            {
                private WoWTotemType type;

                public ElementalTotem(WoWTotemType type)
                {
                    this.type = type;
                }

                public MagicValueType active
                {
                    get
                    {
                        foreach (var tinfo in Me.Totems)
                        {
                            if (!tinfo.Expired && tinfo.Type == type)
                                return  new MagicValueType( true);
                        }
                        return new MagicValueType(false);
                    }
                }

                public MagicValueType remains
                {
                    get
                    {
                        foreach (var tinfo in Me.Totems)
                        {
                            if (!tinfo.Expired && tinfo.Type == type)
                            {
                                var diff = (tinfo.Expires - DateTime.Now).TotalSeconds;
                                return new MagicValueType(diff >= 0 ? diff : 0);
                            }
                        }
                        return new MagicValueType(0);
                    }
                }

            }

            public MagicValueType remains
            {
                get
                {
                    foreach (var tinfo in Me.Totems)
                    {
                        if (Tokenize(tinfo.Name).Equals(name))
                        {
                            //tinfo.
                            var diff = (tinfo.Expires - DateTime.Now).TotalSeconds;
                            return new MagicValueType(diff >= 0 ? diff : 0);
                        } 
                    }
                    if (Me.Pet != null) return new MagicValueType(1);
                    return new MagicValueType(0);
                }
            }

            public virtual MagicValueType active 
            {
                get { return remains > 0; }
            }

            public ElementalTotem fire = new ElementalTotem(WoWTotemType.Fire);
            public ElementalTotem water = new ElementalTotem(WoWTotemType.Water);
            public ElementalTotem air = new ElementalTotem(WoWTotemType.Air);
            public ElementalTotem earth = new ElementalTotem(WoWTotemType.Earth);

            public PetProxy()
            {
                
            }

            public PetProxy(String name)
            {
                this.name = name;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (!children.ContainsKey(binder.Name)) 
                    children[binder.Name] = new PetProxy(binder.Name);
                result = children[binder.Name];
                //result = this;
                return true;
            }

            public class PetCooldownProxy : DynamicObject
            {

                public override bool TryGetMember(GetMemberBinder binder, out object result)
                {
                    result = new CooldownInternal(DBGetSpell(binder.Name).Name);
                    return true;
                }

                public class CooldownInternal : CacheInternal
                {
                    private readonly string _name;
                    private readonly int spellid;
                    private MagicValueType _remains;
                    private int lastIteRemains = -1;

                    public CooldownInternal(string name)
                    {
                        _name = name;
                    }

                    public CooldownInternal(int spellid)
                    {
                        this.spellid = spellid;
                    }

                    public MagicValueType Down
                    {
                        get { return !up; }
                    }

                    public virtual MagicValueType up
                    {
                        get
                        {
                            return remains < 1.5;
                        }
                    }

                    public virtual MagicValueType remains
                    {
                        get
                        {
                            _remains = new MagicValueType(Lua.GetReturnVal<double>("start, duration, enabled = GetSpellCooldown(\"" + _name + "\"); t =GetTime()-start; return duration-t;", 0));
                            lastIteRemains = iterationCounter;
                            if (_remains < 0) _remains = new MagicValueType(0);
                            return _remains;
                        }
                    }

                    public virtual bool React
                    {
                        get { return up; }
                    }
                }              
            }



            public class PetBuffProxy : DynamicObject
            {

                public PrismaticCrystal prismatic_crystal = new PrismaticCrystal();

                public class PrismaticCrystal
                {
                    public bool active
                    {
                        get { return Me.Minions.Any(ret => ret.Name == "Prismatic Crystal"); }
                    }

                    public MagicValueType remains
                    {
                        get
                        {
                            if (Me.Totems.FirstOrDefault() != default(WoWTotemInfo))
                                return new MagicValueType((Me.Totems.First().Expires - DateTime.Now).TotalSeconds);
                            return new MagicValueType(0);
                        }
                       
                    }

                }

                public override bool TryGetMember(GetMemberBinder binder, out object result)
                {
                    result = new BuffInternal(DBGetSpell(binder.Name).Name);
                    return true;
                }


                public class BuffInternal : CacheInternal
                {
                    private readonly string _name;
                    private readonly int spellid;
                    private MagicValueType _remains;
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

                    public virtual MagicValueType down
                    {
                        get { return !up; }
                    }

                    public virtual MagicValueType up
                    {
                        get
                        {
                            if (lastIteUp + iterationCache < iterationCounter || lastIteUp > iterationCounter)
                            {
                                _up =
                                    spellid == 0
                                        ? !Lua.GetReturnVal<bool>(
                                            "name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitAura(\"playerpet\", \"" +
                                            _name + "\"); return name == nil", 0)
                                        : !Lua.GetReturnVal<bool>(
                                            "name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitAura(\"playerpet\", \"" +
                                            spellid + "\"); return name == nil", 0);
                                lastIteUp = iterationCounter;
                            }
                            return new MagicValueType(_up);
                        }
                    }

                    public virtual MagicValueType remains
                    {
                        get
                        {
                            if (lastIteRemains + iterationCache < iterationCounter || lastIteRemains > iterationCounter)
                            {
                                _remains = new MagicValueType(spellid == 0
                                    ? GetAuraTimeLeft(StyxWoW.Me.ToUnit().Pet, _name).TotalSeconds
                                    : GetAuraTimeLeft(StyxWoW.Me.ToUnit().Pet, spellid).TotalSeconds);
                                lastIteRemains = iterationCounter;
                            }
                            return _remains;
                        }
                    }

                    public virtual MagicValueType Stack
                    {
                        get
                        {
                            if (lastIteStack + iterationCache < iterationCounter || lastIteStack > iterationCounter)
                            {
                                _stack = spellid == 0
                                    ? GetAuraStacks(StyxWoW.Me.ToUnit().Pet, _name)
                                    : GetAuraStacks(StyxWoW.Me.ToUnit().Pet, spellid);
                                lastIteStack = iterationCounter;
                            }
                            return new MagicValueType(_stack);
                        }
                    }

                    public virtual MagicValueType react
                    {
                        get { return new MagicValueType(remains > (Decimal)0.1); }
                    }
                }
            }
        }

        public class SealProxy : DynamicObject
        {
            public String active { get; set; }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = active.Equals(binder.Name);
                return true;
            }
        }

        public class SpellProxy : Proxy
        {

            public override CacheInternal NewInternal(string name)
            {
                return new SpellInternal(name);
            }

            public override CacheInternal NewInternal(int name)
            {
                return new SpellInternal(name);
            }

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.ClassSpells);
                _resolvePriority.Add(dbc.Spells);
            }

            public SpellProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public SpellInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new SpellInternal(name));
                    return (SpellInternal)itemsByName[name];
                }
            }

            public SpellInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new SpellInternal(id));
                    return (SpellInternal)itemsById[id];
                }
            }

            public void Reset()
            {
                itemsByName.Clear();
                itemsById.Clear();
            }


            public override bool TryGetMember(
                GetMemberBinder binder, out object result)
            {
                var name = binder.Name.ToLower();

                //SimcraftImpl.Write(name);

                if (overrides.ContainsKey(name))
                {
                    result = overrides[name];
                    return true;
                }

                var val = ResolveName(name).Name;

                int n;
                bool isNumeric = int.TryParse(val, out n);

                //Logging.Write("binder: {0} name: {1} byId?: {2}", name, val, isNumeric);

                if (isNumeric)
                {
                    if (!itemsById.ContainsKey(n))
                        itemsById[n] = NewInternal(n);
                    return itemsById.TryGetValue(n, out result);
                }
                else
                {
                    if (!itemsByName.ContainsKey(val))
                        itemsByName[val] = NewInternal(val);
                    return itemsByName.TryGetValue(val, out result);
                }
            }

            public class SpellInternal : CacheInternal
            {

                public MagicValueType travel_time
                {
                    get { return new MagicValueType(1); }
                }

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
                    StackFrame frame = new StackFrame(4);
                    var method = frame.GetMethod();
                    var type = method.DeclaringType;
                    var __name = method.Name;

                    //SimcraftImpl.Write(name+"::"+__name);
                    _name = name;// dbc.ClassSpells[Tokenize(name)].Name;
                    _hasMe = SpellManager.HasSpell(_name);
                    //SimcraftImpl.Write((_hasMe ? "Found " : "Missing ") + name);
                }

                public MagicValueType execute_time
                {
                    get
                    {
                       
                        return new MagicValueType(Math.Max(Math.Max(gcd.nat, cast_time), channel_time));
                    }
                }

                public MagicValueType gcd
                {
                    get
                    {

                        if (DBHasSpell(_name)) return new MagicValueType(DBGetSpell(_name).Gcd);
                        return new MagicValueType(1.5);
                    }
                }

                public SpellInternal(int spellid)
                {
                    this.spellid = spellid;
                    _hasMe = SpellManager.HasSpell(spellid);
                }

                public MagicValueType range
                {
                    get
                    {

                        var spell = spellid == 0
                            ? GetSpell(_name)
                            : GetSpell(spellid);

                        return new MagicValueType(spell.HasRange ? spell.MaxRange : Me.CombatReach+5);
                    }
                }

                public MagicValueType charges
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);
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
                        return new MagicValueType(_charges);
                    }
                }

                public MagicValueType cast_time
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);
                        if (lastIteCasttime + iterationCache < iterationCounter || lastIteCasttime > iterationCounter)
                        {
                            _castime = spellid == 0
                                ? (double) GetSpell(_name).CastTime/1000
                                : (double) GetSpell(spellid).CastTime/1000;
                            lastIteCasttime = iterationCounter;
                        }
                        return new MagicValueType(_castime);
                    }
                }

                public MagicValueType in_flight
                {
                    get { return new MagicValueType(false); }
                }

                public MagicValueType channel_time
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);
                        if (lastIteChanneltime + iterationCache < iterationCounter ||
                            lastIteChanneltime > iterationCounter)
                        {
                            _channeltime = spellid == 0
                                ? (double) GetSpell(_name).BaseDuration/1000
                                : (double) GetSpell(spellid).BaseDuration/1000;
                            lastIteChanneltime = iterationCounter;
                        }
                        return new MagicValueType(_channeltime); ;
                    }
                }


                public MagicValueType duration
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);

                       return  new MagicValueType(spellid == 0
                                ? (double)GetSpell(_name).BaseDuration / 1000
                                : (double)GetSpell(spellid).BaseDuration / 1000); 
                            lastIteChanneltime = iterationCounter;

                    }                   
                }

                public MagicValueType recharge_time
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);
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
                        return new MagicValueType(_rechargeTime);
                    }
                }


                public MagicValueType charges_fractional
                {
                    get
                    {
                        if (!_hasMe) return new MagicValueType(0);
                        if (lastIteRecharge + iterationCache < iterationCounter || lastIteRecharge > iterationCounter)
                        {
                            _rechargeTime = spellid == 0
                                ? Lua.GetReturnVal<double>(
                                    "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                                    _name +
                                    "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return (cd*100/cooldownDuration)",
                                    0)+charges
                                : Lua.GetReturnVal<double>(
                                    "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                                    spellid +
                                    "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return (cd*100/cooldownDuration)",
                                    0)+charges;
                            lastIteRecharge = iterationCounter;
                        }
                        return  new MagicValueType(_rechargeTime);
                    }
                }

            }
        }

        public class ActiveDot : DynamicObject
        {
            private int lastIte = 0;
            private Dictionary<String, int> counts = new Dictionary<string, int>();


            //public int living_bomb = 0;

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {

                //SimcraftImpl.Write(binder.Name);
                //We need the true spellname to check for auras un special units
                var spellname = DBGetSpell(binder.Name).Name;
                if (SimcraftImpl.iterationCounter > lastIte) counts.Clear();

                if (!counts.ContainsKey(spellname))
                    counts[spellname] = SimcraftImpl.inst.actives.Count(ret => GetAuraUp(ret, spellname, true));

                //SimcraftImpl.Write(spellname+": "+counts[spellname]);

                result = counts[spellname];
                return true;
            }
        }

        public class TargetProxy : Proxy
        {
            protected bool Equals(TargetProxy other)
            {
                return Equals(health, other.health);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TargetProxy) obj);
            }

            public override int GetHashCode()
            {
                return (health != null ? health.GetHashCode() : 0);
            }

            public dynamic dot
            {
                get {return simc.dot; }
            }

            public dynamic debuff
            {
                get { return simc.dot; }
            }

            public static bool operator ==(WoWUnit h1, TargetProxy val)
            {
                return h1 == val.GetUnit();
            }
            public static bool operator !=(WoWUnit h1, TargetProxy val)
            {
                return h1 != val.GetUnit();
            }
            public static bool operator ==(TargetProxy val,WoWUnit h1)
            {
                return h1 == val.GetUnit();
            }
            public static bool operator !=(TargetProxy val,WoWUnit h1)
            {
                return h1 != val.GetUnit();
            }
            public override CacheInternal NewInternal(string name)
            {
                throw new NotImplementedException();
            }

            public override CacheInternal NewInternal(int name)
            {
                throw new NotImplementedException();
            }
            public override void InitResolvePriority()
            {
            }

            public MagicValueType distance
            {
                get { return new MagicValueType(GetUnit().Distance); }
            }

            public HealthProxy health;

            public TargetProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
                health = new HealthProxy(() => GetUnit(), simc);
            }

            public MagicValueType time_to_die
            {
                get { return health.TimeToDie; }
            }

            public MagicValueType melee_range
            {
                get { return new MagicValueType(GetUnit().IsWithinMeleeRange); }
            }

            public MagicValueType facing
            {
                get { return new MagicValueType(Me.IsFacing(GetUnit())); }
            }

            public WoWPoint Location
            {
                get { return GetUnit().Location; }
            }

 
        }

        public class DebuffProxy : Proxy
        {

            public class Casting : DebuffInternal
            {
                public override MagicValueType remains
                {
                    get
                    {
                        return _owner.GetUnit().IsCasting
                            ? new MagicValueType(_owner.GetUnit().CurrentCastTimeLeft.TotalSeconds)
                            : new MagicValueType(0);
                    }
                }
                public override MagicValueType up
                {
                    get { return new MagicValueType(_owner.GetUnit().IsCasting); }
                }
                public Casting(string name, DebuffProxy owner) : base(name, owner)
                {
                }
            }

            public Casting casting;

            public override CacheInternal NewInternal(string name)
            {
                return new DebuffInternal(name, this);
            }

            public override CacheInternal NewInternal(int name)
            {
                return new DebuffInternal(name, this);
            }

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.Spells);
                _resolvePriority.Add(dbc.ClassSpells);
                
            }
            public DebuffProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
                casting = new Casting("casting",this);
            }

            public DebuffInternal this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new DebuffInternal(name, this));
                    return (DebuffInternal)itemsByName[name];
                }
            }

            public DebuffInternal this[int id]
            {
                get
                {
                    if (!itemsById.ContainsKey(id)) itemsById.Add(id, new DebuffInternal(id, this));
                    return (DebuffInternal)itemsById[id];
                }
            }



            public class DebuffInternal : CacheInternal
            {
                private readonly string _name;
                protected DebuffProxy _owner;
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

                public MagicValueType down
                {
                    get { return !up; }
                }

                public virtual MagicValueType up
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
                        return new MagicValueType(cach.Up.Value);
                    }
                }


                private Regex tick = new Regex("every (\\d+) sec");

                private Decimal spt = 0;

                public MagicValueType tick_time
                {
                    get
                    {
                        if (!up) return new MagicValueType(0);
                        if (spt == 0)
                        {

                            WoWSpell sp = spellid == 0
                                    ? GetSpell(_name)
                                    : GetSpell(spellid);
                            var d = sp.AuraDescription;
                            if (tick.IsMatch(d))
                            {
                                spt = Convert.ToInt32(tick.Match(d).Groups[1].ToString());
                            }
                        }
                        return  new MagicValueType(spt);

                    }                 
                }

                public MagicValueType ticks_remain
                {
                    get
                    {
                        if (!up) return new MagicValueType(0);
                        if (spt == 0)
                        {

                            WoWSpell sp = spellid == 0
                                    ? GetSpell(_name)
                                    : GetSpell(spellid);
                            var d = sp.AuraDescription;
                            if (tick.IsMatch(d))
                            {
                                spt = Convert.ToInt32(tick.Match(d).Groups[1].ToString());
                            }
                        }
                        return new MagicValueType(Math.Floor(remains/spt));

                    }
                }

                public virtual MagicValueType remains
                {
                    get
                    {
                        var guid = _owner.GetUnit().Guid;
                        if (!_cache.ContainsKey(guid)) _cache[guid] = new ProxyCacheEntry();
                        dynamic cach = _cache[guid];
                        if (cach.remains.Ite + iterationCache < iterationCounter || cach.remains.Ite > iterationCounter)
                        {
                            cach.remains.Value = new MagicValueType(spellid == 0
                                ? GetAuraTimeLeft(_owner.GetUnit(), _name, true).TotalSeconds
                                : GetAuraTimeLeft(_owner.GetUnit(), spellid, true).TotalSeconds);
                            cach.remains.Ite = iterationCounter;
                        }
                        return cach.remains.Value;
                    }
                }

                public virtual MagicValueType stack
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



                public MagicValueType react
                {
                    get { return up; }
                }

                public MagicValueType ticking
                {
                    get { return up; }
                }
            }
        }
        public class PrevGcdProxy : DynamicObject
        {
            public int Id { get; set; }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var n = binder.Name;
                result = (Id == DBGetSpell(n).Id);
                return true;
            }
        }
        public class TalentProxy : Proxy
        {

            public override CacheInternal NewInternal(string name)
            {
                return new Talent(name);
            }

            public override CacheInternal NewInternal(int name)
            {
                throw new NotImplementedException();
            }

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.Spells);
            }

            //private readonly Dictionary<String, Talent> itemsByName = new Dictionary<string, Talent>();

            public TalentProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public Talent this[String name]
            {
                get
                {
                    if (!itemsByName.ContainsKey(name)) itemsByName.Add(name, new Talent(name));
                    return (Talent)itemsByName[name];
                }
            }

            public void Reset()
            {
                itemsByName.Clear();
            }

            public class Talent : CacheInternal
            {
                private readonly string _name;
                private bool? _enabled;

                public Talent(string name)
                {
                    _name = name;
                }

                public MagicValueType enabled
                {
                    get
                    {
                        if (_enabled == null) _enabled = StyxWoW.Me.GetLearnedTalents().Count(a => a.Name == _name) > 0;
                        return new MagicValueType(_enabled.Value);
                    }
                }
            }
        }

        public class RaidEventProxy
        {
            public class FakeEvent
            {
                public MagicValueType _in
                {
                    get { return new MagicValueType(Decimal.MaxValue); }
                }

                public MagicValueType distance
                {
                    get { return new MagicValueType(0); }
                }
                public MagicValueType exists
                {
                    get
                    {
                        return new MagicValueType(false);
                    }
                }
                public MagicValueType cooldown
                {
                    get { return new MagicValueType(Decimal.MaxValue); }
                }

                public MagicValueType remains
                {
                    get { return new MagicValueType(0); }
                }

                public MagicValueType count
                {
                    get { return new MagicValueType(0); }
                }
            }

            public FakeEvent adds
            {
                get
                {
                    return new FakeEvent();
                }
            }

            public FakeEvent movement
            {
                get
                {
                    return new FakeEvent();
                }
            }

        }

        public class GlyphProxy : Proxy
        {

            public override CacheInternal NewInternal(string name)
            {
                return new Glyph(name);
            }

            public override CacheInternal NewInternal(int name)
            {
                throw new NotImplementedException();
            }

            public override void InitResolvePriority()
            {
                _resolvePriority.Add(dbc.Glyphs);
                
            }

            //private readonly Dictionary<String, Talent> itemsByName = new Dictionary<string, Talent>();

            public GlyphProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var n = binder.Name;
                if (!n.Contains("glyph_of")) n = "glyph_of_" + n;
                result = new Glyph(dbc.Glyphs[n].Name);
                return true;
            }


            public void Reset()
            {
                itemsByName.Clear();
            }

            public class Glyph : CacheInternal
            {
                private readonly string _name;
                private bool? _enabled;

                public Glyph(string name)
                {
                    _name = name;
                }

                public MagicValueType enabled
                {
                    get
                    {
                        if (_enabled == null)
                        {
                            var _lua = @"for i = 1, NUM_GLYPH_SLOTS do
                             e, gt, gti, gsi, ic = GetGlyphSocketInfo(i);
                             if ( e ) then
                                n, r, icon, ct, mir, mar = GetSpellInfo(gsi);
                                if n == '"+_name+@"' then
                                    return true;
                                end
                             end
                            end
                            return false;";
                             _enabled = Lua.GetReturnVal<bool>(_lua, 0);
                        }
                        return new MagicValueType(_enabled.Value);
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

        public abstract class Proxy : DynamicObject
        {
            public delegate WoWUnit GetUnitDelegate();

            protected readonly Dictionary<int, object> itemsById = new Dictionary<int, object>();
            public readonly Dictionary<string, object> itemsByName = new Dictionary<string, object>();


            protected GetUnitDelegate GetUnit;
            protected int lastIte = 0;
            protected SimcraftImpl simc;

            public Proxy(GetUnitDelegate del, SimcraftImpl simc)
            {
                GetUnit = del;
                this.simc = simc;
                InitResolvePriority();
            }

            public abstract void InitResolvePriority();

            protected List<Dictionary<String, dbc.Spell>> _resolvePriority = new List<Dictionary<string, dbc.Spell>>();

            public dbc.Spell ResolveName(String name)
            {
                for (int i = 0; i < _resolvePriority.Count; i++)
                {
                    if (_resolvePriority[i].ContainsKey(name)) return _resolvePriority[i][name];
                }
                throw new MissingMemberException(this.GetType().ToString(), name);
            }


            protected Dictionary<String, CacheInternal> overrides = new Dictionary<string, CacheInternal>();

            public override bool TryGetMember(
                GetMemberBinder binder, out object result)
            {
                var name = binder.Name.ToLower();

                if (overrides.ContainsKey(name))
                {
                    result = overrides[name];
                    return true;
                }

                var val = ResolveName(name).Name;

                int n;
                bool isNumeric = int.TryParse(val, out n);

                //Logging.Write("binder: {0} name: {1} byId?: {2}", name, val, isNumeric);

                if (isNumeric)
                {
                    if (!itemsById.ContainsKey(n))
                        itemsById[n] = NewInternal(n);
                    return itemsById.TryGetValue(n, out result);
                }
                else
                {
                    if (!itemsByName.ContainsKey(val))
                        itemsByName[val] = NewInternal(val);
                    return itemsByName.TryGetValue(val, out result);
                }       
            }

            public CacheInternal TryGetMember(String name)
            {

                if (overrides.ContainsKey(name))
                {
                    return overrides[name];
                }

                var val = ResolveName(name).Name;

                int n;
                bool isNumeric = int.TryParse(val, out n);

                //Logging.Write("binder: {0} name: {1} byId?: {2}", name, val, isNumeric);

                if (isNumeric)
                {
                    if (!itemsById.ContainsKey(n))
                        itemsById[n] = NewInternal(n);
                    return (CacheInternal)itemsById[n];
                }
                else
                {
                    if (!itemsByName.ContainsKey(val))
                        itemsByName[val] = NewInternal(val);
                    return (CacheInternal)itemsByName[val];
                }
            }

            public abstract CacheInternal NewInternal(String name);
            public abstract CacheInternal NewInternal(int name);


        }

        public abstract class ResourceProxy : Proxy
        {
            public override CacheInternal NewInternal(int name)
            {
                throw new NotImplementedException();
            }

            public override CacheInternal NewInternal(string name)
            {
                throw new NotImplementedException();
            }

            private double _crt;
            private double _max;
            private double _pct;
            private int lastIteCrt = -1;
            private int lastIteMax = -1;
            private int lastItePct = -1;

            public ResourceProxy(GetUnitDelegate del, SimcraftImpl simc) : base(del, simc)
            {
            }

            public MagicValueType time_to_max
            {
                get { return deficit/regen; }
            }

            public override void InitResolvePriority()
            {
                //Resources dont need resolution
            }

            public MagicValueType pct
            {
                get
                {
                    if (lastItePct + iterationCache < iterationCounter || lastItePct > iterationCounter)
                    {
                        _pct = GetPercent;
                        lastItePct = iterationCounter;
                    }
                    return new MagicValueType(_pct);
                }
            }

            public MagicValueType percent
            {
                get { return pct; }
            }

            public MagicValueType current
            {
                get
                {
                    if (lastIteCrt + iterationCache < iterationCounter || lastIteCrt > iterationCounter)
                    {
                        _crt = GetCurrent;
                        lastIteCrt = iterationCounter;
                    }
                    return new MagicValueType(_crt);
                }
            }

            public MagicValueType max
            {
                get
                {
                    if (lastIteMax + iterationCache < iterationCounter || lastIteMax > iterationCounter)
                    {
                        _max = GetMax;
                        lastIteMax = iterationCounter;
                    }
                    return new MagicValueType(_max);
                }
            }

            public abstract MagicValueType GetPercent { get; }
            public abstract MagicValueType GetMax { get; }
            public abstract MagicValueType GetCurrent { get; }

            public MagicValueType deficit
            {
                get { return new MagicValueType(GetUnit() != null ? (int) (max - current) : 0); }
            }

            public MagicValueType regen { get; private set; }

            public MagicValueType cast_regen(double seconds)
            {
                return regen*seconds;
            }

            public static bool operator <(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.current < val;
            }

            public static bool operator >(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.current > val;
            }

            public static bool operator <=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.current <= val;
            }

            public static bool operator >=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null && h1.current >= val;
            }

            public static bool operator !=(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.current != val : false;
            }

            public static bool operator ==(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.current == val : false;
            }

            public static double operator -(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val - h1.current : 0;
            }

            public static double operator -(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? h1.current - val : 0;
            }

            public static double operator +(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val + h1.current : 0;
            }

            public static double operator +(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? val + h1.current : 0;
            }

            public static double operator +(ResourceProxy h1, ResourceProxy h2)
            {
                return h1.GetUnit() != null ? h2.current + h1.current : 0;
            }

            public static double operator -(ResourceProxy h1, ResourceProxy h2)
            {
                return h1.GetUnit() != null ? h1.current - h2.current : 0;
            }

            public static double operator *(ResourceProxy h1, ResourceProxy h2)
            {
                return h1.GetUnit() != null ? h2.current * h1.current : 0;
            }

            public static double operator /(ResourceProxy h1, ResourceProxy h2)
            {
                return h1.GetUnit() != null ? h1.current / h2.current : 0;
            }

            public static double operator *(double val, ResourceProxy h1)
            {
                return h1.GetUnit() != null ? val*h1.current : 0;
            }

            public static double operator *(ResourceProxy h1, double val)
            {
                return h1.GetUnit() != null ? val*h1.current : 0;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ResourceProxy)) return false;
                return ((ResourceProxy) obj).current == current;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public void Pulse()
            {
                regen = new MagicValueType(Lua.GetReturnVal<double>(
                    "inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen;", 0));
            }
        }

        public class HealthProxy : ResourceProxy
        {
            public static Decimal AVG_DPS = 22000;

            public HealthProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(GetUnit().HealthPercent); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int) (GetUnit().CurrentHealth) : 0); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxHealth) : 0); }
            }

            public MagicValueType TimeToDie
            {
                get
                {
                    var g = (int) StyxWoW.Me.GroupInfo.GroupSize;
                 
                    if (g > 0)
                        return new MagicValueType(GetCurrent/(((Decimal)StyxWoW.Me.GroupInfo.GroupSize)*(Decimal)0.6*AVG_DPS));

                    return new MagicValueType(GetCurrent/(AVG_DPS));
                }
            }
        }

        public class ManaProxy : ResourceProxy
        {
            public static int AVG_DPS = 22000;

            public ManaProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(GetUnit().ManaPercent); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().CurrentMana) : 0); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxMana) : 0); }
            }

        }

        public class ComboPointProxy : ResourceProxy
        {
            public ComboPointProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? StyxWoW.Me.ComboPoints : 0); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(5); }
            }
        }

        public class HolyPowerProxy : ResourceProxy
        {
            public HolyPowerProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(GetUnit() != null ? StyxWoW.Me.HolyPowerPercent : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)StyxWoW.Me.CurrentHolyPower : 0); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType((int)Me.MaxHolyPower); }
            }
        }

        public class RuneProxy : ResourceProxy
        {
            public RuneProxy(GetUnitDelegate del, SimcraftImpl simc, RuneType type)
                : base(del, simc)
            {
                this.type = type;
            }

            private RuneType type;

            public double frac
            {
                get
                {
                    if (type == RuneType.Blood)
                    {
                        var o1 = get_rune_frac(1);                        
                        var o2 = get_rune_frac(2);
                        var m = Math.Max(o1, o2);
                        var t = 200 - m;
                        return t/100;
                    }
                    if (type == RuneType.Frost)
                    {
                        var o1 = get_rune_frac(5);
                        var o2 = get_rune_frac(6);
                        var m = Math.Max(o1, o2);
                        var t = 200 - m;
                        return t / 100;
                    }
                    if (type == RuneType.Unholy)
                    {
                        var o1 = get_rune_frac(3);
                        var o2 = get_rune_frac(4);
                        var m = Math.Max(o1, o2);
                        var t = 200 - m;
                        return t / 100;
                    }
                    return 0;
                }
            }

            private double get_rune_frac(int id)
            {

                //"start, duration, runeReady = GetRuneCooldown("+id+"); if (runeReady) then return 0 else local t= ((duration-(GetTime()-start))*100/duration); if (t > 100) then t = t; end return t end"
                return
                    Lua.GetReturnVal<double>(
                        "start, duration, runeReady = GetRuneCooldown(" + id + "); if (runeReady) then return 0 else local t= ((duration-(GetTime()-start))*100/duration); if (t > 100) then t = t; end return t end",
                        0);
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(frac); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(Me.GetRuneCount(type)); } 
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(2); }
            }

            public static bool operator !(RuneProxy op1)
            {
                return op1 == 0;
            }

            /*public static bool operator false(RuneProxy x)
            {
                return x.frac < 1;
            }

            public static bool operator true(RuneProxy x)
            {
                return x.frac >= 1;
            }*/

        }

        public class ChiProxy : ResourceProxy
        {
            public ChiProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType((int)GetUnit().MaxChi); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType((int)GetUnit().CurrentChi); }
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(0); }
            }
        }

        public class EnergyProxy : ResourceProxy
        {
            public EnergyProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(StyxWoW.Me.EnergyPercent); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxEnergy) : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().CurrentEnergy) : 0); }
            }
        }

        public class EclipseProxy : ResourceProxy
        {
            public EclipseProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(StyxWoW.Me.EclipsePercent); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxEclipse) : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().CurrentEclipse) : 0); }
            }
        }


        public class RunicPowerProxy : ResourceProxy
        {
            public RunicPowerProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(StyxWoW.Me.EnergyPercent); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxRunicPower) : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().CurrentRunicPower) : 0); }
            }
        }


        public class FocusProxy : ResourceProxy
        {
            public FocusProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(StyxWoW.Me.FocusPercent); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxFocus) : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().CurrentFocus) : 0); }
            }

            //public bool cast_regen

        }

        public class RageProxy : ResourceProxy
        {
            public RageProxy(GetUnitDelegate del, SimcraftImpl simc)
                : base(del, simc)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return new MagicValueType(StyxWoW.Me.RagePercent); }
            }

            public override MagicValueType GetMax
            {
                get { return new MagicValueType(GetUnit() != null ? (int)(GetUnit().MaxRage) : 0); }
            }

            public override MagicValueType GetCurrent
            {
                get { return new MagicValueType(GetUnit() != null ? (int) (GetUnit().CurrentRage) : 0); }
            }
        }
    }
}