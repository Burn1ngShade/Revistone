using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;
using Revistone.App.Command;
using Revistone.Management;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using static Revistone.App.BaseApps.SettingsApp.Setting;
using static Revistone.Functions.PersistentDataFunctions;
using System.Dynamic;
using Revistone.Console.Image;

namespace Revistone.App.BaseApps;

/// <summary> App for all revistone settings that effect entire application. </summary>
public class SettingsApp : App
{
    // --- APP BOILER ---

    public static event SettingEventHandler OnSettingChanged = new SettingEventHandler((settingName) => { });
    public delegate void SettingEventHandler(string settingName);

    static readonly string[] YesNoOpt = ["Yes", "No"];
    static readonly string[] ColourOpt = ["White", "Gray", "Dark Gray", "Black", "Red", "Dark Red", "Yellow", "Dark Yellow", "Green", "Dark Green", "Cyan", "Dark Cyan", "Blue", "Dark Blue", "Magenta", "Dark Magenta"];

    //User, Input, ChatGPT, HoneyC, Widget, Performance, Developer
    static readonly string[] CategoryDescriptions = [
        "Settings About You, And Personalising The Console Just For You.", // Behaviour
        "Settings About Interaction With The Console.", // Input
        "Settings About The Consoles Appearance.", // Appearance
        "Settings About Revistone Custom ChatGPT Model.", // ChatGPT
        "Settings That Effect Console Performance.", // Performance
        "Settings For Developers, And To Debug The Console. These Settings Will Only Apply If In Developer Mode, Else Default Values Are Used.", // Developer
    ];

