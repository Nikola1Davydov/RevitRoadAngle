using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Helpers
{
    public class ElementSelectionFilter : ISelectionFilter
    {
        BuiltInCategory _builtInCategory { get; set; }
        public ElementSelectionFilter(BuiltInCategory builtInCategory)
        {
            _builtInCategory = builtInCategory;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category != null && elem.Category.Id.IntegerValue == (int)_builtInCategory;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
