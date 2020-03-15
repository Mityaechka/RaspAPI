using HtmlAgilityPack;
using Newtonsoft.Json;
using RaspAPI.Models;
using RaspAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public class IneuParser : ScheduleParser
    {

        readonly StaticData<string> StaticData = new StaticData<string>();
        Dictionary<string, string> dayNames = new Dictionary<string, string>()
        {
            {"Понедельник","Пн." },
            {"Вторник","Вт." },
            {"Среда","Ср." },
            {"Четверг","Чт." },
            {"Пятница","Пт." },
            {"Суббота","Сб." },
            {"Воскресенье","Вс." }
        };
        public IneuParser() : base("http://rasp.ineu.kz/index.php", "http://rasp.ineu.kz/index.php")
        {

        }

        public async override Task<List<GroupInfo>> ParseGroupList()
        {


            var list = new List<GroupInfo>();

            HttpClient client = new HttpClient();
            var raw = await client.GetAsync(GroupListUrl);
            HtmlDocument html = new HtmlDocument();
            var s = System.Text.Encoding.UTF8.GetString((await raw.Content.ReadAsByteArrayAsync()));
            html.LoadHtml(s);
            var options = html.DocumentNode
                .SelectSingleNode("//form[@name=\"form\"]//select[@name=\"fakul\"]")
                .SelectNodes("option")
                .Skip(1);
            List<string> fakults = new List<string>();
            List<GroupInfo> groups = new List<GroupInfo>();
            foreach (var o in options)
            {
                var facul = o.Attributes["value"].Value;

                var values = new Dictionary<string, string>
                    {
                    { "fakul" , facul },
                    { "grup", "" }
                    };

                var content = new FormUrlEncodedContent(values);


                var gropRaw = await client.PostAsync(GroupListUrl, content);
                var groupStr = System.Text.Encoding.UTF8.GetString((await gropRaw.Content.ReadAsByteArrayAsync()));
                var groupHtml = new HtmlDocument();
                groupHtml.LoadHtml(groupStr);

                var groupsOptions = groupHtml.DocumentNode
                .SelectSingleNode("//form[@name=\"form\"]").SelectNodes("//select[@name=\"grup\"]").FirstOrDefault()
                .SelectNodes("option")
                .Skip(1);
                foreach (var g in groupsOptions)
                {
                    list.Add(new GroupInfo() { Index = facul + "$$" + g.Attributes["value"].Value, Name = g.InnerText });
                }

            }
            return list;
        }

        public async override Task<Schedule> ParseSchedule(string index)
        {
            HttpClient client = new HttpClient();
            var array = index.Split("$$");
            var schedule = new Schedule();
            schedule.Group = ParseGroupList().Result.Where(x => x.Index == index).FirstOrDefault();
            var values = new Dictionary<string, string>
                    {
                    { "fakul" , array[0] },
                    { "grup", array[1] }
                    };

            var content = new FormUrlEncodedContent(values);

            var raw = await client.PostAsync(GroupListUrl, content);
            var str = System.Text.Encoding.UTF8.GetString((await raw.Content.ReadAsByteArrayAsync()));
            var html = new HtmlDocument();
            html.LoadHtml(str);

            var weeksOptions = html.DocumentNode
                        .SelectSingleNode("//form[@name=\"form\"]//select[@name=\"nedelya\"]");

            if (weeksOptions != null)
            {
                var weeks = weeksOptions.
                    SelectNodes("option").
                    Skip(1).
                    Select(x => Convert.ToInt32(x.Attributes["value"].Value));
                foreach (var i in weeks)
                {
                    values = new Dictionary<string, string>
                    {
                        { "fakul" , array[0] },
                        { "grup", array[1] },
                        {"nedelya", i.ToString() }
                    };

                    content = new FormUrlEncodedContent(values);
                    var rawWeek = await client.PostAsync(GroupListUrl, content);
                    var strWeek = System.Text.Encoding.UTF8.GetString((await rawWeek.Content.ReadAsByteArrayAsync()));
                    var htmlWeek = new HtmlDocument();
                    htmlWeek.LoadHtml(strWeek);
                    schedule.Days.AddRange(Parse(htmlWeek," "+i+" н."));
                }
            }
            else
            {
                var table = html.DocumentNode.SelectSingleNode("//table[@class=\"table\"]");
                schedule.Days.AddRange(Parse(html));
            }

            return schedule;
        }
        List<Day> Parse(HtmlDocument doc, string week = "")
        {
            var table = doc.DocumentNode.SelectSingleNode("//table[@class=\"table\"]");

            var list = new List<Day>();

            var trs = table.SelectNodes("tr").Skip(1).ToList();


            List<int> rowSpans = new List<int>();



            foreach (var t in trs.Select(x => x.SelectSingleNode("td").Attributes["rowspan"]).Where(x => x != null))
            {
                rowSpans.Add(Convert.ToInt32(t.Value));
            }
            foreach (var count in rowSpans)
            {
                var d = new Day();

                var rows = trs.Take(count).ToList();
                d.Name = dayNames[rows[0].SelectSingleNode("td").InnerText] + week;
                d.FullName = rows[0].SelectSingleNode("td").InnerText + week;
                foreach (var row in rows.Where(x => x.Attributes["class"].Value.Contains("today")))
                {
                    var tds = row.SelectNodes("td");
                    if (tds[2].InnerText == "")
                        continue;
                    var nameType = tds[2].InnerText.Split('(', ')');
                    Lesson lesson = null;
                    if (nameType.Count() == 3)
                    {
                        lesson = new Lesson()
                        {
                            Name = nameType[0],
                            Classroom = tds[1].InnerText,
                            Type = nameType[1],
                            TeacherName = tds[3].InnerText
                        };
                    }
                    else
                    {
                        lesson = new Lesson()
                        {
                            Name = tds[2].InnerText,
                            Classroom = tds[1].InnerText,
                            TeacherName = tds[3].InnerText
                        };
                    }
                    var a = tds[0].InnerText.Split('(', ')');
                    string time = a[1];
                    var array = time.Trim().Replace(" ", "").Split("-");
                    string startTime = array[0].Remove(array[0].Count() - 3);
                    string endTime = array[1].Remove(array[1].Count() - 3);
                    int index = Convert.ToInt32(a[0]);

                    var timeRange = new TimeRange()
                    {
                        Index = index,
                        StartTime = startTime,
                        EndTime = endTime,
                        UnionLesson = lesson
                    };
                    d.TimeRanges.Add(timeRange);
                }
                trs = trs.Skip(count).ToList();
                if (d.TimeRanges.Count() != 0)
                    list.Add(d);
            }

            /*
              if (tds[0].Attributes["rowspan"] != null)
        {
            selectRows = Convert.ToInt32(tds[0].Attributes["rowspan"].Value);
        }
             */
            return list;
        }
    }

    class GroupData
    {
        public string GroupName { get; set; }
        public string GroupPage { get; set; }
        public bool HasWeeks => Weeks.Count != 0;
        public List<int> Weeks { get; set; } = new List<int>();
    }
}




/*
 *                 var groupsOptions = groupHtml.DocumentNode
                .SelectSingleNode("//form[@name=\"form\"]").SelectNodes("//select[@name=\"grup\"]").FirstOrDefault()
                .SelectNodes("option")
                .Skip(1);
 * foreach (var g in groupsOptions)
                {
                    var group = g.Attributes["value"].Value;

                    var groupvalues = new Dictionary<string, string>
                    {
                        { "fakul" , facul },
                        { "grup", group }
                    };
                }
 var groupcontent = new FormUrlEncodedContent(values);

                    groupInfo.GroupName = group;
                    raw = await client.PostAsync(GroupListUrl, groupcontent);
                    s = System.Text.Encoding.UTF8.GetString((await raw.Content.ReadAsByteArrayAsync()));

                    html.LoadHtml(s);
                    var weeksOptions = html.DocumentNode
                        .SelectSingleNode("//form[@name=\"form\"]//select[@name=\"nedelya\"]");

                    if (weeksOptions != null)
                    {
                        groupInfo.Weeks.AddRange(weeksOptions.
                            SelectNodes("//option").
                            Skip(1).
                            Select(x => Convert.ToInt32(x.Attributes["value"].Value)).
                            ToArray());
                    }
                }
 */
