using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RoadAngle.Utils;
using RoadAngle.Views;

namespace RoadAngle.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class StartupCommand : ExternalCommand
    {
        public override void Execute()
        {
            if (WindowController.Focus<RoadAngleView>()) return;

            var view = Host.GetService<RoadAngleView>();
            WindowController.Show(view, UiApplication.MainWindowHandle);
        }
    }
}