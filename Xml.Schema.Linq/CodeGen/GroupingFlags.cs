using System;

namespace Xml.Schema.Linq.CodeGen
{
	[Flags]
	internal enum GroupingFlags
	{
		None = 0,
		Nested = 1,
		Repeating = 2,
		HasChildGroups = 4,
		HasRepeatingGroups = 8,
		HasRecurrentElements = 16,
		Optional = 32
	}
}