    public static readonly Setting[] settings = [
        // --- BEHAVIOUR SETTINGS ---
        new InputSetting("Username",
            "The Name Used To Refer To The User.", "User", new UserInputProfile(bannedChars: "\n\"\'"), SettingCategory.Behaviour),
        new DropdownSetting("Pronouns",
            "The Pronouns Used To Refer To The User", "Prefer Not To Say", ["Prefer Not To Say", "He/Him", "She/Her", "They/Them", "He/She",  "He/Them", "She/Them"],SettingCategory.Behaviour),
        new DropdownSetting("Auto Resume",
            "Should The Console Load Last Used App On Startup?", "Yes", YesNoOpt, SettingCategory.Behaviour),
        // --- INPUT SETTINGS ---
        new InputSetting("Input History",
            "The Number Of Previous User Inputs Stored (10 - 1,000).", "100", new UserInputProfile([UserInputProfile.InputType.Float, UserInputProfile.InputType.Int], numericMin: 10, numericMax: 1000), SettingCategory.Input),
        new DropdownSetting("Cursor Jump Separators",
            "The Charchters That The Cursor Uses To Divide Words.", ",. ", [",. ", ",.!?-_;: ", ",.!?-_;:(){}[] ", ",.!?-_;:(){}[]+-/*^% "], SettingCategory.Input),
        new DropdownSetting("Input Text Colour",
            "The Colour Of User Input Text.", "White", ColourOpt, SettingCategory.Input),
        new DropdownSetting("Input Cursor Colour",
            "The Colour Of User Input Cursor.", "White", ColourOpt, SettingCategory.Input),
        new DropdownSetting("Input Cursor Trail Colour",
            "The Colour Of User Input Cursor Trail.", "Dark Gray", ColourOpt, SettingCategory.Input),
        // --- APPEARANCE SETTINGS ---
        new DropdownSetting("Detailed Read Menus",
            "Should Read Menus Display The Number Of Options In Each Category?", "Yes", YesNoOpt, SettingCategory.Appearance),
        new DropdownSetting("Show FPS Widget",
            "Should The FPS Widget Be Shown?", "Yes", YesNoOpt, SettingCategory.Appearance),
        new DropdownSetting("Show Author Widget",
            "Should The Author Widget Be Shown?", "Yes", YesNoOpt, SettingCategory.Appearance),
        new DropdownSetting("Show Time Widget",
            "Should The Time Widget Be Shown?", "Time Only", ["Time Only", "Date And Time", "No"], SettingCategory.Appearance),
        new DropdownSetting("Show Workspace Path Widget",
            "Should The Workspace Path Widget Be Shown?", "Yes", YesNoOpt, SettingCategory.Appearance),
        new DropdownSetting("Workspace Path Widget Collapsing",
            "Collapse Workspace Path Widget To Save Space On The Border Bar", "30", ["No", "15", "30", "50", "75", "100"], SettingCategory.Appearance),
        // --- CHATGPT SETTINGS ---
        new InputSetting("API Key",
            "ChatGPT API Key (I Promise I Don't Steal This Data - The Project Is Open Source Just Check!)", "", new UserInputProfile(canBeEmpty: true), SettingCategory.ChatGPT),
        new InputSetting("Behaviour",
            "Additional Promt Given To Dictate GPT Behaviour.", "", new UserInputProfile(canBeEmpty: true), SettingCategory.ChatGPT),
        new InputSetting("Scenario",
            "Additonal Promt Given To Setup GPT Scenario.", "", new UserInputProfile(canBeEmpty:true), SettingCategory.ChatGPT),
        new InputSetting("Conversation Memory",
            "How Many Previous Messages In The Conversation Should GPT Remember? (This Has A Large Effect On Token Usage).", "10", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 0, numericMax: 50), SettingCategory.ChatGPT),
        new DropdownSetting("Long Term Memory",
            "Should GPT Use It's Long Term Memory? (When Off GPT Is Unable To Create New Memories).", "Yes", YesNoOpt, SettingCategory.ChatGPT),
        new InputSetting("Temperature",
            "How Random GPT Responses Are (0 [Not Random] - 2 [Unintelligible])", "1", new UserInputProfile([UserInputProfile.InputType.Float, UserInputProfile.InputType.Int], numericMin: 0, numericMax: 2), SettingCategory.ChatGPT),
        new InputSetting("GPT Name",
            "The Name Given To GPT", "GPT", new UserInputProfile(bannedChars: "\n\"\' "), SettingCategory.ChatGPT),
        new DropdownSetting("GPT Model",
            "What Model Of GPT Should Be Used, 4o Mini Is Very Cheap, While o3 Mini Is More Expensive, But Better For Programming Tasks.", "gpt-4o-mini", ["gpt-4o-mini", "o3-mini"], SettingCategory.ChatGPT),
        new DropdownSetting("Welcome Message",
            "GPT Will Send A Message Upon Console Startup (Will Increase Inital Load Time).", "No", YesNoOpt, SettingCategory.ChatGPT),
        new DropdownSetting("Use Detailed System Promt",
            "Should GPT Use A Detailed System Promt, Giving It Details And Info About The Revistone Console, More Token Intensive.", "Yes", YesNoOpt, SettingCategory.ChatGPT),
        // --- PERFORMANCE SETTINGS ---
        new DropdownSetting("Target Frame Rate",
            "The Number Of Times The Console Will Render A Second. Using A Frame Rate Faster Than Your Monitors Refresh Rate Will Actually Slightly Slow Done Responsiveness.", "120", ["30", "60", "75", "90", "120", "144", "240"], SettingCategory.Performance),
        new DropdownSetting("Analytics Update Frequency",
            "How Often Should Analytics Update? (Can Effect Performance On Low End Devices, Very Frequent Settings Should Only Be Used For Debugging).", "180s", ["10s", "60s", "120s", "180s", "300s", "600s"], SettingCategory.Performance),
        new DropdownSetting("Widget Update Frequency",
            "How Often Should Widgets Update? (Can Effect Performance On Low End Devices, But Lower Settings Will Make Widgets Appear Laggy).", "0.025s",
            ["0.025s", "0.05s", "0.1s", "0.2s", "0.5s", "1s"], SettingCategory.Performance),
        new DropdownSetting("Show Emojis",
            "Can Emojis Be Used In The Program (Certain Emojis Cause The Console To Misrender, Requiring A Reload To Fix).", "No", YesNoOpt, SettingCategory.Performance),
        // --- DEVELOPER SETTINGS ---
        new DropdownSetting("Developer Mode",
            "Enables Debugging Tools And Commands", "No", YesNoOpt, SettingCategory.Developer),
        new DropdownSetting("Advanced Session Log",
            "Uses An Extra Thread To Run A Debbugging Loop, Useful For Debugging But Will Cause A Performance Hit. Modify Time Between Updates Based On Use Case.", "Off", ["Off", "2s", "5s", "10s", "30s"], SettingCategory.Developer, true),
        new DropdownSetting("Block Rendering On Crash",
            "Pauses Rendering On Crash, Showing C# Compiler Error But Preventing Final Rendering Passes.", "No", YesNoOpt, SettingCategory.Developer),
        new DropdownSetting("Show GPT Tool Results",
            "Outputs GPT Tool Results To The Debug Console (GPT Can Sometimes Be A Little Bit Stupid, Use For Promt Improvement).", "No", YesNoOpt, SettingCategory.Developer),
        new DropdownSetting("Log GPT Messages",
            "Outputs GPT System Messages To The Debug Analytics File.", "None", ["None", "System Message", "Full Context Message", "Query Info"], SettingCategory.Developer),
        new DropdownSetting("Force Default Settings",
            "Override All Settings With Default Value.", "No", YesNoOpt, SettingCategory.Developer),
        new DropdownSetting("Create Log File",
            "Should HoneyC Programs Create A Program Log File?", "No", YesNoOpt, SettingCategory.Developer),
        new DropdownSetting("Use Experimental Rendering",
            "Should Experimental Full Colour Renering Be Used?", "No", YesNoOpt, SettingCategory.Developer, true),
    ];

    public SettingsApp() : base() { }
    public SettingsApp(string name, string description, (ConsoleColour[] primaryColour, ConsoleColour[] secondaryColour, ConsoleColour[] tertiaryColour) consoleSettings, (ConsoleColour[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, description, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 90) { }

    public override App[] OnRegister()
    {
        return [new SettingsApp("Settings", "For Editing And Viewing Console Settings.", (ConsoleColour.DarkBlue.ToArray(), ConsoleColour.Cyan.ToArray(), ConsoleColour.Blue.ToArray()), (BaseBorderColours.Stretch(3).SetLength(18), 5), [], 70, 40)];
    }

    public override void OnRevistoneStartup()
    {
        LoadSettings(); //load all currently saved settings
    }

    ///<summary> Main settings menu. </summary>
    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("SETTINGS", Highlight(97, ConsoleColour.DarkBlue.ToArray(), (ConsoleColour.Cyan.ToArray(), 0, 10), (ConsoleColour.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.ActiveApp.borderColourScheme.speed, true), title.Length).ToArray());

        int catIndex = 0;
        while (true)
        {

            SendConsoleMessage(new ConsoleLine($"Tip - {Manager.GetConsoleTip}", BuildArray(AppRegistry.SecondaryCol.SetLength(5), AppRegistry.PrimaryCol)));
            ShiftLine();

            catIndex = UserInput.CreateOptionMenu("--- Settings ---", ((SettingCategory[])Enum.GetValues(typeof(SettingCategory))).Select(x => new ConsoleLine($"{x} [{settings.Where(y => y.category.ToString() == x.ToString()).ToArray().Length}]", BuildArray(ConsoleColour.Cyan.ToArray(x.ToString().Length), ConsoleColour.DarkBlue.ToArray(5)))).Concat(
                [new ConsoleLine("Exit", ConsoleColour.DarkBlue)]
            ).ToArray(), cursorStartIndex: catIndex);

            ClearLines(2, true);

            if (catIndex == Enum.GetValues(typeof(SettingCategory)).Length)
            {
                ExitApp();
                return;
            }

            Setting[] selectedCat = settings.Where(x => (int)x.category == catIndex).ToArray();
            int setIndex = 0;
            while (true)
            {
                SendConsoleMessage(new ConsoleLine(CategoryDescriptions[catIndex], AppRegistry.SecondaryCol));
                ShiftLine();
                setIndex = UserInput.CreateOptionMenu($"--- Settings -> {(SettingCategory)catIndex} ---", selectedCat.Select(x => new ConsoleLine($"{x.settingName} - {(x.currentValue.Length <= 25 ? x.currentValue : $"{x.currentValue[..25].TrimEnd()}...")}", BuildArray(AppRegistry.SecondaryCol.SetLength(x.settingName.Length + 2), AppRegistry.PrimaryCol.SetLength(30)))).Concat(
                    [new ConsoleLine("Exit", ConsoleColour.DarkBlue)]
                ).ToArray(), cursorStartIndex: setIndex);
                ClearLines(2, true);

                if (setIndex == selectedCat.Length) break;
                SettingSetMenu(selectedCat[setIndex]);
            }
        }
    }

    /// <summary> Allows the user to modify a given setting </summary>
    static void SettingSetMenu(Setting setting, bool clearConsole = true)
    {
        int userOption = 0;
        int settingIndex = GetSettingIndex(setting.settingName);

        while (true)
        {
            ConsoleColour[] inputColur = [new ConsoleColour((ConsoleColor)Enum.Parse(typeof(ConsoleColor), GetValue("Input Text Colour").Replace(" ", "")))];
            int additionalShift = 0;

            SendConsoleMessage(new ConsoleLine($"--- {setting.settingName} - {setting.category} ---", ConsoleColour.DarkBlue));
            SendConsoleMessage(new ConsoleLine($"{setting.settingName} - '{setting.currentValue}'", BuildArray(ConsoleColour.Cyan.SetLength(setting.settingName.Length + 3), inputColur)));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Summary - {setting.description}", BuildArray(ConsoleColour.Cyan.SetLength(9), ConsoleColour.DarkBlue.ToArray())));
            SendConsoleMessage(new ConsoleLine($"Default Value - '{setting.defaultValue}'", BuildArray(ConsoleColour.Cyan.SetLength(15), ConsoleColour.DarkBlue.ToArray())));
            ShiftLine();
            if (setting.requiresRestart || (setting.category == SettingCategory.Developer && setting.settingName != "Developer Mode"))
            {
                if (setting.category == SettingCategory.Developer && setting.settingName != "Developer Mode")
                {
                    SendConsoleMessage(new ConsoleLine("This Setting Only Applies In Developer Mode.", ConsoleColour.DarkYellow));
                    additionalShift++;
                }
                if (setting.requiresRestart)
                {
                    SendConsoleMessage(new ConsoleLine("This Setting Requires A Restart To Take Effect.", ConsoleColour.DarkYellow));
                    additionalShift++;
                }
                ShiftLine();
                additionalShift++;
            }

            userOption = UserInput.CreateOptionMenu("--- Options ---", [
            new ConsoleLine("Edit Value", ConsoleColour.Cyan), new ConsoleLine("Reset Value", ConsoleColour.Cyan), new ConsoleLine("Exit", ConsoleColour.DarkBlue)],
            cursorStartIndex: userOption);

            if (clearConsole) ClearPrimaryConsole();
            else
            {
                ClearLines(6 + additionalShift);
                ShiftLine(-6 - additionalShift);
            }


            switch (userOption)
            {
                case 0:
                    if (setting is InputSetting inputSetting)
                    {
                        settings[settingIndex].currentValue = UserInput.GetValidUserInput($"Update [{inputSetting.settingName}]", inputSetting.inputProfile, setting.currentValue, maxLineCount: 5);
                    }
                    else if (setting is DropdownSetting dropdownSetting)
                    {
                        int selectedIndex = Array.IndexOf(dropdownSetting.options, setting.currentValue);

                        if (dropdownSetting.options.Length >= 6)
                        {
                            int selectedOption = UserInput.CreateMultiPageOptionMenu($"Update [{dropdownSetting.settingName}]", [.. dropdownSetting.options.Select((x, i) => new ConsoleLine(x, i == selectedIndex ? ConsoleColour.DarkYellow : ConsoleColour.Cyan))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 6, selectedIndex);
                            if (selectedOption == -1) break; // user exited
                            settings[settingIndex].currentValue = dropdownSetting.options[selectedOption];
                        }
                        else
                        {
                            int selectedOption = UserInput.CreateOptionMenu($"--- Update [{dropdownSetting.settingName}] ---",
                            dropdownSetting.options.Select((x, i) => new ConsoleLine(x, i == selectedIndex ? ConsoleColour.DarkYellow : ConsoleColour.Cyan)).Concat([new ConsoleLine("Exit", ConsoleColour.DarkBlue)]).ToArray(), cursorStartIndex: selectedIndex);
                            if (selectedOption == dropdownSetting.options.Length) break; // user exited
                            settings[settingIndex].currentValue = dropdownSetting.options[selectedOption];
                        }
                    }

                    SaveSettings();
                    Analytics.General.SettingsChanged++;
                    OnSettingChanged.Invoke(setting.settingName);
                    break;
                case 1:
                    settings[settingIndex].currentValue = settings[settingIndex].defaultValue;
                    SaveSettings();
                    Analytics.General.SettingsChanged++;
                    OnSettingChanged.Invoke(setting.settingName);
                    break;
                case 2:
                    return;
            }
        }
    }

    /// <summary> Allows the user to modify a given setting </summary>
    public static void SettingSetMenu(string setting)
    {
        SettingSetMenu(settings[GetSettingIndex(setting)], false);
        SettingGetMenu(setting);
    }

    /// <summary> Allows the user to view a given setting </summary>
    public static void SettingGetMenu(string setting)
    {
        ConsoleColour[] inputColur = [new ConsoleColour((ConsoleColor)Enum.Parse(typeof(ConsoleColor), GetValue("Input Text Colour").Replace(" ", "")))];

        Setting s = settings[GetSettingIndex(setting)];
        SendConsoleMessage(new ConsoleLine($"--- {s.settingName} - {s.category} ---", ConsoleColour.DarkBlue));
        SendConsoleMessage(new ConsoleLine($"{s.settingName} - '{s.currentValue}'", BuildArray(ConsoleColour.Cyan.ToArray(s.settingName.Length + 3), inputColur)));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"Summary - {s.description}", BuildArray(ConsoleColour.Cyan.SetLength(9), ConsoleColour.DarkBlue.ToArray())));
        SendConsoleMessage(new ConsoleLine($"Default Value - '{s.defaultValue}'", BuildArray(ConsoleColour.Cyan.ToArray(15), ConsoleColour.DarkBlue.ToArray())));
        if (s.requiresRestart || (s.category == SettingCategory.Developer && s.settingName != "Developer Mode"))
        {
            ShiftLine();
            if (s.category == SettingCategory.Developer && s.settingName != "Developer Mode") SendConsoleMessage(new ConsoleLine("This Setting Only Applies In Developer Mode.", ConsoleColour.DarkYellow));
            if (s.requiresRestart) SendConsoleMessage(new ConsoleLine("This Setting Requires A Restart To Take Effect.", ConsoleColour.DarkYellow));
        }
    }

    // --- SAVE AND LOAD ---

    ///<summary> Loads all settings from settings file. </summary>
    void LoadSettings()
    {
        string[][] settingsTxt = LoadFile(GeneratePath(DataLocation.Console, "Settings", "Settings.txt"), 2, true);

        for (int i = 0; i < settingsTxt.Length; i++)
        {
            int s = GetSettingIndex(settingsTxt[i][0]);
            if (s == -1) continue;

            if (settings[s] is DropdownSetting ds && !ds.options.Contains(settingsTxt[i][1])) // prevent crashes on version update
                settings[s].currentValue = ds.defaultValue;
            else
                settings[s].currentValue = settingsTxt[i][1];
        }

        SaveSettings(); // force update broken settings prevent silly people crashing project

        UpdateSettingsDict();
    }

    ///<summary> Save settings to setting save file. </summary>
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

    ///<summary> Update the dictionary of setting values. </summary>
    static void UpdateSettingsDict()
    {
        currentSettings = [];

        foreach (Setting setting in settings)
        {
            currentSettings.Add(setting.settingName.ToLower(), setting.currentValue);
        }
    }

    ///<summary> Gets the index of a setting info. </summary>
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

    ///<summary> Returns if a setting with name settingName exists. </summary>
    public static bool SettingExists(string settingName)
    {
        return currentSettings.ContainsKey(settingName.ToLower());
    }

    ///<summary> Get the value of a setting with the given name. </summary>
    public static string GetValue(string settingName)
    {
        settingName = settingName.ToLower();

        if (!currentSettings.ContainsKey(settingName)) return "";

        Setting setting = settings[GetSettingIndex(settingName)];

        if (currentSettings["force default settings"] == "Yes" && currentSettings["developer mode"] == "Yes" && !settingName.Equals("api key", StringComparison.CurrentCultureIgnoreCase) && setting.category != SettingCategory.Developer)
            return setting.defaultValue;

        if (settingName != "developer mode" && setting.category == SettingCategory.Developer && !(currentSettings["developer mode"] == "Yes"))
            return setting.defaultValue;

        return currentSettings[settingName];
    }

    ///<summary> Get the value of a setting with a given name. </summary>
    public static ConsoleColour[] GetValueAsConsoleColour(string settingName)
    {
        return [new ConsoleColour((ConsoleColor)Enum.Parse(typeof(ConsoleColor), GetValue(settingName).Replace(" ", "")))];
    }

    ///<summary> Get the value of a setting with a given name. </summary>
    public static bool GetValueAsBool(string settingName, string trueValue = "Yes")
    {
        return GetValue(settingName) == trueValue;
    }

    // --- SETTING CLASS ---

    public abstract class Setting
    {
        public enum SettingCategory { Behaviour, Input, Appearance, ChatGPT, Performance, Developer }
        public SettingCategory category;

        public string settingName = "";
        public string description = "Placeholder Description";

        public string currentValue = "";
        public string defaultValue = "";

        public bool requiresRestart;

        public Setting(string settingName, string description, string defaultValue, SettingCategory category, bool requiresRestart = false)
        {
            this.settingName = settingName;
            this.description = description;
            this.currentValue = defaultValue;
            this.defaultValue = defaultValue;
            this.category = category;
            this.requiresRestart = requiresRestart;
        }
    }

    public class InputSetting : Setting
    {
        public UserInputProfile inputProfile;

        public InputSetting(string settingName, string description, string defaultValue, UserInputProfile inputProfile, SettingCategory category, bool requiresRestart = false) : base(settingName, description, defaultValue, category, requiresRestart)
        {
            this.inputProfile = inputProfile;
        }
    }

    public class DropdownSetting : Setting
    {
        public string[] options;

        public DropdownSetting(string settingName, string description, string defaultValue, string[] options, SettingCategory category, bool requiresRestart = false) : base(settingName, description, defaultValue, category, requiresRestart)
        {
            this.options = options;
        }
    }
}