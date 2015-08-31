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
using Simcraft.APL;
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

            private Dictionary<string, int> wornPieces = new Dictionary<string, int>();

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = false;
                try
                {

                    //var p = double.PositiveInfinity;
                    var n = binder.Name;
                    var splits = n.Split('_');
                    var set = dbc.Sets[splits[0]];

                    if (!wornPieces.ContainsKey(splits[0]))
                    {
                        int count = 0;
                        foreach (var it in Me.Inventory.Equipped.Items)
                        {
                            if (it == null) continue;

                            var i = (uint) it.ItemInfo.Id;

                            if (set.Keys.Contains(i)) count++;
                        }

                        wornPieces[splits[0]] = count;
                    }


                    result = new MagicValueType(Convert.ToInt32(splits[1][0]) <= wornPieces[splits[0]]);
                }
                catch (Exception)
                {
                    result = new MagicValueType(false);
                    return true;
                }
                return true;
            }
        }

        public class DiseaseProxy
        {

            public MagicValueType max_ticking
            {
                get
                {
                    bool default_value = true;
                    bool ret = default_value;

                    var val =  simc.debuff.blood_plague.ticking;
                    if (val && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.frost_fever.ticking;
                    if (val && !!simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.necrotic_plague.ticking;
                    if (val && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = false;

                    return new MagicValueType(ret);
                }
            }

            public MagicValueType min_ticking
            {
                get
                {
                    bool default_value = true;
                    bool ret = default_value;
                    var val = simc.debuff.blood_plague.ticking;
                    if (!val && !simc.talent.necrotic_plague.enabled)
                        return val;
                    val = simc.debuff.frost_fever.ticking;
                    if (!val && !simc.talent.necrotic_plague.enabled)
                        return val;
                    val = simc.debuff.necrotic_plague.ticking;
                    if (!val && simc.talent.necrotic_plague.enabled)
                        return val;

                    if (ret == default_value)
                        ret = false;

                    return new MagicValueType(ret);
                }
            }

            public MagicValueType min_remains
            {
                get
                {
                    var default_value = Decimal.MaxValue;
                    var ret = default_value;
           
                    var val = simc.debuff.blood_plague.remains;
                    if (val < ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.frost_fever.remains;
                    if (val < ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.necrotic_plague.remains;
                    if (val < ret && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = 0;

                    return new MagicValueType(ret);
                }
            }

            public MagicValueType max_remains
            {
                get
                {
                    var default_value = Decimal.MinValue;
                    var ret = default_value;

                    var val = simc.debuff.blood_plague.remains;
                    if (val > ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.frost_fever.remains;
                    if (val > ret && !simc.talent.necrotic_plague.enabled)
                        ret = val;
                    val = simc.debuff.necrotic_plague.remains;
                    if (val > ret && simc.talent.necrotic_plague.enabled)
                        ret = val;

                    if (ret == default_value)
                        ret = 0;

                    return new MagicValueType(ret);
                }
            }

            public MagicValueType ticking
            {
                get
                {
                    var val = simc.debuff.blood_plague.ticking;
                    if (val)
                        return val;
                    val = simc.debuff.frost_fever.ticking;
                    if (val)
                        return val;
                    val = simc.debuff.necrotic_plague.ticking;
                    if (val)
                        return val;

                    return new MagicValueType(false);
                }           
            }

            private SimcraftImpl simc;

            public DiseaseProxy(SimcraftImpl simc)
            {
                this.simc = simc;
            }

        }


        public class StatProxy : DynamicObject
        {
            ProxyCacheEntry cache = new ProxyCacheEntry();

            public StatProxy()
            {
                cache["crit"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.CritMelee));
                cache["haste"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.HasteMelee));
                cache["mastery"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.Mastery));
                cache["bonusarmor"].SetRetrievalDelegate(
                    () => Me.GetCombatRating(WoWPlayerCombatRating.ArmorPenetration));
                cache["multistrike"].SetRetrievalDelegate(
                    () => LuaGet<int>("return GetCombatRating(CR_MULTISTRIKE);", 0));
                cache["spellpower"].SetRetrievalDelegate(
                    () => LuaGet<int>("spellDmg = GetSpellBonusDamage(3); return spellDmg;", 0));
                cache["dodge"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.Dodge));
                cache["parry"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.Parry));
                cache["block"].SetRetrievalDelegate(() => Me.GetCombatRating(WoWPlayerCombatRating.Block));
                cache["spell_haste"].SetRetrievalDelegate(
                    () => 1/((100+LuaGet<double>("return UnitSpellHaste(\"player\")", 0))/100));
                cache["multistrike_pct"].SetRetrievalDelegate(
                    () => LuaGet<double>("return GetMultistrike()/100", 0));
                cache["mastery_value"].SetRetrievalDelegate(
                    () => LuaGet<double>("mastery, coefficient = GetMasteryEffect(); return mastery/100", 0));
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = cache[binder.Name].GetValue();
                return true;
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
                        //LuaDoString("_G[\"apl\"] = \"" + current_apl + "\";");

                        if (name.Equals(oocapl)) ooc = DateTime.Now;
                        return RunStatus.Failure;
                    }));

                    foot.AddChild(new Action(delegate
                    {
                        //LuaDoString("_G[\"apl\"] = \"" + oldapl + "\";");
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


        public class SpellbookProxy
        {

            public List<int> spells = new List<int>();
            public List<int> petspells = new List<int>(); 

            public void Reset()
            {
                spells.Clear();
                petspells.Clear();
            }


            public void Fill()
            {
                /*int specTabOffset  = Lua.GetReturnVal<int>("tabName, tabTexture, tabOffset, numEntries = GetSpellTabInfo(2); return tabOffset", 0);
                int specTabEntries = Lua.GetReturnVal<int>("tabName, tabTexture, tabOffset, numEntries = GetSpellTabInfo(2); return numEntries", 0);

                for (int i = specTabOffset; i < specTabOffset + specTabEntries; i++)
                {
                    SpellFindResults res;
                    
                    var sid = Lua.GetReturnVal<String>("spellName, spellSubName = GetSpellBookItemName("+i+",\"player\"); return spellName", 0);

                    Write("Looking for "+sid);

                    var s = GetSpellByName(sid);

                    Write("Found " + s);

                    //L

                    if (s == default(WoWSpell))
                        continue;
                 
                    spells.Add(s.Id);*/
                    //if (dbc.Spells.ContainsKey((uint)sid))
                    //    Write("Found "+dbc.Spells[(uint)sid].token+" in Spellbook !");
                //}
            }
        }
   

        public class BuffProxy : SpellBasedProxy
        {


            public BuffInternal raging_blow;// = new BuffInternal(dbc.Spells[131116]);

            public DebuffProxy.DebuffInternal colossus_smash;
            public DebuffProxy.DebuffInternal colossus_smash_up;

            public static int cShots;

            public AuraProxy Source;

            public BuffProxy(GetUnitDelegate del, AuraProxy ar)
                : base(del)
            {
                Source = ar;
                pre_steady_focus = new PreSteadyFocus(this);
                bloodlust = new Bloodlust(this);
                potion = new Potion(this);
                anytrinket = new AnyTrinket(this);
                incanters_flow = new IncantersFlow(this);
                raging_blow = new BuffInternal(dbc.Spells[131116], this, "raging_blow_special");
                colossus_smash = new DebuffProxy.DebuffInternal(dbc.Spells[SimcNames.debuffs["colossus_smash"].First().V2], this, "colossus_smash");
                colossus_smash_up = new DebuffProxy.DebuffInternal(dbc.Spells[SimcNames.debuffs["colossus_smash"].First().V2], this, "colossus_smash_up");
            }

            public IncantersFlow incanters_flow;// = new IncantersFlow("incanters_flow");
            public Bloodlust bloodlust;
            public Potion potion;
            public AnyTrinket anytrinket;
            public PreSteadyFocus pre_steady_focus;


            public class IncantersFlow : BuffInternal
            {
                private int oldstack = 0;
                public int dir = 1;
                public IncantersFlow(BuffProxy owner)
                    : base(dbc.Spells[116267], owner, "incanters_flow_special")
                {
                    Properties["stack"] = () =>
                    {
                        //Write(""+oldstack);
                        var bstack = (int)owner.Source.GetAuraStacks(116267);
                        if (oldstack < bstack) dir = 1;
                        else if (oldstack > bstack) dir = -1;
                        oldstack = bstack;
                        return bstack;
                    };
                }

               

            }

            public class PreSteadyFocus : BuffInternal
            {
                public PreSteadyFocus(BuffProxy owner)
                    : base(null, owner,"pre_steady_focus")
                {
                    Properties["up"] = () =>
                        (cShots == 1);
                }
            }


            public class Potion : BuffInternal
            {

                private spell_data_t[] potions;

                public Potion(BuffProxy owner)
                    : base(null, owner,"potion_buff")
                {
                   
                    potions = new[] { dbc.Spells[156423], dbc.Spells[156426], dbc.Spells[156428], dbc.Spells[156430], };
                    Properties["up"] = () => GetAuraUp(Owner.GetUnit(), potions);
                    Properties["remains"] =
                        () => GetAuraTimeLeft(Owner.GetUnit(), potions).TotalSeconds;
                    Properties["duration"] = () => 25;
                }

            }

            public class AnyTrinket : BuffInternal
            {


                public AnyTrinket(BuffProxy owner)
                    : base(null, owner,"anytrinket_buff")
                {
                    Properties["up"] = () =>
                    {
                        foreach (var proc in get_procs)
                        {
                            if (simc.buff[DBGetSpell(proc).name].up) return true;
                        }
                        return false;
                    };

                    Properties["remains"] = () =>
                    {
                        Decimal dur = Decimal.MinValue;
                        foreach (var proc in get_procs)
                        {
                            var r = simc.buff[proc].remains;
                            if (dur < r) dur = r;
                        }
                        return dur;
                    };

                    Properties["stack"] = () =>
                    {
                        Decimal dur = Decimal.MinValue;
                        foreach (var proc in get_procs)
                        {
                            var r = simc.buff[proc].stack;
                            if (dur < r) dur = r;
                        }
                        return dur;
                    };

                }

                public List<String> get_procs
                {
                    get
                    {
                        List<String> procs = new List<string>();

                        var t1 = Me.Inventory.GetItemBySlot((uint)WoWEquipSlot.Trinket1);
                        var t2 = Me.Inventory.GetItemBySlot((uint)WoWEquipSlot.Trinket2);

                        if (dbc.ItemProcs.ContainsKey((uint)t2.ItemInfo.Id))
                        {
                            procs.Add(dbc.ItemProcs[(uint)t2.ItemInfo.Id]);
                        }
                        if (dbc.ItemProcs.ContainsKey((uint)t1.ItemInfo.Id))
                        {
                            procs.Add(dbc.ItemProcs[(uint)t1.ItemInfo.Id]);
                        }
                        return procs;
                    }
                } 


            }



            public class Bloodlust : BuffInternal
            {

                private spell_data_t bloodlust;

                public Bloodlust(BuffProxy owner)
                    : base(null,owner,"bloodlust_proxy")
                {

                    bloodlust = dbc.Spells[2825];

                    Properties["up"] =
                        () =>
                            simc.buff.heroism.up || simc.buff[bloodlust].up || simc.buff.time_warp.up;
                    Properties["remains"] = () => simc.buff.heroism.up
                        ? simc.buff.heroism.remains
                        : simc.buff[bloodlust].up
                            ? simc.buff[bloodlust].remains
                            : simc.buff.time_warp.up ? simc.buff.time_warp.remains : 0.0;
                    Properties["duration"] = () => 40;
                    Properties["stack"] =
                                            () =>
                                                0;

                }

            }


            public bool Pre_Steady_Focus
            {
                get { return cShots == 1; }
            }


            public class BuffInternal : AuraInternal
            {
                public BuffInternal(spell_data_t spell, BuffProxy owner, String sn)
                    : base(spell, owner, sn)
                {

                    Properties["Spell"] = () => 1;


                    Properties["duration"] = () => this["up"]
                        ? owner.Source.GetAuraDuration(Spell.id) / 1000
                        : (Decimal)Spell.duration / 1000;
                    Properties["up"] = () => owner.Source.GetAuraUp(Spell.id);
                    Properties["remains"] = () => owner.Source.GetAuraTimeLeft(Spell.id).TotalSeconds;
                    Properties["stack"] = () =>
                    {
                        //Write("AI: " + safeName);
                        return owner.Source.GetAuraStacks(Spell.id);
                    };
                    Properties["down"] = () => !this["up"];
                    Properties["react"] = () => this["stack"] > 0 ? this["stack"] : this["up"];
                    Properties["ticking"] = () => this["up"];
                    Properties["tick_time"] = () => 1;
                    Properties["ticks_remain"] = () => 1;

                }

                protected override void GetTickingEffect()
                {
                    if (Spell == null) return;
                    foreach (var eff in Spell.effects)
                    {
                        if (dbc.Effects[eff].sub_type.Contains("A_PERIODIC_DAMAGE"))
                        {
                            tickingEffect = dbc.Effects[eff];
                        }
                        if (dbc.Effects[eff].sub_type.Contains("A_PERIODIC") && !dbc.Effects[eff].sub_type.Contains("DAMAGE"))
                        {
                            tickingEffect = dbc.Effects[eff];
                            return;
                        }
                    }
                }
            }

            public override Internal NewInternal(string token)
            {

                var b = FindBuff(token);
                //if (b == null) Write("Broken: "+token);
                return new BuffInternal(b,this,token);
            }
        }

        public class ObliterateProxy
        {
            ProxyCacheEntry Cache = new ProxyCacheEntry();

            public ObliterateProxy() 
            {
                Cache["ready_in"].SetRetrievalDelegate(() => LuaGet<double>(
                            "start, duration, enabled = GetSpellCooldown('Obliterate'); t =GetTime()-start; if t < 1 then return 1-t else return 10-t end",
                            0));
            }

            public MagicValueType ready_in
            {
                get { return Cache["ready_in"].GetValue(); }
            }
        }


        public class ItemProxy
        {

            public ItemProxy cooldown
            {
                get { return this; }
            }

            private WoWEquipSlot slot;
            ProxyCacheEntry Cache = new ProxyCacheEntry();

            public ItemProxy(WoWEquipSlot slot)
            {
                this.slot = slot;
                Cache["remains"].SetRetrievalDelegate(() => Me.Inventory.GetItemBySlot((uint)slot).CooldownTimeLeft.TotalSeconds);
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
                get { return Cache["remains"].GetValue(); }
            }

            public virtual MagicValueType React
            {
                get { return up; }
            }
        }



        public class CooldownProxy : SpellBasedProxy
        {

            public CooldownProxy(GetUnitDelegate del)
                : base(del)
            {
                potion = new Potion(this);
                
            }

            public Potion potion;

            public class Potion : CooldownInternal
            {

                public Potion(CooldownProxy owner) : base(dbc.Spells.First().Value, owner,"potion")
                {
                    Properties["remains"] = () => simc.PotionCooldown.nat;
                }

                public override MagicValueType up
                {
                    get { return !(remains > 0); }
                }

                public override MagicValueType remains
                {
                    get { return this["remains"]; }
                }

            }


            public class CooldownInternal : SpellCacheInternal
            {

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

                public virtual MagicValueType duration
                {
                    get
                    {
                        return this["duration"];
                    }
                }


                public virtual MagicValueType remains
                {
                    get { return this["remains"]; }
                }

                public virtual MagicValueType react
                {
                    get { return up; }
                }

                public CooldownInternal(spell_data_t spell, SpellBasedProxy owner, String safename) : base(spell, owner, "cooldown::"+safename)
                {
                    Properties.Add("remains", () => GetSpellCooldown(Spell).TotalSeconds);
                    Properties.Add("duration", () => spell.cooldown);
                }
            }

            public override Internal NewInternal(string token)
            {
               return new CooldownInternal(simc.LearnedSpellFromToken(token), this,token);
            }
        }




        public class PetProxy : DynamicObject
        {
            public CooldownProxy cooldown = new CooldownProxy(() => Me.Pet);
            public PetBuffProxy buff = new PetBuffProxy();
            public HealthProxy health = new HealthProxy(() => Me.Pet);
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
                                return new MagicValueType(true);
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
                        return MagicValueType.Zero;
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
                    return MagicValueType.Zero;
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


            public class PetBuffProxy : BuffProxy
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
                            return MagicValueType.Zero;
                        }

                    }

                }


                public PetBuffProxy() : base(() => Me.Pet, inst.PetAuras)
                {
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

        public class SpellProxy : SpellBasedProxy
        {


            public SpellProxy(GetUnitDelegate del)
                : base(del)
            {
            }


            public override Internal NewInternal(string token)
            {
                return new SpellInternal(simc.LearnedSpellFromToken(token), this,token);
            }


            public class SpellInternal : SpellCacheInternal
            {

                private readonly bool _hasMe;


                public SpellInternal(spell_data_t spell, SpellBasedProxy owner, String safename)
                    : base(spell, owner,"spell::"+safename)
                {
                    
                  
                    _hasMe = SpellManager.HasSpell((int) spell.id);

                    AddProperty("gcd", () =>
                    {
                        var sgcd = ((Decimal)Spell.gcd)/1000;
                        sgcd = sgcd / ((100 + simc.spell_haste) / 100);
                        return Math.Max(sgcd,1);
                    });

                    AddProperty("execute_time", () =>
                    {
                        if (this["cast_time"] > this["gcd"]) return this["cast_time"];
                        return this["gcd"];
                       
                    });

                    AddProperty("range", () =>
                    {
                        if (!_hasMe) return Spell.max_range;
                        var _spell = GetSpell(Spell);
                        return new MagicValueType(_spell.HasRange ? _spell.MaxRange : Me.CombatReach + 5);
                    });

                    AddProperty("charges", () => !_hasMe
                        ? 0
                        : LuaGet<int>(
                            "currentCharges,_,_,_,_ = GetSpellCharges(" + Spell.id + "); return currentCharges", 0));
                                     
                    AddProperty("cast_time", () =>
                    {
                        if (Spell.IsChanneled()) return this["channel_time"];
                        if (!_hasMe)
                            return Math.Max(((Decimal)Spell.cast_max / 1000), this["gcd"]);
                        return
                            Math.Max(((Decimal)GetSpell(Spell).CastTime / 1000), this["gcd"]);
                    });
           
                    AddProperty("channel_time", () =>
                    {
                        if (!Spell.IsChanneled()) return MagicValueType.Zero;
                        if (!_hasMe)
                            return Math.Max((Decimal)Spell.duration / 1000, this["gcd"]);
                        return Math.Max((Decimal)GetSpell(Spell).MaxDuration / 1000, this["gcd"]);
                    });

                    AddProperty("duration", () => !_hasMe ? Spell.duration/1000 : GetSpell(Spell).BaseDuration/1000);

                    AddProperty("in_flight", () => false);

                    AddProperty("recharge_time", () => !_hasMe
                        ? 0
                        : LuaGet<double>(
                            "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                            Spell.id +
                            "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return cd",
                            0));
                    AddProperty("charges_fractional", () => !_hasMe
                        ? 0
                        : LuaGet<double>(
                            "currentCharges, maxCharges, cooldownStart, cooldownDuration = GetSpellCharges(\"" +
                            Spell.id +
                            "\"); cd = (cooldownDuration-(GetTime()-cooldownStart)); if (cd > cooldownDuration or cd < 0) then cd = 0; end return (cd*100/cooldownDuration)",
                            0) + this["charges"]);

                    Write("Created Spell: " + safename + " ex:" + this["execute_time"] + " r:" + this["range"] + " c:" + this["charges"] + " clt:" + this["channel_time"] + " dur:" + this["duration"] + " ct:" + this["cast_time"]+" id: "+Spell.id);


                }

            }
        }

        public class ActiveDot : DynamicObject
        {
            private int lastIte = 0;
            private Dictionary<spell_data_t, int> counts = new Dictionary<spell_data_t, int>();

            public ActiveDot()
            {

            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {

                var spell = DBGetSpell(binder.Name);
                if (SimcraftImpl.iterationCounter > lastIte) counts.Clear();

                if (!counts.ContainsKey(spell))
                    counts[spell] = SimcraftImpl.inst.actives.Count(ret => GetAuraUp(ret, spell, true));


                result = counts[spell];
                return true;
            }
        }

        public class TargetProxy : UnitBasedProxy
        {
            private CacheInternal Cache;// = new CacheInternal(GetUnit);

            public dynamic dot
            {
                get {return simc.dot; }
            }

            public dynamic debuff
            {
                get { return simc.dot; }
            }

            public MagicValueType distance
            {
                get { return Cache["distance"]; }
            }

            public HealthProxy health;

            public TargetProxy(GetUnitDelegate del)
                : base(del)
            {
                health = new HealthProxy(() => GetUnit());
                Cache = new CacheInternal(GetUnit,"targetproxy");

                //Cache.AddProperty("time_to_die", () => health.TimeToDie);
                Cache.AddProperty("melee_range", () => GetUnit().IsWithinMeleeRange);
                Cache.AddProperty("facing", () => Me.IsFacing(GetUnit()));
                Cache.AddProperty("distance", () => GetUnit().Distance);
            }

            public MagicValueType time_to_die
            {
                get { return health.TimeToDie; }
            }

            public MagicValueType melee_range
            {
                get { return Cache["melee_range"]; }
            }

            public MagicValueType facing
            {
                get { return Cache["facing"]; }
            }

            public WoWPoint Location
            {
                get { return GetUnit().Location; }
            }

 
        }

        public class DebuffProxy : SpellBasedProxy
        {



            public class Casting : DebuffInternal
            {
                public Casting(DebuffProxy owner) : base(null, owner,"casting_debuff")
                {
                    
                    Properties["remains"] = () => Owner.GetUnit().IsCasting
                        ? Owner.GetUnit().CurrentCastTimeLeft.TotalSeconds
                        : 0;
                    Properties["up"] = () => Owner.GetUnit().IsCasting;
                    Properties["react"] = () => Owner.GetUnit().IsCasting;
                }
            }


            public Casting casting;

            public override Internal NewInternal(string name)
            {
                /*DEADBEEF*/
                return new DebuffInternal(FindDebuff(name), this,name);
            }


            public DebuffProxy(GetUnitDelegate del)
                : base(del)
            {
                casting = new Casting(this);
            }

            public class DebuffInternal : AuraInternal
            {
                public DebuffInternal(spell_data_t spell, SpellBasedProxy owner, String sn) : base(spell, owner, sn)
                {
                    Properties["up"] = () => GetAuraUp(Owner.GetUnit(), Spell, true);
                    Properties["remains"] = () => GetAuraTimeLeft(Owner.GetUnit(), Spell, true).TotalSeconds;
                }

                protected override void GetTickingEffect()
                {
                    if (Spell == null) return;
                    foreach (var eff in Spell.effects)
                    {
                        if (dbc.Effects[eff].sub_type.Contains("A_PERIODIC_DAMAGE"))
                        {
                            tickingEffect = dbc.Effects[eff];
                            break;
                        }
                        if (dbc.Effects[eff].sub_type.Contains("A_PERIODIC"))
                        {
                            tickingEffect = dbc.Effects[eff];
                        }
                    }
                }
            }
 
        }

        public abstract class AuraInternal : SpellCacheInternal
        {
            protected Effect tickingEffect = null;

            protected abstract void GetTickingEffect();

            protected String safename;

            public AuraInternal(spell_data_t spell, SpellBasedProxy owner, String safeName)
                : base(spell, owner, owner.GetType().ToString().Replace("Simcraft.SimcraftImpl+","")+"::"+safeName)
            {
                safename = safeName;
                if (spell == null) Write("Aura "+safename+" initialized with null");

                GetTickingEffect();

                Properties["Spell"] = () => 1;

                
                Properties["duration"] = () => this["up"]
                    ? (Decimal)GetAura(Owner.GetUnit(), Spell, true).Duration / 1000
                    : (Decimal)Spell.duration / 1000;
                Properties["up"] = () => GetAuraUp(Owner.GetUnit(), Spell);
                Properties["remains"] = () => GetAuraTimeLeft(Owner.GetUnit(), Spell).TotalSeconds;
                Properties["stack"] = () =>
                {
                    //Write("AI: " + safeName);
                    return GetAuraStacks(Owner.GetUnit(), Spell, true);
                };
                Properties["down"] = () => !this["up"];
                Properties["react"] = () => this["stack"] > 0 ? this["stack"] : this["up"];
                Properties["ticking"] = () => this["up"];
                Properties["tick_time"] = () => 1;
                Properties["ticks_remain"] = () => 1;
                
                
            }

            public virtual MagicValueType ticks_remain
            {
                get
                {
                    if (tickingEffect == null) return MagicValueType.Zero;
                    if (!this["up"]) return MagicValueType.Zero;

                    return new MagicValueType(Math.Floor((Decimal)(this["remains"] / (tickingEffect.amplitude / 1000))));
                }
            }

            public virtual MagicValueType tick_time
            {
                get
                {
                    if (tickingEffect != null)
                        return new MagicValueType(tickingEffect.amplitude);
                    return MagicValueType.Zero;
                }
            }

        }

        public class PrevGcdProxy : DynamicObject
        {
            public spell_data_t  spell { get; set; }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var n = binder.Name;
                result = spell == DBGetSpell(n);
                return true;
            }
        }
        public class TalentProxy : SpellBasedProxy
        {


            public override Internal NewInternal(string name)
            {
                return new Talent(DBGetClassSpell(name));
            }


            public TalentProxy(GetUnitDelegate del)
                : base(del)
            {
            }

            public class Talent : SpellInternal
            {
                private bool? _enabled;

                public Talent(spell_data_t name) : base(name, null)
                {
                    Spell = name;
                }

                public MagicValueType enabled
                {
                    get
                    {
                        if (_enabled == null) _enabled = StyxWoW.Me.GetLearnedTalents().Count(a => a.Name == Spell.name) > 0;
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
                    get { return MagicValueType.Zero; }
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
                    get { return MagicValueType.Zero; }
                }

                public MagicValueType count
                {
                    get { return MagicValueType.Zero; }
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

        public class GlyphProxy : SpellBasedProxy
        {


            public override Internal NewInternal(string name)
            {
               var g = new Glyph(DBGetSpell(name));
               return g;
            }

            public GlyphProxy(GetUnitDelegate del)
                : base(del)
            {
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var n = binder.Name;

                if (!n.Contains("glyph_of")) n = "glyph_of_" + n;

                if (!Spells.ContainsKey(n))
                {
                    Spells[n] = NewInternal(n);
                }

                result = Spells[n];
                return true;
            }


            public class Glyph : SpellInternal
            {

                public Glyph(spell_data_t t) : base(t,null)
                {
                    Spell = t;
                }

                private bool? _enabled;

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
                                if n == '"+Spell.name+@"' then
                                    return true;
                                end
                             end
                            end
                            return false;";
                             _enabled = LuaGet<bool>(_lua, 0);
                        }
                        return new MagicValueType(_enabled.Value);
                    }
                }

            }
        }

        public abstract class SpellCacheInternal : CacheInternal
        {
            public spell_data_t Spell { get; set; }
            public SpellBasedProxy Owner { get; set; }

            protected WoWGuid Guid
            {
                get { return Owner.GetUnit().Guid; }
            }

            public SpellCacheInternal(spell_data_t spell, SpellBasedProxy owner, String bossname)
                : base(owner.GetUnit, bossname)
            {
                Spell = spell;
                //Write(""+Spell);
                Owner = owner;
            }
        }

        public abstract class SpellInternal : Internal
        {
            public spell_data_t Spell { get; set; }
            public SpellBasedProxy Owner { get; set; }

            public SpellInternal(spell_data_t spell, SpellBasedProxy owner)
            {
                Spell = spell;
                Owner = owner;
            }
        }

        public abstract class SpellBasedProxy : UnitBasedProxy
        {
            public Dictionary<String, Internal> Spells = new Dictionary<string, Internal>();


            public DynamicItemCollection<CacheInternal> AsList
            {
                get
                {
                    var l = new DynamicItemCollection<CacheInternal>();
                    foreach (var s in Spells.Values)
                    {
                        l.Add((CacheInternal)s);
                    }
                    return l;
                }
            }


            public Internal this[spell_data_t spell]
            {
                get
                {
                    if (!Spells.ContainsKey(spell.token))
                    {
                        
                        Spells[spell.token] = NewInternal(spell.token);
                    }
                    return Spells[spell.token];
                }
            }

            protected SpellBasedProxy(GetUnitDelegate del) : base(del)
            {

            }

            public abstract Internal NewInternal(String token);

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var token = Tokenize(binder.Name);
                if (!Spells.ContainsKey(token)) Spells.Add(token, NewInternal(token));

                result = Spells[token];

                return true;
            }

            public void Reset()
            {
                Spells.Clear();
            }

        }

        public abstract class Proxy : DynamicObject
        {
            protected SimcraftImpl simc
            {
                get { return SimcraftImpl.inst; }
            }
        }

        public delegate WoWUnit GetUnitDelegate();

        public abstract class UnitBasedProxy : Proxy
        {
            protected bool Equals(UnitBasedProxy other)
            {
                return Equals(GetUnit, other.GetUnit);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UnitBasedProxy) obj);
            }

            public override int GetHashCode()
            {
                return (GetUnit != null ? GetUnit.GetHashCode() : 0);
            }

            public GetUnitDelegate GetUnit;

            public static bool operator ==(WoWUnit h1, UnitBasedProxy val)
            {
                return h1 == val.GetUnit();
            }
            public static bool operator !=(WoWUnit h1, UnitBasedProxy val)
            {
                return h1 != val.GetUnit();
            }
            public static bool operator ==(UnitBasedProxy val, WoWUnit h1)
            {
                return h1 == val.GetUnit();
            }
            public static bool operator !=(UnitBasedProxy val, WoWUnit h1)
            {
                return h1 != val.GetUnit();
            }

            public UnitBasedProxy(GetUnitDelegate del)
            {
                GetUnit = del;
            }

        }

        public abstract class ResourceProxy : UnitBasedProxy
        {

            public ResourceProxy(GetUnitDelegate del) : base(del)
            {
                cache["regen"].SetRetrievalDelegate(() =>
                {
                    if (simc.main_resource == simc.mana) return (Decimal)Me.ManaInfo.RegenFlatModifier;
                    return LuaGet<double>(
                        "inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen;", 0);
                });
                cache["percent"].SetRetrievalDelegate(() => GetPercent);
                cache["current"].SetRetrievalDelegate(() => GetCurrent);
                cache["max"].SetRetrievalDelegate(() => GetMax);

            }

            public MagicValueType time_to_max
            {
                get { return deficit/regen; }
            }


            public MagicValueType pct
            {
                get { return cache["percent"].GetValue(); }
            }

            public MagicValueType percent
            {
                get { return cache["percent"].GetValue(); }
            }

            public MagicValueType current
            {
                get { return cache["current"].GetValue(); }
            }

            public MagicValueType max
            {
                get { return cache["max"].GetValue(); }
            }
            public MagicValueType regen
            {
                get { return cache["regen"].GetValue(); }
            }

            public abstract MagicValueType GetPercent { get; }
            public abstract MagicValueType GetMax { get; }
            public abstract MagicValueType GetCurrent { get; }

            public MagicValueType deficit
            {
                get { return new MagicValueType(GetUnit() != null ? (int) (max - current) : 0); }
            }

            private ProxyCacheEntry cache = new ProxyCacheEntry();



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
                return h1.GetUnit() != null ? h2.current*h1.current : 0;
            }

            public static double operator /(ResourceProxy h1, ResourceProxy h2)
            {
                return h1.GetUnit() != null ? h1.current/h2.current : 0;
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


        }

        public class HealthProxy : ResourceProxy
        {
            public static Decimal AVG_DPS = 22000;

            public HealthProxy(GetUnitDelegate del)
                : base(del)
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
                        return new MagicValueType(Math.Max(GetCurrent/(((Decimal)StyxWoW.Me.GroupInfo.GroupSize)*(Decimal)0.6*AVG_DPS),1));

                    return new MagicValueType(GetCurrent/(AVG_DPS));
                }
            }
        }

        public class ManaProxy : ResourceProxy
        {
            public static int AVG_DPS = 22000;

            public ManaProxy(GetUnitDelegate del)
                : base(del)
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
            public ComboPointProxy(GetUnitDelegate del)
                : base(del)
            {
            }

            public override MagicValueType GetPercent
            {
                get { return MagicValueType.Zero; }
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
            public HolyPowerProxy(GetUnitDelegate del)
                : base(del)
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
            public RuneProxy(GetUnitDelegate del, RuneType type)
                : base(del)
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
                    LuaGet<double>(
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
            public ChiProxy(GetUnitDelegate del)
                : base(del)
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
                get { return MagicValueType.Zero; }
            }
        }

        public class EnergyProxy : ResourceProxy
        {
            public EnergyProxy(GetUnitDelegate del)
                : base(del)
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
            public EclipseProxy(GetUnitDelegate del)
                : base(del)
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
            public RunicPowerProxy(GetUnitDelegate del)
                : base(del)
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
            public FocusProxy(GetUnitDelegate del)
                : base(del)
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
            public RageProxy(GetUnitDelegate del)
                : base(del)
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