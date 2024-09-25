using DropletsInMotion.Domain;
using DropletsInMotion.Routers;
using DropletsInMotion.Routers.Functions;
using DropletsInMotion.Schedulers;

namespace DropletsInMotionTests
{
    public class SchedularTests
    {
        [Test]
        public void MergePosition()
        {
            ICommand mergeCommand = new Merge("d1", "d2", "d3", 5, 5); 
            var commands = new List<ICommand>() { mergeCommand };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 8, 0, 1);
            var d2 = new Droplet("d2", 0, 0, 1);

            droplets.Add("d1", d1);
            droplets.Add("d2", d2);

            Scheduler scheduler = new Scheduler();
            //var mergePositions = scheduler.ScheduleCommand(mergeCommand, droplets);

            var board = CreateBoard();

            Router router = new Router(board, droplets);

            router.UpdateContaminationMap(2, 0, 2);
            router.UpdateContaminationMap(3, 0, 2);
            router.UpdateContaminationMap(4, 0, 2);
            router.UpdateContaminationMap(5, 0, 2);
            router.UpdateContaminationMap(6, 0, 2);


            ApplicableFunctions.PrintContaminationState(router.GetContaminationMap());

            scheduler.ScheduleCommand(commands, droplets, router.GetAgents(), router.GetContaminationMap());

            Assert.AreEqual(true, true);

        }

        //[Test]
        //public void SplitPosition()
        //{
        //    ICommand splitCommand = new SplitByVolume("d1", "d2", "d3", 1, 0, 18, 0, 2);
        //    var commands = new List<ICommand>() { splitCommand };

        //    Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
        //    var d1 = new Droplet("d1", 2, 7, 1);

        //    droplets.Add("d1", d1);

        //    Scheduler scheduler = new Scheduler();
        //    var splitPositions = scheduler.ScheduleCommand(splitCommand, droplets);

        //    Console.WriteLine(splitPositions);

        //    Assert.AreEqual(splitPositions.Item1.d1OptimalX, 1);
        //    Assert.AreEqual(splitPositions.Item1.d1OptimalY, 7);

        //    Assert.AreEqual(splitPositions.Item2.d2OptimalX, 3);
        //    Assert.AreEqual(splitPositions.Item2.d2OptimalY, 7);

        //}

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