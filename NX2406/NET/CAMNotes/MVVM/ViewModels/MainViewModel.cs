using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using System;
using System.Linq;
using System.Windows.Input;

namespace CAMNotes.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Fields

        #endregion

        #region Properties

        string _note;
        public string Note {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        #endregion

        #region Commands

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #region Methods

        public void CancelCommandExecute(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);
        }

        public void OkCommandExecute(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);
            Selection selection = NX.UI.SelectionManager;

            string[] note = new string[1];
            if (!string.IsNullOrWhiteSpace(Note))
                note[0] = Note.FirstOrDefault() != ';' ? $";{Note}" : Note;
            else
                note[0] = "";

            // iteriert über alle ausgewählten Objekte
            int num = selection.GetNumSelectedObjects();
            if (num == 0) {
                NX.ShowErrorBox("Keine Elemente ausgewählt!");
                return;
            }

            for (int i = 0; i < num; i++) {
                var sel = selection.GetSelectedTaggedObject(i) as CAMObject;
                NXOpen.CAM.Operation op = null;
                try {
                    op = NX.CAM.Operations.FindObject(sel.Name);
                }
                catch (InvalidCastException) {
                    continue;
                }

                var objectNote = NX.CAM.Setup.CreateObjectNotes(op);
                objectNote.Notes.SetText(note);
                objectNote.Commit();
                objectNote.Destroy();
            }
        }

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
            OkCommand = new RelayCommand<object>(OkCommandExecute);
            CancelCommand = new RelayCommand<object>(CancelCommandExecute);
        }
    }
}
