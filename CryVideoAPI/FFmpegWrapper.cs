using System;
using System.IO;
using System.Diagnostics;

namespace CryVideoAPI
{
    internal static class FFmpegWrapper
    {
        public static (string output, string error) RunCommand(string executable, string command, bool prettify = true)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = $"{command}"
            });

            string output = "", error = "";
            p.OutputDataReceived += (a, d) => output += d.Data + (prettify ? "\n" : "");
            p.ErrorDataReceived += (a, d) => output += d.Data + (prettify ? "\n" : "");
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
            return (output, error);
        }

        public static Stream OpenOutput(string executable, string command)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = $"{command}"
            });

            p.BeginErrorReadLine();

            return p.StandardOutput.BaseStream;
        }

        public static Stream OpenInput(string executable, string command, bool showOutput = false)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = !showOutput,
                RedirectStandardOutput = !showOutput,
                Arguments = $"{command}"
            });

            if (!showOutput)
            {
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
            }

            return p.StandardInput.BaseStream;
        }
    }
}
