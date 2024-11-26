using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using NXWrapper.Enumerations.CAM;
using System;

namespace ReferenceCopy {
    public class Startup {

        /// <summary>
        /// Einstiegspunkt
        /// </summary>
        public static void Main(string[] args) {
            NCGroup mcsMain = NX.CAM.Find("MCS_MAIN");
            NXMatrix mainOrientation = null;
            CAMObject original = null;
            bool orient = false;

            if (args.Length > 0)
                orient = args[0] == "MCS";

            // sucht MCS_MAIN Objekt und speichert dessen Orientierung
            if (orient)
                try {
                    var mcsMainBuilder = NX.CAM.Collection.CreateMillOrientGeomBuilder(mcsMain);
                    mainOrientation = mcsMainBuilder.Mcs.Orientation;
                }
                catch (NXException) {
                    NX.ShowErrorBox("MCS_MAIN nicht gefunden!");
                    Environment.Exit(1);
                }

            // prüft ob das MCS_MAIN weniger als 1 Objekt enthält
            CAMObject[] mainChilds = mcsMain.GetMembers();
            if (mainChilds.Length <= 1) {
                NX.ShowErrorBox("Keine Objekte gefunden denen eine Referenzkopie zugeordnet werden kann!");
                System.Windows.Application.Current.Shutdown(1);
            }

            /// iteriert über alle Elemente des MCS_MAIN.
            /// Wenn das erste WORKPIECE gefunden wurde wird über alle Elemente des WORKPIECE iteriert und abgebrochen wenn das erste MCS gefunden wurde,
            /// wird dieses als Original-Objekt benutzt und ausgerichtet.
            foreach (var element in mainChilds) {
                var type = NX.GetNXType(element);
                if (type.Subtype == (int)GeometrySubtypes.WORKPIECE) {
                    NCGroup workpiece = element as NCGroup;

                    foreach (var mcs in workpiece.GetMembers()) {
                        type = NX.GetNXType(mcs);
                        if (type.Subtype == (int)GeometrySubtypes.MCS) {
                            original = mcs;

                            // richtet das MCS zusätzlich noch nach dem MCS_MAIN aus
                            if (mainOrientation != null)
                                OrientMCS(original, mainOrientation);

                            break;
                        }
                    }
                    break;
                }
            }

            if (original != null) {
                NX.SetUndoMark("Mit Referenz kopieren");
                // buffert die original-Objekte im Zwischenspeicher
                NX.CAM.Setup.BufferObjects(CAMSetup.View.Geometry, new CAMObject[] { original });

                /// iteriert über alle Objekte im MCS_MAIN, angefangen beim zweiten
                for (int i = 1; i < mainChilds.Length; i++) {
                    var type = NX.GetNXType(mainChilds[i]);

                    // wenn das Objekt vom Typ WORKPIECE ist...
                    if (type.Subtype == (int)GeometrySubtypes.WORKPIECE) {
                        NCGroup workpiece = mainChilds[i] as NCGroup;

                        // wird versucht die Originalobjekte als Referenz in das jeweilige WORKPIECE zu kopieren.
                        try {
                            NX.CAM.Setup.CopyObjectsWithReference(CAMSetup.View.Geometry,
                                                                  new CAMObject[] { original },
                                                                  mainChilds[i],
                                                                  CAMSetup.Paste.Inside);
                        }
                        catch (NXException) {
                            NX.ShowErrorBox($"Mit Referenz einfügen bei Objekt {mainChilds[i]} nicht möglich!");
                            Environment.Exit(1);
                        }

                        // wenn eine Orientierung des MAIN_MCS vorhanden ist...
                        if (mainOrientation != null)
                        /// wird über alle direkt untergeordneten Elemente des WORKPIECE iteriert,
                        /// wenn dieses vom SubTyp MCS ist wird es nach der Orientierung des MCS_MAIN ausgerichtet.
                        foreach (var mcsChild in workpiece.GetMembers())
                            if (NX.GetNXType(mcsChild).Subtype == (int)GeometrySubtypes.MCS)
                                    OrientMCS(mcsChild, mainOrientation);
                    }
                }

                NX.ShowInfoBox("Referenzen eingefügt.");
            }
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);

        /// <summary>
        /// Richtet das übergebene <paramref name="mcs"/> nach der <paramref name="orientation"/> aus.
        /// </summary>
        /// <param name="mcs">MCS, welches neu ausgerichtet werden soll.</param>
        /// <param name="orientation">Ausrichtung</param>
        static void OrientMCS(CAMObject mcs, NXMatrix orientation) {
            var mcsOrientBuilder = NX.CAM.Collection.CreateMillOrientGeomBuilder(mcs);
            var origin = mcsOrientBuilder.Mcs.Origin; //Ursprungspunkt
            var cSystem = NX.WorkPart.CoordinateSystems.CreateCoordinateSystem(origin, orientation, true); // neues Koordinatensystem

            mcsOrientBuilder.Mcs = cSystem;
            mcsOrientBuilder.Commit();
            mcsOrientBuilder.Destroy();
        }
    }
}
