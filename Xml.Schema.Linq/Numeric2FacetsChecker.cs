using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class Numeric2FacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public Numeric2FacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				double doubleValue = (double)type.DataType.ChangeType(value, typeof(double));
				exception = this.CheckValueFacets(doubleValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(double value, SimpleTypeValidator type)
		{
			Exception linqToXsdFacetException;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if ((facets == null ? false : facets.HasValueFacets))
			{
				Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
				XmlSchemaDatatype datatype = type.DataType;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) != 0)
				{
					if (value < (double)datatype.ChangeType(facets.MinInclusive, typeof(double)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinInclusive, facets.MinInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) != 0)
				{
					if (value <= (double)datatype.ChangeType(facets.MinExclusive, typeof(double)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinExclusive, facets.MinExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) != 0)
				{
					if (value > (double)datatype.ChangeType(facets.MaxInclusive, typeof(double)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxInclusive, facets.MaxInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) != 0)
				{
					if (value >= (double)datatype.ChangeType(facets.MaxExclusive, typeof(double)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MaxExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.Enumeration) != 0)
				{
					if (!this.MatchEnumeration(value, facets.Enumeration, datatype))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Enumeration, facets.Enumeration, (object)value);
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

		internal override Exception CheckValueFacets(float value, SimpleTypeValidator type)
		{
			return this.CheckValueFacets((double)value, type);
		}

		internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag = this.MatchEnumeration((double)datatype.ChangeType(value, typeof(double)), enumeration, datatype);
			return flag;
		}

		private bool MatchEnumeration(double value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (object correctValue in enumeration)
			{
				if (value == (double)datatype.ChangeType(correctValue, typeof(double)))
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