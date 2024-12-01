using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Application.Services
{
    public interface IContaminationService
    {
        //byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap);
        //byte[,] ApplyContaminationMerge(Agent agent, byte[,] contaminationMap);
        bool IsAreaContaminated(List<int>[,] contaminationMap, int substanceId, int startX, int startY, int width, int height);
        void UpdateContaminationArea(List<int>[,] contaminationMap, int substanceId, int startX, int startY, int width, int height);
        void PrintContaminationState(byte[,] contaminationMap);
        void CopyContaminationMap(List<int>[,] source, List<int>[,] destination);
        //byte[,] ApplyContaminationWithSize(Agent agent, byte[,] contaminationMap);
        //void ApplyIfInBoundsWithContamination(byte[,] contaminationMap, int xPos, int yPos, byte substanceId);

        //public byte[,] ReserveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents,
        //    byte[,] contaminationMap);

        List<int>[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePosition, List<int>[,] contaminationMap);
        List<int>[,] ApplyContaminationSplit(Agent inputAgent, ScheduledPosition splitPositions, List<int>[,] contaminationMap);
        void ApplyIfInBounds(List<int>[,] contaminationMap, int xPos, int yPos, int substanceId);
        List<int>[,] ApplyContamination(Agent agent, List<int>[,] contaminationMap);
        List<int>[,] ApplyContaminationWithSize(Agent agent, List<int>[,] contaminationMap);
        bool IsConflicting(List<int>[,] contaminationMap, int xPos, int yPos, int substanceId);
        int GetResultingSubstanceId(int substance1, int substance2);
        List<int>[,] CloneContaminationMap(List<int>[,] contaminationMap);
        List<int>[,] CreateContaminationMap(int rows, int cols);
        public List<int>[,] ReserveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents,
            List<int>[,] contaminationMap);

        int GetSubstanceId(string substanceName);
        byte[,] CreateByteArrayFromContaminationMap(List<int>[,] contaminationMap);
        public void PrintContaminationMap(List<int>[,] contaminationMap);

        public void ApplyContamination(Agent agent, State state);
        public bool IsConflicting(List<int> contaminationValues, int substanceId);

    }
}