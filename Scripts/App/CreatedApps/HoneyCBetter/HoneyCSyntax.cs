namespace Revistone.Apps.HoneyC;

public static class HoneyCSyntax
{
    // --- LANGUAGE SYNTAX ---

    // Basic language syntax

    public static readonly Dictionary<string[], TokenType> syntax = new Dictionary<string[], TokenType>()
    {
        {["if", "else", "while", "for", "in", "var", "val", "continue", "break", "return", "func", "true", "false"], TokenType.Keyword},
        {["="], TokenType.AssignmentOperator},
        {["==", "!=", "<=", ">=", "<", ">"], TokenType.ComparativeOperator},
        {["+", "-", "*", "/", "\\", "%"], TokenType.MathOperator},
        {["{", "}", ";"], TokenType.Scope},
        {[","], TokenType.Flow},
        {["(", ")"], TokenType.Group},
    };

    // --- TOKENTYPE ---

    public enum TokenType { Invalid, Identifier, Literal, Keyword, Flow, Scope, ComparativeOperator, AssignmentOperator, MathOperator, Group }
    public static readonly string[] tokenTypeCode = ["INV", "ID", "LIT", "KEY", "FLW", "SCP", "COP", "AOP", "MOP", "GRP", "FNC"];
    public static readonly string[] tokenTypeFlowCode = ["[OP]", "[EOP]", "[LOOP]", "[ELOOP]", "[EZLOOP]", "[ERZLOOP]"]; //e -> end, f -> force

    /// <summary> Returns all token types from a given code e.g <INV|ID|LIT> -> [Invalid, Identifier, Literal]. </summary>
    public static TokenType[] GetTokenTypes(string s)
    {
        if (s.Length < 3 || s[0] != '<' || s[^1] != '>') return [];

        string[] types = s[1..^1].Split('|');
        TokenType[] tokenTypes = new TokenType[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            int index = Array.IndexOf(tokenTypeCode, types[i]);
            if (index == -1) return [];
            tokenTypes[i] = (TokenType)index;
        }

        return tokenTypes;
    }

    // --- MEMORY ---

    public static List<HoneyCVariable> globalVariables = [];
    public static List<HoneyCVariable> localVariables = [];

    public static void ClearMemory()
    {
        globalVariables = [];
        localVariables = [];
    }
}