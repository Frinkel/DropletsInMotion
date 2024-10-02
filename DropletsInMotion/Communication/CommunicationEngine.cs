using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DropletsInMotion.Communication
{
    public class CommunicationEngine : ICommunicationEngine, IDisposable, IAsyncDisposable
    {
        private ICommunicationService? _communicationService;
        private readonly IServiceProvider _serviceProvider;

        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;

        private bool _isServerRunning = false;

        public CommunicationEngine(IServiceProvider serviceProvider, IUserService userService)
        {
            _serviceProvider = serviceProvider;

            userService.CommunicationTypeChanged += OnCommunicationTypeChanged;
        }

        private async void OnCommunicationTypeChanged(object? sender, EventArgs e)
        {   
            try
            {
                if (sender == null) throw new ArgumentNullException(nameof(sender));
                if (_isServerRunning) throw new InvalidOperationException("A server is already running");

                var userService = (IUserService)sender;
                _communicationService = await SetCommunicationType(userService.Communication);
                _communicationService.ClientDisconnected += OnClientDisconnected;
                await WaitForConnection();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in communication type handler: {ex.Message}");
            }
        }

        private async Task<ICommunicationService> SetCommunicationType(IUserService.CommunicationType communicationType)
        {
            if (communicationType == IUserService.CommunicationType.Simulator)
            {
                var service = _serviceProvider.GetRequiredService<SimulationCommunicationService>();
                await service.StartCommunication();
                _isServerRunning = true;
                return service;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private async void OnClientDisconnected(object? sender, EventArgs e)
        {
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
            await WaitForConnection();
        }

        public async Task SendActions(List<BoardAction> boardActionDtoList)
        {
            if (!_isServerRunning || _communicationService == null)
            {
                return;
            }

            await _communicationService.SendActions(boardActionDtoList);
        }

        public async Task SendRequest(BoardSensorRequest sensorRequest)
        {
            if (!_isServerRunning || _communicationService == null)
            {
                return;
            }

            await _communicationService.SendRequest(sensorRequest);
        }


        // Implement IDisposable
        public void Dispose()
        {
            StopCommunication().GetAwaiter().GetResult();
        }

        // Implement IAsyncDisposable for async cleanup
        public async ValueTask DisposeAsync()
        {
            await StopCommunication();
        }

        private async Task StopCommunication()
        {
            if (_isServerRunning && _communicationService != null)
            {
                await _communicationService.StopCommunication();
                _isServerRunning = false;
            }
        }

        private async Task WaitForConnection()
        {
            if (_isServerRunning && _communicationService != null)
            {
                await _communicationService.WaitForConnection();
                ClientConnected?.Invoke(this, EventArgs.Empty);
            }
        }

        //public async Task<bool> IsClientConnected()
        //{
        //    return await _communicationService.IsClientConnected();
        //}

        //public async Task<bool> IsConnectionOpen()
        //{
        //    return await _communicationService.IsConnectionOpen();
        //}
    }
}
