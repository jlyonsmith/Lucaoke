using System;
using ToolBelt;

namespace CreateSongDB
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			IList<ParsedPath> dirs = DirectoryUtility.GetDirectories(
				new ParsedPath("*", PathType.Directory), SearchScope.RecurseSubDirectoriesBreadthFirst);
		}
	}
}
