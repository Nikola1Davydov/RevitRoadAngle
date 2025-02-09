using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadAngle.Models
{
    public class ExternalEventCreateVoid : ExternalEventHandler
    {
        public Action<UIApplication> action {  get; set; }
        public override void Execute(UIApplication uiApplication)
        {
            using (Transaction tx = new Transaction(uiApplication.ActiveUIDocument.Document, "Импорт файла"))
            {
                tx.Start();
                action.Invoke(uiApplication);
                tx.Commit();
            }
        }
    }
}
