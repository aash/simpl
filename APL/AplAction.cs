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

namespace Simcraft.APL
{
    public class AplAction : AplExpr
    {

        List<String> Comments = new List<string>();
        private String _condition_string;
        private String _fullExpression;
        private String csCode;
        public bool Understood { get; set; }

        Regex action_reg = new Regex(@"actions(\.*[a-z_ ]*=|\.*[a-z_ ]*\+=)(.+)");
        Regex filter_equals = new Regex("([^><!])=([^><!])");
        Regex filter_in = new Regex("\\.in([><=])");
        Regex filter_trinket = new Regex("(trinket\\.[a-z_.12]+)");
        Regex swing = new Regex("(swing\\.[a-z_.12]+)");
        Regex tokenizer = new Regex("([a-z][\\._a-z0-9]+)");

        public ActionType type = ActionType.none;
        public String apl;
        public ActionPrioriyList myList;
        public String action;
        List<ActionOption> _params = new List<ActionOption>();
        String Name { get; set; }


        private bool ParseExpression()
        {
           if (action_reg.IsMatch(_fullExpression))
            {
                var m = action_reg.Match(_fullExpression);
                apl = m.Groups[1].ToString().Replace("+=", "").Replace("=", "");
                var text = m.Groups[2].ToString().Replace("/", "");

                if (apl.Length <= 0) apl = "default";
                apl = apl.Replace(".", "");
                if (text.Contains(","))
                {
                    var splits = text.Split(',');
                    action = splits[0];
                    if (!Enum.TryParse(action, out type)) type = ActionType.none;
                    
                    int i = 1;
                    for (i = 1; i < splits.Count(); i++)
                    {
                        _params.Add(ActionOption.new_option(splits[i]));
                    }

                }
                else
                {
                    action = text;
                    if (!Enum.TryParse(action, out type)) type = ActionType.none;
                }
            }
            return true;
        }

        public String as_start_pyro_chain_t()
        {
            return  "simc.actions[\"" + (apl == "default" ? "" : "" + apl) +
                             "\"] += simc.StartPyroChain(" +
                             If+
                             ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
        }

        public String as_stop_pyro_chain_t()
        {
            return "simc.actions[\"" + (apl == "default" ? "" : "" + apl) +
                             "\"] += simc.StopPyroChain(" +
                             If+
                             ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
        }

        public String as_wait_t()
        {
            return "simc.actions[\"" + (apl == "default" ? "" : "" + apl) +
                                         "\"] += simc.Wait(" +
                                         If +
                                         ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                                         ");";            
        }

        public bool ParseAction()
        {
            String indent = "";

            ParseExpression();
            FixTokens();

            var __code = "";

            String prefix = indent;

            if (swing.IsMatch(_condition_string))
            {
                Comments.Add("Dont use swing timers man ...");
                prefix += "//";
            }

            if (_condition_string.Contains("aura."))
            {
                Comments.Add("Aura Conditions nono!");
                prefix += "//";
            }

            Understood = true;

            switch (this.type)
            {

                case ActionType.start_pyro_chain:
                    __code = comments(indent) + "" + prefix + as_start_pyro_chain_t();
                    break;
                case ActionType.stop_pyro_chain:
                    __code = comments(indent) + "" + prefix + as_stop_pyro_chain_t();
                    break;

                case ActionType.wait:
                    __code = comments(indent) + "" + prefix + as_wait_t();
                    break;

                case ActionType.run_action_list:
                case ActionType.call_action_list:
                    __code = comments(indent) + "" + prefix + run_action_list_t(this);
                    break;
                case ActionType.potion:
                    __code = comments(indent) + "" + prefix + as_potion_t();
                    break;
                case ActionType.use_item:
                    __code = comments(indent) + "" + prefix + as_use_item_t();
                    break;
                    
                case ActionType.none:
                    __code = comments(indent) + "" + prefix + as_action_t();

                    if (!SimcNames.spells.ContainsKey(action))
                    {
                        SimcraftImpl.Write("Invalid Spell: " + action + ", skipping.");
                        __code = "//" + _fullExpression;
                    }
                    /*if (!SimcraftImpl.DBHasClassSpell(action))
                    {
                        if (!SimcraftImpl.DBHasSpell(action))
                        {
                            SimcraftImpl.Write("Invalid Spell: " + action + ", skipping.");
                            __code = "//" + _fullExpression;
                        }
                    }*/
                    break;
                case ActionType.mana_potion:
                case ActionType.apply_poison:
                case ActionType.cancel_buff:
                case ActionType.cancel_metamorphosis:
                case ActionType.choose_target:
                case ActionType.flask:
                case ActionType.food:
                case ActionType.pool_resource:
                case ActionType.snapshot_stats:
                case ActionType.stance:
                case ActionType.summon_pet:
                case ActionType.wait_until_ready:
                    break;
            }


            foreach (Match m in filter_trinket.Matches(_condition_string))
            {
                __code = __code.Replace(m.Groups[1].ToString(), ResolveTrinketExpression(m.Groups[1].ToString(), myList.Items)[0]) + Environment.NewLine;
            }

            csCode = __code;



            return Understood;
        }

