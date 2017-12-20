using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Xml.Schema.Linq.CodeGen
{
	internal class CodeTypeDeclItems
	{
		public CodeConstructor functionalConstructor;

		public CodeTypeConstructor staticConstructor;

		public CodeObjectCreateExpression contentModelExpression;

		public Dictionary<string, CodeMemberProperty> propertyNameTypeTable;

		public bool hasElementWildCards;

		public CodeTypeDeclItems()
		{
		}

		public void Init()
		{
			this.functionalConstructor = null;
			this.staticConstructor = null;
			this.hasElementWildCards = false;
			this.contentModelExpression = null;
			if (this.propertyNameTypeTable != null)
			{
				this.propertyNameTypeTable.Clear();
			}
		}
	}
}