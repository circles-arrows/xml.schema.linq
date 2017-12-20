using System;
using System.CodeDom;

namespace Xml.Schema.Linq.CodeGen
{
	internal class SequencePropertyBuilder : ContentModelPropertyBuilder
	{
		public SequencePropertyBuilder(GroupingInfo grouping, CodeTypeDeclaration decl, CodeTypeDeclItems declItems) : base(grouping, decl, declItems)
		{
		}

		public override CodeObjectCreateExpression CreateContentModelExpression()
		{
			return new CodeObjectCreateExpression(new CodeTypeReference("SequenceContentModelEntity"), new CodeExpression[0]);
		}
	}
}