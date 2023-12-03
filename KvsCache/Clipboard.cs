using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KvsCache;

// copied from and adjusted / credits to: https://stackoverflow.com/a/45338239/669692
public static class OperatingSystem
{
    public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}

public static class Shell
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        return Run("/bin/bash", $"-c \"{escapedArgs}\"");
    }

    public static string Bat(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        return Run("cmd.exe", $"/c \"{escapedArgs}\"");
    }

    private static string Run (string filename, string arguments){
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            }
        };
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }
}

public static class Clipboard
{
    public static void SetText(string val)
    {
        if (OperatingSystem.IsWindows())
        {
            $"echo|set /p dummyName={val}|clip".Bat();
        }
        else if (OperatingSystem.IsMacOS())
        {
            $"echo -n \"{val}\" | pbcopy".Bash();
        }
    }
}
