using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public abstract class SchemaAwareContentModelEntity : ContentModelEntity
	{
		internal Dictionary<XName, NamedContentModelEntity> elementPositions;

		private int last = 0;

		internal virtual Xml.Schema.Linq.ContentModelType ContentModelType
		{
			get
			{
				return Xml.Schema.Linq.ContentModelType.None;
			}
		}

		internal Dictionary<XName, NamedContentModelEntity> ElementPositions
		{
			get
			{
				if (this.elementPositions == null)
				{
					this.elementPositions = new Dictionary<XName, NamedContentModelEntity>();
				}
				return this.elementPositions;
			}
		}

		protected SchemaAwareContentModelEntity(params ContentModelEntity[] items)
		{
			int num;
			this.elementPositions = new Dictionary<XName, NamedContentModelEntity>();
			ContentModelEntity[] contentModelEntityArray = items;
			for (int i = 0; i < (int)contentModelEntityArray.Length; i++)
			{
				ContentModelEntity cmEntity = contentModelEntityArray[i];
				NamedContentModelEntity named = cmEntity as NamedContentModelEntity;
				if (named == null)
				{
					SchemaAwareContentModelEntity scmEntity = cmEntity as SchemaAwareContentModelEntity;
					Debug.Assert(scmEntity != null);
					foreach (NamedContentModelEntity childEntity in scmEntity.ElementPositions.Values)
					{
						if (!this.elementPositions.ContainsKey(childEntity.Name))
						{
							SchemaAwareContentModelEntity schemaAwareContentModelEntity = this;
							int num1 = schemaAwareContentModelEntity.last;
							num = num1;
							schemaAwareContentModelEntity.last = num1 + 1;
							childEntity.ElementPosition = num;
							this.elementPositions.Add(childEntity.Name, childEntity);
						}
					}
				}
				else if (!this.elementPositions.ContainsKey(named.Name))
				{
					SchemaAwareContentModelEntity schemaAwareContentModelEntity1 = this;
					int num2 = schemaAwareContentModelEntity1.last;
					num = num2;
					schemaAwareContentModelEntity1.last = num2 + 1;
					named.ElementPosition = num;
					named.ParentContentModel = this;
					this.elementPositions.Add(named.Name, named);
					this.CheckSubstitutionGroup(named);
				}
			}
		}

		internal void AddElementInPosition(XName name, XElement parentElement, bool addToExisting, XTypedElement xObj)
		{
			NamedContentModelEntity namedEntity = this.GetNamedEntity(name);
			if (namedEntity == null)
			{
				throw new LinqToXsdException(string.Concat("Name does not belong in content model. Cannot set value for child ", name.LocalName));
			}
			EditAction editAction = EditAction.None;
			XElement elementMarker = this.FindElementPosition(namedEntity, parentElement, addToExisting, out editAction);
			XElement newElement = XTypedServices.GetXElement(xObj, name);
			Debug.Assert(xObj != null);
			switch (editAction)
			{
				case EditAction.AddBefore:
				{
					elementMarker.AddBeforeSelf(newElement);
					return;
				}
				case EditAction.AddAfter:
				{
					return;
				}
				case EditAction.Append:
				{
					parentElement.Add(newElement);
					return;
				}
				case EditAction.Update:
				{
					elementMarker.AddBeforeSelf(newElement);
					elementMarker.Remove();
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public override void AddElementToParent(XName name, object value, XElement parentElement, bool addToExisting, XmlSchemaDatatype datatype)
		{
			Debug.Assert(value != null);
			if (datatype == null)
			{
				this.AddElementInPosition(name, parentElement, addToExisting, value as XTypedElement);
			}
			else
			{
				this.AddValueInPosition(name, parentElement, addToExisting, value, datatype);
			}
		}

		internal void AddValueInPosition(XName name, XElement parentElement, bool addToExisting, object value, XmlSchemaDatatype datatype)
		{
			NamedContentModelEntity namedEntity = this.GetNamedEntity(name);
			if (namedEntity == null)
			{
				throw new LinqToXsdException(string.Concat("Name does not belong in content model. Cannot set value for child ", namedEntity.Name));
			}
			EditAction editAction = EditAction.None;
			XElement elementMarker = this.FindElementPosition(namedEntity, parentElement, addToExisting, out editAction);
			Debug.Assert(datatype != null);
			switch (editAction)
			{
				case EditAction.AddBefore:
				{
					Debug.Assert(elementMarker != null);
					elementMarker.AddBeforeSelf(new XElement(name, XTypedServices.GetXmlString(value, datatype, elementMarker)));
					break;
				}
				case EditAction.AddAfter:
				{
					throw new InvalidOperationException();
				}
				case EditAction.Append:
				{
					parentElement.Add(new XElement(name, XTypedServices.GetXmlString(value, datatype, parentElement)));
					break;
				}
				case EditAction.Update:
				{
					Debug.Assert(elementMarker != null);
					elementMarker.Value = XTypedServices.GetXmlString(value, datatype, elementMarker);
					break;
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
		}

		private void CheckSubstitutionGroup(NamedContentModelEntity named)
		{
			SubstitutedContentModelEntity substEntity = named as SubstitutedContentModelEntity;
			if (substEntity != null)
			{
				XName[] members = substEntity.Members;
				for (int i = 0; i < (int)members.Length; i++)
				{
					XName name = members[i];
					if (!this.elementPositions.ContainsKey(name))
					{
						this.elementPositions.Add(name, named);
					}
				}
			}
		}

		internal XElement FindElementPosition(NamedContentModelEntity namedEntity, XElement parentElement, bool addToExisting, out EditAction editAction)
		{
			XElement xElement;
			Debug.Assert(namedEntity != null);
			editAction = EditAction.None;
			int newElementPos = namedEntity.ElementPosition;
			XElement lastElement = this.GetLastElement(parentElement);
			if (lastElement != null)
			{
				int lastElementPos = this.GetNamedEntity(lastElement.Name).ElementPosition;
				if (newElementPos == lastElementPos)
				{
					if (!addToExisting)
					{
						editAction = EditAction.Update;
					}
					else
					{
						editAction = EditAction.Append;
					}
					xElement = lastElement;
					return xElement;
				}
				else if (newElementPos > lastElementPos)
				{
					editAction = EditAction.Append;
					xElement = lastElement;
					return xElement;
				}
			}
			int instanceElementPos = -1;
			XElement instanceElem = null;
			IEnumerator<XElement> enumerator = parentElement.Elements().GetEnumerator();
			while (enumerator.MoveNext())
			{
				instanceElem = enumerator.Current;
				instanceElementPos = this.GetElementPosition(instanceElem.Name);
				if (instanceElementPos == newElementPos)
				{
					if (!addToExisting)
					{
						editAction = EditAction.Update;
						xElement = instanceElem;
						return xElement;
					}
				}
				else if (instanceElementPos > newElementPos)
				{
					editAction = EditAction.AddBefore;
					xElement = instanceElem;
					return xElement;
				}
			}
			editAction = EditAction.Append;
			xElement = instanceElem;
			return xElement;
		}

		internal int GetElementPosition(XName name)
		{
			NamedContentModelEntity named = this.GetNamedEntity(name);
			return (named == null ? -1 : named.ElementPosition);
		}

		private XElement GetLastElement(XElement parentElement)
		{
			return parentElement.LastNode as XElement;
		}

		internal NamedContentModelEntity GetNamedEntity(XName name)
		{
			NamedContentModelEntity namedEntity = null;
			this.elementPositions.TryGetValue(name, out namedEntity);
			return namedEntity;
		}
	}
}