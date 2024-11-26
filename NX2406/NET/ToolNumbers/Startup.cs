using NXOpen;
using NXOpen.CAM;
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
            foreach (var element in NX.CAM.Find("POCKET").GetMembers()) {
                var type = NX.GetNXType(element);
                if (type.Type == (int)MachineTypes.TOOL) {
                    Tool tool = (Tool)element;
                    NX.UFSession.Param.SetIntValue(tool.Tag, 1038, toolNumber);
                    toolNumber++;
                }
            }
        }

        /// <summary> Gibt die gerade aktive NXSession sofort wieder frei. </summary>
        public static int GetUnloadOption(string _) =>
            Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
    }
}
