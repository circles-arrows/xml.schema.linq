using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xml.Schema.Linq.CodeGen;

namespace Xml.Schema.Linq
{
	public class LinqToXsdSettings
	{
		private Dictionary<string, string> namespaceMapping;

		internal XElement trafo;

		private bool verifyRequired = false;

		private bool enableServiceReference = false;

		public readonly bool NameMangler2;

		public bool EnableServiceReference
		{
			get
			{
				return this.enableServiceReference;
			}
			set
			{
				this.enableServiceReference = value;
			}
		}

		public bool VerifyRequired
		{
			get
			{
				return this.verifyRequired;
			}
		}

		public LinqToXsdSettings(bool nameMangler2)
		{
			this.NameMangler2 = nameMangler2;
			this.namespaceMapping = new Dictionary<string, string>();
		}

		private void GenerateNamespaceMapping(XElement namespaces)
		{
			if (namespaces != null)
			{
				foreach (XElement ns in namespaces.Elements(XName.Get("Namespace", "http://www.microsoft.com/xml/schema/linq")))
				{
					this.namespaceMapping.Add((string)ns.Attribute(XName.Get("Schema")), (string)ns.Attribute(XName.Get("Clr")));
				}
			}
		}

		public string GetClrNamespace(string xmlNamespace)
		{
			string str;
			string clrNamespace = string.Empty;
			if (xmlNamespace == null)
			{
				str = clrNamespace;
			}
			else if (!this.namespaceMapping.TryGetValue(xmlNamespace, out clrNamespace))
			{
				clrNamespace = NameGenerator.MakeValidCLRNamespace(xmlNamespace, this.NameMangler2);
				this.namespaceMapping.Add(xmlNamespace, clrNamespace);
				str = clrNamespace;
			}
			else
			{
				str = clrNamespace;
			}
			return str;
		}

		public void Load(string configFile)
		{
			if ((configFile == null ? true : configFile.Length == 0))
			{
				throw new ArgumentException("Argument configFile should be non-null and non-empty.");
			}
			XElement rootElement = XDocument.Load(configFile).Root;
			this.GenerateNamespaceMapping(rootElement.Element(XName.Get("Namespaces", "http://www.microsoft.com/xml/schema/linq")));
			this.trafo = rootElement.Element(XName.Get("Transformation", "http://www.microsoft.com/FXT"));
			XElement validationSettings = rootElement.Element(XName.Get("Validation", "http://www.microsoft.com/xml/schema/linq"));
			if (validationSettings != null)
			{
				this.verifyRequired = ((string)validationSettings.Element(XName.Get("VerifyRequired", "http://www.microsoft.com/xml/schema/linq")) == "true" ? true : false);
			}
		}
	}
}