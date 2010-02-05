// Copyright (c) 2004-2010 Avencia, Inc.
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

using System.Collections;

namespace Avencia.Open.DAO
{
    /// <summary>
    /// This describes a method to be executed by the QueryAndIterateOverObjects call.  This delegate
    /// will be called once per record returned by the query and passed the data object that was
    /// created for that record.
    /// </summary>
    /// <param name="parameters">A dictionary containing anything at all.  This is used as
    ///                          a way of passing parameters to the delegate, or as a way
    ///                          for the delegate to return values to the function that called
    ///                          it through the Iterate method.
    ///                          This parameter may be null.</param>
    /// <param name="dataObject">A single data object retrieved from a query.</param>
    public delegate void DaoIterationDelegate<T>(Hashtable parameters, T dataObject);
}