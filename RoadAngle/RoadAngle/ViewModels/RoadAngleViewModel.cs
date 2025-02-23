using Autodesk.Revit.UI;
using PLM.Helpers;
using RoadAngle.Helper;
using RoadAngle.Models;
using System.Runtime.InteropServices;

namespace RoadAngle.ViewModels
{
    public partial class RoadAngleViewModel : ObservableObject
    {
        #region params
        [ObservableProperty]
        private string _contextSelectionFilledRegion = string.Empty;
        [ObservableProperty]
        private string _contextSelectionTopo = string.Empty;
        [ObservableProperty]
        private string _contextSelectionFloor = string.Empty;

        private Element selectionTopo;
        private Element selectionFilledRegion;
        private Element selectionFloor;

        ExternalEventCreateVoid handler;
        ExternalEvent exEvent;

        RoadAngleModel roadAngleModel = new RoadAngleModel();
        #endregion

        public RoadAngleViewModel()
        {
            handler = new ExternalEventCreateVoid();
            exEvent = ExternalEvent.Create(handler);
        }
        #region commands

        [RelayCommand]
        private void CreateVoid()
        {
            handler.action = (UIApplication app) => roadAngleModel.CreateVoid(selectionFloor, selectionFilledRegion, selectionTopo);
            exEvent.Raise();
        }
        [RelayCommand]
        private void AddPointOnFloor()
        {
            handler.action = (UIApplication app) => roadAngleModel.AddPointOnFloor(selectionFloor, selectionFilledRegion, selectionTopo);
            exEvent.Raise();
        }
        [RelayCommand]
        private void Close(object parameter)
        {
            CommandsUtils.CloseWindow(parameter);
        }
        [RelayCommand]
        private void SelectTopo()
        {
            try
            {
                selectionTopo = SelectionInModelUtils.PickElementInRevitModelElem(Context.ActiveUiDocument, BuiltInCategory.OST_Topography);
                ContextSelectionTopo = selectionTopo.Id.ToString();
            }
            catch (Exception)
            {

                throw;
            }

        }
        [RelayCommand]
        private void SelectFilledRegion()
        {
            try
            {
                selectionFilledRegion = SelectionInModelUtils.PickElementInRevitModelElem(Context.ActiveUiDocument, BuiltInCategory.OST_DetailComponents);
                ContextSelectionFilledRegion = selectionFilledRegion.Id.ToString();
            }
            catch (Exception)
            {

                throw;
            }

        }
        [RelayCommand]
        private void SelectFloor()
        {
            try
            {
                selectionFloor = SelectionInModelUtils.PickElementInRevitModelElem(Context.ActiveUiDocument, BuiltInCategory.OST_Floors);
                ContextSelectionFloor = selectionFloor.Id.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}