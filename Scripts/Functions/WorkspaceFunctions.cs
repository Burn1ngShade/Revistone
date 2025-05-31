using Revistone.Console;
using Revistone.Interaction;
using Revistone.App;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using Revistone.Management;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Console.ConsoleAction;
using System.Text.RegularExpressions;
using Revistone.App.BaseApps.HoneyC;
using Revistone.Console.Image;
using Revistone.App.BaseApps;

namespace Revistone.Functions;

/// <summary> Class filled with functions to manage console workspace. </summary>
public static class WorkspaceFunctions
{
    public static readonly string RootPath = @"PersistentData\Workspace\"; // path of root directory

    static DirectoryInfo dir = new(path ?? RootPath); // info of current directory
    static string path = RootPath; // path of current directory

    public static string DisplayPath => dir.FullName[(dir.FullName.IndexOf(path, StringComparison.OrdinalIgnoreCase) + 15)..^1]; // use when displaying path
    public static string RawPath => path;

    static readonly string[] ValidTextFileExtensions = {
        ".txt", ".md", ".csv", ".log", ".xml", ".json", ".yaml", ".yml", ".ini",
        ".cfg", ".bat", ".cmd", ".sh", ".py", ".java", ".c", ".c#", ".cpp", ".html",
        ".css", ".js", ".ts", ".tex", ".sql", ".ps1", ".toml", ".env", ".lst",
        ".srt", ".vtt", ".nfo", ".properties", ".pl", ".rb", ".go", ".rss", ".xhtml", ".hc"
    };

    // --- PATHS ---

    ///<summary> Gets the path of the parent folder of the current workspace folder. </summary>
    public static string GetParentPath(string path)
    {
        if (path == RootPath) return path;

        path = path[..path[..^1].LastIndexOf('\\')];
        if (DirectoryExists(path)) path = GetTruePathName(path);
        path += @"\";
        return path[path.IndexOf(RootPath, StringComparison.OrdinalIgnoreCase)..];
    }

