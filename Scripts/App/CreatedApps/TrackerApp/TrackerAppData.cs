using Revistone.Console;

using static Revistone.Interaction.UserInputProfile;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using Revistone.Functions;

namespace Revistone.Apps.Tracker;

/// <summary> Main class pertaining all information tracked by user. </summary>
class TrackerData
{
    public DateOnly startDate { get; private set; } // first day of tracking
    public DateOnly selectedDate { get; private set; } // currently selected day

    public DateOnly today => DateOnly.FromDateTime(DateTime.Now);

    DayTrackerData[] dayData; // individual data for each day

    public TrackerData()
    {
        startDate = today;
        selectedDate = today;
        dayData = new DayTrackerData[] { new DayTrackerData(today) };
    }

    public TrackerData(string[] data)
    {
        startDate = DateOnly.Parse(data[0]);
        selectedDate = today;
        dayData = new DayTrackerData[today.DayNumber - startDate.DayNumber + 1];

        int lastDayStart = -1;
        for (int i = 1; i < data.Length; i++)
        {
            if (data[i].StartsWith("DAY"))
            {
                if (lastDayStart != -1)
                {
                    dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))] = new DayTrackerData(data.Skip(lastDayStart).Take(i - lastDayStart).ToArray());
                }
                lastDayStart = i;
            }
        }

        dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))] = new DayTrackerData(data.Skip(lastDayStart).Take(data.Length - lastDayStart).ToArray());
       
        DayTrackerData.trackedStats = new List<TrackerStat>();
        foreach (TrackerStat stat in dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))].stats)
        {
            if (stat is TrackerDropdownStat dropdownStat) DayTrackerData.trackedStats.Add(new TrackerDropdownStat(dropdownStat.statName, dropdownStat.options));
            else if (stat is TrackerInputStat inputStat) DayTrackerData.trackedStats.Add(new TrackerInputStat(inputStat.statName, inputStat.inputTypes));
        }

        for (int i = 0; i < dayData.Length; i++) if (dayData[i] == null) dayData[i] = new DayTrackerData(startDate.AddDays(i));
    }

    public void ModifySelectedDate(DateOnly date)
    {
        if (date.DayNumber < startDate.DayNumber || date.DayNumber > startDate.DayNumber + dayData.Length - 1) return;
        selectedDate = date;
        ClearPrimaryConsole();
        dayData[GetDayIndex(selectedDate)].Display(this);
        ShiftLine();
    }

    public void IncrementSelectedDate() { ModifySelectedDate(selectedDate.AddDays(1)); }
    public void DecrementSelectedDate() { ModifySelectedDate(selectedDate.AddDays(-1)); }

    public DayTrackerData GetDayTrackerData(DateOnly date)
    {
        return dayData[GetDayIndex(date)];
    }

    public int GetDayIndex(DateOnly date)
    {
        return date.DayNumber - startDate.DayNumber;
    }

    public void SaveData(string fileName = "Data")
    {
        List<string> data = new List<string>() { startDate.ToString() };
        foreach (DayTrackerData day in dayData)
        {
            data.Add($"DAY {day.date}");
            foreach (TrackerStat stat in day.stats)
            {
                if (stat is TrackerInputStat inputStat) data.Add(string.Join("", inputStat.inputTypes.Select(x => $"{x}-")));
                else if (stat is TrackerDropdownStat dropdownStat) data.Add(string.Join("-", dropdownStat.options.Select(x => $"({x.option}, {x.success})")));
                data.Add(stat.ToString());
            }
        }
        AppPersistentData.SaveFile($"Tracker/{fileName}", data.ToArray());
    }
}

/// <summary> class pertaining all information tracked by user in a day. </summary>
class DayTrackerData
{
    public static List<TrackerStat> trackedStats = [
        new TrackerInputStat("Sleep (hours)", InputType.Float, InputType.Int),
        new TrackerInputStat("Weight", InputType.Float, InputType.Int),
        new TrackerInputStat("Calories (kcals)", InputType.Int),
        new TrackerInputStat("Protein (g)", InputType.Int),
        new TrackerInputStat("Water (ml)", InputType.Int),
        new TrackerInputStat("Work (hours)", InputType.Float, InputType.Int),
        new TrackerDropdownStat("Gym", ("Push", true), ("Pull", true), ("Legs", true), ("Rest", false)),
        new TrackerInputStat("Happiness", InputType.Float, InputType.Int),
        new TrackerInputStat("Motivation", InputType.Float, InputType.Int),
        new TrackerInputStat("Grateful For", InputType.FullText, InputType.PartialText),
        new TrackerInputStat("Summary", InputType.FullText, InputType.PartialText),
    ];

    public DateOnly date { get; private set; }
    public List<TrackerStat> stats;

    public DayTrackerData(DateOnly date)
    {
        this.date = date;
        stats = trackedStats;
    }

    public DayTrackerData(string[] data)
    {
        this.date = DateOnly.Parse(data[0].Substring(4));

        stats = new List<TrackerStat>() { };
        for (int i = 1; i < data.Length; i += 2)
        {
            string[] statInfo = data[i].Split('-');
            string[] stat = data[i + 1].Split(" - "); //string without whitespace

            if (data[i].Contains("(")) //dropdown
            {
                List<(string, bool)> options = new List<(string, bool)>();
                foreach (string s in statInfo) options.Add((s.Substring(1, s.IndexOf(',') - 1), bool.Parse(s.Substring(s.IndexOf(',') + 1, s.Length - s.IndexOf(',') - 2))));
                stats.Add(new TrackerDropdownStat(stat[0], stat[1], options.ToArray()));
            }
            else //input
            {
                InputType[] types = new InputType[statInfo.Length - 1];
                for (int j = 0; j < statInfo.Length - 1; j++)
                {
                    Enum.TryParse(statInfo[j], out InputType t);
                    types[j] = t;
                }
                stats.Add(new TrackerInputStat(stat[0], stat[1], types));
            }
        }
    }

    public void Display(TrackerData data)
    {
        SendConsoleMessage(new ConsoleLine($"--- Day {date.DayNumber - data.startDate.DayNumber + 1} - [{date}] ---", ConsoleColor.DarkBlue));
        foreach (TrackerStat stat in stats)
        {
            SendConsoleMessage(new ConsoleLine(stat.ToString(), BuildArray(ConsoleColor.Cyan.Extend(stat.statName.Length + 2), ConsoleColor.White.ToArray())));
        }
    }
}