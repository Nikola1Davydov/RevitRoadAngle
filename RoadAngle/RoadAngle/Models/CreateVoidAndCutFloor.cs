﻿using Autodesk.Revit.UI;
using System;
using RoadAngle.Helper;

namespace RoadAngle.Models
{
    public class CreateVoidAndCutFloor
    {
        int outerLoopGrowNumber = 5;
        // шаблон для семейства берет от сюда
        string familyTemplatePath = @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\German\Allgemeines Modell.rft";
        // Сохраняем семейство в файл (укажите нужный путь для сохранения)
        string newFamilyPath = @"C:\Temp\NewVoidFamily.rfa";
        FamilyInstance cuttingInstance;

        /// <summary>
        /// Создаёт Void-элемент по внешнему BoundingBox `FilledRegion`
        /// и использует контур `FilledRegion` для внутренней границы
        /// </summary>
        public FamilyInstance CreateVoidAndCut(Element floor, Element filledRegion, Element topo)
        {
            double voidHeight = raUtils.GetHeightTopo(topo);
            CurveArrArray profile = raUtils.createProfileForVoid(filledRegion, outerLoopGrowNumber);

            if (!System.IO.File.Exists(familyTemplatePath))
            {
                TaskDialog.Show("Ошибка", "Шаблон семейства не найден: " + familyTemplatePath);
                return null;
            }

            raUtils.createVoidFamily(voidHeight, profile, familyTemplatePath, newFamilyPath);
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
                cuttingInstance = Context.ActiveDocument.Create.NewFamilyInstance(XYZ.Zero, symbol, structuralType: Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                tx.Commit();
            }

           
            using (Transaction tx = new Transaction(Context.ActiveDocument, "Импорт файла"))
            {
                tx.Start();
                try
                {
                    InstanceVoidCutUtils.AddInstanceVoidCut(Context.ActiveDocument, floor,cuttingInstance);
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