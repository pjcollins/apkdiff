﻿using System;
using System.IO;
using Mono.Options;

using static System.Console;

namespace apkdiff {
	class Program {
		static readonly string Name = "apkdiff";

		public static string Comment;
		public static bool SaveDescriptions;
		public static bool Verbose;

		public static long AssemblyRegressionThreshold;
		public static long ApkRegressionThreshold;
		public static bool RegressionFlag;

		public static void Main (string [] args)
		{
			var (path1, path2) = ProcessArguments (args);

			var desc1 = ApkDescription.Load (path1);

			if (path2 != null) {
				var desc2 = ApkDescription.Load (path2);

				desc1.Compare (desc2);

				if (ApkRegressionThreshold != 0 && (desc2.PackageSize - desc1.PackageSize) > ApkRegressionThreshold) {
					Error ($"PackageSize differ more than {ApkRegressionThreshold} bytes. apk1 size: {desc1.PackageSize} bytes, apk2 size: {desc2.PackageSize} bytes.");
					RegressionFlag = true;
				}
			}

			if (RegressionFlag) {
				Error ("Size regression occured, test failed.");
				Environment.Exit (3);
			}
		}

		static (string, string) ProcessArguments (string [] args)
		{
			var help = false;
			var options = new OptionSet {
				$"Usage: {Name}.exe OPTIONS* <package1.apk[desc]> [<package2.apk[desc]>]",
				"",
				"Compares APK packages content or APK package with content description",
				"",
				"Copyright 2020 Microsoft Corporation",
				"",
				"Options:",
				{ "c|comment=",
					"Comment to be saved inside .apkdesc file",
				  v => Comment = v },
				{ "h|help|?",
					"Show this message and exit",
				  v => help = v != null },
				{ "test-apk-size-regression=",
					"Check whether apk size increased more than {BYTES}",
				  v => ApkRegressionThreshold = long.Parse (v) },
				{ "test-assembly-size-regression=",
					"Check whether any assembly size increased more than {BYTES}",
				  v => AssemblyRegressionThreshold = long.Parse (v) },
				{ "s|save-descriptions",
					"Save .apkdesc files next to the apk package(s)",
				  v => SaveDescriptions = true },
				{ "v|verbose",
					"Output information about progress during the run of the tool",
				  v => Verbose = true },
			};

			var remaining = options.Parse (args);

			if (help || args.Length < 1) {
				options.WriteOptionDescriptions (Out);

				Environment.Exit (0);
			}

			if (remaining.Count != 2 && (ApkRegressionThreshold != 0 || AssemblyRegressionThreshold != 0)) {
				Error ("Please specify 2 APK packages for regression testing.");
				Environment.Exit (2);
			}

			if (remaining.Count != 2 && (remaining.Count != 1 || !SaveDescriptions)) {
				Error ("Please specify 2 APK packages to compare or 1 and use -s option.");
				Environment.Exit (1);
			}

			return (remaining [0], remaining.Count > 1 ? remaining [1] : null);
		}

		static void ColorMessage (string message, ConsoleColor color, TextWriter writer, bool writeLine = true)
		{
			ForegroundColor = color;

			if (writeLine)
				writer.WriteLine (message);
			else
				writer.Write (message);

			ResetColor ();
		}

		public static void ColorWriteLine (string message, ConsoleColor color) => ColorMessage (message, color, Out);

		public static void ColorWrite (string message, ConsoleColor color) => ColorMessage (message, color, Out, false);

		public static void Error (string message) => ColorMessage ($"Error: {Name}: {message}", ConsoleColor.Red, Console.Error);

		public static void Warning (string message) => ColorMessage ($"Warning: {Name}: {message}", ConsoleColor.Yellow, Console.Error);
	}
}
