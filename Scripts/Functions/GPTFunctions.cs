using OpenAI.Assistants;
using OpenAI.Chat;
using Revistone.Apps;
using Revistone.Console;
using Revistone.Console.Data;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Functions;

/// <summary>
/// Class filled with functions unrelated to console, but useful for interacting with chatGPT API.
/// </summary>
public static class GPTFunctions
{
    static List<ChatMessage> messageHistory = [];

    public static void Query(string query, bool useMessageHistory)
    {
        messageHistory.Add(new UserChatMessage(query));
        ChatMessage[] messages = GetRelevantChatMessages(useMessageHistory);

        var o = new ChatCompletionOptions
        {
            Temperature = float.Parse(SettingsApp.GetValue("Temperature")),
            Tools = { getCurrentTimeTool },
        };

        ChatClient? client = CreateChatClient("gpt-4-turbo");
        if (client == null) return;
        ExecuteQuery(client, messages, o);
    }

    static ChatMessage[] GetRelevantChatMessages(bool useMessageHistory)
    {
        ChatMessage[] messages = [CreateQuerySystemChatMessage()];

        if (useMessageHistory) messages = messages.Concat(messageHistory.TakeLast(Math.Min(messageHistory.Count, int.Parse(SettingsApp.GetValue("Memory")) + 1))).ToArray();
        else messages = messages.Concat([messageHistory[^1]]).ToArray();
        return messages;
    }

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
                        }
                    }

                    requiresAction = true;
                    break;
            }

        } while (requiresAction);

        messageHistory.Add(new AssistantChatMessage(completion));
        SendFormattedGPTResponse(completion, messages.Length);
    }

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

    static SystemChatMessage CreateQuerySystemChatMessage()
    {
        string msg = $"User's name: {SettingsApp.GetValue("Username")}. User's pronouns: {SettingsApp.GetValue("Pronouns")}. Your Name: {SettingsApp.GetValue("GPT Name")}.";
        if (SettingsApp.GetValue("Behaviour").Length != 0) msg += $"Your Behaviour: {SettingsApp.GetValue("Behaviour")}";
        if (SettingsApp.GetValue("Scenario").Length != 0) msg += $"Current Scenario: {SettingsApp.GetValue("Scenario")}";

        return new SystemChatMessage(msg);
    }

    public static void ClearMessageHistory()
    {
        messageHistory = [];
        SendConsoleMessage(new ConsoleLine("ChatGPT Message History Cleared.", ConsoleColor.DarkBlue));
    }

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

    // --- CHAT TOOLS ---

    private static readonly ChatTool getCurrentTimeTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentTime),
        functionDescription: "Get the current time."
    );

    static string GetCurrentTime()
    {
        return $"{DateTime.Now}";
    }
}