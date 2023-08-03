namespace JP.FileScripts
{
	static class Program
	{
		static void Main(string[] args)
		{
			new FileNameTemplateConverter(
				@"IMG_\YYYY\MM\DD_\HH\MM\SS",
				@"\YYYY-\MM-\DD (\i)"
				).ChangeNames(
					Directory.EnumerateFiles(args[0]).ToArray(),
					TestChangeName);
		}
		
		#if DEBUG
		private static void TestChangeName(string oldPathName, string newPathName)
		{
			Console.WriteLine(oldPathName);
			Console.WriteLine(newPathName);
			Console.WriteLine("====================");
		}
		#endif
	}
}
