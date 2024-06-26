using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using NXWrapper;

namespace ProgramOrder.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Fields

        readonly List<NXOpen.CAM.Operation> _operations = new List<NXOpen.CAM.Operation>();
        readonly HashSet<string> _uniqueOperations = new HashSet<string>();
        readonly Dictionary<string, HashSet<string>> _operationsByTool = new Dictionary<string, HashSet<string>>();

        #endregion

        #region Properties

        public string ToolTooltip =
            "Alle Operationen des ausgewählten Werkzeugs werden verschoben.";

        public string SortOrderTooltip =
            "MCS zuerst: Alle Operationen nacheinander an jedem MCS bearbeiten.";

        public string OperationTooltip =
            "Vor: Fügt alle Operationen vor der gewählten, zuerst gefundenen Operation ein.\n" +
            "Nach: Fügt alle Operationen nach der gewählten, zuletzt gefundenen Operation ein.";

        ListCollectionView _tools;
        public ListCollectionView Tools {
            get => _tools;
            set {
                if (_tools != value)
                    SetProperty(ref _tools, value);
            }
        }

        ListCollectionView _operationsView;
        public ListCollectionView Operations {
            get => _operationsView;
            set {
                if (_operationsView != value)
                    SetProperty(ref _operationsView, value);
            }
        }

        ListCollectionView _excludeOperationsView;
        public ListCollectionView ExcludeOperations {
            get => _excludeOperationsView;
            set {
                if (_excludeOperationsView != value)
                    SetProperty(ref _excludeOperationsView, value);
            }
        }

        bool _before = true;
        public bool Before {
            get => _before;
            set {
                if (_before != value)
                    SetProperty(ref _before, value);
            }
        }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        #region Methods

        public void OKCommandExecute(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);
            SortSelectedOperations();
        }

        public void CancelCommandExecute(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);
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
            GetUniqueOperations();
            GetMultiOperationsSingleTool();

            OKCommand = new RelayCommand<object>(OKCommandExecute);
            CancelCommand = new RelayCommand<object>(CancelCommandExecute);
            Tools = new ListCollectionView(GetToolsMultiOP());

            SetOperations();

            Tools.CurrentChanged += (_, e) => { SetOperations(); };
        }

        /// <summary>
        /// Fügt alle einzigartigen Operationsnamen dem <see cref="_uniqueOperationNames"/> HashSet &
        /// der <see cref="_operationNames"/> Liste alle Operationen in aktueller Reihenfolge hinzu.
        /// Im Unique-Name wird der Kopiezähler "_123" am Ende des Namen entfernt.
        /// </summary>
        void GetUniqueOperations() {
            foreach (var operation in NX.CAM.Operations) {
                var op = (NXOpen.CAM.Operation)operation;
                var opName = Regex.Replace(op.Name, "_\\d+$", "").ToUpper();
                if (opName != "DOCUMENTATION") {
                    _uniqueOperations.Add(opName);
                    _operations.Add(op);
                }
            }
        }

        /// <summary>
        /// Weist dem Dictionary <see cref="_operationsByTool"/> für jedes Werkzeug ein <see cref="HashSet{T}"/> mit jeder zugehörigen Operation zu.
        /// </summary>
        void GetMultiOperationsSingleTool() {
            foreach (var operation in _uniqueOperations) {
                var op = NX.CAM.Operations.FindObject(operation);
                var tool = op.GetParent(NXOpen.CAM.CAMSetup.View.MachineTool);

                if (_operationsByTool.TryGetValue(tool.Name, out var _))
                    _operationsByTool[tool.Name].Add(op.Name);
                else {
                    var operations = new HashSet<string>() { op.Name };
                    _operationsByTool[tool.Name] = operations;
                }
            }
        }

        /// <summary>
        /// Gibt eine Liste mit Werkzeugen zurück, welche mehr als eine Operation besitzen.
        /// </summary>
        /// <returns><see cref="List{T}"/> mit Werkzeugnamen</returns>
        List<string> GetToolsMultiOP() {
            var toolList = new List<string>();
            foreach (var tool in _operationsByTool)
                if (tool.Value.Count > 1)
                    toolList.Add(tool.Key);

            return toolList;
        }

        /// <summary>
        /// Sortiert die Programmreihenfolge.
        /// </summary>
        void SortSelectedOperations() {
            Dictionary<string, List<NXOpen.CAM.Operation>> operationsByMCS = new Dictionary<string, List<NXOpen.CAM.Operation>>();
            NXOpen.CAM.Operation referenceOperation = null;
            NXOpen.CAM.CAMSetup.Paste paste;
            string tool = Tools.CurrentItem.ToString();
            string excludeOperation = ExcludeOperations.CurrentItem.ToString();

            NX.SetUndoMark("Operationen sortieren");

            /// iteriert über jede Operation des gewählten Werkzeugs und damit über die gesamte Operationsliste
            /// enthält der Name einer Operation den einer Werkzeugoperation wird diese dem nach MCS sortierten Dictionary hinzugefügt
            foreach (var toolOperation in _operationsByTool[tool]) {
                foreach (var operation in _operations) {

                    // schließt gewählte Operationen aus
                    if (excludeOperation != "KEIN" && operation.Name.Contains(excludeOperation))
                        continue;

                    if (operation.Name.Contains(toolOperation)) {
                        string mcs = operation.GetParent(NXOpen.CAM.CAMSetup.View.Geometry).Name;

                        if (operationsByMCS.TryGetValue(mcs, out var _))
                            operationsByMCS[mcs].Add(operation);
                        else {
                            var operations = new List<NXOpen.CAM.Operation>() { operation };
                            operationsByMCS[mcs] = operations;
                        }
                    }
                }
            }

            List<NXOpen.CAM.Operation> orderedOperations = new List<NXOpen.CAM.Operation>();

            foreach (var operation in operationsByMCS.Values)
                orderedOperations.AddRange(operation);

            // holt den Name der gewählten Operation
            string selectedOperation = Operations.CurrentItem.ToString();

            // setzt Einfüge-Option und kehrt die Operationsliste um je nach gewählter Option
            if (Before)
                paste = NXOpen.CAM.CAMSetup.Paste.Before;
            else {
                paste = NXOpen.CAM.CAMSetup.Paste.After;
                _operations.Reverse();
            }

            // iteriert über die Operationsliste und hält bei der ersten gefunden Operation an welche den Namen der gewählten enthält und
            // setzt die Referenzoperation
            foreach (var op in _operations) {
                if (op.Name.Contains(selectedOperation)) {
                    referenceOperation = NX.CAM.Operations.FindObject(op.Name);
                    break;
                }
            }

            // Verschiebt alle gewählten Objekte in der Programmreihenfolge
            if (referenceOperation != null) {
                NX.CAM.Setup.MoveObjects(NXOpen.CAM.CAMSetup.View.ProgramOrder,
                    orderedOperations.ToArray(),
                    referenceOperation,
                    paste);
            }
        }

        /// <summary>
        /// Aktualisiert Operations Combobox Source.
        /// </summary>
        void SetOperations() {
            var operations = _uniqueOperations.ToList();
            var exclude = new List<string> { "KEINE" };
            exclude.AddRange(_operationsByTool[Tools.CurrentItem.ToString()].ToList());

            foreach (var operation in _operationsByTool[Tools.CurrentItem.ToString()])
                operations.Remove(operation);

            operations.Sort();
            Operations = new ListCollectionView(operations);
            ExcludeOperations = new ListCollectionView(exclude);
        }
    }
}
