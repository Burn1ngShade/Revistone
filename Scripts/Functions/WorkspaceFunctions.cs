using Revistone.Console;
using Revistone.Interaction;

using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Console.ConsoleAction;
using Revistone.App;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Revistone.Functions;

public static class WorkspaceFunctions
{
    static DirectoryInfo dir = new(path ?? @"PersistentData\Workspace\");
    static string path = @"PersistentData\Workspace\";
    public static string DisplayPath => dir.FullName[(dir.FullName.IndexOf(path, StringComparison.OrdinalIgnoreCase) + 15)..^1];

    public static bool CreateWorkspaceDirectory(string dirName)
    {
        if (!IsNameValid(dirName, true)) return false;
        if (DirectoryExists(path + dirName))
        {
            SendConsoleMessage(new ConsoleLine($"Directory Already Exists - '{dirName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        CreateDirectory(path + dirName);
        SendConsoleMessage(new ConsoleLine($"Created Directory - '{dirName}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool DeleteWorkspaceDirectory(string dirName)
    {
        if (!DirectoryExists(path + dirName) || dirName == "")
        {
            SendConsoleMessage(new ConsoleLine($"Directory Does Not Exist - '{dirName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        if (!UserInput.CreateTrueFalseOptionMenu($"Are You Sure You Want To Delete Directory: {dirName}")) return false;

        DeleteDirectory(path + dirName);
        SendConsoleMessage(new ConsoleLine($"Deleted Directory - '{dirName}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool ChangeWorkspaceDirectory(string dirName)
    {
        if (!DirectoryExists(path + dirName) || dirName == "")
        {
            SendConsoleMessage(new ConsoleLine($"Directory Does Not Exist - '{dirName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        path = GetFinalPathName(path + $@"{dirName}\");
        path = path[path.IndexOf(@"PersistentData\Workspace\", StringComparison.OrdinalIgnoreCase)..];
        path += @"\";
        dir = new(path);
        SendConsoleMessage(new ConsoleLine($"Changed To Directory - '{DisplayPath}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool ChangeBackWorkspaceDirectory()
    {
        if (path == @"PersistentData\Workspace\")
        {
            SendConsoleMessage(new ConsoleLine("Already Within The Root Directory.", AppRegistry.PrimaryCol));
            return false;
        }

        path = GetFinalPathName(path[..path[..^1].LastIndexOf('\\')]);
        path += @"\";
        path = path[path.IndexOf(@"PersistentData\Workspace\", StringComparison.OrdinalIgnoreCase)..];
        dir = new(path);
        SendConsoleMessage(new ConsoleLine($"Changed To Directory - '{DisplayPath}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool ChangeRootWorkspaceDirectory()
    {
        if (path == @"PersistentData\Workspace\")
        {
            SendConsoleMessage(new ConsoleLine("Already Within The Root Directory.", AppRegistry.PrimaryCol));
            return false;
        }

        path = @"PersistentData\Workspace\";
        dir = new(path);
        SendConsoleMessage(new ConsoleLine($"Changed To Directory - '{DisplayPath}'.", AppRegistry.PrimaryCol));
        return true;
    }

    public static void GetWorkspaceDirectoryContents()
    {
        SendConsoleMessage(new ConsoleLine($"--- {DisplayPath} ---", AppRegistry.PrimaryCol));

        string[] directories = GetSubDirectories(path);
        string[] files = GetSubFiles(path);

        SendConsoleMessage(new ConsoleLine("Directories:", AppRegistry.SecondaryCol));
        foreach (string dir in directories)
        {
            DirectoryInfo info = new DirectoryInfo(path + dir);
            SendConsoleMessage(new ConsoleLine($" {dir} - {info.GetFiles().Length} Files, {info.GetDirectories().Length} Directories.", AppRegistry.PrimaryCol));
        }
        if (directories.Length == 0) SendConsoleMessage(new ConsoleLine("No Directories Found.", AppRegistry.PrimaryCol));

        SendConsoleMessage(new ConsoleLine("Files:", AppRegistry.SecondaryCol));
        foreach (string file in files)
        {
            FileInfo info = new FileInfo(path + file);
            SendConsoleMessage(new ConsoleLine($" {file} - {info.Length} Bytes", AppRegistry.PrimaryCol));
        }
        if (files.Length == 0) SendConsoleMessage(new ConsoleLine("No Files Found.", AppRegistry.PrimaryCol));

    }

    public static bool CreateWorkspaceFile(string fileName)
    {
        string fileExtension = Path.GetExtension(fileName);
        if (fileExtension.Length == 0) fileName += ".txt";

        if (!IsNameValid(fileName, true)) return false;
        if (FileExists(path + fileName))
        {
            SendConsoleMessage(new ConsoleLine($"File Already Exists - '{fileName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        SaveFile(path + fileName, []);
        SendConsoleMessage(new ConsoleLine($"Created Text File - '{fileName}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool DeleteWorkspaceFile(string fileName)
    {
        if (!FileExists(path + fileName) || fileName == "")
        {
            SendConsoleMessage(new ConsoleLine($"File Does Not Exist - '{fileName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        if (!UserInput.CreateTrueFalseOptionMenu($"Are You Sure You Want To Delete File: {fileName}")) return false;

        DeleteFile(path + fileName);
        SendConsoleMessage(new ConsoleLine($"Deleted File - '{fileName}'.", AppRegistry.PrimaryCol));

        return true;
    }

    public static bool OpenWorkspaceFile(string fileName)
    {
        if (!FileExists(path + fileName) || fileName == "")
        {
            SendConsoleMessage(new ConsoleLine($"File Does Not Exist - '{fileName}'.", AppRegistry.PrimaryCol));
            return false;
        }

        string fileExtension = Path.GetExtension(fileName);
        if (fileExtension != ".txt" && fileExtension != ".hc")
        {
            SendConsoleMessage(new ConsoleLine($"Can't Open File Extension: '{fileExtension}'.", AppRegistry.PrimaryCol));
            return false;
        }

        string[] content = LoadFile(path + fileName);
        SaveFile(path + fileName, UserInput.MultiLineEdit(Path.GetFileNameWithoutExtension(GetFinalPathName(path + fileName)), content));

        return true;
    }

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern uint GetFinalPathNameByHandle(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);
    private const uint FILE_NAME_NORMALIZED = 0x0;

    static string GetFinalPathNameByHandle(SafeFileHandle fileHandle)
    {
        StringBuilder outPath = new StringBuilder(1024);

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

    public static string GetFinalPathName(string dirtyPath)
    {
        // use 0 for access so we can avoid error on our metadata-only query (see dwDesiredAccess docs on CreateFile)
        // use FILE_FLAG_BACKUP_SEMANTICS for attributes so we can operate on directories (see Directories in remarks section for CreateFile docs)

        using (var directoryHandle = CreateFile(
            dirtyPath, 0, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open,
            (FileAttributes)FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero))
        {
            if (directoryHandle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return GetFinalPathNameByHandle(directoryHandle);
        }
    }
}