using System.Diagnostics.Contracts;
using System.Text;
using JP.Utils;

namespace JP.FileScripts
{
	using Value = Int32;
	using FieldValues = Dictionary<Field, Int32>;
	using FieldPositionsInTemplate = Dictionary<Field, (int StartIndex, int Length)>;
	using FastString = ReadOnlySpan<char>;

	delegate string ComposerFromTemplate(FieldValues fieldValues, int fileCountIndex);

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

	public class FileNameTemplateConverter : IBatchNameChanger
	{
		/// <param name="oldTemplate">Need not match the whole file name; just enough of its start to get the data for <paramref name="newTemplate"/>.</param>
		public FileNameTemplateConverter(string oldTemplate, string newTemplate)
		{
			FieldsInOldTemplate = GetFields(oldTemplate);
			ComposeFromNewTemplate = MakeComposer(newTemplate);
		}

		readonly FieldPositionsInTemplate FieldsInOldTemplate;
		readonly ComposerFromTemplate ComposeFromNewTemplate;

		readonly StringBuilder composeBuffer = new();
		readonly FieldValues fieldValuesBuffer = new();
		readonly FieldValues previousFieldValuesBuffer = new();

		const char FieldEscapeChar = '\\';
		const int FieldEscapeCharLen = 1;
		
		[Pure] // in practice :) there are buffers but hush no one could tell
		public void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName)
		{
			int fileCountIndex = 0;
			foreach (var pathName in pathNames)
			{
				var (path, fileName, extension) = BreakDownPathName(pathName);

				if (!TryGetFieldValues(fileName))
					continue;
				if (OtherFieldsChanged())
					fileCountIndex = 0;

				var newName = ComposeFromNewTemplate(fieldValuesBuffer, ++fileCountIndex);
				newName = Path.Combine(path, newName);
				newName = Path.ChangeExtension(newName, extension);

				changeName(pathName, newName);
			}
		}

		ComposerFromTemplate MakeComposer(string template) => (fieldValues, fileCountIndex) =>
		{
			composeBuffer.Clear();

			for(int i = 0; i < template.Length; i++)
			{
				var c = template[i];
				if(c == FieldEscapeChar)
				{
					var (field, textLength) = GetNextFieldAndLength(template.AsSpan(i + FieldEscapeCharLen));
					i += textLength;
					composeBuffer.Append(Compose(field, GetValue(fieldValues, field, fileCountIndex)));
				}
				else composeBuffer.Append(c);
			}
			return composeBuffer.ToString();
		};

		static Value GetValue(FieldValues fieldValues, Field field, int fileCountIndex)
		{
			if (field == Field.Index)
			{
				return fileCountIndex;
			}
			return fieldValues[field];
		}

		bool TryGetFieldValues(string name)
		{
			fieldValuesBuffer.Clear();
			foreach (var position in FieldsInOldTemplate.Where(position => position.Key != Field.Index))
			{
				var maybeValue = name.AsSpan(position.Value.StartIndex, position.Value.Length);

				if(Value.TryParse(maybeValue, out var value))
				{
					fieldValuesBuffer.Add(position.Key, value);
				}
				else return false;
			}
			return true;
		}

		bool OtherFieldsChanged()
		{
			var ans = !fieldValuesBuffer.IsEqualContentTo(previousFieldValuesBuffer);
			previousFieldValuesBuffer.Clear();
			previousFieldValuesBuffer.AddAndReplace(fieldValuesBuffer);
			return ans;
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
				rest.LastIndexOf(firstChar) + FieldEscapeCharLen );
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

		static string Compose(Field field, Value value)
		{
			switch(field)
			{
				case Field.Index:  return value.ToString();
				case Field.Year:   return value.ToString("D4");
				case Field.Month:
				case Field.Day:
				case Field.Hour:
				case Field.Minute:
				case Field.Second: return value.ToString("D2");

				default: throw new InvalidProgramException($"Unknown field: {field}");
			}
		}

		static (string Path, string FileName, string Extension) BreakDownPathName(string pathName)
		{
			var path = Path.GetDirectoryName(pathName) ?? string.Empty;
			var fileName = Path.GetFileNameWithoutExtension(pathName) ?? string.Empty;
			var extension = Path.GetExtension(pathName) ?? string.Empty;
			return (path, fileName, extension);
		}
	}
}
