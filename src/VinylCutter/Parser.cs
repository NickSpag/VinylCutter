﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Collections.Immutable;

namespace VinylCutter
{
	public class ClassItem
	{
		public string Name { get; private set; }
		public string TypeName { get; private set; }
		public TypeReference Type { get; private set; }
		public bool IsCollection { get; private set; }
		public bool ForcedIncludeWith { get; private set; }

		public ClassItem (string name, string typeName) : this (name, typeName, false, false)
		{
		}

		public ClassItem (string name, string typeName, bool isCollection, bool forcedIncludeWith)
		{
			Name = name;
			TypeName = typeName;
			IsCollection = isCollection;
			ForcedIncludeWith = forcedIncludeWith;
		}

		public static ClassItem Create (PropertyDefinition propertyDefinition)
		{
			return Create (propertyDefinition.Name, propertyDefinition.PropertyType, propertyDefinition.CustomAttributes.ElementAtOrDefault (0));
		}

		public static ClassItem Create (FieldDefinition fieldDefinition)
		{
			return Create (fieldDefinition.Name, fieldDefinition.FieldType, fieldDefinition.CustomAttributes.ElementAtOrDefault (0));
		}

		static ClassItem Create (string name, TypeReference type, CustomAttribute attribute)
		{
			bool forcedIncludeWith = attribute != null && attribute.AttributeType.Name == "With";
			if (type.Name.Contains ("IEnumerable")) 
			{
				GenericInstanceType genericInstance = (GenericInstanceType)type;
				return new ClassItem (name, genericInstance.GenericArguments [0].Name, true, forcedIncludeWith);
			} 
			return new ClassItem (name, type.Name, false, forcedIncludeWith);
		}
	}

	public enum Visibility { Public, Private }

	public class ParseInfo
	{
		public string Name { get; private set; }
		public Visibility Visibility { get; private set; }
		public bool IsClass { get; private set; }
		public ImmutableArray<ClassItem> Items;
		public bool IncludeWith { get; private set; }

		public ParseInfo (string name, bool isClass, Visibility visibility) : this (name, isClass, visibility, true, Enumerable.Empty<ClassItem> ())
		{
		}

		public ParseInfo (string name, bool isClass, Visibility visibility, bool includeWith, IEnumerable<ClassItem> items)
		{
			Name = name;
			Visibility = visibility;
			IsClass = isClass;
			Items = ImmutableArray.CreateRange (items);
			IncludeWith = includeWith;
		}

		public static Visibility GetVisibility (TypeDefinition type)
		{
			if (type.IsPublic)
				return Visibility.Public;
			// https://github.com/chamons/VinylCutter/issues/3 
			// if (type.IsNestedFamily)
			//	return Visibility.Private;
			return Visibility.Private;
		}

		public static ParseInfo Create (TypeDefinition type)
		{
			var properties = type.Properties
			                     .Where (x => !Parser.IsInternalConstruct (x.Name))
			                     .Select (x => ClassItem.Create (x));
			var variables = type.Fields
			                    .Where (x => !Parser.IsInternalConstruct (x.Name))
			                    .Select (x => ClassItem.Create (x));
			
			bool isClass = type.BaseType.FullName != "System.ValueType";
			bool includeWith = !type.CustomAttributes.Any (x => x.AttributeType.Name == "Without");
			return new ParseInfo (type.Name, isClass, GetVisibility (type), includeWith, properties.Union (variables));
		}
	}

	public class Parser
	{
		public static bool IsInternalConstruct (string name) => name.Contains ("<") || name.Contains (">");
		static bool IsAttribute (TypeDefinition t) => t.BaseType.FullName == "System.Attribute";

		string Text;
		string Prelude = @"
using System;
using System.Collections.Generic;

[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
public class Without : System.Attribute { } 

[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
public class With : System.Attribute { } 
";

		public Parser (string text)
		{
			Text = text;
		}

		public List<ParseInfo> Parse ()
		{
			var infos = new List<ParseInfo> ();
			using (Compiler compiler = new Compiler (Prelude + Text))
			{
				string assemblyPath = compiler.Compile ();
				var module = ModuleDefinition.ReadModule (assemblyPath);
				foreach (TypeDefinition type in module.Types.Where (x => x.IsClass && !IsInternalConstruct (x.Name) && !IsAttribute (x)))
				{
					infos.Add (ParseInfo.Create (type));
				}
			}
			return infos;
		}
	}
}