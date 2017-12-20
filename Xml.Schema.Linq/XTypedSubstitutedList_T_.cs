using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class XTypedSubstitutedList<T> : XList<T>
	where T : XTypedElement
	{
		private ILinqToXsdTypeManager typeManager;

		public XTypedSubstitutedList(XTypedElement container, ILinqToXsdTypeManager typeManager, params XName[] itemXNames) : base(container, itemXNames)
		{
			this.typeManager = typeManager;
		}

		public override void Add(T value)
		{
			XName itemXName = value.Untyped.Name;
			this.container.SetElement(itemXName, value, true, null);
		}

		protected override XElement GetElementForValue(T value, bool createNew)
		{
			return value.Untyped;
		}

		protected override T GetValueForElement(XElement element)
		{
			return (T)XTypedServices.ToXTypedElement(element, this.typeManager);
		}

		public static XTypedSubstitutedList<T> Initialize(XTypedElement container, ILinqToXsdTypeManager typeManager, IEnumerable<T> typedObjects, params XName[] itemXNames)
		{
			XTypedSubstitutedList<T> typedList = new XTypedSubstitutedList<T>(container, typeManager, itemXNames);
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