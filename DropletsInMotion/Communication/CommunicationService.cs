using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotion.Communication
{
    public class CommunicationService : ICommunicationService
    {
        private ICommunicationService _communicationService;

        private bool _isServerRunning = false;

        public CommunicationService(IServiceProvider serviceProvider, IUserService userService)
        {
            _communicationService = userService.Communication == IUserService.CommunicationType.Simulator
                ? serviceProvider.GetRequiredService<SimulationCommunicationService>()
                : throw new NotImplementedException(); // TODO: Implement physical communication
        }

        public async Task StartCommunication()
        {
            _isServerRunning = true;
            await _communicationService.StartCommunication();
        }

        public async Task StopCommunication()
        {
            _isServerRunning = false;
            await _communicationService.StopCommunication();
        }

        public async Task SendActions(List<BoardAction> boardActionDtoList)
        {
            if (!_isServerRunning)
            {
                return;
            }

            await _communicationService.SendActions(boardActionDtoList);
        }

        public async Task SendRequest(BoardSensorRequest sensorRequest)
        {
            await _communicationService.SendRequest(sensorRequest);
        }

        public async Task WaitForConnection()
        {
            await _communicationService.WaitForConnection();
        }

        public async Task<bool> IsClientConnected()
        {
            return await _communicationService.IsClientConnected();
        }

        public async Task<bool> IsConnectionOpen()
        {
            return await _communicationService.IsConnectionOpen();
        }
    }
}
