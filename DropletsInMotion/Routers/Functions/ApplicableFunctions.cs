using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Routers.Functions
{
    public static class ApplicableFunctions
    {
        public static bool IsApplicable(Tuple<Types.RouteAction, List<string>> action, Dictionary<string, Agent> agents, byte[,] contamination)
        {
            switch (action.Item1.Type)
            {
                case Types.ActionType.NoOp:
                    return true;

                case Types.ActionType.Move:
                    Agent agent = agents[action.Item2.First()];
                    return IsMoveApplicable(action.Item1, agent, agents, contamination);
                    break;


                default:
                    throw new InvalidOperationException("An illegal action was tried!");
            }

            return false;
        }
        public static bool IsMoveApplicable(Types.RouteAction action, Agent agent, Dictionary<string, Agent> agents, byte[,] contamination)
        {
            var deltaX = agent.PositionX + action.DropletXDelta;
            var deltaY = agent.PositionY + action.DropletYDelta;
            //Check out of bounds
            if (deltaX < 0 || deltaX >= contamination.GetLength(0) || deltaY < 0 || deltaY >= contamination.GetLength(1))
            {
                return false;
            }
            // check for contaminations
            if (contamination[deltaX, deltaY] != 0 && contamination[deltaX, deltaY] != agent.SubstanceId)
            {
                return false;
            }
            //Check for going near other agents of the same substance
            foreach (var otherAgentKvp in agents )
            {
                var otherAgent = otherAgentKvp.Value;
                if (otherAgent.SubstanceId != agent.SubstanceId || otherAgent.DropletName == agent.DropletName) continue;
                if (Math.Abs(otherAgent.PositionX - deltaX) <= 1 && Math.Abs(otherAgent.PositionY - deltaY) <= 1)
                {
                    return false;
                }
            }
            //returns true
            return true;
        }

        public static byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            void ApplyIfInBounds(int xPos, int yPos)
            {
                if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
                {
                    contaminationMap[xPos, yPos] = (byte)(contaminationMap[xPos, yPos] == 0 || (byte)(contaminationMap[xPos, yPos]) == agent.SubstanceId ? agent.SubstanceId : 255);
                }
            }

            ApplyIfInBounds(x, y);
            ApplyIfInBounds(x + 1, y);
            ApplyIfInBounds(x - 1, y);
            ApplyIfInBounds(x, y + 1);
            ApplyIfInBounds(x, y - 1);

            ApplyIfInBounds(x + 1, y + 1);
            ApplyIfInBounds(x + 1, y - 1);
            ApplyIfInBounds(x - 1, y + 1);
            ApplyIfInBounds(x - 1, y - 1);

            return contaminationMap;
        }
        // TEMP FUNCTIONS
        public static void PrintContaminationState(byte[,] contaminationMap)
        {
            // Determine the maximum number of digits for proper alignment
            int maxDigits = 3;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            for (int j = 0; j < colCount; j++)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    byte value = contaminationMap[i, j];

                    // Set the color based on the value using a hash function
                    SetColorForValue(value);

                    // Print each ElectrodeId with a fixed width
                    Console.Write(value.ToString().PadLeft(maxDigits) + " ");

                    // Reset color after printing
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        private static void SetColorForValue(byte value)
        {
            // Handle 0 as a special case
            if (value == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                // Get a unique color for each value using a hash function
                var rgbColor = GetRGBFromHash(value);

                // Convert RGB to closest ConsoleColor
                var consoleColor = GetConsoleColorFromRGB(rgbColor.R, rgbColor.G, rgbColor.B);
                Console.BackgroundColor = consoleColor;
            }
        }

        // Method to generate an RGB color from a hash of the value
        private static (byte R, byte G, byte B) GetRGBFromHash(byte value)
        {
            // Hash the value (using a simple hash function for demonstration)
            int hash = (int)(value * 2654435761); // A simple, fast hash function

            // Extract RGB components from the hash
            byte r = (byte)((hash >> 16) & 0xFF);
            byte g = (byte)((hash >> 8) & 0xFF);
            byte b = (byte)(hash & 0xFF);

            return (r, g, b);
        }

        // Convert RGB to a ConsoleColor (approximation due to limited colors in Console)
        private static ConsoleColor GetConsoleColorFromRGB(byte r, byte g, byte b)
        {
            // Use a simple mapping of RGB to the nearest ConsoleColor
            if (r > 128)
            {
                if (g > 128)
                {
                    if (b > 128) return ConsoleColor.DarkRed;
                    else return ConsoleColor.Yellow;
                }
                else
                {
                    if (b > 128) return ConsoleColor.Magenta;
                    else return ConsoleColor.Red;
                }
            }
            else
            {
                if (g > 128)
                {
                    if (b > 128) return ConsoleColor.Cyan;
                    else return ConsoleColor.Green;
                }
                else
                {
                    if (b > 128) return ConsoleColor.Blue;
                    else return ConsoleColor.DarkCyan;
                }
            }
        }
    }


}
