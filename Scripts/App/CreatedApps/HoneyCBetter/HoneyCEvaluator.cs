using static Revistone.Apps.HoneyC.HoneyCVariable;
using static Revistone.Apps.HoneyC.HoneyCTerminalApp;

namespace Revistone.Apps.HoneyC;

// 3 * (4 / (2 - 1)) + mul(x, y);


/// <summary> Converts list of tokens into a value. </summary>
public static class HoneyCEvaluator
{
    public static (object value, VariableType type) Evaluate(TokenStatement s, int startIndex, int endIndex, int lineNumber, InterpreterDiagnostics diagnostics)
    {
        List<(int start, int end, int priority)> subStatements = [];
        Stack<int> currentBrackets = [];
        int maxPriority = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            Token token = s.tokens[i];

            if (token.content == "(") currentBrackets.Push(i);
            if (token.content == ")")
            {
                if (currentBrackets.Count == 0)
                {
                    ErrorOutput(HoneyCError.MissingSyntax, "Missing Opening Bracket", diagnostics, s, (lineNumber, i + 1));
                    return (0, VariableType.Invalid);
                }

                subStatements.Add((currentBrackets.Pop(), i, currentBrackets.Count));
                maxPriority = Math.Max(maxPriority, currentBrackets.Count);
            }
        } 

        if (currentBrackets.Count != 0)
        {
            ErrorOutput(HoneyCError.MissingSyntax, "Missing Closing Bracket", diagnostics, s, (lineNumber, currentBrackets.Peek() + 1));
            return (0, VariableType.Invalid);
        }

        // now lets do some order of operations stuff

        //(5/4) * (3 + 2*sin(1+3)/2)
        // (5/4), 0 -> (3 + 2*sin(1+3)/2)

        return (0, VariableType.Invalid);
    }

    static (object value, VariableType type) EvaluateLayer()
    {
        return (0, VariableType.Invalid);
    }
}