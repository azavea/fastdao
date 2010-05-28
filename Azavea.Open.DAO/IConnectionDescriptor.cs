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
    /// This interface represents the information needed to establish a connection to a data source.
    /// 
    /// Any class that implements this interface should be thread safe.  If not, it should be well
    /// documented.
    /// </summary>
    public interface IConnectionDescriptor
    {
        /// <summary>
        /// Since we often need to represent database connection info as strings,
        /// child classes must implement ToCompleteString() such that this.Equals(that) and
        /// this.ToCompleteString().Equals(that.ToCompleteString()) will behave the same.
        /// </summary>
        /// <returns>A string representation of all of the connection info.</returns>
        string ToCompleteString();

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
        string ToCleanString();

        /// <summary>
        /// Returns the appropriate data access layer for this connection.
        /// </summary>
        IDaLayer CreateDataAccessLayer();
    }
}