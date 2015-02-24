using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimcParse
{
    class dbc
    {
        static bool Set(String num, int _byte, int t)
        {
            var s = num.Substring(_byte + 2, 1);
            if (s == "a") s = "10";
            if (s == "b") s = "11";
            if (s == "c") s = "12";
            if (s == "d") s = "13";
            if (s == "e") s = "14";
            if (s == "f") s = "15";
            return (Convert.ToInt32(s) & t) == t;
        }

        static bool IsAbility(String num)
        {
            return Set(num, 7, 1);
        }


        static bool IsTradeskill(String num)
        {
            return Set(num, 7, 2);
        }

        public class Spell
        {
            public int Id { get; set; }
            public String Name { get; set; }
            public String Token { get; set; }
            public int Gcd { get; set; }
            public String Desc { get; set; }
        }

        static Dictionary<String, Spell> Spells = new Dictionary<string, Spell>();
        static Dictionary<int, Spell> stringsById = new Dictionary<int, Spell>();
        static Dictionary<String, Spell> ClassSpells = new Dictionary<string, Spell>();
        static Dictionary<String, Spell> Glyphs = new Dictionary<string, Spell>();
        static Dictionary<int, List<Effect>> Effects = new Dictionary<int, List<Effect>>();
        static Dictionary<String, Dictionary<int, int>> Sets = new Dictionary<string, Dictionary<int, int>>();


        static Regex token = new Regex("[^a-z_ 0-9]");

        private static String Tokenize(String s)
        {
            s = s.ToLower();
            s = token.Replace(s, "");
            s = s.Replace(' ', '_');
            return s;
        }

        static List<String> GetFunction(String sig, String s)
        {
            var p = Parser.GetFunctionBodyBySig(
                sig, s, 0);
            var lines = s.Substring(p.index, p.length).Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList(); ;
            return lines;
        }

        public class Effect
        {
            public int Id;
            public int Sid;
            public String Type1;
            public String Type2;
            public int Value;
        }

        public static void DumpDB()
        {
            var fn = @"..\..\sc_spell_data.inc";
            var s = File.ReadAllText(fn);

            var ret = "";

            //{ 253411, 0x00, 177041,  0, E_APPLY_AURA                , A_PROC_TRIGGER_SPELL                       ,  0.0000000000,  0.0000000000,  0.0000000000,  0.0000000000,  0.0000000000,     0,     0.0,     0.0,       0,       0,       0, { 0x00000000, 0x00000000, 0x00000000, 0x00000000 }, 177040, 1.000000,   0.0, 0.000,  0, 0, 0 },
            Regex effects = new Regex("{ (\\d+?),.+?,(.+?),.+?,(.+?),(.+?), .+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?{.+?},(.+?),.+?");
            Regex db = new Regex("\"(.+?)\".*?,(.+?),.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,(.+?),(.+)");
            Regex str = new Regex("\"(.+?)\"");
            Regex val = new Regex("\\$(\\d*?)s1");
            Regex ab = new Regex("(\\d+),");
            //{ "Blacksteel Battleplate"                     ,  "tier17lfr",      1,  1245,   17,   2,   1,              {   0 },    0,   73, 179142, { 120396, 120395, 120394, 120393, 120392, 120391,  0 } },
            Regex sb = new Regex("\".+\".*?,.*?\"(.+?)\".+?{.+?}.+?{(.+?)}");

            var lines = s.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);

            int co = 0;

            for (int i = 0; i < lines.Count(); i++)
            {
                if (db.IsMatch(lines[i]))
                {
                    var m = db.Match(lines[i]);
                    //var l = Convert.ToInt32(m.Groups[3].ToString());

                    var l = m.Groups[2].ToString();
                    var _s = new Spell
                    {
                        Gcd = Convert.ToInt32(m.Groups[3].ToString()),
                        Id = Convert.ToInt32(m.Groups[2].ToString()),
                        Name = m.Groups[1].ToString().Trim(),
                        Token = Tokenize(m.Groups[1].ToString()),
                        Desc = (str.IsMatch(m.Groups[4].ToString()) ? str.Match(m.Groups[4].ToString()).Groups[1].ToString() : "")
                    };
                    stringsById[_s.Id] = _s;
                    if (Spells.ContainsKey(_s.Token) && !_s.Name.Equals(Spells[_s.Token].Name))
                    {
                        Console.WriteLine("Mismatch: " + _s.Token + " 1: " + Spells[_s.Token].Name + " 2: " + _s.Name +
                                          " " + i + "/" + lines.Length);
                        if (!Spells[_s.Token].Name.Contains("!") && _s.Name.Contains("!"))
                        {
                            Spells[_s.Token] = _s;
                        }
                        /*var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.D2)
                        {
                            Spells[_s.Token] = _s;
                        }*/
                    }
                    else
                    {
                        Spells[_s.Token] = _s;
                    }
                }

                if (effects.IsMatch(lines[i]))
                {
                    var m = effects.Match(lines[i]);
                    var id = Convert.ToInt32(m.Groups[1].ToString());
                    var sid = Convert.ToInt32(m.Groups[2].ToString());
                    var t1 = m.Groups[3].ToString().Trim();
                    var t2 = m.Groups[4].ToString().Trim();
                    var proc =  Convert.ToInt32(m.Groups[5].ToString());

                    if (!Effects.ContainsKey(sid)) Effects[sid] = new List<Effect>();
                    Effects[sid].Add(new Effect{Id = id, Sid = sid, Type1 = t1, Type2 = t2, Value = proc});
                }
            }

            Spells["lucky_flip"] = new Spell {Gcd = 0, Id = 177597, Name = "\\\"Lucky\\\" Flip", Token = "lucky_flip", Desc = "Increases your Agility by $s1 for $d."};
            stringsById[Spells["lucky_flip"].Id] = Spells["lucky_flip"];

            fn = @"..\..\sc_spell_lists.inc";
            s = File.ReadAllText(fn);

            var class_ability_data = GetFunction(
                "static unsigned __class_ability_data[][CLASS_ABILITY_TREE_SIZE][CLASS_ABILITY_SIZE] = {", s);

            var race_ability_data = GetFunction(
                "static unsigned __race_ability_data[MAX_RACE][MAX_CLASS][RACE_ABILITY_SIZE] = {", s);

            var glyph_ability_data = GetFunction(
                "static unsigned __glyph_abilities_data[][3][GLYPH_ABILITIES_SIZE] = {", s);

            var setbonus_data = GetFunction(
                "static item_set_bonus_t __set_bonus_data[SET_BONUS_DATA_SIZE] = {", s);

            //s = s.Substring(p.index, p.length);


            foreach (var line in class_ability_data)
            {
                if (ab.IsMatch(line))
                {
                    var m = ab.Match(line);
                    var i = Convert.ToInt32(m.Groups[1].ToString());
                    if (i == 0) continue;

                    var _s = stringsById[i];
                    ClassSpells[_s.Token] = _s;
                }

            }

            foreach (var line in race_ability_data)
            {
                if (ab.IsMatch(line))
                {
                    var m = ab.Match(line);
                    var i = Convert.ToInt32(m.Groups[1].ToString());
                    if (i == 0) continue;

                    var _s = stringsById[i];
                    //Console.WriteLine(_s.Name);
                    ClassSpells[_s.Token] = _s;
                }

            }

            foreach (var line in glyph_ability_data)
            {
                if (ab.IsMatch(line))
                {
                    var m = ab.Match(line);
                    var i = Convert.ToInt32(m.Groups[1].ToString());
                    if (i == 0) continue;

                    var _s = stringsById[i];
                    //Console.WriteLine(_s.Name);
                    Glyphs[_s.Token] = _s;
                }

            }

            foreach (var line in setbonus_data)
            {
                if (sb.IsMatch(line))
                {
                    var m = sb.Match(line);
                    var name = m.Groups[1].ToString();
                    if (!Sets.ContainsKey(name)) Sets[name] = new Dictionary<int, int>();
                    var dict = Sets[name];

                    foreach (var it in m.Groups[2].ToString().Split(','))
                    {
                        var idx = Convert.ToInt32(it);
                        if (!dict.ContainsKey(idx)) dict[idx] = 0;
                    }
                }
            }

            fn = @"..\..\sc_item_data.inc";
            s = File.ReadAllText(fn);

            Regex item = new Regex("(\\d+),.*?\"(.+?)\".*?,.*?,.*?,.*?,.*?(\\d+).*?,.+?{.+?{.+?{.+?{.+?{.+?{.*?(\\d+)");

            lines = s.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);

            foreach (var l in lines)
            {
                if (item.IsMatch(l))
                {
                    var procid = Convert.ToInt32(item.Match(l).Groups[4].ToString().Trim());
                    var ilvl = Convert.ToInt32(item.Match(l).Groups[3].ToString());
                    var iid = Convert.ToInt32(item.Match(l).Groups[1].ToString());

                    if (procid != 0 && ilvl > 600)
                    {
                        if (Effects.ContainsKey(procid))
                        {
                            var efs = Effects[procid].Where(_ret => _ret.Type1.Equals("E_APPLY_AURA")&& _ret.Value > 0);
                            foreach (var e in efs)
                            {
                                    ItemProcs[e.Sid] = stringsById[e.Value].Token;
                                    Console.WriteLine(e.Sid + " = " + stringsById[e.Value].Token + " //" +
                                                      item.Match(l).Groups[2]);
                            }
                            if (efs.Count() == 0 && stringsById.ContainsKey(procid))
                            {

                                ItemProcs[iid] = stringsById[procid].Token;
                                Console.WriteLine(iid + " = " + stringsById[procid].Token + " //" +
                                                  item.Match(l).Groups[2]);
                            }               
                        }

                            
                            
                    }

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

            lines3 += Environment.NewLine + "\tpublic static class " + "dbc" + "{";
            lines3 += Environment.NewLine + "\t\tpublic class Spell";
            lines3 += Environment.NewLine + "\t\t{";
            lines3 += Environment.NewLine + "\t\t\tpublic int Id { get; set; }";
            lines3 += Environment.NewLine + "\t\t\tpublic String Name { get; set; }";
            lines3 += Environment.NewLine + "\t\t\tpublic String Token { get; set; }";
            lines3 += Environment.NewLine + "\t\t\tpublic int Gcd { get; set; }";
            lines3 += Environment.NewLine + "\t\t}";

            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<String, Spell> Spells = new Dictionary<string, Spell>();";
            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<int, Spell> stringsById = new Dictionary<int, Spell>();";
            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<String, Spell> ClassSpells = new Dictionary<string, Spell>(); ";
            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<String, Spell> Glyphs = new Dictionary<string, Spell>();";
            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<String, List<int>> Sets = new Dictionary<string, List<int>>();";
            lines3 += Environment.NewLine +
                      "\t\tpublic static Dictionary<int, String> ItemProcs = new Dictionary<int, String>();";

            lines3 += Environment.NewLine +
                      "\t\tstatic dbc(){";

            foreach (var c in ClassSpells)
            {
                if (c.Value.Token.Length > 1 && c.Value.Id != 118883)
                    lines3 += Environment.NewLine + "\t\t\tClassSpells[\"" + c.Value.Token + "\"] = new Spell{Gcd = " +
                              c.Value.Gcd + ",Id = " + c.Value.Id +
                              ",Name = \"" + c.Value.Name + "\",Token = \"" + c.Value.Token + "\"};  ";
            }
            foreach (var c in Spells)
            {
                if (c.Value.Token.Length > 1 && c.Value.Id != 118883)
                    lines3 += Environment.NewLine + "\t\t\tSpells[\"" + c.Value.Token + "\"] = new Spell{Gcd = " +
                              c.Value.Gcd + ",Id = " + c.Value.Id +
                              ",Name = \"" + c.Value.Name + "\",Token = \"" + c.Value.Token + "\"};  ";
            }
            foreach (var c in Glyphs)
            {
                if (c.Value.Token.Length > 1 && c.Value.Id != 118883)
                    lines3 += Environment.NewLine + "\t\t\tGlyphs[\"" + c.Value.Token + "\"] = new Spell{Gcd = " +
                              c.Value.Gcd + ",Id = " + c.Value.Id +
                              ",Name = \"" + c.Value.Name + "\",Token = \"" + c.Value.Token + "\"};  ";
            }

            foreach (var c in ItemProcs)
            {
                lines3 += Environment.NewLine + "\t\t\tItemProcs[" + c.Key + "] = \"" + c.Value + "\";";
            }

            lines3 += Environment.NewLine + "//Hacks";
            lines3 += Environment.NewLine +
                      "Spells[\"archmages_greater_incandescence_agi\"] = Spells[\"archmages_greater_incandescence\"];";
            lines3 += Environment.NewLine +
                      "Spells[\"archmages_greater_incandescence_str\"] = Spells[\"archmages_greater_incandescence\"];";
            lines3 += Environment.NewLine +
                      "Spells[\"archmages_greater_incandescence_int\"] = Spells[\"archmages_greater_incandescence\"];";

            lines3 += Environment.NewLine +
                      "Spells[\"archmages_incandescence_agi\"] = Spells[\"archmages_incandescence\"];";
            lines3 += Environment.NewLine +
                      "Spells[\"archmages_incandescence_str\"] = Spells[\"archmages_incandescence\"];";
            lines3 += Environment.NewLine +
                      "Spells[\"archmages_incandescence_int\"] = Spells[\"archmages_incandescence\"];";
            //lines3 += Environment.NewLine + "Spells[\"lucky_flip\"] = new Spell { Gcd = 0, Id = 177597, Name = \"\\\"Lucky\\\" Flip\", Token = \"lucky_flip\" };";

            foreach (var c in Sets)
            {
                var ls = "{";
                int i = 0;
                foreach (var it in c.Value.Keys)
                {
                    ls += it + (i < c.Value.Keys.Count - 1 ? "," : "}");
                    i++;
                }
                lines3 += Environment.NewLine + "\t\t\tSets[\"" + c.Key + "\"] = new List<int>" + ls + ";";
            }

            lines3 += Environment.NewLine +
                      "\t\t}";
            lines3 += Environment.NewLine + "\t}";
            lines3 += Environment.NewLine + "}";

            File.WriteAllText("test.txt", lines3);

            Console.ReadKey();
        }


        public static Dictionary<int, String> ItemProcs = new Dictionary<int, String>();


        public static void tt()
        {

            var fn = @"..\..\sc_spell_data.inc";
            var s = File.ReadAllText(fn);

            var ret = "";

            Regex db = new Regex("\"(.+?)\".*?,(.+?),.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,.+?,(.+?),");
            Regex ab = new Regex("(\\d+),");
            //{ "Blacksteel Battleplate"                     ,  "tier17lfr",      1,  1245,   17,   2,   1,              {   0 },    0,   73, 179142, { 120396, 120395, 120394, 120393, 120392, 120391,  0 } },
            Regex sb = new Regex("\".+\".*?,.*?\"(.+?)\".+?{.+?}.+?{(.+?)}");

            var lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            int co = 0;

            for (int i = 0; i < lines.Count(); i++)
            {
                if (db.IsMatch(lines[i]))
                {
                    var m = db.Match(lines[i]);
                    //var l = Convert.ToInt32(m.Groups[3].ToString());

                    var l = m.Groups[2].ToString();
                    var _s = new Spell
                    {
                        Gcd = Convert.ToInt32(m.Groups[3].ToString()),
                        Id = Convert.ToInt32(m.Groups[2].ToString()),
                        Name = m.Groups[1].ToString().Trim(),
                        Token = Tokenize(m.Groups[1].ToString())
                    };
                    stringsById[_s.Id] = _s;
                    if (Spells.ContainsKey(_s.Token) && !_s.Name.Equals(Spells[_s.Token].Name))
                    {
                        Console.WriteLine("Mismatch: " + _s.Token + " 1: " + Spells[_s.Token].Name + " 2: " + _s.Name +
                                          " " + i + "/" + lines.Length);
                        var key = ConsoleKey.D1;
                        if (key == ConsoleKey.D2)
                        {
                            Spells[_s.Token] = _s;
                        }
                    }
                    else
                    {
                        Spells[_s.Token] = _s;
                    }
                }

            }

            fn = @"..\..\sc_item_data.inc";
            s = File.ReadAllText(fn);

            Regex item = new Regex("(\\d+),.*?\"(.+?)\".*?,.*?,.*?,.*?,.*?(\\d+).*?,.+?{.+?{.+?{.+?{.+?{.+?{.*?(\\d+)");

            lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var l in lines)
            {
                if (item.IsMatch(l))
                {
                    var procid = Convert.ToInt32(item.Match(l).Groups[4].ToString().Trim());
                    var ilvl = Convert.ToInt32(item.Match(l).Groups[3].ToString());
                    var iid = Convert.ToInt32(item.Match(l).Groups[1].ToString());
                    var name = item.Match(l).Groups[2].ToString();

                    if (procid != 0 && ilvl > 600)
                    {
                        if (stringsById.ContainsKey(procid))
                            Console.WriteLine(name + " " + ilvl + " " + procid + " " + iid + " " + stringsById[procid].Token);
                        
                    }

                }
            }
        }

    }
}
