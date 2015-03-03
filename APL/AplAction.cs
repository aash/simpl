using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using JetBrains.Annotations;
using Styx;
using Styx.Common;

namespace Simcraft
{
    public class AplAction : AplExpr
    {
        public String code;

        public enum ActionType
        {
            cast,
            flask,
            food,
            stance,
            call_action_list,
            use_item,
            potion,
            snapshot_stats,
            run_action_list
        }

        List<String> Comments = new List<string>();

        public AplAction(String code, List<String> comments)
        {
            Comments.AddRange(comments);
            comments.Clear();
            this.code = code;
            if (action_reg.IsMatch(code))
            {
                var m = action_reg.Match(code);
                apl = m.Groups[1].ToString().Replace("+=", "").Replace("=", "");
                var text = m.Groups[2].ToString().Replace("/", "");

                if (apl.Length <= 0) apl = "default";
                apl = apl.Replace(".", "");
                if (text.Contains(","))
                {
                    var splits = text.Split(',');
                    action = splits[0];
                    Enum.TryParse(action, out type);
                    //Console.WriteLine(type);
                    int i = 1;
                    for (i = 1; i < splits.Count(); i++)
                    {
                        _params.Add(ActionOption.new_option(splits[i]));
                    }

                }
                else
                {
                    action = text;
                    Enum.TryParse(action, out type);
                }

                //Console.WriteLine(apl + " - " + action);
            }
        }

        private String params_to_string()
        {
            String ac = "";
            foreach (var par in _params)
            {
                ac += " " + par.ToString();
            }
            return ac;
        }
        private String comments(String indent)
        {
            String ac = "";
            foreach (var par in Comments)
            {
                ac += indent+"//" + par.ToString() + Environment.NewLine;
            }
            return ac;
        }

       

        public override string ToString()
        {
            return /*code+Environment.NewLine+*/comments("") + apl + " -> " + type + " / " + action + " - " + params_to_string();
        }

        Regex action_reg = new Regex(@"actions(\.*[a-z_ ]*=|\.*[a-z_ ]*\+=)(.+)");
        public ActionType type;
        public String apl;
        public String action;



        public class ActionOption
        {

            public static ActionOption new_option(String text)
            {

                text = text.Trim();
                if (text.StartsWith("if="))             return option(aot.If, text.Substring(text.IndexOf("=")+1));
                if (text.StartsWith("interrupt_if=")) return option(aot.InterruptIf, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("early_chain_if=")) return option(aot.EarlyChainIf, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("interrupt=")) return option(aot.Interrupt, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("chain=")) return option(aot.Chain, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("cycle_targets=")) return option(aot.CycleTargets, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("cycle_players=")) return option(aot.CyclePlayers, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("max_cycle_targets=")) return option(aot.MaxCycleTargets, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("moving=")) return option(aot.Moving, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("sync=")) return option(aot.Sync, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("wait_on_ready=")) return option(aot.WaitOnReady, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("target=")) return option(aot.Target, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("label=")) return option(aot.Label, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("precombat=")) return option(aot.Precombat, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("line_cd=")) return option(aot.LineCd, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("action_skill=")) return option(aot.ActionSkill, text.Substring(text.IndexOf("=") + 1));

                if (text.StartsWith("slot=")) return option(aot.Slot, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("five_stacks=")) return option(aot.FiveStacks, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("damage=")) return option(aot.Damage, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("type=")) return option(aot.Type, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("name=")) return option(aot.Name, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("choose=")) return option(aot.Choose, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("sec=")) return option(aot.Seconds, text.Substring(text.IndexOf("=") + 1));
                if (text.StartsWith("ammo_type=")) return option(aot.AmmoType, text.Substring(text.IndexOf("=") + 1));
                
                Logging.Write("I dont recognize the option "+text);
                return new ActionOption();
            } 

            private static ActionOption option(aot type, String content)
            {
                var a = new ActionOption();

                a.content = content;
                a.type = type;
                return a;
            }

            public aot type = aot.Unknown;
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

        public enum aot
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
            AmmoType
        }

