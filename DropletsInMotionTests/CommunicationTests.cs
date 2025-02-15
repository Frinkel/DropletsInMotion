﻿using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class CommunicationTests : TestBase
    {

        private readonly ITemplateService _templateService;
        private readonly IContaminationService _contaminationService;
        private readonly IUserService _userService;
        //private readonly ICommunicationTemplateService _communicationTemplateService;
        

        public CommunicationTests()
        {
            //_templateService = ServiceProvider.GetRequiredService<ITemplateService>();
            //_contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _userService.ConfigurationPath = "/Assets/Configurations/Configuration";
            //_communicationTemplateService = ServiceProvider.GetRequiredService<ICommunicationTemplateService>();
        }

        [SetUp]
        public void Setup()
        {

        }

        //[Test]
        //public void LoadSensorFiles()
        //{
            
        //    _communicationTemplateService.LoadTemplates();

        //}
    }
}