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

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Represents a transaction (a guarauntee that a series of operations are
    /// executed against a connection without any other operations from other
    /// connections interfering).
    /// 
    /// NOTE: You MUST call Commit or Rollback when done!  If you do not, the destructor
    /// (assuming a nice shutdown where the destructor is called) will attempt to call
    /// Rollback, however this is just a "nice" attempt to clean up, and will not happen
    /// until the garbage collector collects this object, so it should not be relied upon.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Writes all the statements executed as part of this transaction to the
        /// database.  Until this is called, none of the inserts/updates/deletes
        /// you do will be visible to any other users of the database.
        /// </summary>
        void Commit();

        /// <summary>
        /// Undoes all the statements executed as part of this transaction.  The
        /// database will be as though you never executed them.
        /// </summary>
        void Rollback();
    }
}