using System.Text.Json;
using OpenAI.Chat;
using Revistone.App;
using Revistone.Console;
using Revistone.Console.Data;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Functions;

/// <summary> Class filled with functions unrelated to console, but useful for interacting with chatGPT API. </summary>
public static class GPTFunctions
{
    static List<ChatMessage> messageHistory = [];

    // --- PUBLIC FUNCTIONS ---

    ///<summary> Function to send a query to run a query with ChatGPT. </summary>
    public static void Query(string query, bool useMessageHistory)
    {
        messageHistory.Add(new UserChatMessage(query));
        ChatMessage[] messages = GetRelevantChatMessages(useMessageHistory);

        var o = new ChatCompletionOptions
        {
            Temperature = float.Parse(SettingsApp.GetValue("Temperature")),
            Tools = { getCurrentTimeTool, setTimerTool, cancelTimerTool },
        };

        ChatClient? client = CreateChatClient("gpt-4-turbo");
        if (client == null) return;
        ExecuteQuery(client, messages, o);
    }

    ///<summary> Clears the message history of the chatbot. </summary>
    public static void ClearMessageHistory()
    {
        messageHistory = [];
        SendConsoleMessage(new ConsoleLine("ChatGPT Message History Cleared.", ConsoleColor.DarkBlue));
    }

    ///<summary> Outputs a GPT response in a console friendly format. </summary>
    public static void SendFormattedGPTResponse(ChatCompletion response, int messageCount)
    {
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

        SendConsoleMessage(new ConsoleLine($"[{SettingsApp.GetValue("GPT Name")}] - {response.Usage.TotalTokenCount} Tokens Used ({response.Usage.InputTokenCount} Input, {response.Usage.OutputTokenCount} Output). {messageCount} Messages Used.", ConsoleColor.Cyan));
        SendConsoleMessages(lines.Select(x => ConsoleLine.Clean(new ConsoleLine(x, ConsoleColor.DarkBlue))).ToArray());
    }

    // --- GPT LOGIC ---

    ///<summary> Gathers all context messages needed to generate a response. </summary>
    static ChatMessage[] GetRelevantChatMessages(bool useMessageHistory)
    {
        ChatMessage[] messages = [CreateQuerySystemChatMessage()];

        if (useMessageHistory) messages = messages.Concat(messageHistory.TakeLast(Math.Min(messageHistory.Count, int.Parse(SettingsApp.GetValue("Memory")) + 1))).ToArray();
        else messages = messages.Concat([messageHistory[^1]]).ToArray();
        return messages;
    }

    ///<summary> Executes and handles chat tools for the given query. </summary>
    static void ExecuteQuery(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options)
    {
        List<ChatMessage> queryMessages = messages.ToList();

        bool requiresAction;
        ChatCompletion completion;

        do
        {
            requiresAction = false;

            completion = client.CompleteChat(queryMessages, options);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                    queryMessages.Add(new AssistantChatMessage(completion));
                    break;
                case ChatFinishReason.ToolCalls:
                    queryMessages.Add(new AssistantChatMessage(completion));
                    foreach (ChatToolCall toolCall in completion.ToolCalls)
                    {
                        string toolResult = "";
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
                                bool success = true;
                                toolResult = TryGetJsonFunctionArgument("time", toolCall, ref success);
                                if (success) CreateTimer(toolResult);
                                queryMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                        }
                    }

                    requiresAction = true;
                    break;
            }

        } while (requiresAction);

        messageHistory.Add(new AssistantChatMessage(completion));
        SendFormattedGPTResponse(completion, messages.Length);
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
        string msg = $"User's name: {SettingsApp.GetValue("Username")}. User's pronouns: {SettingsApp.GetValue("Pronouns")}. Your Name: {SettingsApp.GetValue("GPT Name")}.";
        if (SettingsApp.GetValue("Behaviour").Length != 0) msg += $"Your Behaviour: {SettingsApp.GetValue("Behaviour")}";
        if (SettingsApp.GetValue("Scenario").Length != 0) msg += $"Current Scenario: {SettingsApp.GetValue("Scenario")}";

        return new SystemChatMessage(msg);
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
        functionDescription: "Get the current time."
    );

    static readonly ChatTool setTimerTool = ChatTool.CreateFunctionTool(
        functionName: nameof(CreateTimer),
        functionDescription: "Creates a countdown timer from 'hh:mm:ss'. Example: '01:30:00' for 1h 30m. Example: '02:40:20 for 2 hours 40 minutes and 20 seconds. Extend to any common abreviations of time e.g mins hrs secs."
    );

    static readonly ChatTool cancelTimerTool = ChatTool.CreateFunctionTool(
        functionName: nameof(CancelTimer),
        functionDescription: "Cancels timer."
    );

    static string GetCurrentTime()
    {
        return $"{DateTime.Now}";
    }

    static void CreateTimer(string time)
    {
        AppCommands.Commands($"timer {time}");
    }

    static void CancelTimer()
    {
        AppCommands.Commands("cancel timer");
    }
}