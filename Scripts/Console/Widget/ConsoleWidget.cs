using Revistone.App;

namespace Revistone.Console.Widget;

/// <summary> Base class for creating and displaying widgets within the console. </summary>
public abstract class ConsoleWidget
{
    public string name;
    public uint order; // the order in which the widget should be displayed (left to right)
    public bool canRemove;

    public bool hide = false;

    public ConsoleWidget(string name, uint order, bool canRemove)
    {
        this.name = name;
        this.order = order;
        this.canRemove = canRemove;
    }

    ///<summary> Gets the current state of the widget for display. </summary>
    public abstract string GetContent(ref bool shouldRemove);

    // --- WIDGET BEHAVIOUR ---

    static List<ConsoleWidget> widgets = new List<ConsoleWidget>()
    {
        new FrameWidget("Frame Rate", 0, false),
        new BillboardWidget("Author", 1, "Creator: Isaac Honeyman", false),
        new TimeWidget("Current Time", uint.MaxValue, DateTime.Now, false, true, false),
    };

    ///<summary> Returns all console widgets content. </summary> 
    public static string[] GetWidgetContents()
    {
        List<string> contents = [];

        List<int> widgetsToRemove = [];
        for (int i = 0; i < widgets.Count; i++)
        {
            ConsoleWidget widget = widgets[i];

            if (widget.hide) continue;

            bool shouldRemove = false;
            string content = widget.GetContent(ref shouldRemove);

            if (shouldRemove) widgetsToRemove.Add(i);
            else contents.Add($" [{content}] ");
        }

        for (int i = widgetsToRemove.Count - 1; i >= 0; i--) TryRemoveWidget(widgets[widgetsToRemove[i]].name);

        return [.. contents];
    }

    ///<summary> Returns if widget of given name exists. </summary>
    public static bool WidgetExists(string widgetName)
    {
        return widgets.Any(w => w.name == widgetName);
    }

    ///<summary> Updates hide status of given widget if it exists. </summary>
    public static bool TryUpdateWidgetHide(string widgetName, bool hide)
    {
        int index = widgets.FindIndex(w => w.name == widgetName);

        if (index != -1)
        {
            widgets[index].hide = hide;
            return true;
        }

        return false;
    }

    ///<summary> Attempts to add widget to console, fails if widget of same name already exists. </summary>
    public static bool TryAddWidget(ConsoleWidget widget)
    {
        if (widgets.Any(w => w.name == widget.name)) return false;

        widgets.Add(widget);
        widgets = [.. widgets.OrderBy(w => w.order)];

        return true;
    }

    ///<summary> Attempts to remove widget from console, fails if widget of name can't be found or widget is marked as unremovable. </summary>
    public static bool TryRemoveWidget(string widgetName)
    {
        int index = widgets.FindIndex(w => w.name == widgetName);

        if (index != -1 && widgets[index].canRemove)
        {
            widgets.RemoveAt(index);
            return true;
        }

        return false;
    }

    ///<summary> Attempts to get widget, fails if widget of name does not exist. </summary>    
    static ConsoleWidget? TryGetWidget(string widgetName)
    {
        return widgets.Find(w => w.name == widgetName);
    }

    ///<summary> Updates the hidden status of widgets upon a changed setting. </summary>
    static void OnSettingChange(string settingName)
    {
        if (settingName == "Show FPS Widget")
        {
            ConsoleWidget? w = TryGetWidget("Frame Rate");
            if (w != null) w.hide = SettingsApp.GetValue("Show FPS Widget") != "Yes";
        }
        else if (settingName == "Show Author Widget")
        {
            ConsoleWidget? w = TryGetWidget("Author");
            if (w != null) w.hide = SettingsApp.GetValue("Show Author Widget") != "Yes";
        }
        else if (settingName == "Show Time Widget")
        {
            ConsoleWidget? w = TryGetWidget("Current Time");
            if (w != null) w.hide = SettingsApp.GetValue("Show Time Widget") != "Yes";
        }
    }

    ///<summary> [DO NOT CALL] Initializes Widgets. </summary>
    public static void InitializeWidgets()
    {
        SettingsApp.OnSettingChanged += OnSettingChange;

        OnSettingChange("Show FPS Widget");
        OnSettingChange("Show Author Widget");
        OnSettingChange("Show Time Widget");
    }
}


