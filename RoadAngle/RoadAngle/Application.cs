using Nice3point.Revit.Toolkit.External;
using RoadAngle.Commands;

namespace RoadAngle
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            Host.Start();
            CreateRibbon();
        }

        public override void OnShutdown()
        {
            Host.Stop();
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Commands", "RoadAngle");

            panel.AddPushButton<StartupCommand>("Execute")
                .SetImage("/RoadAngle;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/RoadAngle;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}