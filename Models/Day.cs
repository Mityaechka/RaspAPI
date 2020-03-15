using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace RaspAPI.Models
{
    public class Day
    {
        
        public string Name { get; set; }
        public string FullName { get; set; }
        public List<TimeRange> TimeRanges { get; set; } = new List<TimeRange>();

        public Day()
        {
        }
    }
}
