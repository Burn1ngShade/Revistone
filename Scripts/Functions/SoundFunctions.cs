using System.Runtime.InteropServices;
using Revistone.Management;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Functions;

/// <summary> Class filled with functions unrelated to console, but useful for sound manipulation. </summary>
public static class SoundFunctions
{
    ///<summary> Play sound from console assets of given fileName, volume from 0 - 1. </summary>
    public static bool PlaySound(string fileName, float volume = 0.5f)
    {
        if (!fileName.EndsWith(".wav")) fileName += ".wav";
        string path = GeneratePath(DataLocation.Console, $"Assets/Sounds/{fileName}");

        PlayWav(path, volume);
        return true;
    }

    const int CALLBACK_NULL = 0x00000000;

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEFORMATEX // WAV format
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEHDR // WAV info
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public uint dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public uint reserved;
    }

    [DllImport("winmm.dll")]
    static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat, int dwCallback, int dwInstance, int dwFlags); // open WAV connection.

    [DllImport("winmm.dll")]
    static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize); // prep wav info.

    [DllImport("winmm.dll")]
    static extern int waveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize); // output audio.

    [DllImport("winmm.dll")]
    static extern int waveOutClose(IntPtr hWaveOut); // close WAV connection.

    [DllImport("winmm.dll")]
    static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume); // set volume of WAV output.

    ///<summary> Play WAV sound file. </summary>
    static void PlayWav(string path, float volume)
    {
        byte[] wavData = File.ReadAllBytes(path);
        using (MemoryStream stream = new MemoryStream(wavData))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            // Skip RIFF header
            reader.ReadBytes(12); // "RIFF" + size + "WAVE"

            // Find "fmt " chunk
            string chunkID = new string(reader.ReadChars(4));
            while (chunkID != "fmt ")
            {
                int chunkSize = reader.ReadInt32();
                reader.ReadBytes(chunkSize);
                chunkID = new string(reader.ReadChars(4));
            }

            int fmtSize = reader.ReadInt32();
            WAVEFORMATEX format = new WAVEFORMATEX
            {
                wFormatTag = reader.ReadUInt16(),
                nChannels = reader.ReadUInt16(),
                nSamplesPerSec = reader.ReadUInt32(),
                nAvgBytesPerSec = reader.ReadUInt32(),
                nBlockAlign = reader.ReadUInt16(),
                wBitsPerSample = reader.ReadUInt16(),
                cbSize = 0
            };

            if (fmtSize > 16)
                reader.ReadBytes(fmtSize - 16);

            // Find "data" chunk
            chunkID = new string(reader.ReadChars(4));
            while (chunkID != "data")
            {
                int chunkSize = reader.ReadInt32();
                reader.ReadBytes(chunkSize);
                chunkID = new string(reader.ReadChars(4));
            }

            int dataSize = reader.ReadInt32();
            byte[] audioData = reader.ReadBytes(dataSize);

            // Play buffer
            Thread t = new(() => PlayBuffer(audioData, format, volume));
            t.Start();
        }
    }

    static void PlayBuffer(byte[] audioData, WAVEFORMATEX format, float volume)
    {
        IntPtr hWaveOut;
        int result = waveOutOpen(out hWaveOut, -1, ref format, 0, 0, CALLBACK_NULL);
        if (result != 0)
        {
            Analytics.Debug.Log($"WaveOutOpen failed: {result}");
            return;
        }

        volume = Math.Clamp(volume, 0f, 1f);
        ushort volumeLevel = (ushort)(volume * 0xFFFF);
        uint combinedVolume = ((uint)volumeLevel) | ((uint)volumeLevel << 16);
        waveOutSetVolume(hWaveOut, combinedVolume);

        IntPtr dataPtr = Marshal.AllocHGlobal(audioData.Length);
        Marshal.Copy(audioData, 0, dataPtr, audioData.Length);

        WAVEHDR header = new WAVEHDR
        {
            lpData = dataPtr,
            dwBufferLength = (uint)audioData.Length,
            dwFlags = 0,
            dwLoops = 0
        };

        waveOutPrepareHeader(hWaveOut, ref header, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
        waveOutWrite(hWaveOut, ref header, (uint)Marshal.SizeOf(typeof(WAVEHDR)));

        // Sleep until playback finishes 
        int durationMs = (int)(audioData.Length * 1000.0 / format.nAvgBytesPerSec);
        Thread.Sleep(durationMs + 500); // add buffer

        waveOutClose(hWaveOut);
        Marshal.FreeHGlobal(dataPtr);

    }
}