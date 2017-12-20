using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

namespace Xml.Schema.Linq.CodeGen
{
	internal static class NameGenerator
	{
		private static int uniqueIdCounter;

		private static Hashtable keywords;

		static NameGenerator()
		{
			NameGenerator.uniqueIdCounter = 0;
			NameGenerator.keywords = new Hashtable();
			string[] strArrays = new string[] { "abstract", "event", "new", "struct", "as", "explicit", "null", "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw", "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float", "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected", "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe", "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate", "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock", "stackalloc", "else", "long", "static", "enum", "namespace", "string", "var" };
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string k = strArrays1[i];
				NameGenerator.keywords.Add(k.ToUpper(CultureInfo.InvariantCulture), k);
			}
		}

		public static string ChangeClrName(string clrName, NameOptions options)
		{
			string str;
			switch (options)
			{
				case NameOptions.MakeCollection:
				{
					if (clrName[0] == '@')
					{
						clrName = clrName.Remove(0, 1);
					}
					str = string.Concat(clrName, "Collection");
					break;
				}
				case NameOptions.MakeList:
				{
					str = string.Concat(clrName, "List");
					break;
				}
				case NameOptions.MakePlural:
				{
					str = string.Concat(clrName, "s");
					break;
				}
				case NameOptions.MakeField:
				{
					str = string.Concat(clrName, "Field");
					break;
				}
				case NameOptions.MakeParam:
				{
					str = string.Concat(clrName, "Param");
					break;
				}
				case NameOptions.MakeLocal:
				{
					str = string.Concat(clrName, "LocalType");
					break;
				}
				case NameOptions.MakeUnion:
				{
					str = string.Concat(clrName, "UnionValue");
					break;
				}
				case NameOptions.MakeDefaultValueField:
				{
					str = string.Concat(clrName, "DefaultValue");
					break;
				}
				case NameOptions.MakeFixedValueField:
				{
					str = string.Concat(clrName, "FixedValue");
					break;
				}
				default:
				{
					str = clrName;
					break;
				}
			}
			return str;
		}

		public static string GetServicesClassName()
		{
			return "LinqToXsdTypeManager";
		}

		public static int GetUniqueID()
		{
			Interlocked.Increment(ref NameGenerator.uniqueIdCounter);
			return NameGenerator.uniqueIdCounter;
		}

		public static bool isKeyword(string identifier)
		{
			bool flag = NameGenerator.keywords.ContainsKey(identifier.ToUpper(CultureInfo.InvariantCulture));
			return flag;
		}

		public static string MakeValidCLRNamespace(string xsdNamespace, bool nameMangler2)
		{
			string empty;
			if ((xsdNamespace == null ? false : !(xsdNamespace == string.Empty)))
			{
				xsdNamespace = xsdNamespace.Replace("http://", string.Empty);
				if (!(xsdNamespace == string.Empty))
				{
					if (nameMangler2)
					{
						xsdNamespace = xsdNamespace.Replace('.', '\u005F').Replace('-', '\u005F');
					}
					string[] pieces = xsdNamespace.Split(new char[] { '/', '.', ':', '-' });
					string clrNS = NameGenerator.MakeValidIdentifier(pieces[0], nameMangler2);
					for (int i = 1; i < (int)pieces.Length; i++)
					{
						if (pieces[i] != string.Empty)
						{
							clrNS = string.Concat(clrNS, ".", NameGenerator.MakeValidIdentifier(pieces[i], nameMangler2));
						}
					}
					empty = clrNS;
				}
				else
				{
					empty = string.Empty;
				}
			}
			else
			{
				empty = string.Empty;
			}
			return empty;
		}

		public static string MakeValidIdentifier(string identifierName, bool nameMangler2)
		{
			string str;
			if (!nameMangler2)
			{
				identifierName = CodeIdentifier.MakeValid(identifierName);
			}
			else
			{
				if (char.IsDigit(identifierName[0]))
				{
					identifierName = string.Concat("_", identifierName);
				}
				identifierName = identifierName.Replace('.', '\u005F').Replace('-', '\u005F');
			}
			str = (!NameGenerator.isKeyword(identifierName) ? identifierName : string.Concat("@", identifierName));
			return str;
		}
	}
}