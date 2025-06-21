using System.Text.Json;
using OpenAI.Chat;
using Revistone.App;
using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Management;
using Revistone.Interaction;
using System.Text.RegularExpressions;
using Revistone.App.Command;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using Revistone.Functions;

namespace Revistone.Modules;

/// <summary> Class for interacting with chat GPT API within the console. </summary>
public class GPTClient
{
    // --- SINGLETON --- // default gpt for general use

    public static GPTClient Default { get; private set; } = new GPTClient();

    ///<summary> [DO NOT CALL] Initializes GPT, fetching message history. </summary>
    internal static void InitializeGPT()
    {
        string msgPath = GeneratePath(DataLocation.Console, "History/GPT", "Messages.json");
        string memPath = GeneratePath(DataLocation.Console, "History/GPT", "Memories.json");

        List<SerializableChatMessage>? serializableChats = LoadFileFromJSON<List<SerializableChatMessage>>(msgPath);
        Default = new GPTClient(serializableChats == null ? [] : [.. serializableChats.Select(x => x.ToChatMessage())], msgPath,
            LoadFileFromJSON<List<ChatMemory>>(memPath) ?? [], memPath);
    }

    // --- GPT PROPERTIES ---

    List<ChatMessage> messageHistory = [];
    readonly string messageHistoryPath = "";

    List<ChatMemory> memoryHistory = [];
    readonly string memoryHistoryPath = "";

    GPTQuery? currentQuery = null;

    public enum QueryMode
    {
        User,
        System,
    }

    ///<summary> Information behind a query to a GPTClient. </summary>
    public class GPTQuery
    {
        public string Query { get; private set; } = ""; // the query to send to gpt

        public QueryMode Mode { get; private set; } = QueryMode.User;

        public bool UseMessageHistory { get; private set; } = true; // whether to use message history
        public bool AddToMessageHistory { get; private set; } = true; // whether to add the query to message history
        public bool OutputToConsole { get; private set; } = true; // whether to output the response to console
        public bool UseLongTermMemory { get; private set; } = true; // should use memories
        public bool UseSystemPromt { get; private set; } = true; // use console info e.g. user name about console file
        public bool UseAdditonalSystemPromt { get; private set; } = true;

        public bool MinimalUI { get; private set; } = true; // hides top ui bar
        public string WaitMessage { get; private set; } = "Waiting For GPT Repsonse"; // message to display while waiting for gpt response

        public bool BaseToolCalls { get; private set; }
        public List<ChatTool> AdditionalTools { get; private set; }
        public Func<ChatToolCall, bool, string?>? AdditionalToolHandling { get; private set; }

        public GPTQuery(string query, QueryMode mode = QueryMode.User, bool useMessageHistory = true, bool addToMessageHistory = true, bool useLongTermMemory = true, bool useSystemPromt = true, bool useAdditonalSystemPromt = true, bool outputToConsole = true, bool minimalUI = false, string waitMessage = "Waiting For GPT Response", List<ChatTool>? additionalTools = null, Func<ChatToolCall, bool, string?>? additionalToolCallHandling = null, bool baseToolCalls = true)
        {
            Query = query;
            Mode = mode;
            UseMessageHistory = useMessageHistory;
            AddToMessageHistory = addToMessageHistory;
            UseLongTermMemory = useLongTermMemory;
            UseSystemPromt = useSystemPromt;
            UseAdditonalSystemPromt = useAdditonalSystemPromt;
            OutputToConsole = outputToConsole;
            MinimalUI = minimalUI;
            WaitMessage = waitMessage;
            AdditionalTools = additionalTools ?? [];
            AdditionalToolHandling = additionalToolCallHandling;
            BaseToolCalls = baseToolCalls;
        }
    }

    public string additionalSystemPromt = ""; // additional system prompt to add to the default system prompt

    public GPTClient() { }

    public GPTClient(List<ChatMessage> messageHistory, string messageHistoryPath, List<ChatMemory> memoryHistory, string memoryHistoryPath, string additionalSystemPromt = "")
    {
        this.messageHistory = messageHistory;
        this.messageHistoryPath = messageHistoryPath;
        this.memoryHistory = memoryHistory;
        this.memoryHistoryPath = memoryHistoryPath;
        this.additionalSystemPromt = additionalSystemPromt;
    }

    // --- INTERACTION FUNCTIONS ---

