using NXOpen.CAM;
using NXTools.General;

namespace NXTools.Milling {
    public class EndMillCr : BaseTool {

        /// <summary>
        /// Instanziert einen neuen Schaftfräser mit Eckenverrundung.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public EndMillCr(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Mill, parentGroup) { }

        public override void Create() {
            MillToolBuilder toolBuilder = (MillToolBuilder)CreateToolBuilder();

            if (toolBuilder != null) {
                toolBuilder.TlCor1RadBuilder.Value = CornerRadius;

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }
    }
}
