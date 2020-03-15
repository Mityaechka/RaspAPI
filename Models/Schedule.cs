using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Models
{
    public class Schedule
    {

        public GroupInfo Group { get; set; }
        public List<Day> Days { get; set; } = new List<Day>();
        public Facility Facility { get; set; }
        public Schedule()
        {
        }
    }
}
