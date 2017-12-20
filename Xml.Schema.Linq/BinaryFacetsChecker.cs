using System;
using System.Collections;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class BinaryFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		public BinaryFacetsChecker()
		{
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				exception = this.CheckValueFacets((byte[])value, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(byte[] value, SimpleTypeValidator type)
		{
			Exception linqToXsdFacetException;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if ((facets == null ? false : facets.HasValueFacets))
			{
				int length = (int)value.Length;
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

		private bool IsEqual(byte[] b1, byte[] b2)
		{
			bool flag;
			if ((int)b1.Length == (int)b2.Length)
			{
				int i = 0;
				while (i < (int)b1.Length)
				{
					if (b1[i] == b2[i])
					{
						i++;
					}
					else
					{
						flag = false;
						return flag;
					}
				}
				flag = true;
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			return this.MatchEnumeration((byte[])value, enumeration, datatype);
		}

		private bool MatchEnumeration(byte[] value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			foreach (byte[] correctValue in enumeration)
			{
				if (this.IsEqual(value, correctValue))
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