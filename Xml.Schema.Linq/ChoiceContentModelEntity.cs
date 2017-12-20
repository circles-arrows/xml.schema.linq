using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class ChoiceContentModelEntity : SchemaAwareContentModelEntity
	{
		internal override Xml.Schema.Linq.ContentModelType ContentModelType
		{
			get
			{
				return Xml.Schema.Linq.ContentModelType.Choice;
			}
		}

		public ChoiceContentModelEntity(params ContentModelEntity[] items) : base(items)
		{
		}

		public override void AddElementToParent(XName name, object value, XElement parentElement, bool addToExisting, XmlSchemaDatatype datatype)
		{
			base.AddElementToParent(name, value, parentElement, addToExisting, datatype);
			this.CheckChoiceBranches(name, parentElement);
		}

		private void CheckChoiceBranches(XName currentBranch, XElement parentElement)
		{
			List<XElement> elementsToRemove = new List<XElement>();
			NamedContentModelEntity otherBranch = null;
			foreach (XElement instanceElement in parentElement.Elements())
			{
				if (!(instanceElement.Name == currentBranch))
				{
					otherBranch = base.GetNamedEntity(instanceElement.Name);
					if (otherBranch != null)
					{
						Debug.Assert(otherBranch.ParentContentModel == this);
						elementsToRemove.Add(instanceElement);
					}
				}
			}
			foreach (XElement elementToRemove in elementsToRemove)
			{
				elementToRemove.Remove();
			}
		}
	}
}