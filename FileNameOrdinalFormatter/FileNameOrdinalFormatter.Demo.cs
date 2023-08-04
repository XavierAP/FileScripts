using System.IO;
using System.Linq;

namespace JP.FileScripts
{
	static class Demo
	{
		static void Main(string[] args)
		{
			new FileNameOrdinalFormatter().ChangeNames(
				Directory.EnumerateFiles(args[0]).ToArray(),
				File.Move);
		}
	}
}
