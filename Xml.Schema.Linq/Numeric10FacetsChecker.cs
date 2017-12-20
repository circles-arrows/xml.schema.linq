using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class Numeric10FacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		private decimal maxValue;

		private decimal minValue;

		internal Numeric10FacetsChecker(decimal minVal, decimal maxVal)
		{
			this.minValue = minVal;
			this.maxValue = maxVal;
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				decimal decimalValue = (decimal)type.DataType.ChangeType(value, typeof(decimal));
				exception = this.CheckValueFacets(decimalValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(decimal value, SimpleTypeValidator type)
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
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.FractionDigits) != 0)
				{
					if (base.CheckTotalAndFractionDigits(value, 29, facets.FractionDigits, false, true) != null)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.FractionDigits, (object)facets.FractionDigits, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) != 0)
				{
					if (value >= (decimal)datatype.ChangeType(facets.MaxExclusive, typeof(decimal)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxExclusive, facets.MaxExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) != 0)
				{
					if (value > (decimal)datatype.ChangeType(facets.MaxInclusive, typeof(decimal)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MaxInclusive, facets.MaxInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) != 0)
				{
					if (value <= (decimal)datatype.ChangeType(facets.MinExclusive, typeof(decimal)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinExclusive, facets.MinExclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) != 0)
				{
					if (value < (decimal)datatype.ChangeType(facets.MinInclusive, typeof(decimal)))
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.MinInclusive, facets.MinInclusive, (object)value);
						return linqToXsdFacetException;
					}
				}
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.TotalDigits) != 0)
				{
					if (base.CheckTotalAndFractionDigits(value, Convert.ToInt32(facets.TotalDigits), 0, true, false) != null)
					{
						linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.TotalDigits, (object)facets.TotalDigits, (object)value);
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
			bool flag = this.MatchEnumeration((decimal)datatype.ChangeType(value, typeof(decimal)), enumeration, datatype);
			return flag;
		}

		internal bool MatchEnumeration(decimal value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (object correctValue in enumeration)
			{
				if (value == (decimal)datatype.ChangeType(correctValue, typeof(decimal)))
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