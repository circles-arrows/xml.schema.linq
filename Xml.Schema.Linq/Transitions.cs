using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class Transitions
	{
		internal Dictionary<XName, int> nameTransitions;

		internal Dictionary<WildCard, int> wildCardTransitions;

		internal int Count
		{
			get
			{
				int count = 0;
				if (this.nameTransitions != null)
				{
					count += this.nameTransitions.Count;
				}
				if (this.wildCardTransitions != null)
				{
					count += this.wildCardTransitions.Count;
				}
				return count;
			}
		}

		internal bool IsEmpty
		{
			get
			{
				return this.Count == 0;
			}
		}

		public Transitions()
		{
		}

		public Transitions(params SingleTransition[] transitions)
		{
			if (transitions != null)
			{
				SingleTransition[] singleTransitionArray = transitions;
				for (int i = 0; i < (int)singleTransitionArray.Length; i++)
				{
					SingleTransition st = singleTransitionArray[i];
					if (!(st.nameLabel != null))
					{
						if (this.wildCardTransitions == null)
						{
							this.wildCardTransitions = new Dictionary<WildCard, int>();
						}
						this.wildCardTransitions.Add(st.wcLabel, st.nextState);
					}
					else
					{
						if (this.nameTransitions == null)
						{
							this.nameTransitions = new Dictionary<XName, int>();
						}
						this.nameTransitions.Add(st.nameLabel, st.nextState);
					}
				}
			}
		}

		public Transitions(Dictionary<XName, int> nameTrans, Dictionary<WildCard, int> wildCardTrans)
		{
			this.nameTransitions = nameTrans;
			this.wildCardTransitions = wildCardTrans;
		}

		internal static void Add<T>(ref Dictionary<T, int> d, T id, int nextState)
		{
			if (d == null)
			{
				d = new Dictionary<T, int>();
			}
			d[id] = nextState;
		}

		internal void Add(XName name, int nextState)
		{
			Transitions.Add<XName>(ref this.nameTransitions, name, nextState);
		}

		internal void Add(WildCard wildCard, int nextState)
		{
			Transitions.Add<WildCard>(ref this.wildCardTransitions, wildCard, nextState);
		}

		internal void Clear()
		{
			if (this.nameTransitions != null)
			{
				this.nameTransitions.Clear();
			}
			if (this.wildCardTransitions != null)
			{
				this.wildCardTransitions.Clear();
			}
		}

		internal void CloneTransitions(Transitions otherTransitions, int srcState, int destState)
		{
			int nextState;
			bool isEmpty = this.IsEmpty;
			if (otherTransitions.nameTransitions != null)
			{
				if (this.nameTransitions == null)
				{
					this.nameTransitions = new Dictionary<XName, int>();
				}
				foreach (KeyValuePair<XName, int> pair in otherTransitions.nameTransitions)
				{
					nextState = pair.Value;
					if ((!isEmpty ? false : nextState == srcState))
					{
						nextState = destState;
					}
					this.nameTransitions[pair.Key] = nextState;
				}
			}
			if (otherTransitions.wildCardTransitions != null)
			{
				if (this.wildCardTransitions == null)
				{
					this.wildCardTransitions = new Dictionary<WildCard, int>();
				}
				foreach (KeyValuePair<WildCard, int> pair in otherTransitions.wildCardTransitions)
				{
					nextState = pair.Value;
					if ((!isEmpty ? false : nextState == srcState))
					{
						nextState = destState;
					}
					this.wildCardTransitions[pair.Key] = nextState;
				}
			}
		}

		internal int GetNextState(XName inputSymbol, out XName matchingName, out WildCard matchingWildCard)
		{
			matchingWildCard = null;
			matchingName = null;
			int state = FSM.InvalidState;
			if (!(this.nameTransitions == null ? true : !this.nameTransitions.TryGetValue(inputSymbol, out state)))
			{
				matchingName = inputSymbol;
			}
			else if (this.wildCardTransitions != null)
			{
				foreach (KeyValuePair<WildCard, int> pair in this.wildCardTransitions)
				{
					if (pair.Key.Allows(inputSymbol))
					{
						matchingWildCard = pair.Key;
						state = pair.Value;
					}
				}
			}
			return state;
		}
	}
}