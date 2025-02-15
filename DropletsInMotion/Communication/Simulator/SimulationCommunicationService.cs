﻿using DropletsInMotion.Communication.Simulator.Models;
using DropletsInMotion.Communication.Simulator.Services;
using System.Text.Json;
using DropletsInMotion.Communication.Models;
using System.Reflection;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Infrastructure;

namespace DropletsInMotion.Communication.Simulator;

public class SimulationCommunicationService : ICommunicationService
{
    private readonly IWebsocketService _websocketService;
    private readonly ILogger _logger;

    private CancellationTokenSource? _cancellationTokenSource;
    public Task? WebSocketTask { get; private set; }

    public event EventHandler? ClientDisconnected;


    public SimulationCommunicationService(IWebsocketService websocketService, ILogger logger)
    {
        _websocketService = websocketService;
        _logger = logger;
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
        if (boardActionDtoList.Count == 0)
        {
            return;
        }

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


    public async Task<double> SendSensorRequest(Sensor sensor, SensorHandler sensorHandler, double time)
    {
        RequestWrapper requestWrapper = new RequestWrapper(sensorHandler.Request, time);

        WebSocketMessage<RequestWrapper> sensorRequestDto =
            new WebSocketMessage<RequestWrapper>(WebSocketMessageTypes.Sensor, requestWrapper);

        string serializedObject = JsonSerializer.Serialize(sensorRequestDto);

        var response = await _websocketService.SendRequestResponseAsync(sensorRequestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);


        switch (response.Type)
        {
            case (WebSocketResponseTypes.Sensor):
                var simulationSensor =
                    JsonSerializer.Deserialize<SimulationSensor>((response.Data?.ToString()) ?? string.Empty);
                
                if (simulationSensor == null)
                {
                    throw new Exception($"SimulationSensor data was faulty: {response.Data}");
                }

                PropertyInfo? propertyInfo = simulationSensor?.GetType()?.GetProperty(sensorHandler.Response, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Sensor {simulationSensor.Name} did not contian property {sensorHandler.Response}");
                }

                double sensorValue = Convert.ToDouble(propertyInfo.GetValue(simulationSensor));
                return sensorValue;

            case (WebSocketResponseTypes.Error):
                throw new Exception($"The sensor {sensor.Name} with id {sensor.SensorId} was not found.");

            default:
                throw new Exception($"Unexpected response type: {response.Type}");
        }
    }

    public async Task<bool> SendActuatorRequest(Actuator actuator, double time)
    {

        ActuatorDto actuatorDto = new ActuatorDto(actuator.ActuatorId, actuator.Arguments);
        RequestWrapper requestWrapper = new RequestWrapper(actuatorDto, time);

        WebSocketMessage<RequestWrapper> requestDto =
            new WebSocketMessage<RequestWrapper>(WebSocketMessageTypes.Actuator, requestWrapper);

        string serializedObject = JsonSerializer.Serialize(requestDto);

        var response = await _websocketService.SendRequestResponseAsync(requestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);

        switch (response.Type)
        {
            case (WebSocketResponseTypes.Actuator):
                _logger.Debug($"Actuator response is: {response}");
                return true;

            case (WebSocketResponseTypes.Error):
                throw new Exception($"The actuator {actuator.Name} with id {actuator.ActuatorId} was not found.");

            default:
                throw new Exception($"Unexpected response type: {response.Type}");
        }
    }

    public async Task<double> SendTimeRequest()
    {
        WebSocketMessage<int> requestDto =
            new WebSocketMessage<int>(WebSocketMessageTypes.Time, 200);

        string serializedObject = JsonSerializer.Serialize(requestDto);
        var response = await _websocketService.SendRequestResponseAsync(requestDto.RequestId.ToString(), serializedObject, _cancellationTokenSource.Token);

        switch (response.Type)
        {
            case WebSocketResponseTypes.Time:
                if (!double.TryParse(response.Data.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var time))
                {
                    throw new Exception($"Unable to parse {response.Data} into a double");
                }

                return time;

            case (WebSocketResponseTypes.Error):
                throw new Exception($"An error occured when trying to fetch the simulator time");

            default:
                throw new Exception($"Unexpected response type: {response.Type}");
        }
    }

    public async Task<bool> IsClientConnected()
    {
        var amountClients = _websocketService.GetNumberOfConnectedClients();
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
