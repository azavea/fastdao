using System;
using System.Collections.Generic;
using System.Text;
using Azavea.Open.DAO;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.CSV;
using Azavea.Open.DAO.SQLite;

namespace WriteAndQuery
{
    class Program
    {
        /// <summary>
        /// This tutorial uses CSV files because it's easy for you to look
        /// at the file and see what it has done.
        /// 
        /// However, since CSVs don't have autonumbers or sequences we have to make up
        /// our own unique IDs.
        /// </summary>
        private static int uid = 0;

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("First, enter some names to store in the CSV file ('done' when finished):");
            // For the super simple example, we'll use a CSV file.
            ConnectionDescriptor csvDescriptor = new CsvDescriptor(
                CsvConnectionType.FileName, "Data.csv");
            // The ../.. is because you're probably running this in the /bin/debug directory.
            FastDAO<DataClass> csvDao = new FastDAO<DataClass>(csvDescriptor, "../../mapping.xml");
            ReadAndStore(csvDao);
            QueryAndPrint(csvDao);

            Console.WriteLine();
            Console.WriteLine("Next, enter some names to store in the SQLite database.");
            Console.WriteLine("Enter a few of the same ones if you want. ('done' when finished):");
            // Just to show how easy it is to switch data sources, we'll run the same code
            // only this time using a SQLite database.
            ConnectionDescriptor sqliteDescriptor = new SQLiteDescriptor("Data.sqlite");
            FastDAO<DataClass> sqliteDao = new FastDAO<DataClass>(sqliteDescriptor, "../../mapping.xml");
            ReadAndStore(sqliteDao);
            QueryAndPrint(sqliteDao);
        }

        private static void ReadAndStore(FastDAO<DataClass> dao)
        {
            string input = Console.ReadLine();
            while (!"done".Equals(input))
            {
                // Create a new object and store it via FastDAO.
                DataClass storeMe = new DataClass(uid++, input);
                // Inserting is as easy as this:
                dao.Insert(storeMe);

                // read the next line of input.
                input = Console.ReadLine();
            }
        }

        private static void QueryAndPrint(FastDAO<DataClass> dao)
        {
            // To get all records at once, just call Get.
            IList<DataClass> all = dao.Get();
            Console.WriteLine("Here's what you put in:");
            foreach (DataClass data in all)
            {
                Console.WriteLine(data.ID + " - " + data.Name);
            }

            // However, maybe you want to sort them, or not get all of them.  So you
            // specify a criteria. 
            DaoCriteria crit = new DaoCriteria();
            // NOTE: All "names" in the criteria are the names of the fields/properties
            //       on the object, FastDAO takes care of translating them to the
            //       column names in the data source (according to what is in the mapping file).
            crit.Orders.Add(new SortOrder("Name"));

            // Now you just pass the criteria to the Get method.
            Console.WriteLine("Here it is sorted by name:");
            foreach (DataClass data in dao.Get(crit))
            {
                Console.WriteLine(data.ID + " - " + data.Name);
            }
            
            // However in real life that may be a really big list, 
            // and running out of memory makes you sad.  So you can
            // iterate over the results instead:
            Console.WriteLine("Here it is by iterating:");
            dao.Iterate(crit,
                // Here is an anonymous function, though it could be a real
                // function too.
                delegate(object parameters, DataClass data)
                {
                    Console.WriteLine(data.ID + " - " + data.Name);
                },
                // In this case we don't need to pass any other parameters, so we pass null.
                // I'll add an overload where we can leave this parameter out in the future.
                null,
                // This is a description of what is going on, which will be included in
                // any exception messages if something breaks.
                "Processing Data Classes");
        }

        #region Tutorial Stuff
        private void SecretSetupDontLookAtThis<T>(FastDAO<T> dao) where T : class, new()
        {
            // What are you doing reading this? :-)

            // Okay so actually what this is doing is using the FastDAO's
            // lower level code (the Data Access Layer) to blow away the
            // tutorial database and recreate a blank one.  This is the kind
            // of thing that is handy for unit tests or tutorials, but you'll
            // probably never use in production, so you can ignore it if you're
            // trying out FastDAO for the first time.
            if (dao.DataAccessLayer is IDaDdlLayer)
            {
                IDaDdlLayer dal = (IDaDdlLayer) dao.DataAccessLayer;
                // In the case of something like SQLite, this will delete
                // and recreate the database file.
                if (!dal.StoreHouseMissing())
                {
                    dal.DeleteStoreHouse();
                }
                dal.CreateStoreHouse();
                // In the case of most databases, this will delete and recreate
                // the table.
                if (!dal.StoreRoomMissing(dao.ClassMap))
                {
                    dal.DeleteStoreRoom(dao.ClassMap);
                }
                dal.CreateStoreRoom(dao.ClassMap);
            }
        }
        #endregion
    }
}
