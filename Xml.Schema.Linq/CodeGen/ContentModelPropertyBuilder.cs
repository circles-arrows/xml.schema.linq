using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class ContentModelPropertyBuilder : TypePropertyBuilder
	{
		protected GroupingInfo grouping;

		protected CodeObjectCreateExpression contentModelExpression;

		public ContentModelPropertyBuilder(GroupingInfo grouping, CodeTypeDeclaration decl, CodeTypeDeclItems declItems) : base(decl, declItems)
		{
			this.grouping = grouping;
		}

		private void AddToContentModel()
		{
			this.contentModelExpression = this.CreateContentModelExpression();
			CodeObjectCreateExpression typeContentModelExp = this.declItems.contentModelExpression;
			if (typeContentModelExp != null)
			{
				typeContentModelExp.Parameters.Add(this.contentModelExpression);
			}
			else
			{
				this.declItems.contentModelExpression = this.contentModelExpression;
			}
		}

		public abstract CodeObjectCreateExpression CreateContentModelExpression();

		public override void GenerateCode(ClrBasePropertyInfo property, List<ClrAnnotation> annotations)
		{
			this.GenerateConstructorCode(property);
			property.AddToType(this.decl, annotations);
			if (!this.declItems.hasElementWildCards)
			{
				property.AddToContentModel(this.contentModelExpression);
			}
		}

		public virtual void GenerateConstructorCode(ClrBasePropertyInfo property)
		{
		}

		public override void StartCodeGen()
		{
			this.AddToContentModel();
		}
	}
}