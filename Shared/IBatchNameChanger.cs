namespace JP.FileScripts
{
	public interface IBatchNameChanger
	{
		void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName);
	}

	public delegate void NameChanger(string oldPathName, string newPathName);
	

	#if DEBUG
	public static partial class Test
	{
		public static void ChangeName(string oldPathName, string newPathName)
		{
			Console.WriteLine(oldPathName);
			Console.WriteLine(newPathName);
			Console.WriteLine("====================");
		}
	}
	#endif
}
