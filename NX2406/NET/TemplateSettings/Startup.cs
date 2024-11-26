using NXOpen;
using NXWrapper;
using System;
using System.Linq;

namespace TemplateSettings {
    public class Startup {
        /// <summary>
        /// Einstiegspunkt
        /// </summary>
        public static void Main(string[] args) {
            var program = NX.CAM.GetNCGroupMembers(NX.CAM.ProgramRoot, true);
            var geometry = NX.CAM.GetNCGroupMembers(NX.CAM.GeometryRoot, true);
            var machine = NX.CAM.GetNCGroupMembers(NX.CAM.MachineRoot, true);
            var method = NX.CAM.GetNCGroupMembers(NX.CAM.MethodRoot, true);

            program.Append(NX.CAM.ProgramRoot);
            geometry.Append(NX.CAM.GeometryRoot);
            machine.Append(NX.CAM.MachineRoot);
            method.Append(NX.CAM.MethodRoot);

            /// setzt die Vorlageneinstellung auf allen Objekten der root_objects & cam_objects
            /// Objekt kann als Vorlage verwendet werden
            /// Mit übergeordnetem Teil erzeugen
            NX.SetUndoMark("Templatevoreinstellungen aktivieren.");
            NX.CAM.Setup.SetTemplateStatus(program.ToArray(), true, true);
            NX.CAM.Setup.SetTemplateStatus(geometry.ToArray(), true, true);
            NX.CAM.Setup.SetTemplateStatus(machine.ToArray(), true, true);
            NX.CAM.Setup.SetTemplateStatus(method.ToArray(), true, true);
            NX.ShowInfoBox("Vorlageneinstellungen aktiviert.");
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
    }
}
