using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;

namespace DropletsInMotion.Application.Services
{
    public interface IContaminationService
    {
        byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap);
        //byte[,] ApplyContaminationMerge(Agent agent, byte[,] contaminationMap);
        bool IsAreaContaminated(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height);
        void UpdateContaminationArea(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height);
        void PrintContaminationState(byte[,] contaminationMap);
        void CopyContaminationMap(byte[,] source, byte[,] destination);
        byte[,] ApplyContaminationWithSize(Agent agent, byte[,] contaminationMap);
        void ApplyIfInBoundsWithContamination(byte[,] contaminationMap, int xPos, int yPos, byte substanceId);
        void ApplyIfInBoundsMerge(byte[,] contaminationMap, int xPos, int yPos, int finalX, int finalY, byte substanceIdInput1, byte substanceIdInput2, byte substanceIdOutput);

        byte[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePosition, byte[,] contaminationMap);
    }
}