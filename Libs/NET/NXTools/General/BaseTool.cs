using NXOpen;
using NXOpen.CAM;
using NXWrapper;
using System.Text.RegularExpressions;

namespace NXTools.General {
    /// <summary>
    /// Standard Werkzeug-Implementierung.
    /// 
    /// <para>Das ist eine abstrakte Klasse und kann nicht instanziert werden.</para>
    /// </summary>
    public abstract class BaseTool {

        #region Fields

        readonly ToolTypes _toolType;
        readonly ToolSubTypes _subType;
        readonly NCGroup _parentGroup;
        bool _toolExists;
        readonly string _holderName;

        #endregion

        #region Properties

        public int Number { get; }

        public int AdjustRegister { get; }

        // ersetzt alle Sonderzeichen, Leerzeichen und transformiert alles in Großbuchstaben
        string _name;
        public string Name {
            get => _name;
            private set {
                value = Regex.Replace(value, @"[äöüÄÖÜß\s]", match => {
                    switch (match.Value) {
                        case "ä":
                            return "ae";
                        case "ö":
                            return "oe";
                        case "ü":
                            return "ue";
                        case "Ä":
                            goto case "ä";
                        case "Ö":
                            goto case "ö";
                        case "Ü":
                            goto case "ü";
                        case "ß":
                            return "ss";
                        case " ":
                            return "_";
                        default:
                            return match.Value;
                    }
                }).ToUpper();

                _name = value;
            }
        }

        // Länge, Durchmesser, Schneidenanzahl, Schneidenlänge und Eckenradius werden auf gültige Eingaben geprüft.
        double _length;
        public double Length {
            get => _length;
            private set {
                if (_length != value) {
                    if (value <= 0.0) {
                        _length = 70;
                        NX.ShowWarningBox($"Werkzeuglänge: {value} war ungültig.\nWurde auf {_length} festgelegt!");
                    }
                    else {
                        _length = value;
                    }
                }
            }
        }

        double _diameter;
        public double Diameter {
            get => _diameter;
            private set {
                if (_diameter != value) {
                    if (value <= 0.0) {
                        _diameter = 30;
                        NX.ShowWarningBox($"Werkzeugdurchmesser: {value} war ungültig.\nWurde auf {_diameter} festgelegt!");
                    }
                    else {
                        _diameter = value;
                    }
                }
            }
        }

        int _flutes;
        public int Flutes {
            get => _flutes;
            private set {
                if (_flutes != value) {
                    if (value <= 0) {
                        _flutes = 2;
                        NX.ShowWarningBox($"Schneidenanzahl: {value} war ungültig.\nWurde auf {_flutes} festgelegt!");
                    }
                    else {
                        _flutes = value;
                    }
                }
            }
        }

        double _fluteLength;
        public double FluteLength {
            get => _fluteLength;
            private set {
                if (_fluteLength != value) {
                    if (value > Length) {
                        _fluteLength = Length / 2;
                        NX.ShowWarningBox($"Schneidenlänge: {value} war größer als Werkzeuglänge.\nWurde auf {_fluteLength} festgelegt!");
                    }
                    else {
                        _fluteLength = value;
                    }
                }
            }
        }

