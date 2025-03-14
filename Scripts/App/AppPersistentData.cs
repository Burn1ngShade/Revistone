using System.Text.Json;

namespace Revistone.App;

/// <summary> Class pertaining all logic for easy saving of data for apps. </summary>
public static class AppPersistentData
{
    public enum SaveType { Overwrite, PartialOverwrite, Append, Insert }

    static readonly string dataPath = "PersistentData/";

    // --- Useful ---

    /// <summary> Splits path into its components. </summary> 
    static string[] SplitPath(string path)
    {
        return path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
    }

    // --- Directorys ---

    /// <summary> Creates directory with given name. </summary> 
    public static void CreateDirectory(string path)
    {
        string[] splitPath = SplitPath(path);
        string usedPath = "";
        for (int i = 0; i < splitPath.Length; i++)
        {
            usedPath += $"/{splitPath[i]}";
            if (!DirectoryExists($"{dataPath}{usedPath}")) Directory.CreateDirectory($"{dataPath}{usedPath}");
        }
    }

    /// <summary> Deletes directory with given name. </summary>
    public static void DeleteDirectory(string path)
    {
        if (DirectoryExists($"{dataPath}{path}")) Directory.Delete($"{dataPath}{path}");
    }

    /// <summary> Checks for directory with given name. </summary>
    public static bool DirectoryExists(string path) { return Directory.Exists($"{dataPath}{path}"); }

    /// <summary> Returns a list of all the names of directorys at a given path. </summary>
    public static string[] GetSubDirectorys(string path)
    {
        if (!DirectoryExists(path)) CreateDirectory(path);
        return Directory.GetDirectories($"{dataPath}{path}")?.Select(filePath => Path.GetFileName(filePath))?.ToArray() ?? Array.Empty<string>();
    }

    // --- FILE MANAGEMENT ---

    /// <summary> Creates file with given name. </summary>
    public static void CreateFile(string path)
    {
        string[] splitPath = SplitPath(path);
        string usedPath = "";
        for (int i = 0; i < splitPath.Length - 1; i++)
        {
            usedPath += $"/{splitPath[i]}";
            if (!DirectoryExists($"{dataPath}{usedPath}")) Directory.CreateDirectory($"{dataPath}{usedPath}");
        }
        if (splitPath.Length > 0 && !FileExists(path)) File.CreateText($"{dataPath}{path}").Close();
    }

    /// <summary> Deletes file with given name. </summary>
    public static void DeleteFile(string path)
    {
        if (FileExists(path)) File.Delete($"{dataPath}{path}");
    }

    /// <summary> Checks for file with given name. </summary>
    public static bool FileExists(string path) { return File.Exists($"{dataPath}{path}"); }

    /// <summary> Returns a list of all the names of directorys at a given path. </summary>
    public static string[] GetSubFiles(string path)
    {
        if (!DirectoryExists(path)) CreateDirectory(path);
        return Directory.GetFiles($"{dataPath}{path}").Select(filePath => Path.GetFileName(filePath)).ToArray();
    }

    // --- FILES SAVE ---

    /// <summary> Saves fileData to given file with given name (startIndex has no effect on SaveType Overwrite or Append). </summary>
    public static bool SaveFile(string path, string[] fileData, int startIndex = 0, SaveType saveType = SaveType.Overwrite)
    {
        if (!FileExists(path)) CreateFile(path);

        string[] currentTextFile = File.ReadAllLines($"{dataPath}{path}");

        switch (saveType)
        {
            case SaveType.Overwrite:
                File.WriteAllLines($"{dataPath}{path}", fileData);
                return true;
            case SaveType.PartialOverwrite:
                string[] newTextFile = new string[Math.Max(startIndex + fileData.Length, currentTextFile.Length)];
                Array.Copy(currentTextFile, newTextFile, currentTextFile.Length);
                Array.Copy(fileData, 0, newTextFile, startIndex, fileData.Length);
                File.WriteAllLines($"{dataPath}{path}", newTextFile);
                return true;
            case SaveType.Append:
                File.AppendAllLines($"{dataPath}{path}", fileData);
                return true;
            case SaveType.Insert:
                string[] insertedTextFile = new string[currentTextFile.Length + fileData.Length];
                Array.Copy(currentTextFile, 0, insertedTextFile, 0, startIndex);
                Array.Copy(fileData, 0, insertedTextFile, startIndex, fileData.Length);
                Array.Copy(currentTextFile, startIndex, insertedTextFile, startIndex + fileData.Length, currentTextFile.Length - startIndex);
                File.WriteAllLines($"{dataPath}{path}", insertedTextFile);
                return true;

        }

        return false;
    }

    // --- FILE LOAD ---

    /// <summary> Loads data from given file, within a start index and given length. </summary>
    public static string[] LoadFile(string path, int startIndex = 0, int length = -1)
    {
        if (!FileExists(path)) CreateFile(path);

        return File.ReadAllLines($"{dataPath}{path}").Skip(startIndex).Take(length > 0 ? length : int.MaxValue).ToArray();
    }

    /// <summary> Loads data from given file, within a start index and given length, and splits it into arrays of given length. </summary>
    public static string[][] LoadFile(string path, int arrayLength, bool removePartialArrays, int startIndex = 0, int length = -1)
    {
        if (!FileExists(path)) CreateFile(path);

        if (arrayLength < 1) arrayLength = 1;

        string[][] s = File.ReadAllLines($"{dataPath}{path}").Skip(startIndex).Take(length > 0 ? length : int.MaxValue)
        .Select((line, index) => new { line, index })
        .GroupBy(x => x.index / arrayLength)
        .Select(group => group.Select(x => x.line).ToArray())
        .ToArray();

        if (s.Length > 0 && removePartialArrays && s[s.Length - 1].Length != arrayLength) return s.SkipLast(1).ToArray();
        else return s;
    }

    /// <summary> Loads data from given file, and given line. </summary>
    public static string LoadFile(string path, int lineIndex)
    {
        if (!FileExists(path)) CreateFile(path);

        string[] s = File.ReadAllLines($"{dataPath}{path}");

        return lineIndex > s.Length - 1 ? "" : s[lineIndex];
    }

    // --- JSON Commands ---

    /// <summary> Saves fileData to given file with given name in the JSON format. </summary>
    public static bool SaveFileAsJSON<T>(string path, T data)
    {
        string json = JsonSerializer.Serialize<T>(data, new JsonSerializerOptions { WriteIndented = true });

        if (!FileExists(path)) CreateFile(path);
        File.WriteAllText($"{dataPath}{path}", json);

        return true;
    }

    /// <summary> Loads data from given file, if not in JSON format, the default value for given type is returned. </summary>
    public static T? LoadFileFromJSON<T>(string path)
    {
        if (!File.Exists(path)) CreateFile(path);

        string json = File.ReadAllText($"{dataPath}{path}");
        try
        {
            T? data = JsonSerializer.Deserialize<T>(json);
            return data;
        }
        catch (Exception)
        {
            return default;
        }
    }
}