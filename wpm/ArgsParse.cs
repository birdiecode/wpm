using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace wpm
{
    public sealed class ArgsParse
    {
        private string[] args;
        public delegate void Callback(string packageName);

        private Callback CallbackInstall;
        private Callback CallbackFind;

        public ArgsParse(string[] args)
        {
            if (args.Length > 0)
            {
                this.args = args;
            } else {
                Usage();
            }
            
        }

        public void RegistraCallback(string commandName, Callback callback)
        {
            switch (commandName)
            {
                case "install":
                    this.CallbackInstall = callback;
                    return;
                case "find":
                    this.CallbackFind = callback;
                    return;
            }
        }

        public void Parse()
        {
            switch (this.args[0])
            {
                case "help":
                    Help();
                    break;
                case "install":
                    if (this.args.Length > 2)
                        this.CallbackInstall(this.args[1]);
                    else Help();
                    break;
                case "find":
                    if (this.args.Length > 2)
                        this.CallbackFind(this.args[1]);
                    else Help();
                    break;
                default:
                    Help();
                    break;
            }
        }

        public void Help()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            foreach (var attribute in attributes)
            {
                if (attribute is AssemblyCopyrightAttribute copyright)
                {
                    Console.WriteLine(copyright.Copyright);
                }
            }
            Console.WriteLine(
                String.Format(
                    "wpn {0}\n\nUsage:\n  wpm <command> [options]\n\n", version));
            Console.WriteLine("Commands:\n  install    Instaletion package\n  find       Search package\n  help       Help\n");
            Console.WriteLine("wpm home page: <https://github.com/birdiecode/wpm>");
            Environment.Exit(1);
        }

        public void Usage()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            foreach (var attribute in attributes)
            {
                if (attribute is AssemblyCopyrightAttribute copyright)
                {
                    Console.WriteLine(copyright.Copyright);
                }
            }
            Console.WriteLine(String.Format("wpn {0}\n\nUsage:\n  wpm <command> [options]\n\nType \"wpm help\" for more information about the commands.", version));
            Environment.Exit(1);
        }
    }
}
