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
        /// <summary>
        /// FFmpeg verbosity. This sets the 'loglevel' parameter on FFmpeg. Useful when showing output and debugging issues.
        /// This may affect the progress tracker that depends on displayed stats. Default is 'info'.
        /// </summary>
        public static Verbosity LogLevel { get; set; } = Verbosity.Info;

        static readonly Regex CodecRegex = new Regex(@"(?<type>[VAS\.])[F\.][S\.][X\.][B\.][D\.] (?<codec>[a-zA-Z0-9_-]+)\W+(?<description>.*)\n?");
        static readonly Regex FormatRegex = new Regex(@"(?<type>[DE]{1,2})\s+?(?<format>[a-zA-Z0-9_\-,]+)\W+(?<description>.*)\n?");

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
                Arguments = $"-loglevel {LogLevel.ToString().ToLowerInvariant()} {command}"
            });

            string output = "", error = "";
            p.OutputDataReceived += (a, d) => output += d.Data + (prettify ? "\n" : "");
            p.ErrorDataReceived += (a, d) => output += d.Data + (prettify ? "\n" : "");
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
            return (output, error);
        }

        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This does not wait for the process to exit or return the output.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public static Process ExecuteCommand(string executable, string command, bool showOutput = false)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = !showOutput,
                CreateNoWindow = !showOutput,
                Arguments = $"-loglevel {LogLevel.ToString().ToLowerInvariant()} {command}"
            });

            if (!showOutput)
            {
                p.BeginErrorReadLine();
            }

            return p;
        }

        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the output and error streams and returns the output stream.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public static Stream OpenOutput(string executable, string command, out Process process, bool showOutput = false)
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardError = !showOutput,
                RedirectStandardOutput = true,
                CreateNoWindow = !showOutput,
                Arguments = $"-loglevel {LogLevel.ToString().ToLowerInvariant()} {command}"
            });

            if (!showOutput) process.BeginErrorReadLine();

            return process.StandardOutput.BaseStream;
        }
        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the output and error streams and returns the output stream.
        /// This does not return any FFmpeg process.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public static Stream OpenOutput(string executable, string command, bool showOutput = false) => OpenOutput(executable, command, out _, showOutput);

        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the input stream and returns it.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public static Stream OpenInput(string executable, string command, out Process process, bool showOutput = false)
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = !showOutput,
                CreateNoWindow = !showOutput,
                Arguments = $"-loglevel {LogLevel.ToString().ToLowerInvariant()} {command}"
            });

            if (!showOutput)
            {
                process.BeginErrorReadLine();
            }

            return process.StandardInput.BaseStream;
        }
        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the input stream and returns it.
        /// This does not return any FFmpeg process.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
        public static Stream OpenInput(string executable, string command, bool showOutput = false) => OpenInput(executable, command, out _, showOutput);

        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the input and output streams and returns them.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="process">FFmpeg process</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
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
                Arguments = $"-loglevel {LogLevel.ToString().ToLowerInvariant()} {command}"
            });

            if (!showOutput)
            {
                process.BeginErrorReadLine();
            }

            return (process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
        }
        /// <summary>
        /// Run given command (arguments) using the given executable name or path. This redirects the input and output streams and returns them.
        /// This does not return any FFmpeg process.
        /// </summary>
        /// <param name="executable">Executable name or path</param>
        /// <param name="command">Command to run. This string will be passed as an argument to the executable</param>
        /// <param name="showOutput">Show output to terminal. Error stream will not be redirected if this is set to true.</param>
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
            var r = RunCommand(ffmpegExecutable, "-formats -v quiet -loglevel silent", true);
            var mtc = FormatRegex.Matches(r.output);
            foreach (Match m in mtc)
            {
                string t = m.Groups["type"].Value.Trim();
                data.Add(m.Groups["format"].Value, (m.Groups["description"].Value,
                    t == "DE" ? MuxingSupport.MuxDemux : (t == "D" ? MuxingSupport.Demux : MuxingSupport.Mux)));
            }
            return data;
        }

        /// <summary>
        /// Take a running FFmpeg process with a redirected Error stream and try to parse progress. Requires the total media duration in seconds.
        /// May not work on certain loglevels.
        /// </summary>
        /// <param name="ffmpegProcess">Running FFmpeg process with redirected Error stream</param>
        /// <param name="duration">Media duration in seconds</param>
        public static Progress<double> RegisterProgressTracker(Process ffmpegProcess, double duration)
        {
            var prg = new Progress<double>();
            var iprg = (IProgress<double>)prg;

            var rgx = new Regex(@"^(frame=\s*?(?<frame>\d+)\s*?)?(fps=\s*?\d+\.?\d*?\s+?)?(q=\s*?[\-0-9\.]+\s*?)?\w+?=\s*?\d+[kMBGTb]+\s*?time=(?<h>\d+):(?<m>\d+):(?<s>[0-9\.]+?)\s", RegexOptions.Compiled);

            ffmpegProcess.ErrorDataReceived += (sender, d) =>
            {
                if (string.IsNullOrEmpty(d.Data)) return;

                var match = rgx.Match(d.Data);
                if (match.Success)
                {
                    var hours = int.Parse(match.Groups["h"].Value);
                    var minutes = int.Parse(match.Groups["m"].Value);
                    var seconds = double.Parse(match.Groups["s"].Value);
                    seconds = seconds + (60 * minutes) + (60 * 60 * hours);

                    var progress = (seconds / duration) * 100;
                    if (progress > 100) progress = 100;

                    iprg.Report(progress);
                }
            };

            return prg;
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

        public enum Verbosity
        {
            /// <summary>
            /// Show nothing at all; be silent.
            /// </summary>
            Quiet,
            /// <summary>
            /// Show informative messages during processing. This is in addition to warnings and errors. This is the default value.
            /// </summary>
            Info,
            /// <summary>
            /// Same as info, except more verbose. 
            /// </summary>
            Verbose,
            /// <summary>
            /// Show everything, including debugging information. 
            /// </summary>
            Debug,
            /// <summary>
            /// Show all warnings and errors. Any message related to possibly incorrect or unexpected events will be shown. 
            /// </summary>
            Warning,
            /// <summary>
            /// Show all errors, including ones which can be recovered from. 
            /// </summary>
            Error,
            /// <summary>
            /// Only show fatal errors. These are errors after which the process absolutely cannot continue. 
            /// </summary>
            Fatal
        }
    }
}
