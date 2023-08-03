﻿using System.Diagnostics.Contracts;
using System.Text;

namespace JP.FileScripts
{
	using Value = UInt32;
	using FieldValues = Dictionary<Field, UInt32>;
	using FieldPositionsInTemplate = Dictionary<Field, (int StartIndex, int Length)>;
    using FastString = ReadOnlySpan<char>;

	enum Field
	{
		Index,
		Year,
		Month,
		Day,
	}

	[Pure]
	public class FileNameTemplateConverter : IBatchNameChanger
	{
		public FileNameTemplateConverter(string oldTemplate, string newTemplate)
		{
			FieldsInOldTemplate = GetFields(oldTemplate);
			ComposeFromNewTemplate = MakeComposer(newTemplate);
		}

		private readonly FieldPositionsInTemplate FieldsInOldTemplate;
		private readonly Action<FieldValues> ComposeFromNewTemplate;

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

		private FieldValues GetFieldValues(string name)
		{
			return FieldsInOldTemplate.ToDictionary(
				position => position.Key,
				position => Value.Parse(name.AsSpan(position.Value.StartIndex, position.Value.Length)));
		}

		private string MakeNewName(FieldValues fieldValues)
		{
			throw new NotImplementedException();
		}

		private const char BeforeField = '\\';

		private FieldPositionsInTemplate GetFields(string template)
		{
			var fields = new FieldPositionsInTemplate();

			FastString rest = template;
			int i;

			while (0 < (i = rest.IndexOf(BeforeField)))
			{
				rest = rest.Slice(i + 1);
				fields.Add(GetNextField(rest));
			}
		}

		private static Field GetNextField(FastString rest)
		{
			var firstChar = rest[0];
			switch(firstChar)
			{
				case 'i': return Field.Index;
				case 'Y': return Field.Year;
				case 'M': return Field.Month;
				case 'D': return Field.Day;
				default:
			}
		}

		private Action<FieldValues> MakeComposer(string newTemplate)
		{
			throw new NotImplementedException();
		}
	}
}
