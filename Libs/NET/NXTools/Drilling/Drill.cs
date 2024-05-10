using NXOpen.CAM;
using NXTools.General;
using NXWrapper;

namespace NXTools.Drilling {
    public class Drill : BaseTool {

        #region Properties

        // Spitzenwinkel wird auf gültige Eingabe geprüft
        private double _angel;
        public double Angel {
            get => _angel;
            private set {
                if (value < 0 || value > 180) {
                    _angel = 118;
                    NX.ShowWarningBox($"Spitzenwinkel: {value} war ungültig.\nWurde auf {_angel} festgelegt!");
                }
                else
                    _angel = value;
            }
        }

        #endregion

        /// <summary>
        /// Instanziert einen neuen Spiral-/Vollbohrer.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public Drill(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Drill, parentGroup) =>
            Angel = toolEntry.Angel;

        public override void Create() {
            DrillStdToolBuilder toolBuilder = (DrillStdToolBuilder)CreateToolBuilder();

            if (toolBuilder != null) {
                toolBuilder.TlNumFlutesBuilder.Value = 1;
                toolBuilder.TlPointAngBuilder.Value = Angel;

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }

        public override string ToString() =>
            base.ToString() +
            $"Schneidenwinkel: {Angel}\n";
    }
}
