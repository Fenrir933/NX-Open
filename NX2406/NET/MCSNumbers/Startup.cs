using System;
using System.Reflection;
using System.Windows;

namespace NXApplication {
    public class Startup {

        /// <summary>
        /// Einstiegspunkt
        /// </summary>
        public static void Main(string[] args) {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Application application = new Application {
                StartupUri = new Uri($"/{assemblyName};component/MVVM/Views/MainWindow.xaml", UriKind.Relative)
            };

            application.Run();
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(NXOpen.Session.LibraryUnloadOption.Immediately);
    }
}
