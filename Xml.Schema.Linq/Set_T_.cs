using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xml.Schema.Linq
{
	public class Set<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private Dictionary<T, bool> dictionary;

		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public Set()
		{
			this.dictionary = new Dictionary<T, bool>();
		}

		public Set(T x) : this()
		{
			this.Add(x);
		}

		public Set(IEnumerable<T> collection) : this()
		{
			foreach (T x in collection)
			{
				this.Add(x);
			}
		}

		public Set(T[] array) : this()
		{
			T[] tArray = array;
			for (int i = 0; i < (int)tArray.Length; i++)
			{
				this.Add(tArray[i]);
			}
		}

		public void Add(T x)
		{
			if (!this.Contains(x))
			{
				this.dictionary.Add(x, false);
			}
		}

		public void Clear()
		{
			this.dictionary.Clear();
		}

		public bool Contains(T x)
		{
			return this.dictionary.ContainsKey(x);
		}

		public void CopyTo(T[] arr, int i)
		{
			this.dictionary.Keys.CopyTo(arr, i);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.dictionary.Keys.GetEnumerator();
		}

		public bool Remove(T x)
		{
			return this.dictionary.Remove(x);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append("{ ");
			bool first = true;
			foreach (T x in this)
			{
				if (!first)
				{
					str.Append(", ");
				}
				str.Append(x);
				first = false;
			}
			str.Append(" }");
			return str.ToString();
		}
	}
}