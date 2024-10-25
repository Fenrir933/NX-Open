using NXOpen.CAM;
using System.Collections.Generic;

namespace NXWrapper.Interfaces {
    /// <summary>
    /// Stellt Eigenschaften und Funktionen zur Interaktion mit dem CAM-Setup bereit.
    /// </summary>
    public interface ICAM {
        /// <summary> Aktuelle CAMSetup. </summary>
        CAMSetup Setup { get; }
        /// <summary> Wurzelelement der Programmreihenfolge-Ansicht. </summary>
        NCGroup ProgramRoot { get; }
        /// <summary> Wurzelelement der Wkz-Maschinenansicht. </summary>
        NCGroup MachineRoot { get; }
        /// <summary> Wurzelelement der Geometrieansicht. </summary>
        NCGroup GeometryRoot { get; }
        /// <summary> Wurzelelement der Bearbeitungsmethodenansicht. </summary>
        NCGroup MethodRoot { get; }
        /// <summary> Sammlung aller Elemente des Operationsnavigator. </summary>
        NCGroupCollection Collection { get; }
        /// <summary> Sammlung aller Operationen. </summary>
        OperationCollection Operations { get; }
        /// <summary> Sammlung aller Werkzeuge in der Wkz-Maschinenansicht. </summary>
        HashSet<Tool> Tools { get; }

        /// <summary>
        /// Gibt ein <see cref="IEnumerable{NCGroup}"/> mit allen untergeordneten <see cref="NCGroup"/> Objekten einer <see cref="NCGroup"/> zurück.
        /// </summary>
        /// <param name="ncGroup"><see cref="NCGroup"/>-Objekt durch welches iteriert werden soll.</param>
        /// <param name="recursive">Über die direkt untergeordneten Elemente iterieren oder über alle.</param> 
        IEnumerable<NCGroup> GetNCGroupMembers(NCGroup ncGroup, bool recursive = false);

        /// <inheritdoc cref="GetNCGroupMembers(NCGroup, bool)"/>
        /// <param name="ncGroupName">Name des <see cref="NCGroup"/>-Objekt</param>
        /// <param name="recursive">Über die direkt untergeordneten Elemente iterieren oder über alle.</param>
        IEnumerable<NCGroup> GetNCGroupMembers(string ncGroupName, bool recursive = false);

        /// <summary>
        /// Sucht ein NCGroup-Objekt anhand eines Namen und gibt es zurück.
        /// </summary>
        /// <param name="ncGroupName">Objektname</param>
        /// <returns></returns>
        NCGroup Find(string ncGroupName);
    }
}
