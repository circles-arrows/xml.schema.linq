using System;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class SimpleTypeValidator
	{
		private Xml.Schema.Linq.RestrictionFacets restrictionFacets;

		private XmlSchemaDatatype dataType;

		private XmlSchemaDatatypeVariety variety;

		internal Xml.Schema.Linq.FacetsChecker facetsChecker;

		internal XmlSchemaDatatype DataType
		{
			get
			{
				return this.dataType;
			}
		}

		internal Xml.Schema.Linq.RestrictionFacets RestrictionFacets
		{
			get
			{
				return this.restrictionFacets;
			}
		}

		internal XmlSchemaDatatypeVariety Variety
		{
			get
			{
				return this.variety;
			}
		}

		internal SimpleTypeValidator(XmlSchemaDatatypeVariety variety, XmlSchemaSimpleType type, Xml.Schema.Linq.FacetsChecker facetsChecker, Xml.Schema.Linq.RestrictionFacets facets)
		{
			this.restrictionFacets = facets;
			this.facetsChecker = facetsChecker;
			this.dataType = type.Datatype;
			this.variety = variety;
		}

		internal virtual Exception TryParseString(object value, NameTable nameTable, XNamespaceResolver resolver, out string parsedString)
		{
			parsedString = null;
			throw new InvalidOperationException();
		}

		internal virtual Exception TryParseValue(object value, NameTable nameTable, XNamespaceResolver resolver, out SimpleTypeValidator matchingType, out object typedValue)
		{
			matchingType = null;
			typedValue = null;
			throw new InvalidOperationException();
		}
	}
}