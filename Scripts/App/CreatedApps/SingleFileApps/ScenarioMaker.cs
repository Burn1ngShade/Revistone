using OpenAI.Chat;
using Revistone.App.Command;
using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Modules;
using Revistone.Console.Image;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Modules.GPTClient;
using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Console.ConsoleAction;

namespace Revistone.App.BaseApps;

public class ScenarioMakerApp : App
{
    public ScenarioMakerApp() : base() { }
    public ScenarioMakerApp(string name, string description, (ConsoleColour[] primaryColour, ConsoleColour[] secondaryColour, ConsoleColour[] tertiaryColour) consoleSettings, (ConsoleColour[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = false) : base(name, description, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 30) { }

    public override App[] OnRegister()
    {
        return [new ScenarioMakerApp("Scenario Maker", "Create Worlds To Experience Through GPT.", (ConsoleColour.DarkBlue.ToArray(), ConsoleColour.Cyan.ToArray(), ConsoleColour.Blue.ToArray()), (BaseBorderColours.Stretch(3).SetLength(18), 5),
        [
            new AppCommand(new UserInputProfile(["quit", "exit", "quitscenario", "exitscenario"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => QuitScenario(), "Quit Scenario", "Quit The Current Scenario."),
            new AppCommand(
                new UserInputProfile(["reset", "resetscenario", "reload", "reloadscenario"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true), (s) => ResetScenario(),
                "Reset Scenario", "Reset The Current Scenario."),
            new AppCommand(new UserInputProfile(["last", "previous"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => PreviousMessage(), "Last", "Shows The Previous Message In A Scenario"),
        ],
        70, 40)];
    }

    static ConsoleLine[] title = TitleFunctions.CreateTitle("SCENARIO MAKER", Highlight(140, ConsoleColour.DarkBlue.ToArray(), (ConsoleColour.Cyan.ToArray(), 0, 10), (ConsoleColour.Cyan.ToArray(), 60, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, topSpace: 1, bottomSpace: 1);

    static readonly List<Scenario> scenarios = [];

    static Scenario? currentScenario = null;
    static int scenarioState = 0;

    // --- MENUS ---

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        Scenario.LoadScenarios();

        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.ActiveApp.borderColourScheme.speed, true), title.Length).ToArray());

        while (true)
        {
            int i = UserInput.CreateOptionMenu("--- Options ---",
            [
                (new ConsoleLine("Use Scenario", AppRegistry.SecondaryCol), UseScenario),
                (new ConsoleLine("Edit Scenario", AppRegistry.SecondaryCol), EditScenarios),
                (new ConsoleLine("Exit", AppRegistry.PrimaryCol), () => { })
            ], cursorStartIndex: 0);

            if (i == 2)
            {
                ExitApp();
                return;
            }
        }
    }

    ///<summary> Menu For Using Scenario. </summary>
    public static void UseScenario()
    {
        int i = UserInput.CreateMultiPageOptionMenu("Scenarios", [.. scenarios.Select(s => new ConsoleLine(s.Name, AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8);

        if (i != -1)
        {
            RunScenario(scenarios[i]);
            SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.ActiveApp.borderColourScheme.speed, true), title.Length).ToArray());
        }
    }

    ///<summary> Menu For Editing Scenarios. </summary>
    public static void EditScenarios()
    {
        int index = 0;
        while (true)
        {
            index = UserInput.CreateMultiPageOptionMenu("Scenarios", [new ConsoleLine("New Scenario", AppRegistry.PrimaryCol), .. scenarios.Select(s => new ConsoleLine(s.Name, AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8, index);

            if (index == 0) CreateScenario();
            else if (index == -1) break;
            else EditScenario(scenarios[index - 1]);
        }


    }

    ///<summary> Menu For Editing Specific Scenario </summary>
    public static void EditScenario(Scenario scenario)
    {
        int option = 0;
        while (true)
        {
            option = UserInput.CreateOptionMenu($"--- {scenario.Name} ---",
            [
                new ConsoleLine("Base Premise", AppRegistry.SecondaryCol),
                new ConsoleLine("General Story", AppRegistry.SecondaryCol),
                new ConsoleLine("World Building", AppRegistry.SecondaryCol),
                new ConsoleLine("Writing Style", AppRegistry.SecondaryCol),
                new ConsoleLine("Scenario Intro", AppRegistry.SecondaryCol),
                new ConsoleLine("Characters", AppRegistry.SecondaryCol),
                new ConsoleLine("Delete Scenario", ConsoleColour.DarkRed),
                new ConsoleLine("Exit", AppRegistry.PrimaryCol)
            ],
            cursorStartIndex: option);

            switch (option)
            {
                case 0:
                    scenario.BasePremise = UserInput.GetUserInput("Enter The Base Premise For The Scenario:", clear: true, maxLineCount: 10, prefilledText: scenario.BasePremise);
                    break;
                case 1:
                    scenario.GeneralStory = UserInput.GetUserInput("Enter The General Story For The Scenario:", clear: true, maxLineCount: 10, prefilledText: scenario.GeneralStory);
                    break;
                case 2:
                    scenario.WorldBuilding = UserInput.GetUserInput("Enter The World Building For The Scenario:", clear: true, maxLineCount: 10, prefilledText: scenario.WorldBuilding);
                    break;
                case 3:
                    scenario.WritingStyle = UserInput.GetUserInput("Enter The Writing Style For The Scenario:", clear: true, maxLineCount: 10, prefilledText: scenario.WritingStyle);
                    break;
                case 4:
                    scenario.Intro = UserInput.GetUserInput("Enter The Intro Message For The Scenario:", clear: true, maxLineCount: 10, prefilledText: scenario.Intro);
                    break;
                case 5:
                    EditCharacters(scenario);
                    Scenario.SaveScenario(scenario, false);
                    break;
                case 6:
                    if (UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Are You Sure You Want To [PERMANENTLY] Delete - {scenario.Name}", BuildArray(AppRegistry.PrimaryCol.SetLength(25), ConsoleColour.DarkRed.SetLength(13), AppRegistry.PrimaryCol.SetLength(10), AppRegistry.SecondaryCol)), cursorStartIndex: 1))
                    {
                        Scenario.DeleteScenario(scenario);
                        return;
                    }
                    break;

                default:
                    Scenario.SaveScenario(scenario, false);
                    return;
            }
        }
    }

    ///<summary> Edit the charchters in a scenario. </summary>
    public static void EditCharacters(Scenario scenario)
    {
        int index = 0;
        while (true)
        {
            index = UserInput.CreateMultiPageOptionMenu("Characters", [new ConsoleLine("New Character", AppRegistry.PrimaryCol), .. scenario.Charchters.Select(s => new ConsoleLine(s.Name, AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8, index);

            switch (index)
            {
                case 0:
                    scenario.Charchters.Add(CreateCharacter());
                    break;
                case -1:
                    return;
                default:
                    scenario.Charchters[index - 1] = EditCharacter(scenario.Charchters[index - 1]);
                    break;
            }


        }
    }

    public static ScenarioCharacter EditCharacter(ScenarioCharacter character)
    {
        bool exit = false;
        int index = 0;
        while (true)
        {
            SendConsoleMessage(new ConsoleLine($"--- {character.Name} ---", AppRegistry.PrimaryCol));
            SendConsoleMessage(new ConsoleLine($"Is User - {character.IsUser}", BuildArray(AppRegistry.PrimaryCol.SetLength(10), AppRegistry.SecondaryCol)));
            ShiftLine();
            index = UserInput.CreateOptionMenu("--- Options ---", [
            (new ConsoleLine("Name", AppRegistry.SecondaryCol), () => {
                character.Name = UserInput.GetUserInput("Charachter's Name:", clear: true, maxLineCount: 1, prefilledText: character.Name);
            }),
            (new ConsoleLine("Relationship", AppRegistry.SecondaryCol), () => {
                character.Relationship = UserInput.GetUserInput("Describe The Charachter's Relationship To The User's Charachter: ", clear: true, maxLineCount: 3, prefilledText: character.Relationship); }),
            (new ConsoleLine("Description", AppRegistry.SecondaryCol), () => {
                character.Description = UserInput.GetUserInput("Describe The Charachter's Physical Attributes: ", clear: true, maxLineCount: 3, prefilledText: character.Description); }),
            (new ConsoleLine("Personality", AppRegistry.SecondaryCol), () => {
                character.Personality = UserInput.GetUserInput("Describe The Charachter's Personality: ", clear: true, maxLineCount: 3, prefilledText: character.Personality); }),
            (new ConsoleLine("Exit", AppRegistry.PrimaryCol), () => { exit = true; })],
            cursorStartIndex: index);

            ClearLines(3, true);

            if (exit)
            {
                break;
            }
        }

        return character;
    }

    public static ScenarioCharacter CreateCharacter()
    {
        string name = UserInput.GetUserInput("Enter The Charachter's Name: ", clear: true, maxLineCount: 1);
        string relationship = UserInput.GetUserInput("Describe The Charachter's Relationship To The User's Charachter: ", clear: true, maxLineCount: 3);
        string description = UserInput.GetUserInput("Describe The Charachter's Physical Attributes: ", clear: true, maxLineCount: 5);
        string personality = UserInput.GetUserInput("Describe The Charachter's Personality:", clear: true, maxLineCount: 5);

        return new ScenarioCharacter { Name = name, Relationship = relationship, Description = description, Personality = personality, IsUser = false };
    }

    ///<summary> Menu for creating scenario. </summary>
    public static void CreateScenario()
    {
        string scenarioName = UserInput.GetValidUserInput("Enter Your Scenarios Name:", new UserInputProfile(UserInputProfile.InputType.FullText, customRequirement: (s => { return IsNameValid(s) && !scenarios.Select(x => x.Name.ToLower()).Contains(s.ToLower()); }, "Scenario Name Must Be A Valid Unique File Name.")));

        string basePremise = UserInput.GetUserInput("Enter The Base Premise For The Scenario:", clear: true, maxLineCount: 10);
        string generalStory = UserInput.GetUserInput("Enter The General Story For The Scenario:", clear: true, maxLineCount: 10);
        string worldBuilding = UserInput.GetUserInput("Enter The World Building For The Scenario:", clear: true, maxLineCount: 10);
        string writingStyle = UserInput.GetUserInput("Enter The Writing Style For The Scenario:", clear: true, maxLineCount: 10);

        string scenarioIntro = UserInput.GetUserInput("Enter The Inital Message For The Scenario:", clear: true, maxLineCount: 10);

        Scenario s = new(scenarioName, basePremise, generalStory, worldBuilding, writingStyle, scenarioIntro);

        Scenario.SaveScenario(s, true);

        EditScenario(s);
    }

    // --- RUN SCENARIO ---

    ///<summary> Runs the given scenario, allowing the user to interact with it. </summary>
    public static void RunScenario(Scenario scenario)
    {
        currentScenario = scenario;
        scenarioState = 1;

        scenario.UpdateScenarioSystemMessage();

        ClearPrimaryConsole();

        if (scenario.gpt.MessageHistoryCount == 0)
        {
            SendConsoleMessage(new ConsoleLine($"Day: {scenario.Day}, Time {scenario.TimeInHourMinute}", AppRegistry.PrimaryCol));
            SendConsoleMessages(ToGPTFormat([.. StringFunctions.FitToConsole(scenario.Intro)]));
            scenario.gpt.AddToMessageHistory(new AssistantChatMessage(scenario.Intro));
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"Day: {scenario.Day}, Time {scenario.TimeInHourMinute}", AppRegistry.PrimaryCol));
            SendConsoleMessages(ToGPTFormat([.. StringFunctions.FitToConsole(scenario.gpt.GetLastMessages(1)[0])]));
        }

        while (true)
        {
            string input = UserInput.GetUserInput(maxLineCount: 10, clear: false, inputPrefix: "You: ");

            if (AppCommandRegistry.Commands(input))
            {
                if (scenarioState == 2)
                {
                    currentScenario = null;
                    scenarioState = 0;
                    return;
                }
                else if (scenarioState == 3)
                {
                    RunScenario(scenario);
                    return;
                }
            }
            else
            {
                scenario.Time += 10;
                scenario.Counter++;

                ShiftLine();
                SendConsoleMessage(new ConsoleLine($"Day: {scenario.Day}, Time {scenario.TimeInHourMinute}", AppRegistry.PrimaryCol));
                scenario.gpt.Query(new GPTQuery($"[Day {scenario.Day}, Time: {scenario.TimeInHourMinute}]\n{input}", QueryMode.User, useSystemPromt: false, minimalUI: true, baseToolCalls: false));

                if (scenario.Time > 18 * 60)
                    scenario.gpt.Query(new GPTQuery($"[SYSTEM] Verify If The Next Day Should Begin, And IF SO Start The Next Day, e.g. User Has Gone To Sleep: {scenario.gpt.GetLastMessages(2).ToElementString()}", QueryMode.System, false, false, false, false, false, false, true, "Updating Scenario", [startNextDay], HandleScenarioToolCalls, false));

                if (scenario.Counter % 15 == 0)
                    scenario.gpt.Query(new GPTQuery($"[SYSTEM] Store A Summary Of The Given Messages As A SINGLE Memory:{string.Join('\n', scenario.gpt.GetLastMessages(30))}", QueryMode.System, false, false, false, false, false, false, true, "Summarising Conversation"));
            }
        }
    }

    ///<summary> Quit active scenario. </summary>
    static void QuitScenario()
    {
        if (UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Are You Sure You Want To Quit Scenario - {currentScenario?.Name}", BuildArray(AppRegistry.PrimaryCol.SetLength(41), AppRegistry.SecondaryCol))))
        {
            ClearPrimaryConsole();
            scenarioState = 2;
            if (currentScenario != null) Scenario.SaveScenario(currentScenario, false);
        }
    }

    ///<summary> Reset active scenario. </summary>
    static void ResetScenario()
    {
        if (UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Are You Sure You Want To Reset Scenario - {currentScenario?.Name}", BuildArray(AppRegistry.PrimaryCol.SetLength(41), AppRegistry.SecondaryCol))))
        {
            ClearPrimaryConsole();
            scenarioState = 3;
            currentScenario?.gpt.ClearMessageHistory(false);
            currentScenario?.gpt.ClearMemories(false);
            if (currentScenario != null)
            {
                currentScenario.Counter = 0;
                currentScenario.Day = 0;
                currentScenario.Time = 7 * 60;
                Scenario.SaveScenario(currentScenario, false);
            }
        }
    }

    ///<summary> Shows the previous message in a scenario. </summary>
    static void PreviousMessage()
    {
        if (currentScenario == null) return;

        SendConsoleMessages([.. StringFunctions.FitToConsole(currentScenario.gpt.GetLastMessages(1)[0]).Select(x => new ConsoleLine(x, AppRegistry.PrimaryCol))]);
    }

    public override void OnRevistoneClose()
    {
        base.OnRevistoneClose();
        if (currentScenario != null) Scenario.SaveScenario(currentScenario, false);
    }

    /// --- SCENARIO ---

    /// <summary> Class representing a scenario, containing all the information needed to run it. </summary>
    public class Scenario
    {
        static readonly string scenarioDataPath = GeneratePath(DataLocation.App, "ScenarioMaker/");

        static string BaseScenarioInfo = "Objective: Your Goal Is To Run A Fully Immersive Scenario For The User To Interact With, You Will Play The Role Of All Charchters Within The Scenario EXCEPT For The User, You Will Respond To The Users Input As If You Were The Charchter They Are Interacting With, As Well As Describe The Scenario And It's World.\nRules: You Must Not Break Character, You Must Not Respond To The User As If You Are The AI, If Replying As A Charchter Your Reply MUST Start With **[Charchter Name:]** Followed By The Charchters Response. Do Not Entertain Off Topic Questions. \nKeep responses a reasonable length, No long spanning pargrahps. NEVER Speak For The User. You MUST keep scenario in line with current day and time.";

        public string Name { get; set; } = "Default";
        public string Intro { get; set; } = "This Is The First Message Of The Scenario.";
        public string BasePremise { get; set; } = "This Is The Basic Premise Of The Scenario, It Will Be Used To Generate The Scenario.";
        public string GeneralStory { get; set; } = "This Is The General Story Of The Scenario, It Will Be Used To Generate The Scenario.";
        public string WorldBuilding { get; set; } = "This Is The World Building Of The Scenario, It Will Be Used To Generate The Scenario.";
        public string WritingStyle { get; set; } = "This Is The Writing Style Of The Scenario, It Will Be Used To Generate The Scenario.";

        public bool TrackTime;

        public List<ScenarioCharacter> Charchters { get; set; } = [];

        public readonly GPTClient gpt = new();

        // in scenario trackers

        public int Counter { get; set; } = 0;
        public int Day { get; set; } = 0;
        public int Time { get; set; } = 7 * 60;
        public string TimeInHourMinute => $"{Time / 60:D2}:{Time % 60:D2}";


        public Scenario(string name, string basePremise, string generalStory, string worldBuilding, string writingStyle, string intro)
        {
            Name = name;
            Intro = intro;
            BasePremise = basePremise;
            GeneralStory = generalStory;
            WorldBuilding = worldBuilding;
            WritingStyle = writingStyle;

            Charchters = [new ScenarioCharacter { IsUser = true, Name = SettingsApp.GetValue("Username") }];

            string msgPath = scenarioDataPath + "Messages/" + name + ".json";
            string memPath = scenarioDataPath + "Memories/" + name + ".json";

            List<SerializableChatMessage>? serializableChats = LoadFileFromJSON<List<SerializableChatMessage>>(msgPath);
            gpt = new GPTClient(serializableChats == null ? [] : [.. serializableChats.Select(x => x.ToChatMessage())], msgPath,
                LoadFileFromJSON<List<ChatMemory>>(memPath) ?? [], memPath);

            UpdateScenarioSystemMessage();
        }

        ///<summary> Generates scenario gpt with up to date info</summary>
        public void UpdateScenarioSystemMessage()
        {
            string charchtersStr = "Charachters:";

            foreach (ScenarioCharacter c in Charchters)
            {
                charchtersStr += $"\nName: {c.Name}, IsUser: {c.IsUser}, Relationship To User: {c.Relationship}";
                if (c.Description.Length != 0) charchtersStr += $"\nDescription: {c.Description}";
                if (c.Personality.Length != 0) charchtersStr += $"\nPersonality: {c.Personality}";
            }

            string scenarioInfo = BaseScenarioInfo + "\n" +
                "Base Premise: " + BasePremise + "\n" +
                "General Story: " + GeneralStory + "\n" +
                "World Building: " + WorldBuilding + "\n" +
                "Writing Style: " + WritingStyle + "\n" +
                charchtersStr;

            gpt.additionalSystemPromt = scenarioInfo;
        }

        ///<summary> Loads all scenarios from file. </summary>
        public static void LoadScenarios()
        {
            scenarios.Clear();
            string[] scenarioFiles = GetSubFiles(scenarioDataPath + "Scenarios/");

            foreach (string file in scenarioFiles)
            {
                Scenario? scenario = LoadFileFromJSON<Scenario>(scenarioDataPath + "Scenarios/" + file, false);
                if (scenario != null)
                {
                    scenario.Charchters ??= [new ScenarioCharacter { IsUser = true, Name = SettingsApp.GetValue("Username") }];
                    scenarios.Add(scenario);
                }
            }
        }

        ///<summary> Save given scenario to file. </summary>
        public static void SaveScenario(Scenario scenario, bool addToScenarios = true)
        {
            SaveFileAsJSON(scenarioDataPath + "Scenarios/" + scenario.Name + ".json", scenario);
            if (addToScenarios) scenarios.Add(scenario);
        }

        ///<summary> Deletes given scenario. </summary>
        public static void DeleteScenario(Scenario scenario)
        {
            DeleteFile(scenarioDataPath + "Scenarios/" + scenario.Name + ".json");
            DeleteFile(scenarioDataPath + "Memories/" + scenario.Name + ".json");
            DeleteFile(scenarioDataPath + "Messages/" + scenario.Name + ".json");

            scenarios.Remove(scenario);
        }
    }

    public class ScenarioCharacter
    {
        public bool IsUser { get; set; } = false;

        public string Name { get; set; } = "";
        public string Relationship { get; set; } = "";

        public string Description { get; set; } = "";
        public string Personality { get; set; } = "";
    }

    // --- SCENARIO CHAT TOOLS ---

    public static readonly ChatTool startNextDay = ChatTool.CreateFunctionTool(
        functionName: nameof(StartNextDay),
        functionDescription:
        @"Starts The Next Day In The Scenario."
    );

    public static void StartNextDay()
    {
        if (currentScenario == null) return;
        currentScenario.Day++;
        currentScenario.Time = 7 * 60; 
    }

    public static string? HandleScenarioToolCalls(ChatToolCall toolCall, bool shouldOutputToolCalls)
    {
        switch (toolCall.FunctionName)
        {
            case nameof(StartNextDay):
                StartNextDay();
                return "Success";
        }

        return null;
    }
}