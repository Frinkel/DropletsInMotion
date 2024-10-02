namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class Merge : ICommand
    {
        public string InputName1 { get; }
        public string InputName2 { get; }
        public string OutputName { get; }
        public int PositionX { get; }
        public int PositionY { get; }

        public Merge(string inputName1, string inputName2, string outputName, int positionX, int positionY)
        {
            InputName1 = inputName1;
            InputName2 = inputName2;
            OutputName = outputName;
            PositionX = positionX;
            PositionY = positionY;
        }

        public override string ToString()
        {
            return $"Merge(InputName1: {InputName1}, InputName2: {InputName2}, OutputName: {OutputName}, PositionX: {PositionX}, PositionY: {PositionY})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { InputName1, InputName2 };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { OutputName };
        }
    }
}
