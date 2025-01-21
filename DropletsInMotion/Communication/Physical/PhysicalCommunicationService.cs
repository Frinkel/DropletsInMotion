using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Communication.Models;
using System.IO.Ports;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Repositories;
using System.Collections.Concurrent;
using System.Text;


namespace DropletsInMotion.Communication.Physical
{
    public class PhysicalCommunicationService : ICommunicationService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPlatformRepository _platformRepository;
        private SerialPort _serialPort;
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<List<BoardAction>> _queue = new ConcurrentQueue<List<BoardAction>>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _workerTask;
        private bool _disposed = false;

        public double? Time = null;
        public bool HighVoltage = false;

        public PhysicalCommunicationService(ILogger logger, IPlatformRepository platformRepository)
        {
            _logger = logger;
            _platformRepository = platformRepository;
            _serialPort = new SerialPort();

            _workerTask = Task.Run(ProcessQueueAsync);
        }

        public async Task StartCommunication()
        {
            string portName = GetPortName();

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = 115200,           // Communication speed
                Parity = Parity.None,        // No parity bit
                DataBits = 8,                // 8 data bits
                StopBits = StopBits.One,     // 1 stop bit
                Handshake = Handshake.None,   // No flow control
                Encoding = Encoding.GetEncoding(28591)
            };

            _serialPort.Open();
            _logger.Info($"Serial port '{_serialPort.PortName}' opened");
            _logger.Info("Sending initial commands..");
            SendCommand("shv 1 280 \r");
            await Task.Delay(100);
            
            SendCommand("hvpoe 1 1 \r");
            HighVoltage = true;
            await Task.Delay(100);

            SendCommand("clra 0 \r");
            await Task.Delay(100);

            SendCommand("clra 1 \r");

            await Task.Delay(100);
        }

        public async Task StopCommunication()
        {
            // Kill the background task
            _cancellationTokenSource.Cancel();
            _signal.Release();
            _workerTask.Wait();
            _workerTask.Dispose();

            // Closing command
            SendCommand("hvpoe 1 0");
            HighVoltage = false;
            await Task.Delay(100);

            SendCommand("clra 0 \r");
            await Task.Delay(100);

            SendCommand("clra 1 \r");
            await Task.Delay(100);


            _serialPort.Close();
            _logger.Info($"Serial port '{_serialPort.PortName}' closed");
        }

        public async Task SendActions(List<BoardAction> boardActionDtoList)
        {
            if (boardActionDtoList == null)
            {
                _logger.Warning("Attempted to enqueue a null value.");
                return;
            }

            // We need to turn on high voltage if it is off
            if (!HighVoltage)
            {
                SendCommand("hvpoe 1 1 \r");
                await Task.Delay(100);

                SendCommand("clra 0 \r");
                await Task.Delay(100);

                SendCommand("clra 1 \r");

                await Task.Delay(100);
            }

            _queue.Enqueue(new List<BoardAction>(boardActionDtoList));
            _logger.Debug($"Enqueued list with {boardActionDtoList.Count} items.");
            _signal.Release();
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(_cancellationTokenSource.Token);

                    if (_queue.TryDequeue(out var itemList))
                    {
                        _logger.Debug($"Dequeued list. Count: {itemList?.Count ?? 0}");
                        if (itemList == null)
                        {
                            _logger.Warning("Dequeued an empty or null list.");
                        }
                        else
                        {
                            await ProcessItemAsync(itemList);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error processing queue: {ex.Message}");
                }
            }
        }

        protected async Task ProcessItemAsync(List<BoardAction> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                _logger.Debug("Terminator received.");

                // Turn off high voltage
                SendCommand("hvpoe 1 0 \r");
                HighVoltage = false;
                await Task.Delay(100);

                Time = null;
                return;
            }

            _logger.Debug($"Processing list with {itemList.Count} items.");

            var len = _platformRepository.Board.Length;

            var groupedActions = itemList
                .GroupBy(action =>
                {
                    var id = action.ElectrodeId - 1;
                    var x = id % len;
                    var y = id / len;
                    var driverId = _platformRepository.Board[x][y].DriverId;
                    return (action.Time, driverId, action.Action);
                })
                .ToList();

            foreach (var group in groupedActions)
            {
                var (time, driverId, action) = group.Key;
                Console.WriteLine();
                _logger.Debug($"Time: {time}");

                if (Time == null)
                {
                    Time = time;
                }

                if (Time < time)
                {
                    var diff = time - Time;
                    await Task.Delay((int)(diff * 1000));
                    Time = time;
                }
                else
                {
                    await Task.Delay(100);
                }


                var baseCommand = "";

                baseCommand += action == 1 ? "setel" : "clrel";
                baseCommand += " ";
                baseCommand += driverId;
                baseCommand += " ";

                var boardActionsList = group.ToList();

                for (int i = 0; i < boardActionsList.Count; i += 8)
                {
                    var chunk = boardActionsList.Skip(i).Take(8);
                    var command = baseCommand;

                    foreach (var boardAction in chunk)
                    {
                        var id = boardAction.ElectrodeId - 1;
                        var x = id % len;
                        var y = id / len;

                        var electrodeId = _platformRepository.Board[x][y].ElectrodeId.ToString();

                        if (!command.Split(" ").Contains(electrodeId))
                        {
                            command += electrodeId;
                            command += " ";
                        }
                    }
                    command = command.TrimEnd();
                    SendCommand(command);
                }
            }

            await Task.CompletedTask;
        }




        public Task<double> SendSensorRequest(Sensor sensor, SensorHandler sensorHandler, double time)
        {
            throw new NotImplementedException();
        }

        public Task WaitForConnection()
        {
            return Task.CompletedTask;
        }

        public event EventHandler? ClientDisconnected;
        public Task<bool> SendActuatorRequest(Actuator actuator, double time)
        {
            throw new NotImplementedException();
        }

        public async Task<double> SendTimeRequest()
        {
            return 0d;
        }


        private void SendCommand(string command)
        {
            if (_serialPort.IsOpen)
            {
                _logger.Debug($"Sent: {command}");
                var sendCommand = command + " \r";
                char[] output = sendCommand.ToCharArray();
                _serialPort.Write(output, 0, output.Length);
            }
            else
            {
                throw new InvalidOperationException($"Serial port '{_serialPort.PortName}' is not open.");
            }
        }

        private string GetPortName()
        {
            _logger.WriteColor("Available Serial Ports:");
            var availablePorts = SerialPort.GetPortNames().ToList();
            foreach (string port in availablePorts)
            {
                _logger.WriteColor(port);
            }
            _logger.WriteEmptyLine(1);

            string? portName = null;

            while (string.IsNullOrEmpty(portName))
            {
                _logger.WriteColor("Choose port:");
                portName = Console.ReadLine();
                if (portName == null)
                {
                    _logger.WriteEmptyLine(1);
                    _logger.WriteColor("Please choose a valid port.", ConsoleColor.DarkRed, ConsoleColor.DarkYellow);
                    _logger.WriteEmptyLine(1);
                    continue;
                }

                portName = availablePorts.Find(p => p.ToLower() == portName.ToLower());

                if (string.IsNullOrEmpty(portName))
                {
                    
                    _logger.WriteEmptyLine(1);
                    _logger.WriteColor("Please choose a valid port.", ConsoleColor.DarkRed, ConsoleColor.DarkYellow);
                    _logger.WriteEmptyLine(1);
                    portName = null;
                }
            }
            
            _logger.Info($"Chosen serial port: {portName}");

            return portName;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopCommunication().GetAwaiter().GetResult();
                _disposed = true;
            }
        }
    }
}
