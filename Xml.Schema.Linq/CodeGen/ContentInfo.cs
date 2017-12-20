using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class ContentInfo
	{
		internal ContentInfo lastChild;

		internal ContentInfo nextSibling;

		protected Xml.Schema.Linq.CodeGen.ContentType contentType;

		protected Occurs occursInSchema;

		internal IEnumerable<ContentInfo> Children
		{
			get
			{
				ContentInfo contentInfo = this.lastChild;
				while (true)
				{
					if (contentInfo != null)
					{
						contentInfo = contentInfo.nextSibling;
						yield return contentInfo;
						if (contentInfo == this.lastChild)
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
			}
		}

		internal Xml.Schema.Linq.CodeGen.ContentType ContentType
		{
			get
			{
				return this.contentType;
			}
		}

		internal bool IsOptional
		{
			get
			{
				return (this.IsQMark ? true : this.IsStar);
			}
		}

		internal bool IsPlus
		{
			get
			{
				return this.occursInSchema == Occurs.OneOrMore;
			}
		}

		internal bool IsQMark
		{
			get
			{
				return this.occursInSchema == Occurs.ZeroOrOne;
			}
		}

		internal bool IsStar
		{
			get
			{
				return this.occursInSchema == Occurs.ZeroOrMore;
			}
		}

		internal Occurs OccursInSchema
		{
			get
			{
				return this.occursInSchema;
			}
		}

		protected ContentInfo()
		{
		}

		internal void AddChild(ContentInfo content)
		{
			if (this.lastChild != null)
			{
				content.nextSibling = this.lastChild.nextSibling;
				this.lastChild.nextSibling = content;
			}
			else
			{
				content.nextSibling = content;
			}
			this.lastChild = content;
		}

		private bool HasNextStates(int state, FSM fsm)
		{
			Transitions currTrans = null;
			fsm.Trans.TryGetValue(state, out currTrans);
			return ((currTrans == null ? false : currTrans.Count != 0) ? true : false);
		}

		internal FSM ImplementFSMCardinality(FSM origFsm, StateNameSource stateNames)
		{
			Debug.Assert(origFsm != null);
			FSM fsm = null;
			switch (this.OccursInSchema)
			{
				case Occurs.ZeroOrOne:
				{
					fsm = this.MakeQMarkFSM(origFsm, stateNames);
					break;
				}
				case Occurs.ZeroOrMore:
				{
					fsm = this.MakeStarFSM(origFsm, stateNames);
					break;
				}
				case Occurs.OneOrMore:
				{
					fsm = this.MakePlusFSM(origFsm, stateNames);
					break;
				}
				default:
				{
					fsm = origFsm;
					break;
				}
			}
			return fsm;
		}

		internal virtual FSM MakeFSM(StateNameSource stateNames)
		{
			throw new InvalidOperationException();
		}

		private FSM MakePlusFSM(FSM origFsm, StateNameSource stateNames)
		{
			int origStart = origFsm.Start;
			foreach (int s in origFsm.Accept)
			{
				if (s != origStart)
				{
					FSM.CloneTransitions(origFsm, origStart, origFsm, s);
				}
			}
			return origFsm;
		}

		private FSM MakeQMarkFSM(FSM origFsm, StateNameSource stateNames)
		{
			origFsm.Accept.Add(origFsm.Start);
			return origFsm;
		}

		private FSM MakeStarFSM(FSM origFsm, StateNameSource stateNames)
		{
			int start = origFsm.Start;
			this.TransformToStar(start, start, origFsm, new Set<int>());
			origFsm.Accept.Add(start);
			return origFsm;
		}

		private void SimulatePlusQMark(FSM fsm, int start, int currState)
		{
			if (currState != start)
			{
				FSM.CloneTransitions(fsm, start, fsm, currState);
			}
		}

		private void TransformToStar<T>(Dictionary<T, int> transitions, int startState, int currentState, FSM fsm, Set<int> visited)
		{
			if (transitions != null)
			{
				List<T> toReroute = new List<T>();
				foreach (KeyValuePair<T, int> transition in transitions)
				{
					int nextState = transition.Value;
					bool hasNextStates = (currentState == nextState ? false : this.HasNextStates(nextState, fsm));
					if (fsm.isAccept(nextState))
					{
						if (!hasNextStates)
						{
							toReroute.Add(transition.Key);
						}
						else if (!visited.Contains(nextState))
						{
							this.SimulatePlusQMark(fsm, startState, nextState);
						}
					}
					if (hasNextStates)
					{
						this.TransformToStar(startState, nextState, fsm, visited);
					}
				}
				foreach (T id in toReroute)
				{
					transitions[id] = startState;
				}
			}
		}

		private void TransformToStar(int start, int currState, FSM fsm, Set<int> visited)
		{
			if (!visited.Contains(currState))
			{
				visited.Add(currState);
				Transitions currTrans = null;
				fsm.Trans.TryGetValue(currState, out currTrans);
				if ((currTrans == null ? false : currTrans.Count != 0))
				{
					this.TransformToStar<XName>(currTrans.nameTransitions, start, currState, fsm, visited);
					this.TransformToStar<WildCard>(currTrans.wildCardTransitions, start, currState, fsm, visited);
				}
			}
		}
	}
}