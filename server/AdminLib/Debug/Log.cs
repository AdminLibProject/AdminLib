using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using db=AdminLib.Database;

namespace AdminLib.Debug {

    /// <summary>
    ///     Define a log entry in the debug
    /// </summary>
    public class Log {

        /******************** Attributes ********************/
        public DateTime                time           { get; set; }
        public string                  message        { get; set; }
        public Level                   level          { get; set; }
        public Exception               error          { get; set; }
        public OracleException         dbError        { get; set; }
        public db.Error.QueryException queryException { get; set; }

        /******************** Enum ********************/
        public enum Level {
                normal
            , warning
            , error
        }

        /******************** Constructors ********************/

        public Log(string message, Level level = Level.normal) {
            this.message = message;
            this.level   = level;
            this.error   = null;
            this.time    = DateTime.Now;
        }

        public Log(string message, Level level=Level.normal, Exception error = null) {
            this.message = message;
            this.level   = level;
            this.error   = error;
            this.time    = DateTime.Now;
        }

        public Log(string message, Level level=Level.normal, OracleException error = null) {
            this.message = message;
            this.level   = level;
            this.dbError = error;
            this.time    = DateTime.Now;
        }

        public Log(string message, Level level=Level.normal, db.Error.QueryException error = null) {
            this.message    = message;
            this.level      = level;
            this.queryException = error;
            this.time       = DateTime.Now;
        }

    }
}