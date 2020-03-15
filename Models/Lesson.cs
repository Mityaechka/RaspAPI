using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Models
{
    public class Lesson
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Type { get; set; }
        public string Classroom { get; set; }
        public string TeacherName { get; set; }
        public string TeacherPosition { get; set; }
        public Lesson()
        {
        }
    }
}
