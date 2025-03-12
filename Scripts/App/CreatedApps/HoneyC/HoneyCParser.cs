using System.Diagnostics;
using System.Runtime.Serialization;
using Revistone.App.HoneyC.Data;
using Revistone.Console;
using Revistone.Functions;
using static Revistone.App.HoneyC.Data.AbstractToken;

namespace Revistone.App.HoneyC;

/// <summary> Class responsible for converting tokens to set of tokenGroups, and carries out syntax checks. </summary>
public static class HoneyCParser
{
    public static List<TokenGroup> Parse(List<Token> tokens)
    {
        List<TokenGroup> groups = [];

        // --- Convert tokens to lines and establish scopes ---

        Stack<int> currentScopes = new();
        List<(int start, int end)> scopes = new();
        TokenGroup currentGroup = new([], TokenGroupType.None);
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
                currentGroup = new([], TokenGroupType.None);
            }
        }

        if (currentGroup.content.Count != 0) return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Missing ';'", groups.Count, currentGroup.ToTokenList().Count - 1, currentGroup.ToTokenList());

        if (currentScopes.Count != 0)
            return Diagnostics.ThrowError<TokenGroup>("Syntax_Exception", "Unmatched '{'", currentScopes.Peek(), groups[currentScopes.Peek()].ToTokenList().Count - 1, groups[currentScopes.Peek()].ToTokenList());

        ProgramData.SetScopeData(scopes);

        Diagnostics.Output("--- Token Lines ---", true);
        Diagnostics.Output("Total - " + groups.Count);
        for (int i = 0; i < groups.Count; i++) Diagnostics.Output($"{(ProgramData.IsScopeStart(i) ? "[S]" : ProgramData.IsScopeEnd(i) ? "[E]" : "   ")}{new string(' ', 4 - (i + 1).ToString().Length)}{i + 1}. {groups[i]}");

        // --- Lets now work out each groups function ---

        for (int i = 0; i < groups.Count; i++)
        {
            TokenGroup g = groups[i];

            for (int j = 0; j < g.content.Count - 2; j++) // merge all multi step variables into one token
            {
                if (g.SubGroup(j, 3).InFormat("<id>:this . <id>"))
                {
                    g.MergeToToken(j, 3, TokenType.Identifier);
                    j--;
                }
            }
        }

        Diagnostics.Output("--- Bracket Data ---", true);

        for (int i = 0; i < groups.Count; i++) // now repeated merges of function calls and calculations
        {
            bool success = true;
            TokenGroup? g = GroupTokens(groups[i].ToTokenList(), i, ref success);
            if (g == null) return [];
            groups[i] = g;
        }

        Diagnostics.Output("--- Grouped Token Lines ---", true);
        Diagnostics.Output("Total - " + groups.Count);
        for (int i = 0; i < groups.Count; i++)
        {
            Diagnostics.Output($"{(ProgramData.IsScopeStart(i) ? "[S]" : ProgramData.IsScopeEnd(i) ? "[E]" : "   ")}{new string(' ', 4 - (i + 1).ToString().Length)}{i + 1}. {groups[i]}");
            foreach (string s in groups[i].ToGroupString()) Diagnostics.Output(new string(' ', 9) + s);
        }

        // now we finally give each line a function yay

        for (int i = 0; i < groups.Count; i++)
        {
            TokenGroup g = groups[i];

            if (g.InFormat("[OP] var:val [EOP] <id> [OP] <mop> [EOP] = <val>:<id>:<CALL>:<CALC> ;")) groups[i].type = TokenGroupType.Assignment;
            else if (g.InFormat("var:val <id> ;")) groups[i].type = TokenGroupType.Assignment;
            else if (g.InFormat("<CALL> ;")) groups[i].type = TokenGroupType.FunctionCall;
            else if (g.InFormat("}")) groups[i].type = TokenGroupType.LineLoopEnd;
            else if (g.InFormat("func <FUNC> {")) groups[i].type = TokenGroupType.Function;
            else if (g.InFormat("obj <id> {")) groups[i].type = TokenGroupType.Object;
            else if (g.InFormat("enum <id> {")) groups[i].type = TokenGroupType.Enum;
            else if (g.InFormat("import <val> ;")) groups[i].type = TokenGroupType.Import;
        }

        Diagnostics.Output("--- Purposed Token Lines ---", true);
        Diagnostics.Output("Total - " + groups.Count);
        for (int i = 0; i < groups.Count; i++)
        {
            Diagnostics.Output($"{groups[i].type}{new string(' ', 14 - groups[i].type.ToString().Length)}{(ProgramData.IsScopeStart(i) ? "[S]" : ProgramData.IsScopeEnd(i) ? "[E]" : "   ")}{new string(' ', 4 - (i + 1).ToString().Length)}{i + 1}. {groups[i]}");
            foreach (string s in groups[i].ToGroupString()) Diagnostics.Output(new string(' ', 23) + s);
        }

        return groups;
    }

    public static TokenGroup? GroupTokens(List<Token> g, int lineNumber, ref bool success)
    {
        Stack<int> openBrackets = [];
        Stack<int> openSquareBrackets = [];
        int totalDepth = 0;
        List<(int index, int endIndex, int layer, bool isSquare)> brackets = [];

        for (int j = 0; j < g.Count; j++)
        {
            if (g[j].type != TokenType.Nest && g[j].type != TokenType.Compound) continue;

            if (g[j].content == "[")
            {
                openSquareBrackets.Push(j);
                totalDepth++;
            }
            else if (g[j].content == "]")
            {
                if (openSquareBrackets.Count == 0)
                {
                    success = false;
                    return Diagnostics.ThrowNullError<TokenGroup>("Syntax_Exception", "Unmatched ']'", lineNumber, j, g);
                }

                totalDepth--;
                brackets.Add((openSquareBrackets.Pop(), j, totalDepth, true));
            }
            else if (g[j].content == "(")
            {
                openBrackets.Push(j);
                totalDepth++;
            }
            else if (g[j].content == ")")
            {
                if (openBrackets.Count == 0)
                {
                    success = false;
                    return Diagnostics.ThrowNullError<TokenGroup>("Syntax_Exception", "Unmatched ')'", lineNumber, j, g);
                }

                totalDepth--;
                brackets.Add((openBrackets.Pop(), j, totalDepth, false));
            }
        }

        brackets = [.. brackets.OrderByDescending(x => x.layer)];
        if (openBrackets.Count != 0)
        {
            success = false;
            return Diagnostics.ThrowNullError<TokenGroup>("Syntax_Exception", "Unmatched '('", lineNumber, openBrackets.Peek(), g);
        }
        if (openSquareBrackets.Count != 0)
        {
            success = false;
            return Diagnostics.ThrowNullError<TokenGroup>("Syntax_Exception", "Unmatched '['", lineNumber, openSquareBrackets.Peek(), g);
        }

        TokenGroup newTokenGroup = new([.. g.Select(x => (AbstractToken)x)], TokenGroupType.None);
        while (brackets.Count > 0)
        {
            (int start, int end, int layer, bool isSquare) = brackets[0];

            bool isFuncCall = start != 0 && newTokenGroup.content[start - 1] is Token t && t.type == TokenType.Identifier;

            if (isFuncCall) // possible func call
            {
                bool isFuncDef = start > 1 && newTokenGroup.content[start - 2] is Token t2 && (t2.content == "func" || t2.content == "obj");
                Diagnostics.Output($"Bracket - Start: {start}, End: {end}, Line: {lineNumber + 1}, IsFuncCall: {isFuncCall}, IsFuncDef: {isFuncDef}, IsSquare {isSquare}");

                if (isFuncDef)
                {
                    if (newTokenGroup.SubGroup(start + 1, end - start - 1).InFormat("[OP] <id> [LP] , <id> [ELP] [EOP]"))
                    {
                        newTokenGroup.MergeToTokenGroup(start - 1, end - start + 2, TokenGroupType.Function);

                        for (int j = 1; j < brackets.Count; j++)
                        {
                            brackets[j] = (brackets[j].index > start - 1 ? (brackets[j].index - (end - start + 1)) : brackets[j].index,
                            brackets[j].endIndex > (brackets[j].index - (end - start + 1)) ? (brackets[j].endIndex - (end - start + 1)) : brackets[j].endIndex, brackets[j].layer, brackets[j].isSquare);
                        }
                    }
                }
                else
                {
                    // we might be missing some calculations here so lets identify that sht

                    int currentStreak = 0;
                    for (int k = end - 1; k > start; k--)
                    {
                        if (newTokenGroup.content[k] is Token t3 && (t3.type == TokenType.MathOperator || t3.type == TokenType.Value || t3.type == TokenType.Identifier)) currentStreak++;
                        else if (newTokenGroup.content[k] is TokenGroup tg && (tg.type == TokenGroupType.Calculation || tg.type == TokenGroupType.FunctionCall)) currentStreak++;
                        else
                        {
                            if (currentStreak > 1)
                            {
                                newTokenGroup.MergeToTokenGroup(k + 1, currentStreak, TokenGroupType.Calculation);
                                for (int j = 0; j < brackets.Count; j++)
                                {
                                    brackets[j] = (brackets[j].index > k + 1 ? (brackets[j].index - (currentStreak - 1)) : brackets[j].index,
                                    brackets[j].endIndex > (k + 1 + currentStreak) ? (brackets[j].endIndex - (currentStreak - 1)) : brackets[j].endIndex, brackets[j].layer, brackets[j].isSquare);
                                }
                            }
                            currentStreak = 0;
                        }
                    }

                    if (currentStreak > 1)
                    {
                        newTokenGroup.MergeToTokenGroup(start + 1, currentStreak, TokenGroupType.Calculation);
                        for (int j = 0; j < brackets.Count; j++)
                        {
                            brackets[j] = (brackets[j].index > start ? (brackets[j].index - (currentStreak - 1)) : brackets[j].index,
                            brackets[j].endIndex > (start + currentStreak) ? (brackets[j].endIndex - (currentStreak - 1)) : brackets[j].endIndex, brackets[j].layer, brackets[j].isSquare);
                        }
                    }

                    (start, end, layer, isSquare) = brackets[0];

                    if (newTokenGroup.SubGroup(start + 1, end - start - 1).InFormat("[OP] <id>:<val>:<CALL>:<CALC> [LP] ,  <id>:<val>:<CALL>:<CALC> [ELP] [EOP]"))
                    {
                        newTokenGroup.MergeToTokenGroup(start - 1, end - start + 2, TokenGroupType.FunctionCall);
                        for (int j = 1; j < brackets.Count; j++)
                        {
                            brackets[j] = (brackets[j].index > start - 1 ? (brackets[j].index - (end - start + 1)) : brackets[j].index,
                            brackets[j].endIndex > (brackets[j].index - (end - start + 1)) ? (brackets[j].endIndex - (end - start + 1)) : brackets[j].endIndex, brackets[j].layer, brackets[j].isSquare);
                        }
                    }
                }
            }
            else // possible calculation
            {
                Diagnostics.Output($"Bracket - Start: {start}, End: {end}, Line: {lineNumber + 1}, IsFuncCall: {isFuncCall}, IsSquare {isSquare}");

                if (newTokenGroup.SubGroup(start + 1, end - start - 1).InFormat("<mop>:<val>:<id>:<CALL>:<CALC> [LP] <mop>:<val>:<id>:<CALL>:<CALC> [ELP]"))
                {
                    newTokenGroup.MergeToTokenGroup(start, end - start + 1, TokenGroupType.Calculation);
                    for (int j = 1; j < brackets.Count; j++)
                    {
                        brackets[j] = (brackets[j].index > start ? (brackets[j].index - (end - start)) : brackets[j].index,
                        brackets[j].endIndex > (brackets[j].index - (end - start)) ? (brackets[j].endIndex - (end - start)) : brackets[j].endIndex, brackets[j].layer, brackets[j].isSquare);
                    }
                }
            }

            brackets.RemoveAt(0);
        }

        int cs = 0;
        for (int k = newTokenGroup.content.Count - 1; k >= 0; k--)
        {
            if (newTokenGroup.content[k] is Token t3 && (t3.type == TokenType.MathOperator || t3.type == TokenType.Value || t3.type == TokenType.Identifier)) cs++;
            else if (newTokenGroup.content[k] is TokenGroup tg && (tg.type == TokenGroupType.Calculation || tg.type == TokenGroupType.FunctionCall)) cs++;
            else
            {
                if (cs > 1)
                {
                    newTokenGroup.MergeToTokenGroup(k + 1, cs, TokenGroupType.Calculation);
                }
                cs = 0;
            }
        }

        if (cs > 1)
        {
            newTokenGroup.MergeToTokenGroup(0, cs, TokenGroupType.Calculation);
        }

        return newTokenGroup;
    }
}