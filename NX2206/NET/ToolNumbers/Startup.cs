using NXOpen;
using NXWrapper;
using NXWrapper.Enumerations.CAM;
using System;

namespace ToolNumbers {
    public class Startup {
        /// <summary>
        /// Einstiegspunkt
        /// </summary>
        public static void Main(string[] args) {
            NX.SetUndoMark("Werkzeuge nummerieren");

            int toolNumber = 1;
            foreach (var element in NX.CAM.MachineRoot.GetMembers()) {
                var type = NX.GetNXType(element);
                if (type.Type == (int)MachineTypes.TOOL) {
                    var toolBuilder = NX.CAM.Collection.CreateTToolBuilder(element);
                    toolBuilder.TlNumberBuilder.Value = toolNumber;
                    toolNumber++;
                    toolBuilder.Commit();
                    toolBuilder.Destroy();
                }
            }

            NX.ShowInfoBox("Werkzeugnummern gesetzt.");
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
    }
}
