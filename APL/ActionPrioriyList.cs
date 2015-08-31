using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using Styx;
using Styx.Common;
using Styx.WoWInternals;
using ModifierKeys = Styx.Common.ModifierKeys;

namespace Simcraft.APL
{

    public class APLHotkey
    {
        public Keys Key;
        public ModifierKeys Mod;
        public String Name;
    }

    public class ActionPrioriyList
    {
        private List<AplAction> Actions { get; set; }
        public Dictionary<string, EquippedItem> Items { get; set; } 

        public String Name { get; set; }
        public WoWClass Class { get; set; }
        public String Spec { get; set; }

        public String myCode = "";

        public List<APLHotkey> hotkeys = new List<APLHotkey>();

        public ActionPrioriyList()
        {
            Actions = new List<AplAction>();//ParseActions(s);
            Items = new Dictionary<string, EquippedItem>();//ParseItems(s);
        }

        private static String[] Classes = new[] { "death_knight", "druid", "mage", "hunter", "monk", "paladin", "priest", "rogue", "shaman", "warlock", "warrior" };
        private static Regex class_regex = new Regex("(deathknight|druid|mage|hunter|monk|paladin|priest|rogue|shaman|warlock|warrior)=\"(.+)\"");
        private static Regex spec_regex = new Regex("spec=([a-z_]+)");
        private static Regex items = new Regex("(.+)=([a-z_]+),id=(\\d+)");

        public bool SetAplHeader(String text)
        {
            Class = ParseClass(class_regex.Match(text).Groups[1].ToString());
            Name = class_regex.Match(text).Groups[2].ToString();
            Spec = spec_regex.Match(text).Groups[1].ToString();
            return Class == StyxWoW.Me.Class;
        }

        public static ActionPrioriyList FromString(String s)
        {

            var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            //if (lines.Length < 2)
            //    lines = s.Split(new string[] { "\r" }, StringSplitOptions.None);

            var apl = new ActionPrioriyList();
            if (!apl.SetAplHeader(s)) return apl;
            foreach (var l in lines)
            {
                var expr = ParseLine(l,apl);
                if (expr is AplAction) apl.Actions.Add((AplAction)expr); else
                if (expr is EquippedItem) apl.Items.Add(((EquippedItem)expr).slot,(EquippedItem)expr);
                if (expr is Comment) CommentBuffer.Add(((Comment)expr).Content);
                if (expr is APLHotkey) apl.hotkeys.Add((APLHotkey)expr);
            }

            foreach (var a in apl.Actions)
            {
                a.ParseAction();
            }

            return apl;

        }
        static Regex hotkeyReg = new Regex("hotkeys\\+=/(?<name>[a-zA-Z0-9_]+?),(?<mod>alt|ctrl|shift|none),(?<key>[a-zA-Z0-9_]+?)");
        //hotkey=potion_enabled,alt,e
        private static object ParseLine(String l, ActionPrioriyList a)
        {
            if (l.StartsWith("hotkeys+="))
            {
                var m = hotkeyReg.Match(l);
                APLHotkey ahk = new APLHotkey();
                Keys k;
                Keys.TryParse(m.Groups["key"].ToString(),out  k);
                ahk.Key = k;
                ModifierKeys mk = ModifierKeys.NoRepeat;
                if (m.Groups["mod"].ToString().Equals("alt")) mk = ModifierKeys.Alt;
                if (m.Groups["mod"].ToString().Equals("ctrl")) mk = ModifierKeys.Control;
                if (m.Groups["mod"].ToString().Equals("shift")) mk = ModifierKeys.Shift;
                if (m.Groups["mod"].ToString().Equals("none")) mk = ModifierKeys.NoRepeat;
                ahk.Key = k;
                ahk.Mod = mk;
                ahk.Name = m.Groups["name"].ToString();
                //SimcraftImpl.Write(ahk.Name+" "+ahk.Mod+" "+ahk.Key);
                return ahk;
            }
            if (l.StartsWith("talent_")) return new Comment { Content = l };
            if (l.StartsWith("#")) return new Comment{Content = l};
            if (l.StartsWith("action")) return ParseAction(l,a);
            if (items.IsMatch(l)) return ParseItem(l);
            return default(AplExpr);
        }

