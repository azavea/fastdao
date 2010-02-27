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
    /// For simplicity's sake, it is possible you don't actually care (or don't know) what type
    /// of objects you're dealing with.  In that case, you can use this interface.
    /// </summary>
    public interface IUntypedJoinResult
    {
        /// <summary>
        /// The object from the left side of the join.  May be null if the join
        /// was an outer join.
        /// </summary>
        object LeftObject { get; }
        /// <summary>
        /// The object from the right side of the join.  May be null if the join
        /// was an outer join.
        /// </summary>
        object RightObject { get; }
    }

    /// <summary>
    /// A join query returns objects representing a row from both DAOs.
    /// This object holds the two objects together.
    /// </summary>
    /// <typeparam name="L">The object type returned by the left DAO.</typeparam>
    /// <typeparam name="R">The object type returned by the right DAO.</typeparam>
    public class JoinResult<L, R> : IUntypedJoinResult where L : class where R : class
    {
        /// <summary>
        /// The object from the left side of the join.  May be null if the join
        /// was an outer join.
        /// </summary>
        public readonly L Left;
        /// <summary>
        /// The object from the right side of the join.  May be null if the join
        /// was an outer join.
        /// </summary>
        public readonly R Right;

        /// <summary>
        /// Creates a JoinResult.
        /// </summary>
        /// <param name="leftVal">The object from the left side of the join.
        ///                       May be null if the join was an outer join.</param>
        /// <param name="rightVal">The object from the right side of the join.  
        ///                        May be null if the join was an outer join.</param>
        public JoinResult(L leftVal, R rightVal)
        {
            Left = leftVal;
            Right = rightVal;
        }

        /// <summary>
        /// The object from the left side of the join.  Returns the same exact thing
        /// as ".Left", but cast to an object.  May be null if the join
        /// was an outer join.
        /// </summary>
        public object LeftObject
        {
            get { return Left; }
        }

        /// <summary>
        /// The object from the right side of the join.  Returns the same exact thing
        /// as ".Right", but cast to an object.  May be null if the join
        /// was an outer join.
        /// </summary>
        public object RightObject
        {
            get { return Right; }
        }

        /// <exclude/>
        public override string ToString()
        {
            return "[L: " + (Left == null ? "<null>" : Left.ToString()) +
                   ", R: " + (Right == null ? "<null>" : Right.ToString()) + "]";
        }

        /// <summary>
        /// For some implementations, it's simpler to only implement one-sided joins.  So
        /// to handle right (or left) joins, you want to flip the criteria so the right
        /// and left daos are swapped and you can do a left (or right) join instead.  Having
        /// this method here allows the results of such a flipped query to be flipped back,
        /// so you can return the correct result.
        /// </summary>
        /// <returns>A copy of this result with the left/right values swapped.</returns>
        public JoinResult<R,L> Flip()
        {
            return new JoinResult<R, L>(Right, Left);
        }
    }
}