using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class XTypedList<T> : XList<T>
	where T : XTypedElement
	{
		private ILinqToXsdTypeManager typeManager;

		public XTypedList(XTypedElement container, XName itemXName) : this(container, null, itemXName)
		{
		}

		public XTypedList(XTypedElement container, ILinqToXsdTypeManager typeManager, XName itemXName) : base(container, new XName[] { itemXName })
		{
			this.typeManager = typeManager;
		}

		public override void Add(T value)
		{
			this.container.SetElement(this.itemXName, value, true, null);
		}

		public static XTypedList<T> CopyFromWithValidation(IEnumerable<T> typedObjects, XTypedElement container, XName itemXName, ILinqToXsdTypeManager typeManager, string propertyName, SimpleTypeValidator typeDef)
		{
			return XTypedList<T>.Initialize(container, typeManager, typedObjects, itemXName);
		}

		protected override XElement GetElementForValue(T value, bool createNew)
		{
			XElement element = value.Untyped;
			element.Name = this.itemXName;
			return element;
		}

		protected override T GetValueForElement(XElement element)
		{
			return XTypedServices.ToXTypedElement<T>(element, this.typeManager);
		}

		public static XTypedList<T> Initialize(XTypedElement container, ILinqToXsdTypeManager typeManager, IEnumerable<T> typedObjects, XName itemXName)
		{
			XTypedList<T> typedList = new XTypedList<T>(container, typeManager, itemXName);
			typedList.Clear();
			foreach (T typedItem in typedObjects)
			{
				typedList.Add(typedItem);
			}
			return typedList;
		}

		protected override bool IsEqual(XElement element, T value)
		{
			return element.Equals(value.Untyped);
		}

		protected override void UpdateElement(XElement oldElement, T value)
		{
			oldElement.AddBeforeSelf(this.GetElementForValue(value, true));
			oldElement.Remove();
		}
	}
}