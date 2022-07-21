using System;
using System.IO;
using System.Xml;

namespace TrueCraft.Core.Test
{
    public static class Utility
    {
        /// <summary>
        /// Gets the top node from the given XML
        /// </summary>
        /// <param name="xml">An XML document in a string.</param>
        /// <returns>The root node of the XML Document.</returns>
        public static XmlNode GetTopNode(string xml)
        {
            XmlDocument doc = new XmlDocument();
            using (StringReader sr = new StringReader(xml))
            using (XmlReader xmlr = XmlReader.Create(sr))
                doc.Load(xmlr);

            return doc.FirstChild!;
        }
    }
}

