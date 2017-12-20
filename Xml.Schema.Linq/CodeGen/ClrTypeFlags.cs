using System;

namespace Xml.Schema.Linq.CodeGen
{
	[Flags]
	internal enum ClrTypeFlags
	{
		None = 0,
		IsAbstract = 1,
		IsSealed = 2,
		IsRoot = 4,
		IsNested = 8,
		InlineBaseType = 16,
		IsSubstitutionHead = 32,
		HasFixedValue = 64,
		HasDefaultValue = 128,
		HasElementWildCard = 256
	}
}