using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public class FxtLog
	{
		private Dictionary<XmlQualifiedName, List<FxtAnnotation>> aTypes = new Dictionary<XmlQualifiedName, List<FxtAnnotation>>();

		private Dictionary<XmlQualifiedName, List<FxtAnnotation>> aElements = new Dictionary<XmlQualifiedName, List<FxtAnnotation>>();

		private Dictionary<XmlQualifiedName, List<FxtAnnotation>> aAttributes = new Dictionary<XmlQualifiedName, List<FxtAnnotation>>();

		private Dictionary<XmlSchemaObject, List<FxtAnnotation>> aObjects = new Dictionary<XmlSchemaObject, List<FxtAnnotation>>();

		public FxtLog()
		{
		}

		public List<FxtAnnotation> AtAttribute(XmlQualifiedName n)
		{
			if (!this.aAttributes.ContainsKey(n))
			{
				this.aAttributes.Add(n, new List<FxtAnnotation>());
			}
			return this.aAttributes[n];
		}

		public List<FxtAnnotation> AtElement(XmlQualifiedName n)
		{
			if (!this.aElements.ContainsKey(n))
			{
				this.aElements.Add(n, new List<FxtAnnotation>());
			}
			return this.aElements[n];
		}

		public List<FxtAnnotation> AtObject(XmlSchemaObject o)
		{
			if (!this.aObjects.ContainsKey(o))
			{
				this.aObjects.Add(o, new List<FxtAnnotation>());
			}
			return this.aObjects[o];
		}

		public List<FxtAnnotation> AtType(XmlQualifiedName n)
		{
			if (!this.aTypes.ContainsKey(n))
			{
				this.aTypes.Add(n, new List<FxtAnnotation>());
			}
			return this.aTypes[n];
		}
	}
}