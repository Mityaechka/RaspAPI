using HtmlAgilityPack;
using Newtonsoft.Json;
using RaspAPI.Models;
using RaspAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public class KaznauParser : ScheduleParser
    {
        string facultyUrl = "https://www.kaznau.kz/page/schedule/ajax.php?lang=ru&faculty=";
        string cafedraUrl = "https://www.kaznau.kz/page/schedule/ajax.php?cafedra=";
        string professionsUrl = "https://www.kaznau.kz/page/schedule/ajax.php?professions=";
        string groupUrl = "https://www.kaznau.kz/page/schedule/ajax.php?spec=";

        readonly StaticData<string> StaticData = new StaticData<string>();
        public KaznauParser() : base("https://www.kaznau.kz/page/schedule/ajax.php?group=", "https://www.kaznau.kz/page/schedule/")
        {
            StaticData.SetDayNamesTypes(new DayName(1, "Пн.", "Понедельник"),
                new DayName(2, "Вт.", "Вторник"),
                new DayName(3, "Ср.", "Среда"),
                new DayName(4, "Чт.", "Четверг"),
                new DayName(5, "Пт.", "Пятница"),
                new DayName(6, "Сб.", "Суббота"));
        }

        public async override Task<List<GroupInfo>> ParseGroupList()
        {
            var list = new List<GroupInfo>();
            var cache = CacheData.GetCache("kaznau") ;
            if (cache.Item1)
            {
                list = JsonConvert.DeserializeObject<List<GroupInfo>>(cache.Item2);
            }
            else
            {
                HttpClient client = new HttpClient();
                var raw = await client.GetAsync(GroupListUrl);

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(await raw.Content.ReadAsStringAsync());
                var faculty = html.DocumentNode.SelectNodes("//select[@onchange='Ajaxfaculty(this);']//option");
                faculty.RemoveAt(0);
                foreach (var f in faculty)
                {
                    var cafRaw = await client.GetAsync(facultyUrl + f.GetAttributeValue("value", ""));
                    HtmlDocument cafHtml = new HtmlDocument();
                    cafHtml.LoadHtml(await cafRaw.Content.ReadAsStringAsync());

                    var caf = cafHtml.DocumentNode.SelectNodes("//option");
                    caf.RemoveAt(0);
                    foreach (var c in caf)
                    {
                        var profRaw = await client.GetAsync(cafedraUrl + c.GetAttributeValue("value", ""));
                        HtmlDocument profHtml = new HtmlDocument();
                        profHtml.LoadHtml(await profRaw.Content.ReadAsStringAsync());

                        var prof = profHtml.DocumentNode.SelectNodes("//option");
                        prof.RemoveAt(0);
                        foreach (var p in prof)
                        {
                            var profNamesRaw = await client.GetAsync(professionsUrl + p.GetAttributeValue("value", ""));
                            HtmlDocument profNamesHtml = new HtmlDocument();
                            profNamesHtml.LoadHtml(await profNamesRaw.Content.ReadAsStringAsync());

                            var profNames = profNamesHtml.DocumentNode.SelectNodes("//option");
                            profNames.RemoveAt(0);
                            foreach (var n in profNames)
                            {
                                var groupRaw = await client.GetAsync(groupUrl + n.GetAttributeValue("value", ""));
                                HtmlDocument groupHtml = new HtmlDocument();
                                groupHtml.LoadHtml(await groupRaw.Content.ReadAsStringAsync());

                                var groups = groupHtml.DocumentNode.SelectNodes("//option");
                                groups.RemoveAt(0);
                                foreach (var g in groups)
                                    list.Add(new GroupInfo()
                                    {
                                        Index = g.GetAttributeValue("value", ""),
                                        Name = g.InnerText
                                    });
                            }


                        }
                    }

                }
                CacheData.SaveCache("kaznau", JsonConvert.SerializeObject(list), 300000);
            }
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

            var tables = html.DocumentNode.SelectNodes("//table");

            foreach(var table in tables)
            {
                Day day = new Day();

                var dayIndex = Convert.ToInt32(table.GetAttributeValue("id", "").Replace("table", ""));

                day.Name = StaticData.GetDayName(dayIndex).Name;
                day.FullName = StaticData.GetDayName(dayIndex).FullName;

                var lessonsHtml = table.SelectNodes("tr");
                lessonsHtml.RemoveAt(0);
                foreach(var tr in lessonsHtml)
                {
                    var tds = tr.SelectNodes("td");
                    TimeRange timeRange = new TimeRange();

                    var str1 = tds[1].InnerText.Split("---")[0].Trim();
                    timeRange.StartTime =str1.Substring(0,str1.Length-3) ;

                    var str2 = tds[1].InnerText.Split("---")[1].Trim();
                    timeRange.EndTime = str2.Substring(0, str2.Length - 3);

                    timeRange.Index = -1;
                        Lesson lesson = new Lesson();
                    lesson.Classroom = tds[2].InnerText + " " + tds[3].InnerText;
                    lesson.Name = tds[4].InnerText;
                    lesson.TeacherName = tds[6].InnerText;
                    lesson.Type = tds[5].InnerText;
                    timeRange.UnionLesson = lesson;
                    day.TimeRanges.Add(timeRange);
                }
                schedule.Days.Add(day);
            }

            return schedule;
        }
    }
}
