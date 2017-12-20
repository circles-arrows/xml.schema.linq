using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal static class SOMQueryExtensions
	{
		public static bool Contains(this XmlSchemaParticle particle, XmlSchemaParticle childParticle)
		{
			bool flag;
			XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
			if (groupBase != null)
			{
				foreach (XmlSchemaParticle item in groupBase.Items)
				{
					if (item == childParticle)
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}

		public static bool ContainsElement(this XmlSchemaParticle particle, XmlSchemaElement element)
		{
			bool flag;
			XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
			if (groupBase != null)
			{
				foreach (XmlSchemaParticle p in groupBase.Items)
				{
					if (p == element)
					{
						flag = true;
						return flag;
					}
					else if (p.ContainsElement(element))
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}

		public static bool ContainsName(this XmlSchemaParticle particle, XmlQualifiedName elementName)
		{
			bool flag;
			XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
			if (groupBase != null)
			{
				foreach (XmlSchemaParticle p in groupBase.Items)
				{
					XmlSchemaElement localElement = p as XmlSchemaElement;
					if (localElement != null)
					{
						if (localElement.QualifiedName == elementName)
						{
							flag = true;
							return flag;
						}
					}
					else if (p.ContainsName(elementName))
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}

		public static bool ContainsName(this XmlSchemaComplexType baseType, XmlQualifiedName elementName)
		{
			return baseType.ContentTypeParticle.ContainsName(elementName);
		}

		public static bool ContainsWildCard(this XmlSchemaParticle particle, XmlSchemaAny any)
		{
			bool flag;
			XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
			if (groupBase != null)
			{
				foreach (XmlSchemaParticle p in groupBase.Items)
				{
					if (p == any)
					{
						flag = true;
						return flag;
					}
					else if (p.ContainsWildCard(any))
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}

		public static XmlSchemaForm FormResolved(this XmlSchemaElement elem)
		{
			XmlSchemaForm xmlSchemaForm;
			if (elem.RefName.IsEmpty)
			{
				XmlSchemaForm form = elem.Form;
				xmlSchemaForm = (form != XmlSchemaForm.None ? form : SOMQueryExtensions.GetParentSchema(elem).ElementFormDefault);
			}
			else
			{
				xmlSchemaForm = XmlSchemaForm.Qualified;
			}
			return xmlSchemaForm;
		}

		public static XmlSchemaForm FormResolved(this XmlSchemaAttribute attribute)
		{
			XmlSchemaForm xmlSchemaForm;
			XmlSchemaForm form = attribute.Form;
			xmlSchemaForm = (form != XmlSchemaForm.None ? form : SOMQueryExtensions.GetParentSchema(attribute).AttributeFormDefault);
			return xmlSchemaForm;
		}

		public static bool FromSchemaNamespace(this XmlSchemaType schemaType)
		{
			bool flag;
			flag = (!(schemaType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema") ? false : true);
			return flag;
		}

		public static XmlSchemaSimpleType GetBaseSimpleType(this XmlSchemaComplexType type)
		{
			XmlSchemaType baseType = type.BaseXmlSchemaType;
			while (true)
			{
				if ((baseType == null ? true : !(baseType is XmlSchemaComplexType)))
				{
					break;
				}
				baseType = baseType.BaseXmlSchemaType;
			}
			if (baseType == null)
			{
				baseType = XmlSchemaType.GetBuiltInSimpleType(type.TypeCode);
			}
			return baseType as XmlSchemaSimpleType;
		}

		public static Xml.Schema.Linq.XmlSchemaWhiteSpace GetBuiltInWSFacet(this XmlSchemaDatatype dt)
		{
			Xml.Schema.Linq.XmlSchemaWhiteSpace xmlSchemaWhiteSpace;
			if (dt.TypeCode != XmlTypeCode.NormalizedString)
			{
				xmlSchemaWhiteSpace = (dt.TypeCode != XmlTypeCode.String ? Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse : Xml.Schema.Linq.XmlSchemaWhiteSpace.Preserve);
			}
			else
			{
				xmlSchemaWhiteSpace = Xml.Schema.Linq.XmlSchemaWhiteSpace.Replace;
			}
			return xmlSchemaWhiteSpace;
		}

		public static XmlSchemaContentType GetContentType(this XmlSchemaType schemaType)
		{
			XmlSchemaContentType contentType;
			if (schemaType == null)
			{
				contentType = XmlSchemaContentType.Empty;
			}
			else if (schemaType.Datatype == null)
			{
				XmlSchemaComplexType ct = schemaType as XmlSchemaComplexType;
				Debug.Assert(ct != null);
				contentType = ct.ContentType;
			}
			else
			{
				contentType = XmlSchemaContentType.TextOnly;
			}
			return contentType;
		}

		public static XmlSchemaSimpleType GetListItemType(this XmlSchemaSimpleType type)
		{
			XmlSchemaSimpleType listItemType;
			Debug.Assert(type != null);
			Debug.Assert(type.Datatype.Variety == XmlSchemaDatatypeVariety.List);
			XmlSchemaSimpleTypeList listContent = type.Content as XmlSchemaSimpleTypeList;
			if (listContent == null)
			{
				Debug.Assert(type.DerivedBy == XmlSchemaDerivationMethod.Restriction);
				listItemType = (type.BaseXmlSchemaType as XmlSchemaSimpleType).GetListItemType();
			}
			else
			{
				listItemType = listContent.BaseItemType;
			}
			return listItemType;
		}

		public static XmlSchema GetParentSchema(XmlSchemaObject currentSchemaObject)
		{
			XmlSchema parentSchema = null;
			while (true)
			{
				if ((parentSchema != null ? true : currentSchemaObject == null))
				{
					break;
				}
				currentSchemaObject = currentSchemaObject.Parent;
				parentSchema = currentSchemaObject as XmlSchema;
			}
			return parentSchema;
		}

		public static ParticleType GetParticleType(this XmlSchemaParticle particle)
		{
			ParticleType particleType;
			if (particle is XmlSchemaElement)
			{
				particleType = ParticleType.Element;
			}
			else if (particle is XmlSchemaSequence)
			{
				particleType = ParticleType.Sequence;
			}
			else if (particle is XmlSchemaChoice)
			{
				particleType = ParticleType.Choice;
			}
			else if (particle is XmlSchemaAll)
			{
				particleType = ParticleType.All;
			}
			else if (!(particle is XmlSchemaAny))
			{
				particleType = (!(particle is XmlSchemaGroupRef) ? ParticleType.Empty : ParticleType.GroupRef);
			}
			else
			{
				particleType = ParticleType.Any;
			}
			return particleType;
		}

		public static string GetTargetNS(this XmlSchemaAny any)
		{
			XmlSchemaObject parentObj = any.Parent;
			XmlSchema schemaObj = parentObj as XmlSchema;
			while (schemaObj == null)
			{
				if (parentObj != null)
				{
					parentObj = parentObj.Parent;
					schemaObj = parentObj as XmlSchema;
				}
				else
				{
					break;
				}
			}
			return (schemaObj != null ? schemaObj.TargetNamespace : "");
		}

		public static XmlSchemaSimpleType[] GetUnionMemberTypes(this XmlSchemaSimpleType type)
		{
			XmlSchemaSimpleType[] unionMemberTypes;
			Debug.Assert(type != null);
			Debug.Assert(type.Datatype.Variety == XmlSchemaDatatypeVariety.Union);
			XmlSchemaSimpleTypeUnion unionContent = type.Content as XmlSchemaSimpleTypeUnion;
			if (unionContent == null)
			{
				Debug.Assert(type.DerivedBy == XmlSchemaDerivationMethod.Restriction);
				unionMemberTypes = (type.BaseXmlSchemaType as XmlSchemaSimpleType).GetUnionMemberTypes();
			}
			else
			{
				unionMemberTypes = unionContent.BaseMemberTypes;
			}
			return unionMemberTypes;
		}

		public static bool HasFacetRestrictions(this XmlSchemaSimpleType sst)
		{
			bool flag;
			if (sst.IsDerivedByRestriction())
			{
				flag = true;
			}
			else if (sst.Datatype.Variety != XmlSchemaDatatypeVariety.List)
			{
				if (sst.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
				{
					XmlSchemaSimpleType[] unionMemberTypes = sst.GetUnionMemberTypes();
					int num = 0;
					while (num < (int)unionMemberTypes.Length)
					{
						if (!unionMemberTypes[num].HasFacetRestrictions())
						{
							num++;
						}
						else
						{
							flag = true;
							return flag;
						}
					}
				}
				flag = false;
			}
			else
			{
				flag = sst.GetListItemType().HasFacetRestrictions();
			}
			return flag;
		}

		public static bool HasFacetRestrictions(this XmlSchemaComplexType ct)
		{
			bool flag;
			if (ct.GetContentType() == XmlSchemaContentType.TextOnly)
			{
				XmlSchemaSimpleType baseType = ct.BaseXmlSchemaType as XmlSchemaSimpleType;
				if (baseType != null)
				{
					flag = baseType.HasFacetRestrictions();
					return flag;
				}
				else if (ct.IsDerivedByRestriction())
				{
					flag = true;
					return flag;
				}
			}
			flag = false;
			return flag;
		}

		public static bool IsBuiltInSimpleType(this XmlSchemaSimpleType derivedType)
		{
			bool flag;
			flag = (!(derivedType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema") ? false : true);
			return flag;
		}

		public static bool IsDerivedByRestriction(this XmlSchemaType derivedType)
		{
			bool flag;
			XmlSchemaComplexType ct = derivedType as XmlSchemaComplexType;
			flag = (ct == null ? (derivedType as XmlSchemaSimpleType).IsDerivedByRestriction() : ct.IsDerivedByRestriction());
			return flag;
		}

		public static bool IsDerivedByRestriction(this XmlSchemaComplexType derivedType)
		{
			return ((derivedType.DerivedBy != XmlSchemaDerivationMethod.Restriction ? true : derivedType.BaseXmlSchemaType == XmlSchemaType.GetBuiltInComplexType(XmlTypeCode.Item)) ? false : true);
		}

		public static bool IsDerivedByRestriction(this XmlSchemaSimpleType derivedType)
		{
			return ((derivedType.DerivedBy != XmlSchemaDerivationMethod.Restriction ? true : derivedType.IsBuiltInSimpleType()) ? false : true);
		}

		public static bool IsFinal(this XmlSchemaComplexType ct)
		{
			return ((ct.FinalResolved == XmlSchemaDerivationMethod.All || ct.FinalResolved == XmlSchemaDerivationMethod.Extension ? false : ct.FinalResolved != XmlSchemaDerivationMethod.Restriction) ? false : true);
		}

		public static bool IsGlobal(this XmlSchemaType schemaType)
		{
			return ((schemaType.QualifiedName.IsEmpty ? true : schemaType.TypeCode == XmlTypeCode.Item) ? false : true);
		}

		public static bool IsGlobal(this XmlSchemaElement elem)
		{
			bool flag;
			if (elem.RefName.IsEmpty)
			{
				flag = (!(elem.Parent is XmlSchema) ? false : true);
			}
			else
			{
				flag = true;
			}
			return flag;
		}

		public static bool IsOrHasList(this XmlSchemaSimpleType type)
		{
			bool flag;
			switch (type.Datatype.Variety)
			{
				case XmlSchemaDatatypeVariety.Atomic:
				{
					flag = false;
					break;
				}
				case XmlSchemaDatatypeVariety.List:
				{
					flag = true;
					break;
				}
				case XmlSchemaDatatypeVariety.Union:
				{
					XmlSchemaSimpleType[] unionMemberTypes = type.GetUnionMemberTypes();
					int num = 0;
					while (num < (int)unionMemberTypes.Length)
					{
						if (!unionMemberTypes[num].IsOrHasList())
						{
							num++;
						}
						else
						{
							flag = true;
							return flag;
						}
					}
					flag = false;
					break;
				}
				default:
				{
					throw new InvalidOperationException("Unknown type variety");
				}
			}
			return flag;
		}

		public static bool IsOrHasUnion(this XmlSchemaSimpleType type)
		{
			bool flag;
			switch (type.Datatype.Variety)
			{
				case XmlSchemaDatatypeVariety.Atomic:
				{
					flag = false;
					break;
				}
				case XmlSchemaDatatypeVariety.List:
				{
					flag = type.GetListItemType().IsOrHasUnion();
					break;
				}
				case XmlSchemaDatatypeVariety.Union:
				{
					flag = true;
					break;
				}
				default:
				{
					throw new InvalidOperationException("Unknown type variety");
				}
			}
			return flag;
		}

		public static string TypeCodeString(this XmlSchemaDatatype datatype)
		{
			string str;
			string typeCodeString = string.Empty;
			XmlTypeCode typeCode = datatype.TypeCode;
			switch (typeCode)
			{
				case XmlTypeCode.None:
				{
					str = "None";
					break;
				}
				case XmlTypeCode.Item:
				{
					str = "AnyType";
					break;
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
					str = typeCode.ToString();
					break;
				}
				case XmlTypeCode.AnyAtomicType:
				{
					str = "AnyAtomicType";
					break;
				}
				case XmlTypeCode.String:
				{
					str = "String";
					break;
				}
				case XmlTypeCode.Boolean:
				{
					str = "Boolean";
					break;
				}
				case XmlTypeCode.Decimal:
				{
					str = "Decimal";
					break;
				}
				case XmlTypeCode.Float:
				{
					str = "Float";
					break;
				}
				case XmlTypeCode.Double:
				{
					str = "Double";
					break;
				}
				case XmlTypeCode.Duration:
				{
					str = "Duration";
					break;
				}
				case XmlTypeCode.DateTime:
				{
					str = "DateTime";
					break;
				}
				case XmlTypeCode.Time:
				{
					str = "Time";
					break;
				}
				case XmlTypeCode.Date:
				{
					str = "Date";
					break;
				}
				case XmlTypeCode.GYearMonth:
				{
					str = "GYearMonth";
					break;
				}
				case XmlTypeCode.GYear:
				{
					str = "GYear";
					break;
				}
				case XmlTypeCode.GMonthDay:
				{
					str = "GMonthDay";
					break;
				}
				case XmlTypeCode.GDay:
				{
					str = "GDay";
					break;
				}
				case XmlTypeCode.GMonth:
				{
					str = "GMonth";
					break;
				}
				case XmlTypeCode.HexBinary:
				{
					str = "HexBinary";
					break;
				}
				case XmlTypeCode.Base64Binary:
				{
					str = "Base64Binary";
					break;
				}
				case XmlTypeCode.AnyUri:
				{
					str = "AnyUri";
					break;
				}
				case XmlTypeCode.QName:
				{
					str = "QName";
					break;
				}
				case XmlTypeCode.Notation:
				{
					str = "Notation";
					break;
				}
				case XmlTypeCode.NormalizedString:
				{
					str = "NormalizedString";
					break;
				}
				case XmlTypeCode.Token:
				{
					str = "Token";
					break;
				}
				case XmlTypeCode.Language:
				{
					str = "Language";
					break;
				}
				case XmlTypeCode.NmToken:
				{
					str = "NmToken";
					break;
				}
				case XmlTypeCode.Name:
				{
					str = "Name";
					break;
				}
				case XmlTypeCode.NCName:
				{
					str = "NCName";
					break;
				}
				case XmlTypeCode.Id:
				{
					str = "Id";
					break;
				}
				case XmlTypeCode.Idref:
				{
					str = "Idref";
					break;
				}
				case XmlTypeCode.Entity:
				{
					str = "Entity";
					break;
				}
				case XmlTypeCode.Integer:
				{
					str = "Integer";
					break;
				}
				case XmlTypeCode.NonPositiveInteger:
				{
					str = "NonPositiveInteger";
					break;
				}
				case XmlTypeCode.NegativeInteger:
				{
					str = "NegativeInteger";
					break;
				}
				case XmlTypeCode.Long:
				{
					str = "Long";
					break;
				}
				case XmlTypeCode.Int:
				{
					str = "Int";
					break;
				}
				case XmlTypeCode.Short:
				{
					str = "Short";
					break;
				}
				case XmlTypeCode.Byte:
				{
					str = "Byte";
					break;
				}
				case XmlTypeCode.NonNegativeInteger:
				{
					str = "NonNegativeInteger";
					break;
				}
				case XmlTypeCode.UnsignedLong:
				{
					str = "UnsignedLong";
					break;
				}
				case XmlTypeCode.UnsignedInt:
				{
					str = "UnsignedInt";
					break;
				}
				case XmlTypeCode.UnsignedShort:
				{
					str = "UnsignedShort";
					break;
				}
				case XmlTypeCode.UnsignedByte:
				{
					str = "UnsignedByte";
					break;
				}
				case XmlTypeCode.PositiveInteger:
				{
					str = "PositiveInteger";
					break;
				}
				default:
				{
					goto case XmlTypeCode.UntypedAtomic;
				}
			}
			return str;
		}
	}
}