using System;

namespace Xml.Schema.Linq.CodeGen
{
	[Flags]
	internal enum ClrTypeRefFlags
	{
		None = 0,
		IsValueType = 1,
		IsLocalType = 2,
		IsSimpleType = 4,
		IsAnyType = 8,
		IsElementRef = 16,
		IsUnion = 32,
		IsSchemaList = 64,
		Validate = 128
	}
}