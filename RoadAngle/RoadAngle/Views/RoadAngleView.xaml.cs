using RoadAngle.ViewModels;

namespace RoadAngle.Views
{
    public sealed partial class RoadAngleView
    {
        public RoadAngleView(RoadAngleViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}