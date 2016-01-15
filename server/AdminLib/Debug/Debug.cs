﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using db=AdminLib.Data.Query;
using AdminLib.App;

namespace AdminLib.Debug {

    using ApplicationParameters = Dictionary<string, Dictionary<string, ApplicationParameter>>;
    using Properties            = Dictionary<String, List<String>>;

    public class Debug {

        /******************** Attributes ********************/
        public Connection[]          connections { get; set; }
        public Controller            controller  { get; set; }
        public ApplicationParameters parameters  { get; set; }
        public Properties            properties  { get; set; }
        public Session[]             sessions    { get; set; }

        /******************** Constructors ********************/
        public Debug(Http.BaseController controller) {

            db.Connection[]    listConnections;
            Connection         connection;
            App.Auth.Session[] sessions;

            this.controller = controller.debug;

            // Parameters
            this.parameters = ApplicationParameter.getParametersOnDebugOnly();

            // Properties
            this.properties = new Properties();

            // Sessions
            sessions = App.Auth.Session.GetSessions();

            this.sessions = new Session[sessions.Length];

            for(int s=0; s < sessions.Length; s++) {
                this.sessions[s] = new Session(sessions[s]);
            }

            // Connexions
            listConnections = new db.Connection[db.Connection.connections.Count];
            db.Connection.connections.CopyTo(listConnections, 0);
            this.connections = new Connection[db.Connection.connections.Count];

            for(int e=0; e < listConnections.Length; e++) {
                connection = listConnections[e].debug;
                this.connections[e] = connection;
            }

        }

        /******************** Static Methods ********************/
        public static bool IsEnabled() {
            return true;
        }

    }
}