using Revistone.Functions;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.App.BaseApps.Tracker;

/// <summary> Main class pertaining all information tracked by user. </summary>
class TrackerData
{
    public static TrackerData DATA = new TrackerData();

    public DateOnly startDate { get; private set; } // first day of tracking
    public DateOnly selectedDate { get; private set; } // currently selected day
    public DateOnly today => DateOnly.FromDateTime(DateTime.Now); // today

    TrackerDayData[] dayData; // individual data for each day

    public TrackerData()
    {
        startDate = today;
        selectedDate = today;
        dayData = [new TrackerDayData(today)];
    }

    public TrackerData(string[] data)
    {
        startDate = DateOnly.Parse(data[0]);
        selectedDate = today;
        dayData = new TrackerDayData[today.DayNumber - startDate.DayNumber + 1];

        int lastDayStart = -1;
        for (int i = 1; i < data.Length; i++)
        {
            if (data[i].StartsWith("DAY"))
            {
                if (lastDayStart != -1)
                {
                    dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))] = new TrackerDayData(data.Skip(lastDayStart).Take(i - lastDayStart).ToArray());
                }
                lastDayStart = i;
            }
        }

        dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))] = new TrackerDayData(data.Skip(lastDayStart).Take(data.Length - lastDayStart).ToArray());
       
        TrackerDayData.trackedStats = new List<TrackerStat>();
        foreach (TrackerStat stat in dayData[GetDayIndex(DateOnly.Parse(data[lastDayStart].Substring(4)))].stats)
        {
            if (stat is TrackerDropdownStat dropdownStat) TrackerDayData.trackedStats.Add(new TrackerDropdownStat(dropdownStat.statName, dropdownStat.options));
            else if (stat is TrackerInputStat inputStat) TrackerDayData.trackedStats.Add(new TrackerInputStat(inputStat.statName, inputStat.inputTypes));
        }

        for (int i = 0; i < dayData.Length; i++) if (dayData[i] == null) dayData[i] = new TrackerDayData(startDate.AddDays(i));
    }

    public void ModifySelectedDate(DateOnly date)
    {
        if (date.DayNumber < startDate.DayNumber || date.DayNumber > startDate.DayNumber + dayData.Length - 1) return;
        selectedDate = date;
        ClearPrimaryConsole();
        dayData[GetDayIndex(selectedDate)].Display();
    }
    
    public TrackerDayData GetDayData(DateOnly date)
    {
        return dayData[GetDayIndex(date)];
    }

    public int GetDayIndex(DateOnly date)
    {
        return date.DayNumber - startDate.DayNumber;
    }

    public void Save(string fileName = "Data")
    {
        List<string> data = new List<string>() { startDate.ToString() };
        foreach (TrackerDayData day in dayData)
        {
            data.Add($"DAY {day.date}");
            foreach (TrackerStat stat in day.stats)
            {
                if (stat is TrackerInputStat inputStat) data.Add(string.Join("", inputStat.inputTypes.Select(x => $"{x}-")));
                else if (stat is TrackerDropdownStat dropdownStat) data.Add(string.Join("-", dropdownStat.options.Select(x => $"({x.option}, {x.success})")));
                data.Add(stat.ToString());
            }
        }
        SaveFile(GeneratePath(DataLocation.App, "Tracker", fileName), data.ToArray());
    }
}