using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx;
using Styx.WoWInternals.WoWObjects;

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        public MagicValueType anticipation_charges
        {
            get { return new MagicValueType(buff.anticipation.stack); }
        }

        public MagicValueType cooldown_react
        {
            get
            {
                return cooldown[_conditionSpell].up;
            }
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

        public MagicValueType level
        {
            get { return new MagicValueType(Me.Level); }
        }

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
                var cDist = conditionUnit.Distance - conditionUnit.CombatReach;
                var dDist = (Decimal) cDist;
                dDist = Math.Max(dDist, 1);
                return new MagicValueType(dDist / (Decimal)_conditionSpell.speed);
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
                var be = LuaGet<int>("return UnitPower(\"player\", SPELL_POWER_BURNING_EMBERS, true)", 0);
                return new MagicValueType((double) be/10);
            }
        }

        public MagicValueType tick_time
        {
            get { return debuff[_conditionSpell].tick_time; }
        }

        public IEnumerable<WoWUnit> actives
        {
            get
            {
               
                var ranged = (_class == WoWClass.Mage || _class == WoWClass.Hunter || _class == WoWClass.Priest ||
                              _class == WoWClass.Warlock ||
                              (_class == WoWClass.Druid && StyxWoW.Me.Specialization == WoWSpec.DruidBalance));
                 
                //var ranged = _conditionSpell.max_range > 10;
                if (ranged) return UnfriendlyUnitsNearTarget(8f);
                return UnfriendlyUnitsNearMe(5f);
            }
        }

        public String eclipse_direction
        {
            get
            {
                return LuaGet<String>("direction = GetEclipseDirection(); return direction;", 0);
            }
        }

        public MagicValueType eclipse_change
        {
            get
            {
                double eclipse_amount = eclipse_energy.current;
                double time_to_next_lunar, time_to_next_solar, eclipse_ch;
                double M_PI = Math.PI;

                double phi_lunar = Math.Asin(100.0/105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2*M_PI/40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = eclipse_direction;
                double phi;

                if (dir.Equals("moon"))
                    phi = 2*M_PI - Math.Asin(eclipse_amount/105) + M_PI;
                else
                    phi = 2*M_PI + Math.Asin(eclipse_amount/105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi%(2*M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2*M_PI)%(2*M_PI))/omega/1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2*M_PI)%(2*M_PI))/omega/1000;


                eclipse_ch = (M_PI - (phi%M_PI))/omega/1000;

                //Write("energy {3} change: {0} next_solar: {1} next_lunar: {2} phi: {4}",eclipse_ch,time_to_next_solar,time_to_next_lunar,eclipse_energy.current,phi);

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

                double phi_lunar = Math.Asin(100.0/105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2*M_PI/40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = eclipse_direction;
                double phi;

                if (dir.Equals("moon"))
                    phi = 2*M_PI - Math.Asin(eclipse_amount/105) + M_PI;
                else
                    phi = 2*M_PI + Math.Asin(eclipse_amount/105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi%(2*M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2*M_PI)%(2*M_PI))/omega/1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2*M_PI)%(2*M_PI))/omega/1000;


                eclipse_ch = (M_PI - (phi%M_PI))/omega/1000;

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

                double phi_lunar = Math.Asin(100.0/105.0);

                double phi_solar = phi_lunar + M_PI;

                double omega = 2*M_PI/40000;

                //double balance_time = (20000 * (2 * M_PI - Math.Asin(eclipse_amount / 105))) / M_PI;

                var dir = eclipse_direction;
                double phi;

                if (dir.Equals("moon"))
                    phi = 2*M_PI - Math.Asin(eclipse_amount/105) + M_PI;
                else
                    phi = 2*M_PI + Math.Asin(eclipse_amount/105);

                if (talent.euphoria.enabled) omega *= 2;

                phi = phi%(2*M_PI);

                if (eclipse_amount >= 100)
                    time_to_next_lunar = 0;
                else
                    time_to_next_lunar = ((phi_lunar - phi + 2*M_PI)%(2*M_PI))/omega/1000;

                if (eclipse_amount <= -100)
                    time_to_next_solar = 0;
                else
                    time_to_next_solar = ((phi_solar - phi + 2*M_PI)%(2*M_PI))/omega/1000;


                eclipse_ch = (M_PI - (phi%M_PI))/omega/1000;

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

        public MagicValueType ptr
        {
            get { return new MagicValueType(false); }
        }

        public MagicValueType miss_react
        {
            get { return new MagicValueType(true); }
        }

        public MagicValueType incanters_flow_dir
        {
            get { return new MagicValueType(buff.incanters_flow.dir); }
        }

        public MagicValueType damage
        {
            get
            {
                return health.deficit;
            }
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
            get { return conditionUnit; }
        }


        public MagicValueType distance
        {
            get { return new MagicValueType(conditionUnit.Distance); }
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
                
                var be = LuaGet<int>("return UnitPower(\"player\", SPELL_POWER_DEMONIC_FURY)", 0);
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

        public MagicValueType soul_shard
        {
            get
            {
                int ss = (int) Me.CurrentSoulShards;
                if (ss > o_soul_shard)
                {
                    SoulShardTimer.Restart();
                }
                o_soul_shard = ss;
                return new MagicValueType(ss);
            }
        }

        public MagicValueType charges
        {
            get { return spell[_conditionSpell].charges; }
        }

        public int ticks_remain
        {
            get { return debuff[_conditionSpell].ticks_remain; }
        }

        public bool line_cd(double seconds)
        {
            if (!line_cds.ContainsKey(conditionName))
                line_cds[conditionName] = new Stopwatch();

            var b = !line_cds[conditionName].IsRunning || line_cds[conditionName].ElapsedMilliseconds/1000 > seconds;
            //line_cds[conditionName].Restart();
            return b;
        }



        public MagicValueType cast_regen
        {
            get
            {
                var cTime = spell[_conditionSpell].cast_time;
                return main_resource.cast_regen(cTime);
            }
        }

        public MagicValueType sync(String spell)
        {
            return new MagicValueType(true);
        }
    }
}