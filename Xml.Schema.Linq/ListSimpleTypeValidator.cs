using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class ListSimpleTypeValidator : SimpleTypeValidator
	{
		private SimpleTypeValidator itemType;

		internal SimpleTypeValidator ItemType
		{
			get
			{
				return this.itemType;
			}
		}

		public ListSimpleTypeValidator(XmlSchemaSimpleType type, Xml.Schema.Linq.RestrictionFacets facets, SimpleTypeValidator itemType) : base(XmlSchemaDatatypeVariety.List, type, Xml.Schema.Linq.FacetsChecker.ListFacetsChecker, facets)
		{
			this.itemType = itemType;
		}

		internal static Exception ToList(object value, ref IList list)
		{
			Exception e = null;
			if (value is IList)
			{
				list = (IList)value;
			}
			else if (!(value is string))
			{
				e = new InvalidCastException();
			}
			else
			{
				string strValue = value as string;
				char[] chrArray = new char[] { ' ' };
				list = new List<object>(strValue.Split(chrArray));
			}
			return e;
		}

		internal static string ToString(object value)
		{
			Debug.Assert(value is IEnumerable);
			IEnumerable list = (IEnumerable)value;
			StringBuilder bldr = new StringBuilder();
			foreach (object o in list)
			{
				if (bldr.Length != 0)
				{
					bldr.Append(' ');
				}
				bldr.Append(o.ToString());
			}
			return bldr.ToString();
		}

		internal override Exception TryParseString(object value, NameTable nameTable, XNamespaceResolver resolver, out string parsedString)
		{
			Exception invalidCastException;
			parsedString = value as string;
			if (parsedString == null)
			{
				IEnumerable list = value as IEnumerable;
				if (list != null)
				{
					StringBuilder bldr = new StringBuilder();
					foreach (object o in list)
					{
						if (bldr.Length != 0)
						{
							bldr.Append(' ');
						}
						string s = null;
						Exception ie = this.itemType.TryParseString(o, nameTable, resolver, out s);
						if (ie != null)
						{
							invalidCastException = ie;
							return invalidCastException;
						}
						else
						{
							bldr.Append(s);
						}
					}
					parsedString = bldr.ToString();
					invalidCastException = null;
				}
				else
				{
					invalidCastException = new InvalidCastException();
				}
			}
			else
			{
				invalidCastException = null;
			}
			return invalidCastException;
		}

		internal override Exception TryParseValue(object value, NameTable nameTable, XNamespaceResolver resolver, out SimpleTypeValidator matchingType, out object typedValue)
		{
			Exception exception;
			Exception e = null;
			typedValue = null;
			matchingType = null;
			if ((base.RestrictionFacets == null ? false : base.RestrictionFacets.HasLexicalFacets))
			{
				string parsedString = null;
				e = this.TryParseString(value, nameTable, resolver, out parsedString);
				if (e == null)
				{
					e = this.facetsChecker.CheckLexicalFacets(ref parsedString, value, nameTable, resolver, this);
				}
			}
			if (e == null)
			{
				e = this.facetsChecker.CheckValueFacets(value, this);
			}
			if (e == null)
			{
				IList listItems = null;
				e = ListSimpleTypeValidator.ToList(value, ref listItems);
				if (e == null)
				{
					foreach (object listItem in listItems)
					{
						object typedItemValue = null;
						SimpleTypeValidator itemMatchingType = null;
						e = this.itemType.TryParseValue(listItem, nameTable, resolver, out itemMatchingType, out typedItemValue);
						if (e != null)
						{
							exception = e;
							return exception;
						}
					}
					typedValue = listItems;
					matchingType = this;
					exception = null;
				}
				else
				{
					exception = e;
				}
			}
			else
			{
				exception = e;
			}
			return exception;
		}
	}
}