using Revistone.Console;
using Revistone.Functions;

using static Revistone.Apps.HoneyC.HoneyCSyntax;
using static Revistone.Apps.HoneyC.HoneyCTerminalApp;

namespace Revistone.Apps.HoneyC;

/// <summary> Responsible for converting query into set of Tokens. </summary>
public static class HoneyCLexer
{
    /// <summary> Converts given query into list of Tokens. </summary>
    public static List<Token> Lex(string query, InterpreterDiagnostics diagnostics, InterpreterSettings settings)
    {
        List<Token> tokens = [];

        (string content, bool isLiteral, bool isIdentifier) currentToken = ("", false, false);

        foreach (char c in query)
        {
            if (c == ' ' && !currentToken.content.StartsWith("\""))
            {
                if (currentToken.content.Length != 0) tokens.Add(GenerateToken(currentToken));
                currentToken = ("", false, false);
                continue;
            }

            (string content, bool isLiteral, bool isIdentifier) newToken = (currentToken.content + c, IsLiteral(currentToken.content + c), IsIdentifier(currentToken.content + c));

            if (currentToken.isLiteral && !newToken.isLiteral)
            {
                tokens.Add(new Token(currentToken.content, TokenType.Literal));
                currentToken = (c.ToString(), IsLiteral(c.ToString()), IsIdentifier(c.ToString()));
            }
            else if (currentToken.isIdentifier && !newToken.isIdentifier)
            {
                tokens.Add(new Token(currentToken.content, TokenType.Identifier));
                currentToken = (c.ToString(), IsLiteral(c.ToString()), IsIdentifier(c.ToString()));
            }
            else
            {
                currentToken = newToken;
            }

            Token syntaxToken;
            if (tokens.Count > 0)
            {
                syntaxToken = GenerateSyntaxToken(tokens[^1].content + currentToken.content);
                if (syntaxToken.type != TokenType.Invalid)
                {
                    tokens[^1] = syntaxToken;
                    currentToken = ("", false, false);
                    continue;
                }
            }

            syntaxToken = GenerateSyntaxToken(currentToken.content);
            if (syntaxToken.type != TokenType.Invalid)
            {
                tokens.Add(syntaxToken);
                currentToken = ("", false, false);
            }
            continue;
        }

        if (currentToken.content.Length != 0) tokens.Add(GenerateToken(currentToken));

        // --- Function call tokens ---

        // List<(int start, int end)> tokensToReplace = [];
        // Stack<int> currentTokenCallFunctions = [];
        // int index = 0;
        // int bracketIndex = 0;
        // bool skipLine = false;
        // while (index < tokens.Count - 1)
        // {
        //     if (tokens[index].content == "func") skipLine = true;
        //     else if (tokens[index].type == TokenType.Scope) skipLine = false;

        //     if (skipLine)
        //     {
        //         index++;
        //         continue;
        //     }

        //     if (tokens[index].type == TokenType.Identifier && tokens[index + 1].content == "(")
        //     {
        //         currentTokenCallFunctions.Push(index);
        //         index++;
        //         bracketIndex = 0;
        //         continue;
        //     }

        //     if (tokens[index].content == "(") bracketIndex++;

        //     if (tokens[index].content == ")")
        //     {
        //         bracketIndex--;

        //         if (bracketIndex == 0)
        //         {
        //             int start = currentTokenCallFunctions.Peek();
        //             TokenStatement st = new TokenStatement(tokens.Skip(start + 2).Take(index - 1 - start).ToList());

        //             if (st.InFormat("[OP] [LOOP] <ID|LIT|MOP|GRP> [EZLOOP] [LOOP] , <ID|LIT|MOP> [LOOP] <ID|LIT|MOP> [ELOOP] [ERZLOOP] [EOP]"))
        //             {
        //                 tokensToReplace.Add((start, index));
        //             }
        //         }
        //     }

        //     index++;
        // }

        // DebugOutput(tokensToReplace.ToElementString());

        // --- DEBUG ---

        diagnostics.IncrementStage();
        if (settings.GeneralDebugOutput)
        {
            DebugHeaderOutput($"[Interpreter] - Lexer Stage Completed");
            DebugOutput($"Time Taken: {diagnostics.GetStageElapsedTime(InterpreterDiagnostics.InterpreterStage.Lexer)} Milliseconds");
            DebugOutput($"Total Tokens Generated: {tokens.Count}");
        }
        if (settings.DetailedLexerDebugOutput)
        {
            foreach (Token t in tokens) DebugOutput(t.ToString());
        }

        return tokens;
    }

    static Token GenerateToken((string content, bool isLiteral, bool isIdentifier) t)
    {
        if (t.isLiteral) return new Token(t.content, TokenType.Literal);
        if (t.isIdentifier) return new Token(t.content, TokenType.Identifier);

        return GenerateSyntaxToken(t.content);
    }

    static Token GenerateSyntaxToken(string content)
    {
        foreach (var s in syntax)
        {
            if (s.Key.Contains(content)) return new Token(content, s.Value);
        }

        return new Token(content, TokenType.Invalid);
    }

    static bool IsLiteral(string s)
    {
        if (s.Contains(',')) return false;
        if (NumericalFunctions.IsNumber(s)) return true;
        if (StringFunctions.InFormat(s, "\"[A:]\"")) return true;

        return false;
    }

    static bool IsIdentifier(string s)
    {
        return StringFunctions.InFormat(s, "[C:]");
    }
}