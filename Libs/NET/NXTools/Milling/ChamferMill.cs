using NXOpen.CAM;
using NXTools.General;
using NXWrapper;
using System;

namespace NXTools.Milling {
    public class ChamferMill : BaseTool {

        #region Properties

        // Spitzenwinkel und unterer Durchmesser wird auf gültige Eingabe geprüft
        private double _angel;
        public double Angel {
            get => _angel;
            private set {
                if (value < 0 || value > 180) {
                    _angel = 45;
                    NX.ShowWarningBox($"Spitzenwinkel: {value} war ungültig.\nSchneidenwinkel wurde auf {_angel} festgelegt!");
                }
                else {
                    _angel = value / 2;
                }
            }
        }

        private double _innerDiameter;
        public double InnerDiameter {
            get => _innerDiameter;
            private set {
                if (value < 0 || value >= Diameter) {
                    _innerDiameter = 0.0;
                    NX.ShowWarningBox($"Unterer Durchmesser: {value} war ungültig.\nWurde auf {_innerDiameter} festgelegt!");
                }
                else {
                    _innerDiameter = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Instanziert einen neuen Fasenfräser.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public ChamferMill(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Mill, parentGroup) {
            Angel = toolEntry.Angel;
            InnerDiameter = toolEntry.InnerDiamaeter;
        }

        public override void Create() {
            MillToolBuilder toolBuilder = (MillToolBuilder)CreateToolBuilder();

            double r = Diameter / 2;
            // Umrechnung in Radiant
            double rad = (Angel / 180) * Math.PI;
            // berechnet Radiusdifferenz falls unterer Durchmesser angegeben ist
            r = InnerDiameter != 0 ? r - InnerDiameter / 2 : r;
            double chamferLength = r / Math.Tan(rad);

            if (toolBuilder != null) {
                toolBuilder.TlCor1RadBuilder.Value = CornerRadius;
                toolBuilder.TlTaperAngBuilder.Value = Angel;
                toolBuilder.ChamferLengthBuilder.Value = chamferLength;
                toolBuilder.TlFluteLnBuilder.Value = chamferLength;

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }

        public override string ToString() =>
            base.ToString() +
            $"Schneidenwinkel: {Angel}\n" +
            $"Unterer Durchmesser: {InnerDiameter}\n";
    }
}
