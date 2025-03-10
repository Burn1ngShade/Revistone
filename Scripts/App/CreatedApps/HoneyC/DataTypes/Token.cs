using Revistone.Functions;

namespace Revistone.Apps.HoneyC.Data;

public abstract class AbstractToken
{
    public enum TokenType { None, Value, Identifier, Keyword, AssignmentOperator, ComparativeOperator, MathOperator, LogicalOperator, Scope, Split, Nest, Compound, BitwiseOperator };
    public enum TokenGroupType { None, LineLoopEnd, Calculation, Function, FunctionCall, Assignment, Object, Enum, Import };

    public bool IsGroup { get; protected set; }
}

public class Token : AbstractToken
{
    public TokenType type;
    public string content;

    public Token(string content, TokenType type)
    {
        IsGroup = false;

        this.content = content;
        this.type = type;
    }

    public override string ToString()
    {
        return $"{type} - {content}";
    }
}

public class TokenGroup : AbstractToken
{
    public TokenGroupType type;
    public List<AbstractToken> content;

    public TokenGroup(List<AbstractToken> content, TokenGroupType type)
    {
        IsGroup = true;

        this.content = content;
        this.type = type;
    }

    public override string ToString()
    {
        string output = "";
        foreach (AbstractToken t in content)
        {
            if (t.IsGroup) output += t.ToString();
            else output += $"{((Token)t).content} ";
        }
        return output;
    }

    public string[] ToGroupString()
    {
        List<string> groupString = [];
        GenerateGroupStringLayer(content, 0, 0);
        if (!groupString[^1].Contains('^')) groupString.RemoveAt(groupString.Count - 1);
        groupString.Reverse();
        return [.. groupString];

        void GenerateGroupStringLayer(List<AbstractToken> c, int layer, int startLength)
        {
            int lli = 0;
            if (groupString.Count == layer) groupString.Add(new string(' ', startLength));
            else groupString[layer] += new string(' ', startLength);
            foreach (AbstractToken t in c)
            {
                if (t.IsGroup)
                {
                    GenerateGroupStringLayer(((TokenGroup)t).content, layer + 1, groupString[layer].Length - lli);
                    groupString[layer] += $"{new string('^', (t.ToString() ?? "").Length - 1)} ";
                    lli = groupString[layer].Length;
                }
                else groupString[layer] += $"{new string(' ', ((Token)t).content.Length)} ";
            }
        }
    }

    public List<Token> ToTokenList()
    {
        List<Token> tokens = [];

        foreach (AbstractToken t in content)
        {
            if (t.IsGroup) tokens.AddRange(((TokenGroup)t).ToTokenList());
            else tokens.Add((Token)t);
        }

        return tokens;
    }

    public TokenGroup SubGroup(int start, int length)
    {
        return new TokenGroup([.. content.GetRange(start, length)], TokenGroupType.None);
    }

    public bool MergeToToken(int start, int length, TokenType type)
    {
        string tokContent = "";
        for (int i = start; i < start + length; i++)
        {
            if (content[i] is Token t) tokContent += t.content;
            else return false;
        }
        content[start] = new Token(tokContent, type);
        content.RemoveRange(start + 1, length - 1);
        return true;
    }

    public void MergeToTokenGroup(int start, int length, TokenGroupType type)
    {
        List<AbstractToken> tokens = [];
        for (int i = start; i < start + length; i++)
        {
            tokens.Add(content[i]);
        }
        content[start] = new TokenGroup(tokens, type);
        content.RemoveRange(start + 1, length - 1);
    }

