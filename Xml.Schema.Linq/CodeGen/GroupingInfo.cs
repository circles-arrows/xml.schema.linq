using System;
using System.Collections.Generic;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class GroupingInfo : ContentInfo
	{
		private Xml.Schema.Linq.ContentModelType contentModelType;

		private GroupingFlags groupingFlags;

		internal Xml.Schema.Linq.ContentModelType ContentModelType
		{
			get
			{
				return this.contentModelType;
			}
		}

		internal bool HasChildGroups
		{
			get
			{
				return (this.groupingFlags & GroupingFlags.HasChildGroups) != GroupingFlags.None;
			}
			set
			{
				if (!value)
				{
					GroupingInfo groupingInfo = this;
					groupingInfo.groupingFlags = groupingInfo.groupingFlags & (GroupingFlags.Nested | GroupingFlags.Repeating | GroupingFlags.HasRepeatingGroups | GroupingFlags.HasRecurrentElements | GroupingFlags.Optional);
				}
				else
				{
					this.groupingFlags |= GroupingFlags.HasChildGroups;
				}
			}
		}

		internal bool HasRecurrentElements
		{
			get
			{
				return (this.groupingFlags & GroupingFlags.HasRecurrentElements) != GroupingFlags.None;
			}
			set
			{
				if (!value)
				{
					GroupingInfo groupingInfo = this;
					groupingInfo.groupingFlags = groupingInfo.groupingFlags & (GroupingFlags.Nested | GroupingFlags.Repeating | GroupingFlags.HasChildGroups | GroupingFlags.HasRepeatingGroups | GroupingFlags.Optional);
				}
				else
				{
					this.groupingFlags |= GroupingFlags.HasRecurrentElements;
				}
			}
		}

		internal bool HasRepeatingGroups
		{
			get
			{
				return (this.groupingFlags & GroupingFlags.HasRepeatingGroups) != GroupingFlags.None;
			}
			set
			{
				if (!value)
				{
					GroupingInfo groupingInfo = this;
					groupingInfo.groupingFlags = groupingInfo.groupingFlags & (GroupingFlags.Nested | GroupingFlags.Repeating | GroupingFlags.HasChildGroups | GroupingFlags.HasRecurrentElements | GroupingFlags.Optional);
				}
				else
				{
					this.groupingFlags |= GroupingFlags.HasRepeatingGroups;
				}
			}
		}

		internal bool IsComplex
		{
			get
			{
				return this.HasChildGroups | this.IsRepeating | this.HasRecurrentElements;
			}
		}

		internal bool IsNested
		{
			get
			{
				return (this.groupingFlags & GroupingFlags.Nested) != GroupingFlags.None;
			}
			set
			{
				if (!value)
				{
					GroupingInfo groupingInfo = this;
					groupingInfo.groupingFlags = groupingInfo.groupingFlags & (GroupingFlags.Repeating | GroupingFlags.HasChildGroups | GroupingFlags.HasRepeatingGroups | GroupingFlags.HasRecurrentElements | GroupingFlags.Optional);
				}
				else
				{
					this.groupingFlags |= GroupingFlags.Nested;
				}
			}
		}

		internal bool IsRepeating
		{
			get
			{
				return (this.groupingFlags & GroupingFlags.Repeating) != GroupingFlags.None;
			}
			set
			{
				if (!value)
				{
					GroupingInfo groupingInfo = this;
					groupingInfo.groupingFlags = groupingInfo.groupingFlags & (GroupingFlags.Nested | GroupingFlags.HasChildGroups | GroupingFlags.HasRepeatingGroups | GroupingFlags.HasRecurrentElements | GroupingFlags.Optional);
				}
				else
				{
					this.groupingFlags |= GroupingFlags.Repeating;
				}
			}
		}

		internal GroupingInfo(Xml.Schema.Linq.ContentModelType cmType, Occurs occursInSchema)
		{
			this.contentModelType = cmType;
			this.occursInSchema = occursInSchema;
			this.contentType = Xml.Schema.Linq.CodeGen.ContentType.Grouping;
			if (occursInSchema > Occurs.ZeroOrOne)
			{
				this.groupingFlags |= GroupingFlags.Repeating;
			}
		}

		private FSM MakeChoiceFSM(StateNameSource stateNames)
		{
			FSM fsm = null;
			int fsmStart = FSM.InvalidState;
			Set<int> fsmAccept = null;
			foreach (ContentInfo child in base.Children)
			{
				FSM currFsm = child.MakeFSM(stateNames);
				if (fsm != null)
				{
					FSM.CloneTransitions(currFsm, currFsm.Start, fsm, fsmStart);
					fsm.AddTransitions(currFsm);
					if (currFsm.isAccept(currFsm.Start))
					{
						fsmAccept.Add(fsmStart);
					}
					foreach (int state in currFsm.Accept)
					{
						fsmAccept.Add(state);
					}
				}
				else
				{
					fsm = currFsm;
					fsmStart = currFsm.Start;
					fsmAccept = currFsm.Accept;
				}
			}
			return fsm;
		}

		internal override FSM MakeFSM(StateNameSource stateNames)
		{
			FSM fsm = null;
			switch (this.contentModelType)
			{
				case Xml.Schema.Linq.ContentModelType.Sequence:
				{
					fsm = this.MakeSequenceFSM(stateNames);
					break;
				}
				case Xml.Schema.Linq.ContentModelType.Choice:
				{
					fsm = this.MakeChoiceFSM(stateNames);
					break;
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
			return base.ImplementFSMCardinality(fsm, stateNames);
		}

		private FSM MakeSequenceFSM(StateNameSource stateNames)
		{
			FSM fsm = null;
			Set<int> fsmAccept = null;
			foreach (ContentInfo child in base.Children)
			{
				FSM currFsm = child.MakeFSM(stateNames);
				if (fsm != null)
				{
					int currStart = currFsm.Start;
					foreach (int oldFinalState in fsmAccept)
					{
						FSM.CloneTransitions(currFsm, currStart, fsm, oldFinalState);
					}
					fsm.AddTransitions(currFsm);
					if (!currFsm.Accept.Contains(currStart))
					{
						fsmAccept.Clear();
					}
					foreach (int state in currFsm.Accept)
					{
						fsmAccept.Add(state);
					}
				}
				else
				{
					fsm = currFsm;
					fsmAccept = currFsm.Accept;
				}
			}
			return fsm;
		}
	}
}