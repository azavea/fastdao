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

using System;
using System.Data;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Util;
using log4net;

namespace Azavea.Open.DAO.SQL
{
    /// <summary>
    /// A simple implementation of an ITransaction for SQL data sources.
    /// </summary>
    public class SqlTransaction : ITransaction
    {
        private readonly static ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// Used by the data access layer, returns the internal object
        /// that can actually be used to perform transactional operations.
        /// </summary>
        public IDbTransaction Transaction;
        /// <summary>
        /// Used by the data access layer, returns the actual database
        /// connection that this transaction is open against.
        /// </summary>
        public IDbConnection Connection;
        /// <summary>
        /// The connection descriptor this transaction is operating against.
        /// </summary>
        public readonly AbstractSqlConnectionDescriptor ConnDesc;

        /// <summary>
        /// Create the interface object from a low-level object.
        /// </summary>
        /// <param name="connDesc">The connection descriptor this is a transaction for.</param>
        public SqlTransaction(AbstractSqlConnectionDescriptor connDesc)
        {
            if (connDesc == null)
            {
                throw new ArgumentNullException("connDesc", "Cannot create a transaction with a null connection.");
            }
            ConnDesc = connDesc;
            Connection = DbCaches.Connections.Get(ConnDesc);
            try
            {
                Transaction = Connection.BeginTransaction();
            }
            catch (Exception e)
            {
                DbCaches.Connections.Return(connDesc, Connection);
                throw new UnableToCreateTransactionException(connDesc, e);
            }
        }

        /// <summary>
        /// Writes all the statements executed as part of this transaction to the
        /// database.  Until this is called, none of the inserts/updates/deletes
        /// you do will be visible to any other users of the database.
        /// </summary>
        public void Commit()
        {
            if (Transaction == null)
            {
                throw new AlreadyTerminatedException(ConnDesc,
                                                     "Trying to commit a transaction that is already terminated.");
            }
            Transaction.Commit();
            Cleanup();
        }
        /// <summary>
        /// Undoes all the statements executed as part of this transaction.  The
        /// database will be as though you never executed them.
        /// </summary>
        public void Rollback()
        {
            if (Transaction == null)
            {
                throw new AlreadyTerminatedException(ConnDesc,
                                                     "Trying to roll back a transaction that is already terminated.");
            }
            Transaction.Rollback();
            Cleanup();
        }

        private void Cleanup()
        {
            DbCaches.Connections.Return(ConnDesc, Connection);
            Transaction = null;
            Connection = null;
        }

        /// <summary>
        /// When destroyed, if we haven't been terminated, attempt to roll back.
        /// </summary>
        ~SqlTransaction()
        {
            if (Transaction != null)
            {
                _log.Warn("Transaction was never terminated on connection " + ConnDesc +
                          ", attempting to roll back.");
                try
                {
                    Rollback();
                }
                catch (Exception e)
                {
                    _log.Error("Unable to roll back transaction on connection: " + ConnDesc, e);
                }
            }
        }
    }
}