using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Application.Execution.Models
{
    public static class BoardActionUtils
    {
        public static void FilterBoardActions(List<BoardAction> list1, List<BoardAction> list2)
        {
            if (!list1.Any()) return; // Return if list1 is empty

            // Define the bounds using the first and last elements in the sorted list1
            double boundStart = list1.First().Time;
            double boundEnd = list2.Last().Time;

            // Remove actions in list2 that turn off an electrode which is turned on in list1 within the bound time
            list2.RemoveAll(action2 =>
                    action2.Time >= boundStart &&
                    action2.Time <= boundEnd &&
                    action2.Action == 0 && // Check if it turns off the electrode
                    list1.Any(a => a.ElectrodeId == action2.ElectrodeId && a.Action == 1) // Ensure electrode was turned on in list1
            );
        }
        public static void FilterBoardActions2(List<BoardAction> list1, List<BoardAction> list2)
        {
            if (!list1.Any()) return; // Return if list1 is empty

            // Define the bounds using the first and last elements in the sorted list1
            double boundStart = list1.First().Time;
            double boundEnd = list2.Last().Time;

            // Remove actions in list2 that turn off an electrode which is turned on in list1 within the bound time
            list2.RemoveAll(action2 =>
                    action2.Time >= boundStart &&
                    action2.Time <= boundEnd &&
                    action2.Action == 0 && // Check if it turns off the electrode
                    list1.Any(a => a.ElectrodeId == action2.ElectrodeId && a.Action == 1) // Ensure electrode was turned on in list1
            );
        }
    }
}
