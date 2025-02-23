namespace RoadAngle.Models
{
    public class RoadAngleModel
    {
        public void CreateVoid(Element floor, Element filledRegion, Element topo)
        {
            CreateVoidAndCutFloor createVoidAndCutFloor = new CreateVoidAndCutFloor();
            FamilyInstance cuttingInstance = createVoidAndCutFloor.CreateVoidAndCut(floor, filledRegion, topo);
        }
        public void AddPointOnFloor(Element floor, Element filledRegion, Element topo)
        {
            WorkWithPointOnTopo workWithPointOnTopo = new WorkWithPointOnTopo(floor, filledRegion, topo);
            workWithPointOnTopo.createPointsOnFloor();
        }
    }
}
