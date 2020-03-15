using RaspAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public abstract class ScheduleParser
    {
        protected string ScheduleUrl { get; private set; }
        protected string GroupListUrl { get; private set; }

        protected ScheduleParser(string scheduleUrl, string groupListUrl)
        {
            ScheduleUrl = scheduleUrl;
            GroupListUrl = groupListUrl;
        }

        public abstract Task<Schedule> ParseSchedule(string index);
        public abstract Task<List<GroupInfo>> ParseGroupList();
    }
}
