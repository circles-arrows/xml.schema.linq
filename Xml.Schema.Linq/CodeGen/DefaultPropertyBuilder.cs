using System;
using System.CodeDom;

namespace Xml.Schema.Linq.CodeGen
{
	internal class DefaultPropertyBuilder : TypePropertyBuilder
	{
		internal DefaultPropertyBuilder(CodeTypeDeclaration decl, CodeTypeDeclItems declItems) : base(decl, declItems)
		{
		}
	}
}