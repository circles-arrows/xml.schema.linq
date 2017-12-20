using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class DateTimeFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public DateTimeFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				DateTime dateTimeValue = (DateTime)type.DataType.ChangeType(value, typeof(DateTime));
				exception = this.CheckValueFacets(dateTimeValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(DateTime value, SimpleTypeValidator type)
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
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) != 0)
				{
					if (DateTime.Compare(value, (DateTime)datatype.ChangeType(facets.MinInclusive, typeof(DateTime))) < 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinInclusive, facets.MinInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) != 0)
				{
					if (DateTime.Compare(value, (DateTime)datatype.ChangeType(facets.MinExclusive, typeof(DateTime))) <= 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinExclusive, facets.MinInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) != 0)
				{
					if (DateTime.Compare(value, (DateTime)datatype.ChangeType(facets.MaxExclusive, typeof(DateTime))) >= 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MaxExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) != 0)
				{
					if (DateTime.Compare(value, (DateTime)datatype.ChangeType(facets.MaxInclusive, typeof(DateTime))) > 0)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxInclusive, facets.MaxInclusive, (object)value);
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
			bool flag = this.MatchEnumeration((DateTime)datatype.ChangeType(value, typeof(DateTime)), enumeration, datatype);
			return flag;
		}

		private bool MatchEnumeration(DateTime value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (DateTime correctValue in enumeration)
			{
				if (DateTime.Compare(value, correctValue) == 0)
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