using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using NXWrapper.Enumerations.CAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MCSNumbers.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Fields

        readonly NCGroup _mainMCS;
        readonly List<string> _templateSubTypes = new List<string>() {
            "MCS_MILL",
            "MCS",
            "WORKPIECE"
        };
        readonly Dictionary<int, int> _offsets = new Dictionary<int, int> {
            { 54, 1 },
            { 55, 2 },
            { 56, 3 },
            { 57, 4 }
        };

        #endregion

        #region Properties

        /// <summary>
        /// Start-Nullpunkt
        /// </summary>
        private string _zeroOffset = "54";
        public string ZeroOffset {
            get => _zeroOffset;
            set => SetProperty(ref _zeroOffset, value);
        }

        #endregion

        #region Commands

        public ICommand Accept { get; }
        public ICommand Cancel { get; }

        #region Methods

        public void AcceptPressed(object obj) {
            NX.SetUndoMark("MCS Objekte umbenennen");
            RenameMCS();
            NX.ShowInfoBox("Nullpunkte angepasst.");
            OnRequestClose.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// kann nur ausgeführt werden wenn sich der Wert zwischen 54 und 57 oder 505 und 598 befindet.
        /// begrenzt auf 598 weil mindestens 2 MCS Objekte bereitstehen müssen und 599 die von Siemens vorgegebene Grenze ist.
        /// </summary>
        public bool CanAcceptExecute(object obj) {
            int.TryParse(ZeroOffset, out int startZero);
            return (startZero >= 54 && startZero <= 57) ||
                   (startZero >= 505 && startZero <= 598);
        }

        public void CancelPressed(object obj) =>
            OnRequestClose.Invoke(this, EventArgs.Empty);

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event feuert wenn das Schließen des Fensters angefordert wurde.
        /// </summary>
        public event EventHandler OnRequestClose;

        #endregion

        /// <summary>
        /// Sucht das MCS_MAIN und initialisiert Kommandos.
        /// </summary>
        public MainViewModel() {
            try {
                _mainMCS = NX.CAM.Find("MCS_MAIN");
            }
            catch (NXException) {
                NX.ShowErrorBox("Kein Objekt mit dem Name MCS_MAIN vorhanden!");
                System.Windows.Application.Current.Shutdown(1);
            }

            Accept = new RelayCommand<object>(AcceptPressed, CanAcceptExecute);
            Cancel = new RelayCommand<object>(CancelPressed);
        }

        /// <summary>
        /// Sucht in jedem WORKPIECE des MCS_MAIN nach dem MCS-Paar 'MCS_GXX' und 'MCS_GXX_CYCLE800'.
        /// Benennt diese um und passt die Nullpunktverschiebung an.
        /// </summary>
        void RenameMCS() {
            int mcsCounter = int.Parse(ZeroOffset), offsetCounter;

            /// zieht den Nullpunkt aus <see cref="_offsets"/> oder nimmt die letzten 2 Ziffern des Startnullpunkt.
            try {
                offsetCounter = _offsets[int.Parse(ZeroOffset)];
            }
            catch (KeyNotFoundException) {
                offsetCounter = int.Parse(ZeroOffset.Substring(1));
            }

            // Erstellt eine Liste mit allen Workpiece Objekten des MCS_MAIN
            IEnumerable<NCGroup> workpieces = new List<NCGroup>();
            foreach (var element in NX.CAM.GetNCGroupMembers(_mainMCS))
                if (CheckWorkpiece(element))
                    workpieces = workpieces.Append(element);

            // Erstellt eine Liste mit allen MCS-Paaren eines Workpiece
            IEnumerable<Tuple<NCGroup, NCGroup>> orients = new List<Tuple<NCGroup, NCGroup>>();
            foreach (var workpiece in workpieces) {
                var mcs = NX.CAM.GetNCGroupMembers(workpiece, true);

                if (mcs.Count() == 2) {
                    if (CheckMCS(mcs.First()) && CheckMCS(mcs.Last()))
                        orients = orients.Append(new Tuple<NCGroup, NCGroup>(mcs.First(), mcs.Last()));
                }
            }

            // alle Namen zurücksetzen
            foreach (var mcsPair in orients) {
                mcsPair.Item1.SetName(Guid.NewGuid().ToString());
                mcsPair.Item2.SetName(Guid.NewGuid().ToString());
            }

            // Alle MCS-Paare umbenennen und Nullpunktverschiebung anpassen
            bool warning = false;
            foreach (var mcsPair in orients) {
                mcsPair.Item1.SetName($"MCS_G{mcsCounter}");
                mcsPair.Item2.SetName($"MCS_G{mcsCounter}_CYCLE800");

                var mcs = NX.CAM.Collection.CreateMillOrientGeomBuilder(mcsPair.Item1);
                mcs.FixtureOffsetBuilder.Value = offsetCounter;
                mcs.Commit();
                mcs.Destroy();

                mcs = NX.CAM.Collection.CreateMillOrientGeomBuilder(mcsPair.Item2);
                mcs.FixtureOffsetBuilder.Value = offsetCounter;
                mcs.Commit();
                mcs.Destroy();

                offsetCounter++;
                mcsCounter++;

                warning = mcsCounter > 599;
                mcsCounter = mcsCounter == 58 ? 505 : mcsCounter;
            }

            if (warning)
                NX.ShowWarningBox("Nullpunktlimit 599 überschritten!");
        }

        /// <summary>
        /// Prüft ob das übergebene <see cref="NXObject"/> vom Typ <see cref="GeometrySubtypes.MCS"/> ist.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        bool CheckMCS(NXObject @object) {
            var type = NX.GetNXType(@object);
            return type.Type == (int)GeometryTypes.GEOMETRY && type.Subtype == (int)GeometrySubtypes.MCS;
        }

        /// <summary>
        /// Prüft ob das übergebene <see cref="NXObject"/> vom Typ <see cref="GeometrySubtypes.WORKPIECE"/> ist.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        bool CheckWorkpiece(NXObject @object) {
            var type = NX.GetNXType(@object);
            return type.Type == (int)GeometryTypes.GEOMETRY && type.Subtype == (int)GeometrySubtypes.WORKPIECE;
        }
    }
}
