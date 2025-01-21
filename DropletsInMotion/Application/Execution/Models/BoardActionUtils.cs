namespace DropletsInMotion.Application.Execution.Models
{
    public static class BoardActionUtils
    {
        public static void FilterBoardActions(List<BoardAction> list1, List<BoardAction> list2)
        {
            if (!list1.Any()) return;

            double boundStart = list1.First().Time;
            double boundEnd = list2.Last().Time;

            list2.RemoveAll(action2 =>
                    action2.Time >= boundStart &&
                    action2.Time <= boundEnd &&
                    action2.Action == 0 && 
                    list1.Any(a => a.ElectrodeId == action2.ElectrodeId && a.Action == 1)
            );
        }
    }
}
