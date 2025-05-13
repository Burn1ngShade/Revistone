using Revistone.Interaction;

namespace Revistone.App.Command;

///<summary> Class representing a command within the console. </summary>
public class AppCommand
{
    public enum CommandType { Console, Apps, Workspace, ChatGPT, Widget, AppSpecific }

    public UserInputProfile format;
    public Action<string> action;

    public string name;
    public string summary;

    public CommandType type;
    public int displayPriority;

    public AppCommand(UserInputProfile format, Action<string> action, string name, string summary, int displayPriority = 0, CommandType type = CommandType.AppSpecific)
    {
        this.format = format;
        this.action = action;
        this.name = name;
        this.summary = summary;
        this.type = type;
        this.displayPriority = displayPriority;
    }
}