using System;

namespace Xml.Schema.Linq.CodeGen
{
	[Flags]
	internal enum PropertyFlags
	{
		None = 0,
		FromBaseType = 1,
		IsDuplicate = 2,
		HasFixedValue = 4,
		HasDefaultValue = 8,
		IsNew = 16,
		IsList = 32,
		IsNullable = 64,
		VerifyRequired = 128
	}
}