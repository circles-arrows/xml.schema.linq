using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq.CodeGen;

namespace Xml.Schema.Linq
{
	internal class FacetsChecker
	{
		internal static Xml.Schema.Linq.Numeric2FacetsChecker numeric2FacetsChecker;

		internal static Xml.Schema.Linq.DurationFacetsChecker durationFacetsChecker;

		internal static Xml.Schema.Linq.DateTimeFacetsChecker dateTimeFacetsChecker;

		internal static Xml.Schema.Linq.StringFacetsChecker stringFacetsChecker;

		internal static Xml.Schema.Linq.QNameFacetsChecker qNameFacetsChecker;

		internal static Xml.Schema.Linq.MiscFacetsChecker miscFacetsChecker;

		internal static Xml.Schema.Linq.BinaryFacetsChecker binaryFacetsChecker;

		internal static Xml.Schema.Linq.ListFacetsChecker listFacetsChecker;

		internal static Xml.Schema.Linq.UnionFacetsChecker unionFacetsChecker;

		private static Dictionary<XmlTypeCode, Xml.Schema.Linq.FacetsChecker> FacetsCheckerMapping;

		internal static Xml.Schema.Linq.FacetsChecker ListFacetsChecker
		{
			get
			{
				return Xml.Schema.Linq.FacetsChecker.listFacetsChecker;
			}
		}

		internal static Xml.Schema.Linq.FacetsChecker UnionFacetsChecker
		{
			get
			{
				return Xml.Schema.Linq.FacetsChecker.unionFacetsChecker;
			}
		}

		static FacetsChecker()
		{
			Xml.Schema.Linq.FacetsChecker.numeric2FacetsChecker = new Xml.Schema.Linq.Numeric2FacetsChecker();
			Xml.Schema.Linq.FacetsChecker.durationFacetsChecker = new Xml.Schema.Linq.DurationFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker = new Xml.Schema.Linq.DateTimeFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.stringFacetsChecker = new Xml.Schema.Linq.StringFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.qNameFacetsChecker = new Xml.Schema.Linq.QNameFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.miscFacetsChecker = new Xml.Schema.Linq.MiscFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.binaryFacetsChecker = new Xml.Schema.Linq.BinaryFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.listFacetsChecker = new Xml.Schema.Linq.ListFacetsChecker();
			Xml.Schema.Linq.FacetsChecker.unionFacetsChecker = new Xml.Schema.Linq.UnionFacetsChecker();
		}

		public FacetsChecker()
		{
		}

		internal virtual Exception CheckLexicalFacets(ref string parsedString, object value, NameTable nameTable, XNamespaceResolver resolver, SimpleTypeValidator type)
		{
			Exception exception;
			Xml.Schema.Linq.RestrictionFacets facets = type.RestrictionFacets;
			if ((facets == null ? false : facets.HasLexicalFacets))
			{
				Xml.Schema.Linq.RestrictionFlags flags = facets.Flags;
				Xml.Schema.Linq.XmlSchemaWhiteSpace wsPattern = Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse;
				if ((int)(flags & Xml.Schema.Linq.RestrictionFlags.WhiteSpace) != 0)
				{
					if (facets.WhiteSpace == Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse)
					{
						wsPattern = Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse;
					}
					else if (facets.WhiteSpace == Xml.Schema.Linq.XmlSchemaWhiteSpace.Preserve)
					{
						wsPattern = Xml.Schema.Linq.XmlSchemaWhiteSpace.Preserve;
					}
				}
				exception = this.CheckLexicalFacets(ref parsedString, type, facets.Patterns, wsPattern);
			}
			else
			{
				exception = null;
			}
			return exception;
		}

		internal virtual Exception CheckLexicalFacets(ref string parsedString, SimpleTypeValidator type, ArrayList patterns, Xml.Schema.Linq.XmlSchemaWhiteSpace wsPattern)
		{
			this.CheckWhitespaceFacets(ref parsedString, type, wsPattern);
			return this.CheckPatternFacets(patterns, parsedString);
		}

