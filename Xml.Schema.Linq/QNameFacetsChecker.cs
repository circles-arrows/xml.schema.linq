using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class QNameFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public QNameFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				XmlQualifiedName qualifiedNameValue = (XmlQualifiedName)type.DataType.ChangeType(value, typeof(XmlQualifiedName));
				exception = this.CheckValueFacets(qualifiedNameValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(XmlQualifiedName value, SimpleTypeValidator type)
		{
			Exception linqToXsdFacetException;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if (!(facets == null ? false : facets.HasValueFacets))
			{
				linqToXsdFacetException = null;
			}
			else if (facets != null)
			{
				Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
				XmlSchemaDatatype datatype = type.DataType;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Enumeration) != 0)
				{
					if (!this.MatchEnumeration(value, facets.Enumeration, datatype))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Enumeration, facets.Enumeration, value);
						return linqToXsdFacetException;
					}
				}
				int length = value.ToString().Length;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Length) != 0)
				{
					if (length != facets.Length)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Length, (object)facets.Length, value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxLength) != 0)
				{
					if (length > facets.MaxLength)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxLength, (object)facets.MaxLength, value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinLength) != 0)
				{
					if (length < facets.MinLength)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinLength, (object)facets.MinLength, value);
						return linqToXsdFacetException;
					}
				}
				linqToXsdFacetException = null;
			}
			else
			{
				linqToXsdFacetException = null;
			}
			return linqToXsdFacetException;
		}

		internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag = this.MatchEnumeration((XmlQualifiedName)datatype.ChangeType(value, typeof(XmlQualifiedName)), enumeration, datatype);
			return flag;
		}

		private bool MatchEnumeration(XmlQualifiedName value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (XmlQualifiedName correctValue in enumeration)
			{
				if (value.Equals(correctValue))
				{
					flag = true;
					return flag;
				}
			}
			flag = false;
			return flag;
		}
	}
}