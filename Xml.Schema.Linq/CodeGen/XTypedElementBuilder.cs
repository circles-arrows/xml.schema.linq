using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class XTypedElementBuilder : TypeBuilder
	{
		private CodeTypeDeclItems declItemsInfo;

		private Stack<TypePropertyBuilder> propertyBuilderStack;

		private TypePropertyBuilder propertyBuilder;

		private CodeStatementCollection propertyDictionaryAddStatements;

		private bool HasElementProperties
		{
			get
			{
				return (this.propertyDictionaryAddStatements == null ? false : this.propertyDictionaryAddStatements.Count > 0);
			}
		}

		internal XTypedElementBuilder()
		{
			this.InnerInit();
		}

		private CodeMemberProperty BuildLocalElementDictionary()
		{
			CodeMemberProperty localDictionaryProperty = CodeDomHelper.CreateInterfaceImplProperty("LocalElementsDictionary", "IXMetaData", CodeDomHelper.CreateDictionaryType("XName", "System.Type"));
			CodeMemberField localDictionaryField = CodeDomHelper.CreateDictionaryField("localElementDictionary", "XName", "System.Type");
			CodeMemberMethod localDictionaryMethod = CodeDomHelper.CreateMethod("BuildElementDictionary", MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private, null);
			localDictionaryMethod.Statements.AddRange(this.propertyDictionaryAddStatements);
			this.decl.Members.Add(localDictionaryField);
			this.decl.Members.Add(localDictionaryMethod);
			localDictionaryProperty.GetStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.CreateFieldReference(null, "localElementDictionary")));
			CodeDomHelper.AddBrowseNever(localDictionaryProperty);
			CodeDomHelper.AddBrowseNever(localDictionaryField);
			return localDictionaryProperty;
		}

		internal override void CreateAttributeProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			this.propertyBuilder = TypePropertyBuilder.Create(this.decl, this.declItemsInfo);
			this.propertyBuilder.GenerateCode(propertyInfo, annotations);
		}

		internal override CodeConstructor CreateFunctionalConstructor(List<ClrAnnotation> annotations)
		{
			CodeConstructor functionalConstructor = this.declItemsInfo.functionalConstructor;
			if ((functionalConstructor == null ? false : functionalConstructor.Parameters.Count > 0))
			{
				TypeBuilder.ApplyAnnotations(functionalConstructor, annotations, null);
				this.decl.Members.Add(functionalConstructor);
			}
			return functionalConstructor;
		}

		internal override void CreateProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			if ((!this.clrTypeInfo.InlineBaseType ? false : propertyInfo.FromBaseType))
			{
				propertyInfo.IsNew = true;
			}
			this.propertyBuilder.GenerateCode(propertyInfo, annotations);
			if ((propertyInfo.ContentType != ContentType.Property ? false : !propertyInfo.IsDuplicate))
			{
				CodeStatementCollection codeStatementCollection = this.propertyDictionaryAddStatements;
				CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.XNameGetExpression(propertyInfo.SchemaName, propertyInfo.PropertyNs), CodeDomHelper.Typeof(propertyInfo.ClrTypeName) };
				codeStatementCollection.Add(CodeDomHelper.CreateMethodCallFromField("localElementDictionary", "Add", codeExpressionArray));
			}
		}

		internal override void CreateStaticConstructor()
		{
			if (this.declItemsInfo.staticConstructor == null)
			{
				this.declItemsInfo.staticConstructor = new CodeTypeConstructor();
				this.decl.Members.Add(this.declItemsInfo.staticConstructor);
			}
		}

		internal override void EndGrouping()
		{
			this.propertyBuilder.EndCodeGen();
			this.propertyBuilderStack.Pop();
			if (this.propertyBuilderStack.Count > 0)
			{
				this.propertyBuilder = this.propertyBuilderStack.Peek();
			}
		}

		protected override void ImplementCommonIXMetaData()
		{
			CodeMemberProperty localElementDictionary = null;
			if (this.HasElementProperties)
			{
				this.CreateStaticConstructor();
				localElementDictionary = this.BuildLocalElementDictionary();
				this.declItemsInfo.staticConstructor.Statements.Add(CodeDomHelper.CreateMethodCall(null, "BuildElementDictionary", new CodeExpression[0]));
				this.decl.Members.Add(localElementDictionary);
			}
		}

		protected override void ImplementContentModelMetaData()
		{
			CodeMemberMethod getContentModelMethod = null;
			if (!this.HasElementProperties)
			{
				getContentModelMethod = TypeBuilder.DefaultContentModel();
			}
			else if (this.declItemsInfo.contentModelExpression == null)
			{
				getContentModelMethod = TypeBuilder.DefaultContentModel();
			}
			else
			{
				CodeTypeReference cmType = new CodeTypeReference("ContentModelEntity");
				this.declItemsInfo.staticConstructor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("contentModel"), this.declItemsInfo.contentModelExpression));
				CodeMemberField contentModelField = new CodeMemberField(cmType, "contentModel");
				CodeDomHelper.AddBrowseNever(contentModelField);
				contentModelField.Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
				this.decl.Members.Add(contentModelField);
				getContentModelMethod = CodeDomHelper.CreateInterfaceImplMethod("GetContentModel", "IXMetaData", cmType, "contentModel");
			}
			this.decl.Members.Add(getContentModelMethod);
		}

		protected override void ImplementFSMMetaData()
		{
			Debug.Assert(this.clrTypeInfo.HasElementWildCard);
			if (this.fsmNameSource != null)
			{
				this.fsmNameSource.Reset();
			}
			else
			{
				this.fsmNameSource = new StateNameSource();
			}
			FSM fsm = this.clrTypeInfo.CreateFSM(this.fsmNameSource);
			this.decl.Members.Add(CodeDomHelper.CreateMemberField("validationStates", "FSM", MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private, false));
			CodeMemberMethod getFSM = CodeDomHelper.CreateInterfaceImplMethod("GetValidationStates", "IXMetaData", new CodeTypeReference("FSM"));
			getFSM.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "validationStates")));
			this.decl.Members.Add(getFSM);
			CodeMemberMethod initFSM = CodeDomHelper.CreateMethod("InitFSM", MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private, new CodeTypeReference());
			FSMCodeDomHelper.CreateFSMStmt(fsm, initFSM.Statements);
			this.decl.Members.Add(initFSM);
			this.CreateStaticConstructor();
			this.declItemsInfo.staticConstructor.Statements.Add(CodeDomHelper.CreateMethodCall(null, "InitFSM", null));
		}

		internal override void Init()
		{
			this.InnerInit();
		}

		private void InitializeTables()
		{
			if (this.propertyBuilderStack == null)
			{
				this.propertyBuilderStack = new Stack<TypePropertyBuilder>();
			}
			if (this.propertyDictionaryAddStatements == null)
			{
				this.propertyDictionaryAddStatements = new CodeStatementCollection();
			}
			if (this.declItemsInfo.propertyNameTypeTable == null)
			{
				this.declItemsInfo.propertyNameTypeTable = new Dictionary<string, CodeMemberProperty>();
			}
		}

		protected new void InnerInit()
		{
			base.InnerInit();
			this.propertyBuilder = null;
			if (this.propertyBuilderStack != null)
			{
				this.propertyBuilderStack.Clear();
			}
			if (this.propertyDictionaryAddStatements != null)
			{
				this.propertyDictionaryAddStatements.Clear();
			}
			if (this.declItemsInfo != null)
			{
				this.declItemsInfo.Init();
			}
			else
			{
				this.declItemsInfo = new CodeTypeDeclItems();
			}
		}

		protected override void SetElementWildCardFlag(bool hasAny)
		{
			this.declItemsInfo.hasElementWildCards = hasAny;
		}

		internal override void StartGrouping(GroupingInfo groupingInfo)
		{
			this.InitializeTables();
			this.propertyBuilder = TypePropertyBuilder.Create(groupingInfo, this.decl, this.declItemsInfo);
			this.propertyBuilder.StartCodeGen();
			this.propertyBuilderStack.Push(this.propertyBuilder);
		}
	}
}