    ///<summary> Updates workspace folder path. </summary>
    public static bool UpdatePath(string newPath, bool output = true)
    {
        if (!DirectoryExists(newPath))
        {
            if (output) SendConsoleMessage(new ConsoleLine($"Directory Does Not Exist - '{newPath[15..^1]}'", BuildArray(AppRegistry.PrimaryCol.Extend(27), AppRegistry.SecondaryCol)));
            return false;
        }

        string newPathLastDir = Path.GetFileName(newPath.EndsWith('\\') ? newPath[..^1] : newPath);
        if (Regex.IsMatch(newPathLastDir, @"^\.+$"))
        {
            if (output) SendConsoleMessage(new ConsoleLine($"Invalid Command.", AppRegistry.PrimaryCol));
            return false;
        }

        if (newPath.Equals(path, StringComparison.CurrentCultureIgnoreCase))
        {
            if (output) SendConsoleMessage(new ConsoleLine($"Already In Directory - '{DisplayPath}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));
            return false;
        }

        newPath = GetTruePathName(newPath);
        if (!newPath.EndsWith('\\')) newPath += @"\";
        path = newPath[newPath.IndexOf(RootPath, StringComparison.OrdinalIgnoreCase)..];
        dir = new(path);
        if (output) SendConsoleMessage(new ConsoleLine($"Changed To Directory - '{DisplayPath}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));

        Analytics.General.DirectoriesOpened++;

        return true;
    }

    ///<summary> Info about each workspace query, allowing for query modifiers (e.g -r, -p, -j). </summary>
    struct WorkspaceQuery
    {
        public readonly string Path;
        public readonly DirectoryInfo Dir;

        public string Name;

        public readonly string Location => Path + Name;
        public readonly string Info; // info about query, e.g. "At Root Directory"

        public readonly Dictionary<string, bool> modifiers = new()
        {
            { "-r", false }, // root directory
            { "-p", false }, // parent directory
            { "-j", false}, // jump to
        };

        public WorkspaceQuery(string queryName, bool isFile = false)
        {
            while (queryName.Length >= 2)
            {
                bool foundModifier = false;
                foreach (string key in modifiers.Keys)
                {
                    if (queryName.EndsWith(key))
                    {
                        modifiers[key] = true;
                        foundModifier = true;
                        queryName = queryName[..^key.Length].TrimEnd();
                        break;
                    }
                }
                if (!foundModifier) break;
            }

            if (modifiers["-r"])
            {
                Path = RootPath;
                Dir = new(Path);
                Info = "Within Root Directory ";
            }
            else if (modifiers["-p"])
            {
                Path = GetParentPath(path);
                Dir = new(Path);
                Info = "Within Parent Directory ";
            }
            else
            {
                Path = path;
                Dir = dir;
                Info = "";
            }

            Name = queryName;

            if (isFile) VerifyFileExtension();
        }

        public void VerifyFileExtension()
        {
            string fileExtension = System.IO.Path.GetExtension(Name);
            if (fileExtension.Length == 0) Name += ".txt"; // default file extension
        }
    }

    // --- DIRECTORIES ---

    ///<summary> Creates a directory within the workspace. </summary>
    public static bool CreateWorkspaceDirectory(string dirName)
    {
        WorkspaceQuery q = new(dirName);

        if (!IsNameValid(q.Name, true, true)) return false;

        string dirLabel = (dirName.Contains('\\') || dirName.Contains('/')) ? "Directories" : "Directory";

        if (DirectoryExists(q.Location))
        {
            SendConsoleMessage(new ConsoleLine($"{dirLabel} Already Exists {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(18 + dirLabel.Length + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        CreateDirectory(q.Location);
        SendConsoleMessage(new ConsoleLine($"Created {dirLabel} {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(11 + dirLabel.Length + q.Info.Length), AppRegistry.SecondaryCol)));

        if (q.modifiers["-j"]) UpdatePath(q.Location); // jump to new directory

        Analytics.General.DirectoriesCreated++;

        return true;
    }

    ///<summary> Removes a directory within workspace. </summary>
    public static bool DeleteWorkspaceDirectory(string dirName)
    {
        WorkspaceQuery q = new(dirName);

        if (!DirectoryExists(q.Location) || q.Name == "")
        {
            SendConsoleMessage(new ConsoleLine($"Directory Does Not Exist {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(27 + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        if (!UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Are You Sure You Want To Delete Directory {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(44 + q.Info.Length), AppRegistry.SecondaryCol)))) return false;

        DeleteDirectory(q.Location);
        SendConsoleMessage(new ConsoleLine($"Deleted Directory {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(20 + q.Info.Length), AppRegistry.SecondaryCol)));

        if (!DirectoryExists(path)) // if we deleted the directory we are currently in using -p or -r
        {
            string validPath = path;
            while (!DirectoryExists(validPath))
            {
                validPath = GetParentPath(validPath);
            }
            UpdatePath(validPath);
        }

        Analytics.General.DirectoriesDeleted++;

        return true;
    }

    // --- DISPLAY FUNCTIONS ---

    ///<summary> Displays general info about current workspace directory. </summary>
    public static void DisplayWorkspaceOverview()
    {
        SendConsoleMessage(new ConsoleLine($"--- {DisplayPath} ---", AppRegistry.PrimaryCol));

        (string name, DirectoryInfo info)[] directories = [.. GetSubDirectories(path).Select(x => (x, new DirectoryInfo(path + x))).OrderByDescending(x => x.Item2.GetFiles().Length)];
        (string name, FileInfo info)[] files = [.. GetSubFiles(path).Select(x => (x, new FileInfo(path + x))).OrderByDescending(x => x.Item2.Length)];

        SendConsoleMessage(new ConsoleLine($"Created On - {dir.CreationTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(12), AppRegistry.PrimaryCol)));
        SendConsoleMessage(new ConsoleLine($"Last Modified - {dir.LastWriteTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(16), AppRegistry.PrimaryCol)));
        SendConsoleMessage(new ConsoleLine("")); // prevent some shiftline weirdness
        SendConsoleMessage(new ConsoleLine("Directories:", AppRegistry.SecondaryCol));
        for (int i = 0; i < Math.Min(directories.Length, 6); i++)
        {
            (string name, DirectoryInfo info) = directories[i];
            SendConsoleMessage(new ConsoleLine($"  {name} - {info.GetFiles().Length} Files, {info.GetDirectories().Length} Directories", AppRegistry.PrimaryCol));
        }
        if (directories.Length == 0) SendConsoleMessage(new ConsoleLine("  No Directories Found.", AppRegistry.PrimaryCol));
        if (directories.Length > 6) SendConsoleMessage(new ConsoleLine($"  + {directories.Length - 6} {(directories.Length - 6 == 1 ? "Directory" : "Directories")}", AppRegistry.SecondaryCol));
        SendConsoleMessage(new ConsoleLine(""));
        SendConsoleMessage(new ConsoleLine("Files:", AppRegistry.SecondaryCol));
        for (int i = 0; i < Math.Min(files.Length, 6); i++)
        {
            (string name, FileInfo info) = files[i];
            SendConsoleMessage(new ConsoleLine($"  {name} - {info.Length} Bytes", AppRegistry.PrimaryCol));
        }
        if (files.Length == 0) SendConsoleMessage(new ConsoleLine("  No Files Found.", AppRegistry.PrimaryCol));
        if (files.Length > 6) SendConsoleMessage(new ConsoleLine($"  + {files.Length - 6} {(files.Length - 6 == 1 ? "File" : "Files")}", AppRegistry.SecondaryCol));
    }

    ///<summary> Displays info about files within the current workspace directory. </summary>
    public static void ListWorkspaceFiles(int option = 0)
    {
        (string name, FileInfo info)[] files = [.. GetSubFiles(path).Select(x => (x, new FileInfo(path + x))).OrderByDescending(x => x.Item2.Length)];

        if (files.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Files Found.", AppRegistry.PrimaryCol));
            return;
        }

        while (true)
        {
            option = UserInput.CreateMultiPageOptionMenu($"{DisplayPath}", [.. files.Select(x => new ConsoleLine($"{x.name} - {x.info.Length} Bytes", AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 5, option);

            if (option == -1) break; // exit option

            (string name, FileInfo info) = files[option];

            SendConsoleMessage(new ConsoleLine($"--- {DisplayPath}\\{info.Name} ---", AppRegistry.PrimaryCol));
            SendConsoleMessage(new ConsoleLine($"Created On - {info.CreationTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(12), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Last Modified - {info.LastWriteTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(16), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Size - {info.Length} Bytes.", BuildArray(AppRegistry.SecondaryCol.Extend(6), AppRegistry.PrimaryCol)));
            SendConsoleMessage("");

            int fileOption = UserInput.CreateOptionMenu("--- Options ---", [new ConsoleLine("Open File", AppRegistry.SecondaryCol), new ConsoleLine("Remove File", AppRegistry.SecondaryCol), new ConsoleLine("Exit", AppRegistry.PrimaryCol)]);

            ClearLines(5, true);

            if (fileOption == 0) OpenWorkspaceFile(name);
            else if (fileOption == 1)
            {
                DeleteWorkspaceFile(name);
                ListWorkspaceFiles(option);
                return;
            }
            else break;
        }
    }

    ///<summary> Displays info about directories within the current workspace directory. </summary>
    public static void ListWorkspaceDirectories(int option = 0)
    {
        (string name, DirectoryInfo info)[] directories = [.. GetSubDirectories(path).Select(x => (x, new DirectoryInfo(path + x))).OrderByDescending(x => x.Item2.GetFiles().Length)];

        if (directories.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Directories Found.", AppRegistry.PrimaryCol));
            return;
        }

        while (true)
        {
            option = UserInput.CreateMultiPageOptionMenu($"{DisplayPath}", [.. directories.Select(x => new ConsoleLine($"{x.name} - {x.info.GetFiles().Length} Files, {x.info.GetDirectories().Length} Directories", AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 5, option);

            if (option == -1) break; // exit option

            (string name, DirectoryInfo info) = directories[option];

            SendConsoleMessage(new ConsoleLine($"--- {DisplayPath}\\{info.Name} ---", AppRegistry.PrimaryCol));
            SendConsoleMessage(new ConsoleLine($"Created On - {info.CreationTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(12), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Last Modified - {info.LastWriteTime}.", BuildArray(AppRegistry.SecondaryCol.Extend(16), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Directories - {info.GetDirectories().Length}.", BuildArray(AppRegistry.SecondaryCol.Extend(11), AppRegistry.PrimaryCol)));
            SendConsoleMessage(new ConsoleLine($"Files - {info.GetFiles().Length}.", BuildArray(AppRegistry.SecondaryCol.Extend(6), AppRegistry.PrimaryCol)));
            SendConsoleMessage("");

            int directoryOption = UserInput.CreateOptionMenu("--- Options ---", [new ConsoleLine("Open Directory", AppRegistry.SecondaryCol), new ConsoleLine("Remove Directory", AppRegistry.SecondaryCol), new ConsoleLine("Exit", AppRegistry.PrimaryCol)]);

            ClearLines(6, true);

            if (directoryOption == 0)
            {
                UpdatePath(RawPath + $@"{name}\");
                return;
            }
            else if (directoryOption == 1)
            {
                DeleteWorkspaceDirectory(name);
                ListWorkspaceDirectories(option);
                return;
            }
        }
    }

    // --- FILES ---

    ///<summary> Create a workspace file. </summary>
    public static bool CreateWorkspaceFile(string fileName)
    {
        if (fileName.Length == 0 || fileName[0] == '.')
        {
            SendConsoleMessage(new ConsoleLine("File Name Can Not Be Empty.", AppRegistry.PrimaryCol));
            return false;
        }

        WorkspaceQuery q = new(fileName, true);

        if (!IsNameValid(q.Name, true)) return false;
        if (FileExists(q.Location))
        {
            SendConsoleMessage(new ConsoleLine($"File Already Exists {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(22 + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        SaveFile(q.Location, []);
        SendConsoleMessage(new ConsoleLine($"Created Text File {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(20 + q.Info.Length), AppRegistry.SecondaryCol)));
        Analytics.General.FilesCreated++;

        if (q.modifiers["-j"]) OpenWorkspaceFile(fileName); // open the new file

        return true;
    }

    ///<summary> Delete a workspace file. </summary>
    public static bool DeleteWorkspaceFile(string fileName)
    {
        WorkspaceQuery q = new(fileName, true);

        if (!FileExists(q.Location))
        {
            SendConsoleMessage(new ConsoleLine($"File Does Not Exist {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(22 + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        if (!UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Are You Sure You Want To Delete File {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(38 + q.Info.Length), AppRegistry.SecondaryCol)))) return false;

        DeleteFile(q.Location);
        SendConsoleMessage(new ConsoleLine($"Deleted File {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(15 + q.Info.Length), AppRegistry.SecondaryCol)));
        Analytics.General.FilesDeleted++;

        return true;
    }

    ///<summary> Opens a workspace file. </summary>
    public static bool OpenWorkspaceFile(string fileName)
    {
        WorkspaceQuery q = new(fileName, true);

        if (!FileExists(q.Location))
        {
            SendConsoleMessage(new ConsoleLine($"File Does Not Exist {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(20 + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        string fileExtension = Path.GetExtension(q.Name);
        if (!ValidTextFileExtensions.Contains(fileExtension))
        {
            if (fileExtension == ".cimg") // console images
            {
                PaintApp.StaticImage = (q.Location, q.Name);
                AppRegistry.SetActiveApp("Paint");
                ReloadConsole();
                return true;
            }

            SendConsoleMessage(new ConsoleLine($"Can't Open File Extension - '{fileExtension}'", BuildArray(AppRegistry.PrimaryCol.Extend(28), AppRegistry.SecondaryCol)));
            return false;
        }

        string[] content = LoadFile(q.Location);
        SaveFile(q.Location, UserInput.GetMultiUserInput(Path.GetFileNameWithoutExtension(GetTruePathName(q.Location)), content));
        Analytics.General.FilesOpened++;

        return true;
    }

    ///<summary> Runs a workspace file. </summary>
    public static bool RunWorkspaceFile(string fileName)
    {
        WorkspaceQuery q = new(fileName, true);

        if (!FileExists(q.Location))
        {
            SendConsoleMessage(new ConsoleLine($"File Does Not Exist {q.Info}- '{q.Name}'", BuildArray(AppRegistry.PrimaryCol.Extend(20 + q.Info.Length), AppRegistry.SecondaryCol)));
            return false;
        }

        string fileExtension = Path.GetExtension(q.Name);

        if (fileExtension == ".hc")
        {
            HoneyCInterpreter.Interpret(LoadFile(q.Location));
            return true;
        }

        if (fileExtension == ".cimg")
        {
            ConsoleImage? stickerImage = ConsoleImage.LoadFromCIMG(q.Location);
            stickerImage?.Output();
            return true;
        }

        SendConsoleMessage(new ConsoleLine($"Can't Run File Extension - '{fileExtension}'", BuildArray(AppRegistry.PrimaryCol.Extend(28), AppRegistry.SecondaryCol)));
        return false;
    }

    // --- LOW LEVEL FILE SYSTEM ---

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern uint GetFinalPathNameByHandle(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);
    private const uint FILE_NAME_NORMALIZED = 0x0;

    ///<summary> Gets the correct captilisation of a path, from a fileHandle. </summary>
    static string GetTruePathNameByHandle(SafeFileHandle fileHandle)
    {
        StringBuilder outPath = new(1024);

        var size = GetFinalPathNameByHandle(fileHandle, outPath, (uint)outPath.Capacity, FILE_NAME_NORMALIZED);
        if (size == 0 || size > outPath.Capacity)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        // may be prefixed with \\?\, which we don't want
        if (outPath[0] == '\\' && outPath[1] == '\\' && outPath[2] == '?' && outPath[3] == '\\')
            return outPath.ToString(4, outPath.Length - 4);

        return outPath.ToString();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern SafeFileHandle CreateFile(
         [MarshalAs(UnmanagedType.LPTStr)] string filename,
         [MarshalAs(UnmanagedType.U4)] FileAccess access,
         [MarshalAs(UnmanagedType.U4)] FileShare share,
         IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
         [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
         [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
         IntPtr templateFile);
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

    ///<summary> Gets the correct captilisation of a path. </summary>
    public static string GetTruePathName(string dirtyPath)
    {
        // use 0 for access so we can avoid error on our metadata-only query (see dwDesiredAccess docs on CreateFile)
        // use FILE_FLAG_BACKUP_SEMANTICS for attributes so we can operate on directories (see Directories in remarks section for CreateFile docs)

        using (var directoryHandle = CreateFile(
            dirtyPath, 0, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open,
            (FileAttributes)FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero))
        {
            if (directoryHandle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return GetTruePathNameByHandle(directoryHandle);
        }
    }
}