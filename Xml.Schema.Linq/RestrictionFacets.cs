using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Xml.Schema.Linq
{
	public class RestrictionFacets
	{
		internal int Length;

		internal int MinLength;

		internal int MaxLength;

		internal ArrayList Patterns;

		internal ArrayList Enumeration;

		internal XmlSchemaWhiteSpace WhiteSpace;

		internal object MaxInclusive;

		internal object MaxExclusive;

		internal object MinInclusive;

		internal object MinExclusive;

		internal int TotalDigits;

		internal int FractionDigits;

		internal RestrictionFlags Flags = 0;

		private bool hasValueFacets;

		private bool hasLexicalFacets;

		public bool HasLexicalFacets
		{
			get
			{
				return this.hasLexicalFacets;
			}
		}

		public bool HasValueFacets
		{
			get
			{
				return this.hasValueFacets;
			}
		}

		public RestrictionFacets(RestrictionFlags flags, object[] enumeration, int fractionDigits, int length, object maxExclusive, object maxInclusive, int maxLength, object minExclusive, object minInclusive, int minLength, string[] patterns, int totalDigits, XmlSchemaWhiteSpace whiteSpace)
		{
			this.hasValueFacets = false;
			this.hasLexicalFacets = false;
			if ((int)(flags & RestrictionFlags.Enumeration) != 0)
			{
				this.Enumeration = new ArrayList();
				object[] objArray = enumeration;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					object o = objArray[i];
					this.Enumeration.Add(o);
				}
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.FractionDigits) != 0)
			{
				this.FractionDigits = fractionDigits;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.Length) != 0)
			{
				this.Length = length;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MaxExclusive) != 0)
			{
				this.MaxExclusive = maxExclusive;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MaxInclusive) != 0)
			{
				this.MaxInclusive = maxInclusive;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MaxLength) != 0)
			{
				this.MaxLength = maxLength;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MinExclusive) != 0)
			{
				this.MinExclusive = minExclusive;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MinInclusive) != 0)
			{
				this.MinInclusive = minInclusive;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.MinLength) != 0)
			{
				this.MinLength = minLength;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.Pattern) != 0)
			{
				this.CompilePatterns(patterns);
				this.hasLexicalFacets = true;
			}
			if ((int)(flags & RestrictionFlags.TotalDigits) != 0)
			{
				this.TotalDigits = totalDigits;
				this.hasValueFacets = true;
			}
			if ((int)(flags & RestrictionFlags.WhiteSpace) != 0)
			{
				this.hasLexicalFacets = true;
				this.WhiteSpace = whiteSpace;
			}
			this.Flags = flags;
		}

		internal void CompilePatterns(string[] patternStrs)
		{
			if (this.Patterns != null)
			{
				this.Patterns.Clear();
			}
			else
			{
				this.Patterns = new ArrayList();
			}
			string[] strArrays = patternStrs;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				this.Patterns.Add(new Regex(str));
			}
		}
	}
}