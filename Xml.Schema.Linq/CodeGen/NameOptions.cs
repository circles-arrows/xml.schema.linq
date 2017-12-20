using System;

namespace Xml.Schema.Linq.CodeGen
{
	internal enum NameOptions
	{
		None,
		MakeCollection,
		MakeList,
		MakePlural,
		MakeField,
		MakeParam,
		MakeLocal,
		MakeUnion,
		MakeDefaultValueField,
		MakeFixedValueField
	}
}