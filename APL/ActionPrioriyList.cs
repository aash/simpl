using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Styx;
using Styx.Common;

namespace Simcraft
{
    public class ActionPrioriyList
    {
        private List<AplAction> Actions { get; set; }
        private Dictionary<string, EquippedItem> Items { get; set; } 

        public String Name { get; set; }
        public WoWClass Class { get; set; }
        public String Spec { get; set; }


        public ActionPrioriyList()
        {
            Actions = new List<AplAction>();//ParseActions(s);
            Items = new Dictionary<string, EquippedItem>();//ParseItems(s);
        }

        private static String[] Classes = new[] { "death_knight", "druid", "mage", "hunter", "monk", "paladin", "priest", "rogue", "shaman", "warlock", "warrior" };
        private static Regex class_regex = new Regex("(deathknight|druid|mage|hunter|monk|paladin|priest|rogue|shaman|warlock|warrior)=\"(.+)\"");
        private static Regex spec_regex = new Regex("spec=([a-z_]+)");
        private static Regex items = new Regex("(.+)=([a-z_]+),id=(\\d+)");

        public void SetAplHeader(String text)
        {
            Class = ParseClass(class_regex.Match(text).Groups[1].ToString());
            Name = class_regex.Match(text).Groups[2].ToString();
            Spec = spec_regex.Match(text).Groups[1].ToString();
        }

        public static ActionPrioriyList FromString(String s)
        {

            var lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var apl = new ActionPrioriyList();
            apl.SetAplHeader(s);
            foreach (var l in lines)
            {
                var expr = ParseLine(l);
                if (expr is AplAction) apl.Actions.Add((AplAction)expr); else
                    if (expr is EquippedItem) apl.Items.Add(((EquippedItem)expr).slot,(EquippedItem)expr);
                if (expr is Comment) CommentBuffer.Add(((Comment)expr).Content);
            }

            return apl;

        }

        private static AplExpr ParseLine(String l)
        {
            if (l.StartsWith("#")) return new Comment{Content = l};
            if (l.StartsWith("action")) return ParseAction(l);
            if (items.IsMatch(l)) return ParseItem(l);
            return default(AplExpr);
        }

        private static AplAction ParseAction(String l)
        {
            return new AplAction(l, CommentBuffer);
        }

        private static EquippedItem ParseItem(String l)
        {
            var m = items.Match(l);
            return new EquippedItem(m.Groups[1].ToString(), m.Groups[2].ToString(), m.Groups[3].ToString());
        }

      
        private static List<String> CommentBuffer = new List<string>();

        public Assembly Assembly { get; set; }

        public void CreateBehavior()
        {

            

            if (Assembly == null) throw new Exception(Name + " has not been compiled");
            var mem = Assembly.GetTypes()[0].GetMembers()[0];
            var typ = Assembly.GetTypes()[0];
            //Logging.Write("func: " + mem);

            typ.InvokeMember(mem.Name,
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                null, null, new object[0]);

            //Logging.Write(SimcraftImpl.inst.actions.ToString());
        }

        private WoWClass ParseClass(String s)
        {
            if (s.Equals("deathknight")) return WoWClass.DeathKnight;
            if (s.Equals("druid")) return WoWClass.Druid;
            if (s.Equals("mage")) return WoWClass.Mage;
            if (s.Equals("hunter")) return WoWClass.Hunter;
            if (s.Equals("monk")) return WoWClass.Monk;
            if (s.Equals("paladin")) return WoWClass.Paladin;
            if (s.Equals("priest")) return WoWClass.Priest;
            if (s.Equals("rogue")) return WoWClass.Rogue;
            if (s.Equals("shaman")) return WoWClass.Shaman;
            if (s.Equals("warlock")) return WoWClass.Warlock;
            if (s.Equals("warrior")) return WoWClass.Warrior;
            return WoWClass.None;
        }

        private WoWSpec ParseSpec(String s)
        {
            var sp = Spec.ToLower();

            foreach (string name in Enum.GetNames(typeof(WoWSpec)))
            {
                //ifLevenshteinDistance.Compute(name.ToLower(), sp.ToLower());
                /*if (name.ToLower().Contains(sp))
                {
                    WoWSpec _s;
                    WoWSpec.TryParse(name, out _s);
                    return _s;
                }*/
            }

            return WoWSpec.None;
        }

        public String ToCode()
        {


            List<String> subapls = new List<string>();
            var capl = "";
            String code = "using Styx;" + Environment.NewLine;
            code += "using Styx.Common;" + Environment.NewLine;
            code += "namespace Simcraft" + Environment.NewLine; ;
            code += "{" + Environment.NewLine; ;
            code += "\tpublic class SimcraftRota" + Environment.NewLine; ;
            code += "\t{" + Environment.NewLine; ;
            code += "\t\tprivate static SimcraftImpl simc{get { return SimcraftImpl.inst; }}" + Environment.NewLine;
            code += "\t\t#region "+ Name+ Environment.NewLine;
            code += "\t\t[Behavior(WoWClass." + Class + ", WoWSpec." + ParseSpec(Spec) + ", WoWContext.PvE)]" + Environment.NewLine;
            code += "\t\tpublic static void Generate" + UppercaseFirst(Class.ToString()).Replace("_", "") + UppercaseFirst(Spec).Replace("_", "") + "PvEBehavior()" + Environment.NewLine;
            code += "\t\t{" + Environment.NewLine;

            /*foreach (var action in Actions)
            {
                if (!subapls.Contains(action.apl)) subapls.Add(action.apl);
            }
            subapls.Remove("default");

            foreach (var action in subapls)
            {
                code += "\t\tsimc.actions.add(\""+action+"\");" + Environment.NewLine;
            }*/

            foreach (var action in Actions)
            {
                if (!capl.Equals(action.apl))
                {
                    capl = action.apl;
                    code += Environment.NewLine;
                }
                
                    code += action.ToCode(Items, "\t\t\t") + Environment.NewLine;

            }
            code += "\t\t\tLogging.Write(\"Behaviors created !\");" + Environment.NewLine; ;
            code += "\t\t}" + Environment.NewLine; ;
            code += "\t\t#endregion"+Environment.NewLine;
            code += "\t}" + Environment.NewLine; ;
            code += "}" + Environment.NewLine; ;

            //SimcraftImpl.Write(code);

            return code;
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public override string ToString()
        {
            return Name + " - " + Class + " - " + Spec + " Actions: " + Actions.Count;
        }
    }
}