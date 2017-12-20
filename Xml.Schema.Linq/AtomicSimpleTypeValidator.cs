using System;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class AtomicSimpleTypeValidator : SimpleTypeValidator
	{
		public AtomicSimpleTypeValidator(XmlSchemaSimpleType type, Xml.Schema.Linq.RestrictionFacets facets) : base(XmlSchemaDatatypeVariety.Atomic, type, Xml.Schema.Linq.FacetsChecker.GetFacetsChecker(type.Datatype.TypeCode), facets)
		{
		}

		private Exception TryMatchAtomicType(object value, NameTable nameTable, XNamespaceResolver resolver)
		{
			XmlSchemaDatatype datatype = base.DataType;
			XmlTypeCode typeCode = datatype.TypeCode;
			Exception exception = null;
			try
			{
				datatype.ChangeType(value, base.DataType.ValueType, resolver);
			}
			catch (Exception exception1)
			{
				exception = exception1;
			}
			return exception;
		}

		internal override Exception TryParseString(object value, NameTable nameTable, XNamespaceResolver resolver, out string parsedString)
		{
			Exception exception;
			parsedString = value as string;
			if (parsedString == null)
			{
				try
				{
					parsedString = (string)base.DataType.ChangeType(value, XTypedServices.typeOfString);
				}
				catch (Exception exception1)
				{
					exception = exception1;
					return exception;
				}
			}
			exception = null;
			return exception;
		}

		internal override Exception TryParseValue(object value, NameTable nameTable, XNamespaceResolver resolver, out SimpleTypeValidator matchingType, out object typedValue)
		{
			Exception exception;
			Exception e = this.TryMatchAtomicType(value, nameTable, resolver);
			matchingType = null;
			typedValue = null;
			if (e == null)
			{
				try
				{
					if ((base.RestrictionFacets == null ? false : base.RestrictionFacets.HasLexicalFacets))
					{
						string parsedString = null;
						e = this.TryParseString(value, nameTable, resolver, out parsedString);
						if (e == null)
						{
							e = this.facetsChecker.CheckLexicalFacets(ref parsedString, value, nameTable, resolver, this);
						}
					}
					if (e == null)
					{
						e = this.facetsChecker.CheckValueFacets(value, this);
					}
					if (e == null)
					{
						matchingType = this;
						typedValue = base.DataType.ChangeType(value, base.DataType.ValueType);
					}
					exception = e;
				}
				catch (Exception exception1)
				{
					exception = exception1;
				}
			}
			else
			{
				exception = e;
			}
			return exception;
		}
	}
}