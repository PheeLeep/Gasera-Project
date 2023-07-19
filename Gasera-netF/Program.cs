using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gasera_netF {
    internal static class Program {
        static void Main(string[] args) {
            GaseraLib.Gasera.PrintWelcome(".NET Framework", Environment.Version.ToString());
            GaseraLib.Gasera.Execute(args);
        }
    }
}