        double _cornerRadius;
        public double CornerRadius {
            get => _cornerRadius;
            private set {
                if (_cornerRadius != value && (value < 0.0 || value > Diameter / 2)) {
                    _cornerRadius = 0;
                    NX.ShowWarningBox($"Eckenradius: {value} war ungültig.\nWurde auf {_cornerRadius} festgelegt!");
                }
                else {
                    _cornerRadius = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Standard Werkzeug-Initialisierung.
        /// </summary>
        /// <param name="toolEntry">Datenbankeintrag des Werkzeug</param>
        /// <param name="toolType">Werkzeugtyp</param>
        /// <param name="parentGroup">Übergeordnete <see cref="NCGroup"/> in welcher das Werkzeug erstellt wird</param>
        public BaseTool(ToolEntry toolEntry, ToolTypes toolType, NCGroup parentGroup) {
            _toolType = toolType;
            _subType = toolEntry.Type;
            _parentGroup = parentGroup;
            _holderName = toolEntry.HolderName;
            double lengthFactor = _toolType == ToolTypes.Drill && _subType != ToolSubTypes.Tap ? 7 : 2.5;

            Number = toolEntry.Number;
            AdjustRegister = toolEntry.AdjustRegister;
            Name = toolEntry.Name;
            Length = toolEntry.Length;
            Diameter = toolEntry.Diameter;
            Flutes = toolEntry.Flutes;
            CornerRadius = toolEntry.CornerRadius;
            FluteLength = _toolType == ToolTypes.Mill && _subType == ToolSubTypes.BallMill ? Diameter / 2 : Diameter * lengthFactor;
        }

        /// <summary>
        /// Erstellt Werkzeug.
        /// </summary>
        public virtual void Create() {
            MillToolBuilder toolBuilder = (MillToolBuilder)CreateToolBuilder();

            if (toolBuilder != null) {

                var committed = toolBuilder.Commit();
                toolBuilder.Destroy();
                GetHolder(committed);
            }
        }

        public override string ToString() =>
            $"Name: {Name}\n" +
            $"Werkzeugnummer: {Number}\n" +
            $"Korrekturregister: {AdjustRegister}\n" +
            $"Länge: {Length}\n" +
            $"Durchmesser: {Diameter}\n" +
            $"Eckenradius: {CornerRadius}\n" +
            $"Schneidenanzahl: {Flutes}\n" +
            $"Halter: {_holderName}";

        /// <summary>
        /// Erstellt ein <see cref="MillingToolBuilder"/> und initialisiert es mit den Standardwerten.
        /// </summary>
        /// <returns><see cref="MillingToolBuilder"/> oder <see langword="null"/></returns>
        protected MillingToolBuilder CreateToolBuilder() {
            var tool = GetTool();
            MillingToolBuilder toolBuilder = CheckToolType(tool);

            if (toolBuilder is null) {
                NX.ShowErrorBox("ToolBuilder Instanzierung fehlgeschlagen!");
                return null;
            }

            toolBuilder.TlNumberBuilder.Value = Number;
            toolBuilder.TlAdjRegBuilder.Value = AdjustRegister;
            toolBuilder.TlHeightBuilder.Value = Length;
            toolBuilder.TlDiameterBuilder.Value = Diameter;
            toolBuilder.TlNumFlutesBuilder.Value = Flutes;
            if (!_toolExists)
                toolBuilder.TlFluteLnBuilder.Value = FluteLength;

            _toolExists = false;
            return toolBuilder;
        }

        /// <summary>
        /// Sucht den passenden Werkzeughalter, wenn bei der Initialisierung einer angegeben wurde.
        /// Wenn einer gefunden wurde, wird das Offset passend zur Halterlänge festgelegt.
        /// </summary>
        /// <param name="committedToolBuilder">Rückgabe der <see cref="Builder.Commit"/> Methode.</param>
        protected void GetHolder(NXObject committedToolBuilder)  {
            if (string.IsNullOrWhiteSpace(_holderName))
                return;

            var tool = (Tool)committedToolBuilder;
            double offset = 0;

            if (tool.RetrieveHolder(_holderName)) {
                MillingToolBuilder toolBuilder = CheckToolType(tool);

                // summiert die Längen aller Halterabschnitte auf
                for (int section = 0; section < toolBuilder.HolderSectionBuilder.NumberOfSections; section++) {
                    var sec = toolBuilder.HolderSectionBuilder.GetSection(section);
                    toolBuilder.HolderSectionBuilder.Get(sec, out _, out double length, out _, out _);
                    offset += length;
                }

                toolBuilder.HolderSectionBuilder.TlHolderOffsetBuilder.Value = offset;
                toolBuilder.Commit();
                toolBuilder.Destroy();
            }
        }

        /// <summary>
        /// Erstellt ein neues Werkzeug oder findet ein bereits vorhandenes.
        /// </summary>
        /// <returns><see cref="NCGroup"/> Werkzeug Objekt</returns>
        NCGroup GetTool() {
            NCGroup tool;
            string subTypeName = "MILL";

            // versucht ein bereits vorhandens Werkzeug zu finden
            try {
                tool = NX.CAM.Find(Name);
                _toolExists = true;
            }
            catch (NXException) {
                string typeName;
                // prüft die Werkzeugtypen und die Subtypen um ein entsprechendes NCGroup Werkzeug zu erstellen
                if (_toolType == ToolTypes.Mill)
                    typeName = "mill_planar";
                else
                    typeName = "hole_making";

                switch (_subType) {
                    case ToolSubTypes.Mill:
                        subTypeName = "MILL";
                        break;
                    case ToolSubTypes.BallMill:
                        subTypeName = "BALL_MILL";
                        break;
                    case ToolSubTypes.FaceMill:
                        subTypeName = "CHAMFER_MILL";
                        break;
                    case ToolSubTypes.TwistDrill:
                        subTypeName = "STD_DRILL";
                        break;
                    case ToolSubTypes.SolidDrill:
                        subTypeName = "STD_DRILL";
                        break;
                    case ToolSubTypes.SpotDrill:
                        subTypeName = "SPOT_DRILL";
                        break;
                    case ToolSubTypes.Tap:
                        subTypeName = "TAP";
                        break;
                    case ToolSubTypes.Reamer:
                        subTypeName = "REAMER";
                        break;

                    case ToolSubTypes.EndMill:
                        goto case ToolSubTypes.Mill;
                    case ToolSubTypes.EndMillCr:
                        goto case ToolSubTypes.Mill;
                    default:
                        break;
                }

                tool = NX.CAM.Collection.CreateTool(_parentGroup, typeName, subTypeName, NCGroupCollection.UseDefaultName.False, Name);
            }

            return tool;
        }

        /// <summary>
        /// Prüft den Werkzeugtyp und erstellt einen passenden ToolBuilder.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns><see cref="MillingToolBuilder"/></returns>
        MillingToolBuilder CheckToolType(NCGroup tool) {
            MillingToolBuilder toolBuilder = null;

            if (_toolType == ToolTypes.Mill)
                toolBuilder = NX.CAM.Collection.CreateMillToolBuilder(tool);
            else {
                switch (_subType) {
                    case ToolSubTypes.TwistDrill:
                        toolBuilder = NX.CAM.Collection.CreateDrillStdToolBuilder(tool);
                        break;
                    case ToolSubTypes.SpotDrill:
                        toolBuilder = NX.CAM.Collection.CreateDrillSpotdrillToolBuilder(tool);
                        break;
                    case ToolSubTypes.Tap:
                        toolBuilder = NX.CAM.Collection.CreateDrillTapToolBuilder(tool);
                        break;
                    case ToolSubTypes.Reamer:
                        toolBuilder = NX.CAM.Collection.CreateDrillReamerToolBuilder(tool);
                        break;

                    case ToolSubTypes.SolidDrill:
                        goto case ToolSubTypes.TwistDrill;
                    default:
                        break;
                }
            }

            return toolBuilder;
        }
    }
}
