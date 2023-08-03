using System.Diagnostics.Contracts;

namespace JP.FileScripts
{
    using FastString = ReadOnlySpan<char>;

	[Pure]
	public class FileNameTemplateConverter : IBatchNameChanger
	{
		public FileNameTemplateConverter(string oldTemplate, string newTemplate)
		{
			var fieldsInNewTemplate = GetFields(newTemplate);
		}

		public void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName)
		{
			foreach (var pathName in pathNames)
			{
				var (path, fileName) = pathName.BreakDownPathName();
				var fieldValues = GetFieldValues(fileName);
				var newName = MakeNewName(fieldValues);

				changeName(pathName, Path.Combine(path, newName));
			}
		}

		private T GetFieldValues(string name)
		{
			throw new NotImplementedException();
		}

		private string MakeNewName(T fieldValues)
		{
			throw new NotImplementedException();
		}

		private const char BeforeField = '\\';

		private static IEnumerable<Field> GetFields(string template)
		{
			int i;
			FastString rest = template;

			while (0 < (i = rest.IndexOf(BeforeField)))
			{
				rest = rest.Slice(i + 1);
				yield return GetNextField(rest);
			}
		}

		private static Field GetNextField(FastString rest)
		{
			var firstChar = rest[0];
			switch(rest[0])
			{
				case 'i': return Field.Index;
				case 'Y': return Field.Year;
				case 'M': return Field.Month;
				case 'D': return Field.Day;
				default:
			}
		}

		private enum Field
		{
			Index,
			Year,
			Month,
			Day,
		}
	}
}
