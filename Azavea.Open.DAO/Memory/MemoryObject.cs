using System.Collections.Generic;
using System.Text;
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.Memory
{
    /// <summary>
    /// The representation of the object data while in the data store.
    /// </summary>
    public class MemoryObject
    {
        ///<summary>
        /// The values of the columns.
        ///</summary>
        public readonly IDictionary<string, object> ColValues;
        private readonly ClassMapping _mapping;

        /// <summary>
        /// Create a datastore object from the values passed in from the original object.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="colValues"></param>
        public MemoryObject(ClassMapping mapping, IEnumerable<KeyValuePair<string, object>> colValues)
        {
            ColValues = new CheckedDictionary<string, object>(colValues);
            _mapping = mapping;
        }
        /// <summary>
        /// Returns a unique (or as unique as the original object's IDs anyway) single key.
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            foreach (string col in _mapping.IdDataColsByObjAttrs.Values)
            {
                sb.Append(ColValues[col]).Append("_");
            }
            return sb.ToString();
        }
    }
}