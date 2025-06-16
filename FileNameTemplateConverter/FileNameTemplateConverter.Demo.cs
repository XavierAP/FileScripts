namespace JP.FileScripts
{
	static class Demo
	{
		static void Main(string[] args)
		{
			new FileNameTemplateConverter(
				@"IMG_\YYYY\MM\DD",
				@"\YYYY-\MM-\DD (\i)"
				).ChangeNames(
					Directory.EnumerateFiles(args[0]).ToArray(),
					File.Move);
		}
	}
}
