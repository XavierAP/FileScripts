namespace JP.FileScripts
{
	public static class PathExtensions
	{
		public static (string path, string fileName) BreakDownPathName(this string pathName)
		{
			var path = Path.GetDirectoryName(pathName) ?? string.Empty;
			var fileName = Path.GetDirectoryName(pathName) ?? string.Empty;
			return (path, fileName);
		}
	}
}
