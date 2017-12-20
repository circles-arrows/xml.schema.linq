using System;
using System.Xml.Schema;

namespace Xml.Schema.Linq.CodeGen
{
	internal class AtomicSimpleTypeInfo : ClrSimpleTypeInfo
	{
		internal AtomicSimpleTypeInfo(XmlSchemaType innerType) : base(innerType)
		{
		}
	}
}