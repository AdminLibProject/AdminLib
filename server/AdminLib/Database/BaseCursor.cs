using System;

namespace AdminLib.Database
{
    public abstract class BaseCursor {

        /******************** Static Attributes ********************/
        private static int lastID = 0;

        /******************** Attributes ********************/
        public    Connection       connection    { get; private set; }
        public    int              id            { get; private set; }
        protected QueryParameter[] parameters;
        public    string           query         { get; protected set; }
        public    DateTime         lastAcessDate { get; private   set; }

        /******************** Constructors ********************/
        public BaseCursor(Connection connection, string query, QueryParameter[] parameters) : base() {
            this.connection = connection;
            this.query      = query;
            this.parameters = parameters;
        }

        public BaseCursor() {
            this.id         = BaseCursor.lastID++;
        }

        /******************** Abstract methods ********************/
        public abstract void Close();
        public abstract void Dispose();
        public abstract bool HasRows();
        public abstract bool IsOpen();
        public abstract void Open();

        /******************** Methods ********************/
        public int GetID() {
            return this.id;
        }

        protected void UpdateLastAccessDate() {
            this.lastAcessDate = DateTime.Now;
        }
    }
}