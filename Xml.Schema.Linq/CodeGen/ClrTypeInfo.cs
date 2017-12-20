using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class ClrTypeInfo
	{
		internal string clrtypeName;

		internal string clrtypeNs;

		internal string schemaName;

		internal string schemaNs;

		internal string clrFullTypeName;

		internal string contentModelRegEx;

		internal XmlSchemaObject baseType;

		internal string baseTypeClrName;

		internal string baseTypeClrNs;

		protected ClrTypeFlags clrTypeFlags;

		internal SchemaOrigin typeOrigin;

		protected List<ClrAnnotation> annotations;

		internal List<ClrAnnotation> Annotations
		{
			get
			{
				return this.annotations;
			}
		}

		internal XmlQualifiedName BaseTypeName
		{
			get
			{
				XmlQualifiedName empty;
				if (this.baseType != null)
				{
					XmlSchemaType schemaType = this.baseType as XmlSchemaType;
					empty = (schemaType == null ? (this.baseType as XmlSchemaElement).QualifiedName : schemaType.QualifiedName);
				}
				else
				{
					empty = XmlQualifiedName.Empty;
				}
				return empty;
			}
		}

		internal string ContentModelRegEx
		{
			get
			{
				return this.contentModelRegEx;
			}
			set
			{
				this.contentModelRegEx = value;
			}
		}

		internal virtual bool HasBaseContentType
		{
			get
			{
				return false;
			}
		}

		internal bool HasElementWildCard
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.HasElementWildCard) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsSealed | ClrTypeFlags.IsRoot | ClrTypeFlags.IsNested | ClrTypeFlags.InlineBaseType | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.HasElementWildCard;
				}
			}
		}

		internal bool InlineBaseType
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.InlineBaseType) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsSealed | ClrTypeFlags.IsRoot | ClrTypeFlags.IsNested | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.InlineBaseType;
				}
			}
		}

		internal bool IsAbstract
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.IsAbstract) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsSealed | ClrTypeFlags.IsRoot | ClrTypeFlags.IsNested | ClrTypeFlags.InlineBaseType | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.IsAbstract;
				}
			}
		}

		internal bool IsDerived
		{
			get
			{
				return this.baseType != null;
			}
		}

		internal bool IsNested
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.IsNested) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsSealed | ClrTypeFlags.IsRoot | ClrTypeFlags.InlineBaseType | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.IsNested;
				}
			}
		}

		internal bool IsRoot
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.IsRoot) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsSealed | ClrTypeFlags.IsNested | ClrTypeFlags.InlineBaseType | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.IsRoot;
				}
			}
		}

		internal bool IsRootElement
		{
			get
			{
				return this.typeOrigin == SchemaOrigin.Element;
			}
		}

		internal bool IsSealed
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.IsSealed) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsRoot | ClrTypeFlags.IsNested | ClrTypeFlags.InlineBaseType | ClrTypeFlags.IsSubstitutionHead | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.IsSealed;
				}
			}
		}

		internal bool IsSubstitutionHead
		{
			get
			{
				return (this.clrTypeFlags & ClrTypeFlags.IsSubstitutionHead) != ClrTypeFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrTypeInfo clrTypeInfo = this;
					clrTypeInfo.clrTypeFlags = clrTypeInfo.clrTypeFlags & (ClrTypeFlags.IsAbstract | ClrTypeFlags.IsSealed | ClrTypeFlags.IsRoot | ClrTypeFlags.IsNested | ClrTypeFlags.InlineBaseType | ClrTypeFlags.HasFixedValue | ClrTypeFlags.HasDefaultValue | ClrTypeFlags.HasElementWildCard);
				}
				else
				{
					this.clrTypeFlags |= ClrTypeFlags.IsSubstitutionHead;
				}
			}
		}

		internal virtual bool IsWrapper
		{
			get
			{
				return false;
			}
		}

		public ClrTypeInfo()
		{
			this.Init();
		}

		internal virtual FSM CreateFSM(StateNameSource stateNames)
		{
			throw new InvalidOperationException();
		}

		private void Init()
		{
			this.clrtypeName = null;
			this.clrtypeNs = null;
			this.schemaName = null;
			this.schemaNs = null;
			this.baseType = null;
			this.clrTypeFlags = ClrTypeFlags.None;
			this.typeOrigin = SchemaOrigin.None;
			this.annotations = new List<ClrAnnotation>();
		}

		private bool IsHeadAnyType()
		{
			Debug.Assert(this.baseType != null);
			XmlSchemaElement headElem = this.baseType as XmlSchemaElement;
			Debug.Assert(headElem != null);
			return headElem.ElementSchemaType.TypeCode == XmlTypeCode.Item;
		}

		internal bool IsSubstitutionMember()
		{
			return ((this.typeOrigin != SchemaOrigin.Element || this.baseType == null ? true : this.IsHeadAnyType()) ? false : true);
		}
	}
}