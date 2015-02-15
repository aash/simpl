#region Honorbuddy

#endregion

#region System

using System;
using System.Collections.Generic;
using System.Linq;
using Bots.Professionbuddy;
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

        private String _conditionSpell;
        public WoWUnit CycleTarget;
        public char NameCount = Convert.ToChar(65);

        public static int active_enemies
        {
            get { return actives.Count(); }
        }

        public static IEnumerable<WoWUnit> actives
        {
            get
            {
                var _class = StyxWoW.Me.Class;
                var ranged = (_class == WoWClass.Mage || _class == WoWClass.Hunter || _class == WoWClass.Priest ||
                              _class == WoWClass.Warlock ||
                              (_class == WoWClass.Druid && StyxWoW.Me.Specialization == WoWSpec.DruidBalance));

                if (ranged) return UnfriendlyUnitsNearTarget(10f);
                return UnfriendlyUnitsNearMe(5f);
            }
        }

        public bool position_front
        {
            get { return !Me.IsBehind(conditionUnit); }
        }

        public WoWUnit clickUnit { get; set; }

        public int anticipation_charges
        {
            get { return buff[Anticipation].Stack; }
        }

        public bool bloodlust_up
        {
            get { return buff[Heroism].up || buff[Bloodlust].up || buff[Time_Warp].up; }
        }

        public double BloodlustRemains
        {
            get
            {
                return buff[Heroism].up
                    ? buff[Heroism].remains
                    : buff[Bloodlust].up ? buff[Bloodlust].remains : buff[Time_Warp].up ? buff[Time_Warp].remains : 0.0;
            }
        }

        public bool melee_range
        {
            get { return Me.IsWithinMeleeRangeOf(conditionUnit); }
        }

        public bool facing
        {
            get { return target.facing; }
        }

        public static bool SpellIsTargeting
        {
            get
            {
                return Lua.GetReturnVal<Boolean>(
                    "return SpellIsTargeting()", 0);
            }
        }

        public static bool damage_enabled
        {
            get { return !_damageEnabled; }
        }

        public static bool aoe_enabled
        {
            get { return !_aoeEnabled; }
        }

        public static bool cooldowns_enabled
        {
            get { return !_cdsEnabled; }
        }

        public bool Ticking
        {
            get { return debuff[_conditionSpell].Ticking; }
        }

        public double channel_time
        {
            get { return spell[_conditionSpell].ChannelTime; }
        }

        public double recharge_time
        {
            get { return spell[_conditionSpell].RechargeTime; }
        }

        public int charges
        {
            get { return spell[_conditionSpell].Charges; }
        }

        public double CastRegen
        {
            get
            {
                var cTime = spell[_conditionSpell].CastTime;
                return focus.Cast_Regen(cTime > 0 ? cTime : 1);
            }
        }

        public Composite CallActionList(Composite list, CanRunDecoratorDelegate del)
        {
            return new Decorator(del, list);
        }

        public Composite CallActionList(String name, CanRunDecoratorDelegate del = null)
        {
            return new Decorator(del ?? (ret => true), actions[name]);
        }

        public Composite Cast(String spell, CanRunDecoratorDelegate del = null, WoWUnit _target = null,
            String Reason = "")
        {
            NameCount++;

            return new NamedComposite("" + NameCount, spell,
                new Action(delegate
                {
                    _conditionSpell = spell;
                    if (_target == null || _target == default(WoWUnit)) conditionUnit = Me.CurrentTarget;
                    else conditionUnit = _target;
                    return RunStatus.Failure;
                }),
                new Decorator(del ?? (ret => true),
                    new Action(delegate
                    {
                        if (CastSpell(spell, conditionUnit is ClusterUnit ? Me.CurrentTarget : conditionUnit, 3, Reason))
                        {
                            clickUnit = conditionUnit;
                            return RunStatus.Success;
                        }
                        return RunStatus.Failure;
                    })));
        }

        public Composite UseItem(String name, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    var item = Me.BagItems.Where(ret => ret.Name.Equals(name)).FirstOrDefault();
                    if (item.Cooldown <= 0) item.Use();
                }));
        }

        public Composite UseItem(uint slot, CanRunDecoratorDelegate del = null, String Reason = "")
        {
            return new Decorator(del ?? (ret => true),
                new Action(delegate
                {
                    Me.Inventory.Equipped.Trinket1.Use();
                    Me.Inventory.Equipped.Trinket2.Use();
                    return RunStatus.Failure;
                }));
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
                            //Logging.Write(DateTime.Now+": Pooling Energy for " + spell + " " + Energy.Current + "/" + energy);
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

        public Composite CycleTargets(String _spell, UnitCriteriaDelegate criteria, String Reason = "")
        {
            return new PrioritySelector(
                new Action(delegate
                {
                    _conditionSpell = _spell;
                    foreach (var w in UnfriendlyUnitsNearMe((float)spell[_spell].Range))
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
                        if (CastSpell(_spell, CycleTarget, 3, Reason)) return RunStatus.Success;
                        return RunStatus.Failure;
                    })));
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

        #region [Spell / Logging]

        public bool CastSpell(string spellName, WoWUnit u, int LC, String Reason)
        {
            if (!ValidUnit(u) || u == null)
                return false;

            //Logging.Write("{7}: CR: {0} / {1} - BR {2} / {3} - MR {4} - Combat {5} {6} - CanCast: {8} / {9}", Me.CombatReach, Me.CurrentTarget.CombatReach, Me.BoundingRadius, Me.CurrentTarget.BoundingRadius, Me.CurrentTarget.IsWithinMeleeRange, Me.IsActuallyInCombat, Me.Combat, DateTime.Now, SpellManager.CanCast(spellName, u), spellName);

            if (SpellManager.CanCast(spellName, u))
                if (SpellManager.Cast(spellName, u))
                {
                    if (Reason.Length > 1) Logging.Write("Casting " + spellName + " to " + Reason);
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
            return Lua.GetReturnVal<bool>("usable, nomana = IsUsableSpell(\"" + spellname + "\"); return usable;",0);
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
                Logging.Write(name + " = " + spell);
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

            public ClusterUnit(double range, double radius) : base(IntPtr.Zero)
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
                var dist = Math.Sqrt(dx*dx + dy*dy);

                return (dist <= radius0 + radius1);
            }

            public WoWPoint ideal_location()
            {
                var unfs = UnfriendlyUnitsNearMe((float) (range));
                var cList = new List<Circle>();

                unfs.ForEach(ret => cList.Add(new Circle(ret.CombatReach, ret.Location, ret == Me.CurrentTarget)));
                /*cList.ForEach(ret =>
                {
                    if (ret.Location.X > maxx) maxx = ret.Location.X;
                    if (ret.Location.X < minx) minx = ret.Location.X;
                    if (ret.Location.Y > maxy) maxy = ret.Location.Y;
                    if (ret.Location.Y < miny) miny = ret.Location.Y;
                });
                int v = 150;

                var rincx = Math.Ceiling((maxx - minx) / v);
                var rincy = Math.Ceiling((maxy - miny) / v);

                var circlex = minx + radius;
                var circley = miny + radius;

                double px = 0;
                double py = 0;

                var cints = 0;
                var ddist = Double.PositiveInfinity;
                var cmt = false;

                /*
                 10 Grad Schritte
                    X = r * cosine(angle)
                    Y = r * sine(angle)
                */


                var centerx = Me.CurrentTarget.Location.X;
                var centery = Me.CurrentTarget.Location.Y;
                var cints = 0;
                var ddist = Double.PositiveInfinity;
                double px = 0;
                double py = 0;
                int ites = 0;

                for (int i = 0; i < 360; i += 20)
                {
                    for (int y = (int)Math.Ceiling(radius)+2; y > 0; y--)
                    {
                        var cx = centerx + (y - 1) * Math.Cos(i);
                        var cy = centery + (y - 1) * Math.Sin(i);
                        var ints = 0;
                        double dist = 0;

                        ites++;
                        

                        cList.ForEach(
                            ret =>
                            {
                                if (DoCirclesIntersect(cx, cy, radius, ret.Location.X,
                                    ret.Location.Y, ret.Radius))
                                {
                                    var dx = (cx) - ret.Location.X;
                                    var dy = (cy) - ret.Location.Y;
                                    var _dist = Math.Sqrt(dx * dx + dy * dy);
                                    dist += dist <= radius / 2 ? radius / 2 : _dist;
                                    ints++;
                                }
                            });

                        if (ints > cints)
                        {
                            cints = ints;
                            px = cx;
                            py = cy;
                            //Logging.Write("New Biggest Cluster " + ints + " dist: " + dist + " " + px + "/" + py);
                        }
                        if (dist < ddist && ints == cints)
                        {
                            ddist = dist;
                            px = cx;
                            py = cy;
                            //Logging.Write("New Best Cluster for MT and Dist" + ints + " dist: " + dist + " "+px+"/"+py);
                        }

                    }

                }

                /*for (var x = 0; x < v; x++)
                {
                    for (var y = 0; y < v; y++)
                    {
                        var ints = 0;
                        double dist = 0;
                        bool mt = false;
                        cList.ForEach(
                            ret =>
                            {
                                if (DoCirclesIntersect(circlex + rincx*x, circley + rincy*y, radius, ret.Location.X,
                                    ret.Location.Y, ret.Radius))
                                {

                                    var dx = (circlex + rincx * x) - ret.Location.X;
                                    var dy = (circley + rincy * y) - ret.Location.Y;
                                    var _dist = Math.Sqrt(dx * dx + dy * dy);
                                    dist += ret.MT ? _dist : _dist <= radius / 2 ? radius / 2 : _dist;
                                    ints++;
                                    if (ret.MT) mt = true;
                                }
                            });
                        if (!mt) continue;
                        if (ints > cints)
                        {
                            cints = ints;
                            px = circlex + rincx*x;
                            py = circley + rincy*y;
                           // Logging.Write("New Biggest Cluster " + ints + " dist: " + dist + " " + px + "/" + py);
                        }
                        if (ints == cints)
                        {
                            /*if (mt && !cmt)
                            {
                                cmt = true;
                                ddist = dist;
                                px = circlex + rinc * x;
                                py = circley + rinc * y;
                                Logging.Write("New Best Cluster for MT " + ints + " dist: " + dist + " " + px + "/" + py);
                            }*/
                            /*if (dist < ddist)
                            {
                                ddist = dist;
                                px = circlex + rincx * x;
                                py = circley + rincy * y;
                                //Logging.Write("New Best Cluster for MT and Dist" + ints + " dist: " + dist + " "+px+"/"+py);
                            }
                        }
                    }
                }*/
                //Logging.Write("Found Cluster of {0} px {1} py {2} ites {3}", cints,  px, py, ites);
                if (cints == 0) return Me.CurrentTarget.Location;
                if (cints == 1) return Me.CurrentTarget.Location;
                return new WoWPoint(px, py, StyxWoW.Me.CurrentTarget.Z);
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

        private static List<WoWUnit> UnfriendlyUnits
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
                return wantedAura != null ? (int) wantedAura.StackCount : 0;
            }
            return 0;
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
                return wantedAura != null ? (int) wantedAura.StackCount : 0;
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
