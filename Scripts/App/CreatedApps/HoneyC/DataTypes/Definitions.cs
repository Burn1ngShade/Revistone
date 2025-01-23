using static Revistone.Apps.HoneyC.Data.AbstractToken;

namespace Revistone.Apps.HoneyC.Data;

/// <summary> Class responsible for holding language syntax and definitions. </summary>
public static class Definitions
{ 
    /// <summary> Key syntax for the HoneyC programming language. </summary>
    static string[][] syntax =
    {
        ["obj", "func", "in", "while", "for", "if", "else", "or", "and", "this", "var", "val"], //keywords
        ["="], 
        ["==", "<=", ">=", "!="],
        ["+", "-", "/", "*", "//", "%", "^"],
        ["{", "}", ";"],
        [",", "."],
        ["(", ")"],
        ["[", "]"]
    };

    public static TokenType GetSyntaxType(string s)
    {
        for (int i = 0; i < syntax.Length; i++)
        {
            if (syntax[i].Contains(s)) return (TokenType)(i + 3);
        }

        return TokenType.None;
    }
}