using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrPropertyInfo : ClrBasePropertyInfo
	{
		private ClrTypeReference typeRef;

		private PropertyFlags propertyFlags;

		private SchemaOrigin propertyOrigin;

		private CodeMethodInvokeExpression xNameGetExpression;

		private string parentTypeFullName;

		private string clrTypeName;

		private string fixedDefaultValue;

		private string simpleTypeClrTypeName;

		private ArrayList substitutionMembers;

		internal Type unionDefaultType;

		internal override string ClrTypeName
		{
			get
			{
				return this.clrTypeName;
			}
		}

		internal string DefaultValue
		{
			get
			{
				string str;
				if ((this.propertyFlags & PropertyFlags.HasDefaultValue) == PropertyFlags.None)
				{
					str = null;
				}
				else
				{
					str = this.fixedDefaultValue;
				}
				return str;
			}
			set
			{
				if (value != null)
				{
					this.propertyFlags |= PropertyFlags.HasDefaultValue;
					this.fixedDefaultValue = value;
				}
			}
		}

		internal string FixedValue
		{
			get
			{
				string str;
				if ((this.propertyFlags & PropertyFlags.HasFixedValue) == PropertyFlags.None)
				{
					str = null;
				}
				else
				{
					str = this.fixedDefaultValue;
				}
				return str;
			}
			set
			{
				if (value != null)
				{
					this.propertyFlags |= PropertyFlags.HasFixedValue;
					this.fixedDefaultValue = value;
				}
			}
		}

		internal override bool FromBaseType
		{
			get
			{
				return (this.propertyFlags & PropertyFlags.FromBaseType) != PropertyFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.IsDuplicate | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsNew | PropertyFlags.IsList | PropertyFlags.IsNullable | PropertyFlags.VerifyRequired);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.FromBaseType;
				}
			}
		}

		internal override bool IsDuplicate
		{
			get
			{
				return (this.propertyFlags & PropertyFlags.IsDuplicate) != PropertyFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.FromBaseType | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsNew | PropertyFlags.IsList | PropertyFlags.IsNullable | PropertyFlags.VerifyRequired);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.IsDuplicate;
				}
			}
		}

		internal override bool IsList
		{
			get
			{
				return (this.propertyFlags & PropertyFlags.IsList) != PropertyFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.FromBaseType | PropertyFlags.IsDuplicate | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsNew | PropertyFlags.IsNullable | PropertyFlags.VerifyRequired);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.IsList;
				}
			}
		}

		internal override bool IsNew
		{
			get
			{
				return (this.propertyFlags & PropertyFlags.IsNew) != PropertyFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.FromBaseType | PropertyFlags.IsDuplicate | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsList | PropertyFlags.IsNullable | PropertyFlags.VerifyRequired);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.IsNew;
				}
			}
		}

		internal override bool IsNullable
		{
			get
			{
				return ((this.propertyFlags & PropertyFlags.IsNullable) == PropertyFlags.None ? false : this.fixedDefaultValue == null);
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.FromBaseType | PropertyFlags.IsDuplicate | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsNew | PropertyFlags.IsList | PropertyFlags.VerifyRequired);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.IsNullable;
				}
			}
		}

		internal bool IsRef
		{
			get
			{
				return this.typeRef.IsTypeRef;
			}
		}

		internal override bool IsSchemaList
		{
			get
			{
				return this.typeRef.IsSchemaList;
			}
		}

		internal bool IsSubstitutionHead
		{
			get
			{
				return this.substitutionMembers != null;
			}
		}

		internal override bool IsUnion
		{
			get
			{
				return this.typeRef.IsUnion;
			}
		}

		internal SchemaOrigin Origin
		{
			get
			{
				return this.propertyOrigin;
			}
			set
			{
				this.propertyOrigin = value;
			}
		}

		internal override XCodeTypeReference ReturnType
		{
			get
			{
				bool flag;
				if (this.returnType == null)
				{
					string fullTypeName = this.clrTypeName;
					if ((!this.typeRef.IsLocalType ? false : !this.typeRef.IsSimpleType))
					{
						fullTypeName = string.Concat(this.parentTypeFullName, ".", this.clrTypeName);
					}
					if (this.IsList)
					{
						flag = false;
					}
					else
					{
						flag = (this.IsRef ? true : !this.IsSchemaList);
					}
					if (!flag)
					{
						this.returnType = this.CreateListReturnType(fullTypeName);
					}
					else if ((this.IsRef || !this.typeRef.IsValueType ? true : !this.IsNullable))
					{
						this.returnType = new XCodeTypeReference(this.clrTypeName)
						{
							fullTypeName = fullTypeName
						};
					}
					else
					{
						CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(fullTypeName) };
						this.returnType = new XCodeTypeReference("System.Nullable", codeTypeReference);
					}
				}
				return this.returnType;
			}
		}

		internal ArrayList SubstitutionMembers
		{
			get
			{
				return this.substitutionMembers;
			}
			set
			{
				this.substitutionMembers = value;
			}
		}

		internal ClrTypeReference TypeReference
		{
			get
			{
				return this.typeRef;
			}
			set
			{
				this.typeRef = value;
			}
		}

		internal bool Validation
		{
			get
			{
				return (!this.typeRef.Validate ? false : !this.IsRef);
			}
		}

		internal override bool VerifyRequired
		{
			get
			{
				return (this.propertyFlags & PropertyFlags.VerifyRequired) != PropertyFlags.None;
			}
			set
			{
				if (!value)
				{
					ClrPropertyInfo clrPropertyInfo = this;
					clrPropertyInfo.propertyFlags = clrPropertyInfo.propertyFlags & (PropertyFlags.FromBaseType | PropertyFlags.IsDuplicate | PropertyFlags.HasFixedValue | PropertyFlags.HasDefaultValue | PropertyFlags.IsNew | PropertyFlags.IsList | PropertyFlags.IsNullable);
				}
				else
				{
					this.propertyFlags |= PropertyFlags.VerifyRequired;
				}
			}
		}

		internal ClrPropertyInfo(string propertyName, string propertyNs, string schemaName, Occurs occursInSchema)
		{
			this.contentType = Xml.Schema.Linq.CodeGen.ContentType.Property;
			this.propertyName = propertyName;
			this.propertyNs = propertyNs;
			this.schemaName = schemaName;
			this.hasSet = true;
			this.returnType = null;
			this.clrTypeName = null;
			this.occursInSchema = occursInSchema;
			if (this.occursInSchema > Occurs.ZeroOrOne)
			{
				this.propertyFlags |= PropertyFlags.IsList;
			}
			if (base.IsOptional)
			{
				this.propertyFlags |= PropertyFlags.IsNullable;
			}
			this.XNameGetExpression();
		}

		protected void AddFixedValueChecking(CodeStatementCollection setStatements)
		{
			if (this.FixedValue != null)
			{
				CodeExpression fixedValueExpr = new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeFixedValueField));
				CodePropertySetValueReferenceExpression codePropertySetValueReferenceExpression = new CodePropertySetValueReferenceExpression();
				CodeExpression[] codeExpressionArray = new CodeExpression[] { fixedValueExpr };
				CodeMethodInvokeExpression codeMethodInvokeExpression = CodeDomHelper.CreateMethodCall(codePropertySetValueReferenceExpression, "Equals", codeExpressionArray);
				CodeStatement[] codeStatementArray = new CodeStatement[0];
				CodeStatement[] codeThrowExceptionStatement = new CodeStatement[1];
				Type type = typeof(LinqToXsdFixedValueException);
				codeExpressionArray = new CodeExpression[] { new CodePropertySetValueReferenceExpression(), fixedValueExpr };
				codeThrowExceptionStatement[0] = new CodeThrowExceptionStatement(new CodeObjectCreateExpression(type, codeExpressionArray));
				setStatements.Add(new CodeConditionStatement(codeMethodInvokeExpression, codeStatementArray, codeThrowExceptionStatement));
			}
		}

		private void AddGetStatements(CodeStatementCollection getStatements)
		{
			CodeExpression[] simpleTypeClassExpression;
			if (!this.IsSubstitutionHead)
			{
				CodeExpression returnExp = null;
				if (this.FixedValue == null)
				{
					getStatements.Add(this.GetValueMethodCall());
					this.CheckOccurrence(getStatements);
					CodeVariableReferenceExpression returnValueExp = new CodeVariableReferenceExpression("x");
					if ((this.IsRef ? true : !this.typeRef.IsSimpleType))
					{
						returnExp = new CodeCastExpression(this.ReturnType, returnValueExp);
					}
					else
					{
						CodeTypeReference parseType = this.ReturnType;
						if ((!this.typeRef.IsValueType ? false : this.IsNullable))
						{
							parseType = new CodeTypeReference(this.clrTypeName);
						}
						if (!this.IsUnion)
						{
							string parseMethodName = null;
							CodeExpression simpleTypeExpression = this.GetSchemaDatatypeExpression();
							if (!this.IsSchemaList)
							{
								parseMethodName = "ParseValue";
							}
							else
							{
								parseMethodName = "ParseListValue";
								parseType = new CodeTypeReference(this.clrTypeName);
							}
							CodeTypeReferenceExpression codeTypeReferenceExpression = CodeDomHelper.CreateTypeReferenceExp("XTypedServices");
							simpleTypeClassExpression = new CodeExpression[] { returnValueExp, simpleTypeExpression };
							returnExp = CodeDomHelper.CreateGenericMethodCall(codeTypeReferenceExpression, parseMethodName, parseType, simpleTypeClassExpression);
							if (this.DefaultValue != null)
							{
								((CodeMethodInvokeExpression)returnExp).Parameters.Add(new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeDefaultValueField)));
							}
						}
						else
						{
							CodeTypeReferenceExpression codeTypeReferenceExpression1 = CodeDomHelper.CreateTypeReferenceExp("XTypedServices");
							simpleTypeClassExpression = new CodeExpression[] { returnValueExp, this.GetSimpleTypeClassExpression() };
							returnExp = CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression1, "ParseUnionValue", simpleTypeClassExpression);
						}
					}
					getStatements.Add(new CodeMethodReturnStatement(returnExp));
				}
				else
				{
					getStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeFixedValueField))));
				}
			}
			else
			{
				this.AddSubstGetStatements(getStatements);
			}
		}

		private void AddListGetStatements(CodeStatementCollection getStatements, CodeTypeReference listType, string listName)
		{
			if (this.FixedValue == null)
			{
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
				CodeStatement[] codeAssignStatement = new CodeStatement[] { new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), new CodeObjectCreateExpression(listType, this.GetListParameters(false, false))) };
				getStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeAssignStatement));
				getStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName)));
			}
			else
			{
				getStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeFixedValueField))));
			}
		}

		private void AddListSetStatements(CodeStatementCollection setStatements, CodeTypeReference listType, string listName)
		{
			this.AddFixedValueChecking(setStatements);
			CodeStatement[] codeAssignStatement = new CodeStatement[] { new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), new CodePrimitiveExpression(null)) };
			CodeStatement[] trueStatements = codeAssignStatement;
			codeAssignStatement = new CodeStatement[1];
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			CodeStatement[] codeExpressionStatement = new CodeStatement[] { new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(listType), "Initialize", this.GetListParameters(true, false))) };
			CodeStatement[] codeStatementArray = codeExpressionStatement;
			codeExpressionStatement = new CodeStatement[1];
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
			string str = string.Concat("SetList<", this.clrTypeName, ">");
			CodeExpression[] codeFieldReferenceExpression = new CodeExpression[] { new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listName), CodeDomHelper.SetValue() };
			codeExpressionStatement[0] = new CodeExpressionStatement(new CodeMethodInvokeExpression(codeTypeReferenceExpression, str, codeFieldReferenceExpression));
			codeAssignStatement[0] = new CodeConditionStatement(codeBinaryOperatorExpression, codeStatementArray, codeExpressionStatement);
			CodeStatement[] falseStatements = codeAssignStatement;
			setStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(CodeDomHelper.SetValue(), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)), trueStatements, falseStatements));
		}

		private void AddMemberField(string memberName, CodeTypeReference memberType, CodeTypeDeclaration parentType)
		{
			CodeMemberField mem = new CodeMemberField(memberType, memberName)
			{
				Attributes = MemberAttributes.Private
			};
			CodeDomHelper.AddBrowseNever(mem);
			parentType.Members.Add(mem);
		}

		private void AddSetStatements(CodeStatementCollection setStatements)
		{
			this.AddFixedValueChecking(setStatements);
			setStatements.Add(this.SetValueMethodCall());
		}

		private void AddSubstGetStatements(CodeStatementCollection getStatements)
		{
			Debug.Assert(this.propertyOrigin == SchemaOrigin.Element);
			CodeExpression[] substParams = new CodeExpression[this.substitutionMembers.Count + 2];
			substParams[0] = CodeDomHelper.This();
			substParams[1] = CodeDomHelper.SingletonTypeManager();
			int i = 2;
			foreach (XmlSchemaElement elem in this.substitutionMembers)
			{
				int num = i;
				i = num + 1;
				substParams[num] = CodeDomHelper.XNameGetExpression(elem.QualifiedName.Name, elem.QualifiedName.Namespace);
			}
			getStatements.Add(new CodeVariableDeclarationStatement("XTypedElement", "x", CodeDomHelper.CreateMethodCall(new CodeTypeReferenceExpression("XTypedServices"), "ToSubstitutedXTypedElement", substParams)));
			this.CheckOccurrence(getStatements);
			getStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.ReturnType, new CodeVariableReferenceExpression("x"))));
		}

		internal override void AddToConstructor(CodeConstructor functionalConstructor)
		{
			if (!this.IsList)
			{
				functionalConstructor.Parameters.Add(new CodeParameterDeclarationExpression(this.ReturnType, this.propertyName));
				if (!this.FromBaseType)
				{
					functionalConstructor.Statements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference("this", this.propertyName), new CodeVariableReferenceExpression(this.propertyName)));
				}
				else
				{
					functionalConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(this.propertyName));
				}
			}
			else
			{
				CodeParameterDeclarationExpressionCollection parameters = functionalConstructor.Parameters;
				CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(this.clrTypeName) };
				parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("IEnumerable", codeTypeReference), this.propertyName));
				if (!this.FromBaseType)
				{
					CodeTypeReference listType = this.GetListType();
					functionalConstructor.Statements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference("this", NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeField)), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(listType), "Initialize", this.GetListParameters(true, true))));
				}
				else
				{
					functionalConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(this.propertyName));
				}
			}
		}

		internal override void AddToContentModel(CodeObjectCreateExpression contentModelExpression)
		{
			Debug.Assert((contentModelExpression == null ? false : this.propertyOrigin == SchemaOrigin.Element));
			if (!this.IsSubstitutionHead)
			{
				CodeExpressionCollection parameters = contentModelExpression.Parameters;
				CodeExpression[] codeExpressionArray = new CodeExpression[] { this.xNameGetExpression };
				parameters.Add(new CodeObjectCreateExpression("NamedContentModelEntity", codeExpressionArray));
			}
			else
			{
				CodeExpression[] substParams = new CodeExpression[this.substitutionMembers.Count];
				int i = 0;
				foreach (XmlSchemaElement elem in this.substitutionMembers)
				{
					int num = i;
					i = num + 1;
					substParams[num] = CodeDomHelper.XNameGetExpression(elem.QualifiedName.Name, elem.QualifiedName.Namespace);
				}
				contentModelExpression.Parameters.Add(new CodeObjectCreateExpression("SubstitutedContentModelEntity", substParams));
			}
		}

		internal override CodeMemberProperty AddToType(CodeTypeDeclaration parentTypeDecl, List<ClrAnnotation> annotations)
		{
			CodeMemberProperty codeMemberProperty;
			bool flag;
			if (this.IsDuplicate)
			{
				flag = false;
			}
			else
			{
				flag = (!this.FromBaseType ? true : this.IsNew);
			}
			if (flag)
			{
				this.CreateFixedDefaultValue(parentTypeDecl);
				CodeMemberProperty clrProperty = CodeDomHelper.CreateProperty(this.ReturnType, this.hasSet);
				clrProperty.Name = this.propertyName;
				this.SetPropertyAttributes(clrProperty);
				if (this.IsNew)
				{
					CodeMemberProperty attributes = clrProperty;
					attributes.Attributes = attributes.Attributes | MemberAttributes.New;
				}
				if (!this.IsList)
				{
					this.AddGetStatements(clrProperty.GetStatements);
					if (this.hasSet)
					{
						this.AddSetStatements(clrProperty.SetStatements);
					}
				}
				else
				{
					CodeTypeReference listType = this.GetListType();
					string listName = NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeField);
					this.AddMemberField(listName, listType, parentTypeDecl);
					this.AddListGetStatements(clrProperty.GetStatements, listType, listName);
					if (this.hasSet)
					{
						this.AddListSetStatements(clrProperty.SetStatements, listType, listName);
					}
				}
				this.ApplyAnnotations(clrProperty, annotations);
				parentTypeDecl.Members.Add(clrProperty);
				codeMemberProperty = clrProperty;
			}
			else
			{
				codeMemberProperty = null;
			}
			return codeMemberProperty;
		}

		private void CheckOccurrence(CodeStatementCollection getStatements)
		{
			string str;
			Debug.Assert(!this.IsList);
			CodeStatement returnStatement = null;
			if (!(!this.IsNullable ? true : this.DefaultValue != null))
			{
				if (this.typeRef.IsValueType)
				{
					returnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(null));
				}
			}
			else if (this.VerifyRequired)
			{
				Debug.Assert(this.occursInSchema == Occurs.One);
				if (this.propertyOrigin == SchemaOrigin.Element)
				{
					str = "Element";
				}
				else if (this.propertyOrigin == SchemaOrigin.Attribute)
				{
					str = "Attribute";
				}
				else
				{
					str = null;
				}
				string origin = str;
				CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression(string.Concat("Missing required ", origin)) };
				returnStatement = new CodeThrowExceptionStatement(new CodeObjectCreateExpression("LinqToXsdException", codePrimitiveExpression));
			}
			if (returnStatement != null)
			{
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("x"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
				CodeStatement[] codeStatementArray = new CodeStatement[] { returnStatement };
				getStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeStatementArray));
			}
		}

		private CodeStatement CreateDefaultValueAssignStmt(object value)
		{
			CodeStatement codeAssignStatement = new CodeAssignStatement(CodeDomHelper.CreateFieldReference(null, this.propertyName), CodeDomHelper.CreateFieldReference(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeDefaultValueField)));
			return codeAssignStatement;
		}

		protected void CreateFixedDefaultValue(CodeTypeDeclaration typeDecl)
		{
			if (this.fixedDefaultValue != null)
			{
				CodeMemberField fixedOrDefaultField = null;
				CodeTypeReference returnType = this.ReturnType;
				if (this.unionDefaultType != null)
				{
					returnType = new CodeTypeReference(this.unionDefaultType.ToString());
				}
				fixedOrDefaultField = (this.FixedValue == null ? new CodeMemberField(returnType, NameGenerator.ChangeClrName(base.PropertyName, NameOptions.MakeDefaultValueField)) : new CodeMemberField(returnType, NameGenerator.ChangeClrName(base.PropertyName, NameOptions.MakeFixedValueField)));
				CodeDomHelper.AddBrowseNever(fixedOrDefaultField);
				fixedOrDefaultField.Attributes = fixedOrDefaultField.Attributes & (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) & (MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.Assembly | MemberAttributes.FamilyAndAssembly | MemberAttributes.Family | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private | MemberAttributes.Public | MemberAttributes.AccessMask | MemberAttributes.VTableMask) | MemberAttributes.Private | MemberAttributes.Static;
				fixedOrDefaultField.InitExpression = SimpleTypeCodeDomHelper.CreateFixedDefaultValueExpression(returnType, this.fixedDefaultValue);
				typeDecl.Members.Add(fixedOrDefaultField);
			}
		}

		private XCodeTypeReference CreateListReturnType(string fullTypeName)
		{
			XCodeTypeReference xCodeTypeReference;
			CodeTypeReference[] codeTypeReference;
			if (!this.hasSet)
			{
				codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(fullTypeName) };
				xCodeTypeReference = new XCodeTypeReference("IEnumerable", codeTypeReference);
			}
			else
			{
				codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(fullTypeName) };
				xCodeTypeReference = new XCodeTypeReference("IList", codeTypeReference);
			}
			return xCodeTypeReference;
		}

		private CodeExpression[] GetListParameters(bool set, bool constructor)
		{
			CodeExpression[] listParameters = null;
			int paramCount = 2;
			CodeExpression typeParam = null;
			CodeExpression nameOrValue = null;
			if (set)
			{
				paramCount++;
				if (!constructor)
				{
					nameOrValue = CodeDomHelper.SetValue();
				}
				else
				{
					nameOrValue = new CodeVariableReferenceExpression(this.propertyName);
				}
			}
			if (!this.IsSubstitutionHead)
			{
				paramCount++;
				if (!this.typeRef.IsSimpleType)
				{
					typeParam = CodeDomHelper.SingletonTypeManager();
				}
				else
				{
					typeParam = this.GetSchemaDatatypeExpression();
					if (this.fixedDefaultValue != null)
					{
						paramCount++;
					}
				}
			}
			else
			{
				paramCount += this.substitutionMembers.Count;
				typeParam = CodeDomHelper.SingletonTypeManager();
			}
			listParameters = new CodeExpression[paramCount];
			int paramIndex = 0;
			int num = paramIndex;
			paramIndex = num + 1;
			listParameters[num] = CodeDomHelper.This();
			int num1 = paramIndex;
			paramIndex = num1 + 1;
			listParameters[num1] = typeParam;
			if (nameOrValue != null)
			{
				int num2 = paramIndex;
				paramIndex = num2 + 1;
				listParameters[num2] = nameOrValue;
			}
			if (!this.IsSubstitutionHead)
			{
				int num3 = paramIndex;
				paramIndex = num3 + 1;
				listParameters[num3] = this.xNameGetExpression;
			}
			else
			{
				foreach (XmlSchemaElement elem in this.substitutionMembers)
				{
					int num4 = paramIndex;
					paramIndex = num4 + 1;
					listParameters[num4] = CodeDomHelper.XNameGetExpression(elem.QualifiedName.Name, elem.QualifiedName.Namespace);
				}
			}
			if (this.fixedDefaultValue != null)
			{
				if (this.FixedValue == null)
				{
					int num5 = paramIndex;
					paramIndex = num5 + 1;
					listParameters[num5] = new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeDefaultValueField));
				}
				else
				{
					int num6 = paramIndex;
					paramIndex = num6 + 1;
					listParameters[num6] = new CodeFieldReferenceExpression(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeFixedValueField));
				}
			}
			return listParameters;
		}

		private CodeTypeReference GetListType()
		{
			string listName;
			if (!this.IsSubstitutionHead)
			{
				listName = (!this.typeRef.IsSimpleType ? "XTypedList" : "XSimpleList");
			}
			else
			{
				listName = "XTypedSubstitutedList";
			}
			CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(this.clrTypeName) };
			return new CodeTypeReference(listName, codeTypeReference);
		}

		protected CodeExpression GetSchemaDatatypeExpression()
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = CodeDomHelper.CreateTypeReferenceExp("XmlSchemaType");
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.CreateFieldReference("XmlTypeCode", this.typeRef.TypeCodeString) };
			CodeExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "GetBuiltInSimpleType", codeExpressionArray), "Datatype");
			return codeFieldReferenceExpression;
		}

		protected CodeExpression GetSimpleTypeClassExpression()
		{
			Debug.Assert(this.simpleTypeClrTypeName != null);
			return CodeDomHelper.CreateFieldReference(this.simpleTypeClrTypeName, "TypeDefinition");
		}

		private CodeVariableDeclarationStatement GetValueMethodCall()
		{
			CodeVariableDeclarationStatement codeVariableDeclarationStatement;
			CodeExpression[] codeExpressionArray;
			switch (this.propertyOrigin)
			{
				case SchemaOrigin.None:
				case SchemaOrigin.Fragment:
				{
					throw new InvalidOperationException();
				}
				case SchemaOrigin.Element:
				{
					CodeThisReferenceExpression codeThisReferenceExpression = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { this.xNameGetExpression };
					codeVariableDeclarationStatement = new CodeVariableDeclarationStatement("XElement", "x", CodeDomHelper.CreateMethodCall(codeThisReferenceExpression, "GetElement", codeExpressionArray));
					break;
				}
				case SchemaOrigin.Attribute:
				{
					CodeThisReferenceExpression codeThisReferenceExpression1 = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { this.xNameGetExpression };
					codeVariableDeclarationStatement = new CodeVariableDeclarationStatement("XAttribute", "x", CodeDomHelper.CreateMethodCall(codeThisReferenceExpression1, "Attribute", codeExpressionArray));
					break;
				}
				case SchemaOrigin.Text:
				{
					codeVariableDeclarationStatement = new CodeVariableDeclarationStatement("XElement", "x", new CodePropertyReferenceExpression(CodeDomHelper.This(), "Untyped"));
					break;
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
			return codeVariableDeclarationStatement;
		}

		internal virtual void InsertDefaultFixedValueInDefaultCtor(CodeConstructor ctor)
		{
			if (this.FixedValue != null)
			{
				ctor.Statements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference(null, this.propertyName), CodeDomHelper.CreateFieldReference(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeFixedValueField))));
			}
			else if (this.DefaultValue != null)
			{
				ctor.Statements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference(null, this.propertyName), CodeDomHelper.CreateFieldReference(null, NameGenerator.ChangeClrName(this.propertyName, NameOptions.MakeDefaultValueField))));
			}
		}

		internal override FSM MakeFSM(StateNameSource stateNames)
		{
			Dictionary<int, Transitions> transitions = new Dictionary<int, Transitions>();
			int start = stateNames.Next();
			int end = stateNames.Next();
			Transitions trans = new Transitions();
			if (!this.IsSubstitutionHead)
			{
				trans.Add(XName.Get(this.schemaName, base.PropertyNs), end);
			}
			else
			{
				foreach (XmlSchemaElement element in this.SubstitutionMembers)
				{
					trans.Add(XName.Get(element.QualifiedName.Name, element.QualifiedName.Namespace), end);
				}
			}
			transitions.Add(start, trans);
			FSM fSM = base.ImplementFSMCardinality(new FSM(start, new Set<int>(end), transitions), stateNames);
			return fSM;
		}

		internal void Reset()
		{
			this.returnType = null;
			this.clrTypeName = null;
			this.fixedDefaultValue = null;
			this.propertyFlags = PropertyFlags.None;
		}

		internal void SetFixedDefaultValue(ClrWrapperTypeInfo typeInfo)
		{
			this.FixedValue = typeInfo.FixedValue;
			this.DefaultValue = typeInfo.DefaultValue;
		}

		internal void SetPropertyAttributes(CodeMemberProperty clrProperty)
		{
			if (this.isVirtual)
			{
				clrProperty.Attributes = clrProperty.Attributes & (MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.Assembly | MemberAttributes.FamilyAndAssembly | MemberAttributes.Family | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private | MemberAttributes.Public | MemberAttributes.AccessMask | MemberAttributes.VTableMask) & (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | MemberAttributes.Public;
			}
			else if (this.isOverride)
			{
				clrProperty.Attributes = clrProperty.Attributes & (MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.Assembly | MemberAttributes.FamilyAndAssembly | MemberAttributes.Family | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private | MemberAttributes.Public | MemberAttributes.AccessMask | MemberAttributes.VTableMask) & (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | MemberAttributes.Public | MemberAttributes.Override;
			}
		}

		private CodeMethodInvokeExpression SetValueMethodCall()
		{
			CodeExpression[] codeExpressionArray;
			CodeMethodInvokeExpression methodCall = null;
			string setMethodName = "Set";
			bool isUnion = (!this.IsUnion ? false : this.propertyOrigin != SchemaOrigin.Element);
			if (!(this.IsRef ? true : !this.IsSchemaList))
			{
				setMethodName = string.Concat(setMethodName, "List");
			}
			else if (isUnion)
			{
				setMethodName = string.Concat(setMethodName, "Union");
			}
			bool validation = this.Validation;
			bool xNameParm = true;
			switch (this.propertyOrigin)
			{
				case SchemaOrigin.None:
				case SchemaOrigin.Fragment:
				{
					throw new InvalidOperationException();
				}
				case SchemaOrigin.Element:
				{
					setMethodName = string.Concat(setMethodName, "Element");
					break;
				}
				case SchemaOrigin.Attribute:
				{
					validation = false;
					setMethodName = string.Concat(setMethodName, "Attribute");
					break;
				}
				case SchemaOrigin.Text:
				{
					setMethodName = string.Concat(setMethodName, "Value");
					xNameParm = false;
					break;
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
			if (isUnion)
			{
				if (!xNameParm)
				{
					CodeThisReferenceExpression codeThisReferenceExpression = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { CodeDomHelper.SetValue(), new CodePrimitiveExpression(this.propertyName), CodeDomHelper.This(), this.GetSimpleTypeClassExpression() };
					methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression, setMethodName, codeExpressionArray);
				}
				else
				{
					CodeThisReferenceExpression codeThisReferenceExpression1 = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { CodeDomHelper.SetValue(), new CodePrimitiveExpression(this.propertyName), CodeDomHelper.This(), this.xNameGetExpression, this.GetSimpleTypeClassExpression() };
					methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression1, setMethodName, codeExpressionArray);
				}
			}
			else if (validation)
			{
				setMethodName = string.Concat(setMethodName, "WithValidation");
				if (!xNameParm)
				{
					CodeThisReferenceExpression codeThisReferenceExpression2 = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { CodeDomHelper.SetValue(), new CodePrimitiveExpression(base.PropertyName), this.GetSimpleTypeClassExpression() };
					methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression2, setMethodName, codeExpressionArray);
				}
				else
				{
					CodeThisReferenceExpression codeThisReferenceExpression3 = CodeDomHelper.This();
					codeExpressionArray = new CodeExpression[] { this.xNameGetExpression, CodeDomHelper.SetValue(), new CodePrimitiveExpression(base.PropertyName), this.GetSimpleTypeClassExpression() };
					methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression3, setMethodName, codeExpressionArray);
				}
			}
			else if (!xNameParm)
			{
				CodeThisReferenceExpression codeThisReferenceExpression4 = CodeDomHelper.This();
				codeExpressionArray = new CodeExpression[] { CodeDomHelper.SetValue(), this.GetSchemaDatatypeExpression() };
				methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression4, setMethodName, codeExpressionArray);
			}
			else
			{
				CodeThisReferenceExpression codeThisReferenceExpression5 = CodeDomHelper.This();
				codeExpressionArray = new CodeExpression[] { this.xNameGetExpression, CodeDomHelper.SetValue() };
				methodCall = CodeDomHelper.CreateMethodCall(codeThisReferenceExpression5, setMethodName, codeExpressionArray);
				if ((this.IsRef ? false : this.typeRef.IsSimpleType))
				{
					methodCall.Parameters.Add(this.GetSchemaDatatypeExpression());
				}
			}
			return methodCall;
		}

		private CodeStatement[] ToStmtArray(CodeStatementCollection collection)
		{
			CodeStatement[] stmts = new CodeStatement[collection.Count];
			for (int i = 0; i < collection.Count; i++)
			{
				stmts[i] = collection[i];
			}
			return stmts;
		}

		internal void UpdateTypeReference(string clrFullTypeName, string currentNamespace, Dictionary<XmlSchemaObject, string> nameMappings)
		{
			string refTypeName = null;
			this.clrTypeName = this.typeRef.GetClrFullTypeName(currentNamespace, nameMappings, out refTypeName);
			if ((this.Validation ? true : this.IsUnion))
			{
				this.simpleTypeClrTypeName = this.typeRef.GetSimpleTypeClrTypeDefName(currentNamespace, nameMappings);
			}
			this.parentTypeFullName = clrFullTypeName;
		}

		private void XNameGetExpression()
		{
			CodeExpression xNameExp = new CodePrimitiveExpression(this.schemaName);
			CodeExpression xNsExp = new CodePrimitiveExpression(this.propertyNs);
			this.xNameGetExpression = CodeDomHelper.XNameGetExpression(xNameExp, xNsExp);
		}
	}
}