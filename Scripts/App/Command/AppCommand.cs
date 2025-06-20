using Revistone.Interaction;

namespace Revistone.App.Command;

///<summary> Class representing a command within the console. </summary>
public class AppCommand
{
    public enum CommandType { Console, Apps, Workspace, ChatGPT, Widget, AppSpecific, Developer }

    public UserInputProfile format;
    public Action<string> action;

    public string commandName;
    public string description;

    public CommandType type;
    public int displayPriority;

    public AppCommand(UserInputProfile format, Action<string> action, string commandName, string description, int displayPriority = 0, CommandType type = CommandType.AppSpecific)
    {
        this.format = format;
        this.action = action;
        this.commandName = commandName;
        this.description = description;
        this.type = type;
        this.displayPriority = displayPriority;
    }
}