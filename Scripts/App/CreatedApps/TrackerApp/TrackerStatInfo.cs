using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;

using static Revistone.App.Tracker.TrackerData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.NumericalFunctions;

namespace Revistone.App.Tracker;

abstract class TrackerStatProfile
{
    public abstract void Display(string timePeriodName, int index, int length);

    public static TrackerStatProfile? GenerateStat(TrackerStat stat)
    {
        if (stat is TrackerInputStat istat)
        {
            if (istat.inputTypes.Contains(UserInputProfile.InputType.Int) || istat.inputTypes.Contains(UserInputProfile.InputType.Float)) return new TrackerNumberInputStatProfile(istat);
            else return new TrackerTextInputStatProfile(istat);
        }
        else
        {
            return new TrackerDropdownStatProfile((TrackerDropdownStat)stat);
        }
    }
}

/// <summary> representation of the info of a numerical input stat for tracker application. </summary>
class TrackerTextInputStatProfile : TrackerStatProfile
{
    readonly string statName;
    readonly List<(int index, string value)> logs = [];

    public TrackerTextInputStatProfile(TrackerInputStat stat)
    {
        statName = stat.statName;
        for (int i = 0; i <= DATA.GetDayIndex(DATA.today); i++) // get all instances of stat
        {
            foreach (TrackerStat s in DATA.GetDayData(DATA.startDate.AddDays(i)).stats)
            {
                if (s.ToIdentificationString() == stat.ToIdentificationString() && s.value != "Not Tracked")
                {
                    logs.Add((i, s.value));
                    break;
                }
            }
        }
    }

    public override void Display(string timePeriodName, int index, int length)
    {
        List<string> l = [];

        foreach ((int index, string value) log in logs)
        {
            if (log.index < index) continue;
            if (log.index >= index + length) continue;

            l.Add(log.value);
        }

        SendConsoleMessage(new ConsoleLine($"--- {timePeriodName} - {statName} ---", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"Last {Math.Min(7, l.Count)} Logs:", ConsoleColor.Cyan));
        for (int i = Math.Max(0, l.Count - 7); i < l.Count; i++)
        {
            SendConsoleMessage(new ConsoleLine($"- {l[i]}", ConsoleColor.Cyan));
        }
    }
}

/// <summary> representation of the info of a numerical input stat for tracker application. </summary>
class TrackerNumberInputStatProfile : TrackerStatProfile
{
    readonly string statName;
    readonly List<(int index, float value)> logs = [];

    public TrackerNumberInputStatProfile(TrackerInputStat stat)
    {
        statName = stat.statName;
        for (int i = 0; i <= DATA.GetDayIndex(DATA.today); i++) // get all instances of stat
        {
            foreach (TrackerStat s in DATA.GetDayData(DATA.startDate.AddDays(i)).stats)
            {
                if (s.ToIdentificationString() == stat.ToIdentificationString() && s.value != "Not Tracked")
                {
                    logs.Add((i, float.Parse(s.value)));
                    break;
                }
            }
        }
    }

