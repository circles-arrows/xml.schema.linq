using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class XSimpleList<T> : XList<T>
	{
		private XmlSchemaDatatype schemaDatatype;

		public XSimpleList(XTypedElement container, XmlSchemaDatatype dataType, XName itemXName) : base(container, new XName[] { itemXName })
		{
			this.schemaDatatype = dataType;
		}

		public override void Add(T value)
		{
			this.container.SetElement(this.itemXName, value, true, this.schemaDatatype);
		}

		public static XSimpleList<T> CopyFromWithValidation(IEnumerable<T> values, XTypedElement container, XName itemXName, XmlSchemaDatatype dataType, string propertyName, SimpleTypeValidator typeDef)
		{
			return XSimpleList<T>.Initialize(container, dataType, values, itemXName);
		}

		protected override XElement GetElementForValue(T value, bool createNew)
		{
			XElement xElement;
			if (!createNew)
			{
				IEnumerator<XElement> listElementsEnumerator = base.GetListElementsEnumerator();
				while (listElementsEnumerator.MoveNext())
				{
					XElement current = listElementsEnumerator.Current;
					if (this.IsEqual(current, value))
					{
						xElement = current;
						return xElement;
					}
				}
				xElement = null;
			}
			else
			{
				xElement = new XElement(this.itemXName, XTypedServices.GetXmlString(value, this.schemaDatatype, this.containerElement));
			}
			return xElement;
		}

		protected override T GetValueForElement(XElement element)
		{
			string stringValue = element.Value;
			return (T)this.schemaDatatype.ChangeType(stringValue, typeof(T));
		}

		public static XSimpleList<T> Initialize(XTypedElement container, XmlSchemaDatatype dataType, IEnumerable<T> values, XName itemXName)
		{
			XSimpleList<T> simpleList = new XSimpleList<T>(container, dataType, itemXName);
			simpleList.Clear();
			foreach (T value in values)
			{
				simpleList.Add(value);
			}
			return simpleList;
		}

		protected override bool IsEqual(XElement element, T value)
		{
			bool flag;
			string stringValue = element.Value;
			flag = (!this.schemaDatatype.ChangeType(stringValue, typeof(T)).Equals(value) ? false : true);
			return flag;
		}

		protected override void UpdateElement(XElement oldElement, T value)
		{
			oldElement.Value = XTypedServices.GetXmlString(value, this.schemaDatatype, oldElement);
		}
	}
}