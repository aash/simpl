using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimcParse
{
    class Parser
    {
        public class SizedString
        {
            public int index;
            public int length;

            public int end
            {
                get { return index + length; }
            }
        }

        public static String[] Classes = new[] { "death_knight", "druid", "mage", "hunter", "monk", "paladin", "priest", "rogue", "shaman", "warlock", "warrior" };
        public static Regex class_regex = new Regex("(deathknight|druid|mage|hunter|monk|paladin|priest|rogue|shaman|warlock|warrior)=\"(.+)\"");
        public static Regex spec_regex = new Regex("spec=([a-z_]+)");

        public static void SetAplHeader(ActionPrioriyList apl, String text)
        {
            apl.Class = class_regex.Match(text).Groups[1].ToString();
            apl.Name = class_regex.Match(text).Groups[2].ToString();
            apl.Spec = spec_regex.Match(text).Groups[1].ToString();
        }

        public static String[] typedefs = new[]
        {
            "spell_t"
            , "attack_t"
            , "buff"
            , "buff_t"
            , "spec."
            , "talent."
            , "glyph."
            , "spell."
            , "perk."
            , "buffs."
            , "gains."
            , "procs."
            , "base_t"
            , "data_t"
            , "heal_t"
            , "passives."
            , "pet_t"
            , "debuffs_"
            , "debuff_t"
            , "talents"
            , "glyphs."
            , "specs"
            , "perks"
            , "proc"
            , "spells"
            , "stance"
            , "poison_t"
        };


        public static void CreateDatabase()
        {
            File.WriteAllText("Database.cs", "");
            List<String> lines = new List<string>();
            String lines2 = "";
            foreach (var _class in Classes)
            {
                int i = 0;
                var fn = @"..\..\sc_" + _class + ".cpp";

                File.ReadAllText(fn).Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList().ForEach(
                    (ret) =>
                    {
                        Regex regex_spell = new Regex("\"[A-Z]");
                        bool x = false;
                        foreach (var s in typedefs)
                        {
                            ret = ret.Trim();
                            if (ret.Substring(0, ret.Length > 30 ? 30 : ret.Length).Contains(s)) x = true;
                        }
                        if (regex_spell.IsMatch(ret) && x)
                        {
                            lines.Add(ret.Trim());
                            lines2 += Environment.NewLine + ret.Trim();
                        }
                        /*if (ret.Contains("find_") && ret.Contains("_spell"))
                        {
                            var st = Regex.Replace(ret, @"\s+", "");
                            Regex regex_newline = new Regex("(\r\n|\r|\n)");
                            st = regex_newline.Replace(ret.Trim(), "");
                            if (st[0] !='.') Console.WriteLine(st);
                        }*/
                    });
                //GetInitSpells(fn, _class);
            }

            Regex spell_t = new Regex(".+(_spell_t|attack_t|buff_t|base_t|heal_t|pet_t|debuff_t|poison_t|buff_creator_t|base_t)\\((.+?)\\)");
            Regex two_strings = new Regex("\"(.+?)\".*,.+\"(.+?)\"");

            Dictionary<String, Dictionary<String, String>> cs = new Dictionary<string, Dictionary<String, String>>();

            //Dark Soul Workaround, stupid ability changes names ^^
            cs["spec"] = new Dictionary<string, string>();
            cs["spec"]["dark_soul"] = "Dark Soul";
            cs["buff_creator_t"] = new Dictionary<string, string>();
            cs["buff_creator_t"]["raging_blow"] = "Raging Blow!";

            foreach (var line in lines)
            {
                if (spell_t.IsMatch(line))
                {
                    var a = spell_t.Match(line);
                    var _type = a.Groups[1].Captures[0].Value;
                    var _params = a.Groups[2].Captures[0].Value;
                    if (two_strings.IsMatch(_params))
                    {
                        var cap = two_strings.Match(_params);
                        //Console.WriteLine(_type+" "+cap.Groups[1].Captures[0] + " -> " + cap.Groups[2].Captures[0]);
                        if (!cs.ContainsKey(_type)) cs[_type] = new Dictionary<string, string>();
                        var name = cap.Groups[1].Captures[0].Value.Trim();
                        var value = cap.Groups[2].Captures[0].Value.Trim();
                        if (!cs[_type].ContainsKey(name))
                            cs[_type].Add(name, value);

                    }
                }

            }

            Regex assign = new Regex("(spec|talents|perk|glyphs|spell)\\.(.+)\\s*=\\s*.+find_.+_spell\\((.+?)\\)");

            foreach (var line in lines)
            {
                if (assign.IsMatch(line))
                {
                    var a = assign.Match(line);
                    var _type = a.Groups[1].Captures[0].Value.Trim();
                    var name = a.Groups[2].Captures[0].Value.Trim();
                    var value = a.Groups[3].Captures[0].Value.Trim().Replace("\"", "");
                    //Console.WriteLine(_type + " " + name + " " + value);
                    //Same Workaround
                    if (name.Contains("dark_soul")) continue;

                    if (!cs.ContainsKey(_type)) cs[_type] = new Dictionary<string, string>();
                    if (!cs[_type].ContainsKey(name))
                        cs[_type].Add(name, value);

                }

            }

            String lines3 = "";

            lines3 += Environment.NewLine + "using System;";
            lines3 += Environment.NewLine + "using System.Collections.Generic;";
            lines3 += Environment.NewLine + "using System.Linq;";
            lines3 += Environment.NewLine + "using System.Text;";
            lines3 += Environment.NewLine + "using System.Threading.Tasks;";

            lines3 += Environment.NewLine + "namespace Simcraft";
            lines3 += Environment.NewLine + "{";



            foreach (var c in cs.Keys)
            {
                List<String> names = new List<string>();
                lines3 += Environment.NewLine + "\tpublic static class " + c + "{";
                lines3 += Environment.NewLine +
                          "\t\tpublic static Dictionary<string,string> db = new Dictionary<string, string>();";
                lines3 += Environment.NewLine + "\t\tstatic " + c + "(){";
                foreach (var k in cs[c].Keys)
                {
                    var val = cs[c][k].Trim();
                    var l = Environment.NewLine + "\t\t\tdb[\"" + k + "\"] = \"" + val + "\";";
                    if (!names.Contains(l))
                    {
                        lines3 += l;
                        names.Add(l);
                    }

                }
                lines3 += Environment.NewLine + "\t\t}";
                lines3 += Environment.NewLine + "\t}";

            }
            lines3 += Environment.NewLine + "}";
            File.WriteAllText("Database.cs", lines3);
        }



        public static List<String> comments = new List<string>();

        public static List<Action> ParseActions(String s)
        {

            var lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            List<Action> actions = new List<Action>();
            foreach (var line in lines)
            {
                var l = line.Trim();

                if (l.StartsWith("#")) comments.Add(l);

                Action a = null;
                if (l.StartsWith("action"))
                {
                    a = new Action(l, comments);
                    //Console.WriteLine(a);
                    actions.Add(a);
                    comments.Clear();
                }
            }
            return actions;
        }
        public static Dictionary<String, EquippedItem> ParseItems(String s)
        {
            Dictionary<String, EquippedItem> _items = new Dictionary<String, EquippedItem>();
            Regex items = new Regex("(.+)=([a-z_]+),id=(\\d+)");

            Match m;

            foreach (Match ItemMatch in items.Matches(s))
            {
                var a = new EquippedItem(ItemMatch.Groups[1].ToString(), ItemMatch.Groups[2].ToString(), ItemMatch.Groups[3].ToString());
                _items[a.slot] = a;
            }
            return _items;
        }
        public static void ParseSpells(List<String> lines)
        {
            int line_count = 0;
            while (line_count < lines.Count)
            {
                var line = lines[line_count];

            }


        }

        public static List<String> GetInitSpells(String fn, String _class)
        {
            var s = File.ReadAllText(fn);

            var bf = GetFunctionBodyBySig("void " + _class + "_t::init_spells()", s, 0);

            return s.Substring(bf.index, bf.length).Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        public static SizedString GetFunctionBodyBySig(String signature, String file, int sindex)
        {
            var x = new SizedString();
            var funcsig = file.IndexOf(signature, sindex);
            var func_start = file.IndexOf("{", funcsig);
            int level = 1;
            int i = func_start;
            while (level >= 1)
            {
                var close = file.IndexOf("}", i + 1);
                var open = file.IndexOf("{", i + 1);
                if (close < open)
                {
                    level--;
                    i = close;
                }
                if (close > open)
                {
                    level++;
                    i = open;
                }
            }
            i++;
            x = new SizedString();
            x.index = funcsig;
            x.length = (i - funcsig);
            return x;
        }
    }

}
