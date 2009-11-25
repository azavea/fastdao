// Copyright (c) 2004-2009 Avencia, Inc.
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
using System.Text;
using Avencia.Open.Common.Collections;

namespace Avencia.Open.DAO.Util
{
    /// <summary>
    /// Concatenating ("param" + x) wound up taking a lot of time, so here's
    /// a cache to hold them so we only ever have to concatenate them once.
    /// </summary>
    public class ParamNameCache
    {
        private readonly IDictionary<int, string> _cache = new CheckedDictionary<int, string>();

        /// <summary>
        /// Returns "param" + paramNum, except without calulating it every time.
        /// </summary>
        public string Get(int paramNum)
        {
            lock (_cache)
            {
                if (!_cache.ContainsKey(paramNum))
                {
                    StringBuilder newName = DbCaches.StringBuilders.Get();
                    newName.Append("param");
                    newName.Append(paramNum);
                    _cache[paramNum] = newName.ToString();
                    DbCaches.StringBuilders.Return(newName);
                }
                return _cache[paramNum];
            }
        }
    }
}