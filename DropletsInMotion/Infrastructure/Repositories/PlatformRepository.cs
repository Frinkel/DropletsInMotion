﻿using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Infrastructure.Repositories
{
    public class PlatformRepository : IPlatformRepository
    {
        public Electrode[][] Board { get; set; }
        public double TimeScaleFactor { get; set; }
        public double MinimumMovementVolume { get; set; }
        public double MaximumMovementVolume { get; set; }
        public double MinSize1x1 { get; set; }
        public double MinSize2x2 { get; set; }
        public double MinSize3x3 { get; set; }

        public PlatformRepository()
        {
            Board = new Electrode[1][];
            MinimumMovementVolume = 0;
            MaximumMovementVolume = 0;
        }
    }
}
