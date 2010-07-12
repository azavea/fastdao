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
    /// If the data source supports transactions (multiple operations as an atomic unit)
    /// (such as most databases) the connection descriptor will implement this interface.
    /// 
    /// Usage: First call BeginTransaction, then pass the new ConnectionDescriptor
    ///        it gives you to every subsequent call (if you call a method but
    ///        do not pass the new ConnectionDescriptor, the method will not happen as part
    ///        of this transaction).  When complete, call Commit (or Rollback if
    ///        something went wrong).
    /// 
    /// NOTE: Since it is now up to you to complete the transaction, this is likely no longer threadsafe.
    /// </summary>
    public interface ITransactionalConnectionDescriptor : IConnectionDescriptor
    {
        /// <summary>
        /// Begins the transaction.  Returns a transaction object that you should
        /// use for operations you wish to be part of the transaction.
        /// 
        /// NOTE: You MUST call Commit or Rollback on the returned ITransaction when you are done.
        /// </summary>
        /// <returns>The Transaction object to pass to calls that you wish to have
        ///          happen as part of this transaction.</returns>
        ITransaction BeginTransaction();
    }
}