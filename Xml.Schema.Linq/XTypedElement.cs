using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xml.Schema.Linq
{
	public class XTypedElement : IXMetaData, IXTyped, IXmlSerializable
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int currentState;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private XElement xElement = null;

		public IXTyped Query
		{
			get
			{
				return this;
			}
		}

		public virtual XElement Untyped
		{
			get
			{
				return this.GetUntyped();
			}
			set
			{
				this.xElement = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal FSM ValidationStates
		{
			get
			{
				IXMetaData metaData = this;
				Debug.Assert(metaData != null);
				return metaData.GetValidationStates();
			}
		}

		XTypedElement Xml.Schema.Linq.IXMetaData.Content
		{
			get
			{
				return null;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Dictionary<XName, Type> Xml.Schema.Linq.IXMetaData.LocalElementsDictionary
		{
			get
			{
				return XTypedServices.EmptyDictionary;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		XName Xml.Schema.Linq.IXMetaData.SchemaName
		{
			get
			{
				return XName.Get("anyType", "http://www.w3.org/2001/XMLSchema");
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ILinqToXsdTypeManager Xml.Schema.Linq.IXMetaData.TypeManager
		{
			get
			{
				return Xml.Schema.Linq.TypeManager.Default;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		SchemaOrigin Xml.Schema.Linq.IXMetaData.TypeOrigin
		{
			get
			{
				return SchemaOrigin.Fragment;
			}
		}

		public XTypedElement()
		{
		}

		public XTypedElement(XElement xe)
		{
			this.xElement = xe;
		}

		protected XAttribute Attribute(XName name)
		{
			return this.GetUntyped().Attribute(name);
		}

		private void CheckXsiNil(XElement parentElement)
		{
			XAttribute xsiNil = parentElement.Attributes(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance")).FirstOrDefault<XAttribute>();
			if ((xsiNil == null ? false : xsiNil.Value == "true"))
			{
				xsiNil.Remove();
			}
		}

		public virtual XTypedElement Clone()
		{
			return new XTypedElement()
			{
				Untyped = new XElement(this.Untyped)
			};
		}

		private XElement CreateXElement()
		{
			IXMetaData schemaMetaData = this;
			Debug.Assert(schemaMetaData != null);
			XName elementName = schemaMetaData.SchemaName;
			Debug.Assert(elementName != null);
			XElement element = new XElement(elementName);
			element.AddAnnotation(new XTypedElementAnnotation(this));
			return element;
		}

		private void DeleteChild(XName name)
		{
			XElement elementToDelete = this.GetElement(name);
			if (elementToDelete != null)
			{
				elementToDelete.Remove();
			}
		}

		protected XElement Element(XName xname)
		{
			return this.GetUntyped().Element(xname);
		}

		internal XElement ExecuteFSM(IEnumerator<XElement> enumerator, XName requestingXName, WildCard requestingWildCard)
		{
			XElement xElement;
			XElement currElem = null;
			WildCard matchingWildCard = null;
			XName matchingName = null;
			while (true)
			{
				if (enumerator.MoveNext())
				{
					currElem = enumerator.Current;
					this.currentState = this.FsmMakeTransition(this.currentState, currElem.Name, out matchingName, out matchingWildCard);
					if (this.currentState == FSM.InvalidState)
					{
						xElement = null;
						break;
					}
					else if (!(requestingXName == null ? true : !(matchingName != null)))
					{
						if (requestingXName.Equals(currElem.Name))
						{
							xElement = currElem;
							break;
						}
					}
					else if ((requestingWildCard == null ? false : matchingWildCard != null))
					{
						if (requestingWildCard.Allows(currElem.Name))
						{
							xElement = currElem;
							break;
						}
					}
				}
				else
				{
					xElement = null;
					break;
				}
			}
			return xElement;
		}

		internal XElement ExecuteFSMSubGroup(IEnumerator<XElement> enumerator, XName[] namesInList)
		{
			XElement xElement;
			Debug.Assert(namesInList != null);
			XElement currElem = null;
			WildCard matchingWildCard = null;
			XName matchingName = null;
			while (true)
			{
				if (enumerator.MoveNext())
				{
					currElem = enumerator.Current;
					this.currentState = this.FsmMakeTransition(this.currentState, currElem.Name, out matchingName, out matchingWildCard);
					if (this.currentState == FSM.InvalidState)
					{
						xElement = null;
						break;
					}
					else if (matchingName != null)
					{
						int i = 0;
						while (i < (int)namesInList.Length)
						{
							if (!namesInList.GetValue(i).Equals(currElem.Name))
							{
								i++;
							}
							else
							{
								xElement = currElem;
								return xElement;
							}
						}
					}
				}
				else
				{
					xElement = null;
					break;
				}
			}
			return xElement;
		}

		internal int FsmMakeTransition(int prevState, XName inputSymbol, out XName matchingName, out WildCard matchingWildCard)
		{
			Transitions currTrans = this.ValidationStates.Trans[prevState];
			return currTrans.GetNextState(inputSymbol, out matchingName, out matchingWildCard);
		}

		private void FSMSetElement(XName name, object value, bool addToExisting, XmlSchemaDatatype datatype)
		{
			XElement pos;
			XElement parentElement = this.GetUntyped();
			this.CheckXsiNil(parentElement);
			if (value == null)
			{
				this.DeleteChild(name);
			}
			else if (datatype == null)
			{
				XElement newElement = XTypedServices.GetXElement(value as XTypedElement, name);
				pos = this.GetElement(name);
				if (pos != null)
				{
					pos.AddAfterSelf(newElement);
					if (!addToExisting)
					{
						pos.Remove();
					}
				}
				else
				{
					parentElement.Add(newElement);
				}
			}
			else
			{
				pos = this.GetElement(name);
				if (pos == null)
				{
					parentElement.Add(new XElement(name, XTypedServices.GetXmlString(value, datatype, parentElement)));
				}
				else if (!addToExisting)
				{
					pos.Value = XTypedServices.GetXmlString(value, datatype, pos);
				}
				else
				{
					pos.AddAfterSelf(new XElement(name, XTypedServices.GetXmlString(value, datatype, pos)));
				}
			}
		}

		private XTypedElement GetContentType(XTypedElement clrRootObject)
		{
			IXMetaData childMetaData = clrRootObject;
			Debug.Assert(childMetaData.TypeOrigin == SchemaOrigin.Element);
			return childMetaData.Content;
		}

		protected internal XElement GetElement(XName requestingName)
		{
			return this.GetElement(requestingName, null);
		}

		protected internal XElement GetElement(WildCard requestingWildCard)
		{
			return this.GetElement(null, requestingWildCard);
		}

		private XElement GetElement(XName requestingName, WildCard requestingWildCard)
		{
			XElement xElement;
			if (this.ValidationStates != null)
			{
				this.StartFsm();
				xElement = this.ExecuteFSM(this.GetUntyped().Elements().GetEnumerator(), requestingName, requestingWildCard);
			}
			else
			{
				Debug.Assert(requestingName != null);
				xElement = this.GetUntyped().Element(requestingName);
			}
			return xElement;
		}

		private XTypedElement getTypedParent()
		{
			XTypedElement xTypedElement;
			XElement parentElement = this.Untyped.Parent;
			if (parentElement != null)
			{
				XTypedElementAnnotation annotation = parentElement.Annotation<XTypedElementAnnotation>();
				if (annotation != null)
				{
					xTypedElement = annotation.typedElement;
					return xTypedElement;
				}
			}
			xTypedElement = null;
			return xTypedElement;
		}

		private XElement GetUntyped()
		{
			if (this.xElement == null)
			{
				this.xElement = this.CreateXElement();
			}
			return this.xElement;
		}

		protected IEnumerable<XElement> GetWildCards(WildCard requestingWildCard)
		{
			IEnumerator<XElement> enumerator = this.GetUntyped().Elements().GetEnumerator();
			XElement xElement = null;
			this.StartFsm();
			while (true)
			{
				xElement = this.ExecuteFSM(enumerator, null, requestingWildCard);
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

		private bool IsAnnoatedElemTypeOf<T>(XElement element, out XTypedElement childTypedElement)
		where T : XTypedElement
		{
			bool flag;
			childTypedElement = null;
			T typedChild = XTypedServices.GetAnnotation<T>(element);
			if (typedChild == null)
			{
				flag = false;
			}
			else
			{
				childTypedElement = typedChild;
				flag = true;
			}
			return flag;
		}

		public static explicit operator XElement(XTypedElement xo)
		{
			return xo.Untyped;
		}

		public static explicit operator XTypedElement(XElement xe)
		{
			return new XTypedElement(xe);
		}

		protected void SetAttribute(XName name, object value, XmlSchemaDatatype datatype)
		{
			XElement element = this.GetUntyped();
			string stringValue = null;
			if (value != null)
			{
				stringValue = XTypedServices.GetXmlString(value, datatype, element);
			}
			element.SetAttributeValue(name, stringValue);
		}

		protected void SetAttributeWithValidation(XName name, object value, string propertyName, SimpleTypeValidator typeDef)
		{
			object typedValue = null;
			SimpleTypeValidator matchingType = null;
			Exception e = typeDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.GetUntyped()), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			this.SetAttribute(name, typedValue, typeDef.DataType);
		}

		protected void SetElement(XName name, XTypedElement typedElement)
		{
			if (this.ValidationStates != null)
			{
				this.FSMSetElement(name, typedElement, false, null);
			}
			else
			{
				Debug.Assert(name != null);
				this.SetElement(name, typedElement, false, null);
			}
		}

		protected void SetElement(XName name, object value, XmlSchemaDatatype datatype)
		{
			if (this.ValidationStates != null)
			{
				this.FSMSetElement(name, value, false, datatype);
			}
			else
			{
				Debug.Assert(name != null);
				this.SetElement(name, value, false, datatype);
			}
		}

		internal void SetElement(XName name, object value, bool addToExisting, XmlSchemaDatatype datatype)
		{
			XElement parentElement = this.GetUntyped();
			this.CheckXsiNil(parentElement);
			if (value != null)
			{
				IXMetaData schemaMetaData = this;
				Debug.Assert(schemaMetaData != null);
				schemaMetaData.GetContentModel().AddElementToParent(name, value, parentElement, addToExisting, datatype);
			}
			else
			{
				Debug.Assert(!addToExisting);
				this.DeleteChild(name);
			}
		}

		protected void SetElementWithValidation(XName name, object value, string propertyName, SimpleTypeValidator typeDef)
		{
			object typedValue = null;
			SimpleTypeValidator matchingType = null;
			Exception e = typeDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.GetUntyped()), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			this.SetElement(name, typedValue, typeDef.DataType);
		}

		protected void SetListAttribute(XName name, object value, XmlSchemaDatatype datatype)
		{
			this.SetAttribute(name, ListSimpleTypeValidator.ToString(value), datatype);
		}

		protected void SetListAttributeWithValidation(XName name, object value, string propertyName, SimpleTypeValidator typeDef)
		{
			object typedValue;
			SimpleTypeValidator matchingType = null;
			Exception e = typeDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.Untyped), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			ListSimpleTypeValidator listDef = typeDef as ListSimpleTypeValidator;
			Debug.Assert(listDef != null);
			this.SetListAttribute(name, value, listDef.ItemType.DataType);
		}

		protected void SetListElement(XName name, object value, XmlSchemaDatatype datatype)
		{
			this.SetElement(name, ListSimpleTypeValidator.ToString(value), datatype);
		}

		protected void SetListElementWithValidation(XName name, object value, string propertyName, SimpleTypeValidator typeDef)
		{
			object typedValue;
			SimpleTypeValidator matchingType = null;
			Exception e = typeDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.Untyped), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			ListSimpleTypeValidator listDef = typeDef as ListSimpleTypeValidator;
			Debug.Assert(listDef != null);
			this.SetListElement(name, typedValue, listDef.ItemType.DataType);
		}

		protected void SetListValue(object value, XmlSchemaDatatype datatype)
		{
			string strValue = ListSimpleTypeValidator.ToString(value);
			this.GetUntyped().Value = strValue;
		}

		protected void SetListValueWithValidation(object value, string propertyName, SimpleTypeValidator typeDef)
		{
			object typedValue;
			Debug.Assert(typeDef is ListSimpleTypeValidator);
			SimpleTypeValidator matchingType = null;
			Exception e = typeDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.GetUntyped()), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			this.SetListValue(value, matchingType.DataType);
		}

		protected void SetUnionAttribute(object value, string propertyName, XTypedElement container, XName itemXName, SimpleTypeValidator typeDef)
		{
			this.SetUnionCatchAll(value, propertyName, container, itemXName, typeDef, SchemaOrigin.Attribute);
		}

		private void SetUnionCatchAll(object value, string propertyName, XTypedElement container, XName itemXName, SimpleTypeValidator typeDef, SchemaOrigin origin)
		{
			object typedValue;
			UnionSimpleTypeValidator unionDef = typeDef as UnionSimpleTypeValidator;
			Debug.Assert(unionDef != null);
			SimpleTypeValidator matchingType = null;
			Exception e = unionDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(container.GetUntyped()), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			if (!(matchingType is ListSimpleTypeValidator))
			{
				switch (origin)
				{
					case SchemaOrigin.Element:
					{
						this.SetElement(itemXName, value, matchingType.DataType);
						break;
					}
					case SchemaOrigin.Attribute:
					{
						this.SetAttribute(itemXName, value, matchingType.DataType);
						break;
					}
					case SchemaOrigin.Fragment:
					{
						break;
					}
					case SchemaOrigin.Text:
					{
						this.SetValue(value, matchingType.DataType);
						break;
					}
					default:
					{
						goto case SchemaOrigin.Fragment;
					}
				}
			}
			else
			{
				ListSimpleTypeValidator listType = matchingType as ListSimpleTypeValidator;
				switch (origin)
				{
					case SchemaOrigin.Element:
					{
						this.SetListElement(itemXName, value, listType.ItemType.DataType);
						break;
					}
					case SchemaOrigin.Attribute:
					{
						this.SetListAttribute(itemXName, value, listType.ItemType.DataType);
						break;
					}
					case SchemaOrigin.Fragment:
					{
						break;
					}
					case SchemaOrigin.Text:
					{
						this.SetListValue(value, listType.ItemType.DataType);
						break;
					}
					default:
					{
						goto case SchemaOrigin.Fragment;
					}
				}
			}
		}

		protected void SetUnionElement(object value, string propertyName, XTypedElement container, XName itemXName, SimpleTypeValidator typeDef)
		{
			this.SetUnionCatchAll(value, propertyName, container, itemXName, typeDef, SchemaOrigin.Element);
		}

		protected void SetUnionValue(object value, string propertyName, XTypedElement container, SimpleTypeValidator typeDef)
		{
			this.SetUnionCatchAll(value, propertyName, this, null, typeDef, SchemaOrigin.Text);
		}

		protected void SetValue(object value, XmlSchemaDatatype datatype)
		{
			Debug.Assert(!(value is XTypedElement), "Cannot set an XTypedElement value as type of simple typed root element");
			XElement element = this.GetUntyped();
			element.Value = XTypedServices.GetXmlString(value, datatype, element);
		}

		protected void SetValueWithValidation(object value, string propertyName, SimpleTypeValidator simpleType)
		{
			Debug.Assert(!(value is XTypedElement), "Cannot set an XTypedElement value as type of simple typed root element");
			object typedValue = null;
			SimpleTypeValidator matchingType = null;
			Exception e = simpleType.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(this.GetUntyped()), out matchingType, out typedValue);
			if (e != null)
			{
				throw new LinqToXsdException(propertyName, e.Message);
			}
			this.SetValue(typedValue, simpleType.DataType);
		}

		internal void StartFsm()
		{
			if (this.ValidationStates != null)
			{
				this.currentState = this.ValidationStates.Start;
			}
		}

		XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
		{
			return null;
		}

		void System.Xml.Serialization.IXmlSerializable.ReadXml(XmlReader reader)
		{
			XElement deserializedElement = new XElement(((IXMetaData)this).SchemaName);
			((IXmlSerializable)deserializedElement).ReadXml(reader);
			this.Untyped = deserializedElement;
		}

		void System.Xml.Serialization.IXmlSerializable.WriteXml(XmlWriter writer)
		{
			this.Untyped.WriteTo(writer);
		}

		public override string ToString()
		{
			return this.Untyped.ToString();
		}

		private XTypedElement TypeChildElement(XElement element, Dictionary<XName, Type> localElementsDict, ILinqToXsdTypeManager typeManager)
		{
			Type clrType = null;
			XTypedElement childTypedElement = null;
			if (!localElementsDict.TryGetValue(element.Name, out clrType))
			{
				childTypedElement = XTypedServices.ToXTypedElement(element, typeManager);
			}
			else
			{
				Type contentType = null;
				if (typeManager.RootContentTypeMapping.TryGetValue(clrType, out contentType))
				{
					childTypedElement = XTypedServices.ToXTypedElement(element, typeManager, clrType, contentType);
				}
				else if (typeof(XTypedElement).IsAssignableFrom(clrType))
				{
					childTypedElement = XTypedServices.ToXTypedElement(element, typeManager, clrType);
				}
			}
			return childTypedElement;
		}

		ContentModelEntity Xml.Schema.Linq.IXMetaData.GetContentModel()
		{
			return ContentModelEntity.Default;
		}

		FSM Xml.Schema.Linq.IXMetaData.GetValidationStates()
		{
			return null;
		}

		IEnumerable<T> Xml.Schema.Linq.IXTyped.Ancestors<T>()
		{
			for (XTypedElement i = this.getTypedParent(); i != null; i = i.getTypedParent())
			{
				T t = (T)(i as T);
				if (t != null)
				{
					yield return t;
				}
			}
		}

		IEnumerable<T> Xml.Schema.Linq.IXTyped.Descendants<T>()
		{
			bool flag;
			bool flag1;
			bool flag2;
			XTypedElement xTypedElement = this;
			Type type = typeof(T);
			IXMetaData xMetaDatum = xTypedElement;
			Dictionary<XName, Type> localElementsDictionary = null;
			ILinqToXsdTypeManager linqToXsdTypeManager = xMetaDatum.TypeManager;
			Dictionary<XName, Type> globalTypeDictionary = linqToXsdTypeManager.GlobalTypeDictionary;
			XName xName = null;
			WildCard wildCard = null;
			int invalidState = FSM.InvalidState;
			XElement untyped = null;
			Stack<XTypedElement> xTypedElements = new Stack<XTypedElement>();
			while (true)
			{
				xMetaDatum = xTypedElement;
				FSM validationStates = xTypedElement.ValidationStates;
				if (validationStates != null)
				{
					this.StartFsm();
					invalidState = validationStates.Start;
				}
				Debug.Assert(xMetaDatum != null);
				localElementsDictionary = xMetaDatum.LocalElementsDictionary;
				untyped = xTypedElement.Untyped;
				xName = null;
				wildCard = null;
				XTypedElement xTypedElement1 = null;
				bool flag3 = true;
				IEnumerator<XElement> enumerator = untyped.Elements().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XElement current = enumerator.Current;
						bool flag4 = this.IsAnnoatedElemTypeOf<T>(current, out xTypedElement1);
						if (validationStates != null)
						{
							invalidState = this.FsmMakeTransition(invalidState, current.Name, out xName, out wildCard);
							if (invalidState == FSM.InvalidState)
							{
								goto Label0;
							}
						}
						if (!flag4)
						{
							flag = (validationStates == null ? true : wildCard == null);
							xTypedElement1 = (flag ? this.TypeChildElement(current, localElementsDictionary, linqToXsdTypeManager) : XTypedServices.ToXTypedElement(current, linqToXsdTypeManager));
							if (xTypedElement1 != null)
							{
								Type type1 = xTypedElement1.GetType();
								if (!type.IsAssignableFrom(type1))
								{
									Type type2 = null;
									flag1 = (!linqToXsdTypeManager.RootContentTypeMapping.TryGetValue(type1, out type2) ? true : !type.IsAssignableFrom(type2));
									if (!flag1)
									{
										xTypedElement1 = this.GetContentType(xTypedElement1);
										flag4 = true;
									}
								}
								else
								{
									flag4 = true;
								}
							}
						}
						if (flag4)
						{
							yield return (T)xTypedElement1;
						}
						if (xTypedElement1 != null)
						{
							xTypedElements.Push(xTypedElement1);
						}
					}
					goto Label1;
				Label0:
					flag3 = false;
				}
				finally
				{
					if (enumerator != null)
					{
						enumerator.Dispose();
					}
				}
			Label1:
				flag2 = (!flag3 ? true : xTypedElements.Count <= 0);
				if (flag2)
				{
					break;
				}
				xTypedElement = xTypedElements.Pop();
			}
		}

		IEnumerable<T> Xml.Schema.Linq.IXTyped.SelfAndAncestors<T>()
		{
			T t = (T)(this as T);
			if (t != null)
			{
				yield return t;
			}
			foreach (T t1 in this.Query.Ancestors<T>())
			{
				yield return t1;
			}
		}

		IEnumerable<T> Xml.Schema.Linq.IXTyped.SelfAndDescendants<T>()
		{
			T t = (T)(this as T);
			if (t != null)
			{
				yield return t;
			}
			foreach (T t1 in this.Query.Descendants<T>())
			{
				yield return t1;
			}
		}
	}
}