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

namespace Azavea.Open.DAO.CSV
{
    /// <summary>
    /// CSV descriptors may be configured with:
    /// 
    /// 1) a directory (in which case all mappings will be assumed to be to [ClassMapping.Table].csv)
    /// 
    /// 2) a filename (in which case ClassMapping.Table is ignored, using this descriptor with
    ///    DAOs of more than one type will result in undefined, but probably undesirable, behavior)
    /// 
    /// 3) a stream (programmatically only, the descriptor assumes the caller owns the stream,
    ///    in other words the descriptor and the DAO will not close the stream when they are done
    ///    using it).  Streams are also limited in that they can be used for querying OR inserting,
    ///    not both, also update and delete are not supported when using a stream.
    /// </summary>
    public enum CsvConnectionType
    {
        /// <summary>
        /// Used internally to indicate the type is not known.
        /// </summary>
        Unknown,
        /// <summary>
        /// The "Table" name in the mapping file will be assumed to be the filename
        /// (without the extension), in the specified directory.  This way the connection works
        /// with multiple DAOs at once.
        /// </summary>
        Directory,
        /// <summary>
        /// The "Table" name in the mapping file is ignored, all IO goes to the
        /// specified filename, and this connection should be used only with DAOs of a single
        /// object type.
        /// </summary>
        FileName,
        /// <summary>
        /// The "Table" name in the mapping file is ignored, all queries will run against
        /// the specified TextReader (which means we'll need to seek back to the start
        /// of the input for each query).  Insert, Update, Delete, etc will not work.
        /// </summary>
        Reader,
        /// <summary>
        /// The "Table" name in the mapping file is ignored, all inserts will be written to
        /// the specified TextWriter.  Query, Update, Delete, etc will not work.
        /// </summary>
        Writer
    }
}