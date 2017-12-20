using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class ClrBasePropertyInfo : ContentInfo
	{
		protected string propertyName;

		protected string schemaName;

		protected string propertyNs;

		protected bool hasSet;

		protected XCodeTypeReference returnType;

		protected bool isVirtual;

		protected bool isOverride;

		protected List<ClrAnnotation> annotations;

		internal List<ClrAnnotation> Annotations
		{
			get
			{
				return this.annotations;
			}
		}

		internal virtual string ClrTypeName
		{
			get
			{
				return null;
			}
		}

		internal virtual bool FromBaseType
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal bool HasSet
		{
			get
			{
				return this.hasSet;
			}
			set
			{
				this.hasSet = value;
			}
		}

		internal virtual bool IsDuplicate
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal virtual bool IsList
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal virtual bool IsNew
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal virtual bool IsNullable
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal bool IsOverride
		{
			get
			{
				return this.isOverride;
			}
			set
			{
				this.isOverride = value;
			}
		}

		internal virtual bool IsSchemaList
		{
			get
			{
				throw new InvalidOperationException();
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal virtual bool IsUnion
		{
			get
			{
				throw new InvalidOperationException();
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		internal bool IsVirtual
		{
			get
			{
				return this.isVirtual;
			}
			set
			{
				this.isVirtual = value;
			}
		}

		internal string PropertyName
		{
			get
			{
				return this.propertyName;
			}
			set
			{
				this.propertyName = value;
			}
		}

		internal string PropertyNs
		{
			get
			{
				return this.propertyNs;
			}
			set
			{
				this.propertyNs = value;
			}
		}

		internal virtual XCodeTypeReference ReturnType
		{
			get
			{
				return this.returnType;
			}
			set
			{
				this.returnType = value;
			}
		}

		internal string SchemaName
		{
			get
			{
				return this.schemaName;
			}
			set
			{
				this.schemaName = value;
			}
		}

		internal virtual bool VerifyRequired
		{
			get
			{
				return false;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public ClrBasePropertyInfo()
		{
			this.IsVirtual = false;
			this.isOverride = false;
			this.returnType = null;
			this.annotations = new List<ClrAnnotation>();
		}

		internal abstract void AddToConstructor(CodeConstructor functionalConstructor);

		internal abstract void AddToContentModel(CodeObjectCreateExpression contentModelExpression);

		internal abstract CodeMemberProperty AddToType(CodeTypeDeclaration decl, List<ClrAnnotation> annotations);

		internal virtual void ApplyAnnotations(CodeMemberProperty propDecl, List<ClrAnnotation> typeAnnotations)
		{
			TypeBuilder.ApplyAnnotations(propDecl, this, typeAnnotations);
		}
	}
}