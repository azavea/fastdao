using System;

namespace WriteAndQuery
{
    /// <summary>
    /// This class is mapped in the mapping.xml file.
    /// </summary>
    public class DataClass
    {
        /// <summary>
        /// FastDAO can access fields...
        /// </summary>
        public int ID;
        /// <summary>
        /// ... or properties.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// FastDAO ignores fields/properties not mapped in the mapping file.
        /// </summary>
        public double NotMapped;

        /// <summary>
        /// The only requirement FastDAO puts on your data classes is that
        /// they must have a default constructor (I.E. one that takes no parameters).
        /// </summary>
        public DataClass()
        {
            // It doesn't have to do anything in particular though.
        }

        /// <summary>
        /// Other constructors are fine to have if you'd like.
        /// </summary>
        public DataClass(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
