using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class SimpleTypeCodeDomHelper
	{
		public SimpleTypeCodeDomHelper()
		{
		}

		private static CodeExpression CreateByteArrayExpression(object value)
		{
			byte[] bytes = (byte[])value;
			CodeArrayCreateExpression array = new CodeArrayCreateExpression()
			{
				CreateType = new CodeTypeReference(typeof(byte))
			};
			byte[] numArray = bytes;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte b = numArray[i];
				array.Initializers.Add(new CodePrimitiveExpression((object)b));
			}
			return array;
		}

		public static CodeExpression CreateFacets(ClrSimpleTypeInfo type)
		{
			//object o = null;
			CodeExpression codePrimitiveExpression;
			CompiledFacets facets = type.RestrictionFacets;
			CodeObjectCreateExpression createFacets = new CodeObjectCreateExpression()
			{
				CreateType = new CodeTypeReference("Xml.Schema.Linq.RestrictionFacets")
			};
			Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
			if ((int)flags != 0)
			{
				CodeCastExpression cast = new CodeCastExpression(new CodeTypeReference("Xml.Schema.Linq.RestrictionFlags"), new CodePrimitiveExpression((object)Convert.ToInt32(flags, CultureInfo.InvariantCulture.NumberFormat)));
				createFacets.Parameters.Add(cast);
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Enumeration) == 0)
				{
					createFacets.Parameters.Add(new CodePrimitiveExpression(null));
				}
				else
				{
					CodeArrayCreateExpression enums = new CodeArrayCreateExpression()
					{
						CreateType = new CodeTypeReference("System.Object")
					};
					foreach (object o in facets.Enumeration)
					{
						SimpleTypeCodeDomHelper.GetCreateValueExpression(o, type, enums.Initializers);
					}
					createFacets.Parameters.Add(enums);
				}
				int fractionDigits = 0;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.FractionDigits) != 0)
				{
					fractionDigits = facets.FractionDigits;
				}
				createFacets.Parameters.Add(new CodePrimitiveExpression((object)fractionDigits));
				int length = 0;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Length) != 0)
				{
					length = facets.Length;
				}
				createFacets.Parameters.Add(new CodePrimitiveExpression((object)length));
				object maxExclusive = null;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) != 0)
				{
					maxExclusive = facets.MaxExclusive;
				}
				SimpleTypeCodeDomHelper.GetCreateValueExpression(maxExclusive, type, createFacets.Parameters);
				object maxInclusive = null;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) != 0)
				{
					maxInclusive = facets.MaxInclusive;
				}
				SimpleTypeCodeDomHelper.GetCreateValueExpression(maxInclusive, type, createFacets.Parameters);
				int maxLength = 0;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxLength) != 0)
				{
					maxLength = facets.MaxLength;
				}
				createFacets.Parameters.Add(new CodePrimitiveExpression((object)maxLength));
				object minExclusive = null;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) != 0)
				{
					minExclusive = facets.MinExclusive;
				}
				SimpleTypeCodeDomHelper.GetCreateValueExpression(minExclusive, type, createFacets.Parameters);
				object minInclusive = null;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) != 0)
				{
					minInclusive = facets.MinInclusive;
				}
				SimpleTypeCodeDomHelper.GetCreateValueExpression(minInclusive, type, createFacets.Parameters);
				int minLength = 0;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinLength) != 0)
				{
					minLength = facets.MinLength;
				}
				createFacets.Parameters.Add(new CodePrimitiveExpression((object)minLength));
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Pattern) == 0)
				{
					createFacets.Parameters.Add(new CodePrimitiveExpression(null));
				}
				else
				{
					CodeArrayCreateExpression patternStrs = new CodeArrayCreateExpression()
					{
						CreateType = new CodeTypeReference(XTypedServices.typeOfString)
					};
					foreach (object pattern in facets.Patterns)
					{
						string str = pattern.ToString();
						patternStrs.Initializers.Add(new CodePrimitiveExpression(str));
					}
					createFacets.Parameters.Add(patternStrs);
				}
				int totalDigits = 0;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.TotalDigits) != 0)
				{
					totalDigits = facets.TotalDigits;
				}
				createFacets.Parameters.Add(new CodePrimitiveExpression((object)totalDigits));
				Xml.Schema.Linq.XmlSchemaWhiteSpace ws = facets.WhiteSpace;
				createFacets.Parameters.Add(CodeDomHelper.CreateFieldReference("XmlSchemaWhiteSpace", ws.ToString()));
				codePrimitiveExpression = createFacets;
			}
			else
			{
				codePrimitiveExpression = new CodePrimitiveExpression(null);
			}
			return codePrimitiveExpression;
		}

		internal static CodeArrayCreateExpression CreateFixedDefaultArrayValueInit(string baseType, string value)
		{
			CodeArrayCreateExpression array = new CodeArrayCreateExpression(baseType, new CodeExpression[0]);
			string[] strArrays = value.Split(new char[] { ' ' });
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string s = strArrays[i];
				array.Initializers.Add(SimpleTypeCodeDomHelper.CreateValueExpression(baseType, s));
			}
			return array;
		}

		internal static CodeExpression CreateFixedDefaultValueExpression(CodeTypeReference type, string value)
		{
			CodeExpression codeExpression;
			string baseType = type.BaseType;
			if (baseType.Contains("Nullable"))
			{
				Debug.Assert(type.TypeArguments.Count == 1);
				baseType = type.TypeArguments[0].BaseType;
				codeExpression = SimpleTypeCodeDomHelper.CreateValueExpression(baseType, value);
			}
			else if (type.ArrayRank != 0)
			{
				baseType = type.ArrayElementType.BaseType;
				codeExpression = SimpleTypeCodeDomHelper.CreateFixedDefaultArrayValueInit(baseType, value);
			}
			else if (!baseType.Contains("List"))
			{
				codeExpression = SimpleTypeCodeDomHelper.CreateValueExpression(baseType, value);
			}
			else
			{
				Debug.Assert(type.TypeArguments.Count == 1);
				baseType = type.TypeArguments[0].BaseType;
				codeExpression = SimpleTypeCodeDomHelper.CreateFixedDefaultArrayValueInit(baseType, value);
			}
			return codeExpression;
		}

		internal static CodeExpression CreateGetBuiltInSimpleType(XmlTypeCode typeCode)
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XmlSchemaType");
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.CreateFieldReference("XmlTypeCode", typeCode.ToString()) };
			return CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "GetBuiltInSimpleType", codeExpressionArray);
		}

		internal static CodeExpression CreateSimpleTypeDef(ClrSimpleTypeInfo typeInfo, Dictionary<XmlSchemaObject, string> nameMappings, LinqToXsdSettings settings, bool memberOrItemType)
		{
			CodeExpression codeExpression;
			if ((!memberOrItemType ? true : !typeInfo.IsGlobal))
			{
				codeExpression = SimpleTypeCodeDomHelper.MaterializeSimpleTypeDef(typeInfo, nameMappings, settings);
			}
			else
			{
				typeInfo.UpdateClrTypeName(nameMappings, settings);
				codeExpression = CodeDomHelper.CreateFieldReference(typeInfo.clrtypeName, "TypeDefinition");
			}
			return codeExpression;
		}

		private static CodeExpression CreateTypeConversionExpr(string typeName, object value)
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(typeof(XmlConvert));
			string str = string.Concat("To", typeName);
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression(value.ToString()) };
			return CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, str, codePrimitiveExpression);
		}

		private static CodeExpression CreateTypedValueExpression(XmlSchemaDatatype dataType, object value)
		{
			CodeExpression codePrimitiveExpression;
			CodeExpression[] codeExpressionArray;
			switch (dataType.TypeCode)
			{
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
				case XmlTypeCode.Idref:
				case XmlTypeCode.Entity:
				{
					throw new InvalidOperationException();
				}
				case XmlTypeCode.Node:
				case XmlTypeCode.Document:
				case XmlTypeCode.Element:
				case XmlTypeCode.Attribute:
				case XmlTypeCode.Namespace:
				case XmlTypeCode.ProcessingInstruction:
				case XmlTypeCode.Comment:
				case XmlTypeCode.Text:
				case XmlTypeCode.UntypedAtomic:
				{
					throw new InvalidOperationException();
				}
				case XmlTypeCode.String:
				case XmlTypeCode.Notation:
				case XmlTypeCode.NormalizedString:
				case XmlTypeCode.Token:
				case XmlTypeCode.Language:
				case XmlTypeCode.Id:
				{
					string str = value as string;
					Debug.Assert(str != null);
					codePrimitiveExpression = new CodePrimitiveExpression(str);
					break;
				}
				case XmlTypeCode.Boolean:
				{
					Debug.Assert(value is bool);
					codePrimitiveExpression = new CodePrimitiveExpression(value);
					break;
				}
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Integer:
				case XmlTypeCode.NonPositiveInteger:
				case XmlTypeCode.NegativeInteger:
				case XmlTypeCode.Long:
				case XmlTypeCode.Int:
				case XmlTypeCode.Short:
				case XmlTypeCode.Byte:
				case XmlTypeCode.NonNegativeInteger:
				case XmlTypeCode.UnsignedLong:
				case XmlTypeCode.UnsignedInt:
				case XmlTypeCode.UnsignedShort:
				case XmlTypeCode.UnsignedByte:
				case XmlTypeCode.PositiveInteger:
				{
					codePrimitiveExpression = new CodePrimitiveExpression(value);
					break;
				}
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
				{
					Debug.Assert((value is double ? true : value is float));
					codePrimitiveExpression = new CodePrimitiveExpression(value);
					break;
				}
				case XmlTypeCode.Duration:
				{
					Debug.Assert(value is TimeSpan);
					TimeSpan ts = (TimeSpan)value;
					Type type = typeof(TimeSpan);
					codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression((object)ts.Ticks) };
					codePrimitiveExpression = new CodeObjectCreateExpression(type, codeExpressionArray);
					break;
				}
				case XmlTypeCode.DateTime:
				case XmlTypeCode.Time:
				case XmlTypeCode.Date:
				case XmlTypeCode.GYearMonth:
				case XmlTypeCode.GYear:
				case XmlTypeCode.GMonthDay:
				case XmlTypeCode.GDay:
				case XmlTypeCode.GMonth:
				{
					Debug.Assert(value is DateTime);
					DateTime dt = (DateTime)value;
					Type type1 = typeof(DateTime);
					codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression((object)dt.Ticks) };
					codePrimitiveExpression = new CodeObjectCreateExpression(type1, codeExpressionArray);
					break;
				}
				case XmlTypeCode.HexBinary:
				case XmlTypeCode.Base64Binary:
				{
					codePrimitiveExpression = SimpleTypeCodeDomHelper.CreateByteArrayExpression(value);
					break;
				}
				case XmlTypeCode.AnyUri:
				{
					Debug.Assert(value is Uri);
					Type type2 = typeof(Uri);
					codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression(((Uri)value).OriginalString) };
					codePrimitiveExpression = new CodeObjectCreateExpression(type2, codeExpressionArray);
					break;
				}
				case XmlTypeCode.QName:
				{
					XmlQualifiedName qname = value as XmlQualifiedName;
					Type type3 = typeof(XmlQualifiedName);
					codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression(qname.Name), new CodePrimitiveExpression(qname.Namespace) };
					codePrimitiveExpression = new CodeObjectCreateExpression(type3, codeExpressionArray);
					break;
				}
				case XmlTypeCode.NmToken:
				case XmlTypeCode.Name:
				case XmlTypeCode.NCName:
				{
					CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(typeof(XmlConvert));
					codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression(value.ToString()) };
					codePrimitiveExpression = CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "EncodeName", codeExpressionArray);
					break;
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
			return codePrimitiveExpression;
		}

		internal static CodeExpression CreateValueExpression(string builtInType, string strValue)
		{
			CodeExpression codePrimitiveExpression;
			int dot = builtInType.LastIndexOf('.');
			Debug.Assert(dot != -1);
			string localType = builtInType.Substring(dot + 1);
			if (!(localType == "String" ? false : !(localType == "Object")))
			{
				codePrimitiveExpression = new CodePrimitiveExpression(strValue);
			}
			else if (!(localType == "Uri"))
			{
				codePrimitiveExpression = SimpleTypeCodeDomHelper.CreateTypeConversionExpr(localType, strValue);
			}
			else
			{
				CodeExpression[] codeExpressionArray = new CodeExpression[] { new CodePrimitiveExpression(strValue) };
				codePrimitiveExpression = new CodeObjectCreateExpression("Uri", codeExpressionArray);
			}
			return codePrimitiveExpression;
		}

		public static void GetCreateUnionValueExpression(object value, UnionSimpleTypeInfo unionDef, CodeExpressionCollection collection)
		{
			Debug.Assert(unionDef != null);
			object typedValue = value.GetType().InvokeMember("TypedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, null, value, null, CultureInfo.InvariantCulture);
			CodeExpressionCollection dummy = new CodeExpressionCollection();
			ClrSimpleTypeInfo matchingType = null;
			ClrSimpleTypeInfo[] memberTypes = unionDef.MemberTypes;
			for (int i = 0; i < (int)memberTypes.Length; i++)
			{
				ClrSimpleTypeInfo type = memberTypes[i];
				try
				{
					SimpleTypeCodeDomHelper.GetCreateValueExpression(typedValue, type, dummy);
					matchingType = type;
					break;
				}
				catch (Exception exception)
				{
				}
			}
			Debug.Assert(matchingType != null);
			SimpleTypeCodeDomHelper.GetCreateValueExpression(typedValue, matchingType, collection);
		}

		public static void GetCreateValueExpression(object value, ClrSimpleTypeInfo typeDef, CodeExpressionCollection collection)
		{
			if (value != null)
			{
				switch (typeDef.Variety)
				{
					case XmlSchemaDatatypeVariety.Atomic:
					{
						if (!(value is string))
						{
							collection.Add(SimpleTypeCodeDomHelper.CreateTypedValueExpression(typeDef.InnerType.Datatype, value));
						}
						else
						{
							collection.Add(new CodePrimitiveExpression(value));
						}
						break;
					}
					case XmlSchemaDatatypeVariety.List:
					{
						string str = ListSimpleTypeValidator.ToString(value);
						collection.Add(new CodePrimitiveExpression(str));
						break;
					}
					case XmlSchemaDatatypeVariety.Union:
					{
						SimpleTypeCodeDomHelper.GetCreateUnionValueExpression(value, typeDef as UnionSimpleTypeInfo, collection);
						break;
					}
				}
			}
			else
			{
				collection.Add(new CodePrimitiveExpression(value));
			}
		}

		internal static CodeExpression MaterializeSimpleTypeDef(ClrSimpleTypeInfo typeInfo, Dictionary<XmlSchemaObject, string> nameMappings, LinqToXsdSettings settings)
		{
			CodeObjectCreateExpression simpleTypeCreate = null;
			CodeExpressionCollection expressions = null;
			switch (typeInfo.Variety)
			{
				case XmlSchemaDatatypeVariety.Atomic:
				{
					simpleTypeCreate = new CodeObjectCreateExpression("Xml.Schema.Linq.AtomicSimpleTypeValidator", new CodeExpression[0]);
					expressions = simpleTypeCreate.Parameters;
					expressions.Add(SimpleTypeCodeDomHelper.CreateGetBuiltInSimpleType(typeInfo.TypeCode));
					expressions.Add(SimpleTypeCodeDomHelper.CreateFacets(typeInfo));
					break;
				}
				case XmlSchemaDatatypeVariety.List:
				{
					simpleTypeCreate = new CodeObjectCreateExpression("Xml.Schema.Linq.ListSimpleTypeValidator", new CodeExpression[0]);
					expressions = simpleTypeCreate.Parameters;
					expressions.Add(SimpleTypeCodeDomHelper.CreateGetBuiltInSimpleType(typeInfo.TypeCode));
					expressions.Add(SimpleTypeCodeDomHelper.CreateFacets(typeInfo));
					ClrSimpleTypeInfo itemType = (typeInfo as ListSimpleTypeInfo).ItemType;
					expressions.Add(SimpleTypeCodeDomHelper.CreateSimpleTypeDef(itemType, nameMappings, settings, true));
					break;
				}
				case XmlSchemaDatatypeVariety.Union:
				{
					simpleTypeCreate = new CodeObjectCreateExpression("Xml.Schema.Linq.UnionSimpleTypeValidator", new CodeExpression[0]);
					expressions = simpleTypeCreate.Parameters;
					expressions.Add(SimpleTypeCodeDomHelper.CreateGetBuiltInSimpleType(typeInfo.TypeCode));
					expressions.Add(SimpleTypeCodeDomHelper.CreateFacets(typeInfo));
					UnionSimpleTypeInfo unionType = typeInfo as UnionSimpleTypeInfo;
					CodeArrayCreateExpression memberTypeCreate = new CodeArrayCreateExpression()
					{
						CreateType = new CodeTypeReference("Xml.Schema.Linq.SimpleTypeValidator")
					};
					ClrSimpleTypeInfo[] memberTypes = unionType.MemberTypes;
					for (int i = 0; i < (int)memberTypes.Length; i++)
					{
						ClrSimpleTypeInfo st = memberTypes[i];
						memberTypeCreate.Initializers.Add(SimpleTypeCodeDomHelper.CreateSimpleTypeDef(st, nameMappings, settings, true));
					}
					expressions.Add(memberTypeCreate);
					break;
				}
			}
			return simpleTypeCreate;
		}
	}
}