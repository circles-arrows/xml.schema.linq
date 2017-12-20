using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public static class FxtDeanonymize
	{
		public static IEnumerable<IFxtTransformation> Deanonymize(this XmlSchemaSet schemas, FxtScope scope, bool strict, FxtLog log)
		{
			bool flag;
			bool flag1;
			bool flag2;
			XmlQualifiedName xmlQualifiedName;
			Dictionary<string, int> strs = new Dictionary<string, int>();
			foreach (XmlSchemaElement xmlSchemaElement in schemas.LocalXsdElements())
			{
				flag = (xmlSchemaElement.SchemaType == null ? true : !(xmlSchemaElement.SchemaType is XmlSchemaComplexType));
				if (flag)
				{
					continue;
				}
				if (strs.ContainsKey(xmlSchemaElement.Name))
				{
					Dictionary<string, int> item = strs;
					Dictionary<string, int> strs1 = item;
					string name = xmlSchemaElement.Name;
					item[name] = strs1[name] + 1;
				}
				else
				{
					strs.Add(xmlSchemaElement.Name, 1);
				}
			}
			IEnumerator<XmlSchemaElement> enumerator = schemas.LocalXsdElements().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					XmlSchemaElement current = enumerator.Current;
					flag1 = (current.SchemaType == null || !(current.SchemaType is XmlSchemaComplexType) ? true : !scope.Test(current));
					if (flag1)
					{
						continue;
					}
					xmlQualifiedName = new XmlQualifiedName(current.Name, current.XmlSchema().TargetNamespace);
					flag2 = (schemas.DefinesXsdType(xmlQualifiedName) ? false : strs[current.Name] <= 1);
					if (flag2)
					{
						ExtractType extractType = new ExtractType()
						{
							element = current
						};
						log.AtType(xmlQualifiedName).Add(new ExtractTypeAnnotation());
						yield return extractType;
					}
					else if (strict)
					{
						throw new FxtTypeClashException(xmlQualifiedName);
					}
				}
				goto Label1;
				throw new FxtTypeClashException(xmlQualifiedName);
			}
			finally
			{
				if (enumerator != null)
				{
					enumerator.Dispose();
				}
			}
		Label1:
			yield break;
		}
	}
}