#region Honorbuddy

using Styx;

#endregion

#region System

using System.Linq;

#endregion

namespace Simcraft
{
    public partial class SimcraftImpl
    {
        [Behavior(WoWClass.Rogue, WoWSpec.RogueSubtlety, WoWContext.PvE)]
        public void GenerateSubtletyPvEBehavior()
        {
            actions += UseItem(13, ret => buff[Shadow_Dance].up);
            actions += Cast(Premeditation,
                ret => combo_points < 4 || (talent[Anticipation].enabled && (combo_points + anticipation_charges < 9)));
            actions += Cast(Shadow_Reflection, ret => buff[Shadow_Dance].up);
            actions += PoolResource(Shadow_Dance, pool => cooldown[Shadow_Dance].up && energy < 50,
                ret =>
                    cooldowns_enabled &&
                    (buff[Stealth].down && buff[Vanish].down && debuff[Find_Weakness].remains < 2 ||
                     (buff[Bloodlust].up && (dot[Hemorrhage].Ticking || dot[Garrote].Ticking || dot[Rupture].Ticking))));
            actions += PoolResource(Vanish, pool => cooldown[Vanish].up && energy < 50,
                ret =>
                    cooldowns_enabled &&
                    (talent[Shadow_Focus].enabled &&
                     (combo_points < 4 || (talent[Anticipation].enabled && combo_points + anticipation_charges < 9)) &&
                     buff[Shadow_Dance].down && buff[Master_of_Subtlety].down && debuff[Find_Weakness].remains < 2));
            actions += PoolResource(Vanish, pool => cooldown[Vanish].up && energy < 90,
                ret =>
                    cooldowns_enabled &&
                    (talent[Subterfuge].enabled &&
                     (combo_points < 4 || (talent[Anticipation].enabled && combo_points + anticipation_charges < 9)) &&
                     buff[Shadow_Dance].down && buff[Master_of_Subtlety].down && debuff[Find_Weakness].remains < 2));
            actions += Cast(Marked_for_Death, ret => combo_points == 0);
            actions += CallActionList("finisher", ret => combo_points == 5);
            actions += CallActionList("generator",
                ret => combo_points < 5 || (talent[Anticipation].enabled && anticipation_charges < 4));

            actions.generator += CallActionList("pool",
                ret =>
                    buff[Master_of_Subtlety].down && buff[Shadow_Dance].down && debuff[Find_Weakness].down &&
                    (energy + cooldown[Shadow_Dance].remains*energy.Regen < energy.GetMax ||
                     energy + cooldown[Vanish].remains*energy.Regen <= energy.GetMax));
            //The Ambush cast is a small hack
            actions.generator += PoolResource(Ambush, ret => true, ret => IsUseableSpell(Ambush) == SpellState.NoMana);
            actions.generator += Cast(Ambush);
            actions.generator += Cast(Fan_of_Knives, ret => active_enemies > 1 && aoe_enabled);
            actions.generator += Cast(Hemorrhage,
                ret =>
                    (debuff[Hemorrhage].remains < 7.2 && target.time_to_die >= debuff[Hemorrhage].remains + 24 + 8 &&
                     debuff[Find_Weakness].down) || !ticking || position_front);
            actions.generator += Cast(Backstab,
                ret =>
                    !position_front && !talent[Death_from_Above].enabled || energy >= energy.GetMax - energy.Regen ||
                    target.time_to_die < 10);
            actions.generator += CallActionList("pool");

            actions.finisher += CycleTargets(Rupture,
                ret =>
                    melee_range && facing &&
                    ((dot[Rupture].remains < 8) && active_enemies <= 8 &&
                     (cooldown[Death_from_Above].remains > 0 || !talent[Death_from_Above].enabled) ||
                     (buff[Shadow_Reflection].remains > 8 && dot[Rupture].remains < 12 &&
                      buff[Shadow_Reflection].remains < 10)) && target.time_to_die >= 8);
            actions.finisher += Cast(Slice_and_Dice,
                ret => (buff[Slice_and_Dice].remains < 10.8) & buff[Slice_and_Dice].remains < target.time_to_die);
            actions.finisher += Cast(Death_from_Above);
            actions.finisher += Cast(Crimson_Tempest,
                ret =>
                    (active_enemies >= 2 && debuff[Find_Weakness].down) ||
                    (active_enemies >= 3 &&
                     (cooldown[Death_from_Above].remains > 0 || !talent[Death_from_Above].enabled)));
            actions.finisher += Cast(Eviscerate);

            actions.pool += Cast(Preparation, ret => !buff[Vanish].up && cooldown[Vanish].remains > 60);
        }

        [Behavior(WoWClass.Rogue, WoWSpec.RogueCombat, WoWContext.PvE)]
        public void GenerateCombatPvEBehavior()
        {
            actions += Cast(Preparation, ret => !buff[Vanish].up && cooldown[Vanish].remains > 30);
            actions += UseItem(13, ret => cooldowns_enabled && target.melee_range);
            actions += Cast(Blade_Flurry, ret => (active_enemies >= 2 && !buff[Blade_Flurry].up));
            actions += Cast(Ambush);
            actions += Cast(Vanish,ret =>cooldowns_enabled && Time > 10 &&(combo_points < 3 || (talent[Anticipation].enabled && anticipation_charges < 3) ||(combo_points < 4 || (talent[Anticipation].enabled && anticipation_charges < 4))) &&((talent[Shadow_Focus].enabled && buff[Adrenaline_Rush].down && energy < 90 && energy >= 15) ||(talent[Subterfuge].enabled && energy >= 90) ||(!talent[Shadow_Focus].enabled && !talent[Subterfuge].enabled && energy >= 60)));
            actions += Cast(Slice_and_Dice,ret =>buff[Slice_and_Dice].remains < 2 ||((target.time_to_die > 45 && combo_points == 5 && buff[Slice_and_Dice].remains < 12) &&buff[Deep_Insight].down));
            actions += CallActionList("adrenaline_rush",ret => target.melee_range && (energy < 35 || bloodlust_up) && cooldown[Killing_Spree].remains > 10);
            actions += CallActionList("killing_spree",ret =>(energy < 40 || (bloodlust_up) || BloodlustRemains > 20) && buff[Adrenaline_Rush].down &&(!talent[Shadow_Reflection].enabled || cooldown[Shadow_Reflection].remains > 30 ||buff[Shadow_Reflection].remains > 3));
            actions += CallActionList("generator",ret =>combo_points < 5 || !dot[Revealing_Strike].Ticking ||(talent[Anticipation].enabled && anticipation_charges <= 4 && buff[Deep_Insight].down));actions += CallActionList("finisher",ret =>combo_points == 5 && dot[Revealing_Strike].Ticking &&(buff[Deep_Insight].up || !talent[Anticipation].enabled ||(talent[Anticipation].enabled && anticipation_charges >= 4)));

            actions.adrenaline_rush += Cast(Adrenaline_Rush, ret => cooldowns_enabled);

            actions.killing_spree += Cast(Killing_Spree, ret => cooldowns_enabled);

            actions.generator += Cast(Revealing_Strike,ret =>(combo_points == 4 && dot[Revealing_Strike].remains < 7.2 &&(target.time_to_die > dot[Revealing_Strike].remains + 7.2)) || !ticking);
            actions.generator += Cast(Sinister_Strike, ret => dot[Revealing_Strike].Ticking);

            actions.finisher += Cast(Death_from_Above);
            actions.finisher += Cast(Eviscerate);
        }
    }
}