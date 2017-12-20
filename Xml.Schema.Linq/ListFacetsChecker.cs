using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class ListFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public ListFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception linqToXsdFacetException;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if ((facets == null ? false : facets.HasValueFacets))
			{
				IList listValue = null;
				Exception e = ListSimpleTypeValidator.ToList(value, ref listValue);
				if (e == null)
				{
					int length = listValue.Count;
					XmlSchemaDatatype datatype = type.DataType;
					Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
					if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Enumeration) != 0)
					{
						if (!this.MatchEnumeration(value, facets.Enumeration, datatype))
						{
							linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Enumeration, facets.Enumeration, value);
							return linqToXsdFacetException;
						}
					}
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
					linqToXsdFacetException = e;
				}
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
			string strValue = ListSimpleTypeValidator.ToString(value);
			foreach (object correctArray in enumeration)
			{
				if (strValue.Equals(correctArray))
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