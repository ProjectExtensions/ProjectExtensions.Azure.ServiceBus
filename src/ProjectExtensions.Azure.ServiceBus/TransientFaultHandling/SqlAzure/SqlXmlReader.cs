//=======================================================================================
// Transient Fault Handling Framework for SQL Azure, Storage, Service Bus & Cache
//
// This sample is supplemental to the technical guidance published on the Windows Azure
// Customer Advisory Team blog at http://windowsazurecat.com/. 
//
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.SqlAzure
{
    #region Using references
    using System;
    using System.Xml;
    using System.Data;
    using System.Data.SqlClient;
    #endregion

    /// <summary>
    /// Provides a disposable wrapper for SQL XML data reader which synchronizes the SQL connection
    /// disposal with its own lifecycle.
    /// </summary>
    internal class SqlXmlReader : XmlReader
    {
        #region Private members
        private readonly IDbConnection connection;
        private readonly XmlReader innerReader;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of a <see cref="SqlXmlReader"/> object associated with the specified SQL command.
        /// </summary>
        /// <param name="command">The associated SQL command providing access to the XML data for the reader.</param>
        public SqlXmlReader(SqlCommand command)
        {
            Guard.ArgumentNotNull(command, "command");

            this.connection = command.Connection;
            this.innerReader = command.ExecuteXmlReader();
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="SqlXmlReader"/> object associated with the specified SQL connection and the original XML reader.
        /// </summary>
        /// <param name="connection">The associated SQL connection providing access to the XML data for this reader.</param>
        /// <param name="innerReader">The original XML reader access to the XML data for this reader.</param>
        public SqlXmlReader(IDbConnection connection, XmlReader innerReader)
        {
            Guard.ArgumentNotNull(connection, "connection");
            Guard.ArgumentNotNull(innerReader, "innerReader");

            this.connection = connection;
            this.innerReader = innerReader;
        } 
        #endregion

        #region XmlReader implementation
        /// <summary>
        /// Closes both the original <see cref="System.Xml.XmlReader"/> as well as associated SQL connection.
        /// </summary>
        public override void Close()
        {
            if (this.innerReader != null)
            {
                this.innerReader.Close();
            }

            if (this.connection != null)
            {
                this.connection.Close();
            }
        }

        /// <summary>
        /// Returns the number of attributes on the current node.
        /// </summary>
        public override int AttributeCount
        {
            get { return this.innerReader.AttributeCount; }
        }

        /// <summary>
        /// Returns the base URI of the current node.
        /// </summary>
        public override string BaseURI
        {
            get { return this.innerReader.BaseURI; }
        }

        /// <summary>
        /// Returns the depth of the current node in the XML document.
        /// </summary>
        public override int Depth
        {
            get { return this.innerReader.Depth; }
        }

        /// <summary>
        /// Returns a value indicating whether the reader is positioned at the end of the stream.
        /// </summary>
        public override bool EOF
        {
            get { return this.innerReader.EOF; }
        }

        /// <summary>
        /// Returns the value of the attribute with the specified index.
        /// </summary>
        /// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.)</param>
        /// <returns>The value of the specified attribute. This method does not move the reader.</returns>
        public override string GetAttribute(int i)
        {
            return this.innerReader.GetAttribute(i);
        }

        /// <summary>
        /// Returns the value of the attribute with the specified <see cref="System.Xml.XmlReader.LocalName"/> and <see cref="System.Xml.XmlReader.NamespaceURI"/>.
        /// </summary>
        /// <param name="name">The local name of the attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute.</param>
        /// <returns>The value of the specified attribute. If the attribute is not found or the value is <see cref="String.Empty"/>, null is returned. This method does not move the reader.</returns>
        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.innerReader.GetAttribute(name, namespaceURI);
        }

        /// <summary>
        /// Returns the value of the attribute with the specified <see cref="System.Xml.XmlReader.Name"/>.
        /// </summary>
        /// <param name="name">The qualified name of the attribute.</param>
        /// <returns>The value of the specified attribute. If the attribute is not found or the value is <see cref="String.Empty"/>, null is returned.</returns>
        public override string GetAttribute(string name)
        {
            return this.innerReader.GetAttribute(name);
        }

        /// <summary>
        /// Returns a value indicating whether the current node can have a <see cref="System.Xml.XmlReader.Value"/>.
        /// </summary>
        public override bool HasValue
        {
            get { return this.innerReader.HasValue; }
        }

        /// <summary>
        /// Returns a value indicating whether the current node is an empty element.
        /// </summary>
        public override bool IsEmptyElement
        {
            get { return this.innerReader.IsEmptyElement; }
        }

        /// <summary>
        /// Returns the local name of the current node.
        /// </summary>
        public override string LocalName
        {
            get { return this.innerReader.LocalName; }
        }

        /// <summary>
        /// Resolves a namespace prefix in the current element's scope
        /// </summary>
        /// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string.</param>
        /// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
        public override string LookupNamespace(string prefix)
        {
            return this.innerReader.LookupNamespace(prefix);
        }

        /// <summary>
        /// Moves to the attribute with the specified <see cref="System.Xml.XmlReader.LocalName"/> and <see cref="System.Xml.XmlReader.NamespaceURI"/>.
        /// </summary>
        /// <param name="name">The local name of the attribute.</param>
        /// <param name="ns">The namespace URI of the attribute.</param>
        /// <returns>True if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
        public override bool MoveToAttribute(string name, string ns)
        {
            return this.innerReader.MoveToAttribute(name, ns);
        }

        /// <summary>
        /// Moves to the attribute with the specified <see cref="System.Xml.XmlReader.Name"/>.
        /// </summary>
        /// <param name="name">The qualified name of the attribute.</param>
        /// <returns>True if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
        public override bool MoveToAttribute(string name)
        {
            return this.innerReader.MoveToAttribute(name);
        }

        /// <summary>
        /// Moves to the element that contains the current attribute node.
        /// </summary>
        /// <returns>True if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); false if the reader is not positioned on an attribute (the position of the reader does not change).</returns>
        public override bool MoveToElement()
        {
            return this.innerReader.MoveToElement();
        }

        /// <summary>
        /// Moves to the first attribute.
        /// </summary>
        /// <returns>True if an attribute exists (the reader moves to the first attribute); otherwise, false (the position of the reader does not change).</returns>
        public override bool MoveToFirstAttribute()
        {
            return this.innerReader.MoveToFirstAttribute();
        }

        /// <summary>
        /// Moves to the next attribute.
        /// </summary>
        /// <returns>True if there is a next attribute; false if there are no more attributes.</returns>
        public override bool MoveToNextAttribute()
        {
            return this.innerReader.MoveToNextAttribute();
        }

        /// <summary>
        /// Returns the <see cref="System.Xml.XmlNameTable"/> associated with this implementation.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get { return this.innerReader.NameTable; }
        }

        /// <summary>
        /// Returns the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
        /// </summary>
        public override string NamespaceURI
        {
            get { return this.innerReader.NamespaceURI; }
        }

        /// <summary>
        /// Returns the type of the current node.
        /// </summary>
        public override XmlNodeType NodeType
        {
            get { return this.innerReader.NodeType; }
        }

        /// <summary>
        /// Returns the namespace prefix associated with the current node
        /// </summary>
        public override string Prefix
        {
            get { return this.innerReader.Prefix; }
        }

        /// <summary>
        /// Reads the next node from the stream.
        /// </summary>
        /// <returns>True if the next node was read successfully; false if there are no more nodes to read.</returns>
        public override bool Read()
        {
            return this.innerReader.Read();
        }

        /// <summary>
        /// Parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.
        /// </summary>
        /// <returns>True if there are nodes to return, false if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read. An empty attribute, such as, misc="", returns true with a single node with a value of <see cref="String.Empty"/>.</returns>
        public override bool ReadAttributeValue()
        {
            return this.innerReader.ReadAttributeValue();
        }

        /// <summary>
        /// Returns the state of the reader.
        /// </summary>
        public override ReadState ReadState
        {
            get { return this.innerReader.ReadState; }
        }

        /// <summary>
        /// Resolves the entity reference for EntityReference nodes.
        /// </summary>
        public override void ResolveEntity()
        {
            this.innerReader.ResolveEntity();
        }

        /// <summary>
        /// Returns the text value of the current node.
        /// </summary>
        public override string Value
        {
            get { return this.innerReader.Value; }
        }
        #endregion
    }
}
