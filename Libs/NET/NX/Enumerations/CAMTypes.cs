/// Typen der uf_object_types.h
namespace NXWrapper.Enumerations.CAM {
    /// <summary> Allgemeine NX CAM Typen </summary>
    public enum General {
        /// <summary> Ordner für unbenutzte Operationen </summary>
        NULL_GRP_TYPE = 112
    }

    /// <summary> Programmtypen </summary>
    public enum ProgramTypes {
        /// <summary> Typ der Bearbeitungsaufgabe </summary>
        PROGRAM = 121
    }

    /// <summary> Maschinentypen </summary>
    public enum MachineTypes {
        /// <summary> Maschinen in der Wkz-Maschinenansicht </summary>
        MACHINE = 106,
        /// <summary> Werkzeug </summary>
        TOOL = 109
    }

    /// <summary> Geometrietypen </summary>
    public enum GeometryTypes {
        /// <summary> Geometriegruppe </summary>
        GEOMETRY = 105
    }

    /// <summary> Methodentypen </summary>
    public enum MethodTypes {
        /// <summary> Methodengruppe </summary>
        METHOD = 128
    }


    /// <summary> Programmsubtypen </summary>
    public enum ProgramSubtypes {
        /// <summary> Operationen </summary>
        OPERATION_ORDER = 160
    }

    /// <summary> Maschinensubtypen </summary>
    public enum MachineSubtypes {
        /// <summary> Haupt-Maschine </summary>
        MAIN_MACHINE = 0,
        /// <summary> Maschinenkomponente </summary>
        COMPONENT = 1,
        /// <summary> Aufnahme </summary>
        POCKET = 2,
        /// <summary> Werkzeugtyp -> Fräsen/Drehen/Taster </summary>
        TOOL_TYPE = 0
    }

    /// <summary> Geometriesubtypen </summary>
    public enum GeometrySubtypes {
        /// <summary> Werkstück/Fräsbereich </summary>
        WORKPIECE = 10,
        /// <summary> Begrenzung </summary>
        BOUNDARY = 20,
        /// <summary> Nullpunkt/Ausrichtung </summary>
        MCS = 30
    }

    /// <summary> Methodensubtypen </summary>
    public enum MethodSubtypes {
        /// <summary> Standardmethode </summary>
        MODE_DEFAULT = 0,
        /// <summary> Fräsmethode </summary>
        MILL = 10,
        /// <summary> Bohren Punkt zu Punkt </summary>
        DRILL = 30,
        /// <summary> Bohren </summary>
        HOLE_MAKING = 110
    }
}
