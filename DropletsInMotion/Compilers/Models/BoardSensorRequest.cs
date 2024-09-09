using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Compilers.Models
{
    public class BoardSensorRequest
    {
        public BoardSensorRequest(int id, decimal time)
        {
            Id = id;
            Time = time;
        }
        public int Id { get; set; }
        public decimal Time { get; set; }
        public override string ToString()
        {
            return $"BoardSensorRequest:(Id: {Id}, Time: {Time})";
        }
    }
}
