using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class GlobalSymbolTable
	{
		internal Dictionary<SymbolEntry, SymbolEntry> symbols;

		internal Dictionary<XmlSchemaObject, string> schemaNameToIdentifiers;

		internal int nFixedNames = 0;

		private LinqToXsdSettings configSettings;

		public GlobalSymbolTable(LinqToXsdSettings settings)
		{
			this.configSettings = settings;
			this.symbols = new Dictionary<SymbolEntry, SymbolEntry>();
			this.schemaNameToIdentifiers = new Dictionary<XmlSchemaObject, string>();
		}

		public SymbolEntry AddElement(XmlSchemaElement element)
		{
			return this.AddSymbol(element.QualifiedName, element, string.Empty);
		}

		protected SymbolEntry AddSymbol(XmlQualifiedName qname, XmlSchemaObject schemaObject, string suffix)
		{
			SymbolEntry symbol = new SymbolEntry()
			{
				xsdNamespace = qname.Namespace,
				clrNamespace = this.configSettings.GetClrNamespace(qname.Namespace),
				symbolName = qname.Name
			};
			string identifierName = NameGenerator.MakeValidIdentifier(symbol.symbolName, this.configSettings.NameMangler2);
			symbol.identifierName = identifierName;
			int id = 0;
			if (this.symbols.ContainsKey(symbol))
			{
				identifierName = string.Concat(identifierName, suffix);
				symbol.identifierName = identifierName;
				while (this.symbols.ContainsKey(symbol))
				{
					id++;
					symbol.identifierName = string.Concat(identifierName, id.ToString(CultureInfo.InvariantCulture.NumberFormat));
				}
			}
			if (symbol.isNameFixed())
			{
				this.nFixedNames++;
			}
			this.symbols.Add(symbol, symbol);
			this.schemaNameToIdentifiers.Add(schemaObject, symbol.identifierName);
			return symbol;
		}

		public SymbolEntry AddType(XmlQualifiedName name, XmlSchemaType type)
		{
			return this.AddSymbol(name, type, "Type");
		}
	}
}