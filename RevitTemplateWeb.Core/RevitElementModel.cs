using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplateWeb.Core
{
    public class RevitElementModel
    {
        public int ElementId { get; }
        public string Name { get;}

        public RevitElementModel(int elementId, string name)
        {
            ElementId = elementId;
            Name = name;
        }
    }
}
