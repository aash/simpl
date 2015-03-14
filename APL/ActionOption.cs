using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Common;

namespace Simcraft.APL
{



    public class ActionOption
    {
        public static ActionOption new_option(String text)
        {

            text = text.Trim();
            if (text.StartsWith("if=")) return option(ActionOptionType.If, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("interrupt_if=")) return option(ActionOptionType.InterruptIf, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("early_chain_if=")) return option(ActionOptionType.EarlyChainIf, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("interrupt=")) return option(ActionOptionType.Interrupt, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("chain=")) return option(ActionOptionType.Chain, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("cycle_targets=")) return option(ActionOptionType.CycleTargets, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("cycle_players=")) return option(ActionOptionType.CyclePlayers, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("max_cycle_targets=")) return option(ActionOptionType.MaxCycleTargets, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("moving=")) return option(ActionOptionType.Moving, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("sync=")) return option(ActionOptionType.Sync, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("wait_on_ready=")) return option(ActionOptionType.WaitOnReady, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("target=")) return option(ActionOptionType.Target, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("label=")) return option(ActionOptionType.Label, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("precombat=")) return option(ActionOptionType.Precombat, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("line_cd=")) return option(ActionOptionType.LineCd, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("action_skill=")) return option(ActionOptionType.ActionSkill, text.Substring(text.IndexOf("=") + 1));

            if (text.StartsWith("slot=")) return option(ActionOptionType.Slot, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("five_stacks=")) return option(ActionOptionType.FiveStacks, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("damage=")) return option(ActionOptionType.Damage, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("type=")) return option(ActionOptionType.Type, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("name=")) return option(ActionOptionType.Name, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("choose=")) return option(ActionOptionType.Choose, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("sec=")) return option(ActionOptionType.Seconds, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("ammo_type=")) return option(ActionOptionType.AmmoType, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("lethal=")) return option(ActionOptionType.LethalPoison, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("for_next=")) return option(ActionOptionType.ForNext, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("max_energy=")) return option(ActionOptionType.MaxEnergy, text.Substring(text.IndexOf("=") + 1));
            if (text.StartsWith("extra_amount=")) return option(ActionOptionType.ExtraAmmount, text.Substring(text.IndexOf("=") + 1));
            if (text.Length > 1)
            SimcraftImpl.Write("I dont recognize the option " + text);



            return new ActionOption();
        }

        private static ActionOption option(ActionOptionType type, String content)
        {
            var a = new ActionOption();

            a.content = content;
            a.type = type;
            return a;
        }

        public ActionOptionType type = ActionOptionType.Unknown;
        public String content;

        public override string ToString()
        {
            return type + ": " + content;
        }

        public String get_content<T>()
        {
            return content.ToString();
        }
    }


    public enum ActionType
    {
        flask,
        food,
        stance,
        call_action_list,
        use_item,
        potion,
        snapshot_stats,
        run_action_list,
        choose_target,
        apply_poison,      
        pool_resource,
        wait,
        wait_until_ready,
        cancel_metamorphosis,
        cancel_buff,
        summon_pet,
        spell_dot,
        start_pyro_chain,
        stop_pyro_chain,
        none,
        mana_potion
    }

    public enum ActionOptionType
    {
        If,
        InterruptIf,
        EarlyChainIf,
        Interrupt,
        Chain,
        CycleTargets,
        CyclePlayers,
        MaxCycleTargets,
        Moving,
        Sync,
        WaitOnReady,
        Precombat,
        LineCd,
        ActionSkill,
        Target,
        Name,
        Type,
        Slot,
        Damage,
        FiveStacks,
        Unknown,
        Label,
        Choose,
        Seconds,
        AmmoType,
        LethalPoison,
        ForNext,
        MaxEnergy,
        ExtraAmmount
    }

}