        private static AplAction ParseAction(String l, ActionPrioriyList apl)
        {
            return new AplAction(l, CommentBuffer, apl);
        }

        private static EquippedItem ParseItem(String l)
        {
            var m = items.Match(l);
            return new EquippedItem(m.Groups[1].ToString(), m.Groups[2].ToString(), m.Groups[3].ToString());
        }

      
        public static List<String> CommentBuffer = new List<string>();

        public Assembly Assembly { get; set; }

        public void Unload()
        {
            foreach (var hk in hotkeys)
            {
                HotkeysManager.Unregister(hk.Name);
            }
        }

        public void CreateBehavior()
        {




            foreach (var hk in hotkeys)
            {
                HotkeysManager.Register(hk.Name,
                  hk.Key,
                  hk.Mod,
                  _hk =>
                  {
                      SimcraftImpl.inst.toggle_hkvar(hk.Name);
                  });
            }

            if (Assembly == null) throw new Exception(Name + " has not been compiled");
            var mem = Assembly.GetTypes()[0].GetMembers()[0];
            var typ = Assembly.GetTypes()[0];
            //SimcraftImpl.Write("func: " + mem);

            //SimcraftImpl.Write(

            typ.InvokeMember(mem.Name,
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                null, null, new object[0]);

            //PrintResolutionTable();


            //SimcraftImpl.Write(SimcraftImpl.inst.actions.ToString());
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


        public HashSet<String> talents = new HashSet<string>();
        public HashSet<String> buffs = new HashSet<string>();
        public HashSet<String> spells = new HashSet<string>();
        public HashSet<String> debuffs = new HashSet<string>();

        Regex names = new Regex("(debuff|talent|buff|dot|cooldown)\\.([a-z_0-9]+?)\\.");
        Regex debuff = new Regex("(debuff)\\.([a-z_0-9]+?)\\.");
        Regex _string = new Regex("Cast\\(\"([a-z0-9_]+?)\"");


        public void table_print(string format, List<String> list, int columns)
        {

            //string format = "{2,-37} - {1,-37} - {0,-37}";
            //string format2 = "{1,-37} - {0,-37} - {0,-37}";
            //string format2 = "{2,-40} - {1,-40} - {0,-40}";

            

            int lasti = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (i % columns == 0 && i > 0)
                {
                    lasti = i;
                    SimcraftImpl.Write(format, list[i - 2], list[i - 1], list[i]);
                }
                if (i == list.Count - 1 && i != lasti)
                {
                    SimcraftImpl.Write(format+" nvm duplicates", list.Count > 2 ? list[i - 2] : "", list.Count > 2 ? list[i - 1] : "", list[i]);
                    //SimcraftImpl.Write(format, lasti < i - 2 ? list[i - 2] : "",
                    //    lasti < i - 1 ? list[i - 1] : "", lasti < i ? list[i] : "");
                }
            }      
            SimcraftImpl.Write("-------------------------------------------------------------------------");
        }

        public void PrintCode()
        {
            Logging.Write(myCode);
        }

