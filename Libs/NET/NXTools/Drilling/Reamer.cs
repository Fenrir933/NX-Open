using NXOpen.CAM;
using NXTools.General;

namespace NXTools.Drilling {
    public class Reamer : BaseTool {

        /// <summary>
        /// Instanziert eine neue Reibahle.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public Reamer(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Drill, parentGroup) { }

        public override void Create() {
            DrillReamerToolBuilder toolBuilder = (DrillReamerToolBuilder)CreateToolBuilder();

            if (toolBuilder != null) {
                toolBuilder.TlShankDiaBuilder.Value = Diameter;
                toolBuilder.TlNumFlutesBuilder.Value = 1;
                toolBuilder.TlTipLengthBuilder.Value = 0;

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }
    }
}
