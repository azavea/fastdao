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
using System.Collections.Generic;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Some data access layers may optionally implement "DDL" tasks (creating and
    /// destroying data sources, indexing the data, etc).  If so, they will implement
    /// this interface.
    /// 
    /// Documentation for each particular data access layer should be explicit about
    /// which methods are implemented vs. which ones throw NotImplementedExceptions,
    /// since some methods may be impractical to implement on certain data sources.
    /// 
    /// House vs. Room Metaphor:  Some data sources are capable of storing more than
    /// one type of data (I.E. multiple tables in a database).  These correspond to
    /// "rooms".  Some data sources are capable of storing multiple groups of "rooms"
    /// in one data source, I.E. multiple "databases" on a single server.
    /// 
    /// IConnectionDescriptors typically point at a single StoreHouse, but may be used
    /// with multiple FastDAOs to store different types of data in different StoreRooms.
    /// 
    /// Examples:
    ///              House        Room
    /// Oracle       N/A          Table
    /// SQL Server   Database     Table
    /// CSV          N/A          File
    /// SQLite       File         Table
    /// 
    /// Implementors of this should ask themselves "What if someone switched to a different
    /// data access layer, would they have to change their code?"  In other words, make the
    /// methods that don't apply act as sensibly as possible.  Examples are given in the
    /// descriptions of the specific methods.
    /// </summary>
    public interface IDaDdlLayer
    {
        /// <summary>
        /// Indexes the data for faster queries.  Some data sources may not support indexes
        /// (such as CSV files), in which case this should throw a NotSupportedException.
        /// 
        /// If the data source supports indexes, but support for creating them is not yet
        /// implemented, this should throw a NotImplementedException.
        /// </summary>
        /// <param name="name">Name of the index.  Some data sources require names for indexes,
        ///                    and even if not this is required so the index can be deleted if desired.</param>
        /// <param name="mapping">ClassMapping for the data that is being indexed.</param>
        /// <param name="propertyNames">Names of the data properties to include in the index (in order).</param>
        void CreateIndex(string name, ClassMapping mapping, ICollection<string> propertyNames);
        /// <summary>
        /// Removes an index on the data for slower queries (but usually faster inserts/updates/deletes).
        /// Some data sources may not support indexes (such as CSV files), 
        /// in which case this method should be a no-op.
        /// 
        /// If the data source supports indexes, but support for deleting them is not yet
        /// implemented, this should throw a NotImplementedException.
        /// </summary>
        /// <param name="name">Name of the index to delete.</param>
        /// <param name="mapping">ClassMapping for the data that was being indexed.</param>
        void DeleteIndex(string name, ClassMapping mapping);
        /// <summary>
        /// Returns whether an index with this name exists or not.  NOTE: This does NOT
        /// verify what properties the index is on, merely whether an index with this
        /// name is already present.
        /// </summary>
        /// <param name="name">Name of the index to check for.</param>
        /// <param name="mapping">ClassMapping for the data that may be indexed.</param>
        /// <returns>Whether an index with this name exists in the data source.</returns>
        bool IndexExists(string name, ClassMapping mapping);
        /// <summary>
        /// Sequences are things that automatically generate unique, usually incrementing,
        /// numbers.  Some data sources may not support sequences, in which case this should
        /// throw a NotSupportedException.
        /// 
        /// If the data source supports sequences, but support for creating them is not yet
        /// implemented, this should throw a NotImplementedException.
        /// </summary>
        /// <param name="name">Name of the new sequence to create.</param>
        void CreateSequence(string name);
        /// <summary>
        /// Removes a sequence.  Some data sources may not support sequences, 
        /// in which case this method should be a no-op.
        /// 
        /// If the data source supports sequences, but support for deleting them is not yet
        /// implemented, this should throw a NotImplementedException.
        /// </summary>
        /// <param name="name">Name of the sequence to delete.</param>
        void DeleteSequence(string name);
        /// <summary>
        /// Returns whether a sequence with this name exists or not.
        /// </summary>
        /// <param name="name">Name of the sequence to check for.</param>
        /// <returns>Whether a sequence with this name exists in the data source.</returns>
        bool SequenceExists(string name);
        /// <summary>
        /// Creates the store house specified in the connection descriptor.  If this
        /// data source doesn't use a store house, this method should be a no-op.
        /// 
        /// If this data source DOES use store houses, but support for adding
        /// them is not implemented yet, this should throw a NotImplementedException.
        /// 
        /// Store house typically corresponds to "database".
        /// </summary>
        void CreateStoreHouse();
        /// <summary>
        /// Deletes the store house specified in the connection descriptor.  If this
        /// data source doesn't use a store house, this method should be a no-op.
        /// 
        /// If this data source DOES use store houses, but support for dropping
        /// them is not implemented yet, this should throw a NotImplementedException.
        /// 
        /// Store house typically corresponds to "database".
        /// </summary>
        void DeleteStoreHouse();
        /// <summary>
        /// Returns true if you need to call "CreateStoreHouse" before storing any
        /// data.  This method is "Missing" not "Exists" because implementations that
        /// do not use a store house (I.E. single-file-based data access layers) can
        /// return "false" from this method without breaking either a user's app or the
        /// spirit of the method.
        /// 
        /// Store house typically corresponds to "database".
        /// </summary>
        /// <returns>Returns true if you need to call "CreateStoreHouse"
        ///          before storing any data.</returns>
        bool StoreHouseMissing();
        /// <summary>
        /// Creates the store room specified in the connection descriptor.  If this
        /// data source doesn't use a store room, this method should be a no-op.
        /// 
        /// If this data source DOES use store rooms, but support for adding
        /// them is not implemented yet, this should throw a NotImplementedException.
        /// 
        /// Store room typically corresponds to "table".
        /// </summary>
        /// <param name="mapping">ClassMapping for the data that will be stored in this room.</param>
        void CreateStoreRoom(ClassMapping mapping);
        /// <summary>
        /// Deletes the store room specified in the connection descriptor.  If this
        /// data source doesn't use a store room, this method should be a no-op.
        /// 
        /// If this data source DOES use store rooms, but support for adding
        /// them is not implemented yet, this should throw a NotImplementedException.
        /// 
        /// Store room typically corresponds to "table".
        /// </summary>
        /// <param name="mapping">ClassMapping for the data that was stored in this room.</param>
        void DeleteStoreRoom(ClassMapping mapping);
        /// <summary>
        /// Returns true if you need to call "CreateStoreRoom" before storing any
        /// data.  This method is "Missing" not "Exists" because implementations that
        /// do not use a store room can return "false" from this method without
        /// breaking either a user's app or the spirit of the method.
        /// 
        /// Store room typically corresponds to "table".
        /// </summary>
        /// <returns>Returns true if you need to call "CreateStoreRoom"
        ///          before storing any data.</returns>
        bool StoreRoomMissing();
        /// <summary>
        /// Uses some form of introspection to determine what data is stored in
        /// this data store, and generates a ClassMapping that can be immediately
        /// used with a DictionaryDAO.  As much data as practical will be populated
        /// on the ClassMapping, at a bare minimum the Table (typically set to
        /// the storeRoomName passed in, or the more correct or fully qualified version
        /// of that name), the TypeName (set to the storeRoomName, since we have no
        /// .NET type), and the "data cols" and "obj attrs" will be the list of 
        /// attributes / columns in the data source, mapped to themselves.
        /// </summary>
        /// <param name="storeRoomName">The name of the storeroom (I.E. table).  May be null
        ///                             if this data source does not use store rooms.</param>
        /// <param name="columnSorter">If you wish the columns / attributes to be in a particular
        ///                            order, supply this optional parameter.  May be null.</param>
        /// <returns>A ClassMapping that can be used with a DictionaryDAO.</returns>
        ClassMapping GenerateClassMappingFromStoreRoom(string storeRoomName,
                                                       IComparer<ClassMapColDefinition> columnSorter);
    }
}