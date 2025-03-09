namespace RoadAngle.Models
{
    public class RoadAngleModel
    {
        public void CreateVoid(Element floor, Element filledRegion, Element topo, int outerLoopGrowNumber)
        {
            CreateVoidAndCutFloor createVoidAndCutFloor = new CreateVoidAndCutFloor();
            FamilyInstance cuttingInstance = createVoidAndCutFloor.CreateVoidAndCut(floor, filledRegion, topo, outerLoopGrowNumber);
        }
        public void AddPointOnFloor(Element floor, Element filledRegion, Element topo)
        {
            WorkWithPointOnTopo workWithPointOnTopo = new WorkWithPointOnTopo(floor, filledRegion, topo);
            workWithPointOnTopo.createPointsOnFloor();
        }
    }
}
