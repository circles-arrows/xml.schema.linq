using System;

namespace Xml.Schema.Linq
{
	[Flags]
	public enum RestrictionFlags
	{
		Length = 1,
		MinLength = 2,
		MaxLength = 4,
		Pattern = 8,
		Enumeration = 16,
		WhiteSpace = 32,
		MaxInclusive = 64,
		MaxExclusive = 128,
		MinInclusive = 256,
		MinExclusive = 512,
		TotalDigits = 1024,
		FractionDigits = 2048
	}
}