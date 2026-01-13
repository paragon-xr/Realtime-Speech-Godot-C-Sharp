using Godot;
using System;
using System.Runtime.InteropServices;
using System.IO;

public static class VoskNativeLoader
{
    public static void Load()
    {
        string osName = OS.GetName();

        string platformFolder =
            osName == "Windows" ? "Windows-x64" :
            osName == "macOS" ? "macOS-universal" :
            osName == "Linux" ? "Linux-x86_64" :
            osName == "Android" ? "Android-arm64" :
                                  "Unknown";

        string libName =
            osName == "Windows" ? "libvosk.dll" :
            osName == "macOS" ? "libvosk.dylib" :
            (osName == "Linux" || osName == "Android") ? "libvosk.so" :
                                                         "Unknown";

        string resPath = $"res://RealtimeSpeech/vosklibs/{platformFolder}/{libName}";
        GD.Print("Vosk native lib path: ", resPath);

        Godot.FileAccess fa = Godot.FileAccess.Open(resPath, Godot.FileAccess.ModeFlags.Read);
        if (fa == null)
        {
            GD.PrintErr("Failed to open Vosk library at: ", resPath);
            return;
        }

        byte[] data = fa.GetBuffer((long)fa.GetLength());
        fa.Close();

        string tempPath;

        if (osName == "Windows")
        {
            // Put the DLL next to the executable / game binary
            string baseDir = AppContext.BaseDirectory;
            tempPath = Path.Combine(baseDir, libName);
        }
        else
        {
            tempPath = Path.Combine(Path.GetTempPath(), libName);
        }

        File.WriteAllBytes(tempPath, data);

        GD.Print("Extracted Vosk native lib to: ", tempPath);

        if (osName == "Windows")
        {
            try
            {
                NativeLibrary.Load(tempPath);
                GD.Print("Vosk native lib preloaded on Windows.");
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to preload Vosk native lib on Windows: ", e.Message);
            }
        }

        NativeLibrary.SetDllImportResolver(typeof(Vosk.Model).Assembly, (name, assembly, path) =>
        {
            if (name == "libvosk")
                return NativeLibrary.Load(tempPath);

            return IntPtr.Zero;
        });

        GD.Print("Vosk native resolver registered.");
    }
}
