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

using System.Collections.Generic;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Joins;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// This interface defines the "query" methods of FastDAO.
    /// </summary>
    /// <typeparam name="T">The type of object that can be written.</typeparam>
    public interface IFastDaoReader<T> : IFastDaoBase<T> where T : class, new()
    {
        /// <summary>
        /// Returns all objects of the given type.
        /// </summary>
        /// <returns>A list of objects, or an empty list (not null).</returns>
        IList<T> Get();

        /// <summary>
        /// Queries for objects where the property matches the given value.
        /// </summary>
        /// <param name="propertyName">Property or Field on the object you want to match a value.</param>
        /// <param name="propertyValue">Value that the Property or Field should have.</param>
        /// <returns>All objects that match the criteria, or an empty list (not null).</returns>
        IList<T> Get(string propertyName, object propertyValue);

        /// <summary>
        /// Queries and returns objects of the specified type matching the criteria.
        /// </summary>
        /// <param name="crit">The criteria that you wish the objects to match.  You can also
        ///                    specify a start/limit, ordering, etc.</param>
        /// <returns>A list of objects, or an empty list (not null).</returns>
        IList<T> Get(DaoCriteria crit);

        /// <summary>
        /// Performs a join, or the nearest possible equivilent depending on the data sources
        /// involved, and returns the results.  If you aren't familiar with joins, do some reading
        /// online.  This should behave like a "normal" database SQL join on all data sources.
        /// 
        /// When "joining" DAOs that are operating on different data sources, or data sources
        /// with no inherent join support (flat files for example), joins will be performed
        /// using the "PseudoJoiner" class, see that class for more details.
        /// </summary>
        /// <param name="crit">An object describing how to join the two DAOs.  Includes any
        ///                    criteria that apply to the right or left DAO.</param>
        /// <param name="rightDao">The other DAO we are joining against.</param>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <returns>A list of JoinResults, containing the matching objects from each DAO.  This is similar
        ///          to the way that </returns>
        List<JoinResult<T, R>> Get<R>(DaoJoinCriteria crit, IFastDaoReader<R> rightDao) where R : class, new();

        /// <summary>
        /// Returns the number of objects of the specified type matching the given criteria.
        /// </summary>
        /// <param name="crit">The criteria that you wish the objects to match.  Start/limit and order are ignored.</param>
        /// <returns>The number of objects that match the criteria.</returns>
        int GetCount(DaoCriteria crit);

        /// <summary>
        /// Performs a join using the given join criteria and returns the number of rows that
        /// result from the join.
        /// </summary>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <param name="crit">An object describing how to join the two DAOs.  Includes any
        ///                    criteria that apply to the right or left DAO.</param>
        /// <param name="rightDao">The other DAO we are joining against.</param>
        /// <returns>The number of join results that matched the criteria.</returns>
        int GetCount<R>(DaoJoinCriteria crit, IFastDaoReader<R> rightDao) where R : class, new();

        /// <summary>
        /// Queries for objects where the property matches the given value.
        /// </summary>
        /// <param name="propName">Property or Field on the object you want to match a value.</param>
        /// <param name="val">Value that the Property or Field should have.</param>
        /// <returns>The first object that matches the criteria.</returns>
        T GetFirst(string propName, object val);

        /// <summary>
        /// Queries for objects, similar to Get, except that this iterates over the resulting
        /// records and invokes the specified delegate for each one.  This allows processing of much
        /// larger result sets since it doesn't attempt to load all the objects into memory at once.
        /// </summary>
        /// <typeparam name="P">The type of the 'parameters' object taken by the delegate.</typeparam>
        /// <param name="criteria">Any criteria for the query.  May be null for "all records".</param>
        /// <param name="invokeMe">The method to invoke for each object returned by the query.</param>
        /// <param name="parameters">Any parameters that you want to pass to the invokeMe method.
        ///                            This may be null.</param>
        /// <param name="desc">Description of the loop for logging purposes.</param>
        /// <returns>The number of objects iterated over.</returns>
        int Iterate<P>(DaoCriteria criteria, DaoIterationDelegate<T, P> invokeMe,
                                   P parameters, string desc);

        /// <summary>
        /// Gets a single value off the data object based on the
        /// name of the field/property.
        /// </summary>
        /// <param name="dataObject">The object to get a value off of.</param>
        /// <param name="fieldName">The name of the field/property to get the value of.</param>
        /// <returns>The value.</returns>
        object GetValueFromObject(T dataObject, string fieldName);
    }
}