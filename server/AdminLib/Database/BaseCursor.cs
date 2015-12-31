using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database {
    public abstract class BaseCursor {

        /******************** Static Attributes ********************/
        public static Dictionary<string, Dictionary<int, BaseCursor>> cursors = new Dictionary<string,Dictionary<int,BaseCursor>>();
        public static int lastID = 0;


        /******************** Attributes ********************/
        protected AdminConnection    connection;
        public    int               id { get; private set; }
        protected OracleParameter[] parameters;
        public    string            query         { get; protected set; }
        public    DateTime          lastAcessDate { get; private   set; }
        public    string sessionID {
            get {

                if (this.connection == null)
                    return null;

                return this.connection.sessionID;
            }
        }

        /******************** Constructors ********************/
        public BaseCursor(AdminConnection connection, string query, OracleParameter[] parameters) : base() {
            this.connection = connection;
            this.query      = query;
            this.parameters = parameters;
        }

        public BaseCursor() {
            this.id         = BaseCursor.lastID++;
        }

        /******************** Abstract methods ********************/
        public virtual void Close() {
            BaseCursor.RemoveCursor(this);
        }

        public abstract void Dispose();
        public abstract bool IsOpen();

        public virtual void Open() {
            BaseCursor.AddCursor(this);
        }

        /******************** Static methods ********************/

        /// <summary>
        ///     Add the cursor to the list of all cursors
        /// </summary>
        /// <param name="cursor"></param>
        private static void AddCursor(BaseCursor cursor) {
            if (!BaseCursor.cursors.ContainsKey(cursor.sessionID))
                BaseCursor.cursors[cursor.sessionID] = new Dictionary<int, BaseCursor>();

            BaseCursor.cursors[cursor.sessionID][cursor.id] = cursor;
        }

        public static void Clean() {
            foreach (KeyValuePair<string, Dictionary<int, BaseCursor>> entry in BaseCursor.cursors) {
                BaseCursor.Clean(entry.Key);
            }
        }

        public static bool Clean(string sessionID) {

            BaseCursor cursor;
            DateTime   now;
            bool       stillOpenCursors;
            double     delta;

            if (!BaseCursor.cursors.ContainsKey(sessionID))
                return false;

            now = DateTime.Now;

            stillOpenCursors = false;

            foreach (KeyValuePair<int, BaseCursor> entry in BaseCursor.cursors[sessionID]) {

                cursor = entry.Value;
                delta = (now - cursor.lastAcessDate).TotalMinutes;

                if (delta > 10) {
                    cursor.Close();
                    stillOpenCursors = true;
                }
            }

            return stillOpenCursors;
        }

        /// <summary>
        ///     Return the cursor corresponding to the ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BaseCursor GetCursor(string sessionID, int id) {

            if (!BaseCursor.HasCursor(sessionID, id))
                return null;

            return BaseCursor.cursors[sessionID][id];
        }

        /// <summary>
        ///     Indicate if a cursor exist for the given session with the given ID
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool HasCursor(string sessionID, int id) {
            if (!BaseCursor.cursors.ContainsKey(sessionID))
                return false;

            return BaseCursor.cursors[sessionID].ContainsKey(id);
        }

        /// <summary>
        ///     Remove the cursor from the list of all cursors
        /// </summary>
        /// <param name="cursor"></param>
        private static void RemoveCursor(BaseCursor cursor) {
            BaseCursor.cursors[cursor.sessionID].Remove(cursor.id);

            if (BaseCursor.cursors[cursor.sessionID].Count == 0)
                BaseCursor.cursors.Remove(cursor.sessionID);
        }

        /******************** Methods ********************/
        public int GetID() {
            return this.id;
        }

        protected void UpdateLastAccessDate() {
            this.lastAcessDate = DateTime.Now;
        }
    }
}