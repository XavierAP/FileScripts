﻿using System.Diagnostics.Contracts;
using System.Text;

namespace JP.FileScripts
{
	using FieldValues = Dictionary<Field, int>;
	using FieldPositionsInTemplate = Dictionary<Field, (int StartIndex, int Length)>;
    using FastString = ReadOnlySpan<char>;

	delegate string ComposerFromTemplate(FieldValues fieldValues, int fileCountIndex, Func<int, string> composeIndex);

	enum Field
	{
		Index,
		Year,
		Month,
		Day,
		Hour,
		Minute,
		Second,
	}

	[Pure]
	public class FileNameTemplateConverter : IBatchNameChanger
	{
		public FileNameTemplateConverter(string oldTemplate, string newTemplate)
		{
			FieldsInOldTemplate = GetFields(oldTemplate);
			ComposeFromNewTemplate = MakeComposer(newTemplate);
		}

		readonly FieldPositionsInTemplate FieldsInOldTemplate;
		readonly ComposerFromTemplate ComposeFromNewTemplate;

		const char FieldEscapeChar = '\\';
		const int FieldEscapeCharLen = 1;

		public void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName)
		{
			var composeIndex = MakeIndexComposer(pathNames.Count);

			for (int fileCountIndex = 0; fileCountIndex < pathNames.Count; fileCountIndex++)
			{
				var pathName = pathNames[fileCountIndex];
				var (path, fileName) = BreakDownPathName(pathName);
				if (!TryGetFieldValues(fileName, out var fieldValues)) //TODO optimize allocation
					continue;

				var newName = ComposeFromNewTemplate(fieldValues, MakeOneBased(fileCountIndex), composeIndex);

				changeName(pathName, Path.Combine(path, newName));
			}
		}

		static ComposerFromTemplate MakeComposer(string template) => (fieldValues, fileCountIndex, composeIndex) =>
		{
			var writer = new StringBuilder(); //TODO optimize allocation
			for(int i = 0; i < template.Length; i++)
			{
				var c = template[i];
				if(c == FieldEscapeChar)
				{
					var (field, textLength) = GetNextFieldAndLength(template.AsSpan(++i));
					i += textLength;
					writer.Append(Compose(field, GetValue(fieldValues, field, fileCountIndex), composeIndex));
				}
				else
				{
					writer.Append(c);
				}
			}
			return writer.ToString();
		};

		static int GetValue(FieldValues fieldValues, Field field, int fileCountIndex)
		{
			if (field == Field.Index)
			{
				return fileCountIndex;
			}
			return fieldValues[field];
		}

		bool TryGetFieldValues(string name, out FieldValues fieldValues)
		{
			fieldValues = new FieldValues(FieldsInOldTemplate.Count);
			foreach (var position in FieldsInOldTemplate)
			{
				var maybeValue = name.AsSpan(position.Value.StartIndex, position.Value.Length);

				if(int.TryParse(maybeValue, out var value))
				{
					fieldValues.Add(position.Key, value);
				}
				else return false;
			}
			return true;
		}

		static FieldPositionsInTemplate GetFields(string template)
		{
			var fields = new FieldPositionsInTemplate();

			FastString rest = template;
			int relativePos, absolutePos = 0;

			while (0 <= (relativePos = rest.IndexOf(FieldEscapeChar)))
			{
				rest = rest.Slice(relativePos + FieldEscapeCharLen);
				var (field, len) = GetNextFieldAndLength(rest);
				fields[field] = (relativePos + absolutePos, len);
				rest = rest.Slice(len);
				absolutePos += relativePos + len;
			}
			return fields;
		}

		static (Field Field, int TextLength) GetNextFieldAndLength(FastString rest)
		{
			var firstChar = rest[0];
			return (
				GetNextField(firstChar),
				rest.LastIndexOf(firstChar) + 1 );
		}

		static Field GetNextField(char firstChar)
		{
			switch(firstChar)
			{
				case 'i': return Field.Index;
				case 'Y': return Field.Year;
				case 'M': return Field.Month;
				case 'D': return Field.Day;
				case 'h': return Field.Hour;
				case 'm': return Field.Minute;
				case 's': return Field.Second;

				default: throw new InvalidProgramException($"Unknown field code: {firstChar}");
			}
		}

		static string Compose(Field field, int value,
			Func<int, string> composeIndex)
		{
			switch(field)
			{
				case Field.Index:  return composeIndex(value);
				case Field.Year:   return value.ToString("D4");
				case Field.Month:
				case Field.Day:
				case Field.Hour:
				case Field.Minute:
				case Field.Second: return value.ToString("D2");

				default: throw new InvalidProgramException($"Unknown field: {field}");
			}
		}

		static Func<int, string> MakeIndexComposer(int nameCount) => value => value.ToString($"D{MakeOneBased(nameCount).ToString().Length}");

		static int MakeOneBased(int index) => index + 1;
		
		static (string Path, string FileName) BreakDownPathName(string pathName)
		{
			var path = Path.GetDirectoryName(pathName) ?? string.Empty;
			var fileName = Path.GetFileNameWithoutExtension(pathName) ?? string.Empty;
			return (path, fileName);
		}
	}
}
