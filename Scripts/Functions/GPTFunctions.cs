using System.Text.Json;
using OpenAI.Chat;
using Revistone.App;
using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Management;
using System.Text.RegularExpressions;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using Revistone.Interaction;
using System.Runtime.Serialization;

namespace Revistone.Functions;

/// <summary> Class filled with functions unrelated to console, but useful for interacting with chatGPT API. </summary>
public static class GPTFunctions
{
    static List<ChatMessage> messageHistory = [];
    static List<ChatMemory> memoryHistory = [];

    // --- PUBLIC FUNCTIONS ---

    ///<summary> [DO NOT CALL] Initializes GPT, fetching message history. </summary>
    internal static void InitializeGPT()
    {
        List<SerializableChatMessage>? serializableChats = LoadFileFromJSON<List<SerializableChatMessage>>(GeneratePath(DataLocation.Console, "History/GPT", "Messages.json"));
        if (serializableChats == null) messageHistory = [];
        else messageHistory = serializableChats.Select(x => x.ToChatMessage()).ToList();

        memoryHistory = LoadFileFromJSON<List<ChatMemory>>(GeneratePath(DataLocation.Console, "History/GPT", "Memories.json")) ?? [];
    }

    ///<summary> Function to send a query to run a query with ChatGPT. </summary>
    public static void Query(string query, bool useMessageHistory)
    {
        AddToMessageHistory(new UserChatMessage(query));
        ChatMessage[] messages = GetRelevantChatMessages(useMessageHistory);

        (ChatClient? client, ChatCompletionOptions o) = CreateResponseModel();
        if (client == null) return;

        SendConsoleMessage(new ConsoleLine($"Generating GPT Response.", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine,
            new ConsoleAnimatedLine(WaitForGptResponseAnimation, 1, tickMod: 12, enabled: true));

        ExecuteQuery(client, messages, o);
        Analytics.General.TotalGPTPromts++;
    }

    ///<summary> Outputs a GPT response in a console friendly format. </summary>
    static void SendFormattedGPTResponse(ChatCompletion response, int messageCount)
    {
        UpdatePrimaryConsoleLineAnimation(ConsoleAnimatedLine.None, ConsoleData.primaryLineIndex);

        List<string> lines = new List<string>() { };
        string currentLine = "";

        foreach (char c in response.Content[0].Text)
        {
            if (c == '\n')
            {
                if (currentLine.Length != 0) lines.Add(currentLine);
                currentLine = "";
                continue;
            }

            if (currentLine.Length >= ConsoleData.windowSize.width - 1)
            {
                int lastWordIndex = currentLine.LastIndexOf(' ');

                if (lastWordIndex == -1 || lastWordIndex == currentLine.Length - 1)
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }
                else
                {
                    lines.Add(currentLine[..(lastWordIndex + 1)]);
                    currentLine = currentLine[(lastWordIndex + 1)..];
                }
            }

            currentLine += c;
        }
        lines.Add(currentLine);

        ConsoleLine[] colouredResponse = ColourFormattedGPTResponse(lines);

        SendConsoleMessage(new ConsoleLine($"[{SettingsApp.GetValue("GPT Name")}] ({SettingsApp.GetValue("GPT Model")}) - {response.Usage.TotalTokenCount} Tokens Used ({response.Usage.InputTokenCount} Input, {response.Usage.OutputTokenCount} Output). {messageCount} Messages Used.", ConsoleColor.Cyan));
        SendConsoleMessages(colouredResponse);
    }

    ///<summary> Colours formatted GPT response. </summary>
    static ConsoleLine[] ColourFormattedGPTResponse(List<string> lines)
    {
        ConsoleLine[] consoleLines = new ConsoleLine[lines.Count];

        Regex regex = new Regex(@"\*\*(.*?)\*\*");

        for (int i = 0; i < lines.Count; i++)
        {
            string originalLine = lines[i];
            List<(int start, int length)> indices = [];
            int shift = 0;

            string processedLine = regex.Replace(originalLine, match =>
            {
                string highlightedText = match.Groups[1].Value;
                indices.Add((match.Index - shift, highlightedText.Length));
                shift += 4;
                return highlightedText;
            });

            consoleLines[i] = new ConsoleLine(processedLine, AdvancedHighlight(processedLine.Length, ConsoleColor.DarkBlue, ConsoleColor.Cyan, indices.ToArray()));
        }


        return [.. consoleLines.Select(ConsoleLine.Clean)];
    }

