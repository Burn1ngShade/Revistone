using Revistone.Apps.HoneyC.Data;

using static Revistone.Apps.HoneyC.Data.AbstractToken;

namespace Revistone.Apps.HoneyC;

/// <summary> Class responsible for converting tokens to set of tokenGroups, and carries out syntax checks. </summary>
public static class HoneyCParser
{
    public static List<TokenGroup> Parse(List<Token> tokens)
    {
        List<TokenGroup> groups = [];

        // --- Convert tokens to lines and establish scopes ---

        Stack<int> currentScopes = new();
        List<(int start, int end)> scopes = new();
        TokenGroup currentGroup = new([], TokenGroupType.Line); 
        for (int i = 0; i < tokens.Count; i++)
        {
            Token t = tokens[i];
            currentGroup.content.Add(t);

            if (t.type == TokenType.Scope)
            {
                if (currentGroup.content.Count == 1 && t.content != "}")
                    return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", $"Invalid Statement", groups.Count, 0, [t]);
                else if (currentGroup.content.Count != 1 && t.content == "}")
                    return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Missing ';'", groups.Count, currentGroup.ToTokenList().Count - 1, currentGroup.ToTokenList());

                if (t.content == "{") currentScopes.Push(groups.Count);
                else if (t.content == "}")
                {
                    if (currentScopes.Count == 0)
                        return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Unmatched '}'", groups.Count, currentGroup.ToTokenList().Count - 1, currentGroup.ToTokenList());

                    scopes.Add((currentScopes.Pop(), groups.Count));
                }

                groups.Add(currentGroup);
                currentGroup = new([], TokenGroupType.Line);
            }
        }

        if (currentGroup.content.Count != 0) return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Missing ';'", groups.Count, currentGroup.ToTokenList().Count - 1, currentGroup.ToTokenList());

        if (currentScopes.Count != 0)
            return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Unmatched '{'", currentScopes.Peek(), groups[currentScopes.Peek()].ToTokenList().Count - 1, groups[currentScopes.Peek()].ToTokenList());

        ProgramData.SetLoopData(scopes);

        Diagnostics.Output("--- Token Lines ---", true);
        Diagnostics.Output("Total - " + groups.Count);
        for (int i = 0; i < groups.Count; i++) Diagnostics.Output($"{(ProgramData.IsScopeStart(i) ? "[S]" : ProgramData.IsScopeEnd(i) ? "[E]" : "   ")}{new string(' ', 4 - (i + 1).ToString().Length)}{i + 1}. {groups[i]}");

        return groups;
    }
}