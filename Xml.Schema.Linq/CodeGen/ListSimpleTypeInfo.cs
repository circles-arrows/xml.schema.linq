using System;
using System.Diagnostics;
using System.Xml.Schema;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ListSimpleTypeInfo : ClrSimpleTypeInfo
	{
		private ClrSimpleTypeInfo itemType;

		public ClrSimpleTypeInfo ItemType
		{
			get
			{
				if (this.itemType == null)
				{
					XmlSchemaSimpleType st = base.InnerType as XmlSchemaSimpleType;
					if (st == null)
					{
						st = (base.InnerType as XmlSchemaComplexType).GetBaseSimpleType();
					}
					Debug.Assert(st.Datatype.Variety == XmlSchemaDatatypeVariety.List);
					this.itemType = ClrSimpleTypeInfo.CreateSimpleTypeInfo(st.GetListItemType());
				}
				return this.itemType;
			}
		}

		internal ListSimpleTypeInfo(XmlSchemaType innerType) : base(innerType)
		{
		}
	}
}