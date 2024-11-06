using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

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

        public byte[,] ReserveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents,
            byte[,] contaminationMap);

        byte[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePosition, byte[,] contaminationMap);
        byte[,] ApplyContaminationSplit(Agent inputAgent, ScheduledPosition splitPositions, byte[,] contaminationMap);
    }
}