using System;
using System.Diagnostics;
using System.Xml.Schema;

namespace Xml.Schema.Linq.CodeGen
{
	internal class UnionSimpleTypeInfo : ClrSimpleTypeInfo
	{
		private ClrSimpleTypeInfo[] memberTypes;

		public ClrSimpleTypeInfo[] MemberTypes
		{
			get
			{
				if (this.memberTypes == null)
				{
					XmlSchemaSimpleType st = base.InnerType as XmlSchemaSimpleType;
					if (st == null)
					{
						st = (base.InnerType as XmlSchemaComplexType).GetBaseSimpleType();
					}
					Debug.Assert(st.Datatype.Variety == XmlSchemaDatatypeVariety.Union);
					XmlSchemaSimpleType[] innerMemberTypes = st.GetUnionMemberTypes();
					this.memberTypes = new ClrSimpleTypeInfo[(int)innerMemberTypes.Length];
					for (int i = 0; i < (int)innerMemberTypes.Length; i++)
					{
						this.memberTypes[i] = ClrSimpleTypeInfo.CreateSimpleTypeInfo(innerMemberTypes[i]);
					}
				}
				return this.memberTypes;
			}
		}

		internal UnionSimpleTypeInfo(XmlSchemaType innerType) : base(innerType)
		{
		}
	}
}