using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RoadAngle.Helper;
using System.Diagnostics;
using System.Reflection;

namespace RoadAngle.Models
{
    public class WorkWithPointOnTopo
    {
        double offsetDistance = 0.5;
        double sampleStep = 10;

        private Element floor;
        private Element filledRegion;
        private Element topo;

        public WorkWithPointOnTopo(Element floor, Element filledRegion, Element topo)
        {
            this.floor = floor;
            this.filledRegion = filledRegion;
            this.topo = topo;
        }
        public void createPointsOnFloor()
        {
            try
            {
                // 1. Получаем исходный контур void‑выреза
                List<CurveLoop> innerLoops = raUtils.GetFilledRegionContours(filledRegion);

                // 2. Вычисляем offset‑контур
                CurveLoop offsetBoundary = OffsetCurveLoop(innerLoops.FirstOrDefault(), offsetDistance);

                // 3. Разбиваем offset‑контур на точки
                List<XYZ> boundaryPoints = SampleCurveLoop(offsetBoundary, sampleStep);

                // 3.5 Добавляем точки где пересекается грид
                List<XYZ> gridIntersectionPoints = raUtils.GetGridIntersectionPoints(Context.ActiveDocument);

                List<XYZ> centralOfGridIntersectionPoints = GetCentralPoints(gridIntersectionPoints);

                // 4. Для каждой точки определяем высоту на топографии
#if REVIT2023
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                // Получаем высоту из топографии для текущей точки 
                double elevation = GetElevationAtPoint(boundaryPoints[i], topo);
                boundaryPoints[i] = new XYZ(boundaryPoints[i].X, boundaryPoints[i].Y, elevation);
            }
#elif REVIT2024_OR_GREATER
                using (Transaction tx = new Transaction(Context.ActiveDocument, "Создание 3D вида"))
                {
                    tx.Start();
                    for (int i = 0; i < boundaryPoints.Count; i++)
                    {
                        // Получаем высоту из топографии для текущей точки 
                        boundaryPoints[i] = GetElevationAtPoint(boundaryPoints[i], topo, Context.ActiveDocument);
                    }
                    for (int i = 0; i < gridIntersectionPoints.Count; i++)
                    {
                        // Получаем высоту из топографии для текущей точки 
                        gridIntersectionPoints[i] = new XYZ(gridIntersectionPoints[i].X, gridIntersectionPoints[i].Y, 0);
                    }
                    for (int i = 0; i < centralOfGridIntersectionPoints.Count; i++)
                    {
                        // Получаем высоту из топографии для текущей точки 
                        centralOfGridIntersectionPoints[i] = new XYZ(centralOfGridIntersectionPoints[i].X, centralOfGridIntersectionPoints[i].Y, -1);
                    }
                    tx.Commit();
                }

#endif
                // 5. Корректируем точки, чтобы уклон между соседними не превышал 2%
                //List<XYZ> smoothedPoints = SmoothPointsByMaxSlope(boundaryPoints, maxSlope: 0.02);
                // Функция SmoothPointsByMaxSlope должна пройтись по точкам, сравнить разницу высот и скорректировать их

                // 6. Создаем эти точки на полу
                ModifyFloorFootprint(floor, centralOfGridIntersectionPoints);
                ModifyFloorFootprint(floor, gridIntersectionPoints);
                ModifyFloorFootprint(floor, boundaryPoints);
                
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }
        private CurveLoop OffsetCurveLoop(CurveLoop curves, double offsetDistance)
        {
            CurveLoop result = CurveLoop.CreateViaOffset(curves, offsetDistance, new XYZ(0, 0, 1));
            if (result.GetExactLength() > curves.GetExactLength())
            {
                return result;
            }
            else
            {
                OffsetCurveLoop(curves, offsetDistance + 1);
                return result;
            }

        }

        /// <summary>
        /// Разбивает заданный CurveLoop на набор точек.
        /// Здесь для каждой линии контура выбирается фиксированное число точек.
        /// </summary>
        private List<XYZ> SampleCurveLoop(CurveLoop offsetBoundary, double sampleStep)
        {
            List<XYZ> points = new List<XYZ>();

            foreach (Curve curve in offsetBoundary)
            {
                double len = curve.Length;
                int numSamples = (int)Math.Ceiling(len / sampleStep);
                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / numSamples;
                    XYZ pt = curve.Evaluate(t, true);
                    points.Add(pt);
                }
            }
            return points;
        }

        /// <summary>
        /// Для заданной точки (X,Y) получает высоту (Z) с топографической поверхности.
        /// Реализовано через вертикальное пересечение с геометрией топографии.
        /// </summary>
        //private double GetElevationAtPoint(XYZ point, Element topo)
        //{

