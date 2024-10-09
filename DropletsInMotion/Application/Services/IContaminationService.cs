using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;

namespace DropletsInMotion.Application.Services
{
    public interface IContaminationService
    {
        byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap);
        byte[,] ApplyContaminationMerge(Agent agent, byte[,] contaminationMap);
        bool IsAreaContaminated(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height);
        void UpdateContaminationArea(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height);
        void PrintContaminationState(byte[,] contaminationMap);
        void CopyContaminationMap(byte[,] source, byte[,] destination);
    }
}