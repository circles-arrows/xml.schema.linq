using System;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public abstract class ContentModelEntity
	{
		public readonly static ContentModelEntity Default;

		static ContentModelEntity()
		{
			ContentModelEntity.Default = new OrderUnawareContentModelEntity();
		}

		protected ContentModelEntity()
		{
		}

		public virtual void AddElementToParent(XName name, object value, XElement parentElement, bool addToExisting, XmlSchemaDatatype datatype)
		{
			Debug.Assert(value != null);
			if (!addToExisting)
			{
				XElement existingElement = parentElement.Element(name);
				if (existingElement == null)
				{
					parentElement.Add(this.GetNewElement(name, value, datatype, parentElement));
				}
				else if (datatype == null)
				{
					existingElement.AddBeforeSelf(XTypedServices.GetXElement(value as XTypedElement, name));
					existingElement.Remove();
				}
				else
				{
					existingElement.Value = XTypedServices.GetXmlString(value, datatype, existingElement);
				}
			}
			else
			{
				parentElement.Add(this.GetNewElement(name, value, datatype, parentElement));
			}
		}

		private XElement GetNewElement(XName name, object value, XmlSchemaDatatype datatype, XElement parentElement)
		{
			XElement newElement = null;
			if (datatype == null)
			{
				newElement = XTypedServices.GetXElement(value as XTypedElement, name);
			}
			else
			{
				string stringValue = XTypedServices.GetXmlString(value, datatype, parentElement);
				newElement = new XElement(name, stringValue);
			}
			return newElement;
		}
	}
}