using NXOpen.CAM;
using NXTools.General;
using NXWrapper;
using System;
using System.Collections.Generic;

namespace NXTools.Drilling {
    public class Tap : BaseTool {

        // Dictionary hält die zum Durchmesser passende Steigung
        private readonly Dictionary<int, double> _pitches = new Dictionary<int, double> {
            {3, 0.5},
            {4, 0.7},
            {5, 0.8},
            {6, 1.0},
            {8, 1.25},
            {10, 1.5},
            {12, 1.75},
            {14, 2.0},
            {16, 2.0},
            {18, 2.5},
            {20, 2.5},
            {22, 2.5},
            {24, 3.0},
            {27, 3.0},
            {30, 3.5},
            {36, 4.0},
            {42, 4.5},
            {48, 5.0},
            {56, 5.5},
            {64, 6.0}
        };

        /// <summary>
        /// Instanziert einen neuen Gewindebohrer.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public Tap(ToolEntry toolEntry, NCGroup parentGroup) : base(toolEntry, ToolTypes.Drill, parentGroup) { }

        public override void Create() {
            DrillTapToolBuilder toolBuilder = (DrillTapToolBuilder)CreateToolBuilder();
            double pitch;

            try {
                pitch = _pitches[(int)Math.Round(Diameter)];
            }
            catch (KeyNotFoundException) {
                pitch = 1.0;
                NX.ShowWarningBox($"Keine passende Steigung zum Durchmesser: {Diameter} gefunden!\nSteigung wurde auf 1.0 festgelegt!");
            }

            // Flankenhöhe entspricht im metrischen Gewinde 61.35% der Gewindesteigung.
            // wird mit 63% berechnet um Kollisionen bei der Simulation mit dem Schaft zu verhindern.
            double threadHeight = pitch * 0.63;

            if (toolBuilder != null) {
                toolBuilder.TlShankDiaBuilder.Value = Diameter - 2 * threadHeight;
                toolBuilder.TlTipDiameterBuilder.Value = Diameter;
                toolBuilder.TlIncludedAngBuilder.Value = 90;
                toolBuilder.TlNumFlutesBuilder.Value = 1;
                toolBuilder.TlPitchBuilder.Value = pitch;

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }
    }
}
