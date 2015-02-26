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
                if (code.StartsWith("target="))
                {
                    type = ActionParameterType.Target;
                }
                if (code.StartsWith("line_cd="))
                {
                    type = ActionParameterType.LineCd;
                }
                if (code.StartsWith("name="))
                {
                    type = ActionParameterType.Name;
                }
                if (code.StartsWith("type="))
                {
                    type = ActionParameterType.Type;
                }
                if (code.StartsWith("slot="))
                {
                    type = ActionParameterType.Slot;
                }
                if (code.StartsWith("damage="))
                {
                    type = ActionParameterType.Damage;
                }
                if (code.StartsWith("sync="))
                {
                    type = ActionParameterType.Sync;
                }
                if (code.StartsWith("cycle_targets="))
                {
                    type = ActionParameterType.Cycle;
                } 
                if (code.StartsWith("moving="))
                {
                    type = ActionParameterType.Moving;
                }
                if (code.StartsWith("five_stacks="))
                {
                    type = ActionParameterType.FiveStacks;
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
                Cycle,
                Moving,
                FiveStacks,
            }
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
                    if (!dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("simc.buff." + dbc.ItemProcs[_items.id] + "." + splits[splits.Count() - 1]);
                    //Console.WriteLine(_items.name+" "+_dbc.ItemProcs[_items.id]);
                }
                else
                {
                    Results.Add("simc.buff.anytrinket." + splits[splits.Count() - 1]);
                    /*var isbool = bools.Contains(splits[splits.Count() - 1]);
                    var _items = items.Values.Where(ret => ret.slot.Contains("trinket") && dbc.ItemProcs.ContainsKey(ret.id)).ToArray();
                    var c = _items.Count();
                    
                    Logging.Write("Found "+c+" proccable trinkets " +isbool);

                    if (c == 2 && isbool)
                    {
                        Results.Add("(simc.buff." + dbc.ItemProcs[_items[0].id] + "." + splits[splits.Count() - 1]+"||simc.buff." + dbc.ItemProcs[_items[1].id] + "." + splits[splits.Count() - 1]+")");
                    } else 
                    if (c == 1)
                    {
                        Results.Add("simc.buff." + dbc.ItemProcs[_items[0].id] + "." + splits[splits.Count() - 1]);
                    }
                    else 
                        if (isbool)
                            Results.Add("false");
                        else
                            Results.Add("new SimcraftImpl.MagicValueType(0)");*/

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
                    if (!dbc.ItemProcs.ContainsKey(_items.id))
                        throw new MissingProcException("trinket" + outval + " has no proc / on use effect");

                    Results.Add("simc.trinket"+outval+".cooldown." + splits[splits.Count() - 1]);
                }
                else
                {
                    /*var _items = items.Values.Where(ret => ret.slot.Contains("trinket"));
                    foreach (var it in _items)
                    {
                        //Console.WriteLine(it.id);
                        if (dbc.ItemProcs.ContainsKey(it.id))
                        {
                            //Console.WriteLine("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                            Results.Add(it.slot + ".cooldown." + splits[splits.Count() - 1]);
                        }
                    }*/
                    //Weird cooldowns expressions get ignored
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
                        if (dbc.ItemProcs.ContainsKey(_items.id))
                            Results.Add("true");
                        else Results.Add("false");
                    }
                    else
                    {
                        /*var _items = items.Values.Where(ret => ret.slot.Contains("trinket"));
                        foreach (var it in _items)
                        {
                            //Console.WriteLine(it.id);
                            if (dbc.ItemProcs.ContainsKey(it.id))
                            {
                                //Console.WriteLine("buff." + _dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                                Results.Add("buff." + dbc.ItemProcs[it.id] + "." + splits[splits.Count() - 1]);
                            }
                            else
                            {
                                Results.Add("false");
                            }
                        }*/
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

        public String ToCode(Dictionary<String, EquippedItem> items, String indent)
        {
           
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

            ActionParameter moving =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.Moving);


            ActionParameter fivestacks =
                _params.FirstOrDefault(ret => ret.type == ActionParameter.ActionParameterType.FiveStacks);

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

            bool hasmoving = moving != default(ActionParameter);

            bool hasfivestacks = fivestacks != default(ActionParameter);

            var condition_string = hascondition ? condition.content.Replace("|", "||").Replace("&", "&&") : "";
            condition_string = condition_string.ToLower();
            var filter_equals = new Regex("([^><!])=([^><!])");
            var filter_in = new Regex("\\.in([><=])");
            condition_string = filter_in.Replace(condition_string, "._in$1");
            condition_string = filter_equals.Replace(condition_string, "$1==$2");
            var filter_trinket = new Regex("(trinket\\.[a-z_.12]+)");

            var swing = new Regex("(swing\\.[a-z_.12]+)");

            Regex tokenizer = new Regex("([a-z][\\._a-z0-9]+)");

            

            condition_string = tokenizer.Replace(condition_string, "simc.$1");

            condition_string = condition_string.Replace("simc.false", "false");
            condition_string = condition_string.Replace("simc.true", "true");
            condition_string = condition_string.Replace("simc.trinket", "trinket");
            condition_string = condition_string.Replace("react==", "stack==");
            //condition_string = condition_string.Replace("moving", "simc.moving");

            

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
                                 (hascondition
                                     ? "_if => (" + condition_string +
                                       (haslinecd ? " && simc.line_cd(" + linecd.content + ")" : "") +
                                       (hassync ? " && simc.sync(\"" + sync.content + "\")" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("stop_pyro_chain"))
                    {
                        __code = comments(indent) + "" + prefix + "simc.actions[\"" + (apl == "default" ? "" : "" + apl) + "\"] += simc.StopPyroChain(" +
                                 (hascondition
                                     ? "_if => (" + condition_string +
                                       (haslinecd ? " && simc.line_cd(" + linecd.content + ")" : "") +
                                       (hassync ? " && simc.sync(\"" + sync.content + "\")" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("wait"))
                    {
                        __code = comments(indent) + "" + prefix + "simc.actions[\"" + (apl == "default" ? "" : "" + apl) + "\"] += simc.Wait(" +
                                 (hascondition
                                     ? "_if => (" + condition_string +
                                       (haslinecd ? " && simc.line_cd(" + linecd.content + ")" : "") +
                                       (hassync ? " && simc.sync(\"" + sync.content + "\")" : "")+
                                       (hasmoving ? " && simc.moving" : "")
                                       + ")"
                                     : "") +
                                     ",\"" + condition_string + "\"" +
                                 ");";
                        break;
                    }
                    if (action.Equals("auto_attack") || action.Equals("auto_shot") || action.Equals("summon_pet") || condition_string.Contains("aura."))
                    {
                        Comments.Add("Nonsupported spells and apis");
                        prefix += "//";
                    }
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                        (hascycle ? " += simc.CycleTargets(\"" : (hasmoving ? " += simc.MovingCast(\"" : " += simc.Cast(\"")) + action + "\"" + ", _if => (" +
                             (hascondition 
                                 ? condition_string +
                                   (haslinecd ? " && simc.line_cd(" + linecd.content + ")" : "") +
                                   (hassync ? " && simc.sync(\"" + sync.content + "\")" : "")+
                                   (hasmoving ? " && simc.moving" : "")+
                                   (hasfivestacks ? " && simc.buff.frenzy.stack == 5" : "")
                                   + ")"
                                 : "true"+
                                   (haslinecd ? " && simc.line_cd(" + linecd.content + ")" : "") +
                                   (hassync ? " && simc.sync(\"" + sync.content + "\")" : "") +
                                   (hasmoving ? " && simc.moving" : "") +
                                   (hasfivestacks ? " && simc.buff.frenzy.stack == 5" : "")
                                   + ")") +
                             (hastarget ? ",simc.Target" + target.content : "") +
                             ",\""+condition_string+"\""+
                             ");";

                    if (!SimcraftImpl.DBHasClassSpell(action))
                    {
                        //SimcraftImpl.Write("Couldnt find ClassSpell: "+action+" trying to find Spell");
                        if (!SimcraftImpl.DBHasSpell(action))
                        {
                            SimcraftImpl.Write("Invalid Spell: " + action + ", skippings");
                            Understood = false;
                        }
                    }

                    break;
                case ActionType.run_action_list:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                             " += simc.CallActionList(\"" + name.content +
                             "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                 (hasmoving ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (hastarget ? ",simc.Target" + target.content : "") +
                             ",\"" + condition_string + "\"" +
                             ");";
                    break;
                case ActionType.call_action_list:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                             " += simc.CallActionList(\"" + name.content +
                             "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                 (hasmoving ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (hastarget ? ",simc.Target" + target.content : "") +
                             ",\"" + condition_string + "\"" +
                             ");";
                    break;
                case ActionType.potion:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl + "\"]") +
                             " += simc.UsePotion(\"" + name.content + "\"" +
                             (hascondition
                                 ? ", _if => (" + condition_string +
                                 (hasmoving ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (hastarget ? ",simc.Target" + target.content : "") +
                             ",\"" + condition_string + "\"" +
                             ");";
                    break;
                case ActionType.use_item:
                    __code = comments(indent) + "" + prefix + "simc.actions" + (apl == "default" ? "" : "[\"" + apl +"\"]") + " += " +
                             (hasslot ? "simc.UseItem(" + items[slot.content].id : "") +
                             (hasname
                                 ? "simc.UseItem(" + items.First(ret => ret.Value.name.Equals(name.content)).Value.id + ""
                                 : "")
                             + (hascondition
                                 ? ", _if => (" + condition_string +
                                 (hasmoving ? " && simc.moving" : "") +
                                   ")"
                                 : "") +
                             (hastarget ? ",simc.Target" + target.content : "") +
                             ",\"" + condition_string + "\"" +
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

        List<ActionParameter> _params = new List<ActionParameter>();
        String Name { get; set; }
    }
}
