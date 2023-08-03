namespace JP.FileScripts
{
	public interface IBatchNameChanger
	{
		void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName);
	}

	public delegate void NameChanger(string oldPathName, string newPathName);
}
