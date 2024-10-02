//using DropletsInMotion.Application.Services;
//using DropletsInMotion.Application.Services.Routers;
//using DropletsInMotion.Infrastructure.Models.Commands;
//using DropletsInMotion.Infrastructure.Models.Domain;

//namespace DropletsInMotionTests
//{
//    public class SchedularTests
//    {
//        [Test]
//        public void MergePosition()
//        {
//            IDropletCommand mergeCommand = new Merge("d1", "d2", "d3", 5, 5);

//            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
//            var d1 = new Droplet("d1", 8, 0, 1);
//            var d2 = new Droplet("d2", 0, 0, 1);

//            droplets.Add("d1", d1);
//            droplets.Add("d2", d2);

//            SchedulerService scheduler = new SchedulerService();
//            //var mergePositions = scheduler.ScheduleCommand(mergeCommand, droplets);

//            var board = CreateBoard();

//            RouterService router = new Router(board, droplets);

//            router.UpdateContaminationMap(2, 0, 2);
//            router.UpdateContaminationMap(3, 0, 2);
//            router.UpdateContaminationMap(4, 0, 2);
//            router.UpdateContaminationMap(5, 0, 2);
//            router.UpdateContaminationMap(6, 0, 2);



//            var optimalPostion = scheduler.ScheduleCommand(mergeCommand, droplets, router.GetAgents(), router.GetContaminationMap());

//            Assert.AreEqual(optimalPostion.Value.Item1.optimalX, 6);
//            Assert.AreEqual(optimalPostion.Value.Item1.optimalY, 1);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalX, 4);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalY, 1);

//        }

//        [Test]
//        public void MergePositionCloseToEachother()
//        {
//            IDropletCommand mergeCommand = new Merge("d1", "d2", "d3", 5, 5);

//            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
//            var d1 = new Droplet("d1", 6, 0, 1);
//            var d2 = new Droplet("d2", 4, 0, 1);

//            droplets.Add("d1", d1);
//            droplets.Add("d2", d2);

//            SchedulerService scheduler = new SchedulerService();
//            //var mergePositions = scheduler.ScheduleCommand(mergeCommand, droplets);

//            var board = CreateBoard();

//            RouterService router = new Router(board, droplets);



//            var optimalPostion = scheduler.ScheduleCommand(mergeCommand, droplets, router.GetAgents(), router.GetContaminationMap());

//            Assert.AreEqual(optimalPostion.Value.Item1.optimalX, 6);
//            Assert.AreEqual(optimalPostion.Value.Item1.optimalY, 0);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalX, 4);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalY, 0);

//        }


//        [Test]
//        public void SplitPosition()
//        {
//            IDropletCommand splitCommand = new SplitByVolume("d1", "d2", "d3", 0, 0, 8, 0, 0.5);

//            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
//            var d1 = new Droplet("d1", 5, 5, 1);

//            droplets.Add("d1", d1);

//            SchedulerService scheduler = new SchedulerService();
//            //var mergePositions = scheduler.ScheduleCommand(mergeCommand, droplets);

//            var board = CreateBoard();

//            RouterService router = new Router(board, droplets);

//            var optimalPostion = scheduler.ScheduleCommand(splitCommand, droplets, router.GetAgents(), router.GetContaminationMap());

//            Assert.AreEqual(optimalPostion.Value.Item1.optimalX, 4);
//            Assert.AreEqual(optimalPostion.Value.Item1.optimalY, 0);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalX, 6);
//            Assert.AreEqual(optimalPostion.Value.Item2.optimalY, 0);

//        }


//        //[Test]
//        //public void SplitPosition()
//        //{
//        //    IDropletCommand splitCommand = new SplitByVolume("d1", "d2", "d3", 1, 0, 18, 0, 2);
//        //    var commands = new List<IDropletCommand>() { splitCommand };

//        //    Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
//        //    var d1 = new Droplet("d1", 2, 7, 1);

//        //    droplets.Add("d1", d1);

//        //    Scheduler scheduler = new Scheduler();
//        //    var splitPositions = scheduler.ScheduleCommand(splitCommand, droplets);

//        //    Console.WriteLine(splitPositions);

//        //    Assert.AreEqual(splitPositions.Item1.d1OptimalX, 1);
//        //    Assert.AreEqual(splitPositions.Item1.d1OptimalY, 7);

//        //    Assert.AreEqual(splitPositions.Item2.d2OptimalX, 3);
//        //    Assert.AreEqual(splitPositions.Item2.d2OptimalY, 7);

//        //}

//        public Electrode[][] CreateBoard()
//        {
//            Electrode[][] board = new Electrode[32][];
//            board = new Electrode[32][];
//            for (int i = 0; i < 32; i++)
//            {
//                board[i] = new Electrode[20];
//                for (int j = 0; j < 20; j++)
//                {
//                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
//                }
//            }
//            return board;
//        }

//    }
//}