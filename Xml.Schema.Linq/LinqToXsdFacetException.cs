using System;

namespace Xml.Schema.Linq
{
	public class LinqToXsdFacetException : LinqToXsdException
	{
		public LinqToXsdFacetException(RestrictionFlags flags, object facetValue, object value) : base(LinqToXsdException.CreateMessage(flags.ToString(), facetValue, value))
		{
		}
	}
}