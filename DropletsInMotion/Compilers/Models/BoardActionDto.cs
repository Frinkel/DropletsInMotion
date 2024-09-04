using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Compilers.Models
{
    public class BoardActionDto
    {
        public int ElectrodeId { get; set; }
        public int Action { get; set; }
        public decimal Time { get; set; }

        public BoardActionDto(int electrodeId, int action, decimal time)
        {
            ElectrodeId = electrodeId;
            Action = action;
            Time = time;
        }

        public override string ToString()
        {
            return $"BoardActionDto(ElectrodeId: {ElectrodeId}, Action: {Action}, Time: {Time})";
        }
    }
}
