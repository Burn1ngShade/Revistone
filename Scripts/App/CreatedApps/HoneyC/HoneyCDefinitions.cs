namespace Revistone.Apps.HoneyC;

public static class HoneyCDefinitions
{
    // keywords and their associated mode values
    public static readonly string[] keyWords = ["calc", "let", "del"];

    public static readonly Dictionary<string, double> constants = new()
    {
        {"pi", 3.1415926535},
        {"-pi", -3.1415926535},
        {"e", 2.7182818284},
        {"-e", -2.7182818284},
        {"g", 9.80665},
        {"-g", 9.80665},
    };

    // any mathematical operation that takes 2 values and creates 1 output
    public static readonly Dictionary<string, (Func<double, double, double> function, int priority)> operators = new()
    {
        {"+", ((x, y) => x + y, 0)},
        {"-", ((x, y) => x - y, 0)},
        {"*", ((x, y) => x * y, 1)},
        {"/", ((x, y) => x / y, 1)},
        {"^", ((x, y) => Math.Pow(x, y), 2)},
        {"root", ((x, y) => Math.Pow(y, 1.0 / x), 2)},
        {"log", ((x, y) => Math.Log(y, x), 2)},
    };

    // any mathematical operation that takes 1 value and creates 1 output
    public static readonly Dictionary<string, Func<double, double>> functions = new()
    {
        {"round", (x) => Math.Round(x)},
        {"floor", (x) => Math.Floor(x)},
        {"roof", (x) => Math.Ceiling(x)},
        {"sqrt", (x) => Math.Sqrt(x)},
        {"sin", (x) => Math.Round(Math.Sin(x), 6)},
        {"asin", (x) => Math.Round(Math.Asin(x), 6)},
        {"cos", (x) => Math.Round(Math.Cos(x), 6)},
        {"acos", (x) => Math.Round(Math.Acos(x), 6)},
        {"tan", (x) => Math.Round(Math.Tan(x), 6)},
        {"atan", (x) => Math.Round(Math.Atan(x), 6)},
        {"sinh", (x) => Math.Round(Math.Sinh(x), 6)},
        {"ln", (x) => Math.Round(Math.Log(x), 6)},
    };

    // variables that can be created by the user
    public static Dictionary<string, double> variables = new() { };
}