using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Simulator.Models;
using DropletsInMotion.Communication.Simulator.Services;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Communication.Simulator;

internal class SimulationCommunicationService : ICommunicationService
{
    private readonly IWebsocketService _websocketService;

    private CancellationTokenSource? _cancellationTokenSource;
    public Task? WebSocketTask { get; private set; }

    public SimulationCommunicationService(IWebsocketService websocketService)
    {
        _websocketService = websocketService;
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
    public async Task SendRequest(BoardSensorRequest sensorRequest)
    {

        WebSocketMessage<SensorRequest> sensorRequestDto =
            new WebSocketMessage<SensorRequest>(WebSocketMessageTypes.Sensor, new SensorRequest(sensorRequest.Id, sensorRequest.Time));

        string serializedObject = JsonSerializer.Serialize(sensorRequestDto);

        //Console.WriteLine($"Request sent with request id {sensorRequestDto.RequestId}");
        Console.WriteLine(sensorRequestDto);

        var response = await _websocketService.SendRequestAndWaitForResponseAsync(sensorRequestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);

        

        // Handle different response types
        switch (response.Type)
        {
            case (WebSocketResponseTypes.Sensor):
                var sensor = JsonSerializer.Deserialize<Sensor>((response.Data?.ToString()) ?? string.Empty);

                if (sensor == null)
                {
                    throw new Exception($"Sensor data was faulty: {response.Data}");
                }
                
                // Handle sensor types
                switch (sensor.Type)
                {
                    case SensorTypes.Rgb:
                        Console.WriteLine($"Color = ({sensor.ValueRed}, {sensor.ValueGreen}, {sensor.ValueBlue})");
                        // TODO: This should return something useful!
                        break;
                    case SensorTypes.Temperature:
                        Console.WriteLine($"Temperature = {sensor.ValueTemperature}");
                        // TODO: This should return something useful!
                        break;
                    default:
                        throw new Exception("The sensor type was not recognized!");
                }

                break;
                
            default:
                throw new Exception("The response type was not recognized!");
        }
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
}
