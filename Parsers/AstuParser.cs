using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RaspAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public class AstuParser : ScheduleParser
    {
        readonly StaticData<int> staticData = new StaticData<int>();
        public AstuParser() : base("http://www.astu.org/Content/Page/5791", "http://api.table.astu.org/search/get?q=")
        {
            staticData.SetLessonTypes(new LessonType(0, "Лекция"),
                new LessonType(1, "Практика"),
                new LessonType(2, "Лабораторная"));
            staticData.SetTimePairs(new TimePair<int>(0, "08:30", "10:00"),
                new TimePair<int>(1, "10:15", "11:45"),
                new TimePair<int>(2, "12:00", "13:30"),
                new TimePair<int>(3, "14:00", "15:30"),
                new TimePair<int>(4, "15:45", "17:15"),
                new TimePair<int>(5, "17:30", "19:00"),
                new TimePair<int>(6, "19:15", "20:45"));
            staticData.SetDayNamesTypes(new DayName(0, "Пн. 1 н.","Понедельник 1-я н."),
                new DayName(1, "Вт. 1 н.", "Вторник 1-я н."),
                new DayName(2, "Ср. 1 н.", "Среда 1-я н."),
                new DayName(3, "Чт. 1 н.", "Четверг 1-я н."),
                new DayName(4, "Пт. 1 н.", "Пятница 1-я н."),
                new DayName(5, "Сб. 1 н.", "Суббота 1-я н."),
                new DayName(6, "Пн. 2 н.", "Понедельник 2-я н."),
                new DayName(7, "Вт. 2 н.", "Вторник 2-я н."),
                new DayName(8, "Ср. 2 н.", "Среда 2-я н."),
                new DayName(9, "Чт. 2 н.", "Четверг 2-я н."),
                new DayName(10, "Пт. 2 н.", "Пятница 2-я н."),
                new DayName(11, "Сб. 2 н.", "Суббота 2-я н."));
        }

        public override async Task<List<GroupInfo>> ParseGroupList()
        {
            
            var list = new List<GroupInfo>();

            HttpClient client = new HttpClient();
            var raw = await client.GetAsync(ScheduleUrl);
            var schedule = new Schedule();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(await raw.Content.ReadAsStringAsync());

            var options = html.DocumentNode.SelectNodes("//td//p//a");

            foreach (var option in options)
            {
                var name = option.InnerText;
                if (name == "_")
                    continue;
                var temp = name.Split("/");
                if (temp.Count() != 2)
                    continue;
               var index =  temp[0]+ "$" + temp[1];
                list.Add(new GroupInfo()
                {
                    Index = index,
                    Name = name
                });

            }
            list.RemoveAt(0);
            return list;
        }

        public override async Task<Schedule> ParseSchedule(string index)
        {
            var client = new HttpClient();
            var uri = GroupListUrl+index.Replace("$","/")+ "&t=group";
            var raw = await client.GetAsync(uri);
            
            
            var schedule = new Schedule();

            schedule.Group = ParseGroupList().Result.Where(x => x.Index == index).FirstOrDefault();

            string text = await raw.Content.ReadAsStringAsync();

            dynamic data = JObject.Parse(text);

            List<TimeRangeDayIndexPair> pairs = new List<TimeRangeDayIndexPair>();
            foreach (var lesson in data.lessons)
            {
                TimeRange timeRange = new TimeRange();

                timeRange.StartTime = staticData.GetTime(Convert.ToInt32(lesson.time)).StartTime;
                timeRange.EndTime = staticData.GetTime(Convert.ToInt32(lesson.time)).EndTime;
                timeRange.Index = Convert.ToInt32(lesson.time) + 1;

                Lesson unionLesson = new Lesson();

                unionLesson.Classroom = lesson.audience.name.ToString();
                unionLesson.Name = lesson.discipline.name.ToString();
                unionLesson.TeacherName = lesson.teacher.name.ToString();
                unionLesson.Type = staticData.GetLessonType(Convert.ToInt32(lesson.type)).Name;

                timeRange.UnionLesson = unionLesson;                

                pairs.Add(new TimeRangeDayIndexPair(timeRange, Convert.ToInt32(lesson.day)));
            }
            pairs = pairs.OrderBy(x => x.DayIndex).ToList();
            foreach(var pair in pairs)
            {
                
                var dayName = staticData.GetDayName(pair.DayIndex).Name;
                var fullDayName = staticData.GetDayName(pair.DayIndex).FullName;
                var day = schedule.Days.Where(x => x.Name == dayName).FirstOrDefault();
                if (day==null)
                {
                    schedule.Days.Add(new Day()
                    {
                        Name = dayName,
                        FullName = fullDayName,
                        TimeRanges = new List<TimeRange>() { pair.TimeRange }
                    });
                }
                else
                {
                    day.TimeRanges.Add(pair.TimeRange);
                }
            }
            for(int i = 0; i < schedule.Days.Count; i++)
            {
                schedule.Days[i].TimeRanges = schedule.Days[i].TimeRanges.OrderBy(x => x.Index).ToList();
            }
            return schedule;
        }
        class TimeRangeDayIndexPair
        {
            public TimeRange TimeRange;
            public int DayIndex;

            public TimeRangeDayIndexPair(TimeRange timeRange, int dayIndex)
            {
                TimeRange = timeRange;
                DayIndex = dayIndex;
            }
        }

    }

}
