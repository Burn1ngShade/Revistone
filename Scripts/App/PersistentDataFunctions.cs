using System.Text.Json;
using Revistone.Console;
using Revistone.Management;

namespace Revistone.Functions;

/// <summary> Class pertaining all logic for easy saving of data for apps. </summary>
public static class PersistentDataFunctions
{
    public enum DataLocation { App, Console, Workspace }
    public enum SaveType { Overwrite, PartialOverwrite, Append, Insert }

    static readonly string[] dataPaths = ["PersistentData/App/", "PersistentData/Console/", "PersistentData/Workspace/"];

    // --- Useful ---

    ///<summary> Generates full data path. </summary>
    public static string GeneratePath(DataLocation location, string path)
    {
        return dataPaths[(int)location] + path;
    }

    ///<summary> Generates full data path. </summary>
    public static string GeneratePath(DataLocation location, string areaName, string path)
    {
        return dataPaths[(int)location] + $"{areaName}/" + path;
    }

    ///<summary> Checks if path is valid, warns user if using an invalid path. </summary>
    static bool IsPathValid(string path)
    {
        bool isValid = path.StartsWith("PersistentData/") || path.StartsWith("PersistentData\\");

        if (!isValid)
        {
            Analytics.Debug.CreateDebugMessage($"Error: Invalid File Path {path}");
        }

        return isValid;
    }

    /// <summary> Splits path into its components. </summary> 
    static string[] SplitPath(string path)
    {
        return path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
    }

    // --- Directorys ---

    /// <summary> Creates directory with given name. </summary> 
    public static void CreateDirectory(string path)
    {
        if (!IsPathValid(path)) return;

        string[] splitPath = SplitPath(path);
        string usedPath = "";
        for (int i = 0; i < splitPath.Length; i++)
        {
            usedPath += i == 0 ? splitPath[i] : $"/{splitPath[i]}";
            if (!DirectoryExists(usedPath)) Directory.CreateDirectory($"{usedPath}");
        }
    }

    /// <summary> Deletes directory with given name. </summary>
    public static void DeleteDirectory(string path)
    {
        if (!IsPathValid(path)) return;

        if (DirectoryExists(path)) Directory.Delete(path);
    }

    /// <summary> Checks for directory with given name. </summary>
    public static bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary> Returns a list of all the names of directorys at a given path. </summary>
    public static string[] GetSubDirectorys(string path)
    {
        if (!IsPathValid(path)) return [];

        if (!DirectoryExists(path)) CreateDirectory(path);
        return Directory.GetDirectories(path)?.Select(filePath => Path.GetFileName(filePath))?.ToArray() ?? Array.Empty<string>();
    }

    // --- FILE MANAGEMENT ---

    /// <summary> Creates file with given name. </summary>
    public static void CreateFile(string path)
    {
        if (!IsPathValid(path)) return;

        string[] splitPath = SplitPath(path);
        string usedPath = "";
        for (int i = 0; i < splitPath.Length - 1; i++)
        {
            usedPath += i == 0 ? splitPath[i] : $"/{splitPath[i]}";
            if (!DirectoryExists(usedPath)) Directory.CreateDirectory(usedPath);
        }
        if (splitPath.Length > 0 && !FileExists(path)) File.CreateText(path).Close();
    }

    /// <summary> Deletes file with given name. </summary>
    public static void DeleteFile(string path)
    {
        if (!IsPathValid(path)) return;

        if (FileExists(path)) File.Delete(path);
    }

    /// <summary> Checks for file with given name. </summary>
    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary> Returns a list of all the names of directorys at a given path. </summary>
    public static string[] GetSubFiles(string path)
    {
        if (!IsPathValid(path)) return [];

        if (!DirectoryExists(path)) CreateDirectory(path);
        return Directory.GetFiles(path).Select(filePath => Path.GetFileName(filePath)).ToArray();
    }

    // --- FILES SAVE ---

    /// <summary> Saves fileData to given file with given name (startIndex has no effect on SaveType Overwrite or Append). </summary>
    public static bool SaveFile(string path, string[] fileData, int startIndex = 0, SaveType saveType = SaveType.Overwrite)
    {
        if (!IsPathValid(path)) return false;

        if (!FileExists(path)) CreateFile(path);

        string[] currentTextFile = File.ReadAllLines(path);

        switch (saveType)
        {
            case SaveType.Overwrite:
                File.WriteAllLines(path, fileData);
                return true;
            case SaveType.PartialOverwrite:
                string[] newTextFile = new string[Math.Max(startIndex + fileData.Length, currentTextFile.Length)];
                Array.Copy(currentTextFile, newTextFile, currentTextFile.Length);
                Array.Copy(fileData, 0, newTextFile, startIndex, fileData.Length);
                File.WriteAllLines(path, newTextFile);
                return true;
            case SaveType.Append:
                File.AppendAllLines(path, fileData);
                return true;
            case SaveType.Insert:
                string[] insertedTextFile = new string[currentTextFile.Length + fileData.Length];
                Array.Copy(currentTextFile, 0, insertedTextFile, 0, startIndex);
                Array.Copy(fileData, 0, insertedTextFile, startIndex, fileData.Length);
                Array.Copy(currentTextFile, startIndex, insertedTextFile, startIndex + fileData.Length, currentTextFile.Length - startIndex);
                File.WriteAllLines(path, insertedTextFile);
                return true;

        }

        return false;
    }

    // --- FILE LOAD ---

    /// <summary> Loads data from given file, within a start index and given length. </summary>
    public static string[] LoadFile(string path, int startIndex = 0, int length = -1)
    {
        if (!IsPathValid(path)) return [];

        if (!FileExists(path)) CreateFile(path);

        return File.ReadAllLines(path).Skip(startIndex).Take(length > 0 ? length : int.MaxValue).ToArray();
    }

    /// <summary> Loads data from given file, within a start index and given length, and splits it into arrays of given length. </summary>
    public static string[][] LoadFile(string path, int arrayLength, bool removePartialArrays, int startIndex = 0, int length = -1)
    {
        if (!IsPathValid(path)) return [];

        if (!FileExists(path)) CreateFile(path);

        if (arrayLength < 1) arrayLength = 1;

        string[][] s = File.ReadAllLines(path).Skip(startIndex).Take(length > 0 ? length : int.MaxValue)
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
        if (!IsPathValid(path)) return "";

        if (!FileExists(path)) CreateFile(path);

        string[] s = File.ReadAllLines(path);

        return lineIndex > s.Length - 1 ? "" : s[lineIndex];
    }

    // --- JSON Commands ---

    /// <summary> Saves fileData to given file with given name in the JSON format. </summary>
    public static bool SaveFileAsJSON<T>(string path, T data)
    {
        if (!IsPathValid(path)) return false;

        string json = JsonSerializer.Serialize<T>(data, new JsonSerializerOptions { WriteIndented = true });

        if (!FileExists(path)) CreateFile(path);
        File.WriteAllText(path, json);

        return true;
    }

    /// <summary> Loads data from given file, if not in JSON format, the default value for given type is returned. </summary>
    public static T? LoadFileFromJSON<T>(string path)
    {
        if (!IsPathValid(path)) return default;
        if (!FileExists(path)) CreateFile(path);

        string json = File.ReadAllText(path);
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