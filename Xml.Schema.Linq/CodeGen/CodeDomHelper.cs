using System;
using System.CodeDom;
using System.Reflection;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal static class CodeDomHelper
	{
		public static CodeTypeMember AddBrowseNever(CodeTypeMember member)
		{
			CodeAttributeArgument[] codeAttributeArgument = new CodeAttributeArgument[] { new CodeAttributeArgument(CodeDomHelper.CreateFieldReference("DebuggerBrowsableState", "Never")) };
			CodeAttributeDeclaration browsableNever = new CodeAttributeDeclaration("DebuggerBrowsable", codeAttributeArgument);
			if (member.CustomAttributes == null)
			{
				member.CustomAttributes = new CodeAttributeDeclarationCollection();
			}
			member.CustomAttributes.Add(browsableNever);
			return member;
		}

		public static CodeSnippetTypeMember CreateCast(string typeT, string typeT1, bool useAutoTyping)
		{
			string[] strArrays;
			CodeSnippetTypeMember castMember = new CodeSnippetTypeMember();
			if (!useAutoTyping)
			{
				strArrays = new string[] { "         public static explicit operator ", typeT, "(XElement xe) { return ", "XTypedServices", ".ToXTypedElement<", CodeDomHelper.GetInnerType(typeT, typeT1), ">(xe,", NameGenerator.GetServicesClassName(), ".Instance as ILinqToXsdTypeManager); }" };
				castMember.Text = string.Concat(strArrays);
			}
			else
			{
				strArrays = new string[] { "         public static explicit operator ", typeT, "(XElement xe) {  ", "return (", typeT, ")", "XTypedServices", ".ToXTypedElement(xe,", NameGenerator.GetServicesClassName(), ".Instance as ILinqToXsdTypeManager); }" };
				castMember.Text = string.Concat(strArrays);
			}
			return castMember;
		}

		public static CodeVariableDeclarationStatement CreateCastToInterface(string interfaceName, string variableName, string propertyToCast)
		{
			CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement(interfaceName, variableName, new CodeCastExpression(interfaceName, new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propertyToCast)));
			return codeVariableDeclarationStatement;
		}

		public static CodeConstructor CreateConstructor(MemberAttributes memAttributes)
		{
			return new CodeConstructor()
			{
				Attributes = memAttributes
			};
		}

		public static CodeMemberField CreateDictionaryField(string dictionaryName, string keyType, string valueType)
		{
			CodeMemberField staticDictionary = new CodeMemberField(CodeDomHelper.CreateDictionaryType(keyType, valueType), dictionaryName)
			{
				Attributes = MemberAttributes.Static,
				InitExpression = new CodeObjectCreateExpression(CodeDomHelper.CreateDictionaryType(keyType, valueType), new CodeExpression[0])
			};
			return staticDictionary;
		}

		public static CodeTypeReference CreateDictionaryType(string keyType, string valueType)
		{
			CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(keyType), new CodeTypeReference(valueType) };
			return new CodeTypeReference("Dictionary", codeTypeReference);
		}

		public static CodeFieldReferenceExpression CreateFieldReference(string typeName, string fieldName)
		{
			CodeExpression targetObject = null;
			if (typeName == "this")
			{
				targetObject = new CodeThisReferenceExpression();
			}
			else if (typeName != null)
			{
				targetObject = new CodeTypeReferenceExpression(typeName);
			}
			return new CodeFieldReferenceExpression(targetObject, fieldName);
		}

		public static CodeMemberField CreateGenericMemberField(string memberName, string typeName, string[] typeStrParams, MemberAttributes attributes, bool init)
		{
			CodeTypeReference[] typeParams = new CodeTypeReference[(int)typeStrParams.Length];
			int index = 0;
			string[] strArrays = typeStrParams;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				int num = index;
				index = num + 1;
				typeParams[num] = new CodeTypeReference(str);
			}
			return CodeDomHelper.CreateGenericMemberField(memberName, typeName, typeParams, attributes, init);
		}

		public static CodeMemberField CreateGenericMemberField(string memberName, string typeName, CodeTypeReference[] typeParams, MemberAttributes attributes, bool init)
		{
			CodeTypeReference typeRef = new CodeTypeReference(typeName, typeParams);
			CodeMemberField field = new CodeMemberField(typeRef, memberName);
			CodeDomHelper.AddBrowseNever(field);
			field.Attributes = attributes;
			if (init)
			{
				field.InitExpression = new CodeObjectCreateExpression(typeRef, new CodeExpression[0]);
			}
			return field;
		}

		public static CodeMethodInvokeExpression CreateGenericMethodCall(CodeExpression targetOBject, string methodName, CodeTypeReference typeParam1, params CodeExpression[] parameters)
		{
			CodeTypeReference[] codeTypeReferenceArray = new CodeTypeReference[] { typeParam1 };
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetOBject, methodName, codeTypeReferenceArray), parameters);
		}

		public static string CreateGenericMethodName(string methodName, string typeName)
		{
			return string.Concat(methodName, "<", typeName, ">");
		}

		public static CodeTypeReference CreateGenericTypeReference(string type, string[] typeStrParams)
		{
			CodeTypeReference[] typeParams = new CodeTypeReference[(int)typeStrParams.Length];
			int index = 0;
			string[] strArrays = typeStrParams;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				int num = index;
				index = num + 1;
				typeParams[num] = new CodeTypeReference(str);
			}
			return CodeDomHelper.CreateGenericTypeReference(type, typeParams);
		}

		public static CodeTypeReference CreateGenericTypeReference(string type, CodeTypeReference[] typeArgs)
		{
			return new CodeTypeReference(type, typeArgs);
		}

		public static CodeIndexerExpression CreateIndexerExpression(string target, string key)
		{
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression(target);
			CodeExpression[] codePrimitiveExpression = new CodeExpression[] { new CodePrimitiveExpression(key) };
			return new CodeIndexerExpression(codeVariableReferenceExpression, codePrimitiveExpression);
		}

		public static CodeMemberMethod CreateInterfaceImplMethod(string methodName, string interfaceName)
		{
			CodeMemberMethod interfaceMethod = CodeDomHelper.CreateMethod(methodName, MemberAttributes.Public, null);
			CodeTypeReference interfaceType = new CodeTypeReference(interfaceName);
			interfaceMethod.PrivateImplementationType = interfaceType;
			interfaceMethod.ImplementationTypes.Add(interfaceType);
			return interfaceMethod;
		}

		public static CodeMemberMethod CreateInterfaceImplMethod(string methodName, string interfaceName, CodeTypeReference returnType, string fieldName)
		{
			CodeMemberMethod interfaceMethod = CodeDomHelper.CreateMethod(methodName, MemberAttributes.Public, returnType);
			interfaceMethod.PrivateImplementationType = new CodeTypeReference(interfaceName);
			interfaceMethod.ImplementationTypes.Add(new CodeTypeReference(interfaceName));
			interfaceMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(fieldName)));
			return interfaceMethod;
		}

		public static CodeMemberMethod CreateInterfaceImplMethod(string methodName, string interfaceName, CodeTypeReference returnType)
		{
			CodeMemberMethod interfaceMethod = CodeDomHelper.CreateMethod(methodName, MemberAttributes.Public, returnType);
			interfaceMethod.PrivateImplementationType = new CodeTypeReference(interfaceName);
			interfaceMethod.ImplementationTypes.Add(new CodeTypeReference(interfaceName));
			return interfaceMethod;
		}

		public static CodeMemberProperty CreateInterfaceImplProperty(string propertyName, string interfaceName, CodeTypeReference returnType, string fieldName)
		{
			CodeMemberProperty interfaceProperty = CodeDomHelper.CreateInterfaceImplProperty(propertyName, interfaceName, returnType);
			interfaceProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(fieldName)));
			return interfaceProperty;
		}

		public static CodeMemberProperty CreateInterfaceImplProperty(string propertyName, string interfaceName, CodeTypeReference returnType)
		{
			CodeMemberProperty interfaceProperty = CodeDomHelper.CreateProperty(propertyName, returnType, MemberAttributes.Public);
			interfaceProperty.PrivateImplementationType = new CodeTypeReference(interfaceName);
			interfaceProperty.ImplementationTypes.Add(new CodeTypeReference(interfaceName));
			return interfaceProperty;
		}

		public static CodeMemberField CreateMemberField(string memberName, string typeName, MemberAttributes attributes, bool init)
		{
			CodeMemberField field = new CodeMemberField(typeName, memberName);
			CodeDomHelper.AddBrowseNever(field);
			field.Attributes = attributes;
			if (init)
			{
				field.InitExpression = new CodeObjectCreateExpression(typeName, new CodeExpression[0]);
			}
			return field;
		}

		public static CodeMemberMethod CreateMethod(string methodName, MemberAttributes methodAttributes, CodeTypeReference returnType)
		{
			CodeMemberMethod method = new CodeMemberMethod()
			{
				Name = methodName,
				Attributes = methodAttributes,
				ReturnType = returnType
			};
			return method;
		}

		public static CodeMethodInvokeExpression CreateMethodCall(CodeExpression targetOBject, string methodName, params CodeExpression[] parameters)
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression;
			codeMethodInvokeExpression = (parameters != null ? new CodeMethodInvokeExpression(targetOBject, methodName, parameters) : new CodeMethodInvokeExpression(targetOBject, methodName, new CodeExpression[0]));
			return codeMethodInvokeExpression;
		}

		public static CodeMethodInvokeExpression CreateMethodCallFromField(string fieldName, string methodName, params CodeExpression[] parameters)
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(CodeDomHelper.CreateFieldReference(null, fieldName), methodName, parameters);
			return codeMethodInvokeExpression;
		}

		public static CodeParameterDeclarationExpression CreateParameter(string paramName, string paramType)
		{
			return new CodeParameterDeclarationExpression(new CodeTypeReference(paramType), paramName);
		}

		public static CodeMemberProperty CreateProperty(string propertyName, string propertyType, CodeMemberField field, MemberAttributes attributes, bool hasSet)
		{
			CodeTypeReference returnType = null;
			returnType = (propertyType == null ? field.Type : new CodeTypeReference(propertyType));
			CodeMemberProperty valueProperty = CodeDomHelper.CreateProperty(propertyName, returnType, attributes);
			valueProperty.GetStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.CreateFieldReference(null, field.Name)));
			if (hasSet)
			{
				CodeExpression rightExpression = null;
				if (!(field.Type.BaseType != returnType.BaseType))
				{
					rightExpression = CodeDomHelper.SetValue();
				}
				else
				{
					rightExpression = new CodeCastExpression(field.Type, CodeDomHelper.SetValue());
				}
				valueProperty.SetStatements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference("this", field.Name), rightExpression));
			}
			return valueProperty;
		}

		public static CodeMemberProperty CreateProperty(string propertyName, CodeTypeReference propertyType, MemberAttributes propertyAttributes)
		{
			CodeMemberProperty clrProperty = new CodeMemberProperty()
			{
				Attributes = (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | propertyAttributes,
				HasGet = true,
				Name = propertyName,
				Type = propertyType
			};
			return clrProperty;
		}

		public static CodeMemberProperty CreateProperty(CodeTypeReference returnType, bool hasSet)
		{
			CodeMemberProperty clrProperty = new CodeMemberProperty()
			{
				Attributes = (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | MemberAttributes.Public,
				HasGet = true,
				HasSet = hasSet,
				Type = returnType
			};
			return clrProperty;
		}

		public static CodeTypeMember CreateSave(string paramName, string paramType)
		{
			CodeMemberMethod saveMethod = new CodeMemberMethod()
			{
				Name = "Save",
				Attributes = (MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Override | MemberAttributes.Const | MemberAttributes.New | MemberAttributes.Overloaded | MemberAttributes.ScopeMask | MemberAttributes.VTableMask) | MemberAttributes.Public
			};
			saveMethod.Parameters.Add(CodeDomHelper.CreateParameter(paramName, paramType));
			CodeStatementCollection statements = saveMethod.Statements;
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression(paramName), new CodePropertyReferenceExpression(null, "Untyped") };
			statements.Add(CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "Save", codeVariableReferenceExpression));
			return saveMethod;
		}

		public static CodeMemberProperty CreateSchemaNameProperty(string schemaName, string schemaNs)
		{
			CodeMemberProperty property = CodeDomHelper.CreateInterfaceImplProperty("SchemaName", "IXMetaData", new CodeTypeReference("XName"));
			property.GetStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.XNameGetExpression(schemaName, schemaNs)));
			return property;
		}

		public static CodeTypeMember CreateStaticMethod(string methodName, string typeT, string typeT1, string parameterName, string parameterType, bool useAutoTyping)
		{
			CodeExpression[] codeExpressionArray;
			CodeMemberMethod staticMethod = new CodeMemberMethod()
			{
				Name = methodName,
				Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public,
				ReturnType = new CodeTypeReference(typeT)
			};
			staticMethod.Parameters.Add(CodeDomHelper.CreateParameter(parameterName, parameterType));
			CodeExpression parameterExp = new CodeVariableReferenceExpression(parameterName);
			if (!useAutoTyping)
			{
				CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
				string str = string.Concat(methodName, "<", CodeDomHelper.GetInnerType(typeT, typeT1), ">");
				codeExpressionArray = new CodeExpression[] { parameterExp };
				CodeMethodInvokeExpression methodCall = CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, str, codeExpressionArray);
				if (typeT1 != null)
				{
					methodCall.Parameters.Add(CodeDomHelper.SingletonTypeManager());
				}
				staticMethod.Statements.Add(new CodeMethodReturnStatement(methodCall));
			}
			else
			{
				CodeStatementCollection statements = staticMethod.Statements;
				CodeTypeReference returnType = staticMethod.ReturnType;
				CodeTypeReferenceExpression codeTypeReferenceExpression1 = new CodeTypeReferenceExpression("XTypedServices");
				codeExpressionArray = new CodeExpression[2];
				CodeTypeReferenceExpression codeTypeReferenceExpression2 = new CodeTypeReferenceExpression("XElement");
				CodeExpression[] codeExpressionArray1 = new CodeExpression[] { parameterExp };
				codeExpressionArray[0] = CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression2, methodName, codeExpressionArray1);
				codeExpressionArray[1] = CodeDomHelper.SingletonTypeManager();
				statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(returnType, CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression1, "ToXTypedElement", codeExpressionArray))));
			}
			return staticMethod;
		}

		public static CodeTypeDeclaration CreateTypeDeclaration(string clrTypeName, string innerType)
		{
			CodeTypeDeclaration typeDecl = new CodeTypeDeclaration(clrTypeName)
			{
				TypeAttributes = TypeAttributes.Public,
				IsPartial = true
			};
			return typeDecl;
		}

		public static CodeMemberProperty CreateTypeManagerProperty()
		{
			CodeMemberProperty property = CodeDomHelper.CreateInterfaceImplProperty("TypeManager", "IXMetaData", new CodeTypeReference("ILinqToXsdTypeManager"));
			property.GetStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.SingletonTypeManager()));
			return property;
		}

		public static CodeMemberProperty CreateTypeOriginProperty(SchemaOrigin typeOrigin)
		{
			CodeTypeReference originType = new CodeTypeReference("SchemaOrigin");
			CodeMemberProperty property = CodeDomHelper.CreateInterfaceImplProperty("TypeOrigin", "IXMetaData", originType);
			property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(originType), (typeOrigin == SchemaOrigin.Element ? "Element" : "Fragment"))));
			return property;
		}

		public static CodeTypeReference CreateTypeReference(string type)
		{
			return new CodeTypeReference(type);
		}

		public static CodeTypeReferenceExpression CreateTypeReferenceExp(string type)
		{
			return new CodeTypeReferenceExpression(type);
		}

		public static CodeConstructor CreateXRootFunctionalConstructor(string typeName)
		{
			CodeConstructor constructor = CodeDomHelper.CreateConstructor(MemberAttributes.Public);
			constructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeName), "root"));
			CodeStatementCollection statements = constructor.Statements;
			CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(CodeDomHelper.This(), "doc");
			CodeExpression[] codePropertyReferenceExpression = new CodeExpression[] { new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("root"), "Untyped") };
			statements.Add(new CodeAssignStatement(codeFieldReferenceExpression, new CodeObjectCreateExpression("XDocument", codePropertyReferenceExpression)));
			constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(CodeDomHelper.This(), "rootObject"), new CodeVariableReferenceExpression("root")));
			return constructor;
		}

		public static CodeSnippetTypeMember CreateXRootGetter(string typeName, string fqTypeName, LocalSymbolTable lst)
		{
			string symbolName = lst.AddMember(typeName);
			CodeSnippetTypeMember castMember = new CodeSnippetTypeMember();
			string[] strArrays = new string[] { "\r\n", "    public ", fqTypeName, " ", symbolName, " {  get {", "return rootObject as ", fqTypeName, "; } }" };
			castMember.Text = string.Concat(strArrays);
			return castMember;
		}

		public static CodeMemberMethod CreateXRootMethod(string returnType, string methodName, string[][] paramList)
		{
			CodeTypeReference xRootType = new CodeTypeReference(returnType);
			CodeMemberMethod staticMethod = new CodeMemberMethod()
			{
				Name = methodName,
				Attributes = MemberAttributes.Abstract | MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public,
				ReturnType = xRootType
			};
			CodeExpression[] parameterExp = new CodeExpression[(int)paramList.Length];
			for (int i = 0; i < (int)paramList.Length; i++)
			{
				string[] paramRef = paramList[i];
				staticMethod.Parameters.Add(CodeDomHelper.CreateParameter(paramRef[1], paramRef[0]));
				parameterExp[i] = new CodeVariableReferenceExpression(paramRef[1]);
			}
			CodeExpression rootExp = new CodeVariableReferenceExpression("root");
			CodeExpression doc = new CodeFieldReferenceExpression(rootExp, "doc");
			staticMethod.Statements.Add(new CodeVariableDeclarationStatement(xRootType, "root", new CodeObjectCreateExpression(xRootType, new CodeExpression[0])));
			staticMethod.Statements.Add(new CodeAssignStatement(doc, CodeDomHelper.CreateMethodCall(new CodeTypeReferenceExpression("XDocument"), methodName, parameterExp)));
			CodeStatementCollection statements = staticMethod.Statements;
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
			CodeExpression[] codePropertyReferenceExpression = new CodeExpression[] { new CodePropertyReferenceExpression(doc, "Root"), CodeDomHelper.SingletonTypeManager() };
			statements.Add(new CodeVariableDeclarationStatement("XTypedElement", "typedRoot", CodeDomHelper.CreateMethodCall(codeTypeReferenceExpression, "ToXTypedElement", codePropertyReferenceExpression)));
			CodeStatementCollection codeStatementCollection = staticMethod.Statements;
			CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("typedRoot"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			CodeStatement[] codeThrowExceptionStatement = new CodeStatement[1];
			codePropertyReferenceExpression = new CodeExpression[] { new CodePrimitiveExpression("Invalid root element in xml document.") };
			codeThrowExceptionStatement[0] = new CodeThrowExceptionStatement(new CodeObjectCreateExpression("LinqToXsdException", codePropertyReferenceExpression));
			codeStatementCollection.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeThrowExceptionStatement));
			staticMethod.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(rootExp, "rootObject"), new CodeVariableReferenceExpression("typedRoot")));
			staticMethod.Statements.Add(new CodeMethodReturnStatement(rootExp));
			return staticMethod;
		}

		public static CodeMemberMethod CreateXRootSave(string[][] paramList)
		{
			CodeMemberMethod staticMethod = new CodeMemberMethod()
			{
				Name = "Save",
				Attributes = MemberAttributes.Public
			};
			CodeExpression[] parameterExp = new CodeExpression[(int)paramList.Length];
			for (int i = 0; i < (int)paramList.Length; i++)
			{
				string[] paramRef = paramList[i];
				staticMethod.Parameters.Add(CodeDomHelper.CreateParameter(paramRef[1], paramRef[0]));
				parameterExp[i] = new CodeVariableReferenceExpression(paramRef[1]);
			}
			CodeExpression doc = new CodeVariableReferenceExpression("doc");
			staticMethod.Statements.Add(CodeDomHelper.CreateMethodCall(doc, "Save", parameterExp));
			return staticMethod;
		}

		public static string GetInnerType(string wrappingType, string wrappedType)
		{
			string str;
			str = (wrappedType != null ? string.Concat(wrappingType, ", ", wrappedType) : wrappingType);
			return str;
		}

		public static CodeAttributeDeclaration SchemaProviderAttribute(string typeName)
		{
			CodeAttributeDeclaration customAtt = new CodeAttributeDeclaration("XmlSchemaProviderAttribute");
			customAtt.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(string.Concat(typeName, "SchemaProvider"))));
			return customAtt;
		}

		public static CodePropertySetValueReferenceExpression SetValue()
		{
			return new CodePropertySetValueReferenceExpression();
		}

		public static CodePropertyReferenceExpression SingletonTypeManager()
		{
			return new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(NameGenerator.GetServicesClassName()), "Instance");
		}

		public static CodeThisReferenceExpression This()
		{
			return new CodeThisReferenceExpression();
		}

		public static CodeTypeOfExpression Typeof(string typeName)
		{
			return new CodeTypeOfExpression(typeName);
		}

		public static CodeMethodInvokeExpression XNameGetExpression(string name, string ns)
		{
			CodeMethodInvokeExpression codeMethodInvokeExpression = CodeDomHelper.XNameGetExpression(new CodePrimitiveExpression(name), new CodePrimitiveExpression(ns));
			return codeMethodInvokeExpression;
		}

		public static CodeMethodInvokeExpression XNameGetExpression(CodeExpression name, CodeExpression ns)
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XName");
			CodeExpression[] codeExpressionArray = new CodeExpression[] { name, ns };
			return new CodeMethodInvokeExpression(codeTypeReferenceExpression, "Get", codeExpressionArray);
		}
	}
}