using System;
using System.Collections;
using System.Text;

namespace Xml.Schema.Linq
{
	public class LinqToXsdException : Exception
	{
		public LinqToXsdException(string errorMsg) : base(errorMsg)
		{
		}

		public LinqToXsdException()
		{
		}

		public LinqToXsdException(string propertyName, string reason) : base(string.Concat("Failed to set value on the property \"", propertyName, "\". Possible reason: ", reason))
		{
		}

		protected static string ConvertIEnumToString(IEnumerable value)
		{
			StringBuilder strBuilder = new StringBuilder();
			foreach (object o in value)
			{
				if (strBuilder.Length != 0)
				{
					strBuilder.Append(' ');
				}
				strBuilder.Append((!(o is IEnumerable) || o is string ? o.ToString() : LinqToXsdException.ConvertIEnumToString(o as IEnumerable)));
			}
			strBuilder.Insert(0, '(');
			strBuilder.Append(')');
			return strBuilder.ToString();
		}

		protected static string CreateMessage(string facetName, string facetValue, string value)
		{
			string[] strArrays = new string[] { "The Given Value ", value, " Violates Restrictions: ", facetName, " = ", facetValue };
			return string.Concat(strArrays);
		}

		protected static string CreateMessage(string facetName, object facetValue, object value)
		{
			return LinqToXsdException.CreateMessage(facetName, (!(facetValue is IEnumerable) || facetValue is string ? facetValue.ToString() : LinqToXsdException.ConvertIEnumToString(facetValue as IEnumerable)), (!(value is IEnumerable) || facetValue is string ? value.ToString() : LinqToXsdException.ConvertIEnumToString(value as IEnumerable)));
		}
	}
}