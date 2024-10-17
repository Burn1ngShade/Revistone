using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;
using Revistone.Apps.Tracker;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.NumericalFunctions;
using static Revistone.Functions.StringFunctions;

namespace Revistone.Apps;

public class TrackerApp : App
{
    // --- Variables ---

    TrackerData data = new TrackerData();

    // --- APP BOILER ---

    public TrackerApp() : base() { }
    public TrackerApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return new TrackerApp[] {
            new TrackerApp("Tracker", (ConsoleColor.DarkBlue, ConsoleColor.Cyan.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 70, 50)
        };
    }

    public override void OnAppInitalisation()
    {
        if (AppPersistentData.FileExists("Tracker/Data"))
        {
            data = new TrackerData(AppPersistentData.LoadFile("Tracker/Data"));
        }

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        base.OnAppInitalisation();
        MainMenu();
    }

    public override void ExitApp()
    {
        data.SaveData();
        base.ExitApp();
    }

    void MainMenu()
    {
        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("FOCUS", AdvancedHighlight(97, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), 99).ToArray());

        data.GetDayTrackerData(data.today).Display();
        ShiftLine();

        int cursorPointer = 0;

        while (true)
        {
            cursorPointer = UserInput.CreateOptionMenu("--- Options ---", new (ConsoleLine name, Action action)[] {
                (new ConsoleLine("Edit", ConsoleColor.Cyan), EditMenu),
                (new ConsoleLine("Stats", ConsoleColor.Cyan), StatsMenu),
                (new ConsoleLine("Settings", ConsoleColor.Cyan), SettingsMenu),
                (new ConsoleLine("Next Day", ConsoleColor.DarkBlue), data.IncrementSelectedDate),
                (new ConsoleLine("Last Day", ConsoleColor.DarkBlue), data.DecrementSelectedDate),
                (new ConsoleLine("Exit", ConsoleColor.DarkBlue), ExitApp)}, cursorStartIndex: cursorPointer);

            if (cursorPointer == 5) return;
        }

    }

    void EditMenu()
    {
        ClearPrimaryConsole();

        int cursorPointer = 0;
        while (true)
        {
            DayTrackerData day = data.GetDayTrackerData(data.selectedDate);
            ConsoleLine[] dayStatsSelect = day.stats.Select(
                x => new ConsoleLine($"{x.statName} - {x.value} ", BuildArray(ConsoleColor.Cyan.Extend(x.statName.Length + 2), ConsoleColor.White.ToArray(1000)))).ToArray();
            dayStatsSelect = dayStatsSelect.Concat(new ConsoleLine[] { new ConsoleLine("Exit", ConsoleColor.DarkBlue) }).ToArray();

            cursorPointer = UserInput.CreateOptionMenu($"--- Day [{data.selectedDate}] ---", dayStatsSelect, cursorStartIndex: cursorPointer);
            if (cursorPointer == day.stats.Count)
            {
                day.Display();
                ShiftLine();
                return;
            }

            if (day.stats[cursorPointer] is TrackerInputStat inputStat)
            {
                inputStat.value = UserInput.GetValidUserInput($"--- Enter Data For [{inputStat.statName}] ---", new UserInputProfile(inputStat.inputTypes, bannedChars: "-"), inputStat.value == "Not Tracked" ? "" : inputStat.value);
            }
            else if (day.stats[cursorPointer] is TrackerDropdownStat dropdownStat)
            {
                dropdownStat.value = dropdownStat.options[UserInput.CreateOptionMenu($"--- Enter Data For [{dropdownStat.statName}] ---",
                dropdownStat.options.Select(x => x.option).ToArray())].option;
            }
        }

    }

    void StatsMenu()
    {
        int cursorPointer = 0;
        while (true)
        {
            ClearPrimaryConsole();

            DayTrackerData day = data.GetDayTrackerData(data.selectedDate);
            ConsoleLine[] dayStatsSelect = day.stats.Select(
                x => new ConsoleLine($"{x.statName} - {x.value} ", BuildArray(ConsoleColor.Cyan.Extend(x.statName.Length + 2), ConsoleColor.White.ToArray(1000)))).ToArray();
            dayStatsSelect = dayStatsSelect.Concat(new ConsoleLine[] { new ConsoleLine("Exit", ConsoleColor.DarkBlue) }).ToArray();

            cursorPointer = UserInput.CreateOptionMenu($"--- Day [{data.selectedDate}] ---", dayStatsSelect, cursorStartIndex: cursorPointer);
            if (cursorPointer == day.stats.Count)
            {
                day.Display();
                ShiftLine();
                return;
            }

            // --- We've selected a input to generate stats for

            TrackerStat stat = day.stats[cursorPointer];

            List<TrackerStat> statList = new List<TrackerStat>(); //lets get all instances of the stat in a list
            for (int i = 0; i <= data.GetDayIndex(data.today); i++)
            {
                DayTrackerData d = data.GetDayTrackerData(data.startDate.AddDays(i));
                foreach (TrackerStat s in d.stats)
                {
                    if (s.IdentificationString() == stat.IdentificationString() && s.value != "Not Tracked")
                    {
                        statList.Add(s);
                        break;
                    }
                }
            }

            SendConsoleMessage(new ConsoleLine($"Showing Stats For [{stat.statName}]", ConsoleColor.Cyan));
            ShiftLine();

            if (stat is TrackerInputStat inputStat)
            {
                if (inputStat.inputTypes.Contains(UserInputProfile.InputType.Int) || inputStat.inputTypes.Contains(UserInputProfile.InputType.Float))
                {
                    List<float> values = statList.Select(s => float.Parse(s.value)).OrderBy(s => s).ToList();
                    List<(float value, int count)> g = values.GroupBy(v => v).OrderByDescending(v => v.Count()).Select(v => (v.Key, v.Count())).ToList();

                    SendConsoleMessage(new ConsoleLine($"--- Distribution [Total {statList.Count}] ---", ConsoleColor.DarkBlue));
                    SendConsoleMessage(new ConsoleLine($"Mean - {Math.Round(values.Sum() / values.Count, 2)}", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"Min - {values[0]}", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"Lower Quartile - {GetMedian(values.Take(values.Count / 2).ToList())}", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"Medium - {GetMedian(values)}", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"Upper Quartile - {GetMedian(values.Skip((values.Count + 1) / 2).ToList())}", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"Max - {values[^1]}", ConsoleColor.Cyan));
                    ShiftLine();
                    SendConsoleMessage(new ConsoleLine($"Last 5 Days - {statList.TakeLast(Math.Min(5, values.Count)).ToList().ToElementString()}", ConsoleColor.Cyan));
                    ShiftLine();
                    SendConsoleMessage(new ConsoleLine("--- Frequency [Value - Count] ---", ConsoleColor.DarkBlue));

                    int runningCount = 0;
                    for (int i = 0; i < Math.Min(5, g.Count); i++)
                    {
                        runningCount += g[i].count;
                        SendConsoleMessage(new ConsoleLine($"{g[i].value} - {g[i].count}", ConsoleColor.Cyan));
                    }
                    if (runningCount < values.Count) SendConsoleMessage(new ConsoleLine($"Other - {values.Count - runningCount}", ConsoleColor.Cyan));


                }
                else
                {
                    SendConsoleMessage(new ConsoleLine("Details For This Form Of Stat Are Not Yet Supported!", ConsoleColor.Cyan));
                }
            }
            else if (stat is TrackerDropdownStat dropdownStat)
            {
                Dictionary<string, int> d = statList.SelectMany(s => ((TrackerDropdownStat)s).options).Select(s => s.option).Distinct().ToDictionary(s => s, s => 0);

                int successCount = 0;
                foreach (TrackerDropdownStat s in statList)
                {
                    d[s.value] += 1;
                    successCount += s.options.Where(st => st.option == s.value).Select(s => s.success).ToList()[0] ? 1 : 0;
                }

                SendConsoleMessage(new ConsoleLine($"--- Distribution [Total {statList.Count}] ---", ConsoleColor.DarkBlue));
                foreach (string dKey in d.Keys)
                {
                    SendConsoleMessage(new ConsoleLine($"{dKey} - {d[dKey]} ({Math.Round((double)d[dKey] / statList.Count * 100)}%)", ConsoleColor.Cyan));
                }
                ShiftLine();
                SendConsoleMessage(new ConsoleLine($"Completion Rate - {successCount} / {statList.Count} ({Math.Round((double)successCount / statList.Count * 100)}%)", ConsoleColor.Cyan));
            }

            UserInput.WaitForUserInput(space: true);
        }
    }

    void SettingsMenu()
    {
        while (true)
        {
            int settingsMenuIndex = UserInput.CreateOptionMenu("--- Settings ---", new ConsoleLine[] {
                new ConsoleLine("Manual Saves", ConsoleColor.Cyan), new ConsoleLine("Tracked Stats", ConsoleColor.Cyan), new ConsoleLine("Exit", ConsoleColor.DarkBlue) });

            if (settingsMenuIndex == 0)
            {
                while (true)
                {
                    if (UserInput.CreateOptionMenu("--- Options ---", new (ConsoleLine, Action)[] {
                (new ConsoleLine("Create Manual Save", ConsoleColor.Cyan), () => {
                    data.SaveData($"Saves/Data {DateTime.Now.ToString("[yyyy-MM-dd_HH-mm-ss]")}");
                    SendConsoleMessage(new ConsoleLine("Manual Save Created!", ConsoleColor.DarkBlue));
                    UserInput.WaitForUserInput(space: true);
                    ShiftLine(-2);
                }),
                (new ConsoleLine("Load Manual Save", ConsoleColor.Cyan), LoadManualSave),
                (new ConsoleLine("Delete Manual Save", ConsoleColor.Cyan), DeleteManualSave),
                (new ConsoleLine("Exit", ConsoleColor.DarkBlue), () => {})}) == 3) break;
                }
            }
            else if (settingsMenuIndex == 1)
            {

            }
            else
            {
                return;
            }
        }
    }

    void LoadManualSave()
    {
        ConsoleLine[] saves = AppPersistentData.GetSubFiles("Tracker/Saves").Select(x => new ConsoleLine(x, ConsoleColor.Cyan)).Reverse().ToArray();
        if (saves.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Manual Saves Exist!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int saveIndex = UserInput.CreateMultiPageOptionMenu("Saves:", saves, new ConsoleLine[] { new ConsoleLine("Exit", ConsoleColor.DarkBlue) }, 3);

        if (saveIndex >= 0)
        {
            data = new TrackerData(AppPersistentData.LoadFile($"Tracker/Saves/{saves[saveIndex].lineText}"));
            ClearPrimaryConsole();
            data.GetDayTrackerData(data.today).Display();
            ShiftLine();
        }
    }

    void DeleteManualSave()
    {
        ConsoleLine[] saves = AppPersistentData.GetSubFiles("Tracker/Saves").Select(x => new ConsoleLine(x, ConsoleColor.Cyan)).Reverse().ToArray();
        if (saves.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Manual Saves Exist!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int saveIndex = UserInput.CreateMultiPageOptionMenu("Saves:", saves, new ConsoleLine[] { new ConsoleLine("Exit", ConsoleColor.DarkBlue) }, 3);

        if (saveIndex >= 0)
        {
            AppPersistentData.DeleteFile($"Tracker/Saves/{saves[saveIndex].lineText}");
            SendConsoleMessage(new ConsoleLine("Manual Save Deleted!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
        }
    }




}