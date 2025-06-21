using Revistone.Console;
using Revistone.Interaction;
using Revistone.Console.Image;

using static Revistone.Console.ConsoleAction;
using static Revistone.App.BaseApps.Tracker.TrackerData;
using static Revistone.Interaction.UserInputProfile;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.App.BaseApps.Tracker;

public static class TrackerSettingMenus
{
    static Setting[] settings = [
        new Setting("Arcs", [("Create Arc", CreateArc), ("Edit Arc", EditArc), ("Delete Arc", DeleteArc)]),
        new Setting("Stats", [("Create Stat", CreateStat), ("Edit Stat", EditStat), ("Delete Stat", DeleteStat)]),
        new Setting("Saves", [("Create Save", CreateSave), ("Load Save", LoadSave), ("Delete Save", DeleteSave)]),
    ];

    // --- GENERAL SETTINGS ---

    public static void SettingsMenu()
    {
        int settingIndex = 0;
        while (true)
        {
            settingIndex = UserInput.CreateOptionMenu("--- Settings ---",
            settings.Select(s => new ConsoleLine(s.settingName, ConsoleColour.Cyan)).Concat([new ConsoleLine("Exit", ConsoleColour.DarkBlue)]).ToArray(),
            cursorStartIndex: settingIndex);

            if (settingIndex == settings.Length) return;

            int i = 0;
            while (true)
            {
                i = UserInput.CreateOptionMenu($"--- {settings[settingIndex].settingName[..^1]} Settings ---",
                settings[settingIndex].options.Select(o => (new ConsoleLine(o.name, ConsoleColour.Cyan), o.action)).Concat([(new ConsoleLine("Exit", ConsoleColour.DarkBlue), () => { })]).ToArray(),
                cursorStartIndex: i);
                if (i == settings[settingIndex].options.Count) break;
            }
        }

    }

    struct Setting(string settingName, List<(string, Action)> options)
    {
        public string settingName = settingName;
        public List<(string name, Action action)> options = options;
    }

    // --- ARCS ---

