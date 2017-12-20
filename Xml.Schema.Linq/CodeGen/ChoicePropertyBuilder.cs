using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Xml.Schema.Linq.CodeGen
{
    internal class ChoicePropertyBuilder : ContentModelPropertyBuilder
    {
        private List<CodeConstructor> choiceConstructors;

        private bool flatChoice;

        private bool hasDuplicateType;

        private Dictionary<string, ClrBasePropertyInfo> propertyTypeNameTable;

        public ChoicePropertyBuilder(GroupingInfo grouping, CodeTypeDeclaration decl, CodeTypeDeclItems declItems) : base(grouping, decl, declItems)
        {
            this.flatChoice = (!grouping.IsNested && !grouping.IsRepeating && !grouping.HasChildGroups);
            this.hasDuplicateType = false;
            if (this.flatChoice)
            {
                this.propertyTypeNameTable = new Dictionary<string, ClrBasePropertyInfo>();
            }
        }

        public override void GenerateConstructorCode(ClrBasePropertyInfo property)
        {
            if (this.flatChoice && !this.hasDuplicateType && property.ContentType != ContentType.WildCardProperty)
            {
                ClrBasePropertyInfo prevProperty = null;
                string propertyReturnType = property.ClrTypeName;
                if (this.propertyTypeNameTable.TryGetValue(propertyReturnType, out prevProperty))
                {
                    this.hasDuplicateType = true;
                }
                else
                {
                    this.propertyTypeNameTable.Add(propertyReturnType, property);
                    if (this.choiceConstructors == null)
                    {
                        this.choiceConstructors = new List<CodeConstructor>();
                    }
                    CodeConstructor choiceConstructor = CodeDomHelper.CreateConstructor(MemberAttributes.Public);
                    property.AddToConstructor(choiceConstructor);
                    this.choiceConstructors.Add(choiceConstructor);
                }
            }
        }

        public override void EndCodeGen()
        {
            if (this.choiceConstructors != null && !this.hasDuplicateType)
            {
                foreach (CodeConstructor choiceConst in this.choiceConstructors)
                {
                    this.decl.Members.Add(choiceConst);
                }
            }
        }

        public override CodeObjectCreateExpression CreateContentModelExpression()
        {
            return new CodeObjectCreateExpression(new CodeTypeReference("ChoiceContentModelEntity"), new CodeExpression[0]);
        }
    }
}
