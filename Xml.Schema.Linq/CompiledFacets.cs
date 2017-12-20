using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq.CodeGen;

namespace Xml.Schema.Linq
{
	internal class CompiledFacets
	{
		private Xml.Schema.Linq.RestrictionFlags flags;

		private int length;

		private int minLength;

		private int maxLength;

		private object maxInclusive;

		private object maxExclusive;

		private object minInclusive;

		private object minExclusive;

		private int totalDigits;

		private int fractionDigits;

		private ArrayList patterns;

		private ArrayList enumerations;

		private Xml.Schema.Linq.XmlSchemaWhiteSpace whiteSpace;

		public ArrayList Enumeration
		{
			get
			{
				return this.enumerations;
			}
		}

		public Xml.Schema.Linq.RestrictionFlags Flags
		{
			get
			{
				return this.flags;
			}
		}

		public int FractionDigits
		{
			get
			{
				return this.fractionDigits;
			}
		}

		public int Length
		{
			get
			{
				return this.length;
			}
		}

		public object MaxExclusive
		{
			get
			{
				return this.maxExclusive;
			}
		}

		public object MaxInclusive
		{
			get
			{
				return this.maxInclusive;
			}
		}

		public int MaxLength
		{
			get
			{
				return this.maxLength;
			}
		}

		public object MinExclusive
		{
			get
			{
				return this.minExclusive;
			}
		}

		public object MinInclusive
		{
			get
			{
				return this.minInclusive;
			}
		}

		public int MinLength
		{
			get
			{
				return this.minLength;
			}
		}

		public ArrayList Patterns
		{
			get
			{
				return this.patterns;
			}
		}

		public int TotalDigits
		{
			get
			{
				return this.totalDigits;
			}
		}

		public Xml.Schema.Linq.XmlSchemaWhiteSpace WhiteSpace
		{
			get
			{
				return this.whiteSpace;
			}
		}

		public CompiledFacets(XmlSchemaDatatype dt)
		{
			this.whiteSpace = dt.GetBuiltInWSFacet();
		}

		public void compileFacets(XmlSchemaSimpleType simpleType)
		{
			XmlSchemaSimpleType type = simpleType;
			XmlSchemaSimpleType enumSimpleType = null;
			this.flags = (Xml.Schema.Linq.RestrictionFlags)0;
			while (true)
			{
				if ((type == null ? true : string.Equals(type.QualifiedName.Namespace, "http://www.w3.org/2001/XMLSchema", StringComparison.Ordinal)))
				{
					break;
				}
				XmlSchemaSimpleTypeRestriction simpleTypeRestriction = type.Content as XmlSchemaSimpleTypeRestriction;
				if (simpleTypeRestriction != null)
				{
					foreach (XmlSchemaFacet facet in simpleTypeRestriction.Facets)
					{
						if (facet is XmlSchemaMinLengthFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MinLength) == 0)
							{
								this.minLength = XmlConvert.ToInt32(facet.Value);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MinLength;
							}
						}
						else if (facet is XmlSchemaMaxLengthFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MaxLength) == 0)
							{
								this.maxLength = XmlConvert.ToInt32(facet.Value);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MaxLength;
							}
						}
						else if (facet is XmlSchemaLengthFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.Length) == 0)
							{
								this.length = XmlConvert.ToInt32(facet.Value);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.Length;
							}
						}
						else if (facet is XmlSchemaEnumerationFacet)
						{
							if (enumSimpleType == null)
							{
								this.enumerations = new ArrayList();
								this.flags |= Xml.Schema.Linq.RestrictionFlags.Enumeration;
								enumSimpleType = type;
							}
							else if (enumSimpleType != type)
							{
								continue;
							}
							this.enumerations.Add(type.BaseXmlSchemaType.Datatype.ParseValue(facet.Value, null, null));
						}
						else if (facet is XmlSchemaPatternFacet)
						{
							if (this.patterns == null)
							{
								this.patterns = new ArrayList();
								this.flags |= Xml.Schema.Linq.RestrictionFlags.Pattern;
							}
							this.patterns.Add(facet.Value);
						}
						else if (facet is XmlSchemaMaxInclusiveFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MaxInclusive) == 0)
							{
								this.maxInclusive = type.BaseXmlSchemaType.Datatype.ParseValue(facet.Value, null, null);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MaxInclusive;
							}
						}
						else if (facet is XmlSchemaMaxExclusiveFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MaxExclusive) == 0)
							{
								this.maxExclusive = type.BaseXmlSchemaType.Datatype.ParseValue(facet.Value, null, null);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MaxExclusive;
							}
						}
						else if (facet is XmlSchemaMinExclusiveFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MinExclusive) == 0)
							{
								this.minExclusive = type.BaseXmlSchemaType.Datatype.ParseValue(facet.Value, null, null);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MinExclusive;
							}
						}
						else if (facet is XmlSchemaMinInclusiveFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.MinInclusive) == 0)
							{
								this.minInclusive = type.BaseXmlSchemaType.Datatype.ParseValue(facet.Value, null, null);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.MinInclusive;
							}
						}
						else if (facet is XmlSchemaFractionDigitsFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.FractionDigits) == 0)
							{
								this.fractionDigits = XmlConvert.ToInt32(facet.Value);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.FractionDigits;
							}
						}
						else if (facet is XmlSchemaTotalDigitsFacet)
						{
							if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.TotalDigits) == 0)
							{
								this.totalDigits = XmlConvert.ToInt32(facet.Value);
								this.flags |= Xml.Schema.Linq.RestrictionFlags.TotalDigits;
							}
						}
						else if (!(facet is XmlSchemaWhiteSpaceFacet))
						{
							continue;
						}
						else if ((int)(this.flags & Xml.Schema.Linq.RestrictionFlags.WhiteSpace) == 0)
						{
							if (facet.Value == "preserve")
							{
								this.whiteSpace = Xml.Schema.Linq.XmlSchemaWhiteSpace.Preserve;
							}
							else if (facet.Value == "replace")
							{
								this.whiteSpace = Xml.Schema.Linq.XmlSchemaWhiteSpace.Replace;
							}
							else if (facet.Value == "collapse")
							{
								this.whiteSpace = Xml.Schema.Linq.XmlSchemaWhiteSpace.Collapse;
							}
							this.flags |= Xml.Schema.Linq.RestrictionFlags.WhiteSpace;
						}
					}
				}
				type = type.BaseXmlSchemaType as XmlSchemaSimpleType;
			}
		}
	}
}