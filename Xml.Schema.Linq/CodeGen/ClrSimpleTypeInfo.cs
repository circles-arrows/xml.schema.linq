using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class ClrSimpleTypeInfo : ClrTypeInfo
	{
		private XmlSchemaType innerType;

		private XmlSchemaDatatypeVariety variety;

		public XmlSchemaDatatype Datatype
		{
			get
			{
				return this.innerType.Datatype;
			}
		}

		internal XmlSchemaType InnerType
		{
			get
			{
				return this.innerType;
			}
			set
			{
				this.innerType = value;
			}
		}

		internal bool IsGlobal
		{
			get
			{
				bool flag;
				XmlSchemaSimpleType st = this.innerType as XmlSchemaSimpleType;
				if (st == null)
				{
					flag = false;
				}
				else
				{
					flag = (st.IsBuiltInSimpleType() ? false : !st.QualifiedName.IsEmpty);
				}
				return flag;
			}
		}

		internal CompiledFacets RestrictionFacets
		{
			get
			{
				return ClrSimpleTypeInfo.GetFacets(this.innerType);
			}
		}

		public XmlTypeCode TypeCode
		{
			get
			{
				return this.innerType.Datatype.TypeCode;
			}
		}

		public XmlSchemaDatatypeVariety Variety
		{
			get
			{
				return this.variety;
			}
		}

		internal ClrSimpleTypeInfo(XmlSchemaType innerType)
		{
			this.innerType = innerType;
			this.variety = innerType.Datatype.Variety;
		}

		internal static ClrSimpleTypeInfo CreateSimpleTypeInfo(XmlSchemaType type)
		{
			ClrSimpleTypeInfo typeInfo = null;
			Debug.Assert(type.Datatype != null);
			switch (type.Datatype.Variety)
			{
				case XmlSchemaDatatypeVariety.Atomic:
				{
					typeInfo = new AtomicSimpleTypeInfo(type);
					break;
				}
				case XmlSchemaDatatypeVariety.List:
				{
					typeInfo = new ListSimpleTypeInfo(type);
					break;
				}
				case XmlSchemaDatatypeVariety.Union:
				{
					typeInfo = new UnionSimpleTypeInfo(type);
					break;
				}
			}
			return typeInfo;
		}

		private static CompiledFacets GetFacets(XmlSchemaType type)
		{
			CompiledFacets compiledFacets = new CompiledFacets(type.Datatype);
			XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
			if (simpleType != null)
			{
				compiledFacets.compileFacets(simpleType);
			}
			return compiledFacets;
		}

		internal void UpdateClrTypeName(Dictionary<XmlSchemaObject, string> nameMappings, LinqToXsdSettings settings)
		{
			string identifier = null;
			string typeName = this.innerType.QualifiedName.Name;
			string clrNameSpace = settings.GetClrNamespace(this.innerType.QualifiedName.Namespace);
			if (!nameMappings.TryGetValue(this.innerType, out identifier))
			{
				this.clrtypeName = typeName;
			}
			else
			{
				this.clrtypeName = identifier;
			}
			if (clrNameSpace != string.Empty)
			{
				this.clrtypeName = string.Concat(clrNameSpace, ".", this.clrtypeName);
			}
		}
	}
}