    // --- MESSAGE HISTORY ---

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
    public static void AddToMemories(string content, string creator = "User")
    {
        memoryHistory.Add(new ChatMemory(content, creator));
        SaveFileAsJSON(GeneratePath(DataLocation.Console, "History/GPT", "Memories.json"), memoryHistory);
        SendConsoleMessage(new ConsoleLine("GPT Has Stored A Memory.", AppRegistry.PrimaryCol));
    }

    ///<summary> Creates menu for user to view, edit, and remove memories. </summary>
    public static void ViewMemories()
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
    static void ViewMemory(int index)
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
            if (option == 0) m.Content = UserInput.GetValidUserInput($"--- Update Memory ---", new UserInputProfile(), m.Content, maxLineCount: 5);
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
    static ChatMessage[] GetRelevantChatMessages(bool useMessageHistory)
    {
        ChatMessage[] messages = [CreateQuerySystemChatMessage()];

        if (useMessageHistory) messages = messages.Concat(messageHistory.TakeLast(Math.Min(messageHistory.Count, int.Parse(SettingsApp.GetValue("Conversation Memory")) + 1))).ToArray();
        else messages = messages.Concat([messageHistory[^1]]).ToArray();
        return messages;
    }

    ///<summary> Add message to previous message history, caps size to 50 to keep data storage reasonable. </summary>
    static void AddToMessageHistory(ChatMessage message)
    {
        while (messageHistory.Count >= 50) messageHistory.RemoveAt(0);

        messageHistory.Add(message);

        SaveFileAsJSON(GeneratePath(DataLocation.Console, "History/GPT", "Messages.json"), messageHistory.Select(x => new SerializableChatMessage(x)).ToList());
    }

    ///<summary> Clears the message history of the chatbot. </summary>
    public static void ClearMessageHistory()
    {
        messageHistory = [];
        SaveFileAsJSON(GeneratePath(DataLocation.Console, "History/GPT", "Messages.json"), messageHistory.Select(x => new SerializableChatMessage(x)).ToList());
        SendConsoleMessage(new ConsoleLine("ChatGPT Message History Cleared.", ConsoleColor.DarkBlue));
    }

    // --- GPT LOGIC ---

