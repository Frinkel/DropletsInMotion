using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Simulator.Models;
using DropletsInMotion.Communication.Simulator.Services;
using System.Text.Json;
using DropletsInMotion.Communication.Models;
using System.Reflection;

namespace DropletsInMotion.Communication.Simulator;

public class SimulationCommunicationService : ICommunicationService
{
    private readonly IWebsocketService _websocketService;

    private CancellationTokenSource? _cancellationTokenSource;
    public Task? WebSocketTask { get; private set; }

    public event EventHandler? ClientDisconnected;

    public SimulationCommunicationService(IWebsocketService websocketService)
    {
        _websocketService = websocketService;
        _websocketService.ClientDisconnected += OnClientDisconnected;
    }

    public async Task StartCommunication()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        WebSocketTask = Task.Run(async () =>
        {
            await _websocketService.StartServerAsync(_cancellationTokenSource.Token);
        });

        await Task.Delay(100);
    }

    public async Task StopCommunication()
    {
        await _websocketService.CloseAllConnectionsAsync();
    }

    public async Task WaitForConnection()
    {
        await _websocketService.WaitForClientConnectionAsync();
    }

    public async Task SendActions(List<BoardAction> boardActionDtoList)
    {
        Queue<ActionQueueItem> actionQueue = new Queue<ActionQueueItem>();

        foreach (var boardActionDto in boardActionDtoList)
        {
            ActionItem actionItem = new ActionItem(ActionTypes.Electrode, boardActionDto.ElectrodeId, boardActionDto.Action);
            ActionQueueItem actionQueueItem = new ActionQueueItem(actionItem, boardActionDto.Time);

            actionQueue.Enqueue(actionQueueItem);
        }

        WebSocketMessage<Queue<ActionQueueItem>> actionDto =
            new WebSocketMessage<Queue<ActionQueueItem>>(WebSocketMessageTypes.Action, actionQueue);

        string serializedObject = JsonSerializer.Serialize(actionDto);
        await _websocketService.SendMessageToAllAsync(serializedObject, _cancellationTokenSource.Token);
    }

    // TODO: We should return something useful in this function!
    public async Task<double> SendRequest(Sensor sensor, Handler handler, double time)
    {

        RequestWrapper requestWrapper = new RequestWrapper(handler.Request, time);

        WebSocketMessage<RequestWrapper> sensorRequestDto =
            new WebSocketMessage<RequestWrapper>(WebSocketMessageTypes.Sensor, requestWrapper);

        string serializedObject = JsonSerializer.Serialize(sensorRequestDto);

        var response = await _websocketService.SendRequestAndWaitForResponseAsync(sensorRequestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);



        switch (response.Type)
        {
            case (WebSocketResponseTypes.Sensor):
                var simulationSensor =
                    JsonSerializer.Deserialize<SimulationSensor>((response.Data?.ToString()) ?? string.Empty);
                
                if (simulationSensor == null)
                {
                    throw new Exception($"SimulationSensor data was faulty: {response.Data}");
                }

                PropertyInfo? propertyInfo = simulationSensor?.GetType()?.GetProperty(handler.Response, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Sensor {simulationSensor.Name} did not contian property {handler.Response}");
                }

                double sensorValue = Convert.ToDouble(propertyInfo.GetValue(simulationSensor));
                return sensorValue;

                break;
            default:
                throw new Exception($"Unexpected response type: {response.Type}");
        }


        //WebSocketMessage<RequestWrapper> sensorRequestDto =
        //    new WebSocketMessage<RequestWrapper>(WebSocketMessageTypes.SimulationSensor, new RequestWrapper(sensorRequest.Id, sensorRequest.Time));

        //string serializedObject = JsonSerializer.Serialize(sensorRequestDto);

        ////Console.WriteLine($"Request sent with request id {sensorRequestDto.RequestId}");
        //Console.WriteLine(sensorRequestDto);

        //var response = await _websocketService.SendRequestAndWaitForResponseAsync(sensorRequestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);



        //// Handle different response types
        //switch (response.Type)
        //{
        //    case (WebSocketResponseTypes.SimulationSensor):
        //        var simulationSensor = JsonSerializer.Deserialize<SimulationSensor>((response.Data?.ToString()) ?? string.Empty);

        //        if (simulationSensor == null)
        //        {
        //            throw new Exception($"SimulationSensor data was faulty: {response.Data}");
        //        }

        //        // Handle simulationSensor types
        //        switch (simulationSensor.Type)
        //        {
        //            case SensorTypes.Rgb:
        //                Console.WriteLine($"Color = ({simulationSensor.ValueRed}, {simulationSensor.ValueGreen}, {simulationSensor.ValueBlue})");
        //                // TODO: This should return something useful!
        //                break;
        //            case SensorTypes.Temperature:
        //                Console.WriteLine($"Temperature = {simulationSensor.ValueTemperature}");
        //                // TODO: This should return something useful!
        //                break;
        //            default:
        //                throw new Exception("The simulationSensor type was not recognized!");
        //        }

        //        break;

        //    default:
        //        throw new Exception("The response type was not recognized!");
        //}

    }

    public async Task<bool> IsClientConnected()
    {
        var amountClients = _websocketService.GetNumberOfConnectedClients();
        //Console.WriteLine($"Amount: {amountClients}");
        return amountClients > 0;
    }

    public async Task<bool> IsConnectionOpen()
    {
        return _websocketService.IsWebsocketRunning();
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        ClientDisconnected?.Invoke(this, EventArgs.Empty);
    }
}
