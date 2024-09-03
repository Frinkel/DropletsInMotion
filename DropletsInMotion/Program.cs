using DropletsInMotion.Services.Websocket;

namespace DropletsInMotion
{
    // C:\Github\DropletsInMotion\tester.txt
    class Program
    {
        private static readonly bool Development = true;

        static async Task Main(string[] args)
        {
            string? path = null;

            if (!Development)
            {
                while (path == null)
                {
                    Console.Write("Enter the path to your program: ");
                    // TODO: REGEX CHECK? CHECK IF PATH 
                    path = Console.ReadLine();
                }
            }
            else
            {
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
                path = projectDirectory + "/testprogram.txt";
            }


            


            Console.WriteLine($"You entered : {path}");
            string contents = File.ReadAllText(path);
            Console.WriteLine(contents);


            // Start a websocket
            var websocketService = new WebsocketService("http://localhost:5000/ws/");
            var cancellationTokenSource = new CancellationTokenSource();

            await websocketService.StartServerAsync(cancellationTokenSource.Token);

            // To stop the server, you can call cancellationTokenSource.Cancel();
        }
    }
}