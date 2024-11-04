using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Models
{
    public class ScheduledPosition
    {
        public ITemplate Template;
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;
        public int SingularX;
        public int SingularY;
        public int OriginX;
        public int OriginY;
        public ScheduledPosition(ITemplate template, int x1, int y1, int x2, int y2, int singularX, int singularY, int originX, int originY)
        {
            Template = template;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            SingularX = singularX;
            SingularY = singularY;
            OriginX = originX;
            OriginY = originY;
        }

        public override string ToString()
        {
            return $"ScheduledPosition: [X1={X1}, Y1={Y1}, X2={X2}, Y2={Y2}, SingularX={SingularX}, SingularY={SingularY}, OriginX={OriginX}, OriginY={OriginY}]";
        }
    }
}
