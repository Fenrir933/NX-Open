using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;

namespace WorkpieceCreator.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Properties

        /// <summary>
        /// WORKPIECE-Zähler
        /// </summary>
        private string _workpieceCounter;
        public string WorkpieceCounter {
            get => _workpieceCounter;
            set => SetProperty(ref _workpieceCounter, value);
        }

        /// <summary>
        /// Name des WORKPIECE.
        /// Ersetzt alle Sonderzeichen und Leerzeichen.
        /// </summary>
        private string _workpieceName = "WERKSTUECK_ROHTEIL";
        public string WorkpieceName {
            get => _workpieceName;
            set {
                value = Regex.Replace(value, @"[äöüß\s]", match => {
                    switch (match.Value) {
                        case "ä":
                            return "ae";
                        case "ö":
                            return "oe";
                        case "ü":
                            return "ue";
                        case "Ä":
                            return "Ae";
                        case "Ö":
                            return "Oe";
                        case "Ü":
                            return "Ue";
                        case "ß":
                            return "ss";
                        case " ":
                            return "_";
                        default:
                            return match.Value;
                    }
                }, RegexOptions.IgnoreCase).ToUpper();
                SetProperty(ref _workpieceName, value);
            }
        }

        /// <summary>
        /// Liste mit allen NCGroup Objekten welchen ein WORKPIECE zugeordnet werden kann.
        /// List with all NCGroup Objects which can be append a Workpiece.
        /// </summary>
        private ListCollectionView _geometryList;
        public ListCollectionView GeometryList {
            get => _geometryList;
            set => SetProperty(ref _geometryList, value);
        }

        #endregion

        #region Commands

        public ICommand Accept { get; }
        public ICommand Cancel { get; }

        #region command methods

        public void AcceptPressed(object obj) {
            string info = CreateWorkpieces();
            NX.ShowInfoBox(info);
            OnRequestClose.Invoke(this, EventArgs.Empty);
        }

        public bool CanAcceptExecute(object obj) =>
            int.TryParse(WorkpieceCounter, out int counter) &&
            !string.IsNullOrWhiteSpace(WorkpieceName) &&
            GeometryList.CurrentItem != null &&
            counter > 0 &&
            counter <= 40;

        public void CancelPressed(object obj) =>
            OnRequestClose.Invoke(this, EventArgs.Empty);

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event feuert wenn Programm schließen angefordert wurde.
        /// </summary>
        public event EventHandler OnRequestClose;

        #endregion

        /// <summary>
        /// Initialisiert Kommandos.
        /// Sucht alle NCGroup Elemente in der Geometrieansicht und fügt diese der <see cref="GeometryList"/> hinzu.
        /// </summary>
        public MainViewModel() {
            Accept = new RelayCommand<object>(AcceptPressed, CanAcceptExecute);
            Cancel = new RelayCommand<object>(CancelPressed);

            List<NCGroup> members = new List<NCGroup>(NX.CAM.GetNCGroupMembers(NX.CAM.GeometryRoot, true));

            // erstellt GeometryList und entfernt None Objekt.
            GeometryList = new ListCollectionView(members.Select(m => m.Name).ToList());
            GeometryList.Remove("NONE");
            GeometryList.MoveCurrentTo("MCS_MAIN");
        }

        /// <summary>
        /// Erstellt die Anzahl an WORKPIECES im gewählten NCGroup Objekt der <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>Anzahl der erstellten Workpieces in gewählten NCGroup Objekt</returns>
        private string CreateWorkpieces() {
            var ncGroup = NX.CAM.Find(GeometryList.CurrentItem.ToString());
            int counter = 1;

            NX.SetUndoMark($"Erzeuge Werkstücke x {WorkpieceCounter}");
            List<NXObject> deleteList = new List<NXObject>();

            for (int i = 0; i < int.Parse(WorkpieceCounter); i++) {
                /// Versucht ein WORKPIECE zu erstellen, falls bereits eines mit dem Name existiert,
                /// wird dem Name der Zählerwert angehangen und nochmal probiert.
                while (true) {
                    try {
                        string workpieceName = WorkpieceName;
                        if (counter > 0)
                            workpieceName = $"{WorkpieceName}_{counter}";

                        var geom = NX.CAM.Collection.CreateGeometry(
                            ncGroup,
                            "mill_planar",
                            "WORKPIECE",
                            NCGroupCollection.UseDefaultName.False,
                            workpieceName);

                        var geomBuilder = NX.CAM.Collection.CreateMillGeomBuilder(geom);
                        geomBuilder.Commit();
                        geomBuilder.Destroy();

                        foreach (var toDelete in geom.GetMembers())
                            deleteList.Add(toDelete);

                        break;
                    }
                    catch (NXException) {
                        counter++;
                    }
                }
            }

            NX.DeleteObjects(deleteList.ToArray());

            return int.Parse(WorkpieceCounter) == 1 ? $"Es wurde {WorkpieceCounter} Werkstück in {ncGroup.Name} erstellt!" :
                                                      $"Es wurden {WorkpieceCounter} Werkstücke in {ncGroup.Name} erstellt!";
        }
    }
}
