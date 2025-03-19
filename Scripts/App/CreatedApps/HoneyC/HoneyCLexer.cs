using Revistone.App.BaseApps.HoneyC.Data;
using Revistone.Functions;

using static Revistone.App.BaseApps.HoneyC.Data.AbstractToken;

namespace Revistone.App.BaseApps.HoneyC;

/// <summary> Class responsible for converting user query to set of tokens. </summary>
public static class HoneyCLexer
{
    /// <summary> Removes comments and joins together lines of a given query. </summary>
    public static string CleanQuery(string[] query)
    {
        string cleanedQuery = "";

        int inComment = 0;
        bool inString = false;
        char? lastChar = null;

        foreach (string s in query)
        {
            foreach (char c in s)
            {
                if (inComment == 0 && c == '"') inString = !inString;

                if (!inString && inComment == 0 && c == '#')
                {
                    if (lastChar == '/')
                    {
                        inComment = 1;
                        cleanedQuery = cleanedQuery[..^1];
                        continue;
                    }
                    else break;
                }
                else if (inComment > 1 && c == '/' && lastChar == '#')
                {
                    inComment = 0;
                    continue;
                }

                if (inComment == 0) cleanedQuery += c;
                else inComment++;

                lastChar = c;
            }
        }

        return cleanedQuery;
    }

    /// <summary> Converts given query into a list of HoneyC Tokens. Returns [] if error occurs. </summary>
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
                    Diagnostics.Output(tokens[^1] + "\n");
                    foreach (Token t in tokens) Diagnostics.Output(t);
                    return Diagnostics.ThrowError<Token>("Token_Exception", "Invalid Token Type 'None'", -1, tokens.Count - 1, tokens);
                }
            }

            if (tokens.Count > 0 && Definitions.GetSyntaxType(tokens[^1].content + tk.content) != TokenType.None)
            {
                tokens[^1] = new Token(tokens[^1].content + tk.content, Definitions.GetSyntaxType(tokens[^1].content + tk.content));
                tk = new();
            }
            else if (Definitions.GetSyntaxType(tk.content) != TokenType.None)
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
                Diagnostics.Output(tokens[^1] + "\n");
                foreach (Token t in tokens) Diagnostics.Output(t);
                return Diagnostics.ThrowError<Token>("Token_Exception", "Invalid Token Type 'None'", -1, tokens.Count - 1, tokens);
            }
        }

        Diagnostics.Output("--- Tokens Generated ---", true);
        Diagnostics.Output($"Total - {tokens.Count}");
        Diagnostics.Output(tokens.ToElementString());
    
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

        static bool IsValue(string s)
        {
            if (!s.Contains(' ') && !s.Contains(',') && float.TryParse(s, out float r)) return true;
            // Diagnostics.Output(s);
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