    ///<summary> Executes and handles chat tools for the given query. </summary>
    static bool ExecuteQuery(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options)
    {
        List<ChatMessage> queryMessages = messages.ToList();

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
                SendConsoleMessage(ConsoleLine.Clean(new ConsoleLine($"GPT Error: {e.Message}", ConsoleColor.Red)));
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
                                if (shouldOutputToolCalls) SendDebugMessage(new ConsoleLine($"[CreateTimer Tool Call] {toolResult}", BuildArray(AppRegistry.SecondaryCol.Extend(23), AppRegistry.PrimaryCol)));
                                if (success) CreateTimer(toolResult);
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(LoadApp):
                                toolResult = TryGetJsonFunctionArgument("appName", toolCall, ref success);
                                if (shouldOutputToolCalls) SendDebugMessage(new ConsoleLine($"[LoadApp Tool Call] {toolResult}", BuildArray(AppRegistry.SecondaryCol.Extend(19), AppRegistry.PrimaryCol)));
                                if (success) delayedChatTools.Add(() => LoadApp(toolResult));
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(SetSetting):
                                toolResult = TryGetJsonFunctionArgument("settingName", toolCall, ref success);
                                if (shouldOutputToolCalls) SendDebugMessage(new ConsoleLine($"[SetSetting Tool Call] {toolResult}", BuildArray(AppRegistry.SecondaryCol.Extend(22), AppRegistry.PrimaryCol)));
                                if (success) delayedChatTools.Add(() => SetSetting(toolResult));
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            case nameof(AddToMemories):
                                toolResult = TryGetJsonFunctionArgument("content", toolCall, ref success);
                                if (shouldOutputToolCalls) SendDebugMessage(new ConsoleLine($"[AddToMemories Tool Call] {toolResult}", BuildArray(AppRegistry.SecondaryCol.Extend(25), AppRegistry.PrimaryCol)));
                                if (success) AddToMemories(toolResult, "GPT");
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                        }
                    }

                    SendConsoleMessage(new ConsoleLine($"Generating GPT Response.", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine,
                    new ConsoleAnimatedLine(WaitForGptResponseAnimation, 1, tickMod: 12, enabled: true));

                    requiresAction = true;
                    break;
            }

        } while (requiresAction);

        AddToMessageHistory(new AssistantChatMessage(completion));
        SendFormattedGPTResponse(completion, messages.Length);

        Analytics.General.UsedGPTInputTokens += completion.Usage.InputTokenCount;
        Analytics.General.UsedGPTOutputTokens += completion.Usage.OutputTokenCount;

        for (int i = 0; i < delayedChatTools.Count; i++)
        {
            delayedChatTools[i].Invoke();
        }

        return true;
    }

    ///<summary> Generates the chat client and options for the currently in use model. </summary>
    static (ChatClient? client, ChatCompletionOptions options) CreateResponseModel()
    {
        string model = SettingsApp.GetValue("GPT Model");

        ChatCompletionOptions o;

        if (model == "gpt-4o-mini")
        {
            o = new ChatCompletionOptions
            {
                Temperature = float.Parse(SettingsApp.GetValue("Temperature")),
                Tools = { getCurrentTimeTool, setTimerTool, cancelTimerTool, loadAppTool, setSetting, remember },
            };
        }
        else
        {
            o = new ChatCompletionOptions
            {
                Tools = { getCurrentTimeTool, setTimerTool, cancelTimerTool, loadAppTool, setSetting, remember },
            };
        }

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
    static SystemChatMessage CreateQuerySystemChatMessage()
    {
        string msg = $"User's name: {SettingsApp.GetValue("Username")}\nUser's pronouns: {SettingsApp.GetValue("Pronouns")}\nYour Name: {SettingsApp.GetValue("GPT Name")}.\n";
        if (SettingsApp.GetValue("Behaviour").Length != 0) msg += $"Your Behaviour: {SettingsApp.GetValue("Behaviour")}\n";
        if (SettingsApp.GetValue("Scenario").Length != 0) msg += $"Current Scenario: {SettingsApp.GetValue("Scenario")}\n";
        if (SettingsApp.GetValue("Long Term Memory") == "Yes") msg += $"Long Term Memory: {string.Join('\n', memoryHistory.Select(x => x.ToString()))}\n";
        if (SettingsApp.GetValue("Use Detailed System Promt") == "Yes") msg += string.Join('\n', LoadFile(GeneratePath(DataLocation.Console, "Assets/GPT", "AboutRevistone.txt")));

        if (SettingsApp.GetValue("Log GPT System Messages") == "Yes") Analytics.Debug.Log($"[GPT] System Message: {msg}");
        return new SystemChatMessage(msg);
    }

    static void WaitForGptResponseAnimation(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
    {
        int dotCount = (int)animationInfo.metaInfo;
        SendConsoleMessage(new ConsoleLine($"Generating GPT Response{new string('.', dotCount)}", AppRegistry.PrimaryCol), ConsoleLineUpdate.SameLine);
        dotCount = dotCount < 3 ? dotCount + 1 : 0;
        animationInfo.metaInfo = dotCount;
    }

    // --- CHAT TOOLS ---

    ///<summary> Attempts to get GPT argument for toolCall function. </summary>
    static string TryGetJsonFunctionArgument(string argument, ChatToolCall toolCall, ref bool success)
    {
        string jsonString = toolCall.FunctionArguments.ToString();
        try
        {
            // Deserialize JSON into a dictionary
            var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (arguments != null && arguments.TryGetValue(argument, out string? argumentValue))
            {
                return argumentValue;
            }
            else
            {
                success = false;
                return $"Error: Missing '{argument}' argument.";
            }
        }
        catch (JsonException ex)
        {
            success = false;
            return $"Error: Invalid JSON format. {ex.Message}";
        }
    }

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
        AppCommands.Commands($"timer {duration}");
    }

    static void CancelTimer()
    {
        AppCommands.Commands("cancel timer");
    }

    static void LoadApp(string appName)
    {
        AppCommands.Commands($"load {appName}");
    }

    static void SetSetting(string settingName)
    {
        SettingsApp.HandleSettingSet(settingName);
    }
}