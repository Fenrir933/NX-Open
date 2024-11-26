using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;

namespace SafeMotion.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Fields

        const string _udeName = "DMG_safe_motion_on_operation";

        #endregion

        #region Properties

        /// <summary> Freifahrposition X </summary
        private string _homeX = Startup.HOME_X;
        public string HomeX {
            get => _homeX;
            set => SetProperty(ref _homeX, value);
        }

        /// <summary> Freifahrposition Y </summary>
        private string _homeY = Startup.HOME_Y;
        public string HomeY {
            get => _homeY;
            set => SetProperty(ref _homeY, value);
        }

        /// <summary> Freifahrposition Z </summary>
        private string _homeZ = Startup.HOME_Z;
        public string HomeZ {
            get => _homeZ;
            set => SetProperty(ref _homeZ, value);
        }

        /// <summary> 1ter Verfahrweg </summary>
        private ListCollectionView _moveFirst = new ListCollectionView(Startup.FIRST);
        public ListCollectionView MoveFirst {
            get => _moveFirst;
            set => SetProperty(ref _moveFirst, value);
        }

        /// <summary> 2ter Verfahrweg </summary>
        private ListCollectionView _moveSecond = new ListCollectionView(Startup.SECOND);
        public ListCollectionView MoveSecond {
            get => _moveSecond;
            set => SetProperty(ref _moveSecond, value);
        }

        /// <summary> 3ter Verfahrweg </summary>
        private ListCollectionView _moveThird = new ListCollectionView(Startup.THIRD);
        public ListCollectionView MoveThird {
            get => _moveThird;
            set => SetProperty(ref _moveThird, value);
        }

        /// <summary> Drehachsen </summary>
        private ListCollectionView _moveRotary = new ListCollectionView(Startup.ROTARY);
        public ListCollectionView MoveRotary {
            get => _moveRotary;
            set => SetProperty(ref _moveRotary, value);
        }

        #endregion

        #region Commands

        /// <summary> OK </summary>
        public ICommand OKCommand { get; }

        /// <summary> Abbrechen </summary>
        public ICommand CancelCommand { get; }

        #region Methods

        public void OKCommandExecute(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);

            Selection selection = NX.UI.SelectionManager;
            List<CAMObject> selected = new List<CAMObject>();

            // erstellt eine Liste mit allen ausgewählten Objekten und prüft ob diese eine Operation oder Werkzeug sind,
            // andernfalls wird die Funktion abgebrochen
            int num = selection.GetNumSelectedObjects();
            if (num == 0) {
                NX.ShowErrorBox("Keine Elemente ausgewählt!");
                return;
            }

            for (int i = 0; i < num; i++) {
                var sel = selection.GetSelectedTaggedObject(i) as NXObject;
                try {

                    NXOpen.CAM.Operation op = null;
                    try {
                        op = NX.CAM.Operations.FindObject(sel.Name);
                        selected.Add(op);
                        continue;
                    }
                    catch (InvalidCastException) {}

                    var tool = NX.CAM.Find(sel.Name);
                    if (tool is Tool)
                        selected.Add(tool);
                    else {
                        NX.ShowErrorBox($"Ausgewähltes Objekt {sel.Name} ist keine Operation oder Werkzeug!");
                        return;
                    }
                }
                catch (Exception ex) {
                    NX.ShowErrorBox($"{ex.Message}");
                    return;
                }
            }

            NX.SetUndoMark("SafeMotionOnOperation Endereignisse");

            foreach (var sel in selected) {
                CAMObject[] objects = new CAMObject[] { sel };
                var udeSet = NX.CAM.Setup.CreateObjectsUdeSet(objects, CAMSetup.Ude.End);
                var ude = udeSet.UdeSet.CreateUdeByName(_udeName);

                // falls dieses UDE bereits vorhanden ist, wird es gelöscht
                foreach (var item in udeSet.UdeSet.UdeList.GetContents()) {
                    if (item.UdeName == _udeName)
                        udeSet.UdeSet.UdeList.Erase(item, ObjectList.DeleteOption.Delete);
                }

                var homeX = ude.GetParameter("home_smoo_position_x");
                var homeY = ude.GetParameter("home_smoo_position_y");
                var homeZ = ude.GetParameter("home_smoo_position_z");
                var moveFirst = ude.GetParameter("safe_motion_smoo_first");
                var moveSecond = ude.GetParameter("safe_motion_smoo_second");
                var moveThird = ude.GetParameter("safe_motion_smoo_third");
                var moveRotary = ude.GetParameter("safe_motion_smoo_rotary");

                homeX.DoubleValue = double.Parse(HomeX);
                homeY.DoubleValue = double.Parse(HomeY);
                homeZ.DoubleValue = double.Parse(HomeZ);
                moveFirst.OptionText = MoveFirst.CurrentItem.ToString();
                moveSecond.OptionText = MoveSecond.CurrentItem.ToString();
                moveThird.OptionText = MoveThird.CurrentItem.ToString();

                string rotary = MoveRotary.CurrentItem.ToString();
                switch (rotary) {
                    case "Beide":
                        rotary = "Both";
                        break;
                    case "B":
                        rotary = "Fourth";
                        break;
                    case "C":
                        rotary = "Fifth";
                        break;
                    default:
                        break;
                }

                moveRotary.OptionText = rotary;

                udeSet.UdeSet.UdeList.Append(ude);
                udeSet.Commit();
                udeSet.Destroy();
            }
        }

        public void CancelCommandExecute(object obj) => OnRequestClose.Invoke(this, EventArgs.Empty);

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event feuert wenn das Fenster geschlossen werden soll.
        /// Muss vom zugehörigen Window abonniert werden.
        /// </summary>
        public event EventHandler OnRequestClose;

        #endregion

        /// <summary>
        /// Instanziert ein neues <see cref="MainViewModel"/> Objekt.
        /// </summary>
        public MainViewModel() {
            OKCommand = new RelayCommand<object>(OKCommandExecute);
            CancelCommand = new RelayCommand<object>(CancelCommandExecute);

            MoveFirst.MoveCurrentTo(Startup.MOVE_FIRST);
            MoveSecond.MoveCurrentTo(Startup.MOVE_SECOND);
            MoveThird.MoveCurrentTo(Startup.MOVE_THIRD);
            MoveRotary.MoveCurrentTo(Startup.MOVE_ROTARY);
        }
    }
}