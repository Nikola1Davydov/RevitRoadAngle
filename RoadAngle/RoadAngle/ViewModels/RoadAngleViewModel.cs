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
        [ObservableProperty]
        private int _outerLoopGrowNumber = 5;

        private Element selectionTopo;
        private Element selectionFilledRegion;
        private Element selectionFloor;

        ExternalEventActionHandler handler;
        ExternalEvent exEvent;

        RoadAngleModel roadAngleModel = new RoadAngleModel();
        #endregion

        public RoadAngleViewModel()
        {
            handler = new ExternalEventActionHandler();
            exEvent = ExternalEvent.Create(handler);
        }
        #region commands

        [RelayCommand]
        private void CreateVoid()
        {
            handler.action = (UIApplication app) => roadAngleModel.CreateVoid(selectionFloor, selectionFilledRegion, selectionTopo, OuterLoopGrowNumber);
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
#if REVIT2023
                BuiltInCategory topoBuiltInCategory = BuiltInCategory.OST_Topography;
#elif REVIT2024_OR_GREATER
                BuiltInCategory topoBuiltInCategory = BuiltInCategory.OST_Toposolid;
#endif
                selectionTopo = SelectionInModelUtils.PickElementInRevitModelElem(Context.ActiveUiDocument, topoBuiltInCategory);
                ContextSelectionTopo = selectionTopo.Id.ToString();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Benutzer hat die Auswahl abgebrochen, keine Aktion erforderlich
            }
            catch (Exception ex)
            {
                // Loggen Sie die Ausnahme oder zeigen Sie eine Fehlermeldung an
                TaskDialog.Show("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}");
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
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Benutzer hat die Auswahl abgebrochen, keine Aktion erforderlich
            }
            catch (Exception ex)
            {
                // Loggen Sie die Ausnahme oder zeigen Sie eine Fehlermeldung an
                TaskDialog.Show("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}");
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
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Benutzer hat die Auswahl abgebrochen, keine Aktion erforderlich
            }
            catch (Exception ex)
            {
                // Loggen Sie die Ausnahme oder zeigen Sie eine Fehlermeldung an
                TaskDialog.Show("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}");
            }
        }
#endregion
    }
}