        //    // Создаем вертикальную линию (от высокой до низкой точки)
        //    XYZ start = new XYZ(point.X, point.Y, 1000);
        //    XYZ end = new XYZ(point.X, point.Y, -1000);
        //    Line vertical = Line.CreateBound(start, end);
        //    Options opt = new Options();
        //    GeometryElement geoElem = topo.get_Geometry(opt);
        //    double closestZ = point.Z;
        //    double minDistance = double.MaxValue;
        //    foreach (GeometryObject geoObj in geoElem)
        //    {
        //        if (geoObj is Solid solid)
        //        {
        //            foreach (Face face in solid.Faces)
        //            {
        //                IntersectionResultArray results = new IntersectionResultArray();
        //                SetComparisonResult res = face.Intersect(vertical, out results);
        //                if (res == SetComparisonResult.Overlap && results.Size > 0)
        //                {
        //                    XYZ intPt = results.get_Item(0).XYZPoint;
        //                    double dist = Math.Abs(intPt.Z - point.Z);
        //                    if (dist < minDistance)
        //                    {
        //                        minDistance = dist;
        //                        closestZ = intPt.Z;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return closestZ;
        //}
        private XYZ GetElevationAtPoint(XYZ point, Element topo, Document doc)
        {
#if REVIT2023
            TopographySurface topoSurface = topo as TopographySurface;
            if (topoSurface == null)
                return point.Z; // если элемент не топография, возвращаем исходное значение

            Options opt = new Options();
            GeometryElement geoElem = topoSurface.get_Geometry(opt);
            double elevation = point.Z;
            bool found = false;
            double highestZ = -1e9;

            foreach (GeometryObject geoObj in geoElem)
            {
                Mesh mesh = geoObj as Mesh;
                if (mesh == null)
                    continue;

                int numTriangles = mesh.NumTriangles;
                for (int i = 0; i < numTriangles; i++)
                {
                    MeshTriangle tri = mesh.get_Triangle(i);
                    XYZ v0 = mesh.Vertices[(int)tri.get_Index(0)];
                    XYZ v1 = mesh.Vertices[(int)tri.get_Index(1)];
                    XYZ v2 = mesh.Vertices[(int)tri.get_Index(2)];

                    // Проверяем, лежит ли точка (с учетом только XY) внутри проекции треугольника.
                    if (IsPointInTriangleXY(point, v0, v1, v2))
                    {
                        // Вычисляем нормаль треугольника.
                        XYZ normal = (v1 - v0).CrossProduct(v2 - v0);
                        if (normal.GetLength() < 1e-9)
                            continue;
                        normal = normal.Normalize();
                        // Если normal.Z близка к 0, треугольник почти вертикальный – пропускаем.
                        if (Math.Abs(normal.Z) < 1e-9)
                            continue;

                        // Решаем уравнение плоскости треугольника:
                        // normal.X*(x - v0.X) + normal.Y*(y - v0.Y) + normal.Z*(z - v0.Z) = 0
                        // Отсюда: z = v0.Z - (normal.X*(point.X - v0.X) + normal.Y*(point.Y - v0.Y)) / normal.Z
                        double z = v0.Z - (normal.X * (point.X - v0.X) + normal.Y * (point.Y - v0.Y)) / normal.Z;

                        // Если несколько треугольников, выбираем тот, где z максимальное (наиболее верхняя поверхность).
                        if (!found || z > highestZ)
                        {
                            highestZ = z;
                            found = true;
                        }
                    }
                }
            }
            if (found)
                elevation = highestZ;
            return elevation;
#elif REVIT2024_OR_GREATER
            ElementId topoElementId = topo.Id;
            // Получаем все типы видов для 3D
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<ViewFamilyType> viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType))
                                                             .Cast<ViewFamilyType>()
                                                             .Where(vft => vft.ViewFamily == ViewFamily.ThreeDimensional)
                                                             .ToList();
            ViewFamilyType view3DType = viewFamilyTypes.First();
            View3D view3D = View3D.CreatePerspective(doc, view3DType.Id);
            ElementCategoryFilter filter = new ElementCategoryFilter(topo.Category.BuiltInCategory);

            // Создаем интерсектор для поиска пересечений с элементами топографии в заданном виде
            ReferenceIntersector intersector = new ReferenceIntersector(filter, FindReferenceTarget.Face, view3D);
            
            // Задаем направление луча – вниз по оси Z
            XYZ direction = new XYZ(0, 0, -1);
            try
            {
                // Ищем ближайшее пересечение луча, исходящего из точки 'point' в направлении 'direction'
                ReferenceWithContext refWithContext = intersector.FindNearest(point, direction);

                if (refWithContext != null)
                {
                    Reference reference = refWithContext.GetReference();
                    // Получаем глобальную точку пересечения
                    XYZ intersectionPoint = reference.GlobalPoint;
                    doc.Delete(view3D.Id);
                    return intersectionPoint;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }

            doc.Delete(view3D.Id);
            return null;
#endif
        }
        /// <summary>
        /// Проверяет, принадлежит ли точка p (используя только координаты X и Y)
        /// треугольнику, заданному вершинами a, b и c.
        /// Использует метод вычисления барицентрических координат.
        /// </summary>
        private bool IsPointInTriangleXY(XYZ p, XYZ a, XYZ b, XYZ c)
        {
            // Работает только с XY координатами
            double denominator = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            if (Math.Abs(denominator) < 1e-9)
                return false;
            double alpha = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denominator;
            double beta = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denominator;
            double gamma = 1.0 - alpha - beta;
            // Точка считается внутри, если все барицентрические координаты неотрицательны
            return (alpha >= 0) && (beta >= 0) && (gamma >= 0);
        }
        /// <summary>
        /// Корректирует набор точек так, чтобы уклон между соседними точками (по горизонтали) не превышал maxSlope.
        /// Уклон определяется как |ΔZ| / горизонтальное расстояние.
        /// </summary>
        private List<XYZ> SmoothPointsByMaxSlope(List<XYZ> boundaryPoints, double maxSlope)
        {
            List<XYZ> smoothed = new List<XYZ>(boundaryPoints);
            bool changed = true;
            int iterations = 0;
            while (changed && iterations < 100)
            {
                changed = false;
                for (int i = 0; i < smoothed.Count; i++)
                {
                    int next = (i + 1) % smoothed.Count;
                    XYZ p1 = smoothed[i];
                    XYZ p2 = smoothed[next];
                    double dx = p2.X - p1.X;
                    double dy = p2.Y - p1.Y;
                    double horiz = Math.Sqrt(dx * dx + dy * dy);
                    if (horiz < 1e-6)
                        continue;
                    double slope = Math.Abs(p2.Z - p1.Z) / horiz;
                    if (slope > maxSlope)
                    {
                        double sign = (p2.Z - p1.Z) >= 0 ? 1 : -1;
                        double newZ = p1.Z + sign * maxSlope * horiz;
                        smoothed[next] = new XYZ(p2.X, p2.Y, newZ);
                        changed = true;
                    }
                }
                iterations++;
            }
            return smoothed;
        }

        /// <summary>
        /// Изменяет эскиз существующего пола, заменяя его контуром, сформированным из newPoints.
        /// Использует FloorEditScope для редактирования эскиза.
        /// </summary>
        private void ModifyFloorFootprint(Element floor, List<XYZ> newPoints)
        {
            try
            {
                Document doc = floor.Document;
                Floor f = floor as Floor;
                if (f == null)
                {
                    TaskDialog.Show("Ошибка", "Элемент не является полом.");
                    return;
                }
                ElementId elementId = f.SketchId;
                Sketch floorSketch = doc.GetElement(elementId) as Sketch;
                using (Transaction tx = new Transaction(doc, "Изменение границы пола через SlabShapeEditor"))
                {
                    tx.Start();
                    // Получаем SlabShapeEditor для данного пола.
#if REVIT2023
                    SlabShapeEditor editor = f.SlabShapeEditor;
#elif REVIT2023_OR_GREATER
                    SlabShapeEditor editor = f.GetSlabShapeEditor();
#endif
                    if (!editor.IsEnabled)
                    {
                        editor.Enable();
                    }
                    foreach (XYZ point in newPoints)
                    {
                        if (point != null)
                        {
                            editor.DrawPoint(point);
                        }
                    }
                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                throw;
            }

        }
        private List<XYZ> GetCentralPoints(List<XYZ> intersectionPoints)
        {
            List<XYZ> points = new List<XYZ>(intersectionPoints);
            if (points == null || points.Count < 4)
            {
                return points;
            }

            // Сортируем точки по координате X (слева направо)
            points.Sort((p1, p2) => p1.X.CompareTo(p2.X));

            List<XYZ> centralPoints = new List<XYZ>();
            List<XYZ> result = new List<XYZ>();

            if (centralPoints.Count == 4)
            {
                XYZ point = GetCentralPointOfFourPoints(points);
                result.Add(point);
            }
            else
            {
                // Берем крайние 4 точки
                centralPoints.AddRange(points.Take(4));

                XYZ point = GetCentralPointOfFourPoints(centralPoints);
                result.Add(point);

                // Убираем две самые левые точки, пока не останется последние 4 точки
                while (points.Count > 4)
                {
                    centralPoints.Clear();
                    points.RemoveAt(0);
                    points.RemoveAt(0);
                    centralPoints.AddRange(points.Take(4));

                    XYZ point1 = GetCentralPointOfFourPoints(centralPoints);
                    result.Add(point1);
                }
            }

            return result;
        }
        private XYZ GetCentralPointOfFourPoints(List<XYZ> points)
        {
            if (points == null || points.Count != 4)
            {
                throw new ArgumentException("The number of points must be 4.");
            }

            int numSquares = points.Count;

            XYZ p1 = points[0];
            XYZ p2 = points[1];
            XYZ p3 = points[2];
            XYZ p4 = points[3];

            double totalX = (p1.X + p2.X + p3.X + p4.X) / 4;
            double totalY = (p1.Y + p2.Y + p3.Y + p4.Y) / 4;
            double totalZ = (p1.Z + p2.Z + p3.Z + p4.Z) / 4;

            return new XYZ(totalX,  totalY, totalZ);
        }
    }
}
