using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class DurationFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public DurationFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				TimeSpan timeSpanValue = (TimeSpan)type.DataType.ChangeType(value, typeof(TimeSpan));
				exception = this.CheckValueFacets(timeSpanValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(TimeSpan value, SimpleTypeValidator type)
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
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Enumeration, facets.Enumeration, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) != 0)
				{
					if (TimeSpan.Compare(value, (TimeSpan)datatype.ChangeType(facets.MaxInclusive, typeof(TimeSpan))) > 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxInclusive, facets.MaxInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) != 0)
				{
					if (TimeSpan.Compare(value, (TimeSpan)datatype.ChangeType(facets.MaxExclusive, typeof(TimeSpan))) >= 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MaxExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) != 0)
				{
					if (TimeSpan.Compare(value, (TimeSpan)datatype.ChangeType(facets.MinInclusive, typeof(TimeSpan))) < 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MinInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) != 0)
				{
					if (TimeSpan.Compare(value, (TimeSpan)datatype.ChangeType(facets.MinExclusive, typeof(TimeSpan))) <= 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MinExclusive, (object)value);
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
			return this.MatchEnumeration((TimeSpan)value, enumeration, datatype);
		}

		private bool MatchEnumeration(TimeSpan value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (TimeSpan correctValue in enumeration)
			{
				if (TimeSpan.Compare(value, correctValue) == 0)
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