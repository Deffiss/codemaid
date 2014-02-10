﻿#region CodeMaid is Copyright 2007-2013 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License version 3 as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2013 Steve Cadwallader.

using System.Diagnostics;
using System.Text.RegularExpressions;
using EnvDTE;

namespace SteveCadwallader.CodeMaid.Helpers
{
    /// <summary>
    /// A set of helper methods focused around code comments.
    /// </summary>
    internal static class CodeCommentHelper
    {
        /// <summary>
        /// Creates the XML close tag string for an XElement.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The XML close tag, or <c>null</c> if the element has no value and is a self-closing tag.
        /// </returns>
        internal static string CreateXmlCloseTag(System.Xml.Linq.XElement element)
        {
            if (element.IsEmpty)
                return null;

            return string.Format("</{0}>", element.Name);
        }

        /// <summary>
        /// Creates the XML open tag string for an XElement.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The XML open tag. In case of an element without value, the tag is self-closing.
        /// </returns>
        internal static string CreateXmlOpenTag(System.Xml.Linq.XElement element)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("<");
            builder.Append(element.Name);
            if (element.HasAttributes)
            {
                foreach (var attr in element.Attributes())
                {
                    builder.Append(" ");
                    builder.Append(attr.ToString());
                }
            }

            if (element.IsEmpty)
            {
                builder.Append("/");
            }

            builder.Append(">");
            return builder.ToString();
        }

        /// <summary>
        /// Gets the regex for matching a complete code line, including leading whitespace and
        /// comment prefix.
        /// </summary>
        internal static Regex GetCodeRegexForDocument(TextDocument document)
        {
            var prefix = GetCommentPrefixForDocument(document);
            if (prefix == null)
                return null;

            var pattern = string.Format(@"^[\t ]*{0}(?!(\t| |\r|\n|$))", prefix);
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        }

        /// <summary>
        /// Get the comment prefix (regex) for the given document's language.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>The comment prefix regex, without trailing spaces.</returns>
        internal static string GetCommentPrefixForDocument(TextDocument document)
        {
            switch (document.Language)
            {
                case "CSharp":
                case "C/C++":
                case "CSS":
                case "JavaScript":
                case "JScript":
                case "LESS":
                case "TypeScript":
                    return "//+";

                case "Basic":
                    return "'+";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the regex for matching a complete comment line.
        /// </summary>
        internal static Regex GetCommentRegexForDocument(TextDocument document, bool includePrefix = true)
        {
            string prefix = null;
            if (includePrefix)
            {
                prefix = GetCommentPrefixForDocument(document);
                if (prefix == null)
                {
                    Debug.Fail("Attempting to create a comment regex for a document that has no comment prefix specified.");
                    return null;
                }

                prefix = string.Format(@"(?<prefix>[\t ]*{0})", prefix);
            }

            var pattern = string.Format(@"^{0}(?<line>(?<indent>[\t ]*)?(?<listprefix>[-=\*\+]+|\w+[\):]|\d+\.)?((?<words>[^\t\r\n ]+)*[\t ]*)*[\r\n]*)$", prefix);
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        }

        internal static int GetTabSize(CodeMaidPackage package, TextDocument document)
        {
            var settings = package.IDE.Properties["TextEditor", document.Language];
            var tabsize = settings.Item("TabSize").Value as int? ?? 4;
            return tabsize;
        }

        internal static bool IsCommentLine(EditPoint point)
        {
            return LineMatchesRegex(point, GetCommentRegexForDocument(point.Parent)).Success;
        }

        internal static Match LineMatchesRegex(EditPoint point, Regex regex)
        {
            return regex.Match(point.GetLine());
        }
    }
}