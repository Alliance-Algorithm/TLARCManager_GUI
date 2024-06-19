using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;

namespace TLARC_Manager.DataModels
{
    [Serializable]
    public struct CompFiles()
    {
        public Dictionary<string, object> arg = [];
        public string type = "";
        public string assembly = "";

        public Dictionary<string, uint> input_id = [];
        public uint this_id = 0;
    }
    [Serializable]
    public struct CompFilesList()
    {
        public CompFiles[] list = [];
    }

    public class ComponentCell(CompFiles component)
    {
        uint _dim = 0;
        uint _ealy = 0;

        CompFiles _component = component;

        public CompFiles Component => _component;

        public Dictionary<string, uint> RecieveID => _component.input_id;
        public uint Dim { get => _dim; set => _dim = value; }
        public uint Ealy { get => _ealy; set => _ealy = value; }
        List<ComponentCell> _forward = [];
        public List<ComponentCell> Forward { get => Forward = _forward; set => _forward = value; }
        public uint ID => _component.this_id;

        public static implicit operator ComponentCell(CompFiles component)
        {
            return new ComponentCell(component);
        }
    }
}