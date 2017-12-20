using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Xml.Linq;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class FSMCodeDomHelper
	{
		public FSMCodeDomHelper()
		{
		}

		internal static void AddTransitions(FSM fsm, int state, CodeStatementCollection stmts, Set<int> visited)
		{
			if (!visited.Contains(state))
			{
				visited.Add(state);
				Transitions currTrans = null;
				fsm.Trans.TryGetValue(state, out currTrans);
				if ((currTrans == null ? false : currTrans.Count != 0))
				{
					FSMCodeDomHelper.CreateAddTransitionStmts(fsm, stmts, state, currTrans, visited);
				}
			}
		}

		internal static void CreateAddTransitionStmts(FSM fsm, CodeStatementCollection stmts, int state, Transitions currTrans, Set<int> visited)
		{
			Set<int> subStates = new Set<int>();
			CodeExpression[] initializers = new CodeExpression[currTrans.Count];
			int index = 0;
			if (currTrans.nameTransitions != null)
			{
				foreach (KeyValuePair<XName, int> s1Trans in currTrans.nameTransitions)
				{
					int num = index;
					index = num + 1;
					initializers[num] = FSMCodeDomHelper.CreateSingleTransitionExpr(FSMCodeDomHelper.CreateXNameExpr(s1Trans.Key), s1Trans.Value);
					subStates.Add(s1Trans.Value);
				}
			}
			if (currTrans.wildCardTransitions != null)
			{
				foreach (KeyValuePair<WildCard, int> s1Trans in currTrans.wildCardTransitions)
				{
					int num1 = index;
					index = num1 + 1;
					initializers[num1] = FSMCodeDomHelper.CreateSingleTransitionExpr(FSMCodeDomHelper.CreateWildCardExpr(s1Trans.Key), s1Trans.Value);
					subStates.Add(s1Trans.Value);
				}
			}
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("transitions");
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression((object)state), new CodeObjectCreateExpression("Transitions", initializers) };
			stmts.Add(CodeDomHelper.CreateMethodCall(codeVariableReferenceExpression, "Add", codePrimitiveExpression));
			foreach (int s in subStates)
			{
				FSMCodeDomHelper.AddTransitions(fsm, s, stmts, visited);
			}
		}

		internal static void CreateFSMStmt(FSM fsm, CodeStatementCollection stmts)
		{
			string[] strArrays = new string[] { "System.Int32", "Transitions" };
			CodeTypeReference typeRef = CodeDomHelper.CreateGenericTypeReference("Dictionary", strArrays);
			stmts.Add(new CodeVariableDeclarationStatement(typeRef, "transitions", new CodeObjectCreateExpression(typeRef, new CodeExpression[0])));
			Set<int> visited = new Set<int>();
			FSMCodeDomHelper.AddTransitions(fsm, fsm.Start, stmts, visited);
			Set<int> reachableAccept = new Set<int>();
			foreach (int state in fsm.Accept)
			{
				if (visited.Contains(state))
				{
					reachableAccept.Add(state);
				}
			}
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("validationStates");
			CodeTypeReference codeTypeReference = new CodeTypeReference("FSM");
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression((object)fsm.Start), FSMCodeDomHelper.CreateSetCreateExpression(reachableAccept), new CodeVariableReferenceExpression("transitions") };
			stmts.Add(new CodeAssignStatement(codeVariableReferenceExpression, new CodeObjectCreateExpression(codeTypeReference, codePrimitiveExpression)));
		}

		internal static CodeObjectCreateExpression CreateSetCreateExpression(Set<int> set)
		{
			string[] strArrays = new string[] { "System.Int32" };
			CodeObjectCreateExpression createSet = new CodeObjectCreateExpression(CodeDomHelper.CreateGenericTypeReference("Set", strArrays), new CodeExpression[0]);
			CodeExpressionCollection parameters = createSet.Parameters;
			if (set.Count == 1)
			{
				foreach (int num in set)
				{
					parameters.Add(new CodePrimitiveExpression((object)num));
				}
			}
			else if (set.Count > 1)
			{
				CodeArrayCreateExpression array = new CodeArrayCreateExpression()
				{
					CreateType = CodeDomHelper.CreateTypeReference("System.Int32")
				};
				CodeExpressionCollection initializers = array.Initializers;
				foreach (int i in set)
				{
					initializers.Add(new CodePrimitiveExpression((object)i));
				}
				parameters.Add(array);
			}
			return createSet;
		}

		internal static CodeExpression CreateSingleTransitionExpr(CodeExpression labelExpr, int nextState)
		{
			CodeExpression[] codeExpressionArray = new CodeExpression[] { labelExpr, new CodePrimitiveExpression((object)nextState) };
			return new CodeObjectCreateExpression("SingleTransition", codeExpressionArray);
		}

		internal static CodeExpression CreateWildCardExpr(WildCard any)
		{
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression(any.NsList.Namespaces), new CodePrimitiveExpression(any.NsList.TargetNamespace) };
			return new CodeObjectCreateExpression("WildCard", codePrimitiveExpression);
		}

		internal static CodeExpression CreateXNameExpr(XName name)
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XName");
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression(name.LocalName), new CodePrimitiveExpression(name.Namespace.NamespaceName) };
			return CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "Get", codePrimitiveExpression);
		}
	}
}