using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CryMediaAPI
{
    /// <summary>
    /// FFmpeg wrapper
    /// </summary>
    public static class FFmpegWrapper
    {
        static readonly Regex CodecRegex = new Regex(@"(?<type>[VAS\.])[F\.][S\.][X\.][B\.][D\.] (?<codec>[a-zA-Z0-9_-]+)\W+(?<description>.*)\n?", RegexOptions.Compiled);
        static readonly Regex FormatRegex = new Regex(@"(?<type>[D\s][E\s]) (?<format>[a-zA-Z0-9_\-,]+)\W+(?<description>.*)\n?", RegexOptions.Compiled);

        /// <summary>
        /// Run given command (arguments) using the given executable name or path
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="prettify">Add new lines to output/error.</param>
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

        public static Stream OpenOutput(string executable, string command, out Process process)
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = $"{command}"
            });

            process.BeginErrorReadLine();

            return process.StandardOutput.BaseStream;
        }
        public static Stream OpenOutput(string executable, string command) => OpenOutput(executable, command, out _);

        public static Stream OpenInput(string executable, string command, out Process process, bool showOutput = false)
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = !showOutput,
                RedirectStandardOutput = !showOutput,
                CreateNoWindow = !showOutput,
                Arguments = $"{command}"
            });

            if (!showOutput)
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }

            return process.StandardInput.BaseStream;
        }
        public static Stream OpenInput(string executable, string command, bool showOutput = false) => OpenInput(executable, command, out _, showOutput); 
        
        public static (Stream input, Stream output) Open(string executable, string command, out Process process, bool showOutput = false)
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = !showOutput,
                RedirectStandardOutput = true,
                CreateNoWindow = !showOutput,
                Arguments = $"{command}"
            });

            if (!showOutput)
            {
                process.BeginErrorReadLine();
            }

            return (process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
        }
        public static (Stream input, Stream output) Open(string executable, string command, bool showOutput = false)
            => Open(executable, command, out _, showOutput);

        public static Dictionary<string, (string Description, MediaType Type)> GetEncoders(string ffmpegExecutable = "ffmpeg")
        {
            var data = new Dictionary<string, (string Description, MediaType Type)>();
            var r = RunCommand(ffmpegExecutable, "-encoders -v quiet", true);
            var mtc = CodecRegex.Matches(r.output);
            foreach (Match m in mtc)
            {
                char t = m.Groups["type"].Value[0];
                data.Add(m.Groups["codec"].Value, (m.Groups["description"].Value, t == 'A' ? MediaType.Audio : (t == 'V' ? MediaType.Video : MediaType.Subtitle)));
            }
            return data;
        }
        public static Dictionary<string, (string Description, MediaType Type)> GetDecoders(string ffmpegExecutable = "ffmpeg")
        {
            var data = new Dictionary<string, (string Description, MediaType Type)>();
            var r = RunCommand(ffmpegExecutable, "-decoders -v quiet", true);
            var mtc = CodecRegex.Matches(r.output);
            foreach (Match m in mtc)
            {
                char t = m.Groups["type"].Value[0];
                data.Add(m.Groups["codec"].Value, (m.Groups["description"].Value, t == 'A' ? MediaType.Audio : (t == 'V' ? MediaType.Video : MediaType.Subtitle)));
            }
            return data;
        }
        public static Dictionary<string, (string Description, MuxingSupport Support)> GetFormats(string ffmpegExecutable = "ffmpeg")
        {
            var data = new Dictionary<string, (string Description, MuxingSupport Support)>();
            var r = RunCommand(ffmpegExecutable, "-formats -v quiet", true);
            var mtc = FormatRegex.Matches(r.output);
            foreach (Match m in mtc)
            {
                string t = m.Groups["type"].Value.Trim();
                data.Add(m.Groups["format"].Value, (m.Groups["description"].Value, 
                    t == "DE" ? MuxingSupport.MuxDemux : (t == "D" ? MuxingSupport.Demux : MuxingSupport.Mux)));
            }
            return data;
        }

        public enum MediaType
        {
            Video,
            Audio,
            Subtitle
        }

        public enum MuxingSupport
        {
            MuxDemux,
            Mux,
            Demux
        }
    }
}
