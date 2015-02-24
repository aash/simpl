using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Microsoft.CSharp;
using Styx.Common;
using System.Windows.Media;

namespace Simcraft.APL
{
    public static class RuntimeCodeCompiler
    {
        private static volatile Dictionary<string, Assembly> cache = new Dictionary<string, Assembly>();
        private static object syncRoot = new object();
        static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        static RuntimeCodeCompiler()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assembly assembly = null;
                assemblies.TryGetValue(e.Name, out assembly);
                return assembly;
            };

        }


        public static Assembly CompileCode(string code)
        {

            Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
#pragma warning disable 618
            ICodeCompiler compiler = provider.CreateCompiler();
#pragma warning restore 618
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = false;



            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string location = assembly.Location;
                    if (!String.IsNullOrEmpty(location))
                    {
                        compilerparams.ReferencedAssemblies.Add(location);
                    }
                }
                catch (NotSupportedException)
                {
                    // this happens for dynamic assemblies, so just ignore it. 
                }
            }


            CompilerResults results =
               compiler.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    SimcraftImpl.Write(String.Format("Line {0},{1}\t: {2} - line {3}\n",error.Line, error.Column, error.ErrorText, error), default(Color), LogLevel.Normal);
                    //Console.WriteLine(code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[error.Line-1]);
                    //errors.AppendFormat("Line {0},{1}\t: {2} - line {3}\n",
                           //error.Line, error.Column, error.ErrorText, error, code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[error.Line]);
                }
                throw new Exception(errors.ToString());
            }
            else
            {

                AppDomain.CurrentDomain.Load(results.CompiledAssembly.GetName());
                return results.CompiledAssembly;
            }

        }

        public static Assembly CompileCodeOrGetFromCache(string code, string key)
        {
            bool exists = cache.ContainsKey(key);

            if (!exists)
            {

                lock (syncRoot)
                {
                    exists = cache.ContainsKey(key);

                    if (!exists)
                    {
                        cache.Add(key, CompileCode(code));
                    }
                }
            }

            return cache[key];
        }


    }
}
