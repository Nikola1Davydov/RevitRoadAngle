using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;

namespace RoadAngle.Models
{
    public class ExternalEventCreateVoid : ExternalEventHandler
    {
        public Action<UIApplication> action { get; set; }
        public override void Execute(UIApplication uiApplication)
        {
            using (TransactionGroup tx = new TransactionGroup(uiApplication.ActiveUIDocument.Document, "Импорт файла"))
            {
                tx.Start();
                action.Invoke(uiApplication);
                tx.Assimilate();
            }
        }
    }
}
