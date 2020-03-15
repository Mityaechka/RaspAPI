using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Parsers
{
    public class StaticData<TimeType>
    {


        List<TimePair<TimeType>> TimePairs = new List<TimePair<TimeType>>();
        List<LessonType> LessonTypes = new List<LessonType>();
        List<DayName> DayNames = new List<DayName>();

        public TimePair<TimeType> DefaultTime = new TimePair<TimeType>(default(TimeType), "", "");
        public LessonType DefaultLessonType = new LessonType(-1, "");
        public DayName DefaultDayName = new DayName(-1, "","");

        public TimePair<TimeType> GetTime(TimeType index)
        {
                var result = TimePairs.Where(x => x.Index.Equals(index)).FirstOrDefault();
                if (result != null)
                    return result;
                else
                    return DefaultTime;
        }
        public LessonType GetLessonType(int index)
        {
                var result = LessonTypes.Where(x => x.Index == index).FirstOrDefault();
                if (result != null)
                    return result;
                else
                    return DefaultLessonType;
        }
        public DayName GetDayName(int index)
        {
            var result = DayNames.Where(x => x.Index == index).FirstOrDefault();
            if (result != null)
                return result;
            else
                return DefaultDayName;
        }
        public void SetTimePairs(params TimePair<TimeType>[] time)
        {
            TimePairs.Clear();
            TimePairs.AddRange(time);

        }
        public void SetLessonTypes(params LessonType[] lessonTypes)
        {
            LessonTypes.Clear();
            LessonTypes.AddRange(lessonTypes);
        }
        public void SetDayNamesTypes(params DayName[] dayNames)
        {
            DayNames.Clear();
            DayNames.AddRange(dayNames);
        }
    }
    public class TimePair<T>
    {
        public T Index { get; private set; }
        public string StartTime { get; private set; }
        public string EndTime { get; private set; }

        public TimePair(T index, string startTime, string endtTime)
        {
            Index = index;
            StartTime = startTime;
            EndTime = endtTime;
        }
    }
    public class LessonType
    {
        public int Index { get; private set; }
        public string Name { get; private set; }

        public LessonType(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
    public class DayName
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }

        public DayName(int index, string name, string fullName)
        {
            Index = index;
            Name = name;
            FullName = fullName;
        }
    }
}
