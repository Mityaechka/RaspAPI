using HtmlAgilityPack;
using RaspAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public class AuesParser : ScheduleParser
    {
        public AuesParser() : base("https://aues.arhit.kz/rasp/scheduleNew.php?sg=", "https://aues.arhit.kz/rasp/start.php")
        {
        }
        public static Dictionary<string, string> daysName = new Dictionary<string, string>()
        {
            {"Понедельник","Пн." },
            {"Вторник","Вт." },
            {"Среда","Ср." },
            {"Четверг","Чт." },
            {"Пятница","Пт." },
            {"Суббота","Сб." }
        };
        public async override Task<List<GroupInfo>> ParseGroupList()
        {
            var list = new List<GroupInfo>();

            HttpClient client = new HttpClient();
            var raw = await client.GetAsync(GroupListUrl);
            var schedule = new Schedule();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(await raw.Content.ReadAsStringAsync());
            var options = html.DocumentNode.SelectSingleNode("//select[@name='sg']").SelectNodes("//option");
            foreach (var option in options)
            {
                list.Add(new GroupInfo()
                {
                    Index = option.GetAttributeValue("value", ""),
                    Name = option.InnerText
                });

            }
            list.RemoveAt(0);
            return list;
        }

        public async override Task<Schedule> ParseSchedule(string index)
        {
            HttpClient client = new HttpClient();
            var raw = await client.GetAsync(ScheduleUrl + index);
            var schedule = new Schedule();

            schedule.Group = ParseGroupList().Result.Where(x => x.Index == index).FirstOrDefault();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(await raw.Content.ReadAsStringAsync());

            var trs = html.DocumentNode.SelectNodes("//tr");
            trs.RemoveAt(0);

            var day = new Day();
            for (int i = trs.Count - 1; i >= 0; i--)
            {
                var tds = trs[i].SelectNodes("th|td");
                var lesson = new Lesson();
                var timeRange = new TimeRange();

                lesson.Classroom = tds[5].InnerText;

                var rawTeacher = tds[4].InnerHtml.Trim().Split("<br>");

                lesson.TeacherName = rawTeacher[0];
                lesson.TeacherPosition = rawTeacher[1];

                var rawLesson = tds[3].InnerHtml.Trim().Split("<br>");
                lesson.Name = rawLesson[0];
                lesson.Type = rawLesson[1];

                var rawTime = tds[1].InnerText.Trim().Split("]");
                timeRange.Index = Convert.ToInt32(rawTime[0].Replace("[", ""));
                timeRange.StartTime = rawTime[1].Split("-")[0];
                timeRange.EndTime = rawTime[1].Split("-")[1];
                if (tds[2].InnerText == "1"  )
                {
                    timeRange.Subgroup1 = lesson;
                }
                if (tds[2].InnerText == "2" )
                {
                    timeRange.Subgroup2 = lesson;
                }
                if (tds[2].InnerText == " ")
                    timeRange.UnionLesson = lesson;
                day.TimeRanges.Add(timeRange);
                if (tds[0].InnerText != "")
                {
                    day.Name = daysName[tds[0].InnerText];
                    day.FullName = tds[0].InnerText;
                    for (int j = 0; j < day.TimeRanges.Count; j++)
                    {
                        for (int d = j + 1; d < day.TimeRanges.Count; d++)
                        {
                            var lesson1 = day.TimeRanges[j];
                            var lesson2 = day.TimeRanges[d];
                            if (lesson1.Index == lesson2.Index &&
                                lesson1.StartTime == lesson2.StartTime &&
                                lesson1.EndTime == lesson2.EndTime)
                            {
                               
                                if (lesson1.Subgroup1 == null&&
                                    lesson2.Subgroup1!=null)
                                {
                                    
                                    day.TimeRanges[j].Subgroup1 = lesson2.Subgroup1;
                                    //day.TimeRanges[j].UnionLesson = lesson2.Subgroup1;
                                } 
                                if (lesson1.Subgroup2 == null&&
                                    lesson2.Subgroup2!=null)
                                {
                                    day.TimeRanges[j].Subgroup2 = lesson2.Subgroup2;
                                    //day.TimeRanges[j].UnionLesson = lesson2.Subgroup2;
                                }
                                
                               // day.TimeRanges[j].Subgroup1 =
                                  //  day.TimeRanges[j].Subgroup2 = null;
                                j--;

                                day.TimeRanges.RemoveAt(d);
                                break;
                            }
                        }
                    }
                    day.TimeRanges = day.TimeRanges.OrderBy(x => x.StartTime).ToList();
                    schedule.Days.Add(day);
                    day = new Day();

                }

            }

            schedule.Days.Reverse();

            return schedule;
        }
    }
}

