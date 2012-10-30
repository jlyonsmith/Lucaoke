using System;
using ToolBelt;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Lucaoke
{
	class MainClass
	{
		public static int Main(string[] args)
		{
            ITool tool = new CreateRsqSongDbTool(new ConsoleOutputter());

			try
			{
				((IProcessCommandLine)tool).ProcessCommandLine(args);

				tool.Execute();

				return tool.Output.HasOutputErrors ? 1 : 0;
			}
			catch (Exception ex)
			{
				tool.Output.Error(ex.Message);
				return 1;
			}
		}
	}
}
