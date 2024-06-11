
using System.Collections.Generic;

namespace TLARCManager.DataModels
{
    public class ComponentsItem(int ID, string Assembly, string Type)
    {
        public int ThisID { get; set; } = ID;
        public string Assembly { get; set; } = Assembly;
        public string Type { get; set; } = Type;
        public int[] InputID { get; set; } = [];
        public string[] Args { get; set; } = [];

        public string Descriptor => ThisID.ToString() + ":\t" + Assembly + ' ' + Type;
    }
    public class ComponentsServeice
    {
        public IEnumerable<ComponentsItem> GetItems() => [new(0, "TLARC", "Alray")];

        private Dictionary<int, ComponentsItem> data_ = new();
    }
}
