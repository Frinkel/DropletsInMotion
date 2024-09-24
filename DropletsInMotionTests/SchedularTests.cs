using DropletsInMotion.Domain;
using DropletsInMotion.Routers;

namespace DropletsInMotionTests
{
    public class SchedularTests
    {
        [Test]
        public void MergePosition()
        {
            ICommand mergeCommand = new Merge("d1", "d2", "d3", 10, 10); 
            var commands = new List<ICommand>() { mergeCommand };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 5, 6, 1);
            var d2 = new Droplet("d2", 11, 6, 1);

            droplets.Add("d1", d1);
            droplets.Add("d2", d2);

            Scheduler scheduler = new Scheduler();
            var mergePositions = scheduler.ScheduleCommand(mergeCommand, droplets);

            Console.WriteLine(mergePositions);

            Assert.AreEqual(mergePositions.Item1.d1OptimalX, 9);
            Assert.AreEqual(mergePositions.Item1.d1OptimalY, 10);

            Assert.AreEqual(mergePositions.Item2.d2OptimalX, 11);
            Assert.AreEqual(mergePositions.Item2.d2OptimalY, 10);

        }

        [Test]
        public void SplitPosition()
        {
            ICommand splitCommand = new SplitByVolume("d1", "d2", "d3", 1, 0, 18, 0, 2);
            var commands = new List<ICommand>() { splitCommand };

            Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
            var d1 = new Droplet("d1", 2, 7, 1);

            droplets.Add("d1", d1);

            Scheduler scheduler = new Scheduler();
            var splitPositions = scheduler.ScheduleCommand(splitCommand, droplets);

            Console.WriteLine(splitPositions);

            Assert.AreEqual(splitPositions.Item1.d1OptimalX, 1);
            Assert.AreEqual(splitPositions.Item1.d1OptimalY, 7);

            Assert.AreEqual(splitPositions.Item2.d2OptimalX, 3);
            Assert.AreEqual(splitPositions.Item2.d2OptimalY, 7);

        }

    }
}