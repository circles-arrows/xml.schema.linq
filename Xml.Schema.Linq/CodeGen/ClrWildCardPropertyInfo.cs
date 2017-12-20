using System;
using System.CodeDom;
using System.Collections.Generic;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrWildCardPropertyInfo : ClrBasePropertyInfo
	{
		private string namespaces;

		private string targetNamespace;

		private bool addToTypeDef;

		public bool AddToTypeDef
		{
			get
			{
				return this.addToTypeDef;
			}
			set
			{
				this.addToTypeDef = value;
			}
		}

		internal string Namespaces
		{
			get
			{
				return this.namespaces;
			}
		}

		internal override XCodeTypeReference ReturnType
		{
			get
			{
				CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference("XElement") };
				return new XCodeTypeReference("IEnumerable", codeTypeReference);
			}
		}

		internal string TargetNamespace
		{
			get
			{
				return this.targetNamespace;
			}
		}

		internal ClrWildCardPropertyInfo(string ns, string targetNs, bool addToType, Occurs schemaOccurs)
		{
			this.namespaces = ns;
			this.targetNamespace = targetNs;
			this.contentType = Xml.Schema.Linq.CodeGen.ContentType.WildCardProperty;
			this.addToTypeDef = addToType;
			this.occursInSchema = schemaOccurs;
		}

		internal void AddGetStatements(CodeStatementCollection getStatements)
		{
			CodeThisReferenceExpression codeThisReferenceExpression = CodeDomHelper.This();
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.CreateFieldReference("WildCard", "DefaultWildCard") };
			getStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.CreateMethodCall(codeThisReferenceExpression, "GetWildCards", codeExpressionArray)));
		}

		internal override void AddToConstructor(CodeConstructor functionalConstructor)
		{
			throw new InvalidOperationException();
		}

		internal override void AddToContentModel(CodeObjectCreateExpression contentModelExpression)
		{
			throw new InvalidOperationException();
		}

		internal override CodeMemberProperty AddToType(CodeTypeDeclaration decl, List<ClrAnnotation> annotations)
		{
			CodeMemberProperty codeMemberProperty;
			if (this.addToTypeDef)
			{
				CodeMemberProperty property = CodeDomHelper.CreateProperty(this.ReturnType, false);
				property.Name = base.PropertyName;
				property.Attributes = property.Attributes & (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | MemberAttributes.Public;
				this.AddGetStatements(property.GetStatements);
				this.ApplyAnnotations(property, annotations);
				decl.Members.Add(property);
				codeMemberProperty = property;
			}
			else
			{
				codeMemberProperty = null;
			}
			return codeMemberProperty;
		}

		internal override FSM MakeFSM(StateNameSource stateNames)
		{
			Dictionary<int, Transitions> transitions = new Dictionary<int, Transitions>();
			int start = stateNames.Next();
			int end = stateNames.Next();
			SingleTransition[] singleTransition = new SingleTransition[] { new SingleTransition(new WildCard(this.Namespaces, this.TargetNamespace), end) };
			transitions.Add(start, new Transitions(singleTransition));
			FSM fsm = new FSM(start, new Set<int>(end), transitions);
			return base.ImplementFSMCardinality(fsm, stateNames);
		}
	}
}