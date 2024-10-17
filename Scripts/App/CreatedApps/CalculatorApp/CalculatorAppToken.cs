using Revistone.Console;

using static Revistone.Apps.Calculator.CalculatorDefinitions;
using static Revistone.Console.ConsoleAction;
using static Revistone.Apps.CalculatorApp;

namespace Revistone.Apps.Calculator;

public abstract class Token
{
    public enum TokenType { Value, Operator, Function };

    public string identifier { get; private set; }
    public TokenType type { get; private set; }
    public int index { get; set; }
    public int layer { get; set; } //bracket depth

    public Token(string identifier, TokenType type, int index, int layer)
    {
        this.identifier = identifier;
        this.type = type;
        this.index = index;
        this.layer = layer;
    }

    public static Token? CreateToken(string identifier, int index, int layer, bool allowNumericalCreation = true)
    {
        if (constants.ContainsKey(identifier)) return new ValueToken(identifier, index, layer);
        if (variables.ContainsKey(identifier)) return new ValueToken(identifier, index, layer);
        if (operators.ContainsKey(identifier)) return new OperatorToken(identifier, index, layer);
        if (functions.ContainsKey(identifier)) return new FunctionToken(identifier, index, layer);
        if (allowNumericalCreation && double.TryParse(identifier, out _)) return new ValueToken(identifier, index, layer);

        return null;
    }

    public static Token[] Tokenize(string calculation, bool debug)
    {
        List<Token> tokens = new List<Token>();

        string tokenStr = "";
        int layer = 0;
        Token? t;

        if (debug) SendConsoleMessage(new ConsoleLine($"> Attempting To Tokenize Calculation", ConsoleColor.Cyan));

        foreach (char c in calculation)
        {
            if (c == '(' || c == ')')
            {
                if (tokenStr.Length > 0)
                {
                    t = Token.CreateToken(tokenStr, tokens.Count, layer);
                    if (t == null)
                    {
                        return ThrowCalculationError($"Invalid Token [{tokenStr}]");
                    }
                    tokens.Add(t);
                    tokenStr = "";
                }

                layer += c == '(' ? 1 : -1;
                continue;
            }

            tokenStr += c;

            if (!IsNumber(tokenStr) && IsNumber(tokenStr.Substring(0, tokenStr.Length - 1)))
            {
                t = Token.CreateToken(tokenStr.Substring(0, tokenStr.Length - 1), tokens.Count, layer);
                if (t == null)
                {
                    return ThrowCalculationError($"Invalid Token [{tokenStr}]");
                }
                tokens.Add(t);

                tokenStr = c.ToString();
            }

            if (tokenStr == "-" && (tokens.Count == 0 || tokens[^1].type != TokenType.Value)) continue;

            t = Token.CreateToken(tokenStr, tokens.Count, layer, false); // lets see if we are dealing with a constant
            if (t != null)
            {
                tokenStr = "";
                tokens.Add(t);
            }
            else if (tokens.Count > 0)
            {
                t = Token.CreateToken(tokens[^1].identifier + tokenStr, tokens.Count - 1, layer, false);
                if (t != null)
                {
                    tokens[^1] = t;
                    tokenStr = "";
                }
            }
        }

        if (tokenStr.Length > 0)
        {
            t = Token.CreateToken(tokenStr, tokens.Count, layer);
            if (t == null)
            {
                return ThrowCalculationError($"Invalid Token [{tokenStr}]");
            }
            tokens.Add(t);
        }

        if (debug)
        {
            SendConsoleMessage(new ConsoleLine($"Token Count: {tokens.Count}", ConsoleColor.DarkBlue));
            foreach (Token token in tokens) SendConsoleMessage(new ConsoleLine(token.ToString(), ConsoleColor.DarkBlue));
        }

        if (layer != 0)
        {
            return ThrowCalculationError("Not All Brackets Were Closed!");
        }

        if (tokens.Count == 0) ThrowCalculationError("Empty Calculation Inputed!");

        return tokens.ToArray();
    }

    public static Token[] RemoveIndex(Token[] tokens, int index)
    {
        tokens = tokens.Where((t, i) => i != index).ToArray();
        for (int i = index; i < tokens.Length; i++) tokens[i].index -= 1;
        return tokens;
    }

    public static Token[] Insert(Token[] tokens, Token token, int index)
    {
        List<Token> tokensList = tokens.ToList();
        tokensList.Insert(index, token);
        for (int i = index + 1; i < tokensList.Count; i++) tokensList[i].index += 1;
        return tokensList.ToArray();
    }

    public override string ToString()
    {
        return $"Identifier: {identifier}, Index: {index}, Layer: {layer}";
    }
}

public class ValueToken : Token
{
    public double value { get; private set; }

    public ValueToken(string identifier, int index, int layer) : base(identifier, TokenType.Value, index, layer)
    {
        if (constants.ContainsKey(identifier)) value = constants[identifier];
        else if (variables.ContainsKey(identifier)) value = variables[identifier];
        else value = double.Parse(identifier);
    }

    public override string ToString()
    {
        return base.ToString() + $", Value: {value}";
    }
}

public class OperatorToken : Token
{
    public Func<double, double, double> operation { get; private set; }
    public int priority { get; private set; }

    public OperatorToken(string identifier, int index, int layer) : base(identifier, TokenType.Operator, index, layer)
    {
        operation = operators[identifier].function;
        priority = operators[identifier].priority;
    }
}

public class FunctionToken : Token
{
    public Func<double, double> operation { get; private set; }

    public FunctionToken(string identifier, int index, int layer) : base(identifier, TokenType.Function, index, layer)
    {
        operation = functions[identifier];
    }
}