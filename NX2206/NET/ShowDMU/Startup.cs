using NXWrapper;
using System;

namespace ShowDMU {
    public class Startup {

        /// <summary> Einstiegspunkt. </summary>
        public static void Main(string[] args) {

            NX.SetUndoMark("Maschine anzeigen");

            NXOpen.DisplayableObject[] displayables = new NXOpen.DisplayableObject[1];

            // Sucht Maschine (Wurzelkomponente) und zeigt diese an
            var dmu = NX.WorkPart.ComponentAssembly.RootComponent.FindObject("COMPONENT DMU90P4_dB_12850001913 1");
            displayables[0] = (NXOpen.DisplayableObject)dmu;
            NX.Session.DisplayManager.UnblankObjects(displayables);
            NX.WorkPart.ModelingViews.WorkView.FitAfterShowOrHide(NXOpen.View.ShowOrHideType.ShowOnly);


            // Sucht Controller Komponente innerhalb der Maschine und blendet diesen aus
            var comp1 = dmu.FindObject("COMPONENT 12850001913_ASM 1");
            var controller = comp1.FindObject("COMPONENT 12850001913_Controller 1");

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
