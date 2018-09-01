using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public static class XTypedServices
	{
		public readonly static Dictionary<XName, Type> EmptyDictionary;

		public readonly static Dictionary<Type, Type> EmptyTypeMappingDictionary;

		internal readonly static Type typeOfString;

		private static System.Xml.NameTable nameTable;

		internal static System.Xml.NameTable NameTable
		{
			get
			{
				if (XTypedServices.nameTable == null)
				{
					Interlocked.CompareExchange<System.Xml.NameTable>(ref XTypedServices.nameTable, new System.Xml.NameTable(), null);
				}
				return XTypedServices.nameTable;
			}
		}

		static XTypedServices()
		{
			XTypedServices.EmptyDictionary = new Dictionary<XName, Type>();
			XTypedServices.EmptyTypeMappingDictionary = new Dictionary<Type, Type>();
			XTypedServices.typeOfString = typeof(string);
		}

		public static T CloneXTypedElement<T>(T xTypedElement)
		    where T : XTypedElement, new()
		{
			if (xTypedElement == null)
			{
				throw new ArgumentNullException("Argument xTypedElement should not be null.");
			}
			XElement clonedElement = new XElement(xTypedElement.Untyped);
			T newObject = Activator.CreateInstance<T>();
			newObject.Untyped = clonedElement;
			clonedElement.AddAnnotation(new XTypedElementAnnotation(newObject));
			return newObject;
		}

		internal static T GetAnnotation<T>(XElement xe)
		    where T : XTypedElement
		{
			return (T)XTypedServices.GetAnnotation(typeof(T), xe);
		}

		internal static XTypedElement GetAnnotation(Type t, XElement xe)
		{
			XTypedElement xTypedElement;
			XTypedElementWrapperAnnotation xoWrapperAnnotation = xe.Annotation<XTypedElementWrapperAnnotation>();
			XTypedElement xObj = null;
			if (xoWrapperAnnotation != null)
			{
				xObj = xoWrapperAnnotation.typedElement;
				if (t.IsAssignableFrom(xObj.GetType()))
				{
					xTypedElement = xObj;
					return xTypedElement;
				}
			}
			XTypedElementAnnotation xoAnnotation = xe.Annotation<XTypedElementAnnotation>();
			if (xoAnnotation != null)
			{
				xObj = xoAnnotation.typedElement;
				if (t.IsAssignableFrom(xObj.GetType()))
				{
					xTypedElement = xObj;
					return xTypedElement;
				}
			}
			xTypedElement = null;
			return xTypedElement;
		}

		public static XTypedElement GetCloneIfRooted(XTypedElement innerType)
		{
			XTypedElement xTypedElement;
			if (innerType == null)
			{
				throw new ArgumentNullException("Argument innerType should not be null.");
			}
			xTypedElement = (innerType.Untyped.Annotation<XTypedElementWrapperAnnotation>() == null ? innerType : innerType.Clone());
			return xTypedElement;
		}

		internal static XElement GetXElement(XTypedElement xObj, XName name)
		{
			XElement newElement = xObj.Untyped;
			if (newElement.Parent != null)
			{
				newElement = xObj.Clone().Untyped;
			}
			IXMetaData metaData = xObj;
			Debug.Assert(metaData != null);
			if (metaData.TypeOrigin == SchemaOrigin.Fragment)
			{
				newElement.Name = name;
			}
			return newElement;
		}

		internal static string GetXmlString(object value, XmlSchemaDatatype datatype, XElement element)
		{
			string stringValue = null;
			if (datatype.TypeCode != XmlTypeCode.QName)
			{
				stringValue = (string)datatype.ChangeType(value, XTypedServices.typeOfString);
			}
			else
			{
				XmlQualifiedName qName = value as XmlQualifiedName;
				Debug.Assert(qName != null);
				stringValue = XTypedServices.QNameToString(qName, element);
			}
			return stringValue;
		}

		internal static Type GetXsiClrType(XElement xe, ILinqToXsdTypeManager typeManager)
		{
			XName typeName = XTypedServices.GetXsiType(xe);
			Type clrType = null;
			if (typeName != null)
			{
				typeManager.GlobalTypeDictionary.TryGetValue(typeName, out clrType);
			}
			return clrType;
		}

		internal static XName GetXsiType(XElement xe)
		{
			XName xName;
			string type = (string)xe.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"));
			if (type == null)
			{
				xName = null;
			}
			else
			{
				string prefix = string.Empty;
				string localName = XTypedServices.ParseQName(type, out prefix);
				XNamespace ns = null;
				ns = (prefix.Length != 0 ? xe.GetNamespaceOfPrefix(prefix) : xe.GetDefaultNamespace());
				xName = (!(ns != null) ? XName.Get(localName) : ns.GetName(localName));
			}
			return xName;
		}

		public static T Load<T>(TextReader reader)
		where T : XTypedElement, new()
		{
			return XTypedServices.ToXTypedElement<T>(XDocument.Load(reader).Root);
		}

		public static T Load<T>(string uri)
		where T : XTypedElement, new()
		{
			return XTypedServices.ToXTypedElement<T>(XDocument.Load(uri).Root);
		}

		public static W Load<W, T>(string uri, ILinqToXsdTypeManager typeManager)
		where W : XTypedElement
		where T : XTypedElement
		{
			XDocument doc = XDocument.Load(uri);
			return XTypedServices.ToXTypedElement<W, T>(doc.Root, typeManager);
		}

		public static W Load<W, T>(TextReader reader, ILinqToXsdTypeManager typeManager)
		where W : XTypedElement
		where T : XTypedElement
		{
			XDocument doc = XDocument.Load(reader);
			return XTypedServices.ToXTypedElement<W, T>(doc.Root, typeManager);
		}

		public static T Parse<T>(string xml)
		where T : XTypedElement, new()
		{
			return XTypedServices.ToXTypedElement<T>(XElement.Parse(xml));
		}

		public static W Parse<W, T>(string xml, ILinqToXsdTypeManager typeManager)
		where W : XTypedElement
		where T : XTypedElement
		{
			return XTypedServices.ToXTypedElement<W, T>(XElement.Parse(xml), typeManager);
		}

		public static IList<T> ParseListValue<T>(XElement element, XmlSchemaDatatype datatype)
		{
			IList<T> ts;
			if (element != null)
			{
				ts = XTypedServices.ParseListValue<T>(element.Value, element, element.Name, ContainerType.Element, datatype);
			}
			else
			{
				ts = null;
			}
			return ts;
		}

		public static IList<T> ParseListValue<T>(XAttribute attribute, XmlSchemaDatatype datatype)
		{
			IList<T> ts;
			if (attribute != null)
			{
				ts = XTypedServices.ParseListValue<T>(attribute.Value, attribute.Parent, attribute.Name, ContainerType.Attribute, datatype);
			}
			else
			{
				ts = null;
			}
			return ts;
		}

		public static IList<T> ParseListValue<T>(XAttribute attribute, XmlSchemaDatatype datatype, IList<T> defaultValue)
		{
			IList<T> ts;
			ts = (attribute != null ? XTypedServices.ParseListValue<T>(attribute.Value, attribute.Parent, attribute.Name, ContainerType.Attribute, datatype) : defaultValue);
			return ts;
		}

		private static IList<T> ParseListValue<T>(string value, XElement element, XName name, ContainerType containerType, XmlSchemaDatatype datatype)
		{
			return new XListContent<T>(value, element, name, containerType, datatype);
		}

		private static string ParseQName(string qName, out string prefix)
		{
			string str;
			prefix = string.Empty;
			int colonPos = qName.IndexOf(':');
			if (colonPos != -1)
			{
				prefix = qName.Substring(0, colonPos);
				str = qName.Substring(colonPos + 1);
			}
			else
			{
				str = qName;
			}
			return str;
		}

		public static object ParseUnionValue(XElement element, SimpleTypeValidator typeDef)
		{
			object obj;
			obj = (element != null ? XTypedServices.ParseUnionValue(element.Value, element, element.Name, typeDef, ContainerType.Element) : null);
			return obj;
		}

		public static object ParseUnionValue(XAttribute attribute, SimpleTypeValidator typeDef)
		{
			object obj;
			obj = (attribute != null ? XTypedServices.ParseUnionValue(attribute.Value, attribute.Parent, attribute.Name, typeDef, ContainerType.Attribute) : null);
			return obj;
		}

		private static object ParseUnionValue(string value, XElement element, XName itemXName, SimpleTypeValidator typeDef, ContainerType containerType)
		{
			object typedValue;
			object objs;
			UnionSimpleTypeValidator unionDef = typeDef as UnionSimpleTypeValidator;
			Debug.Assert(unionDef != null);
			SimpleTypeValidator matchingType = null;
			Exception e = unionDef.TryParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(element), out matchingType, out typedValue);
			ListSimpleTypeValidator listType = matchingType as ListSimpleTypeValidator;
			if (listType == null)
			{
				Debug.Assert(e == null);
				objs = typedValue;
			}
			else
			{
				SimpleTypeValidator itemType = listType.ItemType;
				objs = new XListContent<object>((IList)typedValue, element, itemXName, containerType, itemType.DataType);
			}
			return objs;
		}

		public static T ParseValue<T>(XElement element, XmlSchemaDatatype datatype)
		{
			T t;
			t = (element != null ? XTypedServices.ParseValue<T>(element.Value, element, datatype) : default(T));
			return t;
		}

		public static T ParseValue<T>(XAttribute attribute, XmlSchemaDatatype datatype)
		{
			T t;
			t = (attribute != null ? XTypedServices.ParseValue<T>(attribute.Value, attribute.Parent, datatype) : default(T));
			return t;
		}

		public static T ParseValue<T>(XAttribute attribute, XmlSchemaDatatype datatype, T defaultValue)
		{
			T t;
			t = (attribute != null ? XTypedServices.ParseValue<T>(attribute.Value, attribute.Parent, datatype) : defaultValue);
			return t;
		}

		internal static T ParseValue<T>(string value, XElement element, XmlSchemaDatatype datatype)
		{
			T t;
			t = ((datatype.TypeCode == XmlTypeCode.QName ? false : datatype.TypeCode != XmlTypeCode.NCName) ? ParseValueFast<T>(value, datatype) : (T)datatype.ParseValue(value, XTypedServices.NameTable, new XNamespaceResolver(element)));
			return t;
		}
        private static T ParseValueFast<T>(string value, XmlSchemaDatatype datatype)
        {
            switch (typeof(T).Name)
            {
                case nameof(System.String):
                    return (T)(object)value;
                case nameof(System.Boolean):
                    {
                        switch (value)
                        {
                            case "true":
                            case "1":
                                return (T)(object)true;
                            case "false":
                            case "0":
                                return (T)(object)false;
                        }
                        break;
                    }
                case nameof(System.Int16):
                    {
                        short retval;
                        if (short.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.UInt16):
                    {
                        ushort retval;
                        if (ushort.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.Int32):
                    {
                        int retval;
                        if (int.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.UInt32):
                    {
                        uint retval;
                        if (uint.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.Int64):
                    {
                        long retval;
                        if (long.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.UInt64):
                    {
                        ulong retval;
                        if (ulong.TryParse(value, out retval))
                            return (T)(object)retval;
                        break;
                    }
                case nameof(System.Decimal):
                    {
                        decimal retval;
                        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out retval))
                            return (T)(object)retval;
                        break;
                    }
            }
            return (T)datatype.ChangeType(value, typeof(T));
        }

        internal static string QNameToString(XmlQualifiedName qName, XElement element)
		{
			string str;
			Debug.Assert(qName != null);
			string prefix = element.GetPrefixOfNamespace(qName.Namespace);
			str = ((prefix == null ? false : prefix.Length != 0) ? string.Concat(prefix, ":", qName.Name) : qName.Name);
			return str;
		}

		public static void Save(string xmlFile, XElement xe)
		{
			if (xmlFile == null)
			{
				throw new ArgumentNullException("xmlFile");
			}
			Debug.Assert(xe != null);
			XmlWriterSettings ws = new XmlWriterSettings()
			{
				Indent = true
			};
			XmlWriter w = XmlWriter.Create(xmlFile, ws);
			try
			{
				XTypedServices.Save(w, xe);
			}
			finally
			{
				if (w != null)
				{
					((IDisposable)w).Dispose();
				}
			}
		}

		public static void Save(TextWriter tw, XElement xe)
		{
			if (tw == null)
			{
				throw new ArgumentNullException("tw");
			}
			Debug.Assert(xe != null);
			XmlWriterSettings ws = new XmlWriterSettings()
			{
				Indent = true
			};
			XmlWriter w = XmlWriter.Create(tw, ws);
			try
			{
				XTypedServices.Save(w, xe);
			}
			finally
			{
				if (w != null)
				{
					((IDisposable)w).Dispose();
				}
			}
		}

		public static void Save(XmlWriter xmlWriter, XElement xe)
		{
			if (xmlWriter == null)
			{
				throw new ArgumentNullException("xmlWriter");
			}
			Debug.Assert(xe != null);
			xe.Save(xmlWriter);
		}

		public static void SetContentToNil(XTypedElement xTypedElement)
		{
			XName xsiNil = XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance");
			XElement thisElement = xTypedElement.Untyped;
			thisElement.RemoveNodes();
			if (thisElement.Attribute(xsiNil) == null)
			{
				thisElement.Add(new XAttribute(xsiNil, "true"));
			}
		}

		public static void SetList<T>(IList<T> list, IList<T> value)
		{
			if (list != value)
			{
				list.Clear();
				foreach (T obj in value)
				{
					list.Add(obj);
				}
			}
		}

		public static void SetName(XTypedElement root, XTypedElement type)
		{
			IXMetaData schemaMetaData = root;
			Debug.Assert(schemaMetaData != null);
			XName elementName = schemaMetaData.SchemaName;
			Debug.Assert(elementName != null);
			XElement currentElement = type.Untyped;
			currentElement.Name = elementName;
			currentElement.AddAnnotation(new XTypedElementWrapperAnnotation(root));
			root.Untyped = currentElement;
		}

		public static XTypedElement ToSubstitutedXTypedElement(XTypedElement parentType, ILinqToXsdTypeManager typeManager, params XName[] substitutedMembers)
		{
			XTypedElement xTypedElement;
			XElement substElement = null;
			XElement parentElement = parentType.Untyped;
			int index = 0;
			while (true)
			{
				if ((substElement != null ? true : index >= (int)substitutedMembers.Length))
				{
					break;
				}
				int num = index;
				index = num + 1;
				substElement = parentType.GetElement(substitutedMembers[num]);
			}
			if (substElement == null)
			{
				xTypedElement = null;
			}
			else
			{
				xTypedElement = XTypedServices.ToXTypedElement(substElement, typeManager);
			}
			return xTypedElement;
		}

		public static W ToXTypedElement<W, T>(XElement xe, ILinqToXsdTypeManager typeManager)
		where W : XTypedElement
		where T : XTypedElement
		{
			return (W)XTypedServices.ToXTypedElement(xe, typeManager, typeof(W), typeof(T));
		}

		public static XTypedElement ToXTypedElement(XElement xe, ILinqToXsdTypeManager typeManager, Type rootType, Type contentType)
		{
			XTypedElement rootElement = XTypedServices.GetAnnotation(rootType, xe);
			if (rootElement == null)
			{
				XName instanceElementName = xe.Name;
				XTypedElement innerType = XTypedServices.ToXTypedElement(xe, typeManager, contentType);
				Debug.Assert(innerType != null);
				ConstructorInfo constInfo = rootType.GetConstructor(new Type[] { contentType });
				if (!(constInfo != null))
				{
					throw new LinqToXsdException(string.Concat(contentType.ToString(), " is not an expected content type for root element type ", rootType.ToString()));
				}
				object[] objArray = new object[] { innerType };
				rootElement = (XTypedElement)constInfo.Invoke(objArray);
				if (!XTypedServices.TypeValid(rootElement, instanceElementName))
				{
					throw new LinqToXsdException(string.Concat("Element is not an instance of type ", rootType));
				}
			}
			return rootElement;
		}

		public static T ToXTypedElement<T>(XElement xe)
		where T : XTypedElement, new()
		{
			T t;
			if (xe != null)
			{
				T xoSubType = XTypedServices.GetAnnotation<T>(xe);
				if (xoSubType == null)
				{
					xoSubType = Activator.CreateInstance<T>();
					if (!XTypedServices.TypeValid(xoSubType, xe.Name))
					{
						throw new LinqToXsdException(string.Concat("Element is not an instance of type ", xoSubType.GetType()));
					}
					xoSubType.Untyped = xe;
					xe.AddAnnotation(new XTypedElementAnnotation(xoSubType));
				}
				t = xoSubType;
			}
			else
			{
				t = default(T);
			}
			return t;
		}

		public static T ToXTypedElement<T>(XElement xe, ILinqToXsdTypeManager typeManager)
		where T : XTypedElement
		{
			return (T)(XTypedServices.ToXTypedElement(xe, typeManager, typeof(T)) as T);
		}

		public static XTypedElement ToXTypedElement(XElement xe, ILinqToXsdTypeManager typeManager, Type t)
		{
			XTypedElement xTypedElement;
			if (xe != null)
			{
				if (!t.IsSubclassOf(typeof(XTypedElement)))
				{
					throw new InvalidOperationException("Type t is not a subtype of XTypedElement");
				}
				if (typeManager == null)
				{
					throw new ArgumentNullException("typeManager");
				}
				XTypedElement xoSubType = XTypedServices.GetAnnotation(t, xe);
				if (xoSubType == null)
				{
					Type clrType = XTypedServices.GetXsiClrType(xe, typeManager);
					if (!(clrType != null))
					{
						clrType = t;
					}
					else
					{
						xoSubType = XTypedServices.GetAnnotation(clrType, xe);
						if (xoSubType != null)
						{
							xTypedElement = xoSubType;
							return xTypedElement;
						}
						if (!t.IsAssignableFrom(clrType))
						{
							clrType = t;
						}
					}
					if (clrType.IsAbstract)
					{
						throw new InvalidOperationException("Cannot cast XElement to an abstract type");
					}
					ConstructorInfo constrInfo = clrType.GetConstructor(Type.EmptyTypes);
					xoSubType = (XTypedElement)constrInfo.Invoke(null);
					xoSubType.Untyped = xe;
					xe.AddAnnotation(new XTypedElementAnnotation(xoSubType));
				}
				xTypedElement = xoSubType;
			}
			else
			{
				xTypedElement = null;
			}
			return xTypedElement;
		}

		public static XTypedElement ToXTypedElement(XElement xe, ILinqToXsdTypeManager typeManager)
		{
			XTypedElement xTypedElement;
			if (xe != null)
			{
				XName elementName = xe.Name;
				Type clrType = null;
				if (!typeManager.GlobalElementDictionary.TryGetValue(elementName, out clrType))
				{
					Type xsiClrType = XTypedServices.GetXsiClrType(xe, typeManager);
					if (!(xsiClrType != null))
					{
						xTypedElement = null;
					}
					else
					{
						xTypedElement = XTypedServices.ToXTypedElement(xe, typeManager, xsiClrType);
					}
				}
				else
				{
					Type contentType = null;
					xTypedElement = (!typeManager.RootContentTypeMapping.TryGetValue(clrType, out contentType) ? XTypedServices.ToXTypedElement(xe, typeManager, clrType) : XTypedServices.ToXTypedElement(xe, typeManager, clrType, contentType));
				}
			}
			else
			{
				xTypedElement = null;
			}
			return xTypedElement;
		}

		private static bool TypeValid(XTypedElement typedElement, XName instanceName)
		{
			bool flag;
			IXMetaData metaData = typedElement;
			Debug.Assert(metaData.TypeOrigin == SchemaOrigin.Element);
			flag = (!metaData.SchemaName.Equals(instanceName) ? false : true);
			return flag;
		}
	}
}