		internal Exception CheckPatternFacets(ArrayList patterns, string value)
		{
			Exception linqToXsdFacetException;
			if (patterns != null)
			{
				foreach (object pattern in patterns)
				{
					Regex regex = pattern as Regex;
					Debug.Assert(regex != null);
					if (regex.IsMatch(value))
					{
						linqToXsdFacetException = null;
						return linqToXsdFacetException;
					}
				}
				linqToXsdFacetException = new LinqToXsdFacetException(Xml.Schema.Linq.RestrictionFlags.Pattern, patterns, value);
			}
			else
			{
				linqToXsdFacetException = null;
			}
			return linqToXsdFacetException;
		}

		internal Exception CheckTotalAndFractionDigits(decimal value, int totalDigits, int fractionDigits, bool checkTotal, bool checkFraction)
		{
			Exception linqToXsdException;
			decimal maxValue = Xml.Schema.Linq.FacetsChecker.Power(10, totalDigits) - 1;
			int powerCnt = 0;
			if (value < new decimal(0))
			{
				value = decimal.Negate(value);
			}
			while (decimal.Truncate(value) != value)
			{
				value *= new decimal(10);
				powerCnt++;
			}
			if (checkTotal & (value > maxValue ? true : powerCnt > totalDigits))
			{
				linqToXsdException = new LinqToXsdException();
			}
			else if (!(checkFraction & powerCnt > fractionDigits))
			{
				linqToXsdException = null;
			}
			else
			{
				linqToXsdException = new LinqToXsdException();
			}
			return linqToXsdException;
		}

