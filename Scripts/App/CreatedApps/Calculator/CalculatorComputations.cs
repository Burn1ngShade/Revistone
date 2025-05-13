using Revistone.Interaction;

using static Revistone.App.BaseApps.Calculator.CalculatorDefinitions;
using static Revistone.App.BaseApps.Calculator.CalculatorInterpreter;

namespace Revistone.App.BaseApps.Calculator;

public static class CalculatorComputations
{
    public static (bool success, double value) Calculate(string calculation, FlagProfile flags)
    {
        Token[] tokens = Token.Tokenize(calculation, flags);
        if (tokens.Length == 0) return (false, 0);

        int layer = tokens.Max(t => t.layer);

        while (layer >= -1)
        {
            DebugHeaderOutput($"> Computing Layer [{layer}] Tokens", flags);
            tokens = Token.EvaluateTokens(tokens, layer, flags);
            if (tokens.Length == 0) return (false, 0);
            else if (tokens.Length == 1)
            {
                Output(((ValueToken)tokens[0]).value, flags);
                return (true, ((ValueToken)tokens[0]).value);
            }
            layer--;
        }

        foreach (var token in tokens) DebugOutput(token, flags);
        return (false, 0);
    }

    public static bool Assign(string def, FlagProfile flags)
    {
        if (!def.Contains('=')) { ThrowInterpreterError("All Assignment Statements Must Contain [=]", flags); return false; }

        string variableName = def.Substring(0, def.IndexOf('='));
        string variableAssignment = def.Substring(def.IndexOf('=') + 1);

        DebugHeaderOutput($"> Validating Variable Name [{variableName}]", flags);

        UserInputProfile validVariable = new UserInputProfile(validInputFormat: "[C:]", outputFormat: UserInputProfile.OutputFormat.NoOutput);

        if (!validVariable.InputValid(variableName)) { ThrowInterpreterError($"Name [{variableName}] Is Invalid, Variable Names Must Be Purely Letters", flags); return false; }
        if (operators.ContainsKey(variableName) || constants.ContainsKey(variableName) || functions.ContainsKey(variableName))
        {
            ThrowInterpreterError($"Name [{variableName}] Is Protected", flags);
            return false;
        }

        DebugOutput($"Variable Name [{variableName}] Is Valid", flags);

        (bool success, double value) evaluatedAssignment = Calculate(variableAssignment, new FlagProfile(flags.debug, true, true));
        if (!evaluatedAssignment.success) return false;
        DebugOutput($"Variable Value [{variableAssignment}] Is Valid", flags);

        if (variables.ContainsKey(variableName)) variables[variableName] = evaluatedAssignment.value;
        else variables.Add(variableName, evaluatedAssignment.value);

        Output($"Variable [{variableName}] Assigned Value [{evaluatedAssignment.value}]", flags);
        return true;
    }

    public static bool Unassign(string var, FlagProfile flags)
    {
        DebugHeaderOutput($"> Validating Variable [{var}] Existence", flags);

        if (!variables.ContainsKey(var)) { ThrowInterpreterError($"Variable With Name [{var}] Does Not Exist", flags); return false; }

        DebugOutput($"Variable [{var}] Existence Validated", flags);

        variables.Remove(var);
        Output($"Variable [{var}] Removed.", flags);

        return true;
    }

    public static RangeProfile DefineRange(string def, FlagProfile flags)
    {
        DebugHeaderOutput($"> Validating Range Parameters [{def}]", flags);

        string[] rangeParams = def.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (rangeParams.Length > 3)
        {
            ThrowInterpreterError($"Range Statement Given Too Many Parameters [{rangeParams.Length}]", flags);
            return new RangeProfile();
        }

        if (rangeParams.Length == 1)
        {
            (bool success, double value) param1 = Calculate(rangeParams[0], new FlagProfile(flags.debug, true, flags.hideError));
            if (!param1.success) return new RangeProfile();
            return new RangeProfile(0, (int)Math.Abs(param1.value), "");
        }
        if (rangeParams.Length == 2)
        {
            (bool success, double value) param1 = Calculate(rangeParams[0], new FlagProfile(flags.debug, true, flags.hideError));
            if (!param1.success) return new RangeProfile();

            (bool success, double value) param2 = Calculate(rangeParams[1], new FlagProfile(flags.debug, true, true));
            if (param2.success)
            {
                if ((int)Math.Abs(param2.value) <= (int)Math.Abs(param1.value))
                {
                    ThrowInterpreterError($"Range End [{(int)Math.Abs(param2.value)}] Must Be Larger Then Range Start [{(int)Math.Abs(param1.value)}]");
                    return new RangeProfile();
                }
                return new RangeProfile((int)Math.Abs(param1.value), (int)Math.Abs(param2.value), "");
            }
            else
            {
                if (!Assign($"{rangeParams[1]}=0", new FlagProfile(flags.debug, true, flags.hideError)))
                {
                    return new RangeProfile();
                }

                return new RangeProfile(0, (int)Math.Abs(param1.value), rangeParams[1]);
            }
        }
        if (rangeParams.Length == 3)
        {
            (bool success, double value) param1 = Calculate(rangeParams[0], new FlagProfile(flags.debug, true, flags.hideError));
            if (!param1.success) return new RangeProfile();
            (bool success, double value) param2 = Calculate(rangeParams[1], new FlagProfile(flags.debug, true, flags.hideError));
            if (!param2.success) return new RangeProfile();

            if ((int)Math.Abs(param2.value) <= (int)Math.Abs(param1.value))
            {
                ThrowInterpreterError($"Range End [{(int)Math.Abs(param2.value)}] Must Be Larger Then Range Start [{(int)Math.Abs(param1.value)}]");
                return new RangeProfile();
            }

            if (!Assign($"{rangeParams[2]}={(int)Math.Abs(param1.value)}", new FlagProfile(flags.debug, true, flags.hideError)))
            {
                return new RangeProfile();
            }

            return new RangeProfile((int)Math.Abs(param1.value), (int)Math.Abs(param2.value), rangeParams[2]);
        }

        return new RangeProfile();
    }

    public static bool CreateOutput(string output, FlagProfile flags)
    {
        bool inString = false;
        string currentStatement = "";
        string finalOutput = "";

        foreach (char c in output)
        {
            if (c == '"')
            {
                if (currentStatement != "")
                {
                    if (inString) finalOutput += currentStatement;
                    else
                    {
                        (bool success, double value) = Calculate(currentStatement, new FlagProfile(flags.debug, true, flags.hideError));
                        if (!success) return false;

                        finalOutput += value.ToString();
                    }
                }


                currentStatement = "";
                inString = !inString;
                continue;
            }

            currentStatement += c;
        }

        if (currentStatement != "")
        {
            if (inString) { ThrowInterpreterError($"String [{currentStatement}] Never Closed"); return false; }

            (bool success, double value) = Calculate(currentStatement, new FlagProfile(flags.debug, true, flags.hideError));
            if (!success) return false;

            finalOutput += value.ToString();
        }

        Output(finalOutput, FlagProfile.None);
        return true;
    }
}