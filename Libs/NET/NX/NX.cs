using NXWrapper.Interfaces;
using NXOpen;
using NXOpen.UF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NXWrapper {
    /// <summary> NX Typ und Subtyp die von UF_OBJ_ASK_TYPE_AND_SUBTYPE zurückgegeben werden. </summary>
    public struct NXType {
        public int Type;
        public int Subtype;
    }

    /// <summary>
    /// Statische Klasse <see cref="NX"/> stellt Eigenschaften und Funktionen zur einfachen Interaktion der NX Open API bereit. 
    /// </summary>
    public static class NX {
        /// <summary> Aktive NX <see cref="NXOpen.Session"/> </summary>
        public static Session Session = Session.GetSession();
        /// <summary> NX <see cref="NXOpen.UI"/>. </summary>
        public static UI UI = UI.GetUI();
        /// <summary> Aktuelles Teil </summary>
        public static Part WorkPart = Session.Parts.Work;
        /// <summary> <see cref="UpdateManager"/> der <see cref="Session"/> </summary>
        public static Update UpdateManager = Session.UpdateManager;
        /// <summary> Aktuelle <see cref="NXOpen.UF.UFSession"/>. </summary>
        public static UFSession UFSession = UFSession.GetUFSession();
        internal static Version Version = new Version(Session.GetEnvironmentVariableValue("UGII_FULL_VERSION").Replace("v", ""));

        /// <summary>
        /// <see cref="CAM"/> Implementation des <see cref="ICAM"/>  Interface.
        /// </summary>
        public static ICAM CAM = new CAM.CAM();

        /// <summary> Setzt eine Rückgängig-Marke der ausgeführten Methode. </summary>
        /// <param name="name">Name</param>
        public static Session.UndoMarkId SetUndoMark(string name) =>
            Session.SetUndoMark(Session.MarkVisibility.Visible, name);

        /// <summary> Zeigt eine Infobox. </summary>
        public static void ShowInfoBox(object message) =>
            UI.NXMessageBox.Show("Info", NXMessageBox.DialogType.Information, message.ToString());

        /// <summary> Zeigt eine Fragebox. </summary>
        public static void ShowQuestionBox(object message) =>
            UI.NXMessageBox.Show("Auswahl", NXMessageBox.DialogType.Question, message.ToString());

        /// <summary> Zeigt eine Warnungsbox. </summary>
        public static void ShowWarningBox(object message) =>
            UI.NXMessageBox.Show("Warning", NXMessageBox.DialogType.Warning, message.ToString());

        /// <summary> Zeigt eine Errorbox. </summary>
        public static void ShowErrorBox(object message) =>
            UI.NXMessageBox.Show("Error", NXMessageBox.DialogType.Error, message.ToString());

        /// <summary>
        /// Gibt <see cref="NXType"/> eines <see cref="NXObject"/> zurück.
        /// Kann mit den Typen in <see cref="Enumerations"/> verglichen werden.
        /// </summary>
        /// <param name="xObject"><see cref="NXObject"/> dessen Typen extrahiert werden sollen.</param>
        /// <returns></returns>
        public static NXType GetNXType(NXObject xObject) {
            UFSession.Obj.AskTypeAndSubtype(xObject.Tag, out int type, out int subtype);
            return new NXType { Type = type, Subtype = subtype };
        }

        /// <summary>
        /// Löscht ein Objekt.
        /// </summary>
        /// <param name="object">Objekt welches gelöscht werden soll</param>
        public static void DeleteObject(TaggedObject @object) {
            var undo = SetUndoMark("Objekt loeschen");
            UpdateManager.AddToDeleteList(@object);
            UpdateManager.DoUpdate(undo);
            Session.DeleteUndoMark(undo, "Objekt loeschen");
        }

        /// <summary>
        /// Löscht eine Liste von Objekten.
        /// </summary>
        /// <param name="objects">Objekte welche gelöscht werden sollen</param>
        public static void DeleteObjects(TaggedObject[] objects) {
            var undo = SetUndoMark("Objekte loeschen");
            UpdateManager.AddObjectsToDeleteList(objects);
            UpdateManager.DoUpdate(undo);
            Session.DeleteUndoMark(undo, "Objekte loeschen");
        }

        /// <inheritdoc/>
        public static void DeleteObjects(IEnumerable<TaggedObject> objects) =>
            DeleteObjects(objects.ToArray());
    }
}
