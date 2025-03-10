using static Revistone.Apps.HoneyC.Data.AbstractToken;

namespace Revistone.Apps.HoneyC.Data;

/// <summary> Class responsible for holding language syntax and definitions. </summary>
public static class Definitions
{
    /// <summary> Key syntax for the HoneyC programming language. </summary>
    static string[][] syntax =
    {
        ["obj", "enum", "func", "in", "while", "for", "if", "else", "or", "and", "this", "var", "val", "import", "true", "false", "null"], //keywords
        ["=", "+=", "-=", "*=", "/=", "//=", "%="],
        ["==", "<=", ">=", "!="],
        ["+", "-", "/", "*", "//", "%"],
        ["&&", "||"],
        ["{", "}", ";"],
        [",", "."],
        ["(", ")"],
        ["[", "]"],
        ["!", "&", "|", "^", "<<", ">>"]
    };

    public static TokenType GetSyntaxType(string s)
    {
        for (int i = 0; i < syntax.Length; i++)
        {
            if (syntax[i].Contains(s)) return (TokenType)(i + 3);
        }

        return TokenType.None;
    }

    /// <summary> Token types abr for the HoneyC programming language. </summary>
    static string[] tokenTypeAbr =
    {
        "<val>", // value
        "<id>", // identifier
        "<key>", // keyword
        "<aop>", // assignment operator
        "<cop>", // comparative operator
        "<mop>", // math operator
        "<lop>", // logic operator
        "<scp>", // scope
        "<spl>", // split
        "<nst>", // nest
        "<com>", // compound
        "<bop>" // bitwise operator
    };

    static string[] tokenGroupAbr = {
        "<LLE>", // line loop end
        "<CALC>", // calculation
        "<FUNC>", // function
        "<CALL>", // function call
        "<ASN>", // assignment
        "<OBJ>", // object
        "<ENUM>", // enum
        "<IMP>", // import
    };

    public static TokenType GetTokenTypeAbr(string s)
    {
        for (int i = 0; i < tokenTypeAbr.Length; i++)
        {
            if (tokenTypeAbr[i] == s) return (TokenType)(i + 1);
        }

        return TokenType.None;
    }

    public static TokenGroupType GetTokenGroupTypeAbr(string s)
    {
        for (int i = 0; i < tokenGroupAbr.Length; i++)
        {
            if (tokenGroupAbr[i] == s) return (TokenGroupType)(i + 1);
        }

        return TokenGroupType.None;
    }
}