        public void PrintResolutionTable()
        {

            PrintCode();

            List<String> c = new List<string>();

            SimcraftImpl.Write("-------------------------------------------------------------------------");
            foreach (var t in buffs)
            {
                try
                {
                    var _t =
                        SimcNames.buffs[t].FirstOrDefault(
                            ret => ret.V1 == WoWSpec.None || ret.V1 == StyxWoW.Me.Specialization);
                    if (_t == default(SimcNames.SpecPair)) SimcraftImpl.Write("Couldnt find buff: " + t);
                    else
                        SimcraftImpl.Write("Buff: " + t + " id: " + _t.V2);
                }
                catch (Exception)
                {
                    SimcraftImpl.Write("Couldnt find Buff: " + t);
                }

            }
            SimcraftImpl.Write("-------------------------------------------------------------------------");
            c.Clear();
            foreach (var t in debuffs)
            {
                try
                {
                    var _t =
                        SimcNames.debuffs[t].FirstOrDefault(
                            ret => ret.V1 == WoWSpec.None || ret.V1 == StyxWoW.Me.Specialization);
                    if (_t == default(SimcNames.SpecPair)) SimcraftImpl.Write("Couldnt find debuff: " + t);
                    else
                        SimcraftImpl.Write("Debuff: " + t + " id: " + _t.V2);
                }
                catch (Exception)
                {
                    SimcraftImpl.Write("Couldnt find Debuff: " + t);
                }

            }
            SimcraftImpl.Write("-------------------------------------------------------------------------");
            c.Clear();
            foreach (var t in talents)
            {
                try
                {

                    var _t = SimcraftImpl.DBGetClassSpell(t);
                    SimcraftImpl.Write("Talent: " + t +
                                  (StyxWoW.Me.GetLearnedTalents().Count(a => a.Name == _t.name) > 0
                                      ? " - Enabled"
                                      : " - Disabled") + " id: " + _t.id);
                }
                catch (Exception)
                {
                    SimcraftImpl.Write("Couldnt find Talent: " + t);
                }


            }
            SimcraftImpl.Write("-------------------------------------------------------------------------");
            //table_print(format2, c, 3);
            c.Clear();
            foreach (var t in spells)
            {

                try
                {
                    var _t =
                        SimcNames.spells[t].FirstOrDefault(
                            ret => ret.V1 == WoWSpec.None || ret.V1 == StyxWoW.Me.Specialization);
                    if (_t == default(SimcNames.SpecPair))
                    {
                        SimcraftImpl.Write("Couldnt find spell: " + t);
                    }
                    else
                    {
                        //SimcraftImpl.Write("t2: " + _t.V2);
                        var sp = SimcraftImpl.dbc.Spells[_t.V2];
                        
                        var spdb = SimcraftImpl.inst.spell;    
                        sp.token = t;
                        var ct = spdb[sp];
                        /*var ex = spdb[sp].execute_time;
                        var clt = spdb[sp].channel_time;
                        var dur = spdb[sp].duration;
                        var rec = spdb[sp].recharge_time;
                        var ran =  spdb[sp].range;

                        SimcraftImpl.Write("Spell: {0} id: {1} execute_time: " +
                                      "{2} cast_time: {3} channel_time: " +
                                      "{4} duration: {5} recharge_time: " +
                                      "{6} range: {7}", t, sp.id, ex, ct, clt, dur, rec, ran);*/
                    }
                        
                }
                catch (Exception e)
                {
                    SimcraftImpl.Write(e.ToString());
                    SimcraftImpl.Write("Couldnt find SpellEx: " + t);
                }


            }
            SimcraftImpl.Write("-------------------------------------------------------------------------");

            c.Clear();

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
                fullExpression += "\t\tsimc.actions.add(\""+action+"\");" + Environment.NewLine;
            }*/

            foreach (var action in Actions)
            {
                if (!capl.Equals(action.apl))
                {
                    capl = action.apl;
                    code += Environment.NewLine;
                }
                
                    code += action.ToCode("\t\t\t") + Environment.NewLine;

            }
            code += "\t\t\tSimcraftImpl.Write(\"Behaviors created !\");" + Environment.NewLine; ;
            code += "\t\t}" + Environment.NewLine; ;
            code += "\t\t#endregion"+Environment.NewLine;
            code += "\t}" + Environment.NewLine; ;
            code += "}" + Environment.NewLine; ;

            //SimcraftImpl.SimcraftImpl.Write(fullExpression);

            myCode = code;

            talents.Clear();
            buffs.Clear();
            spells.Clear();
            debuffs.Clear();

            foreach (Match m in names.Matches(myCode))
            {
                
                var list = m.Groups[1].ToString();
                var val = m.Groups[2].ToString();

                //SSimcraftImpl.Write(list+" "+val);

                if (list.Equals("talent")) talents.Add(val);
                if (list.Equals("cooldown")) spells.Add(val);
                if (list.Equals("buff")) buffs.Add(val);
                if (list.Equals("debuff")) debuffs.Add(val);
                if (list.Equals("dot")) debuffs.Add(val);
            }

            foreach (Match m in _string.Matches(myCode))
            {
                var list = m.Groups[1].ToString();

                spells.Add(list);
                //if (list.Equals("talent")) talents.Add(val);
            }

            foreach (Match m in debuff.Matches(myCode))
            {
                var list = m.Groups[1].ToString();
                var val = m.Groups[2].ToString();
                debuffs.Add(val);
            }


            //Logging.Write("code: "+code);

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