namespace Revistone.Functions;

/// <summary>
/// Class filled with functions unrelated to console, but useful for carrying out numerical calculations.
/// </summary>
public static class NumericalFunctions
{
    /// <summary> Returns the median value from a sorted numerical list. </summary>
    public static T GetMedian<T>(List<T> values) where T : IComparable<T>
    {
        if (values.Count == 0) return (dynamic)0;

        // is even or odd number of elements
        if (values.Count % 2 == 0) return ((dynamic)values[(values.Count / 2) - 1] + values[values.Count / 2]) / 2;
        else return values[values.Count / 2];
    }

    /// <summary> Returns the median value from a sorted numerical list. </summary>
    public static T GetMedian<T>(T[] values) where T : IComparable<T> { return GetMedian(values.ToList()); }

   
    /// <summary> Checks if given object is numerical, or in numerical representation. </summary>
    public static bool IsNumber<T>(T value)
    {
        string s = value?.ToString() ?? ""; //if can't convert to string just made ""

        if (s.Length == 0) return false;
        if (s[^1] == '.') s = s.Substring(0, s.Length - 1);

        return double.TryParse(s, out var result);
    }
}