		internal virtual Exception CheckValueFacets(object value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(decimal value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(long value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(int value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(short value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(byte value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(DateTime value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(double value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(float value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(string value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(byte[] value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(TimeSpan value, SimpleTypeValidator type)
		{
			return null;
		}

		internal virtual Exception CheckValueFacets(XmlQualifiedName value, SimpleTypeValidator type)
		{
			return null;
		}

		internal void CheckWhitespaceFacets(ref string s, SimpleTypeValidator type, Xml.Schema.Linq.XmlSchemaWhiteSpace wsPattern)
		{
			Xml.Schema.Linq.RestrictionFacets restriction = type.RestrictionFacets;
			if (type.Variety == XmlSchemaDatatypeVariety.List)
			{
				s = s.Trim();
			}
			else if (type.Variety == XmlSchemaDatatypeVariety.Atomic)
			{
				XmlSchemaDatatype datatype = type.DataType;
				if (datatype.GetBuiltInWSFacet() == Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse)
				{
					s = Xml.Schema.Linq.XmlComplianceUtil.NonCDataNormalize(s);
				}
				else if (datatype.GetBuiltInWSFacet() == Xml.Schema.Linq.XmlSchemaWhiteSpace.Replace)
				{
					s = Xml.Schema.Linq.XmlComplianceUtil.CDataNormalize(s);
				}
				else if (restriction != null & (int)(restriction.Flags & Xml.Schema.Linq.RestrictionFlags.WhiteSpace) != 0)
				{
					if (restriction.WhiteSpace == Xml.Schema.Linq.XmlSchemaWhiteSpace.Replace)
					{
						s = Xml.Schema.Linq.XmlComplianceUtil.CDataNormalize(s);
					}
					else if (restriction.WhiteSpace == Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse)
					{
						s = Xml.Schema.Linq.XmlComplianceUtil.NonCDataNormalize(s);
					}
				}
			}
		}

		internal static Xml.Schema.Linq.FacetsChecker GetFacetsChecker(XmlTypeCode typeCode)
		{
			if (Xml.Schema.Linq.FacetsChecker.FacetsCheckerMapping == null)
			{
				Xml.Schema.Linq.FacetsChecker.InitMapping();
			}
			return Xml.Schema.Linq.FacetsChecker.FacetsCheckerMapping[typeCode];
		}

		private static void InitMapping()
		{
			Xml.Schema.Linq.FacetsChecker.FacetsCheckerMapping = new Dictionary<XmlTypeCode, Xml.Schema.Linq.FacetsChecker>()
			{
				{ XmlTypeCode.AnyAtomicType, Xml.Schema.Linq.FacetsChecker.miscFacetsChecker },
				{ XmlTypeCode.AnyUri, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Base64Binary, Xml.Schema.Linq.FacetsChecker.binaryFacetsChecker },
				{ XmlTypeCode.Boolean, Xml.Schema.Linq.FacetsChecker.miscFacetsChecker },
				{ XmlTypeCode.Byte, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-128), new decimal(127)) },
				{ XmlTypeCode.Date, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.DateTime, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.DayTimeDuration, Xml.Schema.Linq.FacetsChecker.durationFacetsChecker },
				{ XmlTypeCode.Decimal, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-1, -1, -1, true, 0), new decimal(-1, -1, -1, false, 0)) },
				{ XmlTypeCode.Double, Xml.Schema.Linq.FacetsChecker.numeric2FacetsChecker },
				{ XmlTypeCode.Duration, Xml.Schema.Linq.FacetsChecker.durationFacetsChecker },
				{ XmlTypeCode.Entity, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Float, Xml.Schema.Linq.FacetsChecker.numeric2FacetsChecker },
				{ XmlTypeCode.GDay, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.GMonth, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.GMonthDay, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.GYear, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.GYearMonth, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.HexBinary, Xml.Schema.Linq.FacetsChecker.binaryFacetsChecker },
				{ XmlTypeCode.Id, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Idref, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Int, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-2147483648), new decimal(2147483647)) },
				{ XmlTypeCode.Integer, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-1, -1, -1, true, 0), new decimal(-1, -1, -1, false, 0)) },
				{ XmlTypeCode.Item, Xml.Schema.Linq.FacetsChecker.miscFacetsChecker },
				{ XmlTypeCode.Language, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Long, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-9223372036854775808L), new decimal(9223372036854775807L)) },
				{ XmlTypeCode.Name, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.NCName, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.NegativeInteger, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-1, -1, -1, true, 0), new decimal(-1)) },
				{ XmlTypeCode.NmToken, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.NonNegativeInteger, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(0), new decimal(-1, -1, -1, false, 0)) },
				{ XmlTypeCode.NonPositiveInteger, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-1, -1, -1, true, 0), new decimal(0)) },
				{ XmlTypeCode.NormalizedString, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Notation, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.PositiveInteger, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(1), new decimal(-1, -1, -1, false, 0)) },
				{ XmlTypeCode.QName, Xml.Schema.Linq.FacetsChecker.qNameFacetsChecker },
				{ XmlTypeCode.Short, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(-32768), new decimal(32767)) },
				{ XmlTypeCode.String, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.Time, Xml.Schema.Linq.FacetsChecker.dateTimeFacetsChecker },
				{ XmlTypeCode.Token, Xml.Schema.Linq.FacetsChecker.stringFacetsChecker },
				{ XmlTypeCode.UnsignedByte, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(0), new decimal(255)) },
				{ XmlTypeCode.UnsignedInt, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(0), new decimal((long)-1)) },
				{ XmlTypeCode.UnsignedLong, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(0), new decimal(-1, -1, 0, false, 0)) },
				{ XmlTypeCode.UnsignedShort, new Xml.Schema.Linq.Numeric10FacetsChecker(new decimal(0), new decimal(65535)) },
				{ XmlTypeCode.UntypedAtomic, Xml.Schema.Linq.FacetsChecker.miscFacetsChecker },
				{ XmlTypeCode.YearMonthDuration, Xml.Schema.Linq.FacetsChecker.durationFacetsChecker }
			};
		}

		internal virtual bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
		{
			return false;
		}

		internal static decimal Power(int x, int y)
		{
			decimal num;
			decimal returnValue = new decimal(1);
			decimal decimalValue = x;
			if (y <= 28)
			{
				for (int i = 0; i < y; i++)
				{
					returnValue *= decimalValue;
				}
				num = returnValue;
			}
			else
			{
				num = new decimal(-1, -1, -1, false, 0);
			}
			return num;
		}
	}
}