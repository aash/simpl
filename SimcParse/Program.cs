using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimcParse
{
    class Program
    {
  

        private static void Main(string[] args)
        {

            var fn = @"..\..\Profiles\Erza.simc";
            var s = File.ReadAllText(fn);
            //Console.WriteLine("Spells[\"lucky_flip\"] = new dbc.Spell { Gcd = 0, Id = 177597, Name = \"\\\"Lucky\\\" Flip\", Token = \"lucky_flip\" };");
            ;

            fn = @"..\..\hunter.cs";
            File.WriteAllText(fn, new ActionPrioriyList(s).ToCode());

            Console.ReadKey();
            //dbc.DumpDB();
        }


    }
}
