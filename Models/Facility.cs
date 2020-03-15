using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Details { get; set; }
        public string MainUrl { get; set; }
        public string ScheduleUrl { get; set; }
        public bool TestOnly { get; set; } = true;
        public Facility()
        {
        }
    }
}
