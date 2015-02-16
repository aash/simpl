#region Honorbuddy

using Styx;
using CommonBehaviors.Actions;

#endregion

#region System

#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        private readonly int TIGEREYE_ACTV = 116740;
        private readonly int TIGEREYE_PASV = 125195;
        private int Elusive_Brew_Stack = 128939;

        #region Windwalker

        [Behavior(WoWClass.Monk, WoWSpec.MonkWindwalker, WoWContext.PvE)]
        public void CreateWindWalkerPvEBehavior()
        {
            //actions+=/invoke_xuen
            actions += Cast(Invoke_Xuen_the_White_Tiger, ret => cooldowns_enabled);
            //actions+=/storm_earth_and_fire,target=2,if=debuff.storm_earth_and_fire_target.down
            //actions+=/storm_earth_and_fire,target=3,if=debuff.storm_earth_and_fire_target.down
            actions += CallActionList("opener",
                ret => Time < 20 && talent[Serenity].enabled && talent[Chi_Brew].enabled && cooldown[Fists_of_Fury].up);
            //actions+=/chi_sphere,if=talent.power_strikes.enabled&buff.chi_sphere.react&chi<4
            //actions+=/potion,name=draenic_agility,if=buff.serenity.up|(!talent.serenity.enabled&trinket.proc.agility.react)
            actions += UseItem(13, ret => cooldowns_enabled && buff[TIGEREYE_ACTV].up || target.time_to_die < 18);
            actions += Cast(Blood_Fury, ret => buff[TIGEREYE_ACTV].up || target.time_to_die < 18);
            actions += Cast(Berserking, ret => buff[TIGEREYE_ACTV].up || target.time_to_die < 18);
            actions += Cast(Arcane_Torrent,
                ret => chi.Max - chi >= 1 && (buff[TIGEREYE_ACTV].up || target.time_to_die < 18));
            actions += Cast(Chi_Brew,
                ret =>
                    cooldowns_enabled && chi.Max - chi >= 2 &&
                    ((charges == 1 && recharge_time <= 20) | charges == 2 || target.time_to_die < charges*10) &&
                    buff[TIGEREYE_PASV].Stack <= 16);
            actions += Cast(Tiger_Palm, ret => buff[Tiger_Power].remains < 6.6);
            actions += Cast(Tigereye_Brew, ret => buff[TIGEREYE_ACTV].down && buff[TIGEREYE_PASV].Stack == 20);
            actions += Cast(Tigereye_Brew,
                ret => buff[TIGEREYE_ACTV].down && buff[TIGEREYE_PASV].Stack >= 9 && buff[Serenity].up);
            actions += Cast(Tigereye_Brew,
                ret =>
                    buff[TIGEREYE_ACTV].down && buff[TIGEREYE_PASV].Stack >= 9 && cooldown[Fists_of_Fury].up && chi >= 3 &&
                    debuff[Rising_Sun_Kick].Up && buff[Tiger_Power].up);
            actions += Cast(Tigereye_Brew,
                ret =>
                    talent[Hurricane_Strike].enabled && buff[TIGEREYE_ACTV].down && buff[TIGEREYE_PASV].Stack >= 9 &&
                    cooldown[Hurricane_Strike].up && chi >= 3 && debuff[Rising_Sun_Kick].Up && buff[Tiger_Power].up);
            actions += Cast(Tigereye_Brew,
                ret =>
                    buff[TIGEREYE_ACTV].down && chi >= 2 && (buff[TIGEREYE_PASV].Stack >= 16 || target.time_to_die < 40) &&
                    debuff[Rising_Sun_Kick].Up && buff[Tiger_Power].up);
            actions += Cast(Rising_Sun_Kick,
                ret => (debuff[Rising_Sun_Kick].down || debuff[Rising_Sun_Kick].remains < 3));
            actions += Cast(Serenity,
                ret => cooldowns_enabled && (chi >= 2 && buff[Tiger_Power].up && debuff[Rising_Sun_Kick].Up));
            actions += Cast(Fists_of_Fury,
                ret =>
                    buff[Tiger_Power].remains > channel_time && debuff[Rising_Sun_Kick].remains > channel_time &&
                    !buff[Serenity].up);
            //actions+=/fortifying_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&(glyph.touch_of_death.enabled|chi>=3)
            actions += Cast(Touch_of_Death, ret => cooldowns_enabled && target.health.pct < 10);
            actions += Cast(Hurricane_Strike,
                ret =>
                    cooldowns_enabled && energy.time_to_max > channel_time && buff[Tiger_Power].remains > channel_time &&
                    debuff[Rising_Sun_Kick].remains > channel_time && buff[Energizing_Brew].down);
            actions += Cast(Energizing_Brew,
                ret =>
                    cooldowns_enabled && cooldown[Fists_of_Fury].remains > 6 &&
                    (!talent[Serenity].enabled || (!buff[Serenity].up && cooldown[Serenity].remains > 4)) &&
                    energy + energy.Regen*1.5 < 50);
            actions += CallActionList("single_target", ret => active_enemies < 3 && !talent[Chi_Explosion].enabled);
            actions += CallActionList("single_target_chi_ex",
                ret => active_enemies == 1 && talent[Chi_Explosion].enabled);
            actions += CallActionList("cleave_chi_ex", ret => active_enemies == 2 && talent[Chi_Explosion].enabled);
            actions += CallActionList("aoe", ret => active_enemies >= 3 && !talent[Rushing_Jade_Wind].enabled);
            actions += CallActionList("aoe_rushing_jade_wind",
                ret => active_enemies >= 3 && talent[Rushing_Jade_Wind].enabled);

            actions.opener += Cast(Tigereye_Brew, ret => buff[TIGEREYE_ACTV].down && buff[TIGEREYE_PASV].Stack >= 9);
            actions.opener += UseItem(13, ret => cooldowns_enabled && buff[TIGEREYE_ACTV].up);
            actions.opener += Cast(Blood_Fury, ret => buff[TIGEREYE_ACTV].up);
            actions.opener += Cast(Berserking, ret => buff[TIGEREYE_ACTV].up);
            actions.opener += Cast(Arcane_Torrent, ret => buff[TIGEREYE_ACTV].up && chi.Max - chi >= 1);
            actions.opener += Cast(Fists_of_Fury,
                ret =>
                    buff[Tiger_Power].remains > channel_time && debuff[Rising_Sun_Kick].remains > channel_time &&
                    buff[Serenity].up && buff[Serenity].remains < 1.5);
            actions.opener += Cast(Tiger_Palm, ret => buff[Tiger_Power].remains < 2);
            actions.opener += Cast(Rising_Sun_Kick, ret => true);
            actions.opener += Cast(Blackout_Kick, ret => chi.Max - chi <= 2 && buff[Serenity].up);
            actions.opener += Cast(Chi_Brew, ret => chi.Max - chi >= 2);
            actions.opener += Cast(Serenity, ret => cooldowns_enabled && chi.Max - chi <= 2);
            actions.opener += Cast(Jab, ret => chi.Max - chi >= 2 && !buff[Serenity].up);
            actions.opener += new ActionAlwaysSucceed();

            actions.single_target += Cast(Rising_Sun_Kick);
            actions.single_target += Cast(Blackout_Kick,
                ret => buff[Combo_Breaker_Blackout_Kick].up || buff[Serenity].up);
            actions.single_target += Cast(Tiger_Palm,
                ret => buff[Combo_Breaker_Tiger_Palm].up && buff[Combo_Breaker_Tiger_Palm].remains <= 2);
            actions.single_target += Cast(Chi_Wave, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.single_target += Cast(Chi_Burst, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.single_target += Cast(Chi_Torpedo, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.single_target += Cast(Blackout_Kick, ret => chi.Max - chi < 2);
            actions.single_target += Cast(Expel_Harm, ret => chi.Max - chi >= 2 && health.pct < 95);
            actions.single_target += Cast(Jab, ret => chi.Max - chi >= 2);

            actions.single_target_chi_ex += Cast(Chi_Explosion,
                ret => chi >= 2 && buff[Combo_Breaker_Chi_Explosion].up && cooldown[Fists_of_Fury].remains > 2);
            actions.single_target_chi_ex += Cast(Tiger_Palm,
                ret => buff[Combo_Breaker_Tiger_Palm].up && buff[Combo_Breaker_Tiger_Palm].remains <= 2);
            actions.single_target_chi_ex += Cast(Chi_Wave, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.single_target_chi_ex += Cast(Chi_Burst, ret => energy.time_to_max > 2 && buff[Serenity].down);
            //actions.st_chix += CycleTargets(Zen_Sphere, ret => energy.TimeToMax>2&&GetAuraTimeLeft(ret, Rupture, true).TotalMilliseconds > 100);
            actions.single_target_chi_ex += Cast(Rising_Sun_Kick);
            actions.single_target_chi_ex += Cast(Tiger_Palm, ret => chi == 4 && !buff[Combo_Breaker_Tiger_Palm].up);
            actions.single_target_chi_ex += Cast(Chi_Explosion, ret => chi >= 3 && cooldown[Fists_of_Fury].remains > 4);
            actions.single_target_chi_ex += Cast(Chi_Torpedo, ret => energy.time_to_max > 2);
            actions.single_target_chi_ex += Cast(Expel_Harm, ret => chi.Max - chi >= 2 && health.pct < 95);
            actions.single_target_chi_ex += Cast(Jab, ret => chi.Max - chi >= 2);

            actions.cleave_chi_ex += Cast(Chi_Explosion, ret => chi >= 4 && cooldown[Fists_of_Fury].remains > 4);
            actions.cleave_chi_ex += Cast(Tiger_Palm,
                ret => buff[Combo_Breaker_Tiger_Palm].up && buff[Combo_Breaker_Tiger_Palm].remains <= 2);
            actions.cleave_chi_ex += Cast(Chi_Wave, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.cleave_chi_ex += Cast(Chi_Burst, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.cleave_chi_ex += Cast(Chi_Torpedo, ret => energy.time_to_max > 2);
            actions.cleave_chi_ex += Cast(Expel_Harm, ret => chi.Max - chi >= 2 && health.pct < 95);
            actions.cleave_chi_ex += Cast(Jab, ret => chi.Max - chi >= 2);

            actions.aoe += Cast(Chi_Explosion, ret => chi >= 4 && cooldown[Fists_of_Fury].remains > 4);
            actions.aoe += Cast(Rising_Sun_Kick, ret => chi == chi.Max);
            actions.aoe += Cast(Chi_Wave, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.aoe += Cast(Chi_Burst, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.aoe += Cast(Chi_Torpedo, ret => energy.time_to_max > 2);
            actions.aoe += Cast(Spinning_Crane_Kick);

            actions.aoe_rushing_jade_wind += Cast(Chi_Explosion, ret => chi >= 4 && cooldown[Fists_of_Fury].remains > 4);
            actions.aoe_rushing_jade_wind += Cast(Rushing_Jade_Wind);
            actions.aoe_rushing_jade_wind += Cast(Chi_Wave, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.aoe_rushing_jade_wind += Cast(Chi_Burst, ret => energy.time_to_max > 2 && buff[Serenity].down);
            actions.aoe_rushing_jade_wind += Cast(Blackout_Kick,
                ret => buff[Combo_Breaker_Blackout_Kick].up || buff[Serenity].up);
            actions.aoe_rushing_jade_wind += Cast(Tiger_Palm,
                ret => buff[Combo_Breaker_Tiger_Palm].up && buff[Combo_Breaker_Tiger_Palm].remains <= 2);
            actions.aoe_rushing_jade_wind += Cast(Blackout_Kick,
                ret => chi.Max - chi < 2 && (cooldown[Fists_of_Fury].remains > 3 || !talent[Rushing_Jade_Wind].enabled));
            actions.aoe_rushing_jade_wind += Cast(Chi_Torpedo, ret => energy.time_to_max > 2);
            actions.aoe_rushing_jade_wind += Cast(Expel_Harm, ret => chi.Max - chi >= 2 && health.pct < 95);
            actions.aoe_rushing_jade_wind += Cast(Jab, ret => chi.Max - chi >= 2);
        }

        [Behavior(WoWClass.Monk, WoWSpec.MonkWindwalker, WoWContext.PvP)]
        public void CreateWindWalkerPvPBehavior()
        {
            actions += Cast(Touch_of_Death, ret => target.health.pct < 10);
            actions += Cast(Rising_Sun_Kick, ret => chi >= 2);
            actions += Cast(Blackout_Kick,
                ret => buff[Combo_Breaker_Blackout_Kick].up && buff[Combo_Breaker_Blackout_Kick].remains <= 2);
            actions += Cast(Tiger_Palm, ret => buff[Tiger_Power].down);
            actions += Cast(Tiger_Palm,
                ret => buff[Combo_Breaker_Tiger_Palm].up && buff[Combo_Breaker_Tiger_Palm].remains <= 2);
            actions += Cast(Blackout_Kick, ret => chi >= 2);
            actions += Cast(Expel_Harm, ret => chi.deficit >= 2 && health.deficit > 8000);
            actions += Cast(Jab, ret => energy >= 50 && chi.deficit >= 2);
            actions += Cast(Chi_Explosion, ret => chi >= 2);
        }

        #endregion
    }
}