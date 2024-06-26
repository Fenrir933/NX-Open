using NXTools;
using NXTools.Drilling;
using NXTools.General;
using NXTools.Milling;
using NXWrapper;
using NXWrapper.Enumerations.CAM;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Input;

namespace ToolCreator.MVVM.ViewModels {
    public class MainViewModel : ViewModelBase {

        public List<ToolEntry> Tools { get; private set; }
        public ICommand CreateCommand { get; }
        List<int> _settedToolNumbers, _availableNums = new List<int>();

        /// <summary>
        /// Erstellt Werkzeuge und löst das <see cref="OnRequestClose"/> Event aus.
        /// </summary>
        /// <param name="obj"></param>
        public void CreateCommandPressed(object obj) {
            OnRequestClose.Invoke(this, EventArgs.Empty);
            CreateTools();
        }

        /// <summary>
        /// Event feuert wenn Programm schließen angefordert wurde.
        /// </summary>
        public event EventHandler OnRequestClose;

        /// <summary>
        /// Ruft Werkzeuge aus der Datenbank ab und initialisiert Kommando.
        /// </summary>
        public MainViewModel() {
            Tools = GetTools();
            CreateCommand = new RelayCommand<object>(CreateCommandPressed);
            _settedToolNumbers = GetAllSetToolNumbers();

            for (int i = 1; i < 61; i++)
                _availableNums.Add(i);
        }

        /// <summary>
        /// Erstellt alle markierten Werkzeuge.
        /// </summary>
        void CreateTools() {
            NX.SetUndoMark("Werkzeuge erstellen");

            foreach (var entry in Tools) {
                // Quick & Dirty Werkzeugnummern zuweisen
                // TODO - existierendem Werkzeug gleiche Nummer zuweisen
                int num = _availableNums.Except(_settedToolNumbers).Min();
                _settedToolNumbers.Add(num);

                if (entry.Create) {
                    BaseTool tool;
                    entry.Number = num;

                    switch (entry.Type) {
                        case ToolSubTypes.Mill:
                            tool = new EndMill(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.BallMill:
                            tool = new BallMill(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.EndMillCr:
                            tool = new EndMillCr(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.FaceMill:
                            tool = new ChamferMill(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.TwistDrill:
                            tool = new Drill(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.SpotDrill:
                            tool = new SpotDrill(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.Tap:
                            tool = new Tap(entry, Startup.ToolPocket);
                            tool.Create();
                            break;
                        case ToolSubTypes.Reamer:
                            tool = new Reamer(entry, Startup.ToolPocket);
                            tool.Create();
                            break;

                        case ToolSubTypes.EndMill:
                            goto case ToolSubTypes.Mill;
                        case ToolSubTypes.SolidDrill:
                            goto case ToolSubTypes.TwistDrill;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Ruft alle Werkzeuge einer Datenbanktabelle ab.
        /// </summary>
        List<ToolEntry> GetTools() {
            var tools = new List<ToolEntry>();
            var connection = new SQLiteConnection($"Data Source={Startup.DatabasePath}");
            string query = $"SELECT * FROM {Startup.TableName}";

            int register = Startup.TableName.Contains("90") ? 1 : 0 ;

            connection.Open();
            var command = new SQLiteCommand(query, connection);

            var reader = command.ExecuteReader();
            while (reader.Read()) {
                int index = 0;
                tools.Add(new ToolEntry {
                    Number = reader.GetInt32(index++),
                    Type = (ToolSubTypes)reader.GetInt32(index++),
                    Name = reader.GetString(index++),
                    Length = reader.GetDouble(index++),
                    Diameter = reader.GetDouble(index++),
                    InnerDiamaeter = reader.GetDouble(index++),
                    CornerRadius = reader.GetDouble(index++),
                    Angel = reader.GetDouble(index++),
                    Flutes = reader.GetInt32(index++),
                    HolderName = reader.GetString(index),
                    AdjustRegister = register
                });
            }

            reader.Dispose();
            command.Dispose();
            connection.Dispose();

            tools.Sort((x, y) => x.Name.CompareTo(y.Name));
            return tools;
        }

        /// <summary>
        /// Gibt alle benutzten Werkzeugnummer zurück.
        /// </summary>
        /// <returns><![CDATA[List<int>]]></returns>
        List<int> GetAllSetToolNumbers() {
            List<int> toolNumbers = new List<int>();
            foreach (var element in NX.CAM.MachineRoot.GetMembers()) {
                var type = NX.GetNXType(element);
                if (type.Type == (int)MachineTypes.TOOL) {
                    var toolBuilder = NX.CAM.Collection.CreateTToolBuilder(element);
                    toolNumbers.Add(toolBuilder.TlNumberBuilder.Value);
                    toolBuilder.Destroy();
                }
            }

            return toolNumbers;
        }
    }
}
