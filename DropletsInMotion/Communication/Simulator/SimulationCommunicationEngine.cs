﻿using DropletsInMotion.Communication.Simulator.Models;
using DropletsInMotion.Communication.Simulator.Services;
using DropletsInMotion.Compilers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DropletsInMotion.Communication.Simulator
{
    internal class SimulationCommunicationEngine : ICommunication
    {
        private WebsocketService? _websocketService;
        private CancellationTokenSource? _cancellationTokenSource;
        public Task? WebSocketTask { get; private set; }


        public async Task StartCommunication()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _websocketService = new WebsocketService("http://localhost:5000/ws/");

            WebSocketTask = Task.Run(async () =>
            {
                await _websocketService.StartServerAsync(_cancellationTokenSource.Token);
            });

            await Task.Delay(100);
        }

        public async Task WaitForConnection()
        {
            if (_websocketService == null)
            {
                throw new Exception("Error: Websocket is not defined");
            }

            await _websocketService.WaitForClientConnectionAsync();
        }

        public async Task SendActions(List<BoardActionDto> boardActionDtoList)
        {
            if (_websocketService == null)
            {
                throw new Exception("Error: An action cannot be sent without a Websocket communication!");
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

        public void SendRequest<T>(T request)
        {
            throw new NotImplementedException();
        }
    }
}
