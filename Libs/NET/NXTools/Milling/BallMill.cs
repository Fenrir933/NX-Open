using NXOpen.CAM;
using NXTools.General;

namespace NXTools.Milling {
    public class BallMill : BaseTool {

        /// <summary>
        /// Instanziert einen neuen Kugelfräser.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public BallMill(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Mill, parentGroup) { }
    }
}
