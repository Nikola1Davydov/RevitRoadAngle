namespace RoadAngle.Models
{
    public class RoadAngleModel
    {
        public void Main(Element floor, Element filledRegion, Element topo)
        {
            CreateVoidAndCutFloor createVoidAndCutFloor = new CreateVoidAndCutFloor();
            FamilyInstance cuttingInstance = createVoidAndCutFloor.CreateVoidAndCut(floor, filledRegion, topo);

            WorkWithPointOnTopo workWithPointOnTopo = new WorkWithPointOnTopo(floor, filledRegion, topo);
            workWithPointOnTopo.createPointsOnFloor();
        }
    }
}
