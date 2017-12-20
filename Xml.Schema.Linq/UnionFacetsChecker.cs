using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class UnionFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public UnionFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception linqToXsdFacetException;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if ((facets == null ? false : facets.HasValueFacets))
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
			bool flag;
			foreach (object correctValue in enumeration)
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