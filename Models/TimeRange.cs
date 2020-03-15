using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Models
{
    public class TimeRange
    {
        public int Index { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Lesson Subgroup1 { get; set; }
        public Lesson Subgroup2 { get; set; }
        public Lesson UnionLesson { get; set; }
        public TimeRange()
        {
        }
    }
}
