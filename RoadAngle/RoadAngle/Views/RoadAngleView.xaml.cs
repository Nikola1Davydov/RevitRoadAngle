using RoadAngle.ViewModels;

namespace RoadAngle.Views
{
    public sealed partial class RoadAngleView
    {
        public RoadAngleViewModel viewModel { get; set; }
        public RoadAngleView(RoadAngleViewModel _viewModel)
        {
            this.viewModel = _viewModel;
            InitializeComponent();
        }
    }
}