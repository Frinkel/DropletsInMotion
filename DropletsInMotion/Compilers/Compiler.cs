using DropletsInMotion.Domain;
using DropletsInMotion.Compilers.Models;



namespace DropletsInMotion.Compilers
{
    public class Compiler
    {
        public Electrode[][] Board { get; set; }
        public List<Droplet> Droplets { get; } = new List<Droplet>();
        public List<Move> Moves { get; } = new List<Move>();

        public Compiler(List<Droplet> droplets, List<Move> moves)
        {
            Droplets = droplets;
            Moves = moves;

            Board = new Electrode[32][];

            for (int i = 0; i < 32; i++)
            {
                Board[i] = new Electrode[20];

                for (int j = 0; j < 20; j++)
                {
                    Board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
                }
            }
        }

        public List<BoardActionDto> Compile()
        {
            List < BoardActionDto > boardActions = new List <BoardActionDto >();

            foreach (Move move in Moves)
            {
                boardActions.Concat(CompileMove(move));
            }

            return boardActions;
        }

        public List<BoardActionDto> CompileMove(Move move)
        {
            List<BoardActionDto> boardActions = new List<BoardActionDto>();
            string dropletName = move.DropletName;
            Droplet droplet = Droplets.Find(d => d.Name == dropletName);

            if (droplet == null)
            {
                throw new InvalidOperationException($"Droplet with name {dropletName} not found.");
            }

            int currentX = droplet.PositionX;
            int currentY = droplet.PositionY;
            int targetX = move.NewPositionX;
            int targetY = move.NewPositionY;

            decimal time = 0m;

            // Move horizontally first (if needed)
            while (currentX != targetX)
            {
                if (currentX < targetX)
                {
                    currentX++;
                }
                else
                {
                    currentX--;
                }

                // Turn on the electrode at the new position
                int electrodeId = Board[currentX][currentY].Id;
                boardActions.Add(new BoardActionDto(electrodeId, 1, time));
                time += 0.5m; // Increment time (example value, adjust as needed)

                // Turn off the electrode at the previous position
                electrodeId = Board[droplet.PositionX][droplet.PositionY].Id;
                boardActions.Add(new BoardActionDto(electrodeId, 0, time));
                time += 0.5m; // Increment time (example value, adjust as needed)

                droplet.PositionX = currentX;
            }

            // Move vertically (if needed)
            while (currentY != targetY)
            {
                if (currentY < targetY)
                {
                    currentY++;
                }
                else
                {
                    currentY--;
                }

                // Turn on the electrode at the new position
                int electrodeId = Board[currentX][currentY].Id;
                boardActions.Add(new BoardActionDto(electrodeId, 1, time));
                time += 0.5m; // Increment time (example value, adjust as needed)

                // Turn off the electrode at the previous position
                electrodeId = Board[droplet.PositionX][droplet.PositionY].Id;
                boardActions.Add(new BoardActionDto(electrodeId, 0, time));
                time += 0.5m; // Increment time (example value, adjust as needed)

                droplet.PositionY = currentY;
            }

            // Update the droplet's final position
            droplet.PositionX = targetX;
            droplet.PositionY = targetY;

            foreach (var action in boardActions)
            {
                Console.WriteLine(action);
            }
            return boardActions;
        }

        public override string ToString()
        {
            var boardString = new System.Text.StringBuilder();

            for (int i = 0; i < Board.Length; i++)
            {
                for (int j = 0; j < Board[i].Length; j++)
                {
                    boardString.Append(Board[i][j].ToString() + "\t");
                }
                boardString.AppendLine(); // New line after each row
            }

            return boardString.ToString();
        }
    }
}
