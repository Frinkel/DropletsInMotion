using DropletsInMotion.Services.Websocket;

namespace DropletsInMotion
{
    class Program
    {
        // Global
        private static readonly bool Development = true;
        private static readonly string DevelopmentPath = "/testprogram.txt";

        static async Task Main(string[] args)
        {
            string path = GetPathToProgram();

            string contents = File.ReadAllText(path);
            Console.WriteLine(contents);


            await StartWebSocket();


        }

        public static string GetPathToProgram()
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
                path = projectDirectory + DevelopmentPath;
            }

            return path;
        }

        public async static Task StartWebSocket()
        {
            // Start a websocket
            var websocketService = new WebsocketService("http://localhost:5000/ws/");
            var cancellationTokenSource = new CancellationTokenSource();

            await websocketService.StartServerAsync(cancellationTokenSource.Token);

            // To stop the server, you can call cancellationTokenSource.Cancel();
        }
    }
}