using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RoadAngle.Helper;
using System.IO;

namespace RoadAngle.Models
{
    public class CreateVoidAndCutFloor
    {
        int outerLoopGrowNumber = 5;
        // шаблон для семейства берет от сюда


        string familyTemplatePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk",
#if REVIT2023
            "RVT 2023",
#elif REVIT2024
            "RVT 2024",
#elif REVIT2025
            "RVT 2025",
#endif
            "Family Templates",
            "German",
            "Allgemeines Modell.rft"
        );
        // Сохраняем семейство в файл (укажите нужный путь для сохранения)
        string newFamilyPath = Path.Combine(Path.GetTempPath(), "NewVoidFamily.rfa");
        FamilyInstance cuttingInstance;

        /// <summary>
        /// Создаёт Void-элемент по внешнему BoundingBox `FilledRegion`
        /// и использует контур `FilledRegion` для внутренней границы
        /// </summary>
        public FamilyInstance CreateVoidAndCut(Element floor, Element filledRegion, Element topo, int outerLoopGrowNumber)
        {
            this.outerLoopGrowNumber = outerLoopGrowNumber;

            BoundingBoxXYZ bb = topo.get_BoundingBox(null);
            double voidHeight = bb.Max.Z + 1 - bb.Min.Z;

            // 1. Получаем границы FilledRegion
            CurveArrArray profile = raUtils.createProfileForVoid(filledRegion, outerLoopGrowNumber);
            
            if (!System.IO.File.Exists(familyTemplatePath))
            {
                TaskDialog.Show("Ошибка", "Шаблон семейства не найден: " + familyTemplatePath);
                return null;
            }

            raUtils.createVoidFamily(Context.ActiveDocument.Application, voidHeight, profile, familyTemplatePath, newFamilyPath);
            using (Transaction tx = new Transaction(Context.ActiveDocument, "Импорт файла"))
            {
                tx.Start();
                // Загружаем созданное семейство в активный документ проекта
                Family loadedFamily;
                if (!Context.ActiveDocument.LoadFamily(newFamilyPath, out loadedFamily))
                {
                    TaskDialog.Show("Ошибка", "Не удалось загрузить созданное семейство.");
                    return null;
                }
                FamilySymbol symbol = null;

                foreach (ElementId s in loadedFamily.GetFamilySymbolIds())
                {
                    symbol = Context.ActiveDocument.GetElement(s) as FamilySymbol;
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                    }
                    break;
                    // our family only contains one
                    // symbol, so pick it and leave

                }
                XYZ location = new XYZ(0, 0, topo.get_BoundingBox(null).Min.Z);
                cuttingInstance = Context.ActiveDocument.Create.NewFamilyInstance(location, symbol, structuralType: Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                tx.Commit();
            }


            using (Transaction tx = new Transaction(Context.ActiveDocument, "Импорт файла"))
            {
                tx.Start();
                try
                {
                    InstanceVoidCutUtils.AddInstanceVoidCut(Context.ActiveDocument, floor, cuttingInstance);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка", $"{ex.Message}");
                    throw;
                }
                tx.Commit();
            }
            return cuttingInstance;
        }
    }
}