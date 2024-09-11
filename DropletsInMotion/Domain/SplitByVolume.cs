using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class SplitByVolume : ICommand
    {
        public string InputName { get; }
        public string OutputName1 { get; }
        public string OutputName2 { get; }
        public int PositionX1 { get; }
        public int PositionY1 { get; }
        public int PositionX2 { get; }
        public int PositionY2 { get; }
        public double Volume { get; }


        public SplitByVolume(string inputName, string outputName1, string outputName2, int positionX1, int positionY1, int positionX2, int positionY2, double volume)
        {
            InputName = inputName;
            OutputName1 = outputName1;
            OutputName2 = outputName2;
            PositionX1 = positionX1;
            PositionY1 = positionY1;
            PositionX2 = positionX2;
            PositionY2 = positionY2;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"SplitByVolume(InputName: {InputName}, OutputName1: {OutputName1}, OutputName2: {OutputName2}, PositionX1: {PositionX1}, PositionY1: {PositionY1}, PositionX2: {PositionX2}, PositionY2: {PositionY2}, Volume: {Volume})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { InputName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { OutputName1, OutputName2 };
        }
    }
}
