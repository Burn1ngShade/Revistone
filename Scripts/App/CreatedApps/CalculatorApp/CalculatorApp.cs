using Revistone.Console;
using Revistone.Interaction;
using Revistone.Apps.Calculator;

using static Revistone.Apps.Calculator.CalculatorDefinitions;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Apps;

/// <summary> Class for all calculation based functions, used via command [calc] or [c] ... </summary>
public class CalculatorApp : App
{
    // --- APP BOILER ---

    public CalculatorApp() : base() { }
    public CalculatorApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return new CalculatorApp[] {
            new CalculatorApp("Calculator", (ConsoleColor.DarkBlue, ConsoleColor.Cyan.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), new (UserInputProfile format, Action<string> payload, string summary)[0]

            , 70, 40)
        };
    }

    public override void OnUserInput(string userInput)
    {
        if (AppCommands.Commands(userInput)) return;

        IntepretString(userInput);
    }

    // --- FUNCTIONS ---

    public static void IntepretString(string input)
    {
        input = input.Replace(" ", "");

        int calculatorMode = 0;

        if (input.ToLower().StartsWith("calc"))
        {
            input = input[4..];
            calculatorMode = 0;
        }
        else if (input.ToLower().StartsWith("let"))
        {
            input = input[3..];
            calculatorMode = 1;
        }
        else if (input.ToLower().StartsWith("del"))
        {
            input = input[3..];
            calculatorMode = 2;
        }

        switch (calculatorMode)
        {
            case 0:
                if (!Calculate(input).success) return;
                break;
            case 1:
                if (!Assign(input)) return;
                break;
            case 2:
                if (!Unassign(input)) return;
                break;
        }
    }

    public static (bool success, double value) Calculate(string calculation, bool outputValue = true)
    {
        calculation = calculation.Replace(" ", "");

        bool debug = IsDebugFlag(ref calculation);

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine("--- Calculation Debug ---", ConsoleColor.Cyan));
            SendConsoleMessage(new ConsoleLine($"Calculation: {calculation}", ConsoleColor.DarkBlue));
        }

        Token[] tokens = Token.Tokenize(calculation, debug);
        if (tokens.Length == 0) return (false, 0);

        int layer = tokens.Max(t => t.layer);

        while (layer >= -1)
        {
            if (debug) SendConsoleMessage(new ConsoleLine($"> Attempting To Calculate Layer {layer} Tokens", ConsoleColor.Cyan));
            tokens = EvaluateLayer(tokens, layer, debug);
            if (tokens.Length == 0) return (false, 0);
            else if (tokens.Length == 1)
            {
                if (debug) SendConsoleMessage(new ConsoleLine("------- Debug End -------", ConsoleColor.Cyan));
                if (outputValue) SendConsoleMessage(new ConsoleLine(((ValueToken)tokens[0]).value.ToString(), ConsoleColor.DarkBlue));
                return (true, ((ValueToken)tokens[0]).value);
            }
            layer--;
        }

        if (debug) foreach (var token in tokens) SendConsoleMessage(new ConsoleLine(token.ToString(), ConsoleColor.DarkBlue));

        return (false, 0);
    }

    public static bool Assign(string def)
    {
        def = def.Replace(" ", "");

        bool debug = IsDebugFlag(ref def);

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine("--- Assignment Debug ---", ConsoleColor.Cyan));
            SendConsoleMessage(new ConsoleLine($"Statement: {def}", ConsoleColor.DarkBlue));
        }

        if (!def.Contains('='))
        {
            ThrowCalculationError("All Assignment Statements Must Contain [=]!");
            return false;
        }

        string variableName = def.Substring(0, def.IndexOf('='));
        string variableAssignment = def.Substring(def.IndexOf('=') + 1);

        if (debug) SendConsoleMessage(new ConsoleLine($"> Validating Variable Name: {variableName}", ConsoleColor.Cyan));

        UserInputProfile validVariable = new UserInputProfile(inputFormat: "[C:]", outputFormat: UserInputProfile.OutputFormat.NoOutput);

        if (!validVariable.InputValid(variableName))
        {
            ThrowCalculationError($"Name [{variableName}] Is Invalid, Variable Names Must Be Purely Letters!");
            return false;
        }
        if (operators.ContainsKey(variableName) || constants.ContainsKey(variableName) || functions.ContainsKey(variableName))
        {
            ThrowCalculationError($"Name [{variableName}] Is Protected!");
            return false;
        }

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine($"Variable Name [{variableName}] Is Valid", ConsoleColor.DarkBlue));
            SendConsoleMessage(new ConsoleLine($"> Validating Variable Assignment: {variableAssignment}", ConsoleColor.Cyan));
        }

        (bool success, double value) evaluatedAssignment = Calculate($"{(debug ? "-d" : "")}{variableAssignment}", false);
        if (!evaluatedAssignment.success) return false;

        if (variables.ContainsKey(variableName)) variables[variableName] = evaluatedAssignment.value;
        else variables.Add(variableName, evaluatedAssignment.value);

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine("------- Debug End -------", ConsoleColor.Cyan));
        }

        SendConsoleMessage(new ConsoleLine($"Variable [{variableName}] Assigned Value [{evaluatedAssignment.value}].", ConsoleColor.DarkBlue));
        return true;
    }

    public static bool Unassign(string var)
    {
        var = var.Replace(" ", "");

        bool debug = IsDebugFlag(ref var);

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine("--- Unassign Debug ---", ConsoleColor.Cyan));
            SendConsoleMessage(new ConsoleLine($"Statement: {var}", ConsoleColor.DarkBlue));
            SendConsoleMessage(new ConsoleLine("> Validating Variable Existence.", ConsoleColor.Cyan));
        }

        if (!variables.ContainsKey(var))
        {
            ThrowCalculationError($"Variable With Name [{var}] Does Not Exist.");
            return false;
        }

        if (debug) SendConsoleMessage(new ConsoleLine("Variable Existence Validated", ConsoleColor.DarkBlue));

        variables.Remove(var);
        SendConsoleMessage(new ConsoleLine($"Variable [{var}] Removed.", ConsoleColor.DarkBlue));

        if (debug) SendConsoleMessage(new ConsoleLine("------ Debug End ------", ConsoleColor.Cyan));
        return true;
    }

    static Token[] EvaluateLayer(Token[] tokens, int layer, bool debug)
    {
        List<Token> layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Value).ToList();

        for (int i = layerTokens.Count - 1; i >= 0; i--)
        {
            ValueToken t = (ValueToken)layerTokens[i];
            if (t.index != 0 && tokens[t.index - 1].type == Token.TokenType.Value && tokens[t.index - 1].layer == layer) tokens = Token.Insert(tokens, new OperatorToken("*", t.index, t.layer), t.index);
        }

        layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Function).ToList();

        for (int i = layerTokens.Count - 1; i >= 0; i--) // First lets carry out all functions, easy peasy
        {
            FunctionToken t = (FunctionToken)layerTokens[i];

            if (tokens.Length == t.index + 1 || tokens[t.index + 1].layer != layer) return ThrowCalculationError($"Function [{t.identifier}] Was Not Given An Input!");
            if (tokens[t.index + 1].type != Token.TokenType.Value) return ThrowCalculationError($"Function [{t.identifier}] Given Invalid Input [{tokens[t.index + 1].identifier}]!");

            if (debug) SendConsoleMessage(new ConsoleLine($"Evaluating Function {t.identifier}({((ValueToken)tokens[t.index + 1]).value})", ConsoleColor.DarkBlue));

            tokens[t.index] = new ValueToken(t.operation.Invoke(((ValueToken)tokens[t.index + 1]).value).ToString(), t.index, t.layer);
            tokens = Token.RemoveIndex(tokens, t.index + 1);
        }

        layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Operator).OrderByDescending(t => ((OperatorToken)t).priority).ToList();

        for (int i = 0; i < layerTokens.Count; i++)
        {
            OperatorToken t = (OperatorToken)layerTokens[i];

            if (t.index == 0 || t.index + 1 == tokens.Length || tokens[t.index - 1].layer != layer || tokens[t.index + 1].layer != layer)
            {
                return ThrowCalculationError($"Operator [{t.identifier}] Is Missing Input(s)!");
            }

            if (tokens[t.index - 1].type != Token.TokenType.Value) return ThrowCalculationError($"Operator [{t.identifier}] Given Invalid Input [{tokens[t.index - 1].identifier}]!");
            if (tokens[t.index + 1].type != Token.TokenType.Value) return ThrowCalculationError($"Operator [{t.identifier}] Given Invalid Input [{tokens[t.index + 1].identifier}]!");

            if (debug) SendConsoleMessage(new ConsoleLine($"Evaluating Operation {((ValueToken)tokens[t.index - 1]).value} {t.identifier} {((ValueToken)tokens[t.index + 1]).value}", ConsoleColor.DarkBlue));

            tokens[t.index] = new ValueToken(t.operation.Invoke(((ValueToken)tokens[t.index - 1]).value, ((ValueToken)tokens[t.index + 1]).value).ToString(), t.index, t.layer);
            tokens = Token.RemoveIndex(tokens, t.index + 1);
            tokens = Token.RemoveIndex(tokens, t.index - 1);
        }

        layerTokens = tokens.Where(t => t.layer == layer).ToList();
        for (int i = layerTokens.Count - 1; i >= 0; i--)
        {
            ValueToken t = (ValueToken)layerTokens[i];

            tokens[t.index].layer--;
            if (t.index != 0 && tokens[t.index - 1].type == Token.TokenType.Value) tokens = Token.Insert(tokens, new OperatorToken("*", t.index, t.layer), t.index);
        }

        return tokens;
    }

    public static Token[] ThrowCalculationError(string error)
    {
        SendConsoleMessage(new ConsoleLine($"Error: {error}", ConsoleColor.Red));
        return new Token[0];
    }

    public static bool IsNumber(string s)
    {
        if (s.Length == 0) return false;
        if (s[^1] == '.') s = s.Substring(0, s.Length - 1);

        return double.TryParse(s, out var result);
    }

    static bool IsDebugFlag(ref string input)
    {
        if (input.Length >= 2 && input[..2] == "-d")
        {
            input = input[2..];
            return true;
        }
        return false;
    }

}