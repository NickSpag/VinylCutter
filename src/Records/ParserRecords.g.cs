using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VinylCutter.Infrastructure;

namespace VinylCutter.Records
{
	public enum Visibility { Public, Private }

	public partial class ParseCompileError : Exception
	{
		public string ErrorText { get; }

		public ParseCompileError (string errorText)
		{
			ErrorText = errorText;
		}

		public override string ToString () => $"ParseCompileError -  {ErrorText}";
	}

	public partial class ItemInfo
	{
		public string Name { get; }
		public string TypeName { get; }
		public bool IsCollection { get; }
		public bool IncludeWith { get; }
		public string DefaultValue { get; }

		public ItemInfo (string name, string typeName, bool isCollection = false, bool includeWith = false, string defaultValue = null)
		{
			Name = name;
			TypeName = typeName;
			IsCollection = isCollection;
			IncludeWith = includeWith;
			DefaultValue = defaultValue;
		}
	}

	public partial class RecordInfo
	{
		public string Name { get; }
		public bool IsClass { get; }
		public Visibility Visibility { get; }
		public bool IncludeWith { get; }
		public ImmutableArray<ItemInfo> Items { get; }
		public string BaseTypes { get; }
		public string InjectCode { get; }

		public RecordInfo (string name, bool isClass, Visibility visibility, bool includeWith = false, IEnumerable<ItemInfo> items = null, string baseTypes = "", string injectCode = "")
		{
			Name = name;
			IsClass = isClass;
			Visibility = visibility;
			IncludeWith = includeWith;
			Items = ImmutableArray.CreateRange (items ?? Array.Empty<ItemInfo> ());
			BaseTypes = baseTypes;
			InjectCode = injectCode;
		}
	}

    public partial class RecordFileInfo : FileInfo
	{
		public ImmutableArray<RecordInfo> Records { get; }
	
		public RecordFileInfo (IEnumerable<RecordInfo> records, string injectCode = "", string globalNamespace = "")
		{
			Records = ImmutableArray.CreateRange (records ?? Array.Empty<RecordInfo> ());
			InjectCode = injectCode;
			GlobalNamespace = globalNamespace;
		}
	}
}
