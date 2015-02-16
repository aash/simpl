#region Honorbuddy

using Styx;

#endregion

#region System

#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {


        #region PvE
        /*
        actions=auto_shot
        actions+=/arcane_torrent,if=focus.deficit>=30
        actions+=/blood_fury
        actions+=/berserking
        actions+=/potion,name=draenic_agility,if=(((cooldown.stampede.remains<1)&(cooldown.a_murder_of_crows.remains<1))&(trinket.stat.any.up|buff.archmages_greater_incandescence_agi.up))|target.time_to_die<=25
        actions+=/call_action_list,name=aoe,if=active_enemies>1
        actions+=/stampede,if=buff.potion.up|(cooldown.potion.remains&(buff.archmages_greater_incandescence_agi.up|trinket.stat.any.up))|target.time_to_die<=25
        actions+=/a_murder_of_crows
        actions+=/black_arrow,if=!ticking
        actions+=/explosive_shot
        actions+=/dire_beast
        actions+=/arcane_shot,if=buff.thrill_of_the_hunt.react&focus>35&cast_regen<=focus.deficit|dot.serpent_sting.remains<=3|target.time_to_die<4.5
        actions+=/explosive_trap
        # Cast a second shot for steady focus if that won't cap us.
        actions+=/cobra_shot,if=buff.pre_steady_focus.up&buff.steady_focus.remains<5&(14+cast_regen)<=focus.deficit
        actions+=/arcane_shot,if=focus>=80|talent.focusing_shot.enabled
        actions+=/focusing_shot
        actions+=/cobra_shot

        actions.aoe=stampede,if=buff.potion.up|(cooldown.potion.remains&(buff.archmages_greater_incandescence_agi.up|trinket.stat.any.up|buff.archmages_incandescence_agi.up))
        actions.aoe+=/explosive_shot,if=buff.lock_and_load.react&(!talent.barrage.enabled|cooldown.barrage.remains>0)
        actions.aoe+=/barrage
        actions.aoe+=/black_arrow,if=!ticking
        actions.aoe+=/explosive_shot,if=active_enemies<5
        actions.aoe+=/explosive_trap,if=dot.explosive_trap.remains<=5
        actions.aoe+=/a_murder_of_crows
        actions.aoe+=/dire_beast
        actions.aoe+=/multishot,if=buff.thrill_of_the_hunt.react&focus>50&cast_regen<=focus.deficit|dot.serpent_sting.remains<=5|target.time_to_die<4.5
        actions.aoe+=/glaive_toss
        actions.aoe+=/powershot
        actions.aoe+=/cobra_shot,if=buff.pre_steady_focus.up&buff.steady_focus.remains<5&focus+14+cast_regen<80
        actions.aoe+=/multishot,if=focus>=70|talent.focusing_shot.enabled
        actions.aoe+=/focusing_shot
        actions.aoe+=/cobra_shot
        */
        [Behavior(WoWClass.Hunter, WoWSpec.HunterSurvival, WoWContext.PvE)]
        public void GenerateSurvivalPvEBehavior()
        {
            actions += Cast(Arcane_Torrent, ret => focus.deficit >= 30);
            actions += Cast(Blood_Fury);
            actions += Cast(Berserking);
            actions += CallActionList("aoe", ret => active_enemies > 1);
            actions += Cast(A_Murder_of_Crows);
            actions += Cast(Black_Arrow, ret => !ticking);
            actions += Cast(Explosive_Shot);
            actions += Cast(Dire_Beast);
            actions += Cast(Arcane_Shot,
                ret =>
                    buff[Thrill_of_the_Hunt].react && focus > 35 && cast_regen <= focus.deficit ||
                    dot[Serpent_Sting].remains <= 3 || target.time_to_die < 4.5);
            actions += Cast(Explosive_Trap);
            actions += Cast(Cobra_Shot,
                ret => buff.Pre_Steady_Focus && buff[Steady_Focus].remains < 5 && (14 + cast_regen) <= focus.deficit);
            actions += Cast(Arcane_Shot, ret => focus >= 80 | talent[Focusing_Shot].enabled);
            actions += Cast(Focusing_Shot);
            actions += Cast(Cobra_Shot);

            actions.aoe += Cast(Explosive_Shot,
                ret => buff[Lock_and_Load].react && (!talent[Barrage].enabled || cooldown[Barrage].Down));
            actions.aoe += Cast(Barrage);
            actions.aoe += Cast(Black_Arrow, ret => !ticking);
            actions.aoe += Cast(Explosive_Shot, ret => active_enemies < 5);
            actions.aoe += Cast(Explosive_Trap);
            actions.aoe += Cast(A_Murder_of_Crows);
            actions.aoe += Cast(Dire_Beast);
            actions.aoe += Cast(MultiShot,
                ret =>
                    buff[Thrill_of_the_Hunt].react && focus > 50 &&
                    cast_regen <= focus.deficit | dot[Serpent_Sting].remains <= 5 || target.time_to_die < 4.5);
            actions.aoe += Cast(Glaive_Toss);
            actions.aoe += Cast(Powershot);
            actions.aoe += Cast(Cobra_Shot,
                ret => buff.Pre_Steady_Focus && buff[Steady_Focus].remains < 5 && focus + 14 + cast_regen < 80);
            actions.aoe += Cast(MultiShot, ret => focus >= 70 || talent[Focusing_Shot].enabled);
            actions.aoe += Cast(Focusing_Shot);
            actions.aoe += Cast(Cobra_Shot);
        }

        #endregion
    }
}