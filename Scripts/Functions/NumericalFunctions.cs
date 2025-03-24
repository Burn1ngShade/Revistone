namespace Revistone.Functions;

/// <summary> Class filled with functions unrelated to console, but useful for carrying out numerical calculations. </summary>
public static class NumericalFunctions
{
    /// <summary> Returns the median value from a sorted numerical list. </summary>
    public static float GetMedian(List<float> values)
    {
        if (values.Count == 0) return 0;

        // is even or odd number of elements
        if (values.Count % 2 == 0) return ((dynamic)values[(values.Count / 2) - 1] + values[values.Count / 2]) / 2;
        else return values[values.Count / 2];
    }
    /// <summary> Returns the median value from a sorted numerical list. </summary>
    public static float GetMedian(float[] values) { return GetMedian(values.ToList()); }

    // <summary> Returns the variance of a numerical list. </summary>
    public static float GetVariance(List<float> values)
    {
        float mean = values.Sum() / values.Count;
        return (values.Sum(x => x * x) / values.Count) - mean * mean;
    }
    // <summary> Returns the variance of a numerical list. </summary>
    public static float GetVariance(float[] values) { return GetVariance(values.ToList()); }

    /// <summary> Checks if given object is numerical, or in numerical representation. </summary>
    public static bool IsNumber<T>(T value)
    {
        string s = value?.ToString() ?? ""; //if can't convert to string just made ""

        if (s.Length == 0) return false;
        if (s[^1] == '.') s = s.Substring(0, s.Length - 1);

        return double.TryParse(s, out var result);
    }
}