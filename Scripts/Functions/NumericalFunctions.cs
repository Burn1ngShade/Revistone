namespace Revistone.Functions;

/// <summary>
/// Class filled with functions unrelated to console, but useful for carrying out numerical calculations.
/// </summary>
public static class NumericalFunctions
{
    /// <summary> Returns the median value from a sorted numerical list </summary>
    public static T GetMedian<T>(List<T> values) where T : IComparable<T>
    {
        // is even or odd number of elements
        if (values.Count % 2 == 0) return ((dynamic)values[(values.Count / 2) - 1] + values[values.Count / 2]) / 2;
        else return values[values.Count / 2];
    }

    /// <summary> Returns the median value from a sorted numerical list </summary>
    public static T GetMedian<T>(T[] values) where T : IComparable<T> { return GetMedian(values.ToList()); }
}