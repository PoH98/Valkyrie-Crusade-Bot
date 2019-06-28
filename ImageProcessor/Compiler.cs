using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace BotFramework
{
    /// <summary>
    /// Compile string codes to real coding for run!
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imports">Just input System.Linq without .dll!</param>
        /// <param name="output_filename">Compiled file name</param>
        public static void Compile(string[] imports, string output_filename, string typename)
        {
            var param = new CompilerParameters
            {
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                GenerateInMemory = true
            };
            foreach (var assemblies in imports)
            {
                param.ReferencedAssemblies.Add(assemblies.Replace(".dll","") + ".dll");
            }
            var codeProvider = new CSharpCodeProvider();
            var results = codeProvider.CompileAssemblyFromFile(param, output_filename);

            if (results.Errors.HasErrors)
            {
                foreach (var error in results.Errors)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                object o = results.CompiledAssembly.CreateInstance(typename);
            }
        }
    }
}
