using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Communication.Physical;
using DropletsInMotion.Infrastructure.Repositories;

namespace DropletsInMotion.Communication
{
    public class CommunicationEngine : ICommunicationEngine, IDisposable, IAsyncDisposable
    {
        private ICommunicationService? _communicationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPlatformRepository _platformRepository;

        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;

        private bool _isServerRunning = false;

        public CommunicationEngine(IServiceProvider serviceProvider, IUserService userService, IPlatformRepository platformRepository)
        {
            _platformRepository = platformRepository;
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
                var service = _serviceProvider.GetRequiredService<PhysicalCommunicationService>();
                await service.StartCommunication();
                _isServerRunning = true;
                return service;
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

            // Apply the scaling factor
            boardActionDtoList.ForEach(b => b.Time *= _platformRepository.TimeScaleFactor);

            await _communicationService.SendActions(boardActionDtoList);
        }

        public async Task<double> SendSensorRequest(Sensor sensor, SensorHandler handler, double time)
        {
            if (!_isServerRunning || _communicationService == null)
            {
                throw new InvalidOperationException("Tried to send request without an open communication channel!");
            }

            return await _communicationService.SendSensorRequest(sensor, handler, time * _platformRepository.TimeScaleFactor);
        }

        public async Task<bool> SendActuatorRequest(Actuator actuator, double time)
        {
            if (!_isServerRunning || _communicationService == null)
            {
                throw new InvalidOperationException("Tried to send request without an open communication channel!"); // TODO: Communication error here!
            }

            return await _communicationService.SendActuatorRequest(actuator, time * _platformRepository.TimeScaleFactor);
        }

        public async Task<double> SendTimeRequest()
        {
            if (!_isServerRunning || _communicationService == null)
            {
                return 0;
            }

            return await _communicationService.SendTimeRequest();
        }


        public void Dispose()
        {
            StopCommunication().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await StopCommunication();
        }

        public async Task StopCommunication()
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
    }
}
