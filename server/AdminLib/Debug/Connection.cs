using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using db=AdminLib.Database;

namespace AdminLib.Debug {
    public class Connection : DebugObject {

        /******************** Attributes ********************/
        public string         sessionID;
        public string         id;
        public List<SqlQuery> sql     = new List<SqlQuery>();
        public List<Cursor>   cursors = new List<Cursor>();

        /******************** Constructors ********************/
        public Connection(db.AdminConnection connection) {
            this.id        = connection.id;
            this.sessionID = connection.sessionID;
        }

        /******************** Method ********************/
        public SqlQuery addQuery(string query, OracleParameter[] parameters) {
            SqlQuery sqlQuery;

            sqlQuery = new SqlQuery(query, parameters);
            this.sql.Add(sqlQuery);

            return sqlQuery;
        }
    }
}