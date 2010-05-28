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
using System.Collections.Generic;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Represents the results from a count of records grouped (aggregated) by some
    /// field or fields.
    /// </summary>
    public class GroupCountResult
    {
        /// <summary>
        /// The number of results found with this combination of group values.
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// The combination of group values that occurs Count times in the
        /// data source.  Group values are a single set of unique values
        /// for the fields you grouped by, with key being the name of the
        /// grouping and value being the value (I.E. key is "Type" value is "1"
        /// and key is "FirstName" value is "Bob", Count would then be the
        /// number of records in the data source with a Type of "1" and a
        /// FirstName of "Bob".
        /// </summary>
        public readonly IDictionary<string, object> GroupValues;

        /// <summary>
        /// Creates the result.
        /// </summary>
        public GroupCountResult(int count, IDictionary<string,object> values)
        {
            Count = count;
            GroupValues = values;
        }
    }
}