        private string as_action_t()
        {
            return "simc.actions" +
                             (apl == "default" ? "" : "[\"" + apl + "\"]") +
                             (has(ActionOptionType.CycleTargets)
                                 ? " += simc.CycleTargets(\""
                                 : (false ? " += simc.MovingCast(\"" : " += simc.Cast(\"")) +
                             action + "\"," + If +
                             (has(ActionOptionType.Target) ? ",simc.Target" + get(ActionOptionType.Target) : "") +
                             ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
        }

        private string as_use_item_t()
        {
            return "simc.actions" +
                             (apl == "default" ? "" : "[\"" + apl + "\"]") + " += " +
                             (has(ActionOptionType.Slot)
                                 ? "simc.UseItem(" + myList.Items[get(ActionOptionType.Slot)].id
                                 : "") +
                             (has(ActionOptionType.Name)
                                 ? "simc.UseItem(" +
                                   myList.Items.First(ret => ret.Value.name.Equals(get(ActionOptionType.Name))).Value.id +
                                   ""
                                 : "")+
                             ","+ If +
                             (has(ActionOptionType.Target) ? ",simc.Target" + get(ActionOptionType.Target) : "") +
                             ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                             ");";
        }

        private string as_potion_t()
        {
            return  "simc.actions" +
                                         (apl == "default" ? "" : "[\"" + apl + "\"]") +
                                         " += simc.UsePotion(\"" + get(ActionOptionType.Name) + "\"," +
                                         If +
                                         (has(ActionOptionType.Target) ? ",simc.Target" + get(ActionOptionType.Target) : "") +
                                         ",\"" + _condition_string.Replace("\"", "\\\"") + "\"" +
                                         ");";
        }


        public String If
        {
            get
            {

                    String cond; 
                    if (has(ActionOptionType.If)){
                        cond = _condition_string;
                    } else {
                        cond = "true";
                    }

                    return "_if => (" +
                                 cond +
                                   (has(ActionOptionType.LineCd) ? " && simc.line_cd(" + get(ActionOptionType.LineCd) + ")": "") +
                                   (has(ActionOptionType.Sync) ? " && simc.sync(\"" + get(ActionOptionType.Sync) + "\")": "") +
                                   (has(ActionOptionType.Moving) ? " && simc.moving" : "") +
                                   (has(ActionOptionType.Damage) ? " && simc.damage > " + get(ActionOptionType.Damage) : "") +
                                   (has(ActionOptionType.FiveStacks) ? " && simc.buff.frenzy.stack == 5" : "") +
                                   (has(ActionOptionType.Target) ? " && simc.Target" + get(ActionOptionType.Target) + " != null": "")
                                   + ")";
            }
        }

        public AplAction(String fullExpression, List<String> comments, ActionPrioriyList aList)
        {
            myList = aList;
            Comments.AddRange(comments);
            comments.Clear();
            _fullExpression = fullExpression;
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
                ac += indent + "//" + par.ToString() + Environment.NewLine;
            }
            return ac;
        }



        public override string ToString()
        {
            return /*_fullExpression+Environment.NewLine+*/comments("") + apl + " -> " + type + " / " + action + " - " + params_to_string();
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

            string[] bools = new[] { "up", "down", "react" };

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

                    Results.Add("simc.trinket" + outval + ".cooldown." + splits[splits.Count() - 1]);
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

        public bool has(ActionOptionType type)
        {
            return _params.Any(ret => ret.type == type);
        }

        public String get(ActionOptionType type)
        {
            return _params.First(ret => ret.type == type).content;
        }

        public String run_action_list_t(AplAction action)
        {

            var s = "simc.actions" + (action.apl == "default" ? "" : "[\"" + action.apl + "\"]") +
                                      " += simc.CallActionList(\"" + action.get(ActionOptionType.Name) +
                                      "\"," +
                                      If+
                                      (action.has(ActionOptionType.Target) ? ",simc.Target" + action.get(ActionOptionType.Target) : "") +
                                      ",\"" + action._condition_string.Replace("\"", "\\\"") + "\"" +
                                      ");";
            return s;
        }


        public void FixTokens()
        {
            List<APLHotkey> hotkeys = myList.hotkeys;
            var condition = "";

            condition = has(ActionOptionType.If) ? get(ActionOptionType.If).Replace("|", "||").Replace("&", "&&").Replace("%","/") : "";

            condition = condition.ToLower();

            condition = filter_in.Replace(condition, "._in$1");
            condition = filter_equals.Replace(condition, "$1==$2");

            condition = tokenizer.Replace(condition, "simc.$1");


            foreach (var hk in hotkeys)
            {
                condition = condition.Replace("simc." + hk.Name, "simc.hkvar(\"" + hk.Name + "\")");
            }

            condition = condition.Replace("simc.false", "false");
            condition = condition.Replace("simc.true", "true");
            condition = condition.Replace("simc.trinket", "trinket");
            condition = condition.Replace("react==", "stack==");
 
            _condition_string =  condition;
        }


        public String ToCode(String indent)
        {


            return indent + csCode;
        }


    }
}
