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

using System;

namespace Avencia.Open.DAO.Criteria
{
    /// <summary>
    /// Indicates whether the given criteria is an AND or and OR.
    /// For example:
    /// Height &gt; 5 OR Width &gt; 5 =
    /// "Height [MatchType.Greater] 5 [BooleanType.Or]" and
    /// "Width [Matchtype.Greater] 5 [BooleanType.Or]"
    /// 
    /// Height &gt; 5 AND Width &gt; 5 =
    /// "Height [MatchType.Greater] 5 [BooleanType.AND]" and
    /// "Width [Matchtype.Greater] 5 [BooleanType.AND]"
    /// 
    /// To represent complicated criteria groups such as 
    /// "Height &gt; 5 AND (Weight &gt; 5 OR Weight == 0)", you need to use
    /// the CriteriaExpression to nest the OR expression inside another,
    /// AND criteria:
    /// DaoCriteria(heightExp AND DaoCriteria(weightExp1 OR weightExp2)).
    /// </summary>
    [Serializable]
    public enum BooleanOperator
    {
        /// <summary>
        /// Indicates both are required.
        /// </summary>
        And,
        /// <summary>
        /// Indicates either one or the other is required.
        /// </summary>
        Or
    }
}