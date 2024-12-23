using Revistone.Functions;

using static Revistone.Apps.HoneyC.HoneyCTerminalApp;
using static Revistone.Apps.HoneyC.HoneyCSyntax;
using System.Reflection.Metadata;
using Revistone.Console;

namespace Revistone.Apps.HoneyC;

/// <summary> Responsible for converting Tokens into program class. </summary>
public static class HoneyCParser
{
    public static HoneyCProgram? Parse(List<Token> tokens, InterpreterDiagnostics diagnostics, InterpreterSettings settings)
    {
        HoneyCProgram program = new();
        List<Token> currentStatement = [];

        foreach (Token token in tokens)
        {
            currentStatement.Add(token);

            if (token.type == TokenType.Invalid) return ErrorOutput(HoneyCError.InvalidToken, "Token Can Not Be Assigned A Type", diagnostics, token, (program.statements.Count + 1, currentStatement.Count));
            if (token.type != TokenType.Scope) continue;

            if (currentStatement.Count == 1 && token.content != "}")
                return ErrorOutput(HoneyCError.InvalidSyntax, $"{token.content} Can Not Be Preceded By A Scope Token", diagnostics, token, (program.statements.Count + 1, currentStatement.Count));

            if (token.content == "}" && currentStatement.Count != 1)
                return ErrorOutput(HoneyCError.MissingSyntax, "; Expected", diagnostics,
                new TokenStatement([.. (currentStatement.Take(currentStatement.Count - 1)), new Token(";", TokenType.Invalid), currentStatement[^1]]),
                (program.statements.Count + 1, currentStatement.Count));

            program.statements.Add(new TokenStatement(currentStatement));
            currentStatement = [];
        }

        if (currentStatement.Count != 0) return ErrorOutput(HoneyCError.MissingSyntax, "; Expected", diagnostics, new TokenStatement([.. currentStatement, new Token(";", TokenType.Invalid)]), (program.statements.Count + 1, currentStatement.Count + 1));

        // --- Program Structure ---

        List<string> functionNames = [];
        Stack<int> scopes = [];
        TokenStatement? curFunction = null;
        for (int i = 0; i < program.statements.Count; i++)
        {
            TokenStatement ts = program.statements[i];

            if (ts.tokens[^1].content == "{")
            {
                scopes.Push(i);
                if (ts.InFormat("func <ID> ( [OP] <ID> [LOOP] , <ID> [ELOOP] [EOP] ) {"))
                {
                    if (functionNames.Contains(ts.tokens[1].content)) return ErrorOutput(HoneyCError.InvalidIdentifier, $"Function {ts.tokens[1].content} Already Exists", diagnostics, ts, (i + 1, 2));

                    functionNames.Add(ts.tokens[1].content);
                    curFunction = ts;
                }
            }
            else if (ts.tokens[^1].content == "}")
            {
                if (scopes.Count == 0) return ErrorOutput(HoneyCError.InvalidSyntax, "Missing { Bracket ", diagnostics, ts.tokens[^1], (i + 1, ts.tokens.Count));

                if (curFunction != null)
                {
                    string[] parameters = curFunction.tokens.Skip(3).Take(curFunction.tokens.Count - 5).Where(t => t.type != TokenType.Flow).Select(t => t.content).ToArray();
                    if (parameters.GroupBy(s => s).Any(group => group.Count() > 1))
                        return ErrorOutput(HoneyCError.InvalidIdentifier, "Duplicate Parameters", diagnostics, curFunction, (scopes.Peek() + 1, 1));
                    program.functions.Add(new HoneyCFunction(curFunction.tokens[1].content, scopes.Peek(), parameters, program.statements[(scopes.Peek() + 1)..(i)].ToArray()));
                }
                program.scopes.Add(scopes.Pop(), i);
            }
        }

        if (scopes.Count != 0)
            return ErrorOutput(HoneyCError.InvalidSyntax, "Missing } Bracket ", diagnostics, program.statements[scopes.Peek()], (scopes.Peek() + 1, program.statements[scopes.Peek()].tokens.Count));

        // --- Debug ---

        diagnostics.IncrementStage();
        if (settings.GeneralDebugOutput)
        {
            DebugHeaderOutput($"[Interpreter] - Parser Stage Completed");
            DebugOutput($"Total Time Taken: {diagnostics.GetStageElapsedTime(InterpreterDiagnostics.InterpreterStage.Parser)} Milliseconds");
            DebugOutput($"Total Token Statements Generated: {program.statements.Count}");
        }
        if (settings.DetailedParserDebugOutput)
        {
            DebugHeaderOutput($"[Token Statements]");
            foreach (TokenStatement s in program.statements)
            {
                DebugOutput(s.ToString());
            }

            DebugHeaderOutput($"[Functions]");
            foreach (HoneyCFunction f in program.functions)
            {
                DebugOutput(f.name + " : " + f.inputs.ToElementString());
                foreach (TokenStatement s in f.statements) DebugOutput(s);
            }
        }

        return program;
    }
}