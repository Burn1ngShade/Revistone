using static Revistone.Apps.HoneyC.HoneyCVariable;

namespace Revistone.Apps.HoneyC;

public class HoneyCProgram
{
    public List<HoneyCVariable> variables = [];
    public List<HoneyCFunction> functions = [];
    public Dictionary<int, int> scopes = []; // program loops

    public List<TokenStatement> statements = [];
}

public class HoneyCFunction(string name, int lineIndex, string[] inputs, TokenStatement[] statements)
{
    public string name = name;
    public string[] inputs = inputs;
    public int lineIndex = lineIndex;

    public TokenStatement[] statements = statements;
}

public class HoneyCVariable(string name, VariableType type, object value, bool isConstant)
{
    public enum VariableType { Int, Float64, String, Invalid }

    public string name = name;
    public VariableType type = type;

    public object value = value;
    public bool isConstant = isConstant;
}