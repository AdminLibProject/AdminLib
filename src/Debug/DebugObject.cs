using System;
using System.Collections.Generic;
using LogClass = AdminLib.Debug.Log;
using db = AdminLib.Data.Query;

namespace AdminLib.Debug
{
    public abstract class DebugObject {

        /******************** Attributes ********************/
        public List<Log>                 logs   = new List<Log>();
        public Dictionary<string, Timer> timers = new Dictionary<string,Timer>();
        public Dictionary<string, List<string>> properties = new Dictionary<string,List<string>>();

        /******************** Constructors ********************/
        public DebugObject() {}

        /******************** Methods ********************/

        public void Log(string message, LogClass.Level level = LogClass.Level.normal) {
            LogClass log;

            if (!Debug.IsEnabled()) return;

            log = new LogClass ( message : message
                               , level   : level);

            this.logs.Add(log);
        }

        /// <summary>
        ///     Add a log entry
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of the entry</param>
        /// <param name="error">Enventual error</param>
        public void Log(string message, Exception error, LogClass.Level level = LogClass.Level.normal) {

            LogClass log;

            if (!Debug.IsEnabled()) return;

            log = new LogClass ( message : message
                               , level   : level
                               , error   : error);

            this.logs.Add(log);
        }

        /// <summary>
        ///     Add a log entry, with an query error
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of the entry</param>
        /// <param name="error">Enventual error</param>
        public void Log(string message, db.Exception.QueryException error, LogClass.Level level = LogClass.Level.normal) {
            LogClass log;

            if (!Debug.IsEnabled()) return;

            log = new LogClass ( message : message
                               , level   : level
                               , error   : error);

            this.logs.Add(log);
        }

        /// <summary>
        ///     Start a new timer.
        /// </summary>
        /// <param name="name"></param>
        public void startTimer(string name) {
            Timer timer;

            if (!Debug.IsEnabled()) return;

            timer = new Timer();
            timer.start();
            
            this.timers[name] = timer;
        }

        /// <summary>
        ///     End a timer.
        /// </summary>
        /// <param name="name"></param>
        public void stopTimer(string name) {

            if (!Debug.IsEnabled()) return;

            if (!this.timers.ContainsKey(name))
                return;

            this.timers[name].stop();
        }

        public void save(string key, string value) {
            if (!this.properties.ContainsKey(key))
                this.properties[key] = new List<string>();

            this.properties[key].Add(value);
        }
    }
}