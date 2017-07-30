namespace WPFHexaEditor.Core.ROMTable {
    /// <summary>
    /// Represente une adresse favorite dans une ROM
    /// 
    /// Derek Tremblay 2003-2017
    /// </summary>
    public sealed class Bookmark
    {
        public string Position;
        public string Name;
        public string File;
        public string Key;

        public Bookmark()
        {

        }

        public Bookmark(string Name, string Position, string File, string key)
        {
            this.Position = Position;
            this.Name = Name;
            this.File = File;
            this.Key = key;
        }
    }
}
