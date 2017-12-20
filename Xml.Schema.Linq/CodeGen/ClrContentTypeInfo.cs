using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrContentTypeInfo : ClrTypeInfo
	{
		internal ContentInfo lastTypeMember;

		internal List<ClrTypeInfo> nestedTypes;

		internal IEnumerable<ContentInfo> Content
		{
			get
			{
				ContentInfo contentInfo = this.lastTypeMember;
				while (true)
				{
					if (contentInfo != null)
					{
						contentInfo = contentInfo.nextSibling;
						yield return contentInfo;
						if (contentInfo == this.lastTypeMember)
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

		internal List<ClrTypeInfo> NestedTypes
		{
			get
			{
				if (this.nestedTypes == null)
				{
					this.nestedTypes = new List<ClrTypeInfo>();
				}
				return this.nestedTypes;
			}
		}

		internal ClrContentTypeInfo()
		{
		}

		internal void AddMember(ContentInfo member)
		{
			if (this.lastTypeMember != null)
			{
				member.nextSibling = this.lastTypeMember.nextSibling;
				this.lastTypeMember.nextSibling = member;
			}
			else
			{
				member.nextSibling = member;
			}
			this.lastTypeMember = member;
		}

		internal override FSM CreateFSM(StateNameSource stateNames)
		{
			FSM fSM;
			foreach (ContentInfo content in this.Content)
			{
				GroupingInfo group = content as GroupingInfo;
				if (group != null)
				{
					fSM = group.MakeFSM(stateNames);
					return fSM;
				}
			}
			fSM = null;
			return fSM;
		}
	}
}