using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class LocalSymbolTable
	{
		private Hashtable symbolToQName;

		private Hashtable qNameToSymbol;

		private List<AnonymousType> anonymousTypes;

		private readonly LinqToXsdSettings ConfigSettings;

		public LocalSymbolTable(LinqToXsdSettings configSettings)
		{
			this.ConfigSettings = configSettings;
		}

		public void AddAnonymousType(string identifier, XmlSchemaElement parentElement, ClrTypeReference parentElementTypeRef)
		{
			AnonymousType at = new AnonymousType()
			{
				identifier = identifier,
				typeRefence = parentElementTypeRef,
				parentElement = parentElement
			};
			this.anonymousTypes.Add(at);
		}

		public string AddAttribute(XmlSchemaAttribute attribute)
		{
			string identifierName = NameGenerator.MakeValidIdentifier(attribute.QualifiedName.Name, this.ConfigSettings.NameMangler2);
			identifierName = this.getSymbol(identifierName, "");
			this.symbolToQName.Add(identifierName.ToUpper(CultureInfo.InvariantCulture), attribute.QualifiedName);
			return identifierName;
		}

		public void AddComplexRestrictedContentType(XmlSchemaComplexType wrappingType, ClrTypeReference wrappingTypeRef)
		{
			string identifier = NameGenerator.MakeValidIdentifier(wrappingType.Name, this.ConfigSettings.NameMangler2);
			AnonymousType at = new AnonymousType()
			{
				identifier = identifier,
				typeRefence = wrappingTypeRef,
				wrappingType = wrappingType
			};
			this.anonymousTypes.Add(at);
		}

		public string AddLocalElement(XmlSchemaElement element)
		{
			string str;
			string identifierName = (string)this.qNameToSymbol[element.QualifiedName];
			if (identifierName == null)
			{
				identifierName = NameGenerator.MakeValidIdentifier(element.QualifiedName.Name, this.ConfigSettings.NameMangler2);
				identifierName = this.getSymbol(identifierName, "");
				this.symbolToQName.Add(identifierName.ToUpper(CultureInfo.InvariantCulture), element.QualifiedName);
				this.qNameToSymbol.Add(element.QualifiedName, identifierName);
				str = identifierName;
			}
			else
			{
				str = identifierName;
			}
			return str;
		}

		public string AddMember(string identifierName)
		{
			string outputSymbol = null;
			outputSymbol = this.getSymbol(identifierName, string.Empty);
			this.symbolToQName.Add(outputSymbol.ToUpper(CultureInfo.InvariantCulture), identifierName);
			return outputSymbol;
		}

		public List<AnonymousType> GetAnonymousTypes()
		{
			foreach (AnonymousType at in this.anonymousTypes)
			{
				ClrTypeReference typeReference = at.typeRefence;
				string typeIdentifier = this.getSymbol(at.identifier, "LocalType");
				this.symbolToQName.Add(typeIdentifier.ToUpper(CultureInfo.InvariantCulture), XmlQualifiedName.Empty);
				typeReference.Name = typeIdentifier;
				at.identifier = typeIdentifier;
			}
			return this.anonymousTypes;
		}

		private string getSymbol(string identifierName, string suffix)
		{
			string str;
			int id = 0;
			string symbol = identifierName;
			string symbolU = symbol.ToUpper(CultureInfo.InvariantCulture);
			if (this.symbolToQName[symbolU] != null)
			{
				symbol = string.Concat(symbol, suffix);
				symbolU = symbol.ToUpper(CultureInfo.InvariantCulture);
				string temp = symbolU;
				while (this.symbolToQName[symbolU] != null)
				{
					id++;
					symbolU = string.Concat(temp, id.ToString(CultureInfo.InvariantCulture.NumberFormat));
				}
				if (id > 0)
				{
					symbol = string.Concat(symbol, id.ToString(CultureInfo.InvariantCulture.NumberFormat));
				}
				str = symbol;
			}
			else
			{
				str = symbol;
			}
			return str;
		}

		public void Init(XmlSchemaElement element)
		{
			this.Init(element.QualifiedName.Name);
		}

		public void Init(XmlSchemaType type)
		{
			this.Init(type.QualifiedName.Name);
		}

		public void Init(string className)
		{
			if (this.anonymousTypes != null)
			{
				this.Reset();
			}
			else
			{
				this.symbolToQName = new Hashtable();
				this.qNameToSymbol = new Hashtable();
				this.anonymousTypes = new List<AnonymousType>();
			}
			this.symbolToQName.Add(className.ToUpper(CultureInfo.InvariantCulture), XmlQualifiedName.Empty);
		}

		public void RegisterMember(string identifierName)
		{
			string outputSymbol = null;
			outputSymbol = this.AddMember(identifierName);
			Debug.Assert(outputSymbol == identifierName);
		}

		public void Reset()
		{
			this.symbolToQName.Clear();
			this.qNameToSymbol.Clear();
			if (this.anonymousTypes.Count > 0)
			{
				this.anonymousTypes = new List<AnonymousType>();
			}
		}
	}
}