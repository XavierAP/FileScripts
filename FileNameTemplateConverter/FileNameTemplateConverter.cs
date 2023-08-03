using System.Diagnostics.Contracts;

namespace JP.FileScripts
{
	[Pure]
	public class FileNameTemplateConverter : IBatchNameChanger
	{
		public FileNameTemplateConverter(string oldTemplate, string newTemplate)
		{
			throw new NotImplementedException();
		}

		public void ChangeNames(IReadOnlyList<string> names, NameChanger changeName)
		{
			throw new NotImplementedException();
		}
	}
}
