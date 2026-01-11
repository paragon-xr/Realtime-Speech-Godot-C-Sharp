using Godot;
using System;
using System.Runtime.InteropServices;

public static class VoskNativeLoader
{
    public static void Load()
    {
        string platformFolder =
            OS.GetName() == "Windows" ? "Windows-x64" :
            OS.GetName() == "macOS" ? "macOS-universal" :
                                        "Linux-x86_64";

        string libName =
            OS.GetName() == "Windows" ? "libvosk.dll" :
            OS.GetName() == "macOS" ? "libvosk.dylib" :
                                        "libvosk.so";

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

        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), libName);
        System.IO.File.WriteAllBytes(tempPath, data);

        GD.Print("Extracted Vosk native lib to: ", tempPath);

        NativeLibrary.SetDllImportResolver(typeof(Vosk.Model).Assembly, (name, assembly, path) =>
        {
            if (name == "libvosk")
                return NativeLibrary.Load(tempPath);

            return IntPtr.Zero;
        });

        GD.Print("Vosk native resolver registered.");
    }
}
