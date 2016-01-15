using System.Collections.Generic;
using db = AdminLib.Data.Query;

namespace AdminLib.Debug {
    public class Connection : DebugObject {

        /******************** Attributes ********************/
        public string         sessionID;
        public string         id;
        public List<SqlQuery> sql     = new List<SqlQuery>();
        public List<Cursor>   cursors = new List<Cursor>();

        /******************** Constructors ********************/
        public Connection(db.Connection connection) {

            App.Auth.Session.SessionConnection sessionConnection;

            if (connection is App.Auth.Session.SessionConnection) {

                sessionConnection = (App.Auth.Session.SessionConnection) connection;

                this.id        = sessionConnection.id;
                this.sessionID = sessionConnection.session != null ? sessionConnection.session.sessionId : null;
            }

        }

        /******************** Method ********************/
        public SqlQuery addQuery(string query, db.QueryParameter[] parameters) {
            SqlQuery sqlQuery;

            sqlQuery = new SqlQuery(query, parameters);
            this.sql.Add(sqlQuery);

            return sqlQuery;
        }
    }
}