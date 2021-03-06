﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xaml;

namespace Granular.BuildTasks
{
    public interface ITypeParser
    {
        bool TryParseTypeName(string localName, string namespaceName, out string typeName);
    }

    public static class TypeParserExtensions
    {
        public static string ParseTypeName(this ITypeParser typeParser, XamlName name)
        {
            if (XamlLanguage.IsXamlType(name))
            {
                return String.Empty;
            }

            string typeName;

            if (!typeParser.TryParseTypeName(name.LocalName, name.NamespaceName, out typeName))
            {
                throw new Granular.Exception("Type \"{0}\" wasn't found", name);
            }

            return typeName;
        }
    }

    public class TypeParserCollection : ITypeParser
    {
        private IEnumerable<ITypeParser> parsers;

        public TypeParserCollection(params ITypeParser[] parsers)
        {
            this.parsers = parsers.ToArray();
        }

        public bool TryParseTypeName(string localName, string namespaceName, out string typeName)
        {
            foreach (ITypeParser parser in parsers)
            {
                if (parser.TryParseTypeName(localName, namespaceName, out typeName))
                {
                    return true;
                }
            }

            typeName = String.Empty;
            return false;
        }
    }

    public class MarkupExtensionTypeParser : ITypeParser
    {
        private ITypeParser typeParser;

        public MarkupExtensionTypeParser(ITypeParser typeParser)
        {
            this.typeParser = typeParser;
        }

        public bool TryParseTypeName(string localName, string namespaceName, out string typeName)
        {
            return typeParser.TryParseTypeName(localName, namespaceName, out typeName) ||
                typeParser.TryParseTypeName(String.Format("{0}Extension", localName), namespaceName, out typeName);
        }
    }
}