    static void CreateArc()
    {
        List<string> arcData = LoadFile(GeneratePath(DataLocation.App, "Tracker", "ArcData")).ToList();
        string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Arc Name ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.FullText));
        arcData.Add(statName);
        arcData.Add(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{statName}] Start Date ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.DateOnly)));
        arcData.Add(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{statName}] End Date ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.DateOnly)));
        SaveFile(GeneratePath(DataLocation.App, "Tracker", "ArcData"), arcData.ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    static void EditArc()
    {
        (string name, DateOnly startDate, DateOnly endDate)[] arcs = LoadFile(GeneratePath(DataLocation.App, "Tracker", "ArcData"), 3, true).Select(x => (x[0], DateOnly.Parse(x[1]), DateOnly.Parse(x[2]))).ToArray();
        if (arcs.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Arcs Exist!", ConsoleColour.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int arcIndex = UserInput.CreateMultiPageOptionMenu("Arcs", arcs.Select(x => new ConsoleLine($"{x.name}: [{x.startDate}] - [{x.endDate}]", ConsoleColour.Cyan)).ToArray(),
        [new ConsoleLine("Exit", ConsoleColour.DarkBlue)], 3);
        if (arcIndex == -1) return; //exit

        arcs[arcIndex].startDate = DateOnly.Parse(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{arcs[arcIndex].name}] Start Date ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.DateOnly)));
        arcs[arcIndex].endDate = DateOnly.Parse(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{arcs[arcIndex].name}] End Date ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.DateOnly)));

        SaveFile(GeneratePath(DataLocation.App, "Tracker", "ArcData"),
        arcs.Select(x => new string[] { x.name, x.startDate.ToString(), x.endDate.ToString() }).SelectMany(arr => arr).ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    static void DeleteArc()
    {
        List<(string name, DateOnly startDate, DateOnly endDate)> arcs = LoadFile(GeneratePath(DataLocation.App, "Tracker", "ArcData"), 3, true).Select(x => (x[0], System.DateOnly.Parse(x[1]), System.DateOnly.Parse(x[2]))).ToList();
        if (arcs.Count == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Arcs Exist!", ConsoleColour.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int arcIndex = UserInput.CreateMultiPageOptionMenu("Arcs", arcs.Select(x => new ConsoleLine($"{x.name}: [{x.startDate}] - [{x.endDate}]", ConsoleColour.Cyan)).ToArray(),
        [new ConsoleLine("Exit", ConsoleColour.DarkBlue)], 3);
        if (arcIndex == -1) return; //exit

        arcs.RemoveAt(arcIndex);

        SaveFile(GeneratePath(DataLocation.App, "Tracker", "ArcData"),
        arcs.Select(x => new string[] { x.name, x.startDate.ToString(), x.endDate.ToString() }).SelectMany(arr => arr).ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    // --- STATS ---

    static void CreateStat()
    {
        int statType = UserInput.CreateOptionMenu("--- Options ---", [
            new ConsoleLine("Add Input Stat", ConsoleColour.Cyan),
            new ConsoleLine("Add Dropdown Stat", ConsoleColour.Cyan),
            new ConsoleLine("Exit", ConsoleColour.DarkBlue)]);
        if (statType == 2) return; //exit

        string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Stat Name ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.FullText));
        if (TrackerDayData.trackedStats.Select(x => x.statName).Contains(statName))
        {
            SendConsoleMessage(new ConsoleLine("--- Input Invalid ---", ConsoleColour.Cyan));
            SendConsoleMessage(new ConsoleLine($"1. Stat Name [{statName}] Is Already In Use!", ConsoleColour.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-3);
            CreateStat();
            return;
        }

        if (statType == 0)
        {
            int validType = UserInput.CreateOptionMenu("Valid Inputs", [
                new ConsoleLine("Int", ConsoleColour.Cyan),
                new ConsoleLine("Number", ConsoleColour.Cyan),
                new ConsoleLine("Text", ConsoleColour.Cyan),
            ]);

            TrackerDayData.trackedStats.Add(new TrackerInputStat(statName, validType == 0 ? [InputType.Int] : validType == 1 ? [InputType.Float, InputType.Int] : [InputType.FullText, InputType.PartialText]));
            DATA.GetDayData(DATA.today).stats.Add(new TrackerInputStat(statName, validType == 0 ? [InputType.Int] : validType == 1 ? [InputType.Float, InputType.Int] : [InputType.FullText, InputType.PartialText]));
        }
        else
        {
            List<(string option, bool success)> options = [];
            while (true)
            {
                string option = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Dropdown Option ---", ConsoleColour.DarkBlue), new UserInputProfile([InputType.FullText, InputType.PartialText]));
                bool success = UserInput.CreateTrueFalseOptionMenu($"--- Is [{option}] A Success? ---");
                options.Add((option, success));

                if (!UserInput.CreateTrueFalseOptionMenu("--- Options ---", "Add Dropdown Option", "Finish")) break;
            }

            TrackerDayData.trackedStats.Add(new TrackerDropdownStat(statName, options.ToArray()));
            DATA.GetDayData(DATA.today).stats.Add(new TrackerDropdownStat(statName, options.ToArray()));
        }

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    static void EditStat()
    {
        ClearPrimaryConsole();

        TrackerDayData day = DATA.GetDayData(DATA.today);
        int pointer = 0;
        day.DisplayAsMenu(ref pointer);

        if (pointer < day.stats.Count)
        {
            while (true)
            {
                string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter New Stat Name ---", ConsoleColour.DarkBlue), new UserInputProfile(InputType.FullText));
                if (TrackerDayData.trackedStats.Select(x => x.statName).Contains(statName))
                {
                    SendConsoleMessage(new ConsoleLine("--- Input Invalid ---", ConsoleColour.Cyan));
                    SendConsoleMessage(new ConsoleLine($"1. Stat Name [{statName}] Is Already In Use!", ConsoleColour.DarkBlue));
                    UserInput.WaitForUserInput(space: true);
                    ShiftLine(-3);
                }
                else
                {
                    TrackerDayData.trackedStats[pointer].UpdateName(statName);
                    for (int i = 0; i <= DATA.GetDayIndex(DATA.today); i++)
                    {
                        DATA.GetDayData(DATA.startDate.AddDays(i)).stats[pointer].UpdateName(statName);
                    }
                    DATA.Save();
                    break;
                }
            }

        }

        DATA.GetDayData(DATA.selectedDate).Display();
    }

    static void DeleteStat()
    {
        ClearPrimaryConsole();

        TrackerDayData day = DATA.GetDayData(DATA.today);
        int pointer = 0;
        day.DisplayAsMenu(ref pointer);

        if (pointer < day.stats.Count)
        {
            day.stats.RemoveAt(pointer);
            TrackerDayData.trackedStats.RemoveAt(pointer);
        }

        DATA.GetDayData(DATA.selectedDate).Display();
    }

    // --- SAVES ---

    static void CreateSave()
    {
        DATA.Save($"Saves/Data {DateTime.Now.ToString("[yyyy-MM-dd_HH-mm-ss]")}");
        SendConsoleMessage(new ConsoleLine("Manual Save Created!", ConsoleColour.DarkBlue));
        UserInput.WaitForUserInput(space: true);
        ShiftLine(-2);
    }

    static void LoadSave()
    {
        string savePath = GetSavePath();
        if (savePath.Length > 0)
        {
            DATA = new TrackerData(LoadFile(GeneratePath(DataLocation.App, "Tracker", $"Saves/{savePath}")));
            ClearPrimaryConsole();
            DATA.GetDayData(DATA.today).Display();
        }
    }

    static void DeleteSave()
    {
        string savePath = GetSavePath();
        if (savePath.Length > 0)
        {
            DeleteFile(GeneratePath(DataLocation.App, "Tracker", $"Saves/{savePath}"));
            SendConsoleMessage(new ConsoleLine("Manual Save Deleted!", ConsoleColour.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
        }
    }

    static string GetSavePath()
    {
        ConsoleLine[] saves = GetSubFiles(GeneratePath(DataLocation.App, "Tracker", $"Saves")).Select(x => new ConsoleLine(x, ConsoleColour.Cyan)).Reverse().ToArray();
        if (saves.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Manual Saves Exist!", ConsoleColour.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return "";
        }

        int saveIndex = UserInput.CreateMultiPageOptionMenu("Saves:", saves, [new ConsoleLine("Exit", ConsoleColour.DarkBlue)], 3);
        if (saveIndex == -1) return "";
        return saves[saveIndex].LineText;
    }
}