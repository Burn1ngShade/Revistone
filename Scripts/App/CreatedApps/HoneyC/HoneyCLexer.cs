using Revistone.Apps.HoneyC.Data;
using Revistone.Functions;

using static Revistone.Apps.HoneyC.Data.AbstractToken;

namespace Revistone.Apps.HoneyC;

/// <summary> Class responsible for converting user query to set of tokens. </summary>
public static class HoneyCLexer
{
    public static List<Token> Lex(string query)
    {
        query = query.TrimStart();
        if (query.Length == 0)
        {
            return [];
        }

        List<Token> tokens = [];
        TokenConstruct tk = new();

        for (int i = 0; i < query.Length; i++)
        {
            if (tk.content.Length == 0)
            {
                if (query[i] == ' ') continue;
                tk.Set(query[i]);
            }

            if (!tk.Add(query[i]))
            {
                tokens.Add(tk.Reset(query[i]));
                if (tokens[^1].type == TokenType.None)
                {
                    return Diagnostics.ThrowError<Token>("Token_Exception", "Invalid Token Type 'None'", -1, tokens.Count - 1, tokens);
                }
            }

            if (Definitions.GetSyntaxType(tk.content) != TokenType.None)
            {
                tokens.Add(new Token(tk.content, Definitions.GetSyntaxType(tk.content)));
                tk = new();
            }
        }

        if (tk.content.Length != 0)
        {
            tokens.Add(tk.Reset(' '));
            if (tokens[^1].type == TokenType.None)
            {
                return Diagnostics.ThrowError<Token>("Token_Exception", "Invalid Token Type 'None'", -1, tokens.Count - 1, tokens);
            }
        }


        return tokens;
    }

    struct TokenConstruct()
    {
        public string content { private set; get; } = "";
        bool isValue;
        bool isIdentifier;

        public bool Add(char c)
        {
            string newContent = content + c.ToString();

            if (isValue != IsValue(newContent) || isIdentifier != IsIdentifier(newContent)) return false;

            content = newContent;
            return true;
        }

        public void Set(char c)
        {
            content = "";
            isValue = IsValue(c.ToString());
            isIdentifier = IsIdentifier(c.ToString());
        }

        public Token Reset(char c)
        {
            Token t = new Token(content, isValue ? TokenType.Value : isIdentifier ? TokenType.Identifier : TokenType.None);
            Set(c);
            content = c.ToString().Trim();
            return t;
        }

        // --- 

        static bool IsValue(string s)
        {
            if (!s.Contains(' ') && float.TryParse(s, out float r)) return true;
            if (s.StartsWith('"') && (s.LastIndexOf('"') == 0 || s.LastIndexOf('"') == s.Length - 1)) return true;

            return false;
        }

        static bool IsIdentifier(string s)
        {
            if (s.InFormat("[C:]")) return true;
            return false;
        }
    }
}