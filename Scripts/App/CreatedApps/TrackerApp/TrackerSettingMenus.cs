using Revistone.Console;
using Revistone.Interaction;

using static Revistone.Console.ConsoleAction;
using static Revistone.App.Tracker.TrackerData;
using static Revistone.Interaction.UserInputProfile;
using System.Security.Cryptography;

namespace Revistone.App.Tracker;

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
            settings.Select(s => new ConsoleLine(s.settingName, ConsoleColor.Cyan)).Concat([new ConsoleLine("Exit", ConsoleColor.DarkBlue)]).ToArray(),
            cursorStartIndex: settingIndex);

            if (settingIndex == settings.Length) return;

            int i = 0;
            while (true)
            {
                i = UserInput.CreateOptionMenu($"--- {settings[settingIndex].settingName[..^1]} Settings ---",
                settings[settingIndex].options.Select(o => (new ConsoleLine(o.name, ConsoleColor.Cyan), o.action)).Concat([(new ConsoleLine("Exit", ConsoleColor.DarkBlue), () => { })]).ToArray(),
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
        List<string> arcData = AppPersistentData.LoadFile("Tracker/Arc Data").ToList();
        string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Arc Name ---", ConsoleColor.DarkBlue), new UserInputProfile(InputType.FullText));
        arcData.Add(statName);
        arcData.Add(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{statName}] Start Date ---", ConsoleColor.DarkBlue), new UserInputProfile(InputType.DateOnly)));
        arcData.Add(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{statName}] End Date ---", ConsoleColor.DarkBlue), new UserInputProfile(InputType.DateOnly)));
        AppPersistentData.SaveFile("Tracker/Arc Data", arcData.ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    static void EditArc()
    {
        (string name, DateOnly startDate, DateOnly endDate)[] arcs = AppPersistentData.LoadFile("Tracker/Arc Data", 3, true).Select(x => (x[0], DateOnly.Parse(x[1]), DateOnly.Parse(x[2]))).ToArray();
        if (arcs.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Arcs Exist!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int arcIndex = UserInput.CreateMultiPageOptionMenu("Arcs", arcs.Select(x => new ConsoleLine($"{x.name}: [{x.startDate}] - [{x.endDate}]", ConsoleColor.Cyan)).ToArray(),
        [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 3);
        if (arcIndex == -1) return; //exit

        arcs[arcIndex].startDate = DateOnly.Parse(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{arcs[arcIndex].name}] Start Date ---", ConsoleColor.DarkBlue), new UserInputProfile(UserInputProfile.InputType.DateOnly)));
        arcs[arcIndex].endDate = DateOnly.Parse(UserInput.GetValidUserInput(new ConsoleLine($"--- Enter [{arcs[arcIndex].name}] End Date ---", ConsoleColor.DarkBlue), new UserInputProfile(UserInputProfile.InputType.DateOnly)));

        AppPersistentData.SaveFile("Tracker/Arc Data",
        arcs.Select(x => new string[] { x.name, x.startDate.ToString(), x.endDate.ToString() }).SelectMany(arr => arr).ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    static void DeleteArc()
    {
        List<(string name, DateOnly startDate, DateOnly endDate)> arcs = AppPersistentData.LoadFile("Tracker/Arc Data", 3, true).Select(x => (x[0], System.DateOnly.Parse(x[1]), System.DateOnly.Parse(x[2]))).ToList();
        if (arcs.Count == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Arcs Exist!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return;
        }

        int arcIndex = UserInput.CreateMultiPageOptionMenu("Arcs", arcs.Select(x => new ConsoleLine($"{x.name}: [{x.startDate}] - [{x.endDate}]", ConsoleColor.Cyan)).ToArray(),
        [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 3);
        if (arcIndex == -1) return; //exit

        arcs.RemoveAt(arcIndex);

        AppPersistentData.SaveFile("Tracker/Arc Data",
        arcs.Select(x => new string[] { x.name, x.startDate.ToString(), x.endDate.ToString() }).SelectMany(arr => arr).ToArray());

        ClearPrimaryConsole();
        DATA.GetDayData(DATA.today).Display();
    }

    // --- STATS ---

    static void CreateStat()
    {
        int statType = UserInput.CreateOptionMenu("--- Options ---", [
            new ConsoleLine("Add Input Stat", ConsoleColor.Cyan),
            new ConsoleLine("Add Dropdown Stat", ConsoleColor.Cyan),
            new ConsoleLine("Exit", ConsoleColor.DarkBlue)]);
        if (statType == 2) return; //exit

        string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Stat Name ---", ConsoleColor.DarkBlue), new UserInputProfile(InputType.FullText));
        if (TrackerDayData.trackedStats.Select(x => x.statName).Contains(statName))
        {
            SendConsoleMessage(new ConsoleLine("--- Input Invalid ---", ConsoleColor.Cyan));
            SendConsoleMessage(new ConsoleLine($"1. Stat Name [{statName}] Is Already In Use!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-3);
            CreateStat();
            return;
        }

        if (statType == 0)
        {
            int validType = UserInput.CreateOptionMenu("Valid Inputs", [
                new ConsoleLine("Int", ConsoleColor.Cyan),
                new ConsoleLine("Number", ConsoleColor.Cyan),
                new ConsoleLine("Text", ConsoleColor.Cyan),
            ]);

            TrackerDayData.trackedStats.Add(new TrackerInputStat(statName, validType == 0 ? [InputType.Int] : validType == 1 ? [InputType.Float, InputType.Int] : [InputType.FullText, InputType.PartialText]));
            DATA.GetDayData(DATA.today).stats.Add(new TrackerInputStat(statName, validType == 0 ? [InputType.Int] : validType == 1 ? [InputType.Float, InputType.Int] : [InputType.FullText, InputType.PartialText]));
        }
        else
        {
            List<(string option, bool success)> options = [];
            while (true)
            {
                string option = UserInput.GetValidUserInput(new ConsoleLine("--- Enter Dropdown Option ---", ConsoleColor.DarkBlue), new UserInputProfile([InputType.FullText, InputType.PartialText]));
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
                string statName = UserInput.GetValidUserInput(new ConsoleLine("--- Enter New Stat Name ---", ConsoleColor.DarkBlue), new UserInputProfile(InputType.FullText));
                if (TrackerDayData.trackedStats.Select(x => x.statName).Contains(statName))
                {
                    SendConsoleMessage(new ConsoleLine("--- Input Invalid ---", ConsoleColor.Cyan));
                    SendConsoleMessage(new ConsoleLine($"1. Stat Name [{statName}] Is Already In Use!", ConsoleColor.DarkBlue));
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
        SendConsoleMessage(new ConsoleLine("Manual Save Created!", ConsoleColor.DarkBlue));
        UserInput.WaitForUserInput(space: true);
        ShiftLine(-2);
    }

    static void LoadSave()
    {
        string savePath = GetSavePath();
        if (savePath.Length > 0)
        {
            DATA = new TrackerData(AppPersistentData.LoadFile($"Tracker/Saves/{savePath}"));
            ClearPrimaryConsole();
            DATA.GetDayData(DATA.today).Display();
        }
    }

    static void DeleteSave()
    {
        string savePath = GetSavePath();
        SendDebugMessage(savePath);
        if (savePath.Length > 0)
        {
            AppPersistentData.DeleteFile($"Tracker/Saves/{savePath}");
            SendConsoleMessage(new ConsoleLine("Manual Save Deleted!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
        }
    }

    static string GetSavePath()
    {
        ConsoleLine[] saves = AppPersistentData.GetSubFiles("Tracker/Saves").Select(x => new ConsoleLine(x, ConsoleColor.Cyan)).Reverse().ToArray();
        if (saves.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Manual Saves Exist!", ConsoleColor.DarkBlue));
            UserInput.WaitForUserInput(space: true);
            ShiftLine(-2);
            return "";
        }

        int saveIndex = UserInput.CreateMultiPageOptionMenu("Saves:", saves, [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 3);
        if (saveIndex == -1) return "";
        return saves[saveIndex].lineText;
    }
}