using Revistone.Apps.HoneyC.Data;

using Revistone.Functions;

namespace Revistone.Apps.HoneyC;

/// <summary> Class responsible for interpreting user querys. </summary>
public static class HoneyCInterpreter
{
    public static void Interpret(string[] query)
    {
        Diagnostics.Start();
        ProgramData.Wipe();

        string cleanedQuery = HoneyCLexer.Cleaned(query);

        List<Token> tokenisedInput = HoneyCLexer.Lex(cleanedQuery);
        if (!Diagnostics.Running) return;  

        Diagnostics.Output("--- Tokens Generated ---", true);
        Diagnostics.Output($"Total - {tokenisedInput.Count()}");
        Diagnostics.Output(tokenisedInput.ToElementString());

        List<TokenGroup> tokenGroups = HoneyCParser.Parse(tokenisedInput);
        if (!Diagnostics.Running) return;

        Diagnostics.Output("--- Program Info ---", true, true);
        Diagnostics.Finish();
    }
}