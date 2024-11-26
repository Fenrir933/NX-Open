using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ToolCreator {
    public class Startup {

        public static NCGroup ToolPocket;
        public static string Pocket, DatabasePath, TableName;
        public static List<string> HolderNames {  get; private set; }

        /// <summary>
        /// Einstiegspunkt
        /// Registriert das MainWindow in der Application-Klasse und startet diese.
        /// </summary>
        public static void Main(string[] args) {
            if (args.Length == 4)
                HolderNames = new List<string>(File.ReadLines(args[3]));
            else if (args.Length != 3) {
                NX.ShowErrorBox($"Anzahl der Argumente stimmt nicht!\nSiehe README.md");
                return;
            }

            DatabasePath = args[0];
            TableName = args[1];
            Pocket = args[2];

            try {
                ToolPocket = NX.CAM.Find(Pocket);
            }
            catch (NXException) {
                NX.ShowErrorBox($"Keine Werkzeugaufnahme mit dem Name: {Pocket} gefunden!");
                return;
            }

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Application application = new Application {
                StartupUri = new Uri($"/{assemblyName};component/MVVM/Views/MainWindow.xaml", UriKind.Relative)
            };

            application.Run();
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
    }
}
