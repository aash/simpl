using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimcParse
{
    public class Action
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

        public Action(String code, List<String> comments)
        {
            Comments.AddRange(comments);
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
                        _params.Add(new ActionParameter(splits[i]));
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

        public class ActionParameter
        {

            public ActionParameterType type = ActionParameterType.Unknown;
            public String content = "";

            public ActionParameter(String code)
            {

                if (code.Contains("if="))
                {
                    type = ActionParameterType.Condition;
                }
                if (code.Contains("target="))
                {
                    type = ActionParameterType.Target;
                }
                if (code.Contains("line_cd="))
                {
                    type = ActionParameterType.LineCd;
                }
                if (code.Contains("name="))
                {
                    type = ActionParameterType.Name;
                }
                if (code.Contains("type="))
                {
                    type = ActionParameterType.Type;
                }
                if (code.Contains("slot="))
                {
                    type = ActionParameterType.Slot;
                }
                if (code.Contains("damage="))
                {
                    type = ActionParameterType.Damage;
                }
                if (code.Contains("sync="))
                {
                    type = ActionParameterType.Sync;
                }
                if (code.Contains("cycle_targets="))
                {
                    type = ActionParameterType.Cycle;
                }
                content = code.Substring(code.IndexOf("=") + 1, code.Length - (code.IndexOf("=") + 1));

            }

            public override string ToString()
            {
                return type + ": " + content;
            }

            public enum ActionParameterType
            {
                Condition,
                Target,
                LineCd,
                Name,
                Type,
                Slot,
                Damage,
                Unknown,
                Sync,
                Cycle
            }
        }



        public List<String> ResolveTrinketExpression(String expr, Dictionary<String, EquippedItem> items)
        {
            List<String> Results = new List<string>();

            trinket_proc_expr_e pexprtype = trinket_proc_expr_e.PROC_ENABLED;
            trinket_proc_type_e ptype = trinket_proc_type_e.PROC_STAT;
            stat_e stat = stat_e.STAT_NONE;

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
                Results.Add("false");
                return Results;
            }

            if (ptype != trinket_proc_type_e.PROC_COOLDOWN)
            {
                stat = stat_e.STAT_ALL;
            }

            if (pexprtype == trinket_proc_expr_e.PROC_ENABLED && ptype != trinket_proc_type_e.PROC_COOLDOWN)
            {
                if (outval == 1 || outval == 2)
                {
                    var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));


                    if (_items == default(EquippedItem))
                        throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                    if (!_dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("buff." + _dbc.ItemProcs[_items.id] + "." + splits[splits.Count() - 1]);
                    //Console.WriteLine(_items.name+" "+_dbc.ItemProcs[_items.id]);
                }
                else
                {
                    var _items = items.Values.Where(ret => ret.slot.Contains("trinket"));
                    foreach (var it in _items)
                    {
                        //Console.WriteLine(it.id);
                        if (_dbc.ItemProcs.ContainsKey(it.id))
                        {
                            //Console.WriteLine("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                            Results.Add("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                        }
                    }
                    if (Results.Count() == 0)
                    {
                        if (splits[splits.Count() - 1].Equals("up") || splits[splits.Count() - 1].Equals("down"))
                            Results.Add("false");
                        else
                            Results.Add("new BoolTimeSpan(0)");
                    }
                }
            }
            else if (pexprtype == trinket_proc_expr_e.PROC_ENABLED && ptype == trinket_proc_type_e.PROC_COOLDOWN)
            {
                if (outval == 1 || outval == 2)
                {
                    var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));


                    if (_items == default(EquippedItem))
                        throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                    if (!_dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("trinket."+outval+".cooldown." + splits[splits.Count() - 1]);
                    //Console.WriteLine(_items.name+" "+_dbc.ItemProcs[_items.id]);
                }
                else
                {
                    var _items = items.Values.Where(ret => ret.slot.Contains("trinket"));
                    foreach (var it in _items)
                    {
                        //Console.WriteLine(it.id);
                        if (_dbc.ItemProcs.ContainsKey(it.id))
                        {
                            //Console.WriteLine("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                            Results.Add(it.slot + ".cooldown." + splits[splits.Count() - 1]);
                        }
                    }
                    if (splits[splits.Count() - 1].Equals("up") || splits[splits.Count() - 1].Equals("down"))
                        Results.Add("false");
                    else
                        Results.Add("new BoolTimeSpan(0)");
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
                    if (outval == 1 || outval == 2)
                    {
                        var _items = items.Values.FirstOrDefault(ret => ret.slot.Contains("trinket" + outval));


                        if (_items == default(EquippedItem))
                            throw new MissingItemException("You need to add trinket" + outval + " as an equipped item");
                        if (!_dbc.ItemProcs.ContainsKey(_items.id))
                            Results.Add("false");
                        else Results.Add("true");

                        ///Results.Add("buff." + _dbc.ItemProcs[_items.id] + "." + splits[splits.Count() - 1]);
                        //Console.WriteLine(_items.name+" "+_dbc.ItemProcs[_items.id]);
                    }
                    else
                    {
                        var _items = items.Values.Where(ret => ret.slot.Contains("trinket"));
                        foreach (var it in _items)
                        {
                            //Console.WriteLine(it.id);
                            if (_dbc.ItemProcs.ContainsKey(it.id))
                            {
                                //Console.WriteLine("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                                Results.Add("true");
                            }
                            else
                            {
                                Results.Add("false");
                            }
                        }
                        if (splits[splits.Count() - 1].Equals("up") || splits[splits.Count() - 1].Equals("down"))
                            Results.Add("false");
                        else
                            Results.Add("new BoolTimeSpan(0)");
                    }                 
                }
            }
            return Results;
            
        }

        public String ToCode(Dictionary<String, EquippedItem> items, String indent)
        {
            String _code = "";
            ActionParameter condition =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Condition);
            ActionParameter damage =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Damage);
            ActionParameter linecd =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.LineCd);
            ActionParameter name =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Name);
            ActionParameter slot =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Slot);
            ActionParameter target =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Target);
            ActionParameter type =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Type);
            ActionParameter unk =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Unknown);

            ActionParameter sync =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Sync);

            ActionParameter cycle =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Cycle);

            bool hascondition = condition != default(ActionParameter);

            bool hasdamage = damage != default(ActionParameter);

            bool haslinecd = linecd != default(ActionParameter);

            bool hasname = name != default(ActionParameter);

            bool hasslot = slot != default(ActionParameter);

            bool hastarget = target != default(ActionParameter);

            bool hastype = type != default(ActionParameter);

            bool hasunk = unk != default(ActionParameter);

            bool hassync = sync != default(ActionParameter);

            bool hascycle = cycle != default(ActionParameter);

            var condition_string = hascondition ? condition.content.Replace("|", "||").Replace("&", "&&") : "";
            var filter_equals = new Regex("([^><])=([^><])");
            var filter_in = new Regex("\\.in([><=])");
            condition_string = filter_in.Replace(condition_string, "._in$1");
            condition_string = filter_equals.Replace(condition_string, "$1==$2");
            var filter_trinket = new Regex("(trinket\\.[a-z_.12]+)");

            var swing = new Regex("(swing\\.[a-z_.12]+)");


            var __code = "";

            String prefix = indent;

            if (swing.IsMatch(condition_string))
            {
                Comments.Add("Dont use swing timers man ...");
                prefix += "//";
            }
               

            switch (this.type)
            {
                case ActionType.cast:
                    if (action.Equals("auto_attack") || action.Equals("auto_shot") || action.Equals("summon_pet"))
                    {
                        Comments.Add("Skipping auto attack");
                        prefix += "//";
                    }
                    __code = comments(indent) + "" + prefix + "actions" + (apl == "default" ? "" : "." + apl) +
                             (hascycle ? " += CycleTargets(\"" : " += Cast(\"") + action + "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                   (haslinecd ? " && line_cd(" + linecd.content + ")" : "") +
                                   (hassync ? " && sync(" + sync.content + ")" : "")
                                   + ")"
                                 : "") +
                             (hastarget ? ",Target" + target.content : "") +
                             ");";
                    break;
                case ActionType.run_action_list:
                    __code = comments(indent) + "" + prefix + "actions" + (apl == "default" ? "" : "." + apl) +
                             " += CallActionList(\"" + name.content +
                             "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                   ")"
                                 : "") +
                             (hastarget ? ",Target" + target.content : "") +
                             ");";
                    break;
                case ActionType.call_action_list:
                    __code = comments(indent) + "" + prefix + "actions" + (apl == "default" ? "" : "." + apl) +
                             " += CallActionList(\"" + name.content +
                             "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                   ")"
                                 : "") +
                             (hastarget ? ",Target" + target.content : "") +
                             ");";
                    break;
                case ActionType.potion:
                    __code = comments(indent) + "" + prefix + "actions" + (apl == "default" ? "" : "." + apl) +
                             " += UsePotion(\"" + name.content + "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                   ")"
                                 : "") +
                             (hastarget ? ",Target" + target.content : "") +
                             ");";
                    break;
                case ActionType.use_item:
                    __code = comments(indent) + "" + prefix + "actions" + (apl == "default" ? "" : "." + apl) + " += " +
                             (hasslot ? "UseItem(" + items[slot.content].id : "") +
                             (hasname
                                 ? "UseItem(" + items.First(ret => ret.Value.name.Equals(name.content)).Value.id + ""
                                 : "")
                             + (hascondition
                                 ? ", _if => (" + condition_string +
                                   ")"
                                 : "") +
                             (hastarget ? ",Target" + target.content : "") +
                             ");";
                    break;
                default:
                    __code = prefix + "//" + code;
                    break;
            }


            var retcode = "";

            foreach (Match m in filter_trinket.Matches(condition_string))
            {
                foreach (var s in  ResolveTrinketExpression(m.Groups[1].ToString(), items))
                {
                    retcode += __code.Replace(m.Groups[1].ToString(), s) + Environment.NewLine;
                }
              
            }

            if (retcode.Length > 0)
            {
                //Console.WriteLine(retcode);
                return retcode;
            }
            else
            {
                //Console.WriteLine(__code);
                return __code;
            }

            
        }

        List<ActionParameter> _params = new List<ActionParameter>();
        String Name { get; set; }
    }
}
