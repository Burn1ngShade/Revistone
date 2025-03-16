using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using static Revistone.App.SettingsApp.Setting;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.App;

/// <summary> App for all revistone settings that effect entire application. </summary>
public class SettingsApp : App
{
    // --- APP BOILER ---

    public static event SettingEventHandler OnSettingChanged = new SettingEventHandler((settingName) => { });
    public delegate void SettingEventHandler(string settingName);

    static Setting[] settings = [
        new InputSetting("Username", "The Name Used To Refer To The User.", "User", SettingCategory.User, new UserInputProfile(bannedChars: "\n\"\' ")),
        new DropdownSetting("Pronouns", "The Pronouns Used To Refer To The User", "Prefer Not To Say", SettingCategory.User,
        ["Prefer Not To Say", "He/Him", "She/Her", "They/Them", "He/She",  "He/Them", "She/Them"]),
        new InputSetting("API Key", "ChatGPT API Key (I Promise I Don't Steal This Data - The Project Is Open Source Just Check!)", "",
        SettingCategory.ChatGPT, new UserInputProfile(canBeEmpty: true)),
        new InputSetting("Behaviour", "Additional Promt Given To Dictate GPT Behaviour.", "",
        SettingCategory.ChatGPT, new UserInputProfile(canBeEmpty: true)),
        new InputSetting("Scenario", "Additonal Promt Given To Setup GPT Scenario.", "",
        SettingCategory.ChatGPT, new UserInputProfile(canBeEmpty:true)),
        new InputSetting("Memory", "How Many Previous Messages In The Conversation Should GPT Remember? (This Has A Large Effect On Token Usage).", "5", SettingCategory.ChatGPT,
        new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 0, numericMax: 50)),
        new InputSetting("Temperature", "How Random GPT Responses Are (0 [Not Random] - 2 [Unintelligible])", "1", SettingCategory.ChatGPT,
        new UserInputProfile([UserInputProfile.InputType.Float, UserInputProfile.InputType.Int], numericMin: 0, numericMax: 2)),
        new InputSetting("GPT Name", "The Name Given To ChatGPT", "GPT", SettingCategory.ChatGPT, new UserInputProfile(bannedChars: "\n\"\' ")),
        new InputSetting("Input History", "The Number Of Previous User Inputs Stored. (10 - 1,000)", "100", SettingCategory.Input,
        new UserInputProfile([UserInputProfile.InputType.Float, UserInputProfile.InputType.Int], numericMin: 10, numericMax: 1000)),
        new DropdownSetting("Cursor Jump Separators", "The Charchters That The Cursor Uses To Divide Words.", ",. ", SettingCategory.Input,
        [",. ", ",.!?-_;: ", ",.!?-_;:(){}[] "]),
        new DropdownSetting("Create Log File", "Should HoneyC Programs Create A Program Log File?", "Yes", SettingCategory.HoneyC,
        ["Yes", "No"]),
        new DropdownSetting("Show FPS Widget", "Should The FPS Widget Be Shown?", "Yes", SettingCategory.Widget,
        ["Yes", "No"]),
        new DropdownSetting("Show Author Widget", "Should The Author Widget Be Shown?", "Yes", SettingCategory.Widget,
        ["Yes", "No"]),
        new DropdownSetting("Show Time Widget", "Should The Time Widget Be Shown?", "Yes", SettingCategory.Widget,
        ["Yes", "No"]),
        new DropdownSetting("Analytics Update Frequency", "How Often Should Analytics Update? (Can Effect Performance On Low End Devices, All Unsaved Analytics Are Lossed On Console Close).", "0.5s", SettingCategory.Performance,
        ["0.25s", "0.5s", "1s", "2.5s", "5s", "10s", "30s", "60s"]),
        new DropdownSetting("Widget Update Frequency", "How Often Should Widgets Update? (Can Effect Performance On Low End Devices, But Lower Settings Will Make Widgets Appear Laggy).", "0.025s", SettingCategory.Performance,
        ["0.025s", "0.05s", "0.1s", "0.2s", "0.5s", "1s"])
    ];

    public SettingsApp() : base() { }
    public SettingsApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [new SettingsApp("Settings", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 40)];
    }

    public override void OnRevistoneStartup()
    {
        LoadSettings(); //load all currently saved settings
    }

    public override void ExitApp()
    {
        base.ExitApp();
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("SETTINGS", AdvancedHighlight(97, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        int catIndex = 0;
        while (true)
        {
            catIndex = UserInput.CreateOptionMenu("--- Options ---", ((SettingCategory[])Enum.GetValues(typeof(SettingCategory))).Select(x => new ConsoleLine(x.ToString(), ConsoleColor.Cyan)).Concat(
                [new ConsoleLine("Exit", ConsoleColor.DarkBlue)]
            ).ToArray(), cursorStartIndex: catIndex);

            if (catIndex == Enum.GetValues(typeof(SettingCategory)).Length)
            {
                ExitApp();
                return;
            }

            Setting[] selectedCat = settings.Where(x => (int)x.category == catIndex).ToArray();
            int setIndex = 0;
            while (true)
            {
                setIndex = UserInput.CreateOptionMenu($"--- {(SettingCategory)catIndex} ---", selectedCat.Select(x => new ConsoleLine(x.settingName, ConsoleColor.Cyan)).Concat(
                    [new ConsoleLine("Exit", ConsoleColor.DarkBlue)]
                ).ToArray(), cursorStartIndex: setIndex);

                if (setIndex == selectedCat.Length) break;
                HandleSettingSet(selectedCat[setIndex]);
            }
        }
    }

    /// <summary>
    /// Allows the user to modify a given setting
    /// </summary>
    static void HandleSettingSet(Setting setting, bool clearConsole = true)
    {
        int userOption = 0;
        int settingIndex = GetSettingIndex(setting.settingName);

        while (true)
        {
            SendConsoleMessage(new ConsoleLine($"--- {setting.settingName} - {setting.category} ---", ConsoleColor.DarkBlue));
            SendConsoleMessage(new ConsoleLine($"{setting.settingName} - '{setting.currentValue}'", BuildArray(ConsoleColor.Cyan.Extend(setting.settingName.Length + 3), ConsoleColor.White.ToArray())));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Summary: {setting.description}", ConsoleColor.DarkBlue));
            SendConsoleMessage(new ConsoleLine($"Default Value: '{setting.defaultValue}'", ConsoleColor.DarkBlue));
            ShiftLine();

            userOption = UserInput.CreateOptionMenu("--- Options ---", [
            new ConsoleLine("Edit Value", ConsoleColor.Cyan), new ConsoleLine("Reset Value", ConsoleColor.Cyan), new ConsoleLine("Exit", ConsoleColor.DarkBlue)],
            cursorStartIndex: userOption);

            if (clearConsole) ClearPrimaryConsole();
            else
            {
                ClearLines(6);
                ShiftLine(-6);
            }


            switch (userOption)
            {
                case 0:
                    if (setting is InputSetting inputSetting)
                    {
                        settings[settingIndex].currentValue = UserInput.GetValidUserInput($"--- Update [{inputSetting.settingName}] ---", inputSetting.inputProfile, setting.currentValue, 5);
                    }
                    else if (setting is DropdownSetting dropdownSetting)
                    {
                        settings[settingIndex].currentValue = dropdownSetting.options[UserInput.CreateOptionMenu($"--- Update [{dropdownSetting.settingName}] ---",
                        dropdownSetting.options.Select(x => new ConsoleLine(x, ConsoleColor.Cyan)).ToArray())];
                    }
                    SaveSettings();
                    OnSettingChanged.Invoke(setting.settingName);
                    break;
                case 1:
                    settings[settingIndex].currentValue = settings[settingIndex].defaultValue;
                    SaveSettings();
                    OnSettingChanged.Invoke(setting.settingName);
                    break;
                case 2:
                    return;
            }
        }
    }

    /// <summary>
    /// Allows the user to modify a given setting
    /// </summary>
    public static void HandleSettingSet(string setting)
    {
        HandleSettingSet(settings[GetSettingIndex(setting)], false);
        HandleSettingGet(setting);
    }

    /// <summary>
    /// Allows the user to view a given setting
    /// </summary>
    public static void HandleSettingGet(string setting)
    {
        Setting s = settings[GetSettingIndex(setting)];
        SendConsoleMessage(new ConsoleLine($"--- {s.settingName} - {s.category} ---", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"{s.settingName} - '{s.currentValue}'", BuildArray(ConsoleColor.Cyan.Extend(s.settingName.Length + 3), ConsoleColor.White.ToArray())));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"Summary: {s.description}", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"Default Value: '{s.defaultValue}'", ConsoleColor.DarkBlue));
    }

    // --- SAVE AND LOAD ---

    void LoadSettings()
    {
        string[][] settingsTxt = LoadFile(GeneratePath(DataLocation.Console, "Settings", "Settings.txt"), 2, true);

        for (int i = 0; i < settingsTxt.Length; i++)
        {
            int s = GetSettingIndex(settingsTxt[i][0]);
            if (s == -1) continue;
            settings[s].currentValue = settingsTxt[i][1];
        }

        UpdateSettingsDict();
    }

    static void SaveSettings()
    {
        string[] settingsTxt = new string[settings.Length * 2];

        for (int i = 0; i < settings.Length; i++)
        {
            settingsTxt[i * 2] = settings[i].settingName;
            settingsTxt[i * 2 + 1] = settings[i].currentValue;
        }

        SaveFile(GeneratePath(DataLocation.Console, "Settings", "Settings.txt"), settingsTxt);

        UpdateSettingsDict();
    }

    static void UpdateSettingsDict()
    {
        currentSettings = [];

        foreach (Setting setting in settings)
        {
            currentSettings.Add(setting.settingName.ToLower(), setting.currentValue);
        }
    }

    static int GetSettingIndex(string settingName)
    {
        for (int i = 0; i < settings.Length; i++)
        {
            if (settings[i].settingName.ToLower() == settingName.ToLower()) return i;
        }

        return -1;
    }

    // --- USE SETTINGS ---

    // use dictionary to acsess settings to save time on searching through
    static Dictionary<string, string> currentSettings = [];

    public static bool SettingExists(string settingName)
    {
        return currentSettings.ContainsKey(settingName.ToLower());
    }

    public static string GetValue(string settingName)
    {
        if (!currentSettings.ContainsKey(settingName.ToLower())) return "";

        return currentSettings[settingName.ToLower()];
    }

    // --- SETTING CLASS ---

    public abstract class Setting
    {
        public enum SettingCategory { User, Input, ChatGPT, HoneyC, Widget, Performance }
        public SettingCategory category;

        public string settingName = "";
        public string description = "Placeholder Description";

        public string currentValue = "";
        public string defaultValue = "";

        public Setting(string settingName, string description, string defaultValue, SettingCategory category)
        {
            this.settingName = settingName;
            this.description = description;
            this.currentValue = defaultValue;
            this.defaultValue = defaultValue;
            this.category = category;
        }
    }

    public class InputSetting : Setting
    {
        public UserInputProfile inputProfile;

        public InputSetting(string settingName, string description, string defaultValue, SettingCategory category, UserInputProfile inputProfile) : base(settingName, description, defaultValue, category)
        {
            this.inputProfile = inputProfile;
        }
    }

    public class DropdownSetting : Setting
    {
        public string[] options;

        public DropdownSetting(string settingName, string description, string defaultValue, SettingCategory category, string[] options) : base(settingName, description, defaultValue, category)
        {
            this.options = options;
        }
    }
}