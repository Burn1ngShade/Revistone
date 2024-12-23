using System.IO.Compression;
using Revistone.Functions;
using static Revistone.Apps.HoneyC.HoneyCSyntax;
namespace Revistone.Apps.HoneyC;

/// <summary> Statement (line) of Tokens. </summary>
public class TokenStatement
{
public List<Token> tokens;

    public TokenStatement(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    /// <summary> Verifys if TokenStatement is in a given format. <TokenType> [STATE] </summary>
    public bool InFormat(string format)
    {
        string[] s = format.Trim().Split(" ");

        FormatData data = new();

        while (data.formatIndex < s.Length)
        {

            if (data.UpdateFlow(s[data.formatIndex]) || data.optionalState == FormatData.OptionalState.Skipped)
            {
                data.formatIndex++;
                continue;
            }

            TokenType[] tokenTypes = GetTokenTypes(s[data.formatIndex]);

            if (tokenTypes.Length != 0)
            {
                if (data.tokenIndex >= tokens.Count || !tokenTypes.Contains(tokens[data.tokenIndex].type))
                {
                    if (data.TryOptionalSkip()) continue;
                    return false;
                }
            }
            else
            {
                if (data.tokenIndex >= tokens.Count || s[data.formatIndex] != tokens[data.tokenIndex].content)
                {
                    if (data.TryOptionalSkip()) continue;
                    return false;
                }
            }

            if (data.optionalState == FormatData.OptionalState.Optional) data.optionalState = FormatData.OptionalState.PartialOptional;
            data.tokenIndex++;
            data.formatIndex++;
        }

        return true;
    }

    struct FormatData()
    {
        public enum OptionalState { None, Optional, PartialOptional, Skipped }

        public OptionalState optionalState = OptionalState.None;
        // int loopState = 0;
        // int loopStartIndex = 0;

        Stack<(int formatIndex, int tokenIndex)> loops = new();

        public int tokenIndex = 0;
        public int formatIndex = 0;

        public bool UpdateFlow(string s)
        {
            int i = Array.IndexOf(tokenTypeFlowCode, s);
            if (i == -1) return false;

            if (i == 0) optionalState = OptionalState.Optional;
            else if (i == 1) optionalState = OptionalState.None;
            else if (i == 2)
            {
                if (optionalState == OptionalState.PartialOptional) optionalState = OptionalState.Optional;
                if (loops.Count == 0 || loops.Peek().formatIndex != formatIndex) loops.Push((formatIndex, tokenIndex));
            }
            else if (i == 3)
            {
                if (optionalState != OptionalState.Skipped)
                {
                    formatIndex = loops.Peek().formatIndex - 1;
                }
                else loops.Pop();
            }
            else if (i == 4)
            {
                if (optionalState != OptionalState.Skipped)
                {
                    formatIndex = loops.Peek().formatIndex - 1;
                }
                else
                {
                    if (loops.Peek().tokenIndex != tokenIndex) optionalState = OptionalState.Optional;
                    loops.Pop();
                }
            }
            else if (i == 5)
            {
                if (optionalState != OptionalState.Skipped)
                {
                    formatIndex = loops.Peek().formatIndex - 1;
                }
                else
                {
                    if (loops.Peek().tokenIndex != tokenIndex)
                    {
                        optionalState = OptionalState.Optional;
                        int loopStart = loops.Pop().formatIndex;
                        loops.Push((loopStart, tokenIndex));
                        formatIndex = loops.Peek().formatIndex - 1;
                    }
                    else
                    {
                        loops.Pop();
                    }
                }
            }

            return true;
        }

        public bool TryOptionalSkip()
        {
            if (optionalState == OptionalState.Optional)
            {
                optionalState = OptionalState.Skipped;
                formatIndex++;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"TokenIndex: {tokenIndex}, FormatIndex: {formatIndex}, OptionalState: {optionalState}, Loops: {loops.ToArray().ToElementString()},";
        }
    }

    public override string ToString()
    {
        return string.Join(' ', tokens.Select(x => x.content));
    }
}

/// <summary> Base building blocks of the language. </summary>
public class Token
{
    public string content;
    public TokenType type;

    public Token(string content, TokenType type)
    {
        this.content = content;
        this.type = type;
    }

    public override string ToString()
    {
        return $"'{content}' - {type}";
    }
}