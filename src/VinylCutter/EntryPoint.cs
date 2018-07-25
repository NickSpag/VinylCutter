﻿using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace VinylCutter
{
	class EntryPoint
	{
		public static void Main (string[] args)
		{
			VinylCutterTool tool = new VinylCutterTool ();
			bool showHelp = false;

			var p = new OptionSet () {
				{ "stdin", "Read record definitions from stdin, not a file.", v => tool.ReadFromStandardIn = true },
				{ "stdout", "Output record generated code to stdout, not a file.", v => tool.WriteToStandardOut = true },
				{ "o|output=", "Directory to output file to. (Defaults to current directory)", (string v) => tool.OutputDirectory = v },
				{ "extension=", "Suffix to append to each file name written to output directory. (Defaults to .g.cs)", (string v) => tool.FileExtension = v },
				{ "h|help",  "show this message and exit", v => showHelp = true },
			};

			List<string> files = null;
			try {
				files = p.Parse (args);
			}
			catch (OptionException) {
				showHelp = true;
			}

			if (showHelp || !tool.ValidateOptions (files))
			{
				ShowHelp (p);
				return;
			}

			tool.Run ();
		}

		static void ShowHelp (OptionSet p)
		{
			Console.WriteLine ("Usage: VinylCutter.exe [OPTIONS]+ [FILES]+");
			Console.WriteLine ("Generate C# code from record definitions.");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			p.WriteOptionDescriptions (Console.Out);
		}
	}
}
