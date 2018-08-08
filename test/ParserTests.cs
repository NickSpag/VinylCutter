﻿using System;
using System.Linq;
using VinylCutter.Records;
using Xunit;
using VinylCutter.Infrastructure;

namespace VinylCutter.Tests
{
	public class ParserTests
	{
		RecordFileInfo Parse (string text)
		{
            Parser parser = new Parser(text);
            RecordsParseWorkload recordParseWorkload = new RecordsParseWorkload ();
			
            return parser.Parse (recordParseWorkload) as RecordFileInfo;
		}
		
		[Theory]
		[InlineData ("public class SimpleClass { }", "SimpleClass", true)]
		[InlineData ("public struct SimpleStruct { }", "SimpleStruct", false)]
		public void SimpleReflectedInfo (string text, string name, bool isClass)
		{
			RecordFileInfo file = Parse (text);
			Assert.Single (file.Records);
			Assert.Equal (name, file.Records[0].Name);
			Assert.Equal (isClass, file.Records[0].IsClass);
			Assert.False (file.Records[0].IncludeWith);
			Assert.Equal ("", file.Records[0].BaseTypes);
			Assert.Equal ("", file.InjectCode);
			Assert.Equal ("", file.GlobalNamespace);
		}

		[Fact]
		public void PropertiesAreTracked ()
		{
			RecordFileInfo file = Parse ("public class SimpleClass { int X { get; } }");
			Assert.Single (file.Records[0].Items);
			Assert.Equal ("X", file.Records[0].Items[0].Name);
			Assert.Equal ("Int32", file.Records[0].Items[0].TypeName);
			Assert.False (file.Records[0].Items[0].IsCollection);
			Assert.False (file.Records[0].Items[0].IncludeWith);
		}

		[Fact]
		public void VariablesAreTracked ()
		{
			RecordFileInfo file = Parse ("public class SimpleClass { double Y; }");

			Assert.Single (file.Records[0].Items);
			Assert.Equal ("Y", file.Records[0].Items [0].Name);
			Assert.Equal ("Double", file.Records[0].Items [0].TypeName);
			Assert.False (file.Records[0].Items [0].IsCollection);
			Assert.False (file.Records[0].Items [0].IncludeWith);
		}

		[Fact]
		public void IEnumerables ()
		{
			RecordFileInfo file = Parse ("public class SimpleClass { List<int> Z; }");

			Assert.Single (file.Records[0].Items);
			Assert.Equal ("Z", file.Records[0].Items [0].Name);
			Assert.True(file.Records[0].Items [0].IsCollection);
			Assert.Equal ("Int32", file.Records[0].Items [0].TypeName);
		}

