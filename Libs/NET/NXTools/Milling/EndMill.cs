using NXOpen.CAM;
using NXTools.General;

namespace NXTools.Milling {
    public class EndMill : BaseTool {

        /// <summary>
        /// Instanziert einen neuen Schaftfräser.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public EndMill(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Mill, parentGroup) { }
    }
}
