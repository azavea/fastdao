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

namespace Azavea.Open.DAO.Criteria.Grouping
{
    /// <summary>
    /// A group expression that aggregates based on values of a data member.
    /// This is the most common case.  I.E. aggregating based on a "Type"
    /// field or property on the data object.
    /// </summary>
    public class MemberGroupExpression : AbstractGroupExpression
    {
        /// <summary>
        /// The name of the data member (field/property) on the object to
        /// group by.
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        /// Creates the expression using the member name as the group name.
        /// </summary>
        /// <param name="memberName">The name of the data member (field/property) on the object to
        ///                          group by.</param>
        public MemberGroupExpression(string memberName)
            : base(memberName)
        {
            MemberName = memberName;
        }
        /// <summary>
        /// Create the expression with an explicit name to use instead of the data member name
        /// when including it in the GroupCountResult.
        /// </summary>
        /// <param name="memberName">The name of the data member (field/property) on the object to
        ///                          group by.</param>
        /// <param name="groupName">The name to call it in the GroupCountResult's collection.</param>
        public MemberGroupExpression(string memberName, string groupName)
            : base(groupName)
        {
            MemberName = memberName;
        }
    }
}
