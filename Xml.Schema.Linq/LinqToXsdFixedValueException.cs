using System;

namespace Xml.Schema.Linq
{
	public class LinqToXsdFixedValueException : LinqToXsdException
	{
		public LinqToXsdFixedValueException(object value, object fixedValue) : base(LinqToXsdException.CreateMessage("Checking Fixed Value Failed", fixedValue, value))
		{
		}
	}
}