    public override void Display(string timePeriodName, int index, int length)
    {
        List<float> l = [];
        List<float> sl = [];
        List<(float value, int count)> gl = [];


        foreach ((int index, float value) log in logs)
        {
            if (log.index < index) continue;
            if (log.index >= index + length) continue;

            l.Add(log.value);
            sl.Add(log.value);
        }
        sl.Sort();
        gl = l.GroupBy(v => v).OrderByDescending(v => v.Count()).Select(v => (v.Key, v.Count())).ToList(); // weird notation but splits into group by values

        SendConsoleMessage(new ConsoleLine($"--- {timePeriodName} - {statName} ---", ConsoleColor.DarkBlue));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"> Distribution (Total {l.Count})", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"Min - {sl[0]}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Lower Quartile - {GetMedian(sl.Count == 1 ? sl : sl.Take(sl.Count / 2).ToList())}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Medium - {GetMedian(sl)}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Upper Quartile - {GetMedian(sl.Count == 1 ? sl : sl.Skip((sl.Count + 1) / 2).ToList())}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Max - {sl[^1]}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Last {Math.Min(7, l.Count)} Logs - {l.TakeLast(Math.Min(7, l.Count)).ToList().ToElementString()}", ConsoleColor.Cyan));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine("> Statistics", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"Mean - {Math.Round(l.Sum() / l.Count, 2)}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Mode - {gl[0].value}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Sum - {Math.Round(l.Sum(), 2)}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Variance - {Math.Round(GetVariance(sl), 2)}", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Standard Deviation - {Math.Round(Math.Sqrt(GetVariance(sl)), 2)}", ConsoleColor.Cyan));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine("> Frequency (Value - Count)", ConsoleColor.DarkBlue));
        int trackedCount = 0;
        for (int i = 0; i < Math.Min(5, gl.Count); i++)
        {
            trackedCount += gl[i].count;
            SendConsoleMessage(new ConsoleLine($"{gl[i].value} - {gl[i].count} ({Math.Round((double)gl[i].count / l.Count * 100)}%)", ConsoleColor.Cyan));
        }
        if (trackedCount < l.Count) SendConsoleMessage(new ConsoleLine($"Other - {l.Count - trackedCount} ({Math.Round((double)(l.Count - trackedCount) / l.Count * 100)}%)", ConsoleColor.Cyan));
    }
}

class TrackerDropdownStatProfile : TrackerStatProfile
{
    readonly TrackerDropdownStat stat;
    readonly List<(int index, int value)> logs = [];

    public TrackerDropdownStatProfile(TrackerDropdownStat stat)
    {
        this.stat = stat;
        for (int i = 0; i <= DATA.GetDayIndex(DATA.today); i++) // get all instances of stat
        {
            foreach (TrackerStat s in DATA.GetDayData(DATA.startDate.AddDays(i)).stats)
            {
                if (s.ToIdentificationString() == stat.ToIdentificationString() && s.value != "Not Tracked")
                {
                    logs.Add((i, stat.options.Select(x => x.option).ToList().IndexOf(s.value)));
                    break;
                }
            }
        }
    }

    public override void Display(string timePeriodName, int index, int length)
    {
        List<int> l = [];
        List<(int value, int count)> gl = [];
        int bestStreak = 0, currentStreak = 0, totalCorrect = 0;


        foreach ((int index, int value) log in logs)
        {
            if (log.index < index) continue;
            if (log.index >= index + length) continue;

            if (stat.options[log.value].success){
                totalCorrect++;
                currentStreak++;
                bestStreak = Math.Max(bestStreak, currentStreak);
            }
            else currentStreak = 0;

            l.Add(log.value);
        }
        gl = l.GroupBy(v => v).Select(v => (v.Key, v.Count())).OrderBy(v => v.Key).ToList(); // weird notation but splits into group by values

        SendConsoleMessage(new ConsoleLine($"--- {timePeriodName} - {stat.statName} ---", ConsoleColor.DarkBlue));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"> Statistics", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"Completion Rate - {totalCorrect} / {l.Count} ({Math.Round((double)totalCorrect/l.Count * 100)}%)", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Current Streak - {currentStreak} Days", ConsoleColor.Cyan));
        SendConsoleMessage(new ConsoleLine($"Best Streak - {bestStreak} Days", ConsoleColor.Cyan));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"> Frequency (Total {l.Count})", ConsoleColor.DarkBlue));
        foreach ((int value, int count) in gl)
        {
            SendConsoleMessage(new ConsoleLine($"{stat.options[value].option} - {count} ({Math.Round((double)count / l.Count * 100)}%)", ConsoleColor.Cyan));
        }
        SendConsoleMessage(new ConsoleLine($"Last {Math.Min(7, l.Count)} Logs - {l.TakeLast(Math.Min(7, l.Count)).Select(x => stat.options[x].option).ToList().ToElementString()}", ConsoleColor.Cyan));
    }
}