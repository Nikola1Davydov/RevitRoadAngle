using Autodesk.Revit.UI;

namespace RoadAngle.Models
{
    public class CreateVoidAndCutFloor
    {
        int outerLoopGrowNumber = 10;
        /// <summary>
        /// Создаёт Void-элемент по внешнему BoundingBox `FilledRegion`
        /// и использует контур `FilledRegion` для внутренней границы
        /// </summary>
        public void CreateVoidAndCut(Element floor, Element filledRegion, Element topo)
        {
            double voidHeight = GetHeightTopo(topo);

            // 1. Получаем границы FilledRegion
            // Получаем внешний контур из FilledRegion
            CurveLoop outerLoop = GetBoundingBoxAsCurveLoop(filledRegion);
            // Если нужно учитывать внутренние границы, можно получить их:
            List<CurveLoop> innerLoops = GetFilledRegionContours(filledRegion);
            // Для простоты возьмём только outerLoop как профиль.
            // 1. Преобразуем внешний и внутренние контуры в CurveArrArray
            CurveArrArray profile = new CurveArrArray();
            // Внешний контур:
            CurveArray outerArray = new CurveArray();
            foreach (Curve curve in outerLoop)
            {
                outerArray.Append(curve);
            }
            profile.Append(outerArray);
            // Если есть внутренние контуры – добавляем их:
            if (innerLoops != null && innerLoops.Count > 0)
            {
                foreach (CurveLoop loop in innerLoops)
                {
                    CurveArray innerArray = new CurveArray();
                    foreach (Curve curve in loop)
                    {
                        innerArray.Append(curve);
                    }
                    profile.Append(innerArray);
                }
            }

            string familyTemplatePath = @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\German\Allgemeines Modell.rft";
            if (!System.IO.File.Exists(familyTemplatePath))
            {
                TaskDialog.Show("Ошибка", "Шаблон семейства не найден: " + familyTemplatePath);
                return;
            }

            // Создаём новый документ семейства
            Document familyDoc = Context.ActiveDocument.Application.NewFamilyDocument(familyTemplatePath);
            if (familyDoc == null)
            {
                TaskDialog.Show("Ошибка", "Не удалось создать документ семейства.");
                return;
            }

            using (Transaction tx = new Transaction(familyDoc, "Создание void‑геометрии"))
            {
                tx.Start();

                // Создаём плоскость для эскиза (плоскость XY, нормаль (0,0,1))
                Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), XYZ.Zero);
                SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);

                // Создаём экструдированную void‑геометрию.
                // Поскольку создаётся void‑геометрия, параметр isVoid устанавливаем в true.
                Extrusion extrusion = familyDoc.FamilyCreate.NewExtrusion(
                    isSolid: false,
                    profile: profile,
                    sketchPlane: sketchPlane,
                    end: voidHeight);
                Family getFamily = familyDoc.OwnerFamily;
                Parameter parameter = getFamily.get_Parameter(BuiltInParameter.FAMILY_ALLOW_CUT_WITH_VOIDS);
                parameter.Set(1);

                tx.Commit();
            }

            // Сохраняем семейство в файл (укажите нужный путь для сохранения)
            string newFamilyPath = @"C:\Temp\NewVoidFamily.rfa";
            SaveAsOptions saveOpts = new SaveAsOptions { OverwriteExistingFile = true };

            try
            {
                familyDoc.SaveAs(newFamilyPath, saveOpts);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка сохранения", ex.Message);
                familyDoc.Close(false);
                return;
            }

            // Загружаем созданное семейство в активный документ проекта
            Family loadedFamily;
            if (!Context.ActiveDocument.LoadFamily(newFamilyPath, out loadedFamily))
            {
                TaskDialog.Show("Ошибка", "Не удалось загрузить созданное семейство.");
                familyDoc.Close(false);
                return;
            }



            // Закрываем документ семейства
            familyDoc.Close(false);
            TaskDialog.Show("Успех", "Новое void‑семейство создано и загружено в проект.");
        }

        private double GetHeightTopo(Element element)
        {
            BoundingBoxXYZ bb = element.get_BoundingBox(null);
            double height = bb.Max.Z - bb.Min.Z;
            return height;
        }

        /// <summary>
        /// Преобразует `BoundingBoxXYZ` в `CurveLoop`
        /// </summary>
        private CurveLoop GetBoundingBoxAsCurveLoop(Element element)
        {
            BoundingBoxXYZ bb = element.get_BoundingBox(null);
            XYZ min = bb.Min;
            XYZ max = bb.Max;

            // Создаём список линий для границы
            List<Curve> curves = new List<Curve>
            {
                Line.CreateBound(new XYZ(min.X-outerLoopGrowNumber, min.Y-outerLoopGrowNumber, min.Z), new XYZ(max.X+outerLoopGrowNumber, min.Y-outerLoopGrowNumber, min.Z)),
                Line.CreateBound(new XYZ(max.X+outerLoopGrowNumber, min.Y-outerLoopGrowNumber, min.Z), new XYZ(max.X+outerLoopGrowNumber, max.Y+outerLoopGrowNumber, min.Z)),
                Line.CreateBound(new XYZ(max.X+outerLoopGrowNumber, max.Y+outerLoopGrowNumber, min.Z), new XYZ(min.X-outerLoopGrowNumber, max.Y+outerLoopGrowNumber, min.Z)),
                Line.CreateBound(new XYZ(min.X-outerLoopGrowNumber, max.Y+outerLoopGrowNumber, min.Z), new XYZ(min.X-outerLoopGrowNumber, min.Y-outerLoopGrowNumber, min.Z))
            };

            // Используем CurveLoop.Create() вместо .Add()
            return CurveLoop.Create(curves);
        }

        /// <summary>
        /// Получает контуры `FilledRegion` как внутренние границы
        /// </summary>
        private List<CurveLoop> GetFilledRegionContours(Element filledRegion)
        {
            List<CurveLoop> loops = new List<CurveLoop>();

            if (filledRegion is FilledRegion region)
            {
                IList<CurveLoop> profile = region.GetBoundaries();
                if (profile != null)
                {
                    foreach (CurveLoop loop in profile)
                    {
                        loops.Add(loop);
                    }
                }
            }
            return loops;
        }

        /// <summary>
        /// Получает символ семейства Void (если нет - создаёт)
        /// </summary>
        private FamilySymbol GetVoidFamilySymbol(Document doc, string voidFamilyKeyword = "Void")
        {
            // Ищем семейство, имя которого содержит заданное ключевое слово (без учёта регистра)
            Family voidFamily = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(f => f.Name.IndexOf(voidFamilyKeyword, StringComparison.InvariantCultureIgnoreCase) >= 0);

            if (voidFamily != null)
            {
                // Получаем все символы этого семейства
                ICollection<ElementId> symbolIds = voidFamily.GetFamilySymbolIds();
                if (symbolIds.Any())
                {
                    // Выбираем первый символ. При необходимости можно добавить дополнительную логику выбора.
                    FamilySymbol symbol = doc.GetElement(symbolIds.First()) as FamilySymbol;
                    // Активируем символ, если он не активен
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                        doc.Regenerate();
                    }
                    return symbol;
                }
            }
            return null;
        }

        private CurveArrArray ConvertCurveLoopListToCurveArrArray(List<CurveLoop> loops)
        {
            CurveArrArray curveArrArray = new CurveArrArray();
            foreach (CurveLoop loop in loops)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Curve curve in loop)
                {
                    curveArray.Append(curve);
                }
                curveArrArray.Append(curveArray);
            }
            return curveArrArray;
        }
    }
}