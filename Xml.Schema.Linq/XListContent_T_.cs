using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class XListContent<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		internal XElement containerElement;

		private XName itemXName;

		private List<T> items;

		private XmlSchemaDatatype datatype;

		private ContainerType containerType;

		public int Count
		{
			get
			{
				return this.items.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public T this[int index]
		{
			get
			{
				return this.items[index];
			}
			set
			{
				this.items[index] = value;
				this.SaveValue();
			}
		}

		public XListContent(string value, XElement containerElement, XName name, ContainerType type, XmlSchemaDatatype datatype)
		{
			this.containerElement = containerElement;
			this.itemXName = name;
			this.datatype = datatype;
			this.containerType = type;
			this.GenerateList(value);
		}

		public XListContent(IList value, XElement containerElement, XName name, ContainerType type, XmlSchemaDatatype datatype)
		{
			this.containerElement = containerElement;
			this.itemXName = name;
			this.datatype = datatype;
			this.containerType = type;
			this.CopyList(value);
		}

		public void Add(T value)
		{
			this.items.Add(value);
			this.SaveValue();
		}

		public void Clear()
		{
			this.items.Clear();
			this.SaveValue();
		}

		public bool Contains(T value)
		{
			return this.items.Contains(value);
		}

		private void CopyList(IList value)
		{
			if (this.items != null)
			{
				this.items.Clear();
			}
			else
			{
				this.items = new List<T>();
			}
			foreach (T t in value)
			{
				this.items.Add(t);
			}
		}

		public void CopyTo(T[] valuesArray, int arrayIndex)
		{
			this.items.CopyTo(valuesArray, arrayIndex);
		}

		internal void GenerateList(string value)
		{
			string[] strs = value.Split(new char[] { ' ' });
			if ((value == string.Empty ? false : (int)strs.Length != 0))
			{
				this.items = new List<T>((int)strs.Length);
				string[] strArrays = strs;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string item = strArrays[i];
					this.items.Add(XTypedServices.ParseValue<T>(item, this.containerElement, this.datatype));
				}
			}
			else
			{
				this.items = new List<T>();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.items.GetEnumerator();
		}

		public int IndexOf(T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("Argument value should not be null.");
			}
			return this.items.IndexOf(value);
		}

		public void Insert(int index, T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("Argument value should not be null.");
			}
			this.items.Insert(index, value);
			this.SaveValue();
		}

		public bool Remove(T value)
		{
			bool flag;
			if (value == null)
			{
				throw new ArgumentNullException("Argument value should not be null.");
			}
			if (!this.items.Remove(value))
			{
				flag = false;
			}
			else
			{
				this.SaveValue();
				flag = true;
			}
			return flag;
		}

		public void RemoveAt(int index)
		{
			this.items.RemoveAt(index);
			this.SaveValue();
		}

		private void SaveValue()
		{
			switch (this.containerType)
			{
				case ContainerType.Attribute:
				{
					XAttribute attr = this.containerElement.Attribute(this.itemXName);
					Debug.Assert(attr != null);
					attr.Value = ListSimpleTypeValidator.ToString(this.items);
					break;
				}
				case ContainerType.Element:
				{
					this.containerElement.Value = ListSimpleTypeValidator.ToString(this.items);
					break;
				}
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}