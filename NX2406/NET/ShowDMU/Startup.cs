using NXWrapper;
using System;
using System.Windows;

namespace ShowDMU {
    public class Startup {

        /// <summary> Einstiegspunkt. </summary>
        public static void Main(string[] args) {

            if (args.Length < 3) {
                NX.ShowErrorBox("Zuwenig Argumente angegeben!\n1. Wurzelkomponente\n2. Controller Komponente\n3. Controller");
                Application.Current.Shutdown(1);
            }

            string _rootComp = args[0];
            string _comp = args[1];
            string _controller = args[2];

            NX.SetUndoMark("Maschine anzeigen");

            NXOpen.DisplayableObject[] displayables = new NXOpen.DisplayableObject[1];

            // Sucht Maschine (Wurzelkomponente) und zeigt diese an
            var dmu = NX.WorkPart.ComponentAssembly.RootComponent.FindObject(_rootComp);
            displayables[0] = (NXOpen.DisplayableObject)dmu;
            NX.Session.DisplayManager.UnblankObjects(displayables);
            NX.WorkPart.ModelingViews.WorkView.FitAfterShowOrHide(NXOpen.View.ShowOrHideType.ShowOnly);


            // Sucht Controller Komponente innerhalb der Maschine und blendet diesen aus
            var comp1 = dmu.FindObject(_comp);
            var controller = comp1.FindObject(_controller);

            displayables[0] = (NXOpen.DisplayableObject)controller;
            NX.Session.DisplayManager.BlankObjects(displayables);
            NX.WorkPart.ModelingViews.WorkView.FitAfterShowOrHide(NXOpen.View.ShowOrHideType.HideOnly);

        }

        /// <summary> Bibliothek wird nach Beendigung sofort entladen. </summary>
        /// <remarks> NX ruft diese Funktion nachdem das Programm beendet wurde. </remarks>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(NXOpen.Session.LibraryUnloadOption.Immediately);
    }
}
