// Copyright (c) 2004-2010 Azavea, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using Azavea.Open.Common;
using Azavea.Open.Common.Collections;

namespace Azavea.Open.DAO.Memory
{
    /// <summary>
    /// Describes a connection to "memory".  This is mostly intended as a test
    /// implementation, the data source is just a structure in memory, but it is
    /// possible this will have some practical applications as well.
    /// 
    /// Two connections using the same uid will hit the same in-memory collection
    /// of objects.
    /// </summary>
    public class MemoryDescriptor : ConnectionDescriptor
    {
        private static readonly CheckedDictionary<string,MemoryDaLayer> _daLayers =
            new CheckedDictionary<string, MemoryDaLayer>();
        private readonly string _connectionString;
        /// <summary>
        /// The ID of the in-memory store this descriptor connects to.
        /// </summary>
        public readonly string Uid;

        /// <summary>
        /// Populates the descriptor's values from a config file.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public MemoryDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(config.GetParameter(component, "UID")) { }
        /// <summary>
        /// Instantiate an in-memory datastore connection.
        /// </summary>
        /// <param name="uid">The ID of the in-memory store this descriptor connects to.</param>
        public MemoryDescriptor(string uid)
        {
            _connectionString = "MemoryStore_" + uid;
            Uid = uid;
        }

        /// <summary>
        /// Since we often need to represent database connection info as strings,
        /// child classes must implement ToCompleteString() such that this.Equals(that) and
        /// this.ToCompleteString().Equals(that.ToCompleteString()) will behave the same.
        /// </summary>
        /// <returns>A string representation of all of the connection info.</returns>
        public override string ToCompleteString()
        {
            return _connectionString;
        }

        /// <summary>
        /// This method is similar to ToString, except it will not contain any
        /// "sensitive" information, I.E. passwords.
        /// 
        /// This method is intended to be used for logging or error handling, where
        /// we do not want to display passwords to (potentially) just anyone, but
        /// we do want to indicate what DB connection we were using.
        /// </summary>
        /// <returns>A string representation of most of the connection info, except
        ///          passwords or similar items that shouldn't be shown.</returns>
        public override string ToCleanString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Returns the appropriate data access layer for this connection.
        /// </summary>
        public override IDaLayer CreateDataAccessLayer()
        {
            MemoryDaLayer layer;
            lock (_daLayers)
            {
                if (_daLayers.ContainsKey(Uid))
                {
                    layer = _daLayers[Uid];
                }
                else
                {
                    layer = new MemoryDaLayer(this);
                    _daLayers[Uid] = layer;
                }
            }
            return layer;
        }
    }
}