    ///<summary> Function to send a query to run a query with ChatGPT. </summary>
    public void Query(GPTQuery query)
    {
        currentQuery = query;
        ChatMessage[] messages = GetRelevantChatMessages();

        (ChatClient? client, ChatCompletionOptions o) = CreateResponseModel(query);
        if (client == null) return;

        SendConsoleMessage(new ConsoleLine($"{query.WaitMessage}.", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine,
            new ConsoleAnimatedLine(WaitForGptResponseAnimation, 1, tickMod: 12, enabled: true));

        ExecuteQuery(client, messages, o, query);
        Analytics.General.TotalGPTPromts++;
    }

    // --- CHAT MEMORY ---

    ///<summary> Class to hold memory that gpt stores inbetween sessions. </summary>
    public class ChatMemory
    {
        public string Content { get; set; } = "";
        public string Creator { get; set; } = "User";
        public DateTime CreationTime { get; set; }

        public ChatMemory() { }

        public ChatMemory(string content, string creator = "User")
        {
            Content = content;
            Creator = creator;
            CreationTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Creator} - {CreationTime}] {Content}";
        }
    }

    ///<summary> Add memory to list of memories for gpt to recall </summary>
    public void AddToMemories(string content, string creator = "User")
    {
        memoryHistory.Add(new ChatMemory(content, creator));
        SaveFileAsJSON(memoryHistoryPath, memoryHistory);
        SendConsoleMessage(new ConsoleLine("GPT Has Stored A Memory.", AppRegistry.PrimaryCol));
    }

    ///<summary> Creates menu for user to view, edit, and remove memories. </summary>
    public void ViewMemories()
    {
        int option = 0;
        while (true)
        {
            option = UserInput.CreateMultiPageOptionMenu("GPT Memories", [.. memoryHistory.Select(x => new ConsoleLine(x.ToString(),
            BuildArray(AppRegistry.SecondaryCol.Extend(x.ToString().IndexOf(']') + 1), AppRegistry.PrimaryCol.Extend(x.ToString().Length))))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 5, option);

            if (option == -1) break;
            ViewMemory(option);
        }
    }

    ///<summary> Creates menu for user to view, edit, and remove specific memory. </summary>
    void ViewMemory(int index)
    {
        ChatMemory m = memoryHistory[index];

        int option = 0;
        while (option != 2)
        {
            SendConsoleMessage(new ConsoleLine("--- GPT Memories ---", AppRegistry.PrimaryCol));
            SendConsoleMessage(new ConsoleLine($"Content - '{m.Content}'", BuildArray(AppRegistry.SecondaryCol.Extend(9), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Creator - '{m.Creator}'", BuildArray(AppRegistry.SecondaryCol.Extend(9), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Creation Time - {m.CreationTime}", BuildArray(AppRegistry.SecondaryCol.Extend(15), AppRegistry.PrimaryCol)));
            ShiftLine();

            option = UserInput.CreateOptionMenu("--- Options ---", ["Edit Memory", "Delete Memory", "Exit"], cursorStartIndex: option);
            ClearLines(5, true);
            if (option == 0)
            {
                m.Content = UserInput.GetValidUserInput($"--- Update Memory ---", new UserInputProfile(), m.Content, maxLineCount: 5);
                memoryHistory[index].Content = m.Content;
                SaveFileAsJSON(GeneratePath(DataLocation.Console, "History/GPT", "Memories.json"), memoryHistory);
            }
            else if (option == 1)
            {
                if (UserInput.CreateTrueFalseOptionMenu("Are You Sure You Want To Delete This Memory?"))
                {
                    memoryHistory.RemoveAt(index);
                    SaveFileAsJSON(GeneratePath(DataLocation.Console, "History/GPT", "Memories.json"), memoryHistory);
                    SendConsoleMessage(new ConsoleLine("Memory Deleted.", AppRegistry.PrimaryCol));
                    UserInput.WaitForUserInput(space: true);
                    ClearLines(2, true);
                    return;
                }
            }
        }

        memoryHistory[index] = m;
    }

    ///<summary> Clears the memories of the chatbot. </summary>
    public void ClearMemories(bool output = true)
    {
        memoryHistory = [];
        SaveFileAsJSON(memoryHistoryPath, memoryHistory);
        if (output) SendConsoleMessage(new ConsoleLine("ChatGPT Memories Cleared.", ConsoleColor.DarkBlue));
    }

    // --- CHAT MESSAGE ---

    ///<summary> JSON friendly format for chat messages to be saved inbetween sessions. </summary>
    public class SerializableChatMessage
    {
        public enum MessageType { User, Assistant }

        public MessageType Type { get; set; }
        public string Content { get; set; } = "";

        public SerializableChatMessage() { }

        public SerializableChatMessage(ChatMessage message)
        {
            Type = message as UserChatMessage != null ? MessageType.User : MessageType.Assistant;
            Content = message.Content[0].Text;
        }

        public ChatMessage ToChatMessage()
        {
            if (Type == MessageType.User) return new UserChatMessage(Content);
            else return new AssistantChatMessage(Content);
        }
    }

    ///<summary> Gathers all context messages needed to generate a response. </summary>
    ChatMessage[] GetRelevantChatMessages()
    {
        ChatMessage[] messages = [CreateQuerySystemChatMessage()];

        if (currentQuery?.UseMessageHistory ?? true) messages =
        [
            .. messages,
            .. messageHistory.TakeLast(Math.Min(messageHistory.Count, int.Parse(SettingsApp.GetValue("Conversation Memory")) + 1)),
        ];

        messages = [.. messages, currentQuery?.Mode == QueryMode.System ? new SystemChatMessage(currentQuery?.Query) : new UserChatMessage(currentQuery?.Query)];

        if (SettingsApp.GetValue("Log GPT Messages") == "Full Context Message") DeveloperTools.Log($"[GPT] Full Context Message: {messages.Select(x => x.Content[0].Text).ToArray().ToElementString()}");

        if (currentQuery?.AddToMessageHistory ?? true) AddToMessageHistory(new UserChatMessage(currentQuery?.Query));

        return messages;
    }

    ///<summary> Retuns last messages within the conversation. </summary>
    public string[] GetLastMessages(int count)
    {
        if (count <= 0) return [];
        if (count > messageHistory.Count) count = messageHistory.Count;

        return [.. messageHistory.TakeLast(count).Select(x => x.Content[0].Text)];
    }

    ///<summary> View last message in file format. </summary>
    public bool ViewLastMessages(int messageCount = 1)
    {
        if (messageHistory.Count == 0)
        {
            SendConsoleMessage(new ConsoleLine("GPT Has No Previous Messages.", AppRegistry.PrimaryCol));
            return false;
        }

        UserInput.GetMultiUserInput("Last GPT Message",
            [.. GetLastMessages(messageCount)
                .SelectMany((msg, index) =>
                new[] { $"--- Message {index + 1} ---" } 
                .Concat(msg
                .Split(["\r\n", "\n"], StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line)).Reverse()
                .SelectMany(line => line.FitToConsole()))
                .Concat([""]) // Blank line after each message
                ).ToArray()
            ],
            readOnly: true);
        return true;
    }

    ///<summary> Add message to previous message history, caps size to 50 to keep data storage reasonable. </summary>
    public void AddToMessageHistory(ChatMessage message)
    {
        while (messageHistory.Count >= 50) messageHistory.RemoveAt(0);

        messageHistory.Add(message);

        SaveFileAsJSON(messageHistoryPath, messageHistory.Select(x => new SerializableChatMessage(x)).ToList());
    }

    ///<summary> Clears the message history of the chatbot. </summary>
    public void ClearMessageHistory(bool output = true)
    {
        messageHistory = [];
        SaveFileAsJSON(messageHistoryPath, messageHistory.Select(x => new SerializableChatMessage(x)).ToList());
        if (output) SendConsoleMessage(new ConsoleLine("ChatGPT Message History Cleared.", ConsoleColor.DarkBlue));
    }

    public int MessageHistoryCount => messageHistory.Count;

    // --- OUTPUT FUNCTIONS ---

    ///<summary> Outputs a GPT response in a console friendly format. </summary>
    void SendFormattedGPTResponse(ChatCompletion response, int messageCount)
    {
        UpdatePrimaryConsoleLineAnimation(ConsoleAnimatedLine.None, ConsoleData.primaryLineIndex);

        List<string> lines = [.. StringFunctions.FitToConsole(response.Content[0].Text)];

        ConsoleLine[] colouredResponse = ToGPTFormat(lines);

        if (!currentQuery?.MinimalUI ?? true) SendConsoleMessage(new ConsoleLine($"[{SettingsApp.GetValue("GPT Name")}] ({SettingsApp.GetValue("GPT Model")}) - {response.Usage.TotalTokenCount} Tokens Used ({response.Usage.InputTokenCount} Input, {response.Usage.OutputTokenCount} Output). {messageCount} Messages Used.", ConsoleColor.Cyan));
        SendConsoleMessages(colouredResponse);

        if (SettingsApp.GetValue("Log GPT Messages") == "Query Info")
        {
            DeveloperTools.Log($"({SettingsApp.GetValue("GPT Model")}) - {response.Usage.TotalTokenCount} Tokens Used ({response.Usage.InputTokenCount} Input, {response.Usage.OutputTokenCount} Output). {messageCount} Messages Used.");
        }
    }

    ///<summary> Converts list of text to GPT coloured formatting. </summary>
    public static ConsoleLine[] ToGPTFormat(List<string> lines)
    {
        ConsoleLine[] consoleLines = lines.Select(x => ConsoleLine.Clean(new ConsoleLine(x))).ToArray();

        Regex boldRegex = new(@"\*\*(.*?)\*\*");

        for (int i = 0; i < consoleLines.Length; i++)
        {
            string originalLine = consoleLines[i].lineText;
            List<(int start, int length)> indices = [];
            int shift = 0;

            originalLine = boldRegex.Replace(originalLine, match =>
            {
                string highlightedText = match.Groups[1].Value;
                indices.Add((match.Index - shift, highlightedText.Length));
                shift += 4;
                return highlightedText;
            });

            consoleLines[i] = new ConsoleLine(originalLine, AdvancedHighlight(originalLine.Length, ConsoleColor.DarkBlue, ConsoleColor.Cyan, [.. indices]));
        }

        return consoleLines;
    }

    void WaitForGptResponseAnimation(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
    {
        int dotCount = (int)animationInfo.metaInfo;
        SendConsoleMessage(new ConsoleLine($"{currentQuery?.WaitMessage}{new string('.', dotCount)}", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine);
        dotCount = dotCount < 3 ? dotCount + 1 : 0;
        animationInfo.metaInfo = dotCount;
    }

    // --- GPT LOGIC ---

    ///<summary> Executes and handles chat tools for the given query. </summary>
    bool ExecuteQuery(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options, GPTQuery query)
    {
        List<ChatMessage> queryMessages = [.. messages];

        bool requiresAction;
        ChatCompletion completion;
        List<Action> delayedChatTools = [];

        do
        {
            requiresAction = false;

            try
            {
                completion = client.CompleteChat(queryMessages, options);
            }
            catch (Exception e)
            {
                ClearLines(1);
                SendConsoleMessage(ConsoleLine.Clean(new ConsoleLine($"GPT Error: {e.Message}", ConsoleColor.Red)));
                DeveloperTools.Log($"GPT Error: {e.Message}");
                return false;
            }

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                    queryMessages.Add(new AssistantChatMessage(completion));
                    break;
                case ChatFinishReason.ToolCalls:
                    queryMessages.Add(new AssistantChatMessage(completion));

                    UpdatePrimaryConsoleLineAnimation(ConsoleAnimatedLine.None, ConsoleData.primaryLineIndex);

                    bool shouldOutputToolCalls = SettingsApp.GetValue("Show GPT Tool Results") == "Yes";
                    foreach (ChatToolCall toolCall in completion.ToolCalls)
                    {
                        string toolResult = "";
                        bool success = true;
                        switch (toolCall.FunctionName)
                        {
                            case nameof(GetCurrentTime):
                                toolResult = GetCurrentTime();
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(CancelTimer):
                                CancelTimer();
                                toolResult = "Success";
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(CreateTimer):
                                toolResult = TryGetJsonFunctionArgument("duration", toolCall, ref success);

                                if (success) CreateTimer(toolResult);
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(LoadApp):
                                toolResult = TryGetJsonFunctionArgument("appName", toolCall, ref success);

                                if (success) delayedChatTools.Add(() => LoadApp(toolResult));
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(SetSetting):
                                toolResult = TryGetJsonFunctionArgument("settingName", toolCall, ref success);

                                if (success) delayedChatTools.Add(() => SetSetting(toolResult));
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(AddToMemories):
                                toolResult = TryGetJsonFunctionArgument("content", toolCall, ref success);

                                if (success) AddToMemories(toolResult, "GPT");
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                        }

                        if (toolResult == "" && query.AdditionalToolHandling != null)
                        {
                            string? result = query.AdditionalToolHandling.Invoke(toolCall, shouldOutputToolCalls);
                            if (result != null)
                            {
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult)); // so non responded tool calls get picked up
                            }
                        }
                    }

                    SendConsoleMessage(new ConsoleLine($"{currentQuery?.WaitMessage}.", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine,
                    new ConsoleAnimatedLine(WaitForGptResponseAnimation, 1, tickMod: 12, enabled: true));

                    requiresAction = true;
                    break;
            }

        } while (requiresAction);

        if (currentQuery?.AddToMessageHistory ?? true) AddToMessageHistory(new AssistantChatMessage(completion));
        if (currentQuery?.OutputToConsole ?? true) SendFormattedGPTResponse(completion, messages.Length);

        Analytics.General.UsedGPTInputTokens += completion.Usage.InputTokenCount;
        Analytics.General.UsedGPTOutputTokens += completion.Usage.OutputTokenCount;

        for (int i = 0; i < delayedChatTools.Count; i++)
        {
            delayedChatTools[i].Invoke();
        }

        return true;
    }

    ///<summary> Generates the chat client and options for the currently in use model. </summary>
    (ChatClient? client, ChatCompletionOptions options) CreateResponseModel(GPTQuery query)
    {
        string model = SettingsApp.GetValue("GPT Model");

        ChatCompletionOptions o;

        if (currentQuery?.BaseToolCalls ?? true)
        {
            o = new ChatCompletionOptions
            {
                Tools = { getCurrentTimeTool, setTimerTool, cancelTimerTool, loadAppTool, setSetting, remember }
            };
        }
        else o = new ChatCompletionOptions();

        foreach (ChatTool t in query.AdditionalTools)
        {
            o.Tools.Add(t);
        }

        if (model == "gpt-4o-mini") o.Temperature = float.Parse(SettingsApp.GetValue("Temperature"));

        return (CreateChatClient(model), o);
    }

    ///<summary> Attempts to create chat client for given query. </summary>
    static ChatClient? CreateChatClient(string model)
    {
        try
        {
            return new ChatClient(model: model, apiKey: SettingsApp.GetValue("API Key"));
        }
        catch (HttpRequestException ex)
        {
            SendConsoleMessage(new ConsoleLine($"Error: {ex.StatusCode}", ConsoleColor.Red));
        }
        catch (Exception e)
        {
            if (e.Message.StartsWith("HTTP 401") || e.Message == "Value cannot be an empty string. (Parameter 'key')")
            {
                SendConsoleMessage(new ConsoleLine("Error: Invalid Or No API Key Entered, Update Your API Key In The Settings App!", ConsoleColor.Red));
            }
            else
            {
                SendConsoleMessage(new ConsoleLine($"GPT Error: {e.Message}", ConsoleColor.Red));
            }
        }

        return null;
    }

    ///<summary> Creates the system chat message for a query, this will always be the first information shown to GPT. </summary>
    SystemChatMessage CreateQuerySystemChatMessage()
    {
        string msg = "";

        if (currentQuery?.UseSystemPromt ?? true)
        {
            msg += $"User's name: {SettingsApp.GetValue("Username")}\nUser's pronouns: {SettingsApp.GetValue("Pronouns")}\nYour Name: {SettingsApp.GetValue("GPT Name")}.\n";
            if (SettingsApp.GetValue("Behaviour").Length != 0) msg += $"Your Behaviour: {SettingsApp.GetValue("Behaviour")}\n";
            if (SettingsApp.GetValue("Scenario").Length != 0) msg += $"Current Scenario: {SettingsApp.GetValue("Scenario")}\n";
            if (SettingsApp.GetValue("Use Detailed System Promt") == "Yes") msg += string.Join('\n', LoadFile(GeneratePath(DataLocation.Console, "Assets/GPT", "AboutRevistone.txt")));
        }
        if (currentQuery?.UseLongTermMemory ?? true) msg += $"Long Term Memory: {string.Join('\n', memoryHistory.Select(x => x.ToString()))}\n";

        if (currentQuery?.UseAdditonalSystemPromt ?? true && additionalSystemPromt.Length > 0) msg += $"\n{additionalSystemPromt}";

        if (SettingsApp.GetValue("Log GPT Messages") == "System Message") DeveloperTools.Log($"[GPT] System Message: {msg}");
        return new SystemChatMessage(msg);
    }

    // --- CHAT TOOLS ---

    ///<summary> Attempts to get GPT argument for toolCall function. </summary>
    public static string TryGetJsonFunctionArgument(string argument, ChatToolCall toolCall, ref bool success)
    {
        string jsonString = toolCall.FunctionArguments.ToString();
        try
        {
            // Deserialize JSON into a dictionary
            var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (arguments != null && arguments.TryGetValue(argument, out string? argumentValue))
            {
                if (SettingsApp.GetValueAsBool("Show GPT Tool Results"))
                    SendDebugMessage(new ConsoleLine($"[{toolCall.FunctionName}] {argumentValue}", BuildArray(AppRegistry.SecondaryCol.Extend(toolCall.FunctionName.Length + 2), AppRegistry.PrimaryCol)));
                return argumentValue;
            }
            else
            {
                success = false;
                if (SettingsApp.GetValueAsBool("Show GPT Tool Results"))
                    SendDebugMessage(new ConsoleLine($"[{toolCall.FunctionName}] Error: Missing '{argument}' argument.", BuildArray(AppRegistry.SecondaryCol.Extend(toolCall.FunctionName.Length + 2), AppRegistry.PrimaryCol)));
                return $"Error: Missing '{argument}' argument.";
            }
        }
        catch (JsonException ex)
        {
            success = false;
            return $"Error: Invalid JSON format. {ex.Message}";
        }
    }

    ///<summary> Base Console Tools. </summary>

    static readonly ChatTool getCurrentTimeTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentTime),
        functionDescription: "Get the current local time."
    );

    static readonly ChatTool setTimerTool = ChatTool.CreateFunctionTool(
        functionName: nameof(CreateTimer),
        functionDescription:
        @"Creates a timer with a specified duration.
        **Argument:**
        - `duration` (string) → Time in the format `hh:mm:ss`.  
        **Examples:**  
        - duration: '00:05:30` → Creates a 5-minute 30-second timer.  
        - duration: '01:00:00` → Creates a 1-hour timer.  "
    );

    static readonly ChatTool cancelTimerTool = ChatTool.CreateFunctionTool(
        functionName: nameof(CancelTimer),
        functionDescription: "Cancels timer."
    );

    static readonly ChatTool loadAppTool = ChatTool.CreateFunctionTool(
        functionName: nameof(LoadApp),
        functionDescription: @"Loads app of specified name.
        **Argument:**
        - 'appName' (string) -> App name.
        **Examples:**
        - appName: 'revistone' -> Loads The Revistone App.
        - appName: 'FLasH CaRD manager -> Loads The Flash Card Manager App. "
    );

    static readonly ChatTool setSetting = ChatTool.CreateFunctionTool(
        functionName: nameof(SetSetting),
        functionDescription: @"Lets user modify setting of a specified name.
        **Argument:**
        - 'settingName' (string) -> Setting name.
        **Examples:** 
        - settingName: 'gPT NaMe' -> Lets the user set the 'GPT Name' setting.
        - settingName: 'userName' -> Lets the user set the 'Username' setting. "
    );

    static readonly ChatTool remember = ChatTool.CreateFunctionTool(
        functionName: nameof(AddToMemories),
        functionDescription: @"Adds memory to the list of permeant memories that GPT can recall.
        **Argument:**
        - 'content' (string) -> Memory content.
        **Examples:**
        - content: 'The user loves vanilla.' -> Adds the memory 'The user loves vanilla.'
        - content: 'The user is a software engineer.' -> Adds the memory 'The user is a software engineer.'
        "
    );

    static string GetCurrentTime()
    {
        return $"{DateTime.Now}";
    }

    static void CreateTimer(string duration)
    {
        AppCommandRegistry.Commands($"timer {duration}");
    }

    static void CancelTimer()
    {
        AppCommandRegistry.Commands("cancel timer");
    }

    static void LoadApp(string appName)
    {
        AppCommandRegistry.Commands($"load {appName}");
    }

    static void SetSetting(string settingName)
    {
        SettingsApp.SettingSetMenu(settingName);
    }
}