    /*
    Very important function calculates if a given line meets a given format syntax.
    : -> OR, [x] -> Command e.g. a loop, <x> -> Type, <X> -> GroupType
    [LP] -> Loop, [OP] -> Optional, [ELP] -> End Loop, [EOP] -> End Optional
    */
    public bool InFormat(string format, bool dump = false)
    {
        string[] formatParts = format.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        FormatInfo fi = new();
        while (fi.fIndex < formatParts.Length)
        {
            if (dump)
            {
                fi.Dump();
                Diagnostics.Output($"content: {(fi.cIndex < content.Count ? content[fi.cIndex] : "NONE")} <-> format: {formatParts[fi.fIndex]}");
            }

            if (fi.TryHandleCommand(formatParts[fi.fIndex]))
            {
                fi.fIndex++;
                continue;
            }

            if (fi.ShouldSkip())
            {
                fi.fIndex++;
                continue;
            }

            if (fi.cIndex >= content.Count)
            {
                if (!fi.HandleNoMatch())
                {
                    if (dump) Diagnostics.Output("Exit as cIndex is greater than content count");
                    return false;
                }
                else
                {
                    fi.fIndex++;
                    fi.cIndex++;
                    continue;
                }
            }

            string[] partOpt = formatParts[fi.fIndex].Split(':', StringSplitOptions.RemoveEmptyEntries);

            bool match = false;
            foreach (string s in partOpt) // split into each or statement
            {
                TokenType t = Definitions.GetTokenTypeAbr(s);
                if (t != TokenType.None) // if its a token lets check type
                {
                    if (content[fi.cIndex] is Token token && token.type == t)
                    {
                        match = true;
                        break;
                    }
                    continue;
                }

                TokenGroupType tg = Definitions.GetTokenGroupTypeAbr(s);
                if (tg != TokenGroupType.None) // if its a tokengroup lets check type
                {
                    if (content[fi.cIndex] is TokenGroup token && token.type == tg)
                    {
                        match = true;
                        break;
                    }
                    continue;
                }

                if (content[fi.cIndex] is Token tok && tok.content == s)
                {
                    match = true;
                    break;
                }
            }
            if (!match)
            {
                if (!fi.HandleNoMatch()) return false;
                else{
                    fi.fIndex++;
                    continue;
                }
            }

            fi.HandleMatch();

            fi.fIndex++;
            fi.cIndex++;
        }

        if (fi.cIndex < content.Count) return false;

        return true;
    }

    struct FormatInfo()
    {
        public int cIndex = 0;
        public int fIndex = 0;

        public enum FormatState { None, Start, InProgress, Skip }
        public FormatState optionalState = FormatState.None;
        public Stack<(int index, FormatState state)> loopStack = new();

        internal bool TryHandleCommand(string command)
        {
            switch (command)
            {
                case "[LP]":
                    loopStack.Push((fIndex, optionalState == FormatState.Skip ? FormatState.Skip : FormatState.Start));
                    return true;
                case "[ELP]":
                    (int index, FormatState state) = loopStack.Pop();
                    if (state == FormatState.Skip) return true;
                    else
                    {
                        fIndex = index;
                        loopStack.Push((index, FormatState.Start));
                    }
                    return true;
                case "[OP]":
                    optionalState = FormatState.Start;
                    return true;
                case "[EOP]":
                    optionalState = FormatState.None;
                    return true;
                default:
                    return false;
            }
        }

        internal void Dump()
        {
            Diagnostics.Output($"cIndex: {cIndex}, fIndex: {fIndex}, optionalState: {optionalState}, loopStack: {string.Join(", ", loopStack.ToList().ToElementString())}");
        }

        internal void HandleMatch()
        {
            if (optionalState == FormatState.Start) optionalState = FormatState.InProgress;
            if (loopStack.Count > 0 && loopStack.Peek().state == FormatState.Start) loopStack.Push((loopStack.Pop().index, FormatState.InProgress));
        }

        internal bool HandleNoMatch()
        {
            if (loopStack.Count > 0)
            {
                if (loopStack.Peek().state == FormatState.InProgress) return false;
                else loopStack.Push((loopStack.Pop().index, FormatState.Skip));
            }
            else if (optionalState == FormatState.Start) optionalState = FormatState.Skip;
            else return false;
            return true;
        }

        internal bool ShouldSkip()
        {
            if ((loopStack.Count > 0 && loopStack.Peek().state == FormatState.Skip) || optionalState == FormatState.Skip) return true;
            return false;
        }
    }
}