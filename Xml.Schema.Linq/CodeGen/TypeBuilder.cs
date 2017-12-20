using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class TypeBuilder
	{
		protected CodeTypeDeclaration decl;

		protected ClrTypeInfo clrTypeInfo;

		protected StateNameSource fsmNameSource;

		private static CodeMemberMethod defaultContentModel;

		protected virtual string InnerType
		{
			get
			{
				return null;
			}
		}

		internal CodeTypeDeclaration TypeDeclaration
		{
			get
			{
				return this.decl;
			}
		}

		protected TypeBuilder()
		{
		}

		protected virtual void AddBaseType()
		{
			string baseTypeRef;
			string baseTypeClrName = this.clrTypeInfo.baseTypeClrName;
			if (baseTypeClrName == null)
			{
				this.decl.BaseTypes.Add("XTypedElement");
			}
			else
			{
				string baseTypeClrNs = this.clrTypeInfo.baseTypeClrNs;
				baseTypeRef = (!(baseTypeClrNs != string.Empty) ? string.Concat("global::", baseTypeClrName) : string.Concat("global::", baseTypeClrNs, ".", baseTypeClrName));
				this.decl.BaseTypes.Add(baseTypeRef);
			}
		}

		internal void AddTypeToTypeManager(CodeStatementCollection dictionaryStatements, string dictionaryName)
		{
			string typeRef = string.Concat("global::", this.clrTypeInfo.clrFullTypeName);
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.XNameGetExpression(this.clrTypeInfo.schemaName, this.clrTypeInfo.schemaNs), CodeDomHelper.Typeof(typeRef) };
			dictionaryStatements.Add(CodeDomHelper.CreateMethodCallFromField(dictionaryName, "Add", codeExpressionArray));
		}

		internal void ApplyAnnotations(ClrTypeInfo typeInfo)
		{
			TypeBuilder.ApplyAnnotations(this.decl, typeInfo);
		}

		internal static void ApplyAnnotations(CodeMemberProperty propDecl, ClrBasePropertyInfo propInfo, List<ClrAnnotation> typeAnnotations)
		{
			TypeBuilder.ApplyAnnotations(propDecl, propInfo.Annotations, typeAnnotations);
		}

		internal static void ApplyAnnotations(CodeTypeMember typeDecl, ClrTypeInfo typeInfo)
		{
			TypeBuilder.ApplyAnnotations(typeDecl, typeInfo.Annotations, null);
		}

		internal static CodeTypeMember ApplyAnnotations(CodeTypeMember typeDecl, List<ClrAnnotation> annotations, List<ClrAnnotation> typeAnnotations)
		{
			bool fSummaryOpened = false;
			if (annotations != null)
			{
				foreach (ClrAnnotation ann in annotations)
				{
					if (!fSummaryOpened)
					{
						typeDecl.Comments.Add(new CodeCommentStatement("<summary>", true));
						fSummaryOpened = true;
					}
					typeDecl.Comments.Add(new CodeCommentStatement("<para>", true));
					typeDecl.Comments.Add(new CodeCommentStatement(ann.Text, true));
					typeDecl.Comments.Add(new CodeCommentStatement("</para>", true));
				}
			}
			if (typeAnnotations != null)
			{
				foreach (ClrAnnotation typeAnnotation in typeAnnotations)
				{
					if (typeAnnotation.Section == "summaryRegEx")
					{
						if (!fSummaryOpened)
						{
							typeDecl.Comments.Add(new CodeCommentStatement("<summary>", true));
							fSummaryOpened = true;
						}
						typeDecl.Comments.Add(new CodeCommentStatement("<para>", true));
						typeDecl.Comments.Add(new CodeCommentStatement(typeAnnotation.Text, true));
						typeDecl.Comments.Add(new CodeCommentStatement("</para>", true));
					}
				}
			}
			if (fSummaryOpened)
			{
				typeDecl.Comments.Add(new CodeCommentStatement("</summary>", true));
			}
			return typeDecl;
		}

		internal virtual void CreateAttributeProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			throw new InvalidOperationException();
		}

		internal virtual void CreateDefaultConstructor(List<ClrAnnotation> annotations)
		{
			this.decl.Members.Add(TypeBuilder.ApplyAnnotations(CodeDomHelper.CreateConstructor(MemberAttributes.Public), annotations, null));
		}

		internal virtual CodeConstructor CreateFunctionalConstructor(List<ClrAnnotation> annotations)
		{
			throw new InvalidOperationException();
		}

		internal virtual void CreateFunctionalConstructor(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			throw new InvalidOperationException();
		}

		internal virtual void CreateProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			throw new InvalidOperationException();
		}

		internal void CreateServicesMembers()
		{
			CodeMethodInvokeExpression callClone;
			CodeExpression[] codeCastExpression;
			string innerType = this.InnerType;
			string clrTypeName = this.clrTypeInfo.clrtypeName;
			bool useAutoTyping = (this.clrTypeInfo.IsAbstract ? true : this.clrTypeInfo.IsSubstitutionHead);
			if (this.clrTypeInfo.typeOrigin == SchemaOrigin.Element)
			{
				CodeTypeMember load = CodeDomHelper.CreateStaticMethod("Load", clrTypeName, innerType, "xmlFile", "System.String", useAutoTyping);
				CodeTypeMember loadReader = CodeDomHelper.CreateStaticMethod("Load", clrTypeName, innerType, "xmlFile", "System.IO.TextReader", useAutoTyping);
				CodeTypeMember parse = CodeDomHelper.CreateStaticMethod("Parse", clrTypeName, innerType, "xml", "System.String", useAutoTyping);
				if (!this.clrTypeInfo.IsDerived)
				{
					this.decl.Members.Add(CodeDomHelper.CreateSave("xmlFile", "System.String"));
					this.decl.Members.Add(CodeDomHelper.CreateSave("tw", "System.IO.TextWriter"));
					this.decl.Members.Add(CodeDomHelper.CreateSave("xmlWriter", "System.Xml.XmlWriter"));
				}
				else
				{
					CodeTypeMember attributes = load;
					attributes.Attributes = attributes.Attributes | MemberAttributes.New;
					CodeTypeMember codeTypeMember = parse;
					codeTypeMember.Attributes = codeTypeMember.Attributes | MemberAttributes.New;
				}
				this.decl.Members.Add(load);
				this.decl.Members.Add(loadReader);
				this.decl.Members.Add(parse);
			}
			CodeTypeMember cast = CodeDomHelper.CreateCast(clrTypeName, innerType, useAutoTyping);
			this.decl.Members.Add(cast);
			if (!this.clrTypeInfo.IsAbstract)
			{
				CodeMemberMethod clone = CodeDomHelper.CreateMethod("Clone", MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public, new CodeTypeReference("XTypedElement"));
				if (innerType != null)
				{
					callClone = CodeDomHelper.CreateMethodCall(new CodePropertyReferenceExpression(CodeDomHelper.This(), "Content"), "Clone", new CodeExpression[0]);
					CodeStatementCollection statements = clone.Statements;
					codeCastExpression = new CodeExpression[] { new CodeCastExpression(new CodeTypeReference(innerType), callClone) };
					statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(clrTypeName, codeCastExpression)));
				}
				else
				{
					CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
					string str = string.Concat("CloneXTypedElement<", clrTypeName, ">");
					codeCastExpression = new CodeExpression[] { new CodeThisReferenceExpression() };
					callClone = CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, str, codeCastExpression);
					clone.Statements.Add(new CodeMethodReturnStatement(callClone));
				}
				this.decl.Members.Add(clone);
			}
		}

		internal static CodeTypeDeclaration CreateSimpleType(ClrSimpleTypeInfo typeInfo, Dictionary<XmlSchemaObject, string> nameMappings, LinqToXsdSettings settings)
		{
			CodeTypeDeclaration simpleTypeDecl = new CodeTypeDeclaration(typeInfo.clrtypeName)
			{
				TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
			};
			CodeConstructor privateConst = new CodeConstructor()
			{
				Attributes = MemberAttributes.Private
			};
			simpleTypeDecl.Members.Add(privateConst);
			CodeMemberField typeField = CodeDomHelper.CreateMemberField("TypeDefinition", "Xml.Schema.Linq.SimpleTypeValidator", MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public, false);
			typeField.InitExpression = SimpleTypeCodeDomHelper.CreateSimpleTypeDef(typeInfo, nameMappings, settings, false);
			simpleTypeDecl.Members.Add(typeField);
			TypeBuilder.ApplyAnnotations(simpleTypeDecl, typeInfo);
			return simpleTypeDecl;
		}

		internal virtual void CreateStaticConstructor()
		{
			throw new InvalidOperationException();
		}

		internal void CreateTypeDeclaration(ClrTypeInfo clrTypeInfo)
		{
			this.clrTypeInfo = clrTypeInfo;
			this.SetElementWildCardFlag(clrTypeInfo.HasElementWildCard);
			string schemaName = clrTypeInfo.schemaName;
			string schemaNs = clrTypeInfo.schemaNs;
			string clrTypeName = clrTypeInfo.clrtypeName;
			SchemaOrigin typeOrigin = clrTypeInfo.typeOrigin;
			CodeTypeDeclaration typeDecl = CodeDomHelper.CreateTypeDeclaration(clrTypeName, this.InnerType);
			if (clrTypeInfo.IsAbstract)
			{
				CodeTypeDeclaration typeAttributes = typeDecl;
				typeAttributes.TypeAttributes = typeAttributes.TypeAttributes | TypeAttributes.Abstract;
			}
			else if (clrTypeInfo.IsSealed)
			{
				CodeTypeDeclaration codeTypeDeclaration = typeDecl;
				codeTypeDeclaration.TypeAttributes = codeTypeDeclaration.TypeAttributes | TypeAttributes.Sealed;
			}
			this.decl = typeDecl;
			this.AddBaseType();
			this.CreateServicesMembers();
			this.CreateDefaultConstructor(clrTypeInfo.Annotations);
		}

		internal static CodeTypeDeclaration CreateTypeManager(XmlQualifiedName rootElementName, bool enableServiceReference, CodeStatementCollection typeDictionaryStatements, CodeStatementCollection elementDictionaryStatements, CodeStatementCollection wrapperDictionaryStatements)
		{
			string servicesClassName = NameGenerator.GetServicesClassName();
			CodeTypeDeclaration servicesTypeDecl = new CodeTypeDeclaration(servicesClassName)
			{
				Attributes = MemberAttributes.Public
			};
			CodeMemberField singletonField = CodeDomHelper.CreateMemberField("typeManagerSingleton", servicesClassName, MemberAttributes.Static, true);
			CodeMemberProperty singletonProperty = CodeDomHelper.CreateProperty("Instance", null, singletonField, MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public, false);
			MemberAttributes privateStatic = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private;
			CodeTypeConstructor staticServicesConstructor = new CodeTypeConstructor();
			CodeTypeReference returnType = CodeDomHelper.CreateDictionaryType("XName", "System.Type");
			CodeTypeReference wrapperReturnType = CodeDomHelper.CreateDictionaryType("System.Type", "System.Type");
			CodeMemberProperty typeDictProperty = null;
			if (typeDictionaryStatements.Count <= 0)
			{
				typeDictProperty = CodeDomHelper.CreateInterfaceImplProperty("GlobalTypeDictionary", "ILinqToXsdTypeManager", returnType);
				typeDictProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("XTypedServices"), "EmptyDictionary")));
			}
			else
			{
				typeDictProperty = CodeDomHelper.CreateInterfaceImplProperty("GlobalTypeDictionary", "ILinqToXsdTypeManager", returnType, "typeDictionary");
				CodeMemberField staticTypeDictionary = CodeDomHelper.CreateDictionaryField("typeDictionary", "XName", "System.Type");
				CodeMemberMethod buildTypeDictionary = CodeDomHelper.CreateMethod("BuildTypeDictionary", privateStatic, null);
				buildTypeDictionary.Statements.AddRange(typeDictionaryStatements);
				staticServicesConstructor.Statements.Add(CodeDomHelper.CreateMethodCall(null, "BuildTypeDictionary", new CodeExpression[0]));
				servicesTypeDecl.Members.Add(staticTypeDictionary);
				servicesTypeDecl.Members.Add(buildTypeDictionary);
			}
			CodeMemberProperty elementDictProperty = null;
			if (elementDictionaryStatements.Count <= 0)
			{
				elementDictProperty = CodeDomHelper.CreateInterfaceImplProperty("GlobalElementDictionary", "ILinqToXsdTypeManager", returnType);
				elementDictProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("XTypedServices"), "EmptyDictionary")));
			}
			else
			{
				elementDictProperty = CodeDomHelper.CreateInterfaceImplProperty("GlobalElementDictionary", "ILinqToXsdTypeManager", returnType, "elementDictionary");
				CodeMemberField staticElementDictionary = CodeDomHelper.CreateDictionaryField("elementDictionary", "XName", "System.Type");
				CodeMemberMethod buildElementDictionary = CodeDomHelper.CreateMethod("BuildElementDictionary", privateStatic, null);
				buildElementDictionary.Statements.AddRange(elementDictionaryStatements);
				staticServicesConstructor.Statements.Add(CodeDomHelper.CreateMethodCall(null, "BuildElementDictionary", new CodeExpression[0]));
				servicesTypeDecl.Members.Add(staticElementDictionary);
				servicesTypeDecl.Members.Add(buildElementDictionary);
			}
			CodeMemberProperty wrapperDictProperty = null;
			if (wrapperDictionaryStatements.Count <= 0)
			{
				wrapperDictProperty = CodeDomHelper.CreateInterfaceImplProperty("RootContentTypeMapping", "ILinqToXsdTypeManager", wrapperReturnType);
				wrapperDictProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("XTypedServices"), "EmptyTypeMappingDictionary")));
			}
			else
			{
				wrapperDictProperty = CodeDomHelper.CreateInterfaceImplProperty("RootContentTypeMapping", "ILinqToXsdTypeManager", wrapperReturnType, "wrapperDictionary");
				CodeMemberField staticWrapperDictionary = CodeDomHelper.CreateDictionaryField("wrapperDictionary", "System.Type", "System.Type");
				CodeMemberMethod buildWrapperDictionary = CodeDomHelper.CreateMethod("BuildWrapperDictionary", privateStatic, null);
				buildWrapperDictionary.Statements.AddRange(wrapperDictionaryStatements);
				staticServicesConstructor.Statements.Add(CodeDomHelper.CreateMethodCall(null, "BuildWrapperDictionary", new CodeExpression[0]));
				servicesTypeDecl.Members.Add(staticWrapperDictionary);
				servicesTypeDecl.Members.Add(buildWrapperDictionary);
			}
			string schemaSetFieldName = "schemaSet";
			CodeTypeReference schemaSetType = new CodeTypeReference("XmlSchemaSet");
			CodeMemberField schemaSetField = new CodeMemberField(schemaSetType, schemaSetFieldName)
			{
				Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Assembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Private
			};
			CodeMemberMethod addSchemasMethod = CodeDomHelper.CreateMethod("AddSchemas", MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyOrAssembly, null);
			addSchemasMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlSchemaSet", "schemas"));
			CodeStatementCollection statements = addSchemasMethod.Statements;
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("schemas");
			CodeExpression[] codeFieldReferenceExpression = new CodeExpression[] { new CodeFieldReferenceExpression(null, schemaSetFieldName) };
			statements.Add(CodeDomHelper.CreateMethodCall(codeVariableReferenceExpression, "Add", codeFieldReferenceExpression));
			CodeTypeReferenceExpression interLockedType = new CodeTypeReferenceExpression("System.Threading.Interlocked");
			CodeMemberProperty schemaSetProperty = CodeDomHelper.CreateInterfaceImplProperty("Schemas", "ILinqToXsdTypeManager", schemaSetType);
			CodeFieldReferenceExpression schemaSetFieldRef = new CodeFieldReferenceExpression(null, schemaSetFieldName);
			CodeDirectionExpression schemaSetParam = new CodeDirectionExpression(FieldDirection.Ref, schemaSetFieldRef);
			CodeStatementCollection getStatements = schemaSetProperty.GetStatements;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(schemaSetFieldRef, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			CodeStatement[] codeVariableDeclarationStatement = new CodeStatement[] { new CodeVariableDeclarationStatement(schemaSetType, "tempSet", new CodeObjectCreateExpression(schemaSetType, new CodeExpression[0])), null };
			codeFieldReferenceExpression = new CodeExpression[] { schemaSetParam, new CodeVariableReferenceExpression("tempSet"), new CodePrimitiveExpression(null) };
			codeVariableDeclarationStatement[1] = new CodeExpressionStatement(CodeDomHelper.CreateMethodCall(interLockedType, "CompareExchange", codeFieldReferenceExpression));
			getStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeVariableDeclarationStatement));
			schemaSetProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(schemaSetFieldName)));
			schemaSetProperty.SetStatements.Add(new CodeAssignStatement(schemaSetFieldRef, new CodePropertySetValueReferenceExpression()));
			servicesTypeDecl.Members.Add(schemaSetField);
			servicesTypeDecl.Members.Add(schemaSetProperty);
			servicesTypeDecl.Members.Add(addSchemasMethod);
			servicesTypeDecl.Members.Add(typeDictProperty);
			servicesTypeDecl.Members.Add(elementDictProperty);
			servicesTypeDecl.Members.Add(wrapperDictProperty);
			servicesTypeDecl.BaseTypes.Add("ILinqToXsdTypeManager");
			CodeMemberMethod getRootType = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public,
				Name = "GetRootType",
				ReturnType = new CodeTypeReference("System.Type")
			};
			if (!rootElementName.IsEmpty)
			{
				CodeStatementCollection codeStatementCollection = getRootType.Statements;
				CodeFieldReferenceExpression codeFieldReferenceExpression1 = CodeDomHelper.CreateFieldReference(null, "elementDictionary");
				codeFieldReferenceExpression = new CodeExpression[] { CodeDomHelper.XNameGetExpression(rootElementName.Name, rootElementName.Namespace) };
				codeStatementCollection.Add(new CodeMethodReturnStatement(new CodeIndexerExpression(codeFieldReferenceExpression1, codeFieldReferenceExpression)));
			}
			else
			{
				getRootType.Statements.Add(new CodeMethodReturnStatement(CodeDomHelper.Typeof("Xml.Schema.Linq.XTypedElement")));
			}
			servicesTypeDecl.Members.Add(staticServicesConstructor);
			servicesTypeDecl.Members.Add(getRootType);
			servicesTypeDecl.Members.Add(singletonField);
			servicesTypeDecl.Members.Add(singletonProperty);
			return servicesTypeDecl;
		}

		protected static CodeMemberMethod DefaultContentModel()
		{
			if (TypeBuilder.defaultContentModel == null)
			{
				CodeMemberMethod getContentModelMethod = CodeDomHelper.CreateInterfaceImplMethod("GetContentModel", "IXMetaData", new CodeTypeReference("ContentModelEntity"));
				getContentModelMethod.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ContentModelEntity"), "Default")));
				Interlocked.CompareExchange<CodeMemberMethod>(ref TypeBuilder.defaultContentModel, getContentModelMethod, null);
			}
			return TypeBuilder.defaultContentModel;
		}

		internal virtual void EndGrouping()
		{
			throw new InvalidOperationException();
		}

		protected virtual void ImplementCommonIXMetaData()
		{
		}

		protected virtual void ImplementContentModelMetaData()
		{
			this.decl.Members.Add(TypeBuilder.DefaultContentModel());
		}

		protected virtual void ImplementFSMMetaData()
		{
		}

		internal virtual void ImplementInterfaces(bool enableServiceReference)
		{
			this.ImplementIXMetaData();
			if (enableServiceReference)
			{
				this.ImplementIXmlSerializable();
			}
		}

		private void ImplementIXMetaData()
		{
			string interfaceName = "IXMetaData";
			CodeMemberProperty schemaNameProperty = CodeDomHelper.CreateSchemaNameProperty(this.clrTypeInfo.schemaName, this.clrTypeInfo.schemaNs);
			this.ImplementCommonIXMetaData();
			if (!this.clrTypeInfo.HasElementWildCard)
			{
				this.ImplementContentModelMetaData();
			}
			else
			{
				this.ImplementFSMMetaData();
			}
			CodeMemberProperty typeOriginProperty = CodeDomHelper.CreateTypeOriginProperty(this.clrTypeInfo.typeOrigin);
			CodeDomHelper.AddBrowseNever(schemaNameProperty);
			CodeDomHelper.AddBrowseNever(typeOriginProperty);
			this.decl.Members.Add(schemaNameProperty);
			this.decl.Members.Add(typeOriginProperty);
			this.decl.Members.Add(CodeDomHelper.AddBrowseNever(CodeDomHelper.CreateTypeManagerProperty()));
			this.decl.BaseTypes.Add(interfaceName);
		}

		private void ImplementIXmlSerializable()
		{
			string interfaceName = "IXmlSerializable";
			string typeManagerName = NameGenerator.GetServicesClassName();
			string methodName = string.Concat(this.clrTypeInfo.clrtypeName, "SchemaProvider");
			CodeMemberMethod schemaProviderMethod = CodeDomHelper.CreateMethod(methodName, MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public, null);
			schemaProviderMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlSchemaSet", "schemas"));
			CodeStatementCollection statements = schemaProviderMethod.Statements;
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(typeManagerName);
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression("schemas") };
			statements.Add(CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "AddSchemas", codeVariableReferenceExpression));
			codeVariableReferenceExpression = new CodeExpression[] { new CodePrimitiveExpression(this.clrTypeInfo.schemaName), new CodePrimitiveExpression(this.clrTypeInfo.schemaNs) };
			CodeExpression qNameExp = new CodeObjectCreateExpression("XmlQualifiedName", codeVariableReferenceExpression);
			if (this.clrTypeInfo.typeOrigin != SchemaOrigin.Element)
			{
				schemaProviderMethod.ReturnType = new CodeTypeReference("XmlQualifiedName");
				schemaProviderMethod.Statements.Add(new CodeMethodReturnStatement(qNameExp));
			}
			else
			{
				CodeStatementCollection codeStatementCollection = schemaProviderMethod.Statements;
				CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("schemas"), "GlobalElements");
				codeVariableReferenceExpression = new CodeExpression[] { qNameExp };
				codeStatementCollection.Add(new CodeVariableDeclarationStatement("XmlSchemaElement", "element", new CodeCastExpression("XmlSchemaElement", new CodeIndexerExpression(codePropertyReferenceExpression, codeVariableReferenceExpression))));
				CodeStatementCollection statements1 = schemaProviderMethod.Statements;
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("element"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
				CodeStatement[] codeMethodReturnStatement = new CodeStatement[] { new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("element"), "ElementSchemaType")) };
				statements1.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeMethodReturnStatement));
				schemaProviderMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
				schemaProviderMethod.ReturnType = new CodeTypeReference("XmlSchemaType");
			}
			this.decl.CustomAttributes.Add(CodeDomHelper.SchemaProviderAttribute(this.clrTypeInfo.clrtypeName));
			this.decl.BaseTypes.Add(interfaceName);
			this.decl.Members.Add(schemaProviderMethod);
		}

		internal virtual void Init()
		{
			this.InnerInit();
		}

		protected void InnerInit()
		{
			this.decl = null;
			this.clrTypeInfo = null;
		}

		protected virtual void SetElementWildCardFlag(bool hasAny)
		{
		}

		internal virtual void StartGrouping(GroupingInfo grouping)
		{
			throw new InvalidOperationException();
		}
	}
}