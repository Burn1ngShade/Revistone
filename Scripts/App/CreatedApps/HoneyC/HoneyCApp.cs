using Revistone.Console;
using Revistone.Interaction;
using Revistone.Apps.HoneyC;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using Revistone.Functions;

namespace Revistone.Apps;

/// <summary> Class for all calculation based functions, used via command [calc] or [c] ... </summary>
public class HoneyCApp : App
{
    static bool creatingProgram = false;
    static List<string> programLines = [];

    // --- APP BOILER ---

    public HoneyCApp() : base() { }
    public HoneyCApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return new HoneyCApp[] {
            new HoneyCApp("HoneyC Terminal", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5),
            [(new UserInputProfile(new UserInputProfile.InputType[] {}, "create program", caseSettings: StringFunctions.CapitalCasing.Lower, removeTrailingWhitespace: true, removeLeadingWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine("Editing Program [New Program]", ConsoleColor.DarkBlue)); creatingProgram = true; }, "Enable Program Edit Mode."),
             (new UserInputProfile(new UserInputProfile.InputType[] {}, "finish program", caseSettings: StringFunctions.CapitalCasing.Lower, removeTrailingWhitespace: true, removeLeadingWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine("Running Program [New Program]", ConsoleColor.DarkBlue)); FinishProgramCreation(); }, "Exit Program Edit Mode And Run Program."),
            ], 70, 40)
        };
    }



    public override void OnUserInput(string userInput)
    {
        if (AppCommands.Commands(userInput)) return;

        if (creatingProgram)
        {
            programLines.Add(userInput);
            int index = GetConsoleLineIndex() - 1;
            UpdatePrimaryConsoleLine(new ConsoleLine($"{programLines.Count}. {GetConsoleLine(index).lineText[2..]}", BuildArray(ConsoleColor.Cyan.ToArray(), GetConsoleLine(index).lineColour)), index);
        }
        else
        {
            HoneyCInterpreter.Intepret(userInput);
        }
    }

    static void FinishProgramCreation()
    {
        creatingProgram = false;
        programLines = programLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        string program = "";
        for (int i = 0; i < programLines.Count; i++)
        {
            string l = programLines[i];
            string nl = i + 1 == programLines.Count ? " " : programLines[i + 1];

            if (l.Length == 0) continue; // empty line

            if (l[^1] == ';' || l[^1] == '}' || l[^1] == '{' || nl[0] == '{' || nl[0] == '}') program += l;
            else program += $"{l};";
        }

        programLines = [];
        HoneyCInterpreter.Intepret(program);

        return;
    }
}