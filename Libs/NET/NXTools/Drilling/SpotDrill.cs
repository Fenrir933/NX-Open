using NXOpen.CAM;
using NXTools.General;
using NXWrapper;
using System;

namespace NXTools.Drilling {
    public class SpotDrill : BaseTool {

        #region Properties

        // Spitzenwinkel wird auf gültige Eingabe geprüft
        private double _angel;
        public double Angel {
            get => _angel;
            private set {
                if (value < 0 || value >= 180) {
                    _angel = 90;
                    NX.ShowWarningBox($"Spitzenwinkel: {value} war ungültig.\nWurde auf 90 festgelegt!");
                }
                else
                    _angel = value;
            }
        }

        #endregion

        /// <summary>
        /// Instanziert einen neuen Anbohrer.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public SpotDrill(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Drill, parentGroup) {
            Angel = toolEntry.Angel;
        }

        public override void Create() {
            DrillSpotdrillToolBuilder toolBuilder = (DrillSpotdrillToolBuilder)CreateToolBuilder();

            if (toolBuilder != null) {
                toolBuilder.TlNumFlutesBuilder.Value = 1;
                toolBuilder.TlPointAngBuilder.Value = Angel;
                toolBuilder.TlFluteLnBuilder.Value = 0;

                if (Math.Round(Diameter / 2 / Math.Tan(Math.PI / 180 * (Angel / 2)), 3) > Length) {
                    double chamferLength = Length / 3;
                    toolBuilder.TlPointLengthBuilder.Value = chamferLength;
                    NX.ShowWarningBox($"Die Fasenhöhe hat mit dem Spitzenwinkel:" +
                        $"{Angel} die Gesamtlänge überschritten.\nFasenhöhe wurde auf {chamferLength} festgelegt!");
                }

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }
    }
}
