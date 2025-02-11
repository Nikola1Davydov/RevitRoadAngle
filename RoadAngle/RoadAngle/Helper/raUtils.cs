using Autodesk.Revit.UI;

namespace RoadAngle.Helper
{
    public static class raUtils
    {
        public static void createVoidFamily(double voidHeight, CurveArrArray profile, string familyTemplatePath, string newFamilyPath)
        {
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
                // Поскольку создаётся void‑геометрия, параметр isSolid устанавливаем в false.
                Extrusion extrusion = familyDoc.FamilyCreate.NewExtrusion(
                    isSolid: false,
                    profile: profile,
                    sketchPlane: sketchPlane,
                    end: voidHeight);

                // поскольку мы хотим вырезать геометрию в проекте,
                // поэтому ставим параметр "семество разрешаеться вырезать геометрию войдом" в true
                Family getFamily = familyDoc.OwnerFamily;
                Parameter parameter = getFamily.get_Parameter(BuiltInParameter.FAMILY_ALLOW_CUT_WITH_VOIDS);
                parameter.Set(1);

                tx.Commit();
            }

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



            // Закрываем документ семейства
            familyDoc.Close(false);
            //TaskDialog.Show("Успех", "Новое void‑семейство создано в проект.");
        }
        public static double GetHeightTopo(Element element)
        {
            BoundingBoxXYZ bb = element.get_BoundingBox(null);
            double height = bb.Max.Z - bb.Min.Z;
            return height;
        }
        /// <summary>
        /// Преобразует `BoundingBoxXYZ` в `CurveLoop`
        /// </summary>
        public static CurveLoop GetBoundingBoxAsCurveLoop(Element filledRegion, int number)
        {
            BoundingBoxXYZ bb = filledRegion.get_BoundingBox(null);
            XYZ min = bb.Min;
            XYZ max = bb.Max;

            // Создаём список линий для границы
            List<Curve> curves = new List<Curve>
            {
                Line.CreateBound(new XYZ(min.X-number, min.Y-number, min.Z), new XYZ(max.X+number, min.Y-number, min.Z)),
                Line.CreateBound(new XYZ(max.X+number, min.Y-number, min.Z), new XYZ(max.X+number, max.Y+number, min.Z)),
                Line.CreateBound(new XYZ(max.X+number, max.Y+number, min.Z), new XYZ(min.X-number, max.Y+number, min.Z)),
                Line.CreateBound(new XYZ(min.X-number, max.Y+number, min.Z), new XYZ(min.X-number, min.Y-number, min.Z))
            };

            // Используем CurveLoop.Create() вместо .Add()
            return CurveLoop.Create(curves);
        }
        public static CurveArrArray createProfileForVoid(Element filledRegion, int outerLoopGrowNumber)
        {
            // 1. Получаем границы FilledRegion
            // Получаем внешний контур из FilledRegion
            CurveLoop outerLoop = GetBoundingBoxAsCurveLoop(filledRegion, outerLoopGrowNumber);
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

            return profile;
        }
        /// <summary>
        /// Получает контуры `FilledRegion` как внутренние границы
        /// </summary>
        public static List<CurveLoop> GetFilledRegionContours(Element filledRegion)
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
    }
}
