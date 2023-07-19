namespace Gasera {
    internal static class Program {
        static void Main(string[] args) {
            string netType = "";
#if NET
            netType = ".NET";
#elif NETCOREAPP
            netType = ".NET Core";
#endif
            GaseraLib.Gasera.PrintWelcome(netType, Environment.Version.ToString());
            GaseraLib.Gasera.Execute(args);
        }
    }
}