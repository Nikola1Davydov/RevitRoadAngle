using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace PLM.Helpers
{
    public static class SelectionInModelUtils
    {
        public static void SelectDoorInRevitModel(UIDocument uIDocument, int plmElementsId)
        {
            ElementId elementIds = new ElementId(plmElementsId);
            uIDocument.Selection.SetElementIds(new List<ElementId> { elementIds });
        }
        public static ElementId PickElementInRevitModelElemId(UIDocument uIDocument, BuiltInCategory builtInCategory)
        {
            ElementId ElementId = uIDocument.Selection.PickObject(ObjectType.Element, new ElementSelectionFilter(builtInCategory), "Please select the door").ElementId;
            return ElementId;
        }
        public static Element PickElementInRevitModelElem(UIDocument uIDocument, BuiltInCategory builtInCategory)
        {
            ElementId _elementId = uIDocument.Selection.PickObject(ObjectType.Element, new ElementSelectionFilter(builtInCategory), "Please select the door").ElementId;
            return uIDocument.Document.GetElement(_elementId);
        }
    }
}
