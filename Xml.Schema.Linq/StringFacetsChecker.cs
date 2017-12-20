using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class StringFacetsChecker : Xml.Schema.Linq.FacetsChecker
	{
		private static Regex languagePattern;

		private static Regex LanguagePattern
		{
			get
			{
				if (Xml.Schema.Linq.StringFacetsChecker.languagePattern == null)
				{
					Regex langRegex = new Regex("^([a-zA-Z]{1,8})(-[a-zA-Z0-9]{1,8})*$", RegexOptions.None);
					Interlocked.CompareExchange<Regex>(ref Xml.Schema.Linq.StringFacetsChecker.languagePattern, langRegex, null);
				}
				return Xml.Schema.Linq.StringFacetsChecker.languagePattern;
			}
		}

		public StringFacetsChecker()
		{
		}

		private Exception CheckBuiltInFacets(string s, XmlTypeCode typeCode, bool verifyUri)
		{
			Exception exception = null;
			switch (typeCode)
			{
				case XmlTypeCode.AnyUri:
				{
					if (verifyUri)
					{
						Uri uri = null;
						exception = XmlConvertExt.TryToUri(s, out uri);
					}
					break;
				}
				case XmlTypeCode.QName:
				case XmlTypeCode.Notation:
				{
					break;
				}
				case XmlTypeCode.NormalizedString:
				{
					exception = XmlConvertExt.VerifyNormalizedString(s);
					break;
				}
				case XmlTypeCode.Token:
				{
					try
					{
						XmlConvert.VerifyTOKEN(s);
					}
					catch (Exception exception1)
					{
						exception = exception1;
					}
					break;
				}
				case XmlTypeCode.Language:
				{
					if ((s == null ? true : s.Length == 0))
					{
						exception = new LinqToXsdException();
					}
					if (!Xml.Schema.Linq.StringFacetsChecker.LanguagePattern.IsMatch(s))
					{
						exception = new LinqToXsdException();
					}
					break;
				}
				case XmlTypeCode.NmToken:
				{
					try
					{
						XmlConvert.VerifyNMTOKEN(s);
					}
					catch (Exception exception2)
					{
						exception = exception2;
					}
					break;
				}
				case XmlTypeCode.Name:
				{
					try
					{
						XmlConvert.VerifyName(s);
					}
					catch (Exception exception3)
					{
						exception = exception3;
					}
					break;
				}
				case XmlTypeCode.NCName:
				case XmlTypeCode.Id:
				case XmlTypeCode.Idref:
				case XmlTypeCode.Entity:
				{
					try
					{
						XmlConvert.VerifyNCName(s);
					}
					catch (Exception exception4)
					{
						exception = exception4;
					}
					break;
				}
				default:
				{
					goto case XmlTypeCode.Notation;
				}
			}
			return exception;
		}

		internal override Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			Exception exception;
			if ((type.RestrictionFacets == null ? false : type.RestrictionFacets.HasValueFacets))
			{
				XmlSchemaDatatype datatype = type.DataType;
				string stringValue = null;
				stringValue = (type.DataType.TypeCode != XmlTypeCode.AnyUri ? (string)datatype.ChangeType(value, XTypedServices.typeOfString) : ((Uri)datatype.ChangeType(value, typeof(Uri))).OriginalString);
				exception = this.CheckValueFacets(stringValue, type);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal override Exception CheckValueFacets(string value, SimpleTypeValidator type)
		{
			return this.CheckValueFacets(value, type, true);
		}

		internal Exception CheckValueFacets(string value, SimpleTypeValidator type, bool verifyUri)
		{
			Exception linqToXsdFacetException;
			int length = value.Length;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if (facets != null)
			{
				Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
				XmlSchemaDatatype datatype = type.DataType;
				Exception exception = this.CheckBuiltInFacets(value, datatype.TypeCode, verifyUri);
				if (exception == null)
				{
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
					linqToXsdFacetException = exception;
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
			bool flag = this.MatchEnumeration((string)datatype.ChangeType(value, XTypedServices.typeOfString), enumeration, datatype);
			return flag;
		}

		private bool MatchEnumeration(string value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			bool flag;
			if (datatype.TypeCode != XmlTypeCode.AnyUri)
			{
				foreach (string correctValue in enumeration)
				{
					if (value.Equals(correctValue))
					{
						flag = true;
						return flag;
					}
				}
			}
			else
			{
				foreach (Uri correctValue in enumeration)
				{
					if (value.Equals(correctValue.OriginalString))
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}
	}
}