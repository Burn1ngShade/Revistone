using System.Text.RegularExpressions;
using Revistone.Console;
using Revistone.Console.Image;
using Revistone.Functions;

using static Revistone.App.BaseApps.Calculator.CalculatorComputations;
using static Revistone.Console.ConsoleAction;

namespace Revistone.App.BaseApps.Calculator;

public static class CalculatorInterpreter
{
    /// <summary> Inteprets and runs a honeyC query or program. </summary>
    public static void Intepret(string s)
    {
        string sanitisedS = "";
        bool inString = false;
        foreach (char c in s)
        {
            if (c == '\"') inString = !inString;

            if (!inString && c == ' ') continue;
            sanitisedS += c;
        }
        s = sanitisedS;
        s = s.Replace("};", "}");
        s = s.Replace(";}", "}");

        // Elimate Illegal Matches

        if (s.Contains("{;") || s.Contains(";{")) { ThrowInterpreterError("Characters [{] And [;] Can Not Be Used Sequentially"); return; }
        if (s.Contains("{}")) { ThrowInterpreterError("Range Statement Is Empty"); return; }
        if (s.Contains("{{")) { ThrowInterpreterError("[{{] Is Invalid Loop Syntax"); return; }
        if (s.Contains(";;")) { ThrowInterpreterError("[;;] Is Invalid Statement Syntax"); return; }

        // Split up string and calculate loops

        string[] lines = Regex.Split(s, @"(?<=\})(?!\})|(?<!\{)(?=\{)|;").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        Dictionary<int, int> bracketIndexes = [];
        Stack<int> bracketOpenStack = [];
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i][0] == '{')
            {
                if (lines[i].Length == 1) { ThrowInterpreterError("Range Statement Missing Closing [}]"); return; }
                lines[i] = lines[i][1..];
                bracketOpenStack.Push(i);
            }
            while (lines[i][^1] == '}')
            {
                lines[i] = lines[i][..^1];
                if (bracketOpenStack.Count == 0) { ThrowInterpreterError("Range Statement Missing Opening [{]"); return; }
                bracketIndexes.Add(bracketOpenStack.Pop(), i);
            }
        }

        // now lets actually run the code 

        FlagProfile defaultFlags = FlagProfile.None;
        List<RangeProfile> ranges = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines[i]; // NEVER reference lines[i], as loops will need to do the same inteperting again
            int calculatorMode = 0;
            FlagProfile flags = new FlagProfile(ref l);

            if (l.ToLower().StartsWith("flags"))
            {
                defaultFlags = flags;
                if (l.Length != 5) { ThrowInterpreterError($"Flag Statement Contains Invalid Flag(s) [{l[5..]}]"); return; }
                calculatorMode = 4; // so it will skip trying to calculate
            }
            else
            {
                if (FlagProfile.IsNone(flags)) flags = defaultFlags; // if no flags referenced we use the whole program flags
            }

            DebugOutput("--- Debug Begin ---", flags);

            if (l.ToLower().StartsWith("range"))
            {
                if (l.Length == 5) { ThrowInterpreterError("Range Statement Must Have Atleast 1 Parameter", flags); return; }

                RangeProfile rangeProfile = DefineRange(l[5..], flags);
                if (rangeProfile.startValue == -1) return;

                if (!bracketIndexes.ContainsKey(i + 1)) { ThrowInterpreterError("Range Statement Must Be Directly Followed By [{].", flags); return; }

                rangeProfile.SetIndexes(i + 1, bracketIndexes[i + 1]);
                ranges.Add(rangeProfile);

                if (flags.debug) SendConsoleMessage(new ConsoleLine("---- Debug End ----", ConsoleColour.Cyan));
                continue;
            }

            if (l.ToLower().StartsWith("let"))
            {
                l = l[3..];
                calculatorMode = 1;
            }
            else if (l.ToLower().StartsWith("del"))
            {
                l = l[3..];
                calculatorMode = 2;
            }
            else if (l.ToLower().StartsWith("out"))
            {
                l = l[3..];
                calculatorMode = 3;
            }

            switch (calculatorMode)
            {
                case 0:
                    if (!Calculate(l, flags).success) return;
                    break;
                case 1:
                    if (!Assign(l, flags)) return;
                    break;
                case 2:
                    if (!Unassign(l, flags)) return;
                    break;
                case 3:
                    if (!CreateOutput(l, flags)) return; //send actual line to preserve spaces
                    break;
            }

            for (int j = ranges.Count - 1; j >= 0; j--) //loop through backwards so deeper loops adressed first
            {
                if (ranges[j].endIndex == i) // we reached end of range
                {
                    if (ranges[j].currentValue + 1 == ranges[j].endValue) // range is complete
                    {
                        if (ranges[j].tempVariable != "")
                        {
                            if (!Unassign(ranges[j].tempVariable, new FlagProfile(false, true, true)))
                            {
                                ThrowInterpreterError($"Range Variable [{ranges[j].tempVariable}] Can Not Be Removed");
                                return;
                            }
                        }

                        ranges.RemoveAt(j);
                        continue;
                    }

                    if (ranges[j].tempVariable != "")
                    {
                        if (!CalculatorDefinitions.variables.ContainsKey(ranges[j].tempVariable))
                        {
                            ThrowInterpreterError($"Range Variable [{ranges[j].tempVariable}] Can Not Be Removed");
                            return;
                        }

                        CalculatorDefinitions.variables[ranges[j].tempVariable] += 1;
                    }


                    ranges[j].currentValue++;
                    i = ranges[j].startIndex - 1; // -1 as will be immediatly incremeted
                    break;
                }
            }

            DebugOutput("---- Debug End ----", flags);
        }
    }

    // --- CONSOLE OUTPUTS ---



    /// <summary> Throws interpreter error and halts program. </summary>
    public static void ThrowInterpreterError(string error, FlagProfile flags)
    {
        if (!flags.hideError) SendConsoleMessage(new ConsoleLine($"Error: {error}!", ConsoleColour.Red));
    }

    /// <summary> Throws interpreter error and halts program. </summary>
    public static TReturn[] ThrowInterpreterError<TReturn>(string error, FlagProfile flags)
    {
        ThrowInterpreterError(error, flags);
        return [];
    }

    /// <summary> Throws interpreter error and halts program. </summary>
    public static void ThrowInterpreterError(string error) { ThrowInterpreterError(error, FlagProfile.None); }

    public static void Output<T>(T output, FlagProfile flags)
    {
        if (!flags.hideOutput) SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.PrimaryCol));
    }

    public static void DebugHeaderOutput<T>(T output, FlagProfile flags)
    {
        if (flags.debug) SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.TertiaryColour));
    }

    public static void DebugOutput<T>(T output, FlagProfile flags)
    {
        if (flags.debug) SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.SecondaryCol));
    }


    public struct FlagProfile
    {
        public bool debug = false;
        public bool hideOutput = false;
        public bool hideError = false;

        public FlagProfile(ref string s)
        {
            if (s.Contains("-d"))
            {
                debug = true;
                s = s.Replace("-d", "");
            }

            if (s.Contains("-ho"))
            {
                hideOutput = true;
                s = s.Replace("-ho", "");
            }

            if (s.Contains("-he"))
            {
                hideError = true;
                s = s.Replace("-he", "");
            }
        }

        public FlagProfile(bool debug, bool hideOutput, bool hideError)
        {
            this.debug = debug;
            this.hideOutput = hideOutput;
            this.hideError = hideError;
        }

        public static FlagProfile None => new FlagProfile(false, false, false);

        public static bool IsNone(FlagProfile flags) { return flags.debug == false && flags.hideOutput == false && flags.hideError == false; }
    }

    public class RangeProfile
    {
        public int startValue;
        public int endValue;
        public int currentValue;
        public string tempVariable;

        public int startIndex;
        public int endIndex;

        public RangeProfile(int startIndex, int endIndex, string tempVariable = "")
        {
            this.startValue = startIndex;
            this.endValue = endIndex;
            this.tempVariable = tempVariable;

            currentValue = startIndex;
        }

        public RangeProfile() : this(-1, -1, "") { }

        public void SetIndexes(int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }
    }
}