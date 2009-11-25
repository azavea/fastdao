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