namespace LibMyFuckingMonitors
{
    public class MonitorModel
    {
        public MonitorModel() { }
        public int MonNum { get; set; }
        public uint InternalId { get; set; }
        public int Xpos { get; set; }
        public int Ypos { get; set; }
        public bool Primary { get; set; }
        public int Frequency { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ColorBits { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GPU { get; set; }
        public List<MonitorModes> Modes { get; set; } = new List<MonitorModes>();
    }
    public class MonitorModes
    { 
        public MonitorModes() { }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ColorBits { get; set; }
        public int Frequency { get; set; }
    }
}
