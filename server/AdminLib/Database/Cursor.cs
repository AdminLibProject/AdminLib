using System;

namespace AdminLib.Database {
    public abstract class Cursor<QueryStructure> : BaseCursor
        where QueryStructure : new() {


        /******************** Methods ********************/

        /// <summary>
        ///     The [count] rows from the cursor
        /// </summary>
        /// <param name="count">Number of rows to fetch</param>
        /// <returns></returns>
        public abstract QueryStructure[] Fetch(int count = 100);
    }
}