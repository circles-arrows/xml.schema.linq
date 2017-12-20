using System;
using System.Collections.Generic;

namespace Xml.Schema.Linq
{
	public class FSM
	{
		internal static int InvalidState;

		private readonly int startState;

		private readonly Set<int> acceptStates;

		private readonly IDictionary<int, Transitions> trans;

		public Set<int> Accept
		{
			get
			{
				return this.acceptStates;
			}
		}

		public int Start
		{
			get
			{
				return this.startState;
			}
		}

		public IDictionary<int, Transitions> Trans
		{
			get
			{
				return this.trans;
			}
		}

		static FSM()
		{
			FSM.InvalidState = 0;
		}

		public FSM(int startState, Set<int> acceptStates, IDictionary<int, Transitions> trans)
		{
			this.startState = startState;
			this.acceptStates = acceptStates;
			this.trans = trans;
		}

		internal void AddTransitions(FSM otherFSM)
		{
			foreach (KeyValuePair<int, Transitions> pair in otherFSM.Trans)
			{
				this.trans.Add(pair);
			}
		}

		internal static void CloneTransitions(FSM srcFsm, int srcState, FSM destFsm, int destState)
		{
			Transitions srcTrans = null;
			srcFsm.Trans.TryGetValue(srcState, out srcTrans);
			if (srcTrans != null)
			{
				Transitions destTrans = null;
				destFsm.Trans.TryGetValue(destState, out destTrans);
				if (destTrans == null)
				{
					destTrans = new Transitions();
					destFsm.Trans.Add(destState, destTrans);
				}
				destTrans.CloneTransitions(srcTrans, srcState, destState);
			}
		}

		public bool isAccept(int state)
		{
			return this.acceptStates.Contains(state);
		}

		public override string ToString()
		{
			object[] objArray = new object[] { "DFA start=", this.startState, "\naccept=", this.acceptStates };
			return string.Concat(objArray);
		}
	}
}