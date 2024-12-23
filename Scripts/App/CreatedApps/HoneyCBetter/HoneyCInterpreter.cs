using System.Collections.Immutable;
using System.Diagnostics;
using Revistone.Console;
using Revistone.Functions;
using static Revistone.Apps.HoneyC.HoneyCTerminalApp;
using static Revistone.Apps.HoneyC.HoneyCVariable;

namespace Revistone.Apps.HoneyC;

public static class HoneyCInterpreter
{
    public static void Interpret(string query)
    {
        InterpreterDiagnostics diagnostics = new InterpreterDiagnostics();
        InterpreterSettings settings = new InterpreterSettings(true, false, true, true, false);

        HoneyCSyntax.ClearMemory();
        HoneyCProgram? program = HoneyCParser.Parse(HoneyCLexer.Lex(query, diagnostics, settings), diagnostics, settings);

        if (program == null) return;

        // --- Actually Run Program ---

        DebugHeaderOutput("-------");

        int i = 0;
        while (i < program.statements.Count)
        {
            TokenStatement s = program.statements[i];

            DebugOutput(s);

            if (s.tokens[0].content == "func") // skip over functions
            {
                if (!program.scopes.ContainsKey(i))
                {
                    ErrorOutput(HoneyCError.InvalidSyntax, "Invalid Function Scope", diagnostics, s, (i + 1, 1));
                    return;
                }
                i = program.scopes[i] + 1;
                i++;
                continue;
            }

            if (s.tokens[0].content == "var" || s.tokens[0].content == "val") // variable assignement
            {
                if (!s.InFormat("<KEY> <ID> = <LIT|ID|FLW|GRP> [OP] [LOOP] <LIT|ID|FLW|MOP|GRP> [ELOOP] [EOP] ;"))
                {
                    ErrorOutput(HoneyCError.InvalidSyntax, "Invalid Variable Assignment", diagnostics, s, (i + 1, 1));
                    return;
                }

                (object val, VariableType type) = HoneyCEvaluator.Evaluate(s, 2, s.tokens.Count - 1, i + 1, diagnostics);

                if (type == VariableType.Invalid) return;
            }

            i++;
        }

        /// --- DEBUG ---

        diagnostics.IncrementStage();
        if (settings.GeneralDebugOutput)
        {
            DebugHeaderOutput("[Interpreter] - Running Stage Completed");
            DebugOutput($"Time Taken: {diagnostics.GetStageElapsedTime(InterpreterDiagnostics.InterpreterStage.Running)} Milliseconds");

            DebugHeaderOutput("[Interpreter] - Program Complete");
            DebugOutput($"Time Taken: {diagnostics.GetTotalElapsedTime()} Milliseconds");
        }
    }
}

// --- DIAGNOSTICS ---

public class InterpreterDiagnostics
{
    public enum InterpreterStage { Lexer, Parser, Running, Finished }
    public InterpreterStage CurrentStage { get; private set; } = InterpreterStage.Lexer;

    Stopwatch stageTimer = new();
    long[] stageTicks = new long[3];

    // --- CONSTRUCTORS ---

    public InterpreterDiagnostics()
    {
        stageTimer.Start();
    }

    // --- METHODS ---

    public void IncrementStage()
    {
        stageTicks[(int)CurrentStage] = stageTimer.ElapsedTicks;
        CurrentStage++;
    }

    /// <summary> Returns given stages completion time in milliseconds. </summary>
    public double GetStageElapsedTime(InterpreterStage stage)
    {
        return Math.Round((stageTicks[(int)stage] - (stage == 0 ? 0 : stageTicks[(int)stage - 1])) / 10000f, 2);
    }

    /// <summary> Returns total elapsed time in milliseconds. </summary>
    public double GetTotalElapsedTime()
    {
        return Math.Round(stageTimer.ElapsedTicks / 10000f, 2);
    }
}

public class InterpreterSettings
{
    public bool GeneralDebugOutput { get; private set; }
    public bool DetailedLexerDebugOutput { get; private set; }
    public bool DetailedParserDebugOutput { get; private set; }
    public bool DetailedRunningDebugOutput { get; private set; }

    public bool SimpleMathSyntax { get; private set; }

    public InterpreterSettings(bool debugOutput, bool lexerDebugOutput, bool parserDebugOutput, bool runningDebugOutput, bool simpleMathSyntax)
    {
        GeneralDebugOutput = debugOutput;
        DetailedLexerDebugOutput = lexerDebugOutput;
        DetailedParserDebugOutput = parserDebugOutput;
        DetailedRunningDebugOutput = runningDebugOutput;
        SimpleMathSyntax = simpleMathSyntax;
    }
}