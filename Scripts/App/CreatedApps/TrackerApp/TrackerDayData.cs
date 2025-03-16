using Revistone.Console;
using Revistone.Interaction;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using static Revistone.Interaction.UserInputProfile;
using static Revistone.App.Tracker.TrackerData;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.App.Tracker;

/// <summary> class pertaining all information tracked by user in a day. </summary>
class TrackerDayData
{
    public static List<TrackerStat> trackedStats = [ //default tracked stats
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

    public TrackerDayData(DateOnly date)
    {
        this.date = date;
        stats = trackedStats;
    }

    public TrackerDayData(string[] data)
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

    public void Display()
    {
        (string name, DateOnly startDate, DateOnly endDate)[] arc = LoadFile(GeneratePath(DataLocation.App, "Tracker", $"ArcData"), 3, true).Select(x => (x[0], DateOnly.Parse(x[1]), DateOnly.Parse(x[2]))).ToArray();
        string arcString = string.Join(' ', arc.Where(x => x.startDate <= date && x.endDate >= date).Select(x => $"[{x.name}]"));

        string dayString = $"--- Day {DATA.GetDayIndex(date) + 1} - [{date}] ---";
        SendConsoleMessage(new ConsoleLine($"{dayString} {arcString}", BuildArray(ConsoleColor.DarkBlue.Extend(dayString.Length), ConsoleColor.Red.Extend(arcString.Length))));
        foreach (TrackerStat stat in stats)
        {
            SendConsoleMessage(new ConsoleLine(stat.ToString(), BuildArray(ConsoleColor.Cyan.Extend(stat.statName.Length + 2), ConsoleColor.White.ToArray())));
        }
        ShiftLine();
    }

    public void DisplayAsMenu(ref int pointer)
    {
        ConsoleLine[] dayStatsSelect = stats.Select(
            x => new ConsoleLine($"{x} ", BuildArray(ConsoleColor.Cyan.Extend(x.statName.Length + 2), ConsoleColor.White.ToArray(1000)))).ToArray();
        dayStatsSelect = dayStatsSelect.Concat(new ConsoleLine[] { new ConsoleLine("Exit", ConsoleColor.DarkBlue) }).ToArray();

        (string name, DateOnly startDate, DateOnly endDate)[] arc = LoadFile(GeneratePath(DataLocation.App, "Tracker", $"ArcData"), 3, true).Select(x => (x[0], DateOnly.Parse(x[1]), DateOnly.Parse(x[2]))).ToArray();
        string arcString = string.Join(' ', arc.Where(x => x.startDate <= date && x.endDate >= date).Select(x => $"[{x.name}]"));
        string dayString = $"--- Day {DATA.GetDayIndex(date) + 1} - [{date}] ---";

        pointer = UserInput.CreateOptionMenu(new ConsoleLine($"{dayString} {arcString}", BuildArray(ConsoleColor.DarkBlue.Extend(dayString.Length), ConsoleColor.Red.Extend(arcString.Length))), dayStatsSelect, cursorStartIndex: pointer);
    }
}