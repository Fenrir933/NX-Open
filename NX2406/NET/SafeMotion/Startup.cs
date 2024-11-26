using NXOpen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace SafeMotion {
    public class Startup {

        public static List<string> FIRST    = new List<string>() { "KEIN", "XYZ", "XY", "YZ", "ZX", "X", "Y", "Z" };
        public static List<string> SECOND   = new List<string>() { "KEIN", "XY", "YZ", "ZX", "X", "Y", "Z" };
        public static List<string> THIRD    = new List<string>() { "KEIN", "X", "Y", "Z" };
        public static List<string> ROTARY   = new List<string>() { "KEIN", "Beide", "B", "C" };

        public static string HOME_X         = "0";
        public static string HOME_Y         = "0";
        public static string HOME_Z         = "0";
        public static string MOVE_FIRST     = "Z";
        public static string MOVE_SECOND    = "Y";
        public static string MOVE_THIRD     = "KEIN";
        public static string MOVE_ROTARY    = "KEIN";

        /// <summary> Einstiegspunkt </summary>
        public static void Main(string[] args) {
            try {
                HOME_X = CheckHomePosition(args[0]);
                HOME_Y = CheckHomePosition(args[1]);
                HOME_Z = CheckHomePosition(args[2]);
                MOVE_FIRST  = !FIRST .Contains(args[3].ToUpper()) ? "KEIN" : args[3];
                MOVE_SECOND = !SECOND.Contains(args[4].ToUpper()) ? "KEIN" : args[4];
                MOVE_THIRD  = !THIRD .Contains(args[5].ToUpper()) ? "KEIN" : args[5];
                MOVE_ROTARY = !ROTARY.Contains(args[6].ToUpper()) ? "KEIN" : args[6];
            }
            catch (IndexOutOfRangeException) {}


            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Application application = new Application {
                StartupUri = new Uri($"/{assemblyName};component/MVVM/Views/MainWindow.xaml", UriKind.Relative)
            };

            application.Run();
        }

        /// <summary> Bibliothek wird nach Beendigung sofort entladen. </summary>
        /// <remarks> NX ruft diese Funktion nachdem das Programm beendet wurde. </remarks>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);

        /// <summary>
        /// Prüft Position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static string CheckHomePosition(string pos) {
            if (double.TryParse(pos, out double value))
                value = value > 0 ? 0 : value < -1000 ? -1000 : value;

            return value.ToString();
        }
    }
}