using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using db=AdminLib.Database;

namespace AdminLib.Debug {
    public class Controller : DebugObject {

        /******************** Attributes ********************/
        private Http.BaseController controller;
        public List<Connection>     connections = new List<Connection>();

        private Session _session;
        public Session session {
            get {
                if (this._session == null)
                    this._session = new Session(session : this.controller.session);

                return this._session;
            }
        }

        public HttpSession http {
            get {
                if (this._http == null)
                    this._http = new HttpSession ( this.controller);

                return this._http;
            }
        }

        private HttpSession _http;

        /******************** Constructors ********************/
        public Controller(Http.BaseController controller) : base() {
            this.controller = controller;
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Add a connection to the controller.
        ///     This is specialy useful for cursors who keep connections alive
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public Connection add(db.AdminConnection connection) {
            Connection debugConnection;

            debugConnection = new Connection(connection);

            this.connections.Add(debugConnection);

            return debugConnection;
        }

    }
}