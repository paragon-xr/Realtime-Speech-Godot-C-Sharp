using Godot;
using System;
using System.IO;

public static class VoskModelLoader
{
    public static string ExtractModel(string resModelPath)
    {
        if (!resModelPath.EndsWith("/"))
            resModelPath += "/";

        string dstRoot = Path.Combine(Path.GetTempPath(), "vosk_model");
        GD.Print("Extracting Vosk model to: ", dstRoot);

        if (!Directory.Exists(dstRoot))
            Directory.CreateDirectory(dstRoot);

        CopyDirRecursive(resModelPath, dstRoot);
        return dstRoot;
    }

    private static void CopyDirRecursive(string srcRes, string dstFs)
    {
        var dir = DirAccess.Open(srcRes);
        if (dir == null)
        {
            GD.PrintErr("Failed to open model directory: ", srcRes);
            return;
        }

        Directory.CreateDirectory(dstFs);

        dir.ListDirBegin();
        while (true)
        {
            string name = dir.GetNext();
            if (name == "")
                break;

            if (name == "." || name == "..")
                continue;

            string srcPath = srcRes + name;
            string dstPath = Path.Combine(dstFs, name);

            if (dir.CurrentIsDir())
            {
                CopyDirRecursive(srcPath + "/", dstPath);
            }
            else
            {
                Godot.FileAccess fa = Godot.FileAccess.Open(srcPath, Godot.FileAccess.ModeFlags.Read);
                if (fa == null)
                {
                    GD.PrintErr("Failed to open model file: ", srcPath);
                    continue;
                }

                byte[] data = fa.GetBuffer((long)fa.GetLength());
                fa.Close();

                File.WriteAllBytes(dstPath, data);
            }
        }

        dir.ListDirEnd();
    }
}
