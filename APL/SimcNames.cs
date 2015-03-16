using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Styx;

namespace Simcraft.APL
{
    class SimcNames
    {
        //(.+?)\"(.+?)\"(.+?)\};
        //$1\"$2\"$3\}; //http://www.wowhead.com/search?q=$2#specialization
        public class SpecPair
        {
            public SpecPair(WoWSpec none, uint i)
            {
                V1 = none;
                V2 = i;
            }

            public WoWSpec V1 { get; set; }
            public uint V2 { get; set; }
        }
        public class SpecList : List<SimcNames.SpecPair> { }

        public static Dictionary<String, SpecList> spells = new Dictionary<string, SpecList>();
        public static Dictionary<String, SpecList> buffs = new Dictionary<string, SpecList>();
        public static Dictionary<String, SpecList> debuffs = new Dictionary<string, SpecList>();

        public static void Populate()
        {
            spells.Clear();
            buffs.Clear();
            debuffs.Clear();

            spells["stance"] = new SpecList { };
            spells["food"] = new SpecList { };
            spells["choose_target"] = new SpecList { }; //http://www.wowhead.com/search?q=choose_target#specialization
            spells["flask"] = new SpecList { }; //http://www.wowhead.com/search?q=flask#specialization
            spells["exotic_munitions"] = new SpecList { }; //http://www.wowhead.com/search?q=exotic_munitions#specialization
            spells["summon_pet"] = new SpecList { }; //http://www.wowhead.com/search?q=summon_pet#specialization
            spells["heroic_charge"] = new SpecList { }; //http://www.wowhead.com/search?q=heroic_charge#specialization
            spells["cancel_buff"] = new SpecList { }; //http://www.wowhead.com/search?q=cancel_buff#specialization
            spells["apply_poison"] = new SpecList { }; //http://www.wowhead.com/search?q=apply_poison#specialization
            spells["spell_dot"] = new SpecList();
            spells["melee_nuke"] = new SpecList();
            spells["spell_nuke"] = new SpecList();
            spells["spell_aoe"] = new SpecList();
            spells["potion"] = new SpecList();
            spells["call_action_list"] = new SpecList();
            spells["use_item"] = new SpecList();
            spells["pool_resource"] = new SpecList();
            spells["wait"] = new SpecList();
            spells["run_action_list"] = new SpecList();
            spells["cancel_metamorphosis"] = new SpecList();
            spells["chi_sphere"] = new SpecList(); //http://www.wowhead.com/search?q=chi_sphere#specialization


            buffs["damage_taken"] = new SpecList();
            //buffs["casting"] = new SpecList();
            buffs["flying"] = new SpecList();
            buffs["colossus_smash_up"] = new SpecList();
            buffs["ravager_protection"] = new SpecList();
            buffs["beast_within"] = new SpecList();
            //buffs["potion"] = new SpecList();
            //buffs["pre_steady_focus"] = new SpecList();
            buffs["stampede"] = new SpecList();
            buffs["rising_sun_kick"] = new SpecList { }; //http://www.wowhead.com/search?q=rising_sun_kick#specialization
            buffs["vendetta"] = new SpecList { }; //http://www.wowhead.com/search?q=vendetta#specialization
            buffs["find_weakness"] = new SpecList { }; //http://www.wowhead.com/search?q=find_weakness#specialization
            buffs["storm_earth_and_fire_target"] = new SpecList { new SpecPair(WoWSpec.None, 138130) };
            buffs["rune_of_power"] = new SpecList { }; //http://www.wowhead.com/search?q=rune_of_power#specialization
            buffs["ravager"] = new SpecList { }; //http://www.wowhead.com/search?q=ravager#specialization
            buffs["chi_sphere"] = new SpecList { }; //http://www.wowhead.com/search?q=chi_sphere#specialization
            buffs["colossus_smash"] = new SpecList { }; //http://www.wowhead.com/search?q=colossus_smash#specialization

            debuffs["casting"] = new SpecList { };
            debuffs["flying"] = new SpecList { };
            debuffs["damage_taken"] = new SpecList { };

            spells["mark_of_the_wild"] = new SpecList { new SpecPair(WoWSpec.None, 1126) };
            spells["starsurge"] = new SpecList { new SpecPair(WoWSpec.None, 78674) };
            spells["celestial_alignment"] = new SpecList { new SpecPair(WoWSpec.None, 112071) };
            spells["sunfire"] = new SpecList { new SpecPair(WoWSpec.None, 93402) }; //http://www.wowhead.com/search?q=sunfire#specialization
            spells["stellar_flare"] = new SpecList { new SpecPair(WoWSpec.None, 152221) }; //http://www.wowhead.com/search?q=stellar_flare#specialization
            spells["moonfire"] = new SpecList { new SpecPair(WoWSpec.None, 8921) }; //http://www.wowhead.com/search?q=moonfire#specialization
            spells["wrath"] = new SpecList { new SpecPair(WoWSpec.None, 5176) }; //http://www.wowhead.com/search?q=wrath#specialization
            spells["starfire"] = new SpecList { new SpecPair(WoWSpec.None, 2912) }; //http://www.wowhead.com/search?q=starfire#specialization
            spells["starfall"] = new SpecList { new SpecPair(WoWSpec.None, 48505) }; //http://www.wowhead.com/search?q=starfall#specialization
            spells["rip"] = new SpecList { new SpecPair(WoWSpec.None, 1079) }; //http://www.wowhead.com/search?q=rip#specialization
            spells["moonfire_cat"] = new SpecList { new SpecPair(WoWSpec.None, 155625) }; //http://www.wowhead.com/search?q=moonfire_cat#specialization
            spells["shred"] = new SpecList { new SpecPair(WoWSpec.None, 5221) }; //http://www.wowhead.com/search?q=shred#specialization

            spells["nether_tempest"] = new SpecList { new SpecPair(WoWSpec.None, 114923) }; //http://www.wowhead.com/search?q=nether_tempest#specialization
            spells["supernova"] = new SpecList { new SpecPair(WoWSpec.None, 157980) }; //http://www.wowhead.com/search?q=supernova#specialization

            spells["arcane_explosion"] = new SpecList { new SpecPair(WoWSpec.None, 1449) }; //http://www.wowhead.com/search?q=arcane_explosion#specialization
            spells["arcane_barrage"] = new SpecList { new SpecPair(WoWSpec.None, 44425) }; //http://www.wowhead.com/search?q=arcane_barrage#specialization
            spells["cone_of_cold"] = new SpecList { new SpecPair(WoWSpec.None, 120) }; //http://www.wowhead.com/search?q=cone_of_cold#specialization
            spells["pyroblast"] = new SpecList { new SpecPair(WoWSpec.None, 11366) }; //http://www.wowhead.com/search?q=pyroblast#specialization
            spells["start_pyro_chain"] = new SpecList { }; //http://www.wowhead.com/search?q=start_pyro_chain#specialization
            spells["blast_wave"] = new SpecList { new SpecPair(WoWSpec.None, 157981) }; //http://www.wowhead.com/search?q=blast_wave#specialization
            spells["living_bomb"] = new SpecList { new SpecPair(WoWSpec.None, 44457) }; //http://www.wowhead.com/search?q=living_bomb#specialization
            spells["flamestrike"] = new SpecList { new SpecPair(WoWSpec.None, 2120) }; //http://www.wowhead.com/search?q=flamestrike#specialization
            spells["scorch"] = new SpecList { new SpecPair(WoWSpec.None, 2948) }; //http://www.wowhead.com/search?q=scorch#specialization
            spells["frost_bomb"] = new SpecList { new SpecPair(WoWSpec.None, 112948) }; //http://www.wowhead.com/search?q=frost_bomb#specialization
            spells["ice_nova"] = new SpecList { new SpecPair(WoWSpec.None, 157997) }; //http://www.wowhead.com/search?q=ice_nova#specialization
            spells["frostfire_bolt"] = new SpecList { new SpecPair(WoWSpec.None, 44614) }; //http://www.wowhead.com/search?q=frostfire_bolt#specialization
            spells["blizzard"] = new SpecList { new SpecPair(WoWSpec.None, 10) }; //http://www.wowhead.com/search?q=blizzard#specialization

            spells["blessing_of_kings"] = new SpecList { new SpecPair(WoWSpec.None, 20217) }; //http://www.wowhead.com/search?q=blessing_of_kings#specialization
            spells["blessing_of_might"] = new SpecList { new SpecPair(WoWSpec.None, 19740) }; //http://www.wowhead.com/search?q=blessing_of_might#specialization
            spells["seal_of_truth"] = new SpecList { new SpecPair(WoWSpec.None, 31801) }; //http://www.wowhead.com/search?q=seal_of_truth#specialization
            spells["divine_storm"] = new SpecList { new SpecPair(WoWSpec.None, 53385) }; //http://www.wowhead.com/search?q=divine_storm#specialization
            spells["templars_verdict"] = new SpecList { new SpecPair(WoWSpec.None, 85256) }; //http://www.wowhead.com/search?q=templars_verdict#specialization
            spells["final_verdict"] = new SpecList { new SpecPair(WoWSpec.None, 157048) }; //http://www.wowhead.com/search?q=final_verdict#specialization
            spells["exorcism"] = new SpecList { new SpecPair(WoWSpec.None, 879) }; //http://www.wowhead.com/search?q=exorcism#specialization
            spells["power_word_fortitude"] = new SpecList { new SpecPair(WoWSpec.None, 21562) }; //http://www.wowhead.com/search?q=power_word_fortitude#specialization
            spells["shadowform"] = new SpecList { new SpecPair(WoWSpec.None, 15473) }; //http://www.wowhead.com/search?q=shadowform#specialization
            spells["devouring_plague"] = new SpecList { new SpecPair(WoWSpec.None, 2944) }; //http://www.wowhead.com/search?q=devouring_plague#specialization
            spells["searing_insanity"] = new SpecList { new SpecPair(WoWSpec.None, 179338) }; //http://www.wowhead.com/search?q=searing_insanity#specialization
            spells["insanity"] = new SpecList { new SpecPair(WoWSpec.None, 129197) }; //http://www.wowhead.com/search?q=insanity#specialization
            spells["vampiric_touch"] = new SpecList { new SpecPair(WoWSpec.None, 34914) }; //http://www.wowhead.com/search?q=vampiric_touch#specialization
            spells["mind_spike"] = new SpecList { new SpecPair(WoWSpec.None, 73510) }; //http://www.wowhead.com/search?q=mind_spike#specialization
            spells["mind_flay"] = new SpecList { new SpecPair(WoWSpec.None, 15407) }; //http://www.wowhead.com/search?q=mind_flay#specialization
            spells["power_word_shield"] = new SpecList { new SpecPair(WoWSpec.None, 17) }; //http://www.wowhead.com/search?q=power_word_shield#specialization
            spells["void_entropy"] = new SpecList { new SpecPair(WoWSpec.None, 155361) }; //http://www.wowhead.com/search?q=void_entropy#specialization
            spells["lightning_shield"] = new SpecList { new SpecPair(WoWSpec.None, 324) }; //http://www.wowhead.com/search?q=lightning_shield#specialization
            spells["spiritwalkers_grace"] = new SpecList { new SpecPair(WoWSpec.None, 79206) }; //http://www.wowhead.com/search?q=spiritwalkers_grace#specialization
            spells["earth_shock"] = new SpecList { new SpecPair(WoWSpec.None, 8042) }; //http://www.wowhead.com/search?q=earth_shock#specialization
            spells["unleash_flame"] = new SpecList { new SpecPair(WoWSpec.None, 165462) }; //http://www.wowhead.com/search?q=unleash_flame#specialization
            spells["flame_shock"] = new SpecList { new SpecPair(WoWSpec.None, 8050) }; //http://www.wowhead.com/search?q=flame_shock#specialization
            spells["earthquake"] = new SpecList { new SpecPair(WoWSpec.None, 61882) }; //http://www.wowhead.com/search?q=earthquake#specialization
            spells["searing_totem"] = new SpecList { new SpecPair(WoWSpec.None, 3599) }; //http://www.wowhead.com/search?q=searing_totem#specialization
            spells["thunderstorm"] = new SpecList { new SpecPair(WoWSpec.None, 51490) }; //http://www.wowhead.com/search?q=thunderstorm#specialization
            spells["chain_lightning"] = new SpecList { new SpecPair(WoWSpec.None, 421) }; //http://www.wowhead.com/search?q=chain_lightning#specialization
            spells["unleash_elements"] = new SpecList { new SpecPair(WoWSpec.None, 73680) }; //http://www.wowhead.com/search?q=unleash_elements#specialization
            spells["lightning_bolt"] = new SpecList { new SpecPair(WoWSpec.None, 403) }; //http://www.wowhead.com/search?q=lightning_bolt#specialization
            spells["frost_shock"] = new SpecList { new SpecPair(WoWSpec.None, 8056) }; //http://www.wowhead.com/search?q=frost_shock#specialization
            spells["magma_totem"] = new SpecList { new SpecPair(WoWSpec.None, 8190) }; //http://www.wowhead.com/search?q=magma_totem#specialization
            spells["lava_lash"] = new SpecList { new SpecPair(WoWSpec.None, 60103) }; //http://www.wowhead.com/search?q=lava_lash#specialization
            spells["windstrike"] = new SpecList { new SpecPair(WoWSpec.None, 115356) }; //http://www.wowhead.com/search?q=windstrike#specialization
            spells["stormstrike"] = new SpecList { new SpecPair(WoWSpec.None, 17364) }; //http://www.wowhead.com/search?q=stormstrike#specialization
            spells["dark_intent"] = new SpecList { new SpecPair(WoWSpec.None, 109773) }; //http://www.wowhead.com/search?q=dark_intent#specialization
            spells["mend_pet"] = new SpecList { new SpecPair(WoWSpec.None, 136) }; //http://www.wowhead.com/search?q=dark_intent#specialization
            spells["grimoire_of_sacrifice"] = new SpecList { new SpecPair(WoWSpec.None, 108503) }; //http://www.wowhead.com/search?q=grimoire_of_sacrifice#specialization
            spells["shadowburn"] = new SpecList { new SpecPair(WoWSpec.None, 17877) }; //http://www.wowhead.com/search?q=shadowburn#specialization
            spells["fire_and_brimstone"] = new SpecList { new SpecPair(WoWSpec.None, 108683) }; //http://www.wowhead.com/search?q=fire_and_brimstone#specialization

            spells["chaos_bolt"] = new SpecList { new SpecPair(WoWSpec.None, 116858) }; //http://www.wowhead.com/search?q=chaos_bolt#specialization
            spells["conflagrate"] = new SpecList { new SpecPair(WoWSpec.None, 17962) }; //http://www.wowhead.com/search?q=conflagrate#specialization
            spells["rain_of_fire"] = new SpecList { new SpecPair(WoWSpec.None, 5740) }; //http://www.wowhead.com/search?q=rain_of_fire#specialization
            spells["havoc"] = new SpecList { new SpecPair(WoWSpec.None, 80240) }; //http://www.wowhead.com/search?q=havoc#specialization
            spells["charge"] = new SpecList { new SpecPair(WoWSpec.None, 100) }; //http://www.wowhead.com/search?q=charge#specialization
            spells["mortal_strike"] = new SpecList { new SpecPair(WoWSpec.None, 12294) }; //http://www.wowhead.com/search?q=mortal_strike#specialization
            spells["execute"] = new SpecList { new SpecPair(WoWSpec.WarriorArms, 163201), new SpecPair(WoWSpec.WarriorFury, 5308), new SpecPair(WoWSpec.WarriorProtection, 5308) }; //http://www.wowhead.com/search?q=execute#specialization
            spells["impending_victory"] = new SpecList { new SpecPair(WoWSpec.None, 103840) }; //http://www.wowhead.com/search?q=impending_victory#specialization
            spells["slam"] = new SpecList { new SpecPair(WoWSpec.None, 1464) }; //http://www.wowhead.com/search?q=slam#specialization
            spells["thunder_clap"] = new SpecList { new SpecPair(WoWSpec.None, 6343) }; //http://www.wowhead.com/search?q=thunder_clap#specialization
            spells["whirlwind"] = new SpecList { new SpecPair(WoWSpec.None, 1680) }; //http://www.wowhead.com/search?q=whirlwind#specialization
            spells["rend"] = new SpecList { new SpecPair(WoWSpec.None, 772) }; //http://www.wowhead.com/search?q=rend#specialization

            spells["wild_strike"] = new SpecList { new SpecPair(WoWSpec.None, 100130) }; //http://www.wowhead.com/search?q=wild_strike#specialization
            spells["shockwave"] = new SpecList { new SpecPair(WoWSpec.None, 46968) }; //http://www.wowhead.com/search?q=shockwave#specialization
            spells["raging_blow"] = new SpecList { new SpecPair(WoWSpec.None, 85288) }; //http://www.wowhead.com/search?q=raging_blow#specialization
            spells["devastate"] = new SpecList { new SpecPair(WoWSpec.None, 20243) }; //http://www.wowhead.com/search?q=devastate#specialization
            spells["shield_barrier"] = new SpecList { new SpecPair(WoWSpec.None, 47585) }; //http://www.wowhead.com/search?q=shield_barrier#specialization
            spells["demoralizing_shout"] = new SpecList { new SpecPair(WoWSpec.None, 47585) }; //http://www.wowhead.com/search?q=demoralizing_shout#specialization
            spells["enraged_regeneration"] = new SpecList { new SpecPair(WoWSpec.None, 47585) }; //http://www.wowhead.com/search?q=enraged_regeneration#specialization
            spells["shield_wall"] = new SpecList { new SpecPair(WoWSpec.None, 174926) }; //http://www.wowhead.com/search?q=shield_wall#specialization
            spells["last_stand"] = new SpecList { new SpecPair(WoWSpec.None, 12975) }; //http://www.wowhead.com/search?q=last_stand#specialization
            spells["stoneform"] = new SpecList { new SpecPair(WoWSpec.None, 20594) }; //http://www.wowhead.com/search?q=stoneform#specialization
            spells["victory_rush"] = new SpecList { new SpecPair(WoWSpec.None, 34428) }; //http://www.wowhead.com/search?q=victory_rush#specialization
            spells["blood_tap"] = new SpecList { new SpecPair(WoWSpec.None, 45529) }; //http://www.wowhead.com/search?q=blood_tap#specialization
            spells["frost_strike"] = new SpecList { new SpecPair(WoWSpec.None, 49143) }; //http://www.wowhead.com/search?q=frost_strike#specialization
            spells["howling_blast"] = new SpecList { new SpecPair(WoWSpec.None, 49184) }; //http://www.wowhead.com/search?q=howling_blast#specialization
            spells["obliterate"] = new SpecList { new SpecPair(WoWSpec.None, 49020) }; //http://www.wowhead.com/search?q=obliterate#specialization
            spells["scourge_strike"] = new SpecList { new SpecPair(WoWSpec.None, 55090) }; //http://www.wowhead.com/search?q=scourge_strike#specialization
            spells["festering_strike"] = new SpecList { new SpecPair(WoWSpec.None, 85948) }; //http://www.wowhead.com/search?q=festering_strike#specialization
            spells["blackout_kick"] = new SpecList { new SpecPair(WoWSpec.None, 100784) }; //http://www.wowhead.com/search?q=blackout_kick#specialization
            spells["purifying_brew"] = new SpecList { new SpecPair(WoWSpec.None, 119582) }; //http://www.wowhead.com/search?q=purifying_brew#specialization
            spells["chi_explosion"] = new SpecList { new SpecPair(WoWSpec.MonkBrewmaster, 157676), new SpecPair(WoWSpec.MonkMistweaver, 157675), new SpecPair(WoWSpec.MonkWindwalker, 152174) }; //http://www.wowhead.com/search?q=chi_explosion#specialization
            spells["guard"] = new SpecList { new SpecPair(WoWSpec.None, 115295) }; //http://www.wowhead.com/search?q=guard#specialization
            spells["chi_burst"] = new SpecList { new SpecPair(WoWSpec.None, 123986) }; //http://www.wowhead.com/search?q=chi_burst#specialization
            spells["chi_wave"] = new SpecList { new SpecPair(WoWSpec.None, 115098) }; //http://www.wowhead.com/search?q=chi_wave#specialization
            spells["zen_sphere"] = new SpecList { new SpecPair(WoWSpec.None, 124081) }; //http://www.wowhead.com/search?q=zen_sphere#specialization
            spells["jab"] = new SpecList { new SpecPair(WoWSpec.None, 100780) }; //http://www.wowhead.com/search?q=jab#specialization
            spells["breath_of_fire"] = new SpecList { new SpecPair(WoWSpec.None, 115181) }; //http://www.wowhead.com/search?q=breath_of_fire#specialization
            spells["rushing_jade_wind"] = new SpecList { new SpecPair(WoWSpec.None, 116847) }; //http://www.wowhead.com/search?q=rushing_jade_wind#specialization
            spells["chi_torpedo"] = new SpecList { new SpecPair(WoWSpec.None, 115008) }; //http://www.wowhead.com/search?q=chi_torpedo#specialization

            spells["adrenaline_rush"] = new SpecList { new SpecPair(WoWSpec.None, 13750) }; //http://www.wowhead.com/search?q=adrenaline_rush#specialization
            spells["sinister_strike"] = new SpecList { new SpecPair(WoWSpec.None, 1752) }; //http://www.wowhead.com/search?q=sinister_strike#specialization
            spells["eviscerate"] = new SpecList { new SpecPair(WoWSpec.None, 2098) }; //http://www.wowhead.com/search?q=eviscerate#specialization
            spells["hemorrhage"] = new SpecList { new SpecPair(WoWSpec.None, 16511) }; //http://www.wowhead.com/search?q=hemorrhage#specialization
            spells["shuriken_toss"] = new SpecList { new SpecPair(WoWSpec.None, 114014) }; //http://www.wowhead.com/search?q=shuriken_toss#specialization


            spells["berserk"] = new SpecList { new SpecPair(WoWSpec.None, 106952), }; //http://www.wowhead.com/search?q=berserk#specialization
            spells["incarnation"] = new SpecList { new SpecPair(WoWSpec.None, 117679), }; //http://www.wowhead.com/search?q=incarnation#specialization
            spells["tigers_fury"] = new SpecList { new SpecPair(WoWSpec.None, 5217), }; //http://www.wowhead.com/search?q=tigers_fury#specialization
            spells["stampede"] = new SpecList { new SpecPair(WoWSpec.None, 121818) }; //http://www.wowhead.com/search?q=stampede#specialization
            spells["bestial_wrath"] = new SpecList { new SpecPair(WoWSpec.None, 19574), }; //http://www.wowhead.com/search?q=bestial_wrath#specialization
            spells["aimed_shot"] = new SpecList { new SpecPair(WoWSpec.None, 19434), }; //http://www.wowhead.com/search?q=aimed_shot#specialization
            spells["rapid_fire"] = new SpecList { new SpecPair(WoWSpec.None, 3045), }; //http://www.wowhead.com/search?q=rapid_fire#specialization
            spells["a_murder_of_crows"] = new SpecList { new SpecPair(WoWSpec.None, 131894), }; //http://www.wowhead.com/search?q=a_murder_of_crows#specialization

            spells["barrage"] = new SpecList { new SpecPair(WoWSpec.None, 120360), }; //http://www.wowhead.com/search?q=barrage#specialization
            spells["arcane_missiles"] = new SpecList { new SpecPair(WoWSpec.None, 5143) }; //http://www.wowhead.com/search?q=arcane_missiles#specialization
            spells["presence_of_mind"] = new SpecList { new SpecPair(WoWSpec.None, 12043), }; //http://www.wowhead.com/search?q=presence_of_mind#specialization
            spells["prismatic_crystal"] = new SpecList { new SpecPair(WoWSpec.None, 152087), }; //http://www.wowhead.com/search?q=prismatic_crystal#specialization
            spells["evocation"] = new SpecList { new SpecPair(WoWSpec.None, 12051) }; //http://www.wowhead.com/search?q=evocation#specialization
            spells["arcane_power"] = new SpecList { new SpecPair(WoWSpec.None, 12042), }; //http://www.wowhead.com/search?q=arcane_power#specialization
            spells["cold_snap"] = new SpecList { new SpecPair(WoWSpec.None, 11958), }; //http://www.wowhead.com/search?q=cold_snap#specialization
            spells["arcane_blast"] = new SpecList { new SpecPair(WoWSpec.None, 30451) }; //http://www.wowhead.com/search?q=arcane_blast#specialization
            spells["arcane_orb"] = new SpecList { new SpecPair(WoWSpec.None, 153626), }; //http://www.wowhead.com/search?q=arcane_orb#specialization
            spells["fireball"] = new SpecList { new SpecPair(WoWSpec.None, 133), }; //http://www.wowhead.com/search?q=fireball#specialization
            spells["meteor"] = new SpecList { new SpecPair(WoWSpec.None, 153561), }; //http://www.wowhead.com/search?q=meteor#specialization
            spells["combustion"] = new SpecList { new SpecPair(WoWSpec.None, 11129) }; //http://www.wowhead.com/search?q=combustion#specialization
            spells["dragons_breath"] = new SpecList { new SpecPair(WoWSpec.None, 31661), }; //http://www.wowhead.com/search?q=dragons_breath#specialization
            spells["inferno_blast"] = new SpecList { new SpecPair(WoWSpec.None, 108853) }; //http://www.wowhead.com/search?q=inferno_blast#specialization
            spells["frostbolt"] = new SpecList { new SpecPair(WoWSpec.None, 116) }; //http://www.wowhead.com/search?q=frostbolt#specialization
            spells["icy_veins"] = new SpecList { new SpecPair(WoWSpec.None, 12472) }; //http://www.wowhead.com/search?q=icy_veins#specialization
            spells["frozen_orb"] = new SpecList { new SpecPair(WoWSpec.None, 84714), }; //http://www.wowhead.com/search?q=frozen_orb#specialization
            spells["water_jet"] = new SpecList { new SpecPair(WoWSpec.None, 135029), }; //http://www.wowhead.com/search?q=water_jet#specialization
            spells["ice_lance"] = new SpecList { new SpecPair(WoWSpec.None, 30455), }; //http://www.wowhead.com/search?q=ice_lance#specialization
            spells["seraphim"] = new SpecList { new SpecPair(WoWSpec.None, 152262), }; //http://www.wowhead.com/search?q=seraphim#specialization
            spells["judgment"] = new SpecList { new SpecPair(WoWSpec.None, 20271) }; //http://www.wowhead.com/search?q=judgment#specialization
            spells["crusader_strike"] = new SpecList { new SpecPair(WoWSpec.None, 35395), }; //http://www.wowhead.com/search?q=crusader_strike#specialization
            spells["divine_protection"] = new SpecList { new SpecPair(WoWSpec.None, 498) }; //http://www.wowhead.com/search?q=divine_protection#specialization
            spells["avenging_wrath"] = new SpecList { new SpecPair(WoWSpec.PaladinHoly, 31842), new SpecPair(WoWSpec.PaladinRetribution, 31884) }; //http://www.wowhead.com/search?q=avenging_wrath#specialization
            spells["dispersion"] = new SpecList { new SpecPair(WoWSpec.None, 47585) }; //http://www.wowhead.com/search?q=dispersion#specialization
            spells["mind_blast"] = new SpecList { new SpecPair(WoWSpec.None, 8092) }; //http://www.wowhead.com/search?q=mind_blast#specialization
            spells["shadow_word_death"] = new SpecList { new SpecPair(WoWSpec.None, 32379), }; //http://www.wowhead.com/search?q=shadow_word_death#specialization
            spells["ascendance"] = new SpecList { new SpecPair(WoWSpec.ShamanElemental, 114050), new SpecPair(WoWSpec.ShamanEnhancement, 114051), new SpecPair(WoWSpec.ShamanRestoration, 114052), }; //http://www.wowhead.com/search?q=ascendance#specialization
            spells["fire_elemental_totem"] = new SpecList { new SpecPair(WoWSpec.None, 2894) }; //http://www.wowhead.com/search?q=fire_elemental_totem#specialization
            spells["lava_burst"] = new SpecList { new SpecPair(WoWSpec.None, 51505), }; //http://www.wowhead.com/search?q=lava_burst#specialization
            spells["elemental_blast"] = new SpecList { new SpecPair(WoWSpec.None, 117014), }; //http://www.wowhead.com/search?q=elemental_blast#specialization
            spells["storm_elemental_totem"] = new SpecList { new SpecPair(WoWSpec.None, 152256), }; //http://www.wowhead.com/search?q=storm_elemental_totem#specialization
            spells["shock"] = new SpecList { new SpecPair(WoWSpec.None, 26415), }; //http://www.wowhead.com/search?q=shock#specialization
            spells["fire_nova"] = new SpecList { new SpecPair(WoWSpec.None, 1535), }; //http://www.wowhead.com/search?q=fire_nova#specialization
            spells["cataclysm"] = new SpecList { new SpecPair(WoWSpec.None, 152108), }; //http://www.wowhead.com/search?q=cataclysm#specialization
            spells["imp_swarm"] = new SpecList { new SpecPair(WoWSpec.None, 104316), }; //http://www.wowhead.com/search?q=imp_swarm#specialization
            spells["dark_soul"] = new SpecList { new SpecPair(WoWSpec.WarlockAffliction, 113860), new SpecPair(WoWSpec.WarlockDemonology, 113861), new SpecPair(WoWSpec.WarlockDestruction, 113858), }; //http://www.wowhead.com/search?q=dark_soul#specialization
            spells["wrathstorm"] = new SpecList { new SpecPair(WoWSpec.None, 115831) }; //http://www.wowhead.com/search?q=wrathstorm#specialization
            spells["shadow_bolt"] = new SpecList { new SpecPair(WoWSpec.None, 686), }; //http://www.wowhead.com/search?q=shadow_bolt#specialization
            spells["hand_of_guldan"] = new SpecList { new SpecPair(WoWSpec.None, 105174), }; //http://www.wowhead.com/search?q=hand_of_guldan#specialization
            spells["metamorphosis"] = new SpecList { new SpecPair(WoWSpec.None, 103958) }; //http://www.wowhead.com/search?q=metamorphosis#specialization
            spells["soul_fire"] = new SpecList { new SpecPair(WoWSpec.None, 6353), }; //http://www.wowhead.com/search?q=soul_fire#specialization
            spells["demonbolt"] = new SpecList { new SpecPair(WoWSpec.None, 157695), }; //http://www.wowhead.com/search?q=demonbolt#specialization
            spells["immolate"] = new SpecList { new SpecPair(WoWSpec.None, 348), }; //http://www.wowhead.com/search?q=immolate#specialization
            spells["bladestorm"] = new SpecList { new SpecPair(WoWSpec.None, 46924), }; //http://www.wowhead.com/search?q=bladestorm#specialization
            spells["colossus_smash"] = new SpecList { new SpecPair(WoWSpec.None, 167105), }; //http://www.wowhead.com/search?q=colossus_smash#specialization
            spells["recklessness"] = new SpecList { new SpecPair(WoWSpec.None, 1719), }; //http://www.wowhead.com/search?q=recklessness#specialization
            spells["bloodthirst"] = new SpecList { new SpecPair(WoWSpec.None, 23881), }; //http://www.wowhead.com/search?q=bloodthirst#specialization
            spells["shield_slam"] = new SpecList { new SpecPair(WoWSpec.None, 23922), }; //http://www.wowhead.com/search?q=shield_slam#specialization
            spells["bloodbath"] = new SpecList { new SpecPair(WoWSpec.None, 12292), }; //http://www.wowhead.com/search?q=bloodbath#specialization
            spells["shield_block"] = new SpecList { new SpecPair(WoWSpec.None, 2565), }; //http://www.wowhead.com/search?q=shield_block#specialization
            spells["dragon_roar"] = new SpecList { new SpecPair(WoWSpec.None, 118000) }; //http://www.wowhead.com/search?q=dragon_roar#specialization
            spells["storm_bolt"] = new SpecList { new SpecPair(WoWSpec.None, 107570), }; //http://www.wowhead.com/search?q=storm_bolt#specialization
            spells["ravager"] = new SpecList { new SpecPair(WoWSpec.None, 152277), }; //http://www.wowhead.com/search?q=ravager#specialization
            spells["avatar"] = new SpecList { new SpecPair(WoWSpec.None, 107574), }; //http://www.wowhead.com/search?q=avatar#specialization
            spells["outbreak"] = new SpecList { new SpecPair(WoWSpec.None, 77575), }; //http://www.wowhead.com/search?q=outbreak#specialization
            spells["unholy_blight"] = new SpecList { new SpecPair(WoWSpec.None, 115989) }; //http://www.wowhead.com/search?q=unholy_blight#specialization
            spells["breath_of_sindragosa"] = new SpecList { new SpecPair(WoWSpec.None, 152279), }; //http://www.wowhead.com/search?q=breath_of_sindragosa#specialization
            spells["soul_reaper"] = new SpecList { new SpecPair(WoWSpec.DeathKnightBlood, 114866), new SpecPair(WoWSpec.DeathKnightFrost, 130735) }; //http://www.wowhead.com/search?q=soul_reaper#specialization
            spells["defile"] = new SpecList { new SpecPair(WoWSpec.None, 152280), }; //http://www.wowhead.com/search?q=defile#specialization
            spells["antimagic_shell"] = new SpecList { new SpecPair(WoWSpec.None, 48707), }; //http://www.wowhead.com/search?q=antimagic_shell#specialization
            spells["pillar_of_frost"] = new SpecList { new SpecPair(WoWSpec.None, 51271), }; //http://www.wowhead.com/search?q=pillar_of_frost#specialization
            spells["plague_leech"] = new SpecList { new SpecPair(WoWSpec.None, 123693), }; //http://www.wowhead.com/search?q=plague_leech#specialization
            spells["empower_rune_weapon"] = new SpecList { new SpecPair(WoWSpec.None, 47568) }; //http://www.wowhead.com/search?q=empower_rune_weapon#specialization
            spells["touch_of_death"] = new SpecList { new SpecPair(WoWSpec.None, 115080), }; //http://www.wowhead.com/search?q=touch_of_death#specialization
            spells["keg_smash"] = new SpecList { new SpecPair(WoWSpec.None, 121253), }; //http://www.wowhead.com/search?q=keg_smash#specialization
            spells["expel_harm"] = new SpecList { new SpecPair(WoWSpec.None, 115072) }; //http://www.wowhead.com/search?q=expel_harm#specialization
            spells["fists_of_fury"] = new SpecList { new SpecPair(WoWSpec.None, 113656), }; //http://www.wowhead.com/search?q=fists_of_fury#specialization
            spells["hurricane_strike"] = new SpecList { new SpecPair(WoWSpec.None, 152175) }; //http://www.wowhead.com/search?q=hurricane_strike#specialization
            spells["serenity"] = new SpecList { new SpecPair(WoWSpec.None, 152173), }; //http://www.wowhead.com/search?q=serenity#specialization
            spells["chi_brew"] = new SpecList { new SpecPair(WoWSpec.None, 115399), }; //http://www.wowhead.com/search?q=chi_brew#specialization
            spells["vanish"] = new SpecList { new SpecPair(WoWSpec.None, 1856), }; //http://www.wowhead.com/search?q=vanish#specialization
            spells["death_from_above"] = new SpecList { new SpecPair(WoWSpec.None, 152150) }; //http://www.wowhead.com/search?q=death_from_above#specialization
            spells["killing_spree"] = new SpecList { new SpecPair(WoWSpec.None, 51690), new SpecPair(WoWSpec.None, 57841), new SpecPair(WoWSpec.None, 61851), new SpecPair(WoWSpec.None, 63879), }; //http://www.wowhead.com/search?q=killing_spree#specialization
            spells["shadow_reflection"] = new SpecList { new SpecPair(WoWSpec.None, 152151), }; //http://www.wowhead.com/search?q=shadow_reflection#specialization
            spells["honor_among_thieves"] = new SpecList { new SpecPair(WoWSpec.None, 51701), }; //http://www.wowhead.com/search?q=honor_among_thieves#specialization
            spells["shadow_dance"] = new SpecList { new SpecPair(WoWSpec.None, 51713), }; //http://www.wowhead.com/search?q=shadow_dance#specialization
            spells["kill_command"] = new SpecList { new SpecPair(WoWSpec.None, 34026), }; //http://www.wowhead.com/search?q=kill_command#specialization
            spells["chimaera_shot"] = new SpecList { new SpecPair(WoWSpec.None, 53209), }; //http://www.wowhead.com/search?q=chimaera_shot#specialization
            spells["fervor"] = new SpecList { new SpecPair(WoWSpec.None, 806), }; //http://www.wowhead.com/search?q=fervor#specialization
            spells["reckoning"] = new SpecList { new SpecPair(WoWSpec.None, 62124), }; //http://www.wowhead.com/search?q=reckoning#specialization
            spells["taunt"] = new SpecList { new SpecPair(WoWSpec.None, 355), }; //http://www.wowhead.com/search?q=taunt#specialization
            spells["aspect_of_the_cheetah"] = new SpecList { new SpecPair(WoWSpec.None, 5118) }; //http://www.wowhead.com/search?q=dark_intent#specialization
            spells["blood_fury"] = new SpecList { new SpecPair(WoWSpec.None, 20572), new SpecPair(WoWSpec.None, 24571), new SpecPair(WoWSpec.None, 33697), new SpecPair(WoWSpec.None, 33702), }; //http://www.wowhead.com/search?q=blood_fury#specialization
            spells["berserking"] = new SpecList { new SpecPair(WoWSpec.None, 26297), new SpecPair(WoWSpec.None, 59621), }; //http://www.wowhead.com/search?q=berserking#specialization
            spells["arcane_torrent"] = new SpecList { new SpecPair(WoWSpec.None, 25046), new SpecPair(WoWSpec.None, 28730), new SpecPair(WoWSpec.None, 50613), new SpecPair(WoWSpec.None, 69179), new SpecPair(WoWSpec.None, 80483), new SpecPair(WoWSpec.None, 129597), new SpecPair(WoWSpec.None, 155145), }; //http://www.wowhead.com/search?q=arcane_torrent#specialization
            spells["force_of_nature"] = new SpecList { new SpecPair(WoWSpec.DruidBalance, 33831), new SpecPair(WoWSpec.DruidRestoration, 102693), new SpecPair(WoWSpec.DruidFeral, 102703), new SpecPair(WoWSpec.DruidGuardian, 102706), }; //http://www.wowhead.com/search?q=force_of_nature#specialization

            spells["displacer_beast"] = new SpecList { new SpecPair(WoWSpec.None, 102280), }; //http://www.wowhead.com/search?q=displacer_beast#specialization
            spells["dash"] = new SpecList { new SpecPair(WoWSpec.None, 1850), new SpecPair(WoWSpec.PetCunning, 61684), }; //http://www.wowhead.com/search?q=dash#specialization
            spells["rake"] = new SpecList { new SpecPair(WoWSpec.None, 1822), }; //http://www.wowhead.com/search?q=rake#specialization

            spells["shadowmeld"] = new SpecList { new SpecPair(WoWSpec.None, 58984), }; //http://www.wowhead.com/search?q=shadowmeld#specialization
            spells["ferocious_bite"] = new SpecList { new SpecPair(WoWSpec.None, 22568), }; //http://www.wowhead.com/search?q=ferocious_bite#specialization
            spells["healing_touch"] = new SpecList { new SpecPair(WoWSpec.None, 5185) }; //http://www.wowhead.com/search?q=healing_touch#specialization
            spells["savage_roar"] = new SpecList { new SpecPair(WoWSpec.None, 52610), }; //http://www.wowhead.com/search?q=savage_roar#specialization

            spells["thrash_cat"] = new SpecList { new SpecPair(WoWSpec.None, 106830), };
            spells["savage_defense"] = new SpecList { new SpecPair(WoWSpec.None, 62606), }; //http://www.wowhead.com/search?q=savage_defense#specialization
            spells["barkskin"] = new SpecList { new SpecPair(WoWSpec.None, 22812) }; //http://www.wowhead.com/search?q=barkskin#specialization
            spells["bristling_fur"] = new SpecList { new SpecPair(WoWSpec.None, 155835), }; //http://www.wowhead.com/search?q=bristling_fur#specialization
            spells["maul"] = new SpecList { new SpecPair(WoWSpec.None, 6807), }; //http://www.wowhead.com/search?q=maul#specialization
            spells["frenzied_regeneration"] = new SpecList { new SpecPair(WoWSpec.None, 22842), }; //http://www.wowhead.com/search?q=frenzied_regeneration#specialization
            spells["renewal"] = new SpecList { new SpecPair(WoWSpec.None, 108238), }; //http://www.wowhead.com/search?q=renewal#specialization
            spells["rejuvenation"] = new SpecList { new SpecPair(WoWSpec.None, 774), }; //http://www.wowhead.com/search?q=rejuvenation#specialization
            spells["pulverize"] = new SpecList { new SpecPair(WoWSpec.None, 80313), }; //http://www.wowhead.com/search?q=pulverize#specialization
            spells["lacerate"] = new SpecList { new SpecPair(WoWSpec.None, 33745), }; //http://www.wowhead.com/search?q=lacerate#specialization
            spells["thrash_bear"] = new SpecList { new SpecPair(WoWSpec.None, 77758), };
            spells["explosive_trap"] = new SpecList { new SpecPair(WoWSpec.None, 13813), }; //http://www.wowhead.com/search?q=explosive_trap#specialization
            spells["focus_fire"] = new SpecList { new SpecPair(WoWSpec.None, 82692), }; //http://www.wowhead.com/search?q=focus_fire#specialization
            spells["multishot"] = new SpecList { new SpecPair(WoWSpec.None, 2643), }; //http://www.wowhead.com/search?q=multishot#specialization
            spells["kill_shot"] = new SpecList { new SpecPair(WoWSpec.None, 53351), }; //http://www.wowhead.com/search?q=kill_shot#specialization
            spells["focusing_shot"] = new SpecList { new SpecPair(WoWSpec.HunterSurvival, 152245), new SpecPair(WoWSpec.HunterBeastMastery, 152245), new SpecPair(WoWSpec.HunterMarksmanship, 163485), }; //http://www.wowhead.com/search?q=focusing_shot#specialization
            spells["cobra_shot"] = new SpecList { new SpecPair(WoWSpec.None, 77767), }; //http://www.wowhead.com/search?q=cobra_shot#specialization
            spells["powershot"] = new SpecList { new SpecPair(WoWSpec.None, 109259), }; //http://www.wowhead.com/search?q=powershot#specialization
            spells["arcane_shot"] = new SpecList { new SpecPair(WoWSpec.None, 3044), }; //http://www.wowhead.com/search?q=arcane_shot#specialization
            spells["dire_beast"] = new SpecList { new SpecPair(WoWSpec.None, 120679), }; //http://www.wowhead.com/search?q=dire_beast#specialization
            spells["steady_shot"] = new SpecList { new SpecPair(WoWSpec.None, 56641), }; //http://www.wowhead.com/search?q=steady_shot#specialization
            spells["black_arrow"] = new SpecList { new SpecPair(WoWSpec.None, 3674), }; //http://www.wowhead.com/search?q=black_arrow#specialization
            spells["blink"] = new SpecList { new SpecPair(WoWSpec.None, 1953) }; //http://www.wowhead.com/search?q=blink#specialization
            spells["blazing_speed"] = new SpecList { new SpecPair(WoWSpec.None, 108843), }; //http://www.wowhead.com/search?q=blazing_speed#specialization
            spells["time_warp"] = new SpecList { new SpecPair(WoWSpec.None, 80353), }; //http://www.wowhead.com/search?q=time_warp#specialization
            spells["ice_floes"] = new SpecList { new SpecPair(WoWSpec.None, 108839), }; //http://www.wowhead.com/search?q=ice_floes#specialization
            spells["rune_of_power"] = new SpecList { new SpecPair(WoWSpec.None, 116011), }; //http://www.wowhead.com/search?q=rune_of_power#specialization
            spells["mirror_image"] = new SpecList { new SpecPair(WoWSpec.None, 55342), }; //http://www.wowhead.com/search?q=mirror_image#specialization
            spells["speed_of_light"] = new SpecList { new SpecPair(WoWSpec.None, 85499), }; //http://www.wowhead.com/search?q=speed_of_light#specialization
            spells["guardian_of_ancient_kings"] = new SpecList { new SpecPair(WoWSpec.None, 86659), }; //http://www.wowhead.com/search?q=guardian_of_ancient_kings#specialization
            spells["ardent_defender"] = new SpecList { new SpecPair(WoWSpec.None, 31850), }; //http://www.wowhead.com/search?q=ardent_defender#specialization
            spells["eternal_flame"] = new SpecList { new SpecPair(WoWSpec.None, 114163), }; //http://www.wowhead.com/search?q=eternal_flame#specialization
            spells["shield_of_the_righteous"] = new SpecList { new SpecPair(WoWSpec.None, 53600), }; //http://www.wowhead.com/search?q=shield_of_the_righteous#specialization
            spells["seal_of_insight"] = new SpecList { new SpecPair(WoWSpec.None, 20165), }; //http://www.wowhead.com/search?q=seal_of_insight#specialization
            spells["seal_of_righteousness"] = new SpecList { new SpecPair(WoWSpec.None, 20154), }; //http://www.wowhead.com/search?q=seal_of_righteousness#specialization
            spells["avengers_shield"] = new SpecList { new SpecPair(WoWSpec.None, 31935), }; //http://www.wowhead.com/search?q=avengers_shield#specialization
            spells["hammer_of_the_righteous"] = new SpecList { new SpecPair(WoWSpec.None, 53595) }; //http://www.wowhead.com/search?q=hammer_of_the_righteous#specialization

            spells["holy_wrath"] = new SpecList { new SpecPair(WoWSpec.None, 119072), }; //http://www.wowhead.com/search?q=holy_wrath#specialization
            spells["sacred_shield"] = new SpecList { new SpecPair(WoWSpec.PaladinProtection, 20925), new SpecPair(WoWSpec.PaladinRetribution, 20925), new SpecPair(WoWSpec.PaladinHoly, 148039), }; //http://www.wowhead.com/search?q=sacred_shield#specialization
            spells["lights_hammer"] = new SpecList { new SpecPair(WoWSpec.None, 114158), }; //http://www.wowhead.com/search?q=lights_hammer#specialization
            spells["holy_prism"] = new SpecList { new SpecPair(WoWSpec.None, 114165), }; //http://www.wowhead.com/search?q=holy_prism#specialization
            spells["consecration"] = new SpecList { new SpecPair(WoWSpec.None, 26573), }; //http://www.wowhead.com/search?q=consecration#specialization
            spells["execution_sentence"] = new SpecList { new SpecPair(WoWSpec.None, 114157), }; //http://www.wowhead.com/search?q=execution_sentence#specialization
            spells["flash_of_light"] = new SpecList { new SpecPair(WoWSpec.None, 19750), }; //http://www.wowhead.com/search?q=flash_of_light#specialization

            spells["holy_avenger"] = new SpecList { new SpecPair(WoWSpec.None, 105809), }; //http://www.wowhead.com/search?q=holy_avenger#specialization
            spells["mindbender"] = new SpecList { new SpecPair(WoWSpec.None, 123040), }; //http://www.wowhead.com/search?q=mindbender#specialization
            spells["shadowfiend"] = new SpecList { new SpecPair(WoWSpec.None, 34433), }; //http://www.wowhead.com/search?q=shadowfiend#specialization
            spells["power_infusion"] = new SpecList { new SpecPair(WoWSpec.None, 10060), }; //http://www.wowhead.com/search?q=power_infusion#specialization
            spells["shadow_word_pain"] = new SpecList { new SpecPair(WoWSpec.None, 589), }; //http://www.wowhead.com/search?q=shadow_word_pain#specialization
            spells["power_word_solace"] = new SpecList { new SpecPair(WoWSpec.None, 129250), }; //http://www.wowhead.com/search?q=power_word_solace#specialization
            spells["holy_fire"] = new SpecList { new SpecPair(WoWSpec.None, 14914), }; //http://www.wowhead.com/search?q=holy_fire#specialization
            spells["smite"] = new SpecList { new SpecPair(WoWSpec.None, 585), }; //http://www.wowhead.com/search?q=smite#specialization
            spells["penance_heal"] = new SpecList { new SpecPair(WoWSpec.None, 47540), };
            spells["flash_heal"] = new SpecList { new SpecPair(WoWSpec.None, 2061), }; //http://www.wowhead.com/search?q=flash_heal#specialization
            spells["heal"] = new SpecList { new SpecPair(WoWSpec.None, 2060), }; //http://www.wowhead.com/search?q=heal#specialization
            spells["mind_sear"] = new SpecList { new SpecPair(WoWSpec.None, 48045), }; //http://www.wowhead.com/search?q=mind_sear#specialization
            spells["holy_word"] = new SpecList { new SpecPair(WoWSpec.None, 88625), new SpecPair(WoWSpec.None, 88648), new SpecPair(WoWSpec.None, 88685), };
            spells["prayer_of_mending"] = new SpecList { new SpecPair(WoWSpec.None, 33076), }; //http://www.wowhead.com/search?q=prayer_of_mending#specialization
            spells["halo"] = new SpecList { new SpecPair(WoWSpec.PriestHoly, 120517), new SpecPair(WoWSpec.PriestDiscipline, 120517), new SpecPair(WoWSpec.PriestShadow, 120644), }; //http://www.wowhead.com/search?q=halo#specialization
            spells["cascade"] = new SpecList { new SpecPair(WoWSpec.PriestHoly, 121135), new SpecPair(WoWSpec.PriestDiscipline, 121135), new SpecPair(WoWSpec.PriestShadow, 127632), }; //http://www.wowhead.com/search?q=cascade#specialization
            spells["divine_star"] = new SpecList { new SpecPair(WoWSpec.PriestHoly, 110744), new SpecPair(WoWSpec.PriestDiscipline, 110744), new SpecPair(WoWSpec.PriestShadow, 122121) }; //http://www.wowhead.com/search?q=divine_star#specialization
            spells["renew"] = new SpecList { new SpecPair(WoWSpec.None, 139), }; //http://www.wowhead.com/search?q=renew#specialization
            spells["bloodlust"] = new SpecList { new SpecPair(WoWSpec.None, 2825), }; //http://www.wowhead.com/search?q=bloodlust#specialization
            spells["elemental_mastery"] = new SpecList { new SpecPair(WoWSpec.None, 16166), }; //http://www.wowhead.com/search?q=elemental_mastery#specialization
            spells["ancestral_swiftness"] = new SpecList { new SpecPair(WoWSpec.None, 16188), }; //http://www.wowhead.com/search?q=ancestral_swiftness#specialization
            spells["liquid_magma"] = new SpecList { new SpecPair(WoWSpec.None, 152255), }; //http://www.wowhead.com/search?q=liquid_magma#specialization
            spells["service_pet"] = new SpecList { new SpecPair(WoWSpec.None, 111898), };
            spells["summon_doomguard"] = new SpecList { new SpecPair(WoWSpec.None, 18540), }; //http://www.wowhead.com/search?q=summon_doomguard#specialization
            spells["summon_infernal"] = new SpecList { new SpecPair(WoWSpec.None, 1122), }; //http://www.wowhead.com/search?q=summon_infernal#specialization
            spells["kiljaedens_cunning"] = new SpecList { new SpecPair(WoWSpec.None, 137587), }; //http://www.wowhead.com/search?q=kiljaedens_cunning#specialization
            spells["soulburn"] = new SpecList { new SpecPair(WoWSpec.None, 74434), }; //http://www.wowhead.com/search?q=soulburn#specialization
            spells["seed_of_corruption"] = new SpecList { new SpecPair(WoWSpec.None, 27243), }; //http://www.wowhead.com/search?q=seed_of_corruption#specialization
            spells["haunt"] = new SpecList { new SpecPair(WoWSpec.None, 48181), }; //http://www.wowhead.com/search?q=haunt#specialization
            spells["agony"] = new SpecList { new SpecPair(WoWSpec.None, 980), }; //http://www.wowhead.com/search?q=agony#specialization
            spells["unstable_affliction"] = new SpecList { new SpecPair(WoWSpec.None, 30108), }; //http://www.wowhead.com/search?q=unstable_affliction#specialization
            spells["corruption"] = new SpecList { new SpecPair(WoWSpec.None, 172), }; //http://www.wowhead.com/search?q=corruption#specialization
            spells["life_tap"] = new SpecList { new SpecPair(WoWSpec.None, 1454), }; //http://www.wowhead.com/search?q=life_tap#specialization
            spells["drain_soul"] = new SpecList { new SpecPair(WoWSpec.None, 103103), }; //http://www.wowhead.com/search?q=drain_soul#specialization
            spells["wrathguard:mortal_cleave"] = new SpecList { new SpecPair(WoWSpec.None, 115625), };
            spells["immolation_aura"] = new SpecList { new SpecPair(WoWSpec.None, 104025) }; //http://www.wowhead.com/search?q=immolation_aura#specialization
            spells["doom"] = new SpecList { new SpecPair(WoWSpec.None, 603) }; //http://www.wowhead.com/search?q=doom#specialization

            spells["chaos_wave"] = new SpecList { new SpecPair(WoWSpec.None, 124915) }; //http://www.wowhead.com/search?q=chaos_wave#specialization
            spells["touch_of_chaos"] = new SpecList { new SpecPair(WoWSpec.None, 103964), }; //http://www.wowhead.com/search?q=touch_of_chaos#specialization
            spells["hellfire"] = new SpecList { new SpecPair(WoWSpec.None, 1949) }; //http://www.wowhead.com/search?q=hellfire#specialization
            spells["heroic_leap"] = new SpecList { new SpecPair(WoWSpec.None, 6544) }; //http://www.wowhead.com/search?q=heroic_leap#specialization
            spells["berserker_rage"] = new SpecList { new SpecPair(WoWSpec.None, 18499), }; //http://www.wowhead.com/search?q=berserker_rage#specialization
            spells["shield_charge"] = new SpecList { new SpecPair(WoWSpec.None, 156321), }; //http://www.wowhead.com/search?q=shield_charge#specialization
            spells["heroic_strike"] = new SpecList { new SpecPair(WoWSpec.None, 78), }; //http://www.wowhead.com/search?q=heroic_strike#specialization
            spells["conversion"] = new SpecList { new SpecPair(WoWSpec.None, 119975), }; //http://www.wowhead.com/search?q=conversion#specialization
            spells["lichborne"] = new SpecList { new SpecPair(WoWSpec.None, 49039), }; //http://www.wowhead.com/search?q=lichborne#specialization
            spells["death_strike"] = new SpecList { new SpecPair(WoWSpec.None, 49998), }; //http://www.wowhead.com/search?q=death_strike#specialization
            spells["army_of_the_dead"] = new SpecList { new SpecPair(WoWSpec.None, 42650), }; //http://www.wowhead.com/search?q=army_of_the_dead#specialization
            spells["bone_shield"] = new SpecList { new SpecPair(WoWSpec.None, 49222), }; //http://www.wowhead.com/search?q=bone_shield#specialization
            spells["vampiric_blood"] = new SpecList { new SpecPair(WoWSpec.None, 55233), }; //http://www.wowhead.com/search?q=vampiric_blood#specialization
            spells["icebound_fortitude"] = new SpecList { new SpecPair(WoWSpec.None, 48792), }; //http://www.wowhead.com/search?q=icebound_fortitude#specialization
            spells["rune_tap"] = new SpecList { new SpecPair(WoWSpec.None, 48982), }; //http://www.wowhead.com/search?q=rune_tap#specialization
            spells["dancing_rune_weapon"] = new SpecList { new SpecPair(WoWSpec.None, 49028), }; //http://www.wowhead.com/search?q=dancing_rune_weapon#specialization
            spells["death_pact"] = new SpecList { new SpecPair(WoWSpec.None, 48743), }; //http://www.wowhead.com/search?q=death_pact#specialization
            spells["death_coil"] = new SpecList { new SpecPair(WoWSpec.None, 47541), }; //http://www.wowhead.com/search?q=death_coil#specialization
            spells["plague_strike"] = new SpecList { new SpecPair(WoWSpec.None, 45462), }; //http://www.wowhead.com/search?q=plague_strike#specialization
            spells["icy_touch"] = new SpecList { new SpecPair(WoWSpec.None, 45477) }; //http://www.wowhead.com/search?q=icy_touch#specialization
            spells["death_and_decay"] = new SpecList { new SpecPair(WoWSpec.None, 43265), }; //http://www.wowhead.com/search?q=death_and_decay#specialization
            spells["blood_boil"] = new SpecList { new SpecPair(WoWSpec.None, 50842) }; //http://www.wowhead.com/search?q=blood_boil#specialization
            spells["deaths_advance"] = new SpecList { new SpecPair(WoWSpec.None, 96268), }; //http://www.wowhead.com/search?q=deaths_advance#specialization

            spells["gift_of_the_ox"] = new SpecList { new SpecPair(WoWSpec.None, 124502), }; //http://www.wowhead.com/search?q=gift_of_the_ox#specialization
            spells["diffuse_magic"] = new SpecList { new SpecPair(WoWSpec.None, 122783), }; //http://www.wowhead.com/search?q=diffuse_magic#specialization
            spells["dampen_harm"] = new SpecList { new SpecPair(WoWSpec.None, 122278), }; //http://www.wowhead.com/search?q=dampen_harm#specialization
            spells["fortifying_brew"] = new SpecList { new SpecPair(WoWSpec.None, 115203), }; //http://www.wowhead.com/search?q=fortifying_brew#specialization
            spells["elusive_brew"] = new SpecList { new SpecPair(WoWSpec.None, 115308), }; //http://www.wowhead.com/search?q=elusive_brew#specialization
            spells["invoke_xuen"] = new SpecList { new SpecPair(WoWSpec.None, 123904) };
            spells["invoke_xuen_the_white_tiger"] = new SpecList { new SpecPair(WoWSpec.None, 123904) };
            spells["storm_earth_and_fire"] = new SpecList { new SpecPair(WoWSpec.None, 137639) }; //http://www.wowhead.com/search?q=storm_earth_and_fire#specialization
            spells["tiger_palm"] = new SpecList { new SpecPair(WoWSpec.None, 100787), }; //http://www.wowhead.com/search?q=tiger_palm#specialization
            spells["tigereye_brew"] = new SpecList { new SpecPair(WoWSpec.None, 116740) }; //http://www.wowhead.com/search?q=tigereye_brew#specialization
            spells["rising_sun_kick"] = new SpecList { new SpecPair(WoWSpec.None, 107428), }; //http://www.wowhead.com/search?q=rising_sun_kick#specialization
            spells["energizing_brew"] = new SpecList { new SpecPair(WoWSpec.None, 115288), }; //http://www.wowhead.com/search?q=energizing_brew#specialization
            spells["preparation"] = new SpecList { new SpecPair(WoWSpec.None, 14185), }; //http://www.wowhead.com/search?q=preparation#specialization
            spells["rupture"] = new SpecList { new SpecPair(WoWSpec.None, 1943), }; //http://www.wowhead.com/search?q=rupture#specialization
            spells["mutilate"] = new SpecList { new SpecPair(WoWSpec.None, 1329), }; //http://www.wowhead.com/search?q=mutilate#specialization
            spells["slice_and_dice"] = new SpecList { new SpecPair(WoWSpec.None, 5171), }; //http://www.wowhead.com/search?q=slice_and_dice#specialization
            spells["marked_for_death"] = new SpecList { new SpecPair(WoWSpec.None, 137619) }; //http://www.wowhead.com/search?q=marked_for_death#specialization
            spells["crimson_tempest"] = new SpecList { new SpecPair(WoWSpec.None, 121411), }; //http://www.wowhead.com/search?q=crimson_tempest#specialization
            spells["fan_of_knives"] = new SpecList { new SpecPair(WoWSpec.None, 51723), }; //http://www.wowhead.com/search?q=fan_of_knives#specialization
            spells["vendetta"] = new SpecList { new SpecPair(WoWSpec.None, 79140), }; //http://www.wowhead.com/search?q=vendetta#specialization
            spells["envenom"] = new SpecList { new SpecPair(WoWSpec.None, 32645), }; //http://www.wowhead.com/search?q=envenom#specialization
            spells["dispatch"] = new SpecList { new SpecPair(WoWSpec.None, 111240), }; //http://www.wowhead.com/search?q=dispatch#specialization
            spells["blade_flurry"] = new SpecList { new SpecPair(WoWSpec.None, 13877), }; //http://www.wowhead.com/search?q=blade_flurry#specialization
            spells["premeditation"] = new SpecList { new SpecPair(WoWSpec.None, 14183), }; //http://www.wowhead.com/search?q=premeditation#specialization
            spells["garrote"] = new SpecList { new SpecPair(WoWSpec.None, 703), }; //http://www.wowhead.com/search?q=garrote#specialization
            spells["explosive_shot"] = new SpecList { new SpecPair(WoWSpec.None, 53301), }; //http://www.wowhead.com/search?q=explosive_shot#specialization
            spells["glaive_toss"] = new SpecList { new SpecPair(WoWSpec.None, 117050), };
            spells["disable"] = new SpecList{new SpecPair(WoWSpec.None, 116095)};




            spells["horn_of_winter"] = new SpecList { new SpecPair(WoWSpec.None, 57330), };
            spells["frost_presence"] = new SpecList { new SpecPair(WoWSpec.None, 48266), };
            spells["auto_attack"] = new SpecList { };
            spells["unholy_presence"] = new SpecList { new SpecPair(WoWSpec.None, 48265), };
            spells["raise_dead"] = new SpecList { new SpecPair(WoWSpec.None, 46584), };
            spells["summon_gargoyle"] = new SpecList { new SpecPair(WoWSpec.None, 49206), };
            spells["dark_transformation"] = new SpecList { new SpecPair(WoWSpec.None, 63560), };
            spells["moonkin_form"] = new SpecList { new SpecPair(WoWSpec.None, 24858), };
            spells["siegebreaker"] = new SpecList { new SpecPair(WoWSpec.None, 176289), };
            spells["auto_shot"] = new SpecList { };
            spells["arcane_brilliance"] = new SpecList { new SpecPair(WoWSpec.None, 1459), };
            spells["counterspell"] = new SpecList { new SpecPair(WoWSpec.None, 2139), };

            spells["water_elemental"] = new SpecList { new SpecPair(WoWSpec.None, 31687), };
            spells["comet_storm"] = new SpecList { new SpecPair(WoWSpec.None, 153595), };
            spells["spinning_crane_kick"] = new SpecList { new SpecPair(WoWSpec.None, 101546), };

            spells["rebuke"] = new SpecList { new SpecPair(WoWSpec.None, 96231), };
            spells["hammer_of_wrath"] = new SpecList { new SpecPair(WoWSpec.None, 24275), };
            spells["stealth"] = new SpecList { new SpecPair(WoWSpec.None, 1784), };
            spells["kick"] = new SpecList { new SpecPair(WoWSpec.None, 1766), };

            spells["ambush"] = new SpecList { new SpecPair(WoWSpec.None, 8676), };
            spells["revealing_strike"] = new SpecList { new SpecPair(WoWSpec.None, 84617), };
            spells["wind_shear"] = new SpecList { new SpecPair(WoWSpec.None, 57994), };
            spells["lava_beam"] = new SpecList { new SpecPair(WoWSpec.None, 114074), };
            spells["wind_shear"] = new SpecList { new SpecPair(WoWSpec.None, 57994), };
            spells["auto_attack"] = new SpecList { new SpecPair(WoWSpec.None, 117050), };
            spells["feral_spirit"] = new SpecList { new SpecPair(WoWSpec.None, 51533), };
            spells["Windstrike"] = new SpecList { new SpecPair(WoWSpec.None, 115356), };
            spells["mannoroths_fury"] = new SpecList { new SpecPair(WoWSpec.None, 108508), };
            spells["felguard:felstorm"] = new SpecList { new SpecPair(WoWSpec.None, 89751), };
            spells["wrathguard:wrathstorm"] = new SpecList { new SpecPair(WoWSpec.None, 115831), };
            spells["incinerate"] = new SpecList { new SpecPair(WoWSpec.None, 29722), };


            spells["blood_presence"] = new SpecList { new SpecPair(WoWSpec.None, 48263), };
            spells["cat_form"] = new SpecList { new SpecPair(WoWSpec.None, 768), };
            spells["prowl"] = new SpecList { new SpecPair(WoWSpec.None, 5215), };
            spells["wild_charge"] = new SpecList { new SpecPair(WoWSpec.None, 102401), };
            spells["skull_bash"] = new SpecList { new SpecPair(WoWSpec.None, 106839), };
            spells["swipe"] = new SpecList { new SpecPair(WoWSpec.None, 106785), };
            spells["bear_form"] = new SpecList { new SpecPair(WoWSpec.None, 5487), };
            spells["cenarion_ward"] = new SpecList { new SpecPair(WoWSpec.None, 102351), };
            spells["heart_of_the_wild"] = new SpecList { new SpecPair(WoWSpec.DruidRestoration, 108294), new SpecPair(WoWSpec.DruidGuardian, 108293), new SpecPair(WoWSpec.DruidFeral, 108292), new SpecPair(WoWSpec.DruidBalance, 108291), };
            spells["natures_vigil"] = new SpecList { new SpecPair(WoWSpec.None, 124974), };
            spells["mangle"] = new SpecList { new SpecPair(WoWSpec.None, 33917), };
            spells["penance"] = new SpecList { new SpecPair(WoWSpec.None, 47540), };
            spells["clarity_of_will"] = new SpecList { new SpecPair(WoWSpec.None, 152118), };
            spells["chakra_chastise"] = new SpecList { new SpecPair(WoWSpec.None, 81209), };
            spells["chakra_serenity"] = new SpecList { new SpecPair(WoWSpec.None, 81208), };
            spells["lightwell"] = new SpecList { new SpecPair(WoWSpec.None, 126135), };
            spells["circle_of_healing"] = new SpecList { new SpecPair(WoWSpec.None, 34861), };

            spells["backstab"] = new SpecList { new SpecPair(WoWSpec.None, 53), };
            spells["heroic_throw"] = new SpecList { new SpecPair(WoWSpec.None, 57755), };
            spells["sweeping_strikes"] = new SpecList { new SpecPair(WoWSpec.None, 12328), };
            spells["revenge"] = new SpecList { new SpecPair(WoWSpec.None, 6572), };


            buffs["moonwell_chalice"] = new SpecList { new SpecPair(WoWSpec.None, 100612) };

            buffs["celestial_alignment"] = new SpecList { new SpecPair(WoWSpec.None, 112071) }; //http://www.wowhead.com/search?q=celestial_alignment#specialization
            buffs["lunar_empowerment"] = new SpecList { new SpecPair(WoWSpec.None, 164547), }; //http://www.wowhead.com/search?q=lunar_empowerment#specialization
            buffs["solar_empowerment"] = new SpecList { new SpecPair(WoWSpec.None, 164545), }; //http://www.wowhead.com/search?q=solar_empowerment#specialization
            buffs["solar_peak"] = new SpecList { new SpecPair(WoWSpec.None, 171744), }; //http://www.wowhead.com/search?q=solar_peak#specialization
            buffs["lunar_peak"] = new SpecList { new SpecPair(WoWSpec.None, 171743), }; //http://www.wowhead.com/search?q=lunar_peak#specialization
            buffs["starfall"] = new SpecList { new SpecPair(WoWSpec.None, 48505) }; //http://www.wowhead.com/search?q=starfall#specialization
            buffs["displacer_beast"] = new SpecList { new SpecPair(WoWSpec.None, 102280) }; //http://www.wowhead.com/search?q=displacer_beast#specialization
            buffs["wild_charge_movement"] = new SpecList { new SpecPair(WoWSpec.None, 102401) };
            buffs["prowl"] = new SpecList { new SpecPair(WoWSpec.None, 5215) }; //http://www.wowhead.com/search?q=prowl#specialization
            buffs["shadowmeld"] = new SpecList { new SpecPair(WoWSpec.None, 58984), }; //http://www.wowhead.com/search?q=shadowmeld#specialization
            buffs["king_of_the_jungle"] = new SpecList { new SpecPair(WoWSpec.None, 102543), };
            buffs["berserk"] = new SpecList { new SpecPair(WoWSpec.DruidGuardian, 50334), new SpecPair(WoWSpec.DruidFeral, 106951) }; //http://www.wowhead.com/search?q=berserk#specialization
            buffs["omen_of_clarity"] = new SpecList { new SpecPair(WoWSpec.DruidFeral, 135700), new SpecPair(WoWSpec.DruidRestoration, 16870), }; //http://www.wowhead.com/search?q=omen_of_clarity#specialization
            buffs["bloodtalons"] = new SpecList { new SpecPair(WoWSpec.None, 145152) }; //http://www.wowhead.com/search?q=bloodtalons#specialization
            buffs["predatory_swiftness"] = new SpecList { new SpecPair(WoWSpec.None, 69369), }; //http://www.wowhead.com/search?q=predatory_swiftness#specialization
            buffs["savage_roar"] = new SpecList { new SpecPair(WoWSpec.None, 52610) }; //http://www.wowhead.com/search?q=savage_roar#specialization
            buffs["barkskin"] = new SpecList { new SpecPair(WoWSpec.None, 22812), }; //http://www.wowhead.com/search?q=barkskin#specialization
            buffs["bristling_fur"] = new SpecList { new SpecPair(WoWSpec.None, 155835), }; //http://www.wowhead.com/search?q=bristling_fur#specialization
            buffs["savage_defense"] = new SpecList { new SpecPair(WoWSpec.None, 132402), }; //http://www.wowhead.com/search?q=savage_defense#specialization
            buffs["tooth_and_claw"] = new SpecList { new SpecPair(WoWSpec.None, 135288) }; //http://www.wowhead.com/search?q=tooth_and_claw#specialization
            buffs["pulverize"] = new SpecList { new SpecPair(WoWSpec.None, 80313) }; //http://www.wowhead.com/search?q=pulverize#specialization
            buffs["heart_of_the_wild"] = new SpecList { new SpecPair(WoWSpec.DruidBalance, 108291), new SpecPair(WoWSpec.DruidFeral, 108292), new SpecPair(WoWSpec.DruidGuardian, 108293), new SpecPair(WoWSpec.DruidRestoration, 108294), }; //http://www.wowhead.com/search?q=heart_of_the_wild#specialization
            buffs["dream_of_cenarius"] = new SpecList { new SpecPair(WoWSpec.DruidBalance, 108373), new SpecPair(WoWSpec.DruidFeral, 158497), new SpecPair(WoWSpec.DruidGuardian, 158501), new SpecPair(WoWSpec.DruidRestoration, 158504), new SpecPair(WoWSpec.None, 172176), }; //http://www.wowhead.com/search?q=dream_of_cenarius#specialization
            buffs["bestial_wrath"] = new SpecList { new SpecPair(WoWSpec.None, 19574), }; //http://www.wowhead.com/search?q=bestial_wrath#specialization
            buffs["bloodlust"] = new SpecList { new SpecPair(WoWSpec.None, 2825), }; //http://www.wowhead.com/search?q=bloodlust#specialization
            buffs["focus_fire"] = new SpecList { new SpecPair(WoWSpec.None, 82692), }; //http://www.wowhead.com/search?q=focus_fire#specialization
            //{ new SpecPair(WoWSpec.None, 58234), new SpecPair(WoWSpec.None, 121818), new SpecPair(WoWSpec.None, 130201), new SpecPair(WoWSpec.None, 178875), }; //http://www.wowhead.com/search?q=stampede#specialization
            buffs["beast_cleave"] = new SpecList { new SpecPair(WoWSpec.None, 118455), }; //http://www.wowhead.com/search?q=beast_cleave#specialization

            buffs["thrill_of_the_hunt"] = new SpecList { new SpecPair(WoWSpec.None, 34720), }; //http://www.wowhead.com/search?q=thrill_of_the_hunt#specialization
            buffs["rapid_fire"] = new SpecList { new SpecPair(WoWSpec.None, 3045), }; //http://www.wowhead.com/search?q=rapid_fire#specialization
            buffs["careful_aim"] = new SpecList { new SpecPair(WoWSpec.None, 34483), }; //http://www.wowhead.com/search?q=careful_aim#specialization


            buffs["steady_focus"] = new SpecList { new SpecPair(WoWSpec.None, 177668), }; //http://www.wowhead.com/search?q=steady_focus#specialization

            buffs["lock_and_load"] = new SpecList { new SpecPair(WoWSpec.None, 168980), }; //http://www.wowhead.com/search?q=lock_and_load#specialization

            buffs["ice_floes"] = new SpecList { new SpecPair(WoWSpec.None, 108839), }; //http://www.wowhead.com/search?q=ice_floes#specialization


            buffs["frenzy"] = new SpecList { new SpecPair(WoWSpec.None, 19615) };
            buffs["presence_of_mind"] = new SpecList { new SpecPair(WoWSpec.None, 12043), }; //http://www.wowhead.com/search?q=presence_of_mind#specialization
            buffs["arcane_power"] = new SpecList { new SpecPair(WoWSpec.None, 12042) }; //http://www.wowhead.com/search?q=arcane_power#specialization
            buffs["arcane_charge"] = new SpecList { new SpecPair(WoWSpec.None, 36032), }; //http://www.wowhead.com/search?q=arcane_charge#specialization
            buffs["arcane_missiles"] = new SpecList { new SpecPair(WoWSpec.None, 79683) }; //http://www.wowhead.com/search?q=arcane_missiles#specialization
            buffs["arcane_instability"] = new SpecList { new SpecPair(WoWSpec.None, 166872), }; //http://www.wowhead.com/search?q=arcane_instability#specialization
            buffs["heating_up"] = new SpecList { new SpecPair(WoWSpec.None, 48107), }; //http://www.wowhead.com/search?q=heating_up#specialization
            buffs["pyroblast"] = new SpecList { new SpecPair(WoWSpec.None, 48108), }; //http://www.wowhead.com/search?q=pyroblast#specialization
            buffs["pyromaniac"] = new SpecList { new SpecPair(WoWSpec.None, 166868), }; //http://www.wowhead.com/search?q=pyromaniac#specialization
            buffs["incanters_flow"] = new SpecList { new SpecPair(WoWSpec.None, 116267), }; //http://www.wowhead.com/search?q=incanters_flow#specialization
            buffs["potent_flames"] = new SpecList { new SpecPair(WoWSpec.None, 145254), }; //http://www.wowhead.com/search?q=potent_flames#specialization
            buffs["water_jet"] = new SpecList { new SpecPair(WoWSpec.None, 135029), }; //http://www.wowhead.com/search?q=water_jet#specialization
            buffs["fingers_of_frost"] = new SpecList { new SpecPair(WoWSpec.None, 44544) }; //http://www.wowhead.com/search?q=fingers_of_frost#specialization
            buffs["brain_freeze"] = new SpecList { new SpecPair(WoWSpec.None, 57761), }; //http://www.wowhead.com/search?q=brain_freeze#specialization
            buffs["icy_veins"] = new SpecList { new SpecPair(WoWSpec.None, 12472) }; //http://www.wowhead.com/search?q=icy_veins#specialization
            buffs["frost_bomb"] = new SpecList { new SpecPair(WoWSpec.None, 112948), }; //http://www.wowhead.com/search?q=frost_bomb#specialization
            buffs["ice_shard"] = new SpecList { new SpecPair(WoWSpec.None, 166869), }; //http://www.wowhead.com/search?q=ice_shard#specialization
            buffs["seraphim"] = new SpecList { new SpecPair(WoWSpec.None, 152262), }; //http://www.wowhead.com/search?q=seraphim#specialization
            buffs["shield_of_the_righteous"] = new SpecList { new SpecPair(WoWSpec.None, 132403), }; //http://www.wowhead.com/search?q=shield_of_the_righteous#specialization
            buffs["divine_protection"] = new SpecList { new SpecPair(WoWSpec.None, 498), }; //http://www.wowhead.com/search?q=divine_protection#specialization
            buffs["guardian_of_ancient_kings"] = new SpecList { new SpecPair(WoWSpec.None, 86659), }; //http://www.wowhead.com/search?q=guardian_of_ancient_kings#specialization
            buffs["ardent_defender"] = new SpecList { new SpecPair(WoWSpec.None, 31850), }; //http://www.wowhead.com/search?q=ardent_defender#specialization
            buffs["holy_avenger"] = new SpecList { new SpecPair(WoWSpec.None, 105809), }; //http://www.wowhead.com/search?q=holy_avenger#specialization
            buffs["eternal_flame"] = new SpecList { new SpecPair(WoWSpec.None, 156322), }; //http://www.wowhead.com/search?q=eternal_flame#specialization
            buffs["bastion_of_glory"] = new SpecList { new SpecPair(WoWSpec.None, 114637), }; //http://www.wowhead.com/search?q=bastion_of_glory#specialization
            buffs["divine_purpose"] = new SpecList { new SpecPair(WoWSpec.None, 90174), }; //http://www.wowhead.com/search?q=divine_purpose#specialization
            buffs["bastion_of_power"] = new SpecList { new SpecPair(WoWSpec.None, 144569), }; //http://www.wowhead.com/search?q=bastion_of_power#specialization
            buffs["uthers_insight"] = new SpecList { new SpecPair(WoWSpec.None, 156988), }; //http://www.wowhead.com/search?q=uthers_insight#specialization
            buffs["liadrins_righteousness"] = new SpecList { new SpecPair(WoWSpec.None, 156989), }; //http://www.wowhead.com/search?q=liadrins_righteousness#specialization
            buffs["grand_crusader"] = new SpecList { new SpecPair(WoWSpec.None, 85416), new SpecPair(WoWSpec.None, 98057), }; //http://www.wowhead.com/search?q=grand_crusader#specialization

            buffs["selfless_healer"] = new SpecList { new SpecPair(WoWSpec.None, 85804), new SpecPair(WoWSpec.None, 114250), }; //http://www.wowhead.com/search?q=selfless_healer#specialization
            buffs["avenging_wrath"] = new SpecList { new SpecPair(WoWSpec.None, 31842), }; //http://www.wowhead.com/search?q=avenging_wrath#specialization
            buffs["divine_crusader"] = new SpecList { new SpecPair(WoWSpec.None, 144595), }; //http://www.wowhead.com/search?q=divine_crusader#specialization
            buffs["final_verdict"] = new SpecList { new SpecPair(WoWSpec.None, 157048), }; //http://www.wowhead.com/search?q=final_verdict#specialization
            buffs["maraads_truth"] = new SpecList { new SpecPair(WoWSpec.None, 156990), }; //http://www.wowhead.com/search?q=maraads_truth#specialization
            buffs["blazing_contempt"] = new SpecList { new SpecPair(WoWSpec.None, 166831), }; //http://www.wowhead.com/search?q=blazing_contempt#specialization
            buffs["borrowed_time"] = new SpecList { new SpecPair(WoWSpec.None, 59889), }; //http://www.wowhead.com/search?q=borrowed_time#specialization
            buffs["surge_of_light"] = new SpecList { new SpecPair(WoWSpec.None, 114255), }; //http://www.wowhead.com/search?q=surge_of_light#specialization
            buffs["power_infusion"] = new SpecList { new SpecPair(WoWSpec.None, 10060), }; //http://www.wowhead.com/search?q=power_infusion#specialization
            buffs["divine_insight"] = new SpecList { new SpecPair(WoWSpec.None, 109175), }; //http://www.wowhead.com/search?q=divine_insight#specialization
            buffs["serendipity"] = new SpecList { new SpecPair(WoWSpec.None, 63735), }; //http://www.wowhead.com/search?q=serendipity#specialization
            buffs["shadowform"] = new SpecList { new SpecPair(WoWSpec.None, 15473), }; //http://www.wowhead.com/search?q=shadowform#specialization
            buffs["mental_instinct"] = new SpecList { new SpecPair(WoWSpec.None, 167254), }; //http://www.wowhead.com/search?q=mental_instinct#specialization
            buffs["insanity"] = new SpecList { new SpecPair(WoWSpec.None, 132573) }; //http://www.wowhead.com/search?q=insanity#specialization
            buffs["surge_of_darkness"] = new SpecList { new SpecPair(WoWSpec.None, 87160), }; //http://www.wowhead.com/search?q=surge_of_darkness#specialization
            buffs["lightning_shield"] = new SpecList { new SpecPair(WoWSpec.None, 324), }; //http://www.wowhead.com/search?q=lightning_shield#specialization
            buffs["ascendance"] = new SpecList { new SpecPair(WoWSpec.ShamanElemental, 114050), new SpecPair(WoWSpec.ShamanEnhancement, 114051), new SpecPair(WoWSpec.ShamanRestoration, 114052), }; //http://www.wowhead.com/search?q=ascendance#specialization
            buffs["elemental_mastery"] = new SpecList { new SpecPair(WoWSpec.None, 16166), }; //http://www.wowhead.com/search?q=elemental_mastery#specialization
            buffs["lava_surge"] = new SpecList { new SpecPair(WoWSpec.None, 77756), }; //http://www.wowhead.com/search?q=lava_surge#specialization
            buffs["liquid_magma"] = new SpecList { new SpecPair(WoWSpec.None, 152255), }; //http://www.wowhead.com/search?q=liquid_magma#specialization
            buffs["enhanced_chain_lightning"] = new SpecList { new SpecPair(WoWSpec.None, 157766), }; //http://www.wowhead.com/search?q=enhanced_chain_lightning#specialization
            buffs["maelstrom_weapon"] = new SpecList { new SpecPair(WoWSpec.None, 53817), }; //http://www.wowhead.com/search?q=maelstrom_weapon#specialization
            buffs["ancestral_swiftness"] = new SpecList { new SpecPair(WoWSpec.None, 16188), }; //http://www.wowhead.com/search?q=ancestral_swiftness#specialization
            buffs["elemental_fusion"] = new SpecList { new SpecPair(WoWSpec.None, 157174), }; //http://www.wowhead.com/search?q=elemental_fusion#specialization
            buffs["unleash_flame"] = new SpecList { new SpecPair(WoWSpec.None, 165462), }; //http://www.wowhead.com/search?q=unleash_flame#specialization
            buffs["stormstrike"] = new SpecList { new SpecPair(WoWSpec.None, 17364), }; //http://www.wowhead.com/search?q=stormstrike#specialization
            buffs["grimoire_of_sacrifice"] = new SpecList { new SpecPair(WoWSpec.None, 108503), }; //http://www.wowhead.com/search?q=grimoire_of_sacrifice#specialization
            buffs["dark_soul"] = new SpecList { new SpecPair(WoWSpec.WarlockDemonology, 113861), new SpecPair(WoWSpec.WarlockDestruction, 113858), new SpecPair(WoWSpec.WarlockAffliction, 113860), }; //http://www.wowhead.com/search?q=dark_soul#specialization
            buffs["soulburn"] = new SpecList { new SpecPair(WoWSpec.None, 74434), }; //http://www.wowhead.com/search?q=soulburn#specialization
            buffs["haunting_spirits"] = new SpecList { new SpecPair(WoWSpec.None, 157698), }; //http://www.wowhead.com/search?q=haunting_spirits#specialization
            buffs["demonbolt"] = new SpecList { new SpecPair(WoWSpec.None, 157695), }; //http://www.wowhead.com/search?q=demonbolt#specialization
            buffs["metamorphosis"] = new SpecList { new SpecPair(WoWSpec.None, 103958) }; //http://www.wowhead.com/search?q=metamorphosis#specialization
            buffs["immolation_aura"] = new SpecList { new SpecPair(WoWSpec.None, 104025) }; //http://www.wowhead.com/search?q=immolation_aura#specialization
            buffs["molten_core"] = new SpecList { new SpecPair(WoWSpec.None, 122355), }; //http://www.wowhead.com/search?q=molten_core#specialization
            buffs["fire_and_brimstone"] = new SpecList { new SpecPair(WoWSpec.None, 108683), }; //http://www.wowhead.com/search?q=fire_and_brimstone#specialization
            buffs["havoc"] = new SpecList { new SpecPair(WoWSpec.None, 80240), }; //http://www.wowhead.com/search?q=havoc#specialization
            buffs["mannoroths_fury"] = new SpecList { new SpecPair(WoWSpec.None, 108508), }; //http://www.wowhead.com/search?q=mannoroths_fury#specialization
            buffs["backdraft"] = new SpecList { new SpecPair(WoWSpec.None, 117828), }; //http://www.wowhead.com/search?q=backdraft#specialization
            buffs["ember_master"] = new SpecList { new SpecPair(WoWSpec.None, 145164), }; //http://www.wowhead.com/search?q=ember_master#specialization

            buffs["bloodbath"] = new SpecList { new SpecPair(WoWSpec.None, 12292), }; //http://www.wowhead.com/search?q=bloodbath#specialization

            buffs["charge"] = new SpecList { new SpecPair(WoWSpec.None, 100) }; //http://www.wowhead.com/search?q=charge#specialization
            buffs["recklessness"] = new SpecList { new SpecPair(WoWSpec.None, 1719), }; //http://www.wowhead.com/search?q=recklessness#specialization
            buffs["sudden_death"] = new SpecList { new SpecPair(WoWSpec.None, 52437), }; //http://www.wowhead.com/search?q=sudden_death#specialization

            buffs["avatar"] = new SpecList { new SpecPair(WoWSpec.None, 107574), }; //http://www.wowhead.com/search?q=avatar#specialization

            buffs["enrage"] = new SpecList { new SpecPair(WoWSpec.None, 12880) }; //http://www.wowhead.com/search?q=enrage#specialization
            buffs["raging_blow"] = new SpecList { new SpecPair(WoWSpec.None, 131116), }; //http://www.wowhead.com/search?q=raging_blow#specialization
            buffs["bloodsurge"] = new SpecList { new SpecPair(WoWSpec.None, 46916), }; //http://www.wowhead.com/search?q=bloodsurge#specialization
            buffs["meat_cleaver"] = new SpecList { new SpecPair(WoWSpec.None, 85739), }; //http://www.wowhead.com/search?q=meat_cleaver#specialization
            buffs["shield_charge"] = new SpecList { new SpecPair(WoWSpec.None, 169667), }; //http://www.wowhead.com/search?q=shield_charge#specialization
            buffs["unyielding_strikes"] = new SpecList { new SpecPair(WoWSpec.None, 169686), }; //http://www.wowhead.com/search?q=unyielding_strikes#specialization
            buffs["ultimatum"] = new SpecList { new SpecPair(WoWSpec.None, 122510), }; //http://www.wowhead.com/search?q=ultimatum#specialization
            buffs["demoralizing_shout"] = new SpecList { new SpecPair(WoWSpec.None, 1160), }; //http://www.wowhead.com/search?q=demoralizing_shout#specialization
            buffs["mend_pet"] = new SpecList { new SpecPair(WoWSpec.None, 136) }; //http://www.wowhead.com/search?q=dark_intent#specialization
            buffs["aspect_of_the_cheetah"] = new SpecList { new SpecPair(WoWSpec.None, 5118) }; //http://www.wowhead.com/search?q=dark_intent#specialization
            buffs["shield_wall"] = new SpecList { new SpecPair(WoWSpec.None, 871), }; //http://www.wowhead.com/search?q=shield_wall#specialization
            buffs["last_stand"] = new SpecList { new SpecPair(WoWSpec.None, 12975), }; //http://www.wowhead.com/search?q=last_stand#specialization
            buffs["enraged_regeneration"] = new SpecList { new SpecPair(WoWSpec.None, 55694), }; //http://www.wowhead.com/search?q=enraged_regeneration#specialization
            buffs["shield_block"] = new SpecList { new SpecPair(WoWSpec.None, 132404), }; //http://www.wowhead.com/search?q=shield_block#specialization
            buffs["shield_barrier"] = new SpecList { new SpecPair(WoWSpec.None, 112048), }; //http://www.wowhead.com/search?q=shield_barrier#specialization
            buffs["blood_shield"] = new SpecList { new SpecPair(WoWSpec.None, 77535), }; //http://www.wowhead.com/search?q=blood_shield#specialization
            buffs["conversion"] = new SpecList { new SpecPair(WoWSpec.None, 119975), }; //http://www.wowhead.com/search?q=conversion#specialization
            buffs["bone_shield"] = new SpecList { new SpecPair(WoWSpec.None, 49222), }; //http://www.wowhead.com/search?q=bone_shield#specialization
            buffs["dancing_rune_weapon"] = new SpecList { new SpecPair(WoWSpec.None, 81256), }; //http://www.wowhead.com/search?q=dancing_rune_weapon#specialization
            buffs["icebound_fortitude"] = new SpecList { new SpecPair(WoWSpec.None, 48792), }; //http://www.wowhead.com/search?q=icebound_fortitude#specialization
            buffs["vampiric_blood"] = new SpecList { new SpecPair(WoWSpec.None, 55233), }; //http://www.wowhead.com/search?q=vampiric_blood#specialization
            buffs["army_of_the_dead"] = new SpecList { new SpecPair(WoWSpec.None, 42650), }; //http://www.wowhead.com/search?q=army_of_the_dead#specialization
            buffs["crimson_scourge"] = new SpecList { new SpecPair(WoWSpec.None, 81141), }; //http://www.wowhead.com/search?q=crimson_scourge#specialization
            buffs["blood_charge"] = new SpecList { new SpecPair(WoWSpec.None, 114851), }; //http://www.wowhead.com/search?q=blood_charge#specialization
            buffs["pillar_of_frost"] = new SpecList { new SpecPair(WoWSpec.None, 51271), }; //http://www.wowhead.com/search?q=pillar_of_frost#specialization
            buffs["killing_machine"] = new SpecList { new SpecPair(WoWSpec.None, 51124), }; //http://www.wowhead.com/search?q=killing_machine#specialization
            buffs["antimagic_shell"] = new SpecList { new SpecPair(WoWSpec.None, 48707), }; //http://www.wowhead.com/search?q=antimagic_shell#specialization
            buffs["rime"] = new SpecList { new SpecPair(WoWSpec.None, 59057), }; //http://www.wowhead.com/search?q=rime#specialization
            buffs["dark_transformation"] = new SpecList { new SpecPair(WoWSpec.None, 63560), }; //http://www.wowhead.com/search?q=dark_transformation#specialization
            buffs["shadow_infusion"] = new SpecList { new SpecPair(WoWSpec.None, 91342), }; //http://www.wowhead.com/search?q=shadow_infusion#specialization
            buffs["sudden_doom"] = new SpecList { new SpecPair(WoWSpec.None, 81340), }; //http://www.wowhead.com/search?q=sudden_doom#specialization

            buffs["elusive_brew_stacks"] = new SpecList { new SpecPair(WoWSpec.None, 128939) };
            buffs["shuffle"] = new SpecList { new SpecPair(WoWSpec.None, 115307), }; //http://www.wowhead.com/search?q=shuffle#specialization
            buffs["gift_of_the_ox"] = new SpecList { new SpecPair(WoWSpec.None, 124502) }; //http://www.wowhead.com/search?q=gift_of_the_ox#specialization
            buffs["fortifying_brew"] = new SpecList { new SpecPair(WoWSpec.None, 120954) }; //http://www.wowhead.com/search?q=fortifying_brew#specialization
            buffs["elusive_brew_activated"] = new SpecList { new SpecPair(WoWSpec.None, 115308) };
            buffs["dampen_harm"] = new SpecList { new SpecPair(WoWSpec.None, 122278), }; //http://www.wowhead.com/search?q=dampen_harm#specialization
            buffs["diffuse_magic"] = new SpecList { new SpecPair(WoWSpec.None, 122783), }; //http://www.wowhead.com/search?q=diffuse_magic#specialization
            buffs["serenity"] = new SpecList { new SpecPair(WoWSpec.None, 152173), }; //http://www.wowhead.com/search?q=serenity#specialization

            buffs["tigereye_brew_use"] = new SpecList { new SpecPair(WoWSpec.None, 116740) };
            buffs["tigereye_brew"] = new SpecList { new SpecPair(WoWSpec.None, 125195), }; //http://www.wowhead.com/search?q=tigereye_brew#specialization
            buffs["tiger_power"] = new SpecList { new SpecPair(WoWSpec.None, 125359), }; //http://www.wowhead.com/search?q=tiger_power#specialization

            buffs["energizing_brew"] = new SpecList { new SpecPair(WoWSpec.None, 115288), }; //http://www.wowhead.com/search?q=energizing_brew#specialization
            buffs["combo_breaker_bok"] = new SpecList { new SpecPair(WoWSpec.None, 116768) };
            buffs["combo_breaker_tp"] = new SpecList { new SpecPair(WoWSpec.None, 118864) };
            buffs["combo_breaker_ce"] = new SpecList { new SpecPair(WoWSpec.None, 159407) };

            buffs["vanish"] = new SpecList { new SpecPair(WoWSpec.None, 1856), }; //http://www.wowhead.com/search?q=vanish#specialization
            buffs["stealth"] = new SpecList { new SpecPair(WoWSpec.None, 1784), }; //http://www.wowhead.com/search?q=stealth#specialization
            buffs["slice_and_dice"] = new SpecList { new SpecPair(WoWSpec.None, 5171), }; //http://www.wowhead.com/search?q=slice_and_dice#specialization
            buffs["shadow_reflection"] = new SpecList { new SpecPair(WoWSpec.None, 152151), }; //http://www.wowhead.com/search?q=shadow_reflection#specialization
            buffs["envenom"] = new SpecList { new SpecPair(WoWSpec.None, 32645), }; //http://www.wowhead.com/search?q=envenom#specialization
            buffs["adrenaline_rush"] = new SpecList { new SpecPair(WoWSpec.None, 13750), }; //http://www.wowhead.com/search?q=adrenaline_rush#specialization
            buffs["blade_flurry"] = new SpecList { new SpecPair(WoWSpec.None, 13877), }; //http://www.wowhead.com/search?q=blade_flurry#specialization
            buffs["deep_insight"] = new SpecList { new SpecPair(WoWSpec.None, 84747), }; //http://www.wowhead.com/search?q=deep_insight#specialization
            buffs["killing_spree"] = new SpecList { new SpecPair(WoWSpec.None, 51690), }; //http://www.wowhead.com/search?q=killing_spree#specialization
            buffs["shadow_dance"] = new SpecList { new SpecPair(WoWSpec.None, 51713), }; //http://www.wowhead.com/search?q=shadow_dance#specialization
            buffs["subterfuge"] = new SpecList { new SpecPair(WoWSpec.None, 115192), }; //http://www.wowhead.com/search?q=subterfuge#specialization

            buffs["master_of_subtlety"] = new SpecList { new SpecPair(WoWSpec.None, 31665) }; //http://www.wowhead.com/search?q=master_of_subtlety#specialization


            buffs["archmages_greater_incandescence_agi"] = new SpecList { new SpecPair(WoWSpec.None, 177172) };
            buffs["archmages_incandescence_agi"] = new SpecList { new SpecPair(WoWSpec.None, 177161) };
            buffs["archmages_greater_incandescence_int"] = new SpecList { new SpecPair(WoWSpec.None, 177176) };
            buffs["archmages_incandescence_int"] = new SpecList { new SpecPair(WoWSpec.None, 177159) };
            buffs["archmages_greater_incandescence_str"] = new SpecList { new SpecPair(WoWSpec.None, 177175) };
            buffs["archmages_incandescence_str"] = new SpecList { new SpecPair(WoWSpec.None, 177160) };


            debuffs["rake"] = new SpecList { new SpecPair(WoWSpec.None, 155722) }; //http://www.wowhead.com/search?q=rake#specialization
            debuffs["rip"] = new SpecList { new SpecPair(WoWSpec.None, 1079), }; //http://www.wowhead.com/search?q=rip#specialization
            debuffs["lacerate"] = new SpecList { new SpecPair(WoWSpec.None, 33745), }; //http://www.wowhead.com/search?q=lacerate#specialization
            debuffs["serpent_sting"] = new SpecList { new SpecPair(WoWSpec.None, 118253), }; //http://www.wowhead.com/search?q=serpent_sting#specialization
            debuffs["explosive_trap"] = new SpecList { new SpecPair(WoWSpec.None, 13812), }; //http://www.wowhead.com/search?q=explosive_trap#specialization
            
            debuffs["nether_tempest"] = new SpecList { new SpecPair(WoWSpec.None, 114923) }; //http://www.wowhead.com/search?q=nether_tempest#specialization
            debuffs["combustion"] = new SpecList { new SpecPair(WoWSpec.None, 83853), }; //http://www.wowhead.com/search?q=combustion#specialization
            debuffs["ignite"] = new SpecList { new SpecPair(WoWSpec.None, 12654) }; //http://www.wowhead.com/search?q=ignite#specialization
            debuffs["living_bomb"] = new SpecList { new SpecPair(WoWSpec.None, 44457), }; //http://www.wowhead.com/search?q=living_bomb#specialization
            debuffs["pyroblast"] = new SpecList { new SpecPair(WoWSpec.None, 11366) }; //http://www.wowhead.com/search?q=pyroblast#specialization
            debuffs["water_jet"] = new SpecList { new SpecPair(WoWSpec.None, 135029), }; //http://www.wowhead.com/search?q=water_jet#specialization
            debuffs["frozen_orb"] = new SpecList { new SpecPair(WoWSpec.None, 84721), }; //http://www.wowhead.com/search?q=frozen_orb#specialization
            debuffs["frost_bomb"] = new SpecList { new SpecPair(WoWSpec.None, 112948) }; //http://www.wowhead.com/search?q=frost_bomb#specialization
            debuffs["sacred_shield"] = new SpecList { new SpecPair(WoWSpec.PaladinProtection, 20925), new SpecPair(WoWSpec.PaladinRetribution, 20925), new SpecPair(WoWSpec.PaladinHoly, 148039) }; //http://www.wowhead.com/search?q=sacred_shield#specialization

            debuffs["power_word_solace"] = new SpecList { new SpecPair(WoWSpec.None, 129250), }; //http://www.wowhead.com/search?q=power_word_solace#specialization
            debuffs["holy_fire"] = new SpecList { new SpecPair(WoWSpec.None, 14914), }; //http://www.wowhead.com/search?q=holy_fire#specialization
            debuffs["devouring_plague_dot"] = new SpecList { new SpecPair(WoWSpec.None, 158831), };
            debuffs["devouring_plague_tick"] = new SpecList { new SpecPair(WoWSpec.None, 158831), };
            debuffs["void_entropy"] = new SpecList { new SpecPair(WoWSpec.None, 155361), }; //http://www.wowhead.com/search?q=void_entropy#specialization
            debuffs["vampiric_touch"] = new SpecList { new SpecPair(WoWSpec.None, 34914), }; //http://www.wowhead.com/search?q=vampiric_touch#specialization
            debuffs["shadow_word_pain"] = new SpecList { new SpecPair(WoWSpec.None, 589), }; //http://www.wowhead.com/search?q=shadow_word_pain#specialization
            debuffs["devouring_plague"] = new SpecList { new SpecPair(WoWSpec.None, 158831), };//http://www.wowhead.com/search?q=devouring_plague#specialization
            debuffs["flame_shock"] = new SpecList { new SpecPair(WoWSpec.None, 8050), }; //http://www.wowhead.com/search?q=flame_shock#specialization
            debuffs["stormstrike"] = new SpecList { new SpecPair(WoWSpec.None, 17364), }; //http://www.wowhead.com/search?q=stormstrike#specialization
            debuffs["corruption"] = new SpecList { new SpecPair(WoWSpec.None, 146739), }; //http://www.wowhead.com/search?q=corruption#specialization
            debuffs["seed_of_corruption"] = new SpecList { new SpecPair(WoWSpec.None, 27243), }; //http://www.wowhead.com/search?q=seed_of_corruption#specialization
            debuffs["haunt"] = new SpecList { new SpecPair(WoWSpec.None, 48181), }; //http://www.wowhead.com/search?q=haunt#specialization
            debuffs["shadowflame"] = new SpecList { new SpecPair(WoWSpec.None, 159878), }; //http://www.wowhead.com/search?q=shadowflame#specialization
            debuffs["doom"] = new SpecList { new SpecPair(WoWSpec.None, 603), }; //http://www.wowhead.com/search?q=doom#specialization
            debuffs["immolate"] = new SpecList { new SpecPair(WoWSpec.None, 108686), }; //http://www.wowhead.com/search?q=immolate#specialization
            debuffs["colossus_smash"] = new SpecList { new SpecPair(WoWSpec.None, 167105), }; //http://www.wowhead.com/search?q=colossus_smash#specialization
            debuffs["rend"] = new SpecList { new SpecPair(WoWSpec.None, 772), }; //http://www.wowhead.com/search?q=rend#specialization
            debuffs["charge"] = new SpecList { new SpecPair(WoWSpec.None, 105771), }; //http://www.wowhead.com/search?q=charge#specialization
            debuffs["deep_wounds"] = new SpecList { new SpecPair(WoWSpec.None, 115767), }; //http://www.wowhead.com/search?q=deep_wounds#specialization
            debuffs["demoralizing_shout"] = new SpecList { new SpecPair(WoWSpec.None, 1160), }; //http://www.wowhead.com/search?q=demoralizing_shout#specialization
            debuffs["blood_plague"] = new SpecList { new SpecPair(WoWSpec.None, 55078), }; //http://www.wowhead.com/search?q=blood_plague#specialization
            debuffs["necrotic_plague"] = new SpecList { new SpecPair(WoWSpec.None, 155159), }; //http://www.wowhead.com/search?q=necrotic_plague#specialization
            debuffs["frost_fever"] = new SpecList { new SpecPair(WoWSpec.None, 55095), }; //http://www.wowhead.com/search?q=frost_fever#specialization
            debuffs["breath_of_sindragosa"] = new SpecList { new SpecPair(WoWSpec.None, 152279), }; //http://www.wowhead.com/search?q=breath_of_sindragosa#specialization
            debuffs["zen_sphere"] = new SpecList { new SpecPair(WoWSpec.None, 124081), }; //http://www.wowhead.com/search?q=zen_sphere#specialization
            debuffs["breath_of_fire"] = new SpecList { new SpecPair(WoWSpec.None, 115181) }; //http://www.wowhead.com/search?q=breath_of_fire#specialization
            debuffs["storm_earth_and_fire_target"] = new SpecList { new SpecPair(WoWSpec.None, 138130) };
            debuffs["rising_sun_kick"] = new SpecList { new SpecPair(WoWSpec.None, 130320), }; //http://www.wowhead.com/search?q=rising_sun_kick#specialization
            debuffs["vendetta"] = new SpecList { new SpecPair(WoWSpec.None, 79140), }; //http://www.wowhead.com/search?q=vendetta#specialization
            debuffs["deadly_poison_dot"] = new SpecList { new SpecPair(WoWSpec.None, 2823), };
            debuffs["revealing_strike"] = new SpecList { new SpecPair(WoWSpec.None, 84617), }; //http://www.wowhead.com/search?q=revealing_strike#specialization
            debuffs["find_weakness"] = new SpecList { new SpecPair(WoWSpec.None, 91021), }; //http://www.wowhead.com/search?q=find_weakness#specialization
            debuffs["hemorrhage"] = new SpecList { new SpecPair(WoWSpec.None, 16511), }; //http://www.wowhead.com/search?q=hemorrhage#specialization
            debuffs["garrote"] = new SpecList { new SpecPair(WoWSpec.None, 703), }; //http://www.wowhead.com/search?q=garrote#specialization
            debuffs["rupture"] = new SpecList { new SpecPair(WoWSpec.None, 1943), }; //http://www.wowhead.com/search?q=rupture#specialization
        }

        static SimcNames()
        {


        }
    }
}
