using static Revistone.Apps.HoneyC.Data.AbstractToken;

namespace Revistone.Apps.HoneyC.Data;

/// <summary> Class responsible for language syntax and definitions. </summary>
public static class Definitions
{
    /// <summary> Key TokenType syntax for the HoneyC programming language. </summary>
    static readonly string[][] syntax =
    {
        ["obj", "enum", "func", "in", "while", "for", "if", "else", "or", "and", "this", "var", "val", "import", "true", "false", "null"], //keywords
        ["=", "+=", "-=", "*=", "/=", "//=", "%="], // assigment operators
        ["==", "<=", ">=", "!="], // comparative operators
        ["+", "-", "/", "*", "//", "%"], // math operators
        ["&&", "||"], // logic operators
        ["{", "}", ";"], // scope
        [",", "."], // split
        ["(", ")"], // nest
        ["[", "]"], // compound
        ["!", "&", "|", "^", "<<", ">>"] // bitwise operators
    };

    /// <summary> Returns the equivalent TokenType from a tokenContent string, returning TokenType.None upon lookup failure. </summary>
    public static TokenType GetSyntaxType(string tokenContent)
    {
        for (int i = 0; i < syntax.Length; i++)
        {
            if (syntax[i].Contains(tokenContent)) return (TokenType)(i + 3);
        }

        return TokenType.None;
    }

    /// <summary> TokenType abbreviations for the HoneyC programming language. </summary>
    static readonly string[] tokenTypeAbr =
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

    /// <summary> Returns the equivalent TokenType for a given abbreviation, returning TokenType.None upon lookup failure. </summary>
    public static TokenType GetTokenTypeAbr(string s)
    {
        for (int i = 0; i < tokenTypeAbr.Length; i++)
        {
            if (tokenTypeAbr[i] == s) return (TokenType)(i + 1);
        }

        return TokenType.None;
    }

    /// <summary> TokenGroupType abbreviations for the HoneyC programming language. </summary>
    static readonly string[] tokenGroupAbr = {
        "<LLE>", // line loop end
        "<CALC>", // calculation
        "<FUNC>", // function
        "<CALL>", // function call
        "<ASN>", // assignment
        "<OBJ>", // object
        "<ENUM>", // enum
        "<IMP>", // import
        "<ARR>", // array
    };

    /// <summary> Returns the equivalent TokenGroupType for a given abbreviation, returning TokenGroupType.None upon lookup failure. </summary>
    public static TokenGroupType GetTokenGroupTypeAbr(string s)
    {
        for (int i = 0; i < tokenGroupAbr.Length; i++)
        {
            if (tokenGroupAbr[i] == s) return (TokenGroupType)(i + 1);
        }

        return TokenGroupType.None;
    }
}