using Revistone.App.BaseApps.HoneyC.Data;

using Revistone.Functions;

namespace Revistone.App.BaseApps.HoneyC;

/// <summary> Class responsible for interpreting user querys. </summary>
public static class HoneyCInterpreter
{
    public static void Interpret(string[] query)
    {
        Diagnostics.Start(); // start diagnositcs
        ProgramData.Wipe(); // clear any previous variable and function data

        Diagnostics.Output("HoneyC Is Still In Development, So Expect Programs To Have Issues, Or Not Run Atall!", false, true, true);   

        string cleanedQuery = HoneyCLexer.CleanQuery(query); // remove comments and join lines

        List<Token> tokenisedInput = HoneyCLexer.Lex(cleanedQuery); // converts query to list of tokens
        if (!Diagnostics.Running) return;  

        List<TokenGroup> tokenGroups = HoneyCParser.Parse(tokenisedInput); // converts list of tokens to purposed lines
        if (!Diagnostics.Running) return;

        Diagnostics.Output("--- Program Info ---", true, true);
        Diagnostics.Finish();
    }
}