        public List<String> ResolveTrinketExpression(String expr, Dictionary<String, EquippedItem> items)
        {
            List<String> Results = new List<string>();

            trinket_proc_expr_e pexprtype = trinket_proc_expr_e.PROC_ENABLED;
            trinket_proc_type_e ptype = trinket_proc_type_e.PROC_STAT;
            //stat_e stat = stat_e.STAT_NONE;

            var splits = expr.Split('.');
            var slots = new List<int>();
            int ptype_idx = 1, stat_idx = 2, expr_idx = 3;

            int outval = 0;

            foreach (var sp in splits)
            {
                //Console.WriteLine("sp: "+sp);
            }

            if (int.TryParse(splits[1], out outval))
            {
                if (outval != 1 && outval != 2)
                {
                    throw new TrinketExpressionException("There are only two trinket slots: " + outval);
                }
                slots.Add(outval);
                ptype_idx++;
                stat_idx++;
                expr_idx++;
            }
            else
            {
                slots.Add(1);
                slots.Add(2);
            }

            if (splits[ptype_idx].StartsWith("has_"))
                pexprtype = trinket_proc_expr_e.PROC_EXISTS;

            if (splits[ptype_idx].Contains("cooldown"))
            {
                ptype = trinket_proc_type_e.PROC_COOLDOWN;
                expr_idx--;
            }

            if (splits[ptype_idx].Contains("stacking_"))
            {
                splits[ptype_idx].Replace("stacking_", "");
            }

            if (ptype != trinket_proc_type_e.PROC_COOLDOWN)
            {
                //stat = stat_e.STAT_ALL;
            }

            string[] bools = new[] {"up","down","react"};

            //Proc Expression
            if (pexprtype == trinket_proc_expr_e.PROC_ENABLED && ptype != trinket_proc_type_e.PROC_COOLDOWN)
            {
                if (outval == 1 || outval == 2)
                {
                    var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));

                    if (_items == default(EquippedItem))
                        throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                    if (!SimcraftImpl.dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("simc.buff." + SimcraftImpl.dbc.ItemProcs[_items.id] + "." + splits[splits.Count() - 1]);
                }
                else
                {
                    Results.Add("simc.buff.anytrinket." + splits[splits.Count() - 1]);

                }
            }
            //Cooldown expression
            else if (pexprtype == trinket_proc_expr_e.PROC_ENABLED && ptype == trinket_proc_type_e.PROC_COOLDOWN)
            {
                if (outval == 1 || outval == 2)
                {
                    var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));


                    if (_items == default(EquippedItem))
                        throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                    if (!SimcraftImpl.dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("simc.trinket"+outval+".cooldown." + splits[splits.Count() - 1]);
                }
                else
                {
                    if (splits[splits.Count() - 1].Equals("up") || splits[splits.Count() - 1].Equals("down"))
                        Results.Add("false");
                    else
                        Results.Add("new SimcraftImpl.MagicValueType(0)");
                }
            }
            else if (pexprtype == trinket_proc_expr_e.PROC_EXISTS)
            {
                if (ptype != trinket_proc_type_e.PROC_COOLDOWN)
                {
                    Results.Add("false");
                    return Results;
                }
                else
                {

                    var isbool = bools.Contains(splits[splits.Count() - 1]);
                    if (outval == 1 || outval == 2)
                    {
                        var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));


                        if (_items == default(EquippedItem))
                            throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                        if (SimcraftImpl.dbc.ItemProcs.ContainsKey(_items.id))
                            Results.Add("true");
                        else Results.Add("false");
                    }
                    else
                    {
                        //Weird expressions ^^
                        if (isbool)
                            Results.Add("false");
                        else
                            Results.Add("new SimcraftImpl.MagicValueType(0)");
                    }                 
                }
            }
            return Results;
            
        }

        public bool Understood { get; set; }

        public bool has(aot type)
        {
            return _params.Any(ret => ret.type == type);
        }

        public String get(aot type)
        {
            return _params.First(ret => ret.type == type).content;
        }

        public static String run_action_list_t(AplAction action)
        {

            var s = "simc.actions" + (action.apl == "default" ? "" : "[\"" + action.apl + "\"]") +
                                      " += simc.CallActionList(\"" + action.get(aot.Name) +
                                      "\"" +
                                      (action.has(aot.If)
                                          ? ", _if => (" + action.condition_string +
                                          (action.has(aot.Moving) ? " && simc.moving" : "") +
                                            ")"
                                          : "") +
                                      (action.has(aot.Target) ? ",simc.Target" + action.get(aot.Target) : "") +
                                      ",\"" + action.condition_string.Replace("\"", "\\\"") + "\"" +
                                      ");";
            return s;
        }

        public String condition_string;

        Regex filter_equals = new Regex("([^><!])=([^><!])");
        Regex filter_in = new Regex("\\.in([><=])");
        Regex filter_trinket = new Regex("(trinket\\.[a-z_.12]+)");
        Regex swing = new Regex("(swing\\.[a-z_.12]+)");

        Regex tokenizer = new Regex("([a-z][\\._a-z0-9]+)");

        public String fix_condition_string(String condition, List<APLHotkey> hotkeys)
        {
            condition = has(aot.If) ? get(aot.If).Replace("|", "||").Replace("&", "&&") : "";
            condition = condition.ToLower();

            condition = filter_in.Replace(condition, "._in$1");
            condition = filter_equals.Replace(condition, "$1==$2");
            
            condition = tokenizer.Replace(condition, "simc.$1");

            //condition_string = condition_string.Replace("simc.false", "false");

            foreach (var hk in hotkeys)
            {
                condition = condition.Replace("simc." + hk.Name, "simc.hkvar(\"" + hk.Name + "\")");
            }

            condition = condition.Replace("simc.false", "false");
            condition = condition.Replace("simc.true", "true");
            condition = condition.Replace("simc.trinket", "trinket");
            condition = condition.Replace("react==", "stack==");
            //condition_string = condition_string.Replace("moving", "simc.moving");  
            return condition;
        }

        public String ToCode(Dictionary<String, EquippedItem> items, String indent, List<APLHotkey> hotkeys)
        {
            condition_string = fix_condition_string(condition_string, hotkeys);       

            var __code = "";

            String prefix = indent;

            if (swing.IsMatch(condition_string))
            {
                Comments.Add("Dont use swing timers man ...");
                prefix += "//";
            }

            Understood = true;

            switch (this.type)
            {
                case ActionType.cast:

                    if (action.Equals("start_pyro_chain"))
                    {
                        __code = comments(indent) + "" + prefix + "simc.actions[\"" + (apl == "default" ? "" : "" + apl) + "\"] += simc.StartPyroChain(" +
                                 (has(aot.If)
                                     ? "_if => (" + condition_string +
                                       (has(aot.LineCd) ? " && simc.line_cd(" + get(aot.LineCd) + ")" : "") +
                                       (has(aot.Sync) ? " && simc.sync(\"" + get(aot.Sync) + "\")" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("stop_pyro_chain"))
                    {
                        __code = comments(indent) + "" + prefix + "simc.actions[\"" + (apl == "default" ? "" : "" + apl) + "\"] += simc.StopPyroChain(" +
                                 (has(aot.If)
                                     ? "_if => (" + condition_string +
                                       (has(aot.LineCd) ? " && simc.line_cd(" + get(aot.LineCd) + ")" : "") +
                                       (has(aot.Sync) ? " && simc.sync(\"" + get(aot.Sync) + "\")" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("wait"))
                    {
                        __code = comments(indent) + "" + prefix + "simc.actions[\"" + (apl == "default" ? "" : "" + apl) + "\"] += simc.Wait(" +
                                 (has(aot.If)
                                     ? "_if => (" + condition_string +
                                       (has(aot.LineCd) ? " && simc.line_cd(" + get(aot.LineCd) + ")" : "") +
                                       (has(aot.Sync) ? " && simc.sync(\"" + get(aot.Sync) + "\")" : "")+
                                       (has(aot.Moving) ? " && simc.moving" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("auto_attack") || action.Equals("auto_shot") || action.Equals("summon_pet") || condition_string.Contains("aura."))
                    {
                        Comments.Add("Nonsupported spells and apis");
                        prefix += "//";
                    }
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                        (has(aot.CycleTargets) ? " += simc.CycleTargets(\"" : (has(aot.Moving) ? " += simc.MovingCast(\"" : " += simc.Cast(\"")) + action + "\"" + ", _if => (" +
                             (has(aot.If) 
                                 ? condition_string +
                                   (has(aot.LineCd) ? " && simc.line_cd(" + get(aot.LineCd) + ")" : "") +
                                   (has(aot.Sync) ? " && simc.sync(\"" + get(aot.Sync) + "\")" : "")+
                                   (has(aot.Moving) ? " && simc.moving" : "")+
                                   (has(aot.FiveStacks) ? " && simc.buff.frenzy.stack == 5" : "")+
                                   (has(aot.Target) ? " && simc.Target" + get(aot.Target) + " != null" : "") 
                                   + ")"
                                 : "true"+
                                   (has(aot.LineCd) ? " && simc.line_cd(" + get(aot.LineCd) + ")" : "") +
                                   (has(aot.Sync) ? " && simc.sync(\"" + get(aot.Sync) + "\")" : "") +
                                   (has(aot.Moving) ? " && simc.moving" : "") +
                                   (has(aot.FiveStacks) ? " && simc.buff.frenzy.stack == 5" : "") +
                                   (has(aot.Target) ? " && simc.Target" + get(aot.Target) + " != null" : "") 
                                   + ")") +
                             (has(aot.Target) ? ",simc.Target" + get(aot.Target) : "") +
                             ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
                    
                    if (!SimcraftImpl.DBHasClassSpell(action))
                    {
                        //SimcraftImpl.Write("Couldnt find ClassSpell: "+action+" trying to find Spell");
                        if (!SimcraftImpl.DBHasSpell(action))
                        {
                            SimcraftImpl.Write("Invalid Spell: " + action + ", skipping.");
                            __code = "//" + __code;
                        }
                    }

                    break;
                case ActionType.run_action_list:
                case ActionType.call_action_list:
                    __code = comments(indent) + "" + prefix + run_action_list_t(this);
                    break;
                case ActionType.potion:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                             " += simc.UsePotion(\"" + get(aot.Name) + "\"" +
                             (has(aot.If)
                                 ? ", _if => (" + condition_string +
                                 (has(aot.Moving) ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (has(aot.Target) ? ",simc.Target" + get(aot.Target) : "") +
                             ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
                    break;
                case ActionType.use_item:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl +"\"]") + " += " +
                             (has(aot.Slot) ? "simc.UseItem(" + items[get(aot.Slot)].id : "") +
                             (has(aot.Name)
                                 ? "simc.UseItem(" + items.First(ret => ret.Value.name.Equals(get(aot.Name))).Value.id + ""
                                 : "")
                             + (has(aot.If)
                                 ? ", _if => (" + condition_string +
                                 (has(aot.Moving) ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (has(aot.Target) ? ",simc.Target" + get(aot.Target) : "") +
                             ",\"" + condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
                    break;
                default:
                    __code = prefix + "//" + code;
                    break;
            }


            var retcode = "";


            //Logging.Write("code: "+__code);

            foreach (Match m in filter_trinket.Matches(condition_string))
            {
                //foreach (var s in  ResolveTrinketExpression(m.Groups[1].ToString(), items))
                //{
                __code = __code.Replace(m.Groups[1].ToString(), ResolveTrinketExpression(m.Groups[1].ToString(), items)[0]) + Environment.NewLine;
                //}
                //retcode += __code;
            }

            if (retcode.Length == 0)
                retcode = __code;

            //Logging.Write("code2: " + retcode);

            return retcode;        
        }

        List<ActionOption> _params = new List<ActionOption>();
        String Name { get; set; }
    }
}