		[Fact]
		public void OtherRecordTypes ()
		{
			RecordFileInfo file = Parse (@"
public class Element { int X; }
public class Container { List <Element> E; }
");

			Assert.Single (file.Records[0].Items);
			var container = file.Records.First (x => x.Name == "Container");
			Assert.Equal ("E", container.Items [0].Name);
			Assert.Equal ("Element", container.Items [0].TypeName);
			Assert.True (container.Items [0].IsCollection);
		}

		[Fact]
		public void ClassWithAttributes ()
		{
			RecordFileInfo file = Parse (@"
[With]
public class SimpleClass { int X; }
");

			Assert.True (file.Records[0].IncludeWith);
			Assert.False (file.Records[0].Items[0].IncludeWith);

		}

		[Fact]
		public void ItemSpecificWithAttributes ()
		{
			RecordFileInfo file = Parse (@"
public class SimpleClass { [With] int X; }
");

			Assert.False (file.Records[0].IncludeWith);
			Assert.True (file.Records[0].Items[0].IncludeWith);
		}
		
		[Fact]
		public void Visibilities ()
		{
			Func <string, Visibility> parseVisibility = s => (Parse(s).Records[0].Visibility);

			Assert.Equal (Visibility.Public, parseVisibility ("public class SimpleClass {}"));
			Assert.Equal (Visibility.Private, parseVisibility ("class SimpleClass {}"));
		}

		[Fact]
		public void Skip ()
		{
			RecordFileInfo file = Parse (@"
public class SimpleClass { int X; }
[Skip]
public class SkippedSimpleClass { int X; }
[Skip]
public interface SkippedInterface { int X { get; } }
");

			Assert.Single (file.Records);
			Assert.Equal ("SimpleClass", file.Records[0].Name);
		}

		[Fact]
		public void SkipEnums ()
		{
			RecordFileInfo file = Parse (@"
public enum ParsingConfidence
{
	High,
	Likely,
	Low,
	Invalid,
}
");
			Assert.Empty (file.Records);
		}

		[Fact]
		public void Inject ()
		{
			RecordFileInfo file = Parse (@"public class SimpleClass 
{
	int X; 
	int Y; 

	[Inject]
	int Size => X * Y;
}
");
			Assert.Equal ("\tint Size => X * Y;", file.Records[0].InjectCode);
			Assert.Equal (2, file.Records[0].Items.Length);
		}

		[Fact]
		public void InjectTopLevelItems ()
		{
			RecordFileInfo file = Parse (@"[Inject]
public enum Visibility { Public, Private }

public class SimpleClass
{
	Visibility Status;
	int Size;

	[Inject]
	bool Show => Status == Visibility.Public;
}
");
			Assert.Equal ("public enum Visibility { Public, Private }", file.InjectCode);
			Assert.Single (file.Records);
			Assert.Equal ("\tbool Show => Status == Visibility.Public;", file.Records[0].InjectCode);
			Assert.Equal (2, file.Records[0].Items.Length);
		}

		[Fact]
		public void InjectTopLevelItemsWithNamespace ()
		{
			RecordFileInfo file = Parse (@"namespace Test
{
	[Inject]
	public enum Visibility { Public, Private }

	public class SimpleClass
	{
		Visibility Status;
		int Size;

		[Inject]
		bool Show => Status == Visibility.Public;
	}
}
");
			Assert.Equal ("Test", file.GlobalNamespace);
			Assert.Equal ("\tpublic enum Visibility { Public, Private }", file.InjectCode);
			Assert.Single (file.Records);
			Assert.Equal ("\t\tbool Show => Status == Visibility.Public;", file.Records[0].InjectCode);
			Assert.Equal (2, file.Records[0].Items.Length);
		}

		[Fact]
		public void Inherit ()
		{
			RecordFileInfo file = Parse (@"public interface IFoo {} public class Foo {}
public class SimpleClass : Foo, IFoo
{
	int X; 
	int Y; 
}
");
			Assert.Equal ("Foo, IFoo", file.Records.First (x => x.Name == "SimpleClass").BaseTypes);
		}

		[Fact]
		public void Default ()
		{
			RecordFileInfo file = Parse (@"public class SimpleClass
{
	[Default (""0"")]
	int X;
	int Y; 
}
");
			Assert.Equal ("0", file.Records[0].Items[0].DefaultValue);
			Assert.Null (file.Records[0].Items[1].DefaultValue);
		}

		[Fact]
		public void NullDefault ()
		{
			RecordFileInfo file = Parse (@"public class SimpleClass
{
	[Default (""null"")]
	string X;
}
");
			Assert.Equal ("null", file.Records[0].Items[0].DefaultValue);
		}

		[Fact]
		public void BoolDefault ()
		{
			RecordFileInfo file = Parse (@"public class SimpleClass
{
	[Default (""false"")]
	bool X;
}
");
			Assert.Equal ("false", file.Records[0].Items[0].DefaultValue);
		}

		[Fact]
		public void EmptyStringDefault ()
		{
			RecordFileInfo file = Parse (@"public class SimpleClass
{
	[Default ("""")]
	bool X;
}
");
			Assert.Equal ("\"\"", file.Records[0].Items[0].DefaultValue);
		}

		[Fact]
		public void Namespace ()
		{
			RecordFileInfo file = Parse (@"namespace Test { public class SimpleClass { } }");
			Assert.Equal ("Test", file.GlobalNamespace);
		}

		[Fact]
		public void CompileError ()
		{
            var uncompileableCode = @"public class SimpleClass { ";

            Parser parser = new Parser(uncompileableCode);
            RecordsParseWorkload recordParseWorkload = new RecordsParseWorkload();

            Assert.Throws<ParseCompileError> (() => parser.Parse (recordParseWorkload) as RecordFileInfo);
		}
	}
}
