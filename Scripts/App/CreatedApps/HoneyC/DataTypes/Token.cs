using System.ComponentModel;

namespace Revistone.Apps.HoneyC.Data;

public abstract class AbstractToken
{
    public enum TokenType { None, Value, Identifier, Keyword, AssignmentOperator, ComparativeOperator, MathOperator, Scope, Split, Nest, Compound };
    public enum TokenGroupType { None, Line, LineLoopStart, LineLoopEnd, Calculation, Function, FunctionCall, Assignment, Object };

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
}