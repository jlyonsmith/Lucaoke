using System;
using ToolBelt;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Lucaoke
{
	[CommandLineTitle("Create RSQ Karaoke Song Database Creator")]
	[CommandLineCopyright("Copyright (c) 2012, John Lyon-Smith & Dave Lucas")]
	[CommandLineExample(@"
CreateRsqSongDb -md:C:\Music\ -ad:H:\Music\ -n:1000 
")]
	public class CreateRsqSongDb : ITool, IProcessCommandLine
	{
		#region Fields
		CommandLineParser parser;

		#endregion

		#region Properties
		[CommandLineArgument("help", ShortName="?", Description="Shows this help")]
		public bool ShowHelp { get; set; }

		[CommandLineArgument("musicdir", ShortName="md", Description="Source directory containing .mp3 & .cdg files. Required.", 
							 Initializer=typeof(CreateRsqSongDb), MethodName="ParseDirectory")]
		public ParsedPath MusicDirName { get; set; }

		[CommandLineArgument("altdir", ShortName="ad", Description="Directory which will replace the source directory in the song database file.  Useful when generating the song database on a different machine from the that will build the RSQ song drive.", 
		                     Initializer=typeof(CreateRsqSongDb), MethodName="ParseDirectory")]
		public ParsedPath AltDirName { get; set; }

		[CommandLineArgument("songnum", ShortName="n", Description="Number to use for the first song. Defaults to 1000.")]
		public int? FirstSongNumber { get; set; }

		[CommandLineArgument("songdb", ShortName="db", Description="Song database file name.  Defaults to 'songdb.txt' in the root of the music directory.")]
		public ParsedPath SongDbFileName { get; set; }

		private CommandLineParser Parser 
		{
			get
			{
				if (parser == null)
					parser = new CommandLineParser(this.GetType(), CommandLineParserFlags.Default);

				return parser;
			}

		}

		#endregion

		#region Construction
		public CreateRsqSongDb(IOutputter outputter)
		{
			this.Output = new OutputHelper(outputter);
		}

		#endregion

		#region ITool implementation
		public void Execute()
		{
			Console.WriteLine(Parser.LogoBanner);

			if (ShowHelp)
			{
				Console.WriteLine(Parser.Usage);
				return;
			}
			
			if (SongDbFileName == null)
			{
				SongDbFileName = MusicDirName.WithFileAndExtension("songdb.txt");
			}

			if (MusicDirName == null)
			{
				Output.Error("Music directory must be specified");
				return;
			}

			if (AltDirName == null)
			{
				AltDirName = MusicDirName;
			}

			if (File.Exists(SongDbFileName))
			{
				// TODO: Need to load the file to get song numbers and merge existing songs in with the new stuff
				// TODO: If we delete stuff, keep a list of used song numbers?
				Output.Warning("'{0}' - Overwriting existing song database", SongDbFileName);
			}

			if (!FirstSongNumber.HasValue)
				FirstSongNumber = 1000;

			int nextUnusedSongNum = FirstSongNumber.Value;
			string lineFormat = "{0, -7} {1, -6} {2, -8} {3, -8} {4, -32} {5, -32} {6} {7}";
			Regex regex = new Regex(
				@"^(?<singer>.+?)( ?- ?\d*? ?- ?)(?<title>.+)", 
				RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			
			using (StreamWriter writer = new StreamWriter(SongDbFileName))
			{
				writer.NewLine = "\r\n";
				writer.WriteLine(
					lineFormat,
				    "SongNum", "Nation", "Songtype", "Language", "Title", "Singer", "", "");
				
				IList<ParsedPath> dirs = DirectoryUtility.GetDirectories(
					MusicDirName.Append("*", PathType.Wildcard), 
					SearchScope.RecurseSubDirectoriesBreadthFirst);
				
				foreach (var dir in dirs)
				{
					IList<ParsedPath> mp3s = DirectoryUtility.GetFiles(dir.WithFileAndExtension("*.mp3"), SearchScope.DirectoryOnly);
					
					foreach (var mp3 in mp3s)
					{
						ParsedPath cdg = mp3.WithExtension(".cdg");
						
						if (!File.Exists(cdg))
						{
							Output.Warning("'{0}' - MP3 has no CDG file.", mp3);
							continue;
						}
						
						Match m = regex.Match(mp3.File);
						
						if (!m.Success)
						{
							Output.Warning("'{0}' - MP3 file name is not in correct format", mp3);
							continue;
						}
						
						string title = m.Groups["title"].Value;
						string singer = m.Groups["singer"].Value;
						string tweakedMp3 = (AltDirName.ToString() + mp3.ToString().Substring(MusicDirName.ToString().Length)).Replace('/', '\\');
						string tweakedCdg = (AltDirName.ToString() + cdg.ToString().Substring(MusicDirName.ToString().Length)).Replace('/', '\\');
						
						writer.WriteLine(
							lineFormat,
							"#" + (nextUnusedSongNum++).ToString(),
							"#" + 2.ToString(),
							"#" + 5.ToString(),
							"#" + 2.ToString(),
							"#" + title.ToUpper(),
							"#" + singer.ToUpper(),
							"#" + tweakedMp3,
							"#" + tweakedCdg);
					}
				}
			}
		}

		public OutputHelper Output { get; private set; }

		#endregion

		#region IProcessCommandLine implementation

		public void ProcessCommandLine(string[] args)
		{
#if MONO
			Parser.CommandName = "mono CreateRsqSongDb.exe";
#endif

			this.Parser.ParseAndSetTarget(args, this);
		}

		#endregion

		#region Methods
		private static ParsedPath ParseDirectory(string dir)
		{
			return new ParsedPath(dir, PathType.Directory);
		}
		#endregion
	}
}

