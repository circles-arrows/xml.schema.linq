using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrTypeReference
	{
		private string typeName;

		private string typeNs;

		private string typeCodeString;

		private XmlSchemaObject schemaObject;

		private ClrTypeRefFlags typeRefFlags;

		private SchemaOrigin typeRefOrigin;

		internal bool IsAnyType
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsAnyType) != ClrTypeRefFlags.None;
			}
		}

		internal bool IsLocalType
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsLocalType) != ClrTypeRefFlags.None;
			}
		}

		internal bool IsNamedComplexType
		{
			get
			{
				return ((this.typeRefFlags & ClrTypeRefFlags.IsSimpleType) != ClrTypeRefFlags.None ? false : (this.typeRefFlags & ClrTypeRefFlags.IsAnyType) == ClrTypeRefFlags.None);
			}
		}

		internal bool IsSchemaList
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsSchemaList) != ClrTypeRefFlags.None;
			}
		}

		internal bool IsSimpleType
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsSimpleType) != ClrTypeRefFlags.None;
			}
		}

		internal bool IsTypeRef
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsElementRef) != ClrTypeRefFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeReference clrTypeReference = this;
					clrTypeReference.typeRefFlags = clrTypeReference.typeRefFlags & (ClrTypeRefFlags.IsValueType | ClrTypeRefFlags.IsLocalType | ClrTypeRefFlags.IsSimpleType | ClrTypeRefFlags.IsAnyType | ClrTypeRefFlags.IsUnion | ClrTypeRefFlags.IsSchemaList | ClrTypeRefFlags.Validate);
				}
				else
				{
					this.typeRefFlags |= ClrTypeRefFlags.IsElementRef;
				}
			}
		}

		internal bool IsUnion
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsUnion) != ClrTypeRefFlags.None;
			}
		}

		internal bool IsValueType
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.IsValueType) != ClrTypeRefFlags.None;
			}
		}

		internal string Name
		{
			get
			{
				return this.typeName;
			}
			set
			{
				this.typeName = value;
			}
		}

		internal string Namespace
		{
			get
			{
				return this.typeNs;
			}
		}

		internal SchemaOrigin Origin
		{
			get
			{
				return this.typeRefOrigin;
			}
			set
			{
				this.typeRefOrigin = value;
			}
		}

		internal XmlSchemaObject SchemaObject
		{
			get
			{
				return this.schemaObject;
			}
		}

		internal string TypeCodeString
		{
			get
			{
				return this.typeCodeString;
			}
		}

		internal bool Validate
		{
			get
			{
				return (this.typeRefFlags & ClrTypeRefFlags.Validate) != ClrTypeRefFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeReference clrTypeReference = this;
					clrTypeReference.typeRefFlags = clrTypeReference.typeRefFlags & (ClrTypeRefFlags.IsValueType | ClrTypeRefFlags.IsLocalType | ClrTypeRefFlags.IsSimpleType | ClrTypeRefFlags.IsAnyType | ClrTypeRefFlags.IsElementRef | ClrTypeRefFlags.IsUnion | ClrTypeRefFlags.IsSchemaList);
				}
				else
				{
					this.typeRefFlags |= ClrTypeRefFlags.Validate;
				}
			}
		}

		internal ClrTypeReference(string name, string ns, XmlSchemaObject schemaObject, bool anonymousType, bool setVariety)
		{
			this.typeName = name;
			this.typeNs = ns;
			this.schemaObject = schemaObject;
			XmlSchemaType schemaType = schemaObject as XmlSchemaType;
			if (schemaType == null)
			{
				XmlSchemaElement elem = schemaObject as XmlSchemaElement;
				this.typeRefFlags |= ClrTypeRefFlags.IsElementRef;
				schemaType = elem.ElementSchemaType;
			}
			Debug.Assert(schemaType != null);
			XmlSchemaSimpleType st = schemaType as XmlSchemaSimpleType;
			if (st != null)
			{
				if ((st.HasFacetRestrictions() ? true : st.IsOrHasUnion()))
				{
					this.typeRefFlags |= ClrTypeRefFlags.Validate;
				}
				XmlSchemaDatatype datatype = st.Datatype;
				if (setVariety)
				{
					this.SetVariety(datatype);
				}
				this.typeRefFlags |= ClrTypeRefFlags.IsSimpleType;
				this.typeCodeString = datatype.TypeCodeString();
				if (datatype.ValueType.IsValueType)
				{
					this.typeRefFlags |= ClrTypeRefFlags.IsValueType;
				}
			}
			else if (schemaType.TypeCode == XmlTypeCode.Item)
			{
				this.typeRefFlags |= ClrTypeRefFlags.IsAnyType;
			}
			if (anonymousType)
			{
				this.typeRefFlags |= ClrTypeRefFlags.IsLocalType;
			}
			this.typeRefOrigin = SchemaOrigin.Fragment;
		}

		internal string GetClrFullTypeName(string parentTypeClrNs, Dictionary<XmlSchemaObject, string> nameMappings, out string refTypeName)
		{
			string clrTypeName = null;
			refTypeName = null;
			if (!(this.IsNamedComplexType ? false : !this.IsTypeRef))
			{
				string identifier = null;
				clrTypeName = (!nameMappings.TryGetValue(this.schemaObject, out identifier) ? this.typeName : identifier);
				refTypeName = clrTypeName;
				if ((this.typeNs == string.Empty ? false : this.typeNs != parentTypeClrNs))
				{
					clrTypeName = string.Concat(this.typeNs, ".", clrTypeName);
				}
			}
			else if (!this.IsAnyType)
			{
				Debug.Assert(this.IsSimpleType);
				XmlSchemaSimpleType st = this.schemaObject as XmlSchemaSimpleType;
				Debug.Assert(st != null);
				clrTypeName = (!this.IsSchemaList ? st.Datatype.ValueType.ToString() : st.GetListItemType().Datatype.ValueType.ToString());
			}
			else
			{
				clrTypeName = "XTypedElement";
			}
			return clrTypeName;
		}

		internal string GetSimpleTypeClrTypeDefName(string parentTypeClrNs, Dictionary<XmlSchemaObject, string> nameMappings)
		{
			Debug.Assert(this.IsSimpleType);
			string clrTypeName = null;
			XmlSchemaObject key = this.schemaObject;
			if (this.IsTypeRef)
			{
				key = ((XmlSchemaElement)this.schemaObject).ElementSchemaType as XmlSchemaSimpleType;
				Debug.Assert(key != null);
			}
			string identifier = null;
			clrTypeName = (!nameMappings.TryGetValue(key, out identifier) ? this.typeName : identifier);
			if (!this.IsLocalType)
			{
				clrTypeName = string.Concat("global::", (this.typeNs == string.Empty ? parentTypeClrNs : this.typeNs), ".", clrTypeName);
			}
			return clrTypeName;
		}

		private void SetVariety(XmlSchemaDatatype datatype)
		{
			XmlSchemaDatatypeVariety variety = datatype.Variety;
			if (variety == XmlSchemaDatatypeVariety.List)
			{
				this.typeRefFlags |= ClrTypeRefFlags.IsSchemaList;
			}
			else if (variety == XmlSchemaDatatypeVariety.Union)
			{
				this.typeRefFlags |= ClrTypeRefFlags.IsUnion;
			}
		}
	}
}