using System.Diagnostics.Contracts;
using System.Text;

namespace JP.FileScripts
{
	using Value = UInt32;
	using FieldValues = Dictionary<Field, UInt32>;
	using FieldPositionsInTemplate = Dictionary<Field, (int StartIndex, int Length)>;
    using FastString = ReadOnlySpan<char>;

	delegate string ComposerFromTemplate(FieldValues fieldValues, Func<Value, string> composeIndex);

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
		private readonly ComposerFromTemplate ComposeFromNewTemplate;

		private const char FieldEscapeChar = '\\';

		public void ChangeNames(IReadOnlyList<string> pathNames, NameChanger changeName)
		{
			var composeIndex = MakeIndexComposer(pathNames.Count);

			foreach (var pathName in pathNames)
			{
				var (path, fileName) = pathName.BreakDownPathName();
				var fieldValues = GetFieldValues(fileName); //TODO optimize allocation
				var newName = ComposeFromNewTemplate(fieldValues, composeIndex);

				changeName(pathName, Path.Combine(path, newName));
			}
		}

		private static ComposerFromTemplate MakeComposer(string template) => (fieldValues, composeIndex) =>
		{
			var writer = new StringBuilder(); //TODO optimize allocation
			for(int i = 0; i < template.Length; i++)
			{
				var c = template[i];
				if(c == FieldEscapeChar)
				{
					var (field, textLength) = GetNextFieldAndLength(template.AsSpan(i));
					i += textLength;
					writer.Append(Compose(field, fieldValues[field], composeIndex));
				}
				else
				{
					writer.Append(c);
				}
			}
			return writer.ToString();
		};

		private FieldValues GetFieldValues(string name)
		{
			return FieldsInOldTemplate.ToDictionary(
				position => position.Key,
				position => Value.Parse(name.AsSpan(position.Value.StartIndex, position.Value.Length)));
		}

		private static FieldPositionsInTemplate GetFields(string template)
		{
			var fields = new FieldPositionsInTemplate();

			FastString rest = template;
			int i;

			while (0 < (i = rest.IndexOf(FieldEscapeChar)))
			{
				rest = rest.Slice(++i);
				var (field, len) = GetNextFieldAndLength(rest);
				fields[field] = (i, len);
			}
			return fields;
		}

		private static (Field Field, int TextLength) GetNextFieldAndLength(FastString rest)
		{
			var firstChar = rest[0];
			return (
				GetNextField(firstChar),
				rest.LastIndexOf(firstChar) );
		}

		private static Field GetNextField(char firstChar)
		{
			switch(firstChar)
			{
				case 'i': return Field.Index;
				case 'Y': return Field.Year;
				case 'M': return Field.Month;
				case 'D': return Field.Day;

				default: throw new InvalidProgramException($"Unknown field code: {firstChar}");
			}
		}

		private static string Compose(Field field, Value value,
			Func<Value, string> composeIndex)
		{
			switch(field)
			{
				case Field.Index: return composeIndex(value);
				case Field.Year:  return value.ToString("D4");
				case Field.Month:
				case Field.Day:   return value.ToString("D2");

				default: throw new InvalidProgramException($"Unknown field: {field}");
			}
		}

		private static Func<Value, string> MakeIndexComposer(int nameCount) => value => value.ToString($"D{nameCount}");
	}
}
