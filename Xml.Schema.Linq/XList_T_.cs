using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
    public abstract class XList<T> : XListVisualizable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ICountAndCopy
    {
        internal XTypedElement container;

        internal XElement containerElement;

        internal XName itemXName;

        internal XName[] namesInList;

        public T this[int index]
        {
            get
            {
                int count = 0;
                XElement element = this.GetElementAt(index, out count);
                return this.GetValueForElement(element);
            }
            set
            {
                int count = 0;
                XElement oldElement = this.GetElementAt(index, out count);
                Debug.Assert(oldElement != null);
                this.UpdateElement(oldElement, value);
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
                while (listElementsEnumerator.MoveNext())
                {
                    count++;
                }
                return count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        protected XList(XTypedElement container, params XName[] names)
        {
            this.container = container;
            this.containerElement = container.Untyped;
            this.namesInList = names;
            this.itemXName = names[names.Length - 1];
        }

        public int IndexOf(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Argument value should not be null.");
            }
            return this.GetIndexOf(value);
        }

        public void Insert(int index, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Argument value should not be null.");
            }
            int count = 0;
            XElement prevElement = this.GetElementAt(index, out count);
            if (index < 0 || index > count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (index == count)
            {
                Debug.Assert(prevElement == null);
                this.Add(value);
            }
            else
            {
                Debug.Assert(prevElement != null);
                XElement elementToAdd = this.GetElementForValue(value, true);
                prevElement.AddBeforeSelf(elementToAdd);
            }
        }

        public void RemoveAt(int index)
        {
            int count = 0;
            XElement elementToRemove = this.GetElementAt(index, out count);
            Debug.Assert(elementToRemove != null);
            elementToRemove.Remove();
        }

        public bool Remove(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Argument value should not be null.");
            }
            XElement element = this.GetElementForValue(value, false);
            XElement x = (from e in this.containerElement.Elements(element.Name)
                          where e == element
                          select e).FirstOrDefault<XElement>();
            bool result;
            if (x != null)
            {
                element.Remove();
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public virtual void Add(T value)
        {
            XElement element = this.GetElementForValue(value, true);
            this.container.SetElement(element.Name, value, true, null);
        }

        public void Clear()
        {
            ArrayList elementArray = new ArrayList();
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            while (listElementsEnumerator.MoveNext())
            {
                elementArray.Add(listElementsEnumerator.Current);
            }
            foreach (XElement listElement in elementArray)
            {
                listElement.Remove();
            }
        }

        public void CopyTo(T[] valuesArray, int arrayIndex)
        {
            if (valuesArray == null)
            {
                throw new ArgumentNullException("Argument valuesArray should not be null.");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (valuesArray.Rank != 1 || arrayIndex >= valuesArray.Length)
            {
                throw new ArgumentException("valuesArray");
            }
            int index = arrayIndex;
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            while (listElementsEnumerator.MoveNext())
            {
                if (index > valuesArray.Length)
                {
                    throw new ArgumentException("valuesArray");
                }
                valuesArray[index++] = this.GetValueForElement(listElementsEnumerator.Current);
            }
        }

        void ICountAndCopy.CopyTo(Array valuesArray, int arrayIndex)
        {
            if (valuesArray == null)
            {
                throw new ArgumentNullException("Argument valuesArray should not be null.");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (valuesArray.Rank != 1 || arrayIndex >= valuesArray.Length)
            {
                throw new ArgumentException("valuesArray");
            }
            int index = arrayIndex;
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            while (listElementsEnumerator.MoveNext())
            {
                if (index > valuesArray.Length)
                {
                    throw new ArgumentException("valuesArray");
                }
                valuesArray.SetValue(this.GetValueForElement(listElementsEnumerator.Current), index++);
            }
        }

        public bool Contains(T value)
        {
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            bool result;
            while (listElementsEnumerator.MoveNext())
            {
                if (this.IsEqual(listElementsEnumerator.Current, value))
                {
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            while (listElementsEnumerator.MoveNext())
            {
                yield return this.GetValueForElement(listElementsEnumerator.Current);
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected abstract bool IsEqual(XElement element, T value);

        protected abstract XElement GetElementForValue(T value, bool createNew);

        protected abstract T GetValueForElement(XElement element);

        protected abstract void UpdateElement(XElement oldElement, T value);

        protected XElement GetElementAt(int index, out int count)
        {
            count = 0;
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            XElement result;
            while (listElementsEnumerator.MoveNext())
            {
                if (count++ == index)
                {
                    result = listElementsEnumerator.Current;
                    return result;
                }
            }
            result = null;
            return result;
        }

        protected int GetIndexOf(T value)
        {
            int currentIndex = 0;
            IEnumerator<XElement> listElementsEnumerator = this.GetListElementsEnumerator();
            int result;
            while (listElementsEnumerator.MoveNext())
            {
                if (this.IsEqual(listElementsEnumerator.Current, value))
                {
                    result = currentIndex;
                    return result;
                }
                currentIndex++;
            }
            result = -1;
            return result;
        }

        protected IEnumerator<XElement> GetListElementsEnumerator()
        {
            IEnumerator<XElement> result;
            if (this.container.ValidationStates == null)
            {
                if (this.namesInList.Length == 1)
                {
                    result = this.containerElement.Elements(this.itemXName).GetEnumerator();
                }
                else
                {
                    result = new SubstitutionMembersList(this.container, this.namesInList).GetEnumerator();
                }
            }
            else if (this.namesInList.Length == 1)
            {
                result = this.FSMGetEnumerator();
            }
            else
            {
                result = new SubstitutionMembersList(this.container, this.namesInList).FSMGetEnumerator();
            }
            return result;
        }

        private IEnumerator<XElement> FSMGetEnumerator()
        {
            IEnumerator<XElement> enumerator = this.containerElement.Elements().GetEnumerator();
            XElement xElement = null;
            this.container.StartFsm();
            do
            {
                xElement = this.container.ExecuteFSM(enumerator, this.itemXName, null);
                if (xElement == null)
                {
                    break;
                }
                yield return xElement;
            }
            while (xElement != null);
            yield break;
        }
    }
}
