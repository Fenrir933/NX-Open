namespace NXTools {
    /// <summary> Werkzeugtypen </summary>
    public enum ToolTypes {
        Mill = 1,
        Drill = 2
    }

    /// <summary> Fräswerkzeuge / Bohrwerkzeuge </summary>
    public enum ToolSubTypes {
        Mill = 100,
        BallMill = 110,
        EndMill = 120,
        EndMillCr = 121,
        FaceMill = 140,

        TwistDrill = 200,
        SolidDrill = 205,
        SpotDrill = 220,
        Tap = 240,
        Reamer = 250
    }

    /// <summary> Alle benötigten Eigenschaften zur Erstellung eines Werkzeugs. </summary>
    public class ToolEntry {
        public ToolSubTypes Type { get; set; }
        public int Number { get; set; }
        public int AdjustRegister { get; set; } = 1;
        public string Name { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public double InnerDiamaeter { get; set; }
        public double CornerRadius { get; set; }
        public double Angel { get; set; }
        public int Flutes { get; set; }
        public bool Create { get; set; }
        public string HolderName { get; set; } = "";
        public string Article { get; set; } = "";
    }
}
