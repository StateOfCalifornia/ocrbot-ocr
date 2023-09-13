using CDT.Accessibility.Documents;
using CDT.Helpers;
using CDT.Log;
using CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDT.Accessibility.UI {
	public class ConsoleApp {
		private static string OutputLog { get; set; }

		public static void RunWithOptions(string[] args) {
			Parser.Default
				.ParseArguments<Options>(args)
				.WithParsed(o => {
					//map cmd args to SessionConfig
					var sessionConfig = new SessionConfig() {
						InputFiles = o.InputFiles,
						InputDirectory = o.InputDirectory,
						OutputDirectory = o.OutputDirectory,
						DoAutoTag = o.DoAutotag,
						ShowTextOnly = o.ShowTextOnly,
						OverwriteDestination = o.Overwrite,
						UseOCRCache = o.UseOCRCache
					};

					ConsoleApp.OutputLog = o.LogFile;
					Program.Process(sessionConfig, ConsoleApp.LogMessage);

					if (o.WaitForExit) {
						Console.WriteLine("Done..hit key to exit");
						Console.ReadKey();
					}
				});
		}


		public static void LogMessage(LogEntry logEntry) {
			//write to log file
			Console.WriteLine(logEntry);
			File.AppendAllText(OutputLog, $"{logEntry}{Environment.NewLine}");
		}
	}







	// Command line optionss
	public class Options {
		[Option('F', "InputFiles", Required=true, SetName = "InputByFiles", HelpText = "Input files to be processed. (Need -F or -D)")]
		public IEnumerable<string> InputFiles { get; set; }

		[Option('D', "InputDirectory", Required = true, SetName = "InputByDirectory", HelpText = "Input Directory to be processed. (Need -F or -D)")]
		public string InputDirectory { get; set; }

		[Option('T', "OutputDirectory", Required = true, HelpText = "Target directory")]
		public string OutputDirectory { get; set; }

		[Option('A', "DoAutotag", Required = false, HelpText = "DoAutoag [true|false]")]
		public bool DoAutotag { get; set; }

		[Option('L', "LogFile", Required = true, HelpText = "Log File")]
		public string LogFile { get; set; }

		[Option('W', "WaitForExit", Required = false, HelpText = "Wait For Exit [true|false")]
		public bool WaitForExit { get; set; }

		[Option('t', "ShowTextOnly", Required = false, HelpText = "Show Text Only [true|false]")]
		public bool ShowTextOnly { get; set; }

		[Option('O', "Overwrite", Required = false, HelpText = "Overwrite Destination")]
		public bool Overwrite { get; set; }

		[Option('C', "UseOCRCache", Required = false, HelpText = "Uses OCR cache if available")]
		public bool UseOCRCache { get; set; }

		// Omitting long name, defaults to name of property, ie "--verbose"
		[Option(
			Default = false,
			HelpText = "Prints all messages to standard output.")]
		public bool Verbose { get; set; }
	}
}
