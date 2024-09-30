using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Application.Services;

namespace DropletsInMotionTests
{
    public class RouterTests
    {
        
        [Test]
        public void AStarSearchAroundEachother()
        {
            ApplicableFunctions.StateAmount = 0;
            ApplicableFunctions.StateAmountExists = 0;

            ICommand command = new Move("d1", 20, 5);
            ICommand command2 = new Move("d2", 1, 5);
            var commands = new List<ICommand>() { command, command2 };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 5, 5, 1);
            var d2 = new Droplet("d2", 12, 5, 1);
            droplets.Add("d1", d1);
            droplets.Add("d2", d2);

            var board = CreateBoard();

            Router router = new Router(board, droplets);
            router.Seed = 123;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var boardActions = router.Route(droplets, commands, 0);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

            Console.WriteLine($"Amount of states {ApplicableFunctions.StateAmount}");
            Console.WriteLine($"Amount of states that existed {ApplicableFunctions.StateAmountExists}");

            Assert.AreEqual(IsOneGoalState(commands, droplets), true);
        }

        [Test]
        public void AStarSearchAroundEachotherSameSubstance()
        {
            ApplicableFunctions.StateAmount = 0;
            ApplicableFunctions.StateAmountExists = 0;

            ICommand command = new Move("d1", 20, 5);
            ICommand command2 = new Move("d2", 1, 5);
            var commands = new List<ICommand>() { command, command2 };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 5, 5, 1);
            var d2 = new Droplet("d2", 12, 5, 1);
            droplets.Add("d1", d1);
            droplets.Add("d2", d2);

            var board = CreateBoard();

            Router router = new Router(board, droplets);
            router.Seed = 123;
            byte substanceId = router.GetAgentSubstanceId("d1");
            router.UpdateAgentSubstanceId("d2", substanceId);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var boardActions = router.Route(droplets, commands, 0);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

            Console.WriteLine($"Amount of states {ApplicableFunctions.StateAmount}");
            Console.WriteLine($"Amount of states that existed {ApplicableFunctions.StateAmountExists}");

            Assert.AreEqual(IsOneGoalState(commands, droplets), true);
        }

        [Test]
        public void AStarSearchGreatWallOfDmf()
        {
            ApplicableFunctions.StateAmount = 0;
            ApplicableFunctions.StateAmountExists = 0;

            ICommand command = new Move("d1", 15, 10);
            var commands = new List<ICommand>() { command };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 5, 10, 1);
            droplets.Add("d1", d1);

            var board = CreateBoard();
            Router router = new Router(board, droplets);
            router.UpdateContaminationMap(10, 7, 255);
            router.UpdateContaminationMap(10, 8, 255);
            router.UpdateContaminationMap(10, 9, 255);
            router.UpdateContaminationMap(10, 10, 255);
            router.UpdateContaminationMap(10, 11, 255);
            router.UpdateContaminationMap(10, 12, 255);
            router.UpdateContaminationMap(10, 13, 255);
            router.Seed = 123;


            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var boardActions = router.Route(droplets, commands, 0);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

            Console.WriteLine($"Amount of states {ApplicableFunctions.StateAmount}");
            Console.WriteLine($"Amount of states that existed {ApplicableFunctions.StateAmountExists}");


            

            Assert.AreEqual(IsOneGoalState(commands, droplets), true);


        }

        public bool IsOneGoalState(List<ICommand> commands, Dictionary<string, Droplet> droplets)
        {
            foreach (var command in commands)
            {
                Droplet droplet;
                switch (command)
                {
                    case Move moveCommand:
                        droplet = droplets[moveCommand.GetInputDroplets().First()];
                        if (droplet.PositionX == moveCommand.PositionX && droplet.PositionY == moveCommand.PositionY)
                            return true;
                        break;
                    default:
                        return false;
                        break;
                }
            }

            return false;
        }

        public Electrode[][] CreateBoard()
        {
            Electrode[][] board = new Electrode[32][];
            board = new Electrode[32][];
            for (int i = 0; i < 32; i++)
            {
                board[i] = new Electrode[20];
                for (int j = 0; j < 20; j++)
                {
                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
                }
            }
            return board;
        }
    }
}