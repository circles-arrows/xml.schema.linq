using System;
using System.Globalization;

namespace Xml.Schema.Linq.CodeGen
{
	internal class SymbolEntry
	{
		public string xsdNamespace;

		public string clrNamespace;

		public string symbolName;

		public string identifierName;

		public SymbolEntry()
		{
		}

		public override bool Equals(object obj)
		{
			bool flag;
			SymbolEntry se = obj as SymbolEntry;
			if (se == null)
			{
				flag = false;
			}
			else
			{
				flag = (this.xsdNamespace != se.xsdNamespace ? false : this.identifierName.Equals(se.identifierName, StringComparison.OrdinalIgnoreCase));
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return this.identifierName.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
		}

		public bool isNameFixed()
		{
			return this.symbolName != this.identifierName;
		}
	}
}