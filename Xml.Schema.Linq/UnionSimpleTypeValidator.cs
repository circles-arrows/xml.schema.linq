using System;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class UnionSimpleTypeValidator : SimpleTypeValidator
	{
		private SimpleTypeValidator[] memberTypes;

		internal SimpleTypeValidator[] MemberTypes
		{
			get
			{
				return this.memberTypes;
			}
		}

		public UnionSimpleTypeValidator(XmlSchemaSimpleType type, Xml.Schema.Linq.RestrictionFacets facets, SimpleTypeValidator[] memberTypes) : base(XmlSchemaDatatypeVariety.Union, type, Xml.Schema.Linq.FacetsChecker.UnionFacetsChecker, facets)
		{
			this.memberTypes = memberTypes;
		}

		internal override Exception TryParseValue(object value, NameTable nameTable, XNamespaceResolver resolver, out SimpleTypeValidator matchingType, out object typedValue)
		{
			Exception unionMemberTypeNotFoundException;
			typedValue = null;
			matchingType = null;
			if (value != null)
			{
				object typedMemberValue = null;
				SimpleTypeValidator[] simpleTypeValidatorArray = this.memberTypes;
				int num = 0;
				while (num < (int)simpleTypeValidatorArray.Length)
				{
					if (simpleTypeValidatorArray[num].TryParseValue(value, nameTable, resolver, out matchingType, out typedMemberValue) != null)
					{
						num++;
					}
					else
					{
						break;
					}
				}
				if (typedMemberValue != null)
				{
					Exception e = null;
					if ((base.RestrictionFacets == null ? false : base.RestrictionFacets.HasLexicalFacets))
					{
						string parsedString = null;
						e = matchingType.TryParseString(value, nameTable, resolver, out parsedString);
						if (e == null)
						{
							e = this.facetsChecker.CheckLexicalFacets(ref parsedString, value, nameTable, resolver, this);
						}
					}
					if (e == null)
					{
						e = this.facetsChecker.CheckValueFacets(typedMemberValue, this);
					}
					if (e == null)
					{
						typedValue = typedMemberValue;
						unionMemberTypeNotFoundException = null;
					}
					else
					{
						unionMemberTypeNotFoundException = e;
					}
				}
				else
				{
					unionMemberTypeNotFoundException = new UnionMemberTypeNotFoundException(value, this);
				}
			}
			else
			{
				unionMemberTypeNotFoundException = new ArgumentNullException("Argument value should not be null.");
			}
			return unionMemberTypeNotFoundException;
		}
	}
}