using Revistone.Functions;

using static Revistone.App.Calculator.CalculatorDefinitions;
using static Revistone.App.Calculator.CalculatorInterpreter;

namespace Revistone.App.Calculator;

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

    public static Token[] Tokenize(string calculation, FlagProfile flags)
    {
        List<Token> tokens = new List<Token>();

        string tokenStr = "";
        int layer = 0;
        Token? t;

        DebugHeaderOutput($"> Tokenizing [{calculation}]", flags);
        foreach (char c in calculation)
        {
            if (c == '(' || c == ')')
            {
                if (tokenStr.Length > 0)
                {
                    t = CreateToken(tokenStr, tokens.Count, layer);
                    if (t == null)
                    {
                        return ThrowInterpreterError<Token>($"Invalid Token [{tokenStr}]", flags);
                    }
                    tokens.Add(t);
                    tokenStr = "";
                }

                layer += c == '(' ? 1 : -1;
                continue;
            }

            tokenStr += c;

            if (!NumericalFunctions.IsNumber(tokenStr) && NumericalFunctions.IsNumber(tokenStr.Substring(0, tokenStr.Length - 1)))
            {
                t = CreateToken(tokenStr.Substring(0, tokenStr.Length - 1), tokens.Count, layer);
                if (t == null)
                {
                    return ThrowInterpreterError<Token>($"Invalid Token [{tokenStr}]", flags);
                }
                tokens.Add(t);

                tokenStr = c.ToString();
            }

            if (tokenStr == "-" && (tokens.Count == 0 || tokens[^1].type != TokenType.Value)) continue;

            t = CreateToken(tokenStr, tokens.Count, layer, false); // lets see if we are dealing with a constant
            if (t != null)
            {
                tokenStr = "";
                tokens.Add(t);
            }
            else if (tokens.Count > 0)
            {
                t = CreateToken(tokens[^1].identifier + tokenStr, tokens.Count - 1, layer, false);
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
                return ThrowInterpreterError<Token>($"Invalid Token [{tokenStr}]", flags);
            }
            tokens.Add(t);
        }

        DebugOutput($"Token Count: {tokens.Count}", flags);
        foreach (Token token in tokens) DebugOutput(token, flags);

        if (layer != 0)
        {
            return ThrowInterpreterError<Token>("Not All Brackets Were Closed", flags);
        }

        if (tokens.Count == 0) ThrowInterpreterError<Token>("Empty Calculation Inputed", flags);

        return tokens.ToArray();
    }

    public static Token[] EvaluateTokens(Token[] tokens, int layer, FlagProfile flags)
    {
        List<Token> layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Value).ToList();

        for (int i = layerTokens.Count - 1; i >= 0; i--)
        {
            ValueToken t = (ValueToken)layerTokens[i];
            if (t.index != 0 && tokens[t.index - 1].type == Token.TokenType.Value && tokens[t.index - 1].layer == layer) tokens = Token.Insert(tokens, new OperatorToken("*", t.index, t.layer), t.index);
        }

        layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Function).ToList();

        for (int i = layerTokens.Count - 1; i >= 0; i--) // First lets carry out all functions, easy peasy
        {
            FunctionToken t = (FunctionToken)layerTokens[i];

            if (tokens.Length == t.index + 1 || tokens[t.index + 1].layer != layer) return ThrowInterpreterError<Token>($"Function [{t.identifier}] Was Not Given An Input", flags);
            if (tokens[t.index + 1].type != Token.TokenType.Value) return ThrowInterpreterError<Token>($"Function [{t.identifier}] Given Invalid Input [{tokens[t.index + 1].identifier}]", flags);

            DebugOutput($"Evaluating Function {t.identifier}({tokens[t.index + 1].identifier})", flags);

            tokens[t.index] = new ValueToken(t.operation.Invoke(((ValueToken)tokens[t.index + 1]).value).ToString(), t.index, t.layer);
            tokens = Token.RemoveIndex(tokens, t.index + 1);
        }

        layerTokens = tokens.Where(t => t.layer == layer && t.type == Token.TokenType.Operator).OrderByDescending(t => ((OperatorToken)t).priority).ToList();

        for (int i = 0; i < layerTokens.Count; i++)
        {
            OperatorToken t = (OperatorToken)layerTokens[i];

            if (t.index == 0 || t.index + 1 == tokens.Length || tokens[t.index - 1].layer != layer || tokens[t.index + 1].layer != layer)
            {
                return ThrowInterpreterError<Token>($"Operator [{t.identifier}] Is Missing Input(s)", flags);
            }

            if (tokens[t.index - 1].type != Token.TokenType.Value) return ThrowInterpreterError<Token>($"Operator [{t.identifier}] Given Invalid Input [{tokens[t.index - 1].identifier}]", flags);
            if (tokens[t.index + 1].type != Token.TokenType.Value) return ThrowInterpreterError<Token>($"Operator [{t.identifier}] Given Invalid Input [{tokens[t.index + 1].identifier}]", flags);

            DebugOutput($"Evaluating Operation {tokens[t.index - 1].identifier} {t.identifier} {tokens[t.index + 1].identifier}", flags);

            tokens[t.index] = new ValueToken(t.operation.Invoke(((ValueToken)tokens[t.index - 1]).value, ((ValueToken)tokens[t.index + 1]).value).ToString(), t.index, t.layer);
            tokens = Token.RemoveIndex(tokens, t.index + 1);
            tokens = Token.RemoveIndex(tokens, t.index - 1);
        }

        layerTokens = tokens.Where(t => t.layer == layer).ToList();
        for (int i = layerTokens.Count - 1; i >= 0; i--)
        {
            ValueToken t = (ValueToken)layerTokens[i];

            tokens[t.index].layer--;
            if (t.index != 0 && tokens[t.index - 1].type == Token.TokenType.Value) tokens = Token.Insert(tokens, new OperatorToken("*", t.index, t.layer), t.index);
        }

        return tokens;
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