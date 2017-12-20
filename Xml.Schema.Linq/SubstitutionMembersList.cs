using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	internal class SubstitutionMembersList : IEnumerable<XElement>, IEnumerable
	{
		private XTypedElement container;

		private XName[] namesInList;

		internal SubstitutionMembersList(XTypedElement container, params XName[] memberNames)
		{
			this.container = container;
			this.namesInList = memberNames;
		}

		internal IEnumerator<XElement> FSMGetEnumerator()
		{
			IEnumerator<XElement> enumerator = this.container.Untyped.Elements().GetEnumerator();
			XElement xElement = null;
			this.container.StartFsm();
			while (true)
			{
				xElement = this.container.ExecuteFSMSubGroup(enumerator, this.namesInList);
				if (xElement == null)
				{
					break;
				}
				else
				{
					yield return xElement;
					if (xElement == null)
					{
						break;
					}
				}
			}
		}

		public IEnumerator<XElement> GetEnumerator()
		{
			foreach (XElement xElement in this.container.Untyped.Elements())
			{
				for (int i = 0; i < (int)this.namesInList.Length; i++)
				{
					if (this.namesInList.GetValue(i).Equals(xElement.Name))
					{
						yield return xElement;
					}
				}
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}