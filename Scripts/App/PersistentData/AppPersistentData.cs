namespace Revistone
{
    namespace Apps
    {
        /// <summary> Class pertaining all logic for easy saving of data for apps. </summary>
        public static class AppPersistentData
        {
            public enum SaveType { Overwrite, PartialOverwrite, Append, Insert }

            static string dataPath = "Scripts/App/PersistentData/";

            // --- Directorys ---

            /// <summary> Creates directory with given name. </summary>
            public static void CreateDirectory(string directoryName)
            {
                if (!Directory.Exists($"{dataPath}{directoryName}")) Directory.CreateDirectory($"{dataPath}{directoryName}");
            }

            /// <summary> Deletes directory with given name. </summary>
            public static void DeleteDirectory(string directoryName)
            {
                if (Directory.Exists($"{dataPath}{directoryName}")) Directory.Delete($"{dataPath}{directoryName}");
            }

            /// <summary> Checks for directory with given name. </summary>
            public static bool DirectoryExists(string directoryName) { return Directory.Exists($"{dataPath}{directoryName}"); }

            // --- FILE MANAGEMENT ---

            /// <summary> Creates file with given name. </summary>
            public static void CreateFile(string directoryName, string fileName)
            {
                if (!DirectoryExists(directoryName)) CreateDirectory(directoryName);
                if (!File.Exists($"{dataPath}{directoryName}/{fileName}")) File.CreateText($"{dataPath}{directoryName}/{fileName}").Close();
            }

            /// <summary> Deletes file with given name. </summary>
            public static void DeleteFile(string directoryName, string fileName)
            {
                if (!DirectoryExists(directoryName)) return;
                if (File.Exists($"{dataPath}{directoryName}/{fileName}")) File.Delete($"{dataPath}{directoryName}/{fileName}");
            }

            /// <summary> Checks for file with given name. </summary>
            public static bool FileExists(string directoryName, string fileName)
            {
                if (!DirectoryExists(directoryName)) return false;
                return File.Exists($"{dataPath}{directoryName}/{fileName}");
            }

            // --- FILES SAVE ---

            /// <summary> Saves fileData to given file with given name (startIndex has no effect on SaveType Overwrite or Append). </summary>
            public static bool SaveFile(string directoryName, string fileName, string[] fileData, int startIndex = 0, SaveType saveType = SaveType.Overwrite)
            {
                if (!FileExists(directoryName, fileName)) CreateFile(directoryName, fileName);

                string[] currentTextFile = File.ReadAllLines($"{dataPath}{directoryName}/{fileName}");

                switch (saveType)
                {
                    case SaveType.Overwrite:
                        File.WriteAllLines($"{dataPath}{directoryName}/{fileName}", fileData);
                        return true;
                    case SaveType.PartialOverwrite:
                        string[] newTextFile = new string[Math.Max(startIndex + fileData.Length, currentTextFile.Length)];
                        Array.Copy(currentTextFile, newTextFile, currentTextFile.Length);
                        Array.Copy(fileData, 0, newTextFile, startIndex, fileData.Length);
                        File.WriteAllLines($"{dataPath}{directoryName}/{fileName}", newTextFile);
                        return true;
                    case SaveType.Append:
                        File.AppendAllLines($"{dataPath}{directoryName}/{fileName}", fileData);
                        return true;
                    case SaveType.Insert:
                        string[] insertedTextFile = new string[currentTextFile.Length + fileData.Length];
                        Array.Copy(currentTextFile, 0, insertedTextFile, 0, startIndex);
                        Array.Copy(fileData, 0, insertedTextFile, startIndex, fileData.Length);
                        Array.Copy(currentTextFile, startIndex, insertedTextFile, startIndex + fileData.Length, currentTextFile.Length - startIndex);
                        File.WriteAllLines($"{dataPath}{directoryName}/{fileName}", insertedTextFile);
                        return true;

                }

                return false;
            }

            // --- FILE LOAD ---

            /// <summary> Loads data from given file, within a start index and given length. </summary>
            public static string[] LoadFile(string directoryName, string fileName, int startIndex = 0, int length = -1)
            {
                if (!FileExists(directoryName, fileName)) CreateFile(directoryName, fileName);

                return File.ReadAllLines($"{dataPath}{directoryName}/{fileName}").Skip(startIndex).Take(length > 0 ? length : int.MaxValue).ToArray();
            }

            /// <summary> Loads data from given file, within a start index and given length, and splits it into arrays of given length. </summary>
            public static string[][] LoadFile(string directoryName, string fileName, int arrayLength, bool removePartialArrays, int startIndex = 0, int length = -1)
            {
                if (!FileExists(directoryName, fileName)) CreateFile(directoryName, fileName);

                if (arrayLength < 1) arrayLength = 1;

                string[][] s = File.ReadAllLines($"{dataPath}{directoryName}/{fileName}").Skip(startIndex).Take(length > 0 ? length : int.MaxValue)
                .Select((line, index) => new { line, index })
                .GroupBy(x => x.index / arrayLength)
                .Select(group => group.Select(x => x.line).ToArray())
                .ToArray();

                if (s.Length > 0 && removePartialArrays && s[s.Length - 1].Length != arrayLength) return s.SkipLast(1).ToArray();
                else return s;
            }

            /// <summary> Loads data from given file, and given line. </summary>
            public static string LoadFile(string directoryName, string fileName, int lineIndex)
            {
                if (!FileExists(directoryName, fileName)) CreateFile(directoryName, fileName);

                string[] s = File.ReadAllLines($"{dataPath}{directoryName}/{fileName}");

                return lineIndex > s.Length - 1 ? "" : s[lineIndex];
            }
        }
    }
}