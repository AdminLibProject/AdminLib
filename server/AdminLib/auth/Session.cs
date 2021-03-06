﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using db=AdminLib.Database;
using AdminLib.Http;

namespace AdminLib.Auth {
    public class Session {

        /*
         * How sessions are managed
         * ------------------------
         * 
         * Sessions are materialized in the database by a table : APP_SESSION.
         * This tables contains all sessions (opened or not), including there session_id and the id of the user
         * connected to it.
         * The PAK_SESSION package allow the manipulation of session (creation/closing and user connection/disconnection)
         * 
         * On the server side, all sessions are stored in the Session.sessions static attribute (each time a new Session
         * object is created, it's added to this attribute).
         * 
         * Two cases are possible :
         *      1. A request arrive without any session ID, an invalid one or a ID of a closed session :
         *          In this case, we simply create a new session.
         *      2. A request arrive with a valid session ID : We automaticaly connect to the session
         *          corresponding to the id.
         *
         * Those two case are handled by the "initialize" function of the AdminHttpController : it will look
         * for a session ID in the header of the request.
         * 
         * 
         */

        /******************** Static Attributes ********************/
        private static Session serverSession;
        private static Dictionary<string, Session> sessions = new Dictionary<string,Session>();

        /******************** Attributes ********************/

        private Dictionary<string, db.AdminConnection> connections;

        /// <summary>
        ///     ID of the session
        /// </summary>
        public  string           sessionId  { get; private set; }
        public  User             user       { get; private set; }

        /******************** Class ********************/

        private static class dbPackage {

            /// <summary>
            ///     Return the user ID of the given session.
            ///     If the session don't exists, then throw "Error.SessionDontExists" error.
            ///     If no user connected, then return null.
            /// </summary>
            /// <param name="sessionID"></param>
            /// <returns></returns>
            public static int? GetUserIdFromSession ( string            sessionID
                                                    , db.AdminConnection connection) {
                string            sqlFunction;
                OracleParameter[] parameters;
                int?              userId;

                // Creating the function
                sqlFunction = "NVL(PAK_AUTH.GET_USER_ID_FROM_SESSION(p_session_id => :session_id), -1)";

                // Defining parameters
                parameters = new OracleParameter[1];

                parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                    , obj          : sessionID
                                                    , parameterName: ":session_id"
                                                    , type         : OracleDbType.Varchar2);

                // Retreiving the user_id
                // We use the server session to retreive the ID

                try {

                    userId = connection.FunctionAsInt ( sqlFunction : sqlFunction
                                                      , parameters  : parameters);

                    return userId;
                }
                catch (db.Error.SessionDontExists) {
                    throw new Error.SessionDontExists();
                }

            }

            /// <summary>
            ///     Create a new session.
            /// </summary>
            /// <returns>Id of the created session</returns>
            public static string create(db.AdminConnection connection) {
                string sessionId;
                string sqlFunction;

                // To create a new session in the database, we use the server session.

                sqlFunction = "PAK_AUTH.CREATE_SESSION()";

                sessionId = (string) connection.ExecuteFunction ( function : sqlFunction );

                return sessionId;
            }

        }

        /******************** Structure ********************/
        public struct SessionQueryResult {
            public string   SESSION_ID         {get; set;}
            public int?     USER_ID            {get; set;}
            public DateTime SESSION_START_DATE {get; set;}

        }

        /******************** Constructors ********************/

        /// <summary>
        ///     This constructor is used when loading all the sessions from the database.
        /// </summary>
        /// <param name="queryResult"></param>
        private Session(SessionQueryResult queryResult) {

            User user;

            if (queryResult.USER_ID != null)
                user = User.getOrCreate(queryResult.USER_ID ?? -1);
            else
                user = null;

            this.sessionId = queryResult.SESSION_ID;
            this.user      = user;

            Session.sessions[this.sessionId] = this;
            this.connections = new Dictionary<string, db.AdminConnection>();
        }

        /// <summary>
        ///     This constructor is called when creating a new session for a controller
        ///     or for the server.
        /// </summary>
        /// <param name="sessionId"></param>
        private Session(string sessionId) {
            this.sessionId = sessionId;
            this.connections = new Dictionary<string,db.AdminConnection>();
        }

        /******************** Methods ********************/        

        /// <summary>
        ///  This function will drop the given connection
        /// </summary>
        /// <param name="connection">Connection to drop</param>
        /// <param name="force">If false, then drop will fail if there is remaining opened cursors</param>
        /// <param name="commitTransactions"></param>
        public bool DropConnection(db.AdminConnection connection, bool force=false, bool commitTransactions = false) {
            bool closing;

            if (!this.HasConnection(connection.id))
                return true;

            closing = connection.Close ( force              : force
                                       , commitTransactions : commitTransactions);

            // Removing the connection from the current connections if the
            // closing was successful
            if (closing)
                this.connections.Remove(connection.id);

            return closing;
        }

        /// <summary>
        ///     Indicate if a user is connected to the current session (true) or not (false).
        /// </summary>
        /// <returns></returns>
        public bool isConnected() {
            return this.user != null;
        }

        public db.AdminConnection GetConnection ( string connectionID
                                               , bool   autoCommit = true
                                               , bool   keepAlive  = false) {

            db.AdminConnection connection;
            bool              createConnection;
            bool              createSession;
            int?              user_id;

            // Creating or retrieving the connection of the controller

            if (connectionID != null && this.connections != null)
                createConnection = !this.connections.ContainsKey(connectionID);
            else 
                createConnection = true;

            if (createConnection)
                // Creating a new connection
                connection = new db.AdminConnection ( session    : this
                                                   , autoCommit : autoCommit
                                                   , id         : connectionID);
            else
                connection = this.connections[connectionID];

            if (keepAlive) {

                this.connections[connection.id] = connection;
            }

            user_id = null;

            // If the session already has a sessionID, then initialize the connection
            if (this.sessionId != null) {

                // Retreiving from the database the user ID.
                // The function will raise an error if the session don't exists.
                try {
                    user_id = dbPackage.GetUserIdFromSession ( sessionID  : this.sessionId
                                                             , connection : connection);

                    createSession = false;
                }
                catch (Error.SessionDontExists) {
                    createSession = true;
                }
            }
            else
                createSession = true;

            if (createSession)
                // If the session don't exists, we create a new one
                this.SetNewSessionId(dbPackage.create(connection));

            if (user_id != null)
                this.user = User.getOrCreate(user_id);

            connection.Initialize();

            return connection;
        }

        /// <summary>
        ///     Indicate if the session has a connection with the given ID
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        private bool HasConnection(string connectionID) {

            if (connectionID == null)
                return false;

            return this.connections.ContainsKey(connectionID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSessionId"></param>
        private void SetNewSessionId(string newSessionId) {
            if (this.sessionId != null)
                Session.sessions.Remove(this.sessionId);

            Session.sessions[newSessionId] = this;

            this.sessionId = newSessionId;
        }

        /******************** Static Methods ********************/

        /// <summary>
        ///     Connect the user to the session of the controller.
        ///     This function take one (or several) values among user ID, user Email and/or username
        ///     and a password. If the password is valid, it will return the ID of the session.
        ///     
        ///     If the password is valid but the user is disabled, it will raise a "DISABLED_ACCOUNT" exception.
        ///     If the password is not valid, or if the user don't exist then it will raise a "INVALID_PASSWORD" exception.
        ///     
        ///     Once the session created, we define the current session and the current user.
        ///     
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <exception cref="Error.DisabledAccount">Raised when the user/password is valid but the account has been disabled</exception>
        /// <exception cref="Error.InvalidPassword">Raised when the user don't exist or the password is not corresponding</exception>
        public static void ConnectUser ( BaseController controller
                                       , int?           userId   = null
                                       , string         email    = null
                                       , string         username = null
                                       , string         password = null)
        {

            User user;

            user = User.getOrCreate ( id       : userId
                                    , email    : email
                                    , username : username );

            ConnectUser ( controller : controller
                        , user       : user
                        , password   : password);

        }

        /// <summary>
        ///     Connect the user to the current session.
        ///     This function take one (or several) values among user ID, user Email and/or username
        ///     and a password. If the password is valid, it will return the ID of the session.
        ///     
        ///     If the password is valid but the user is disabled, it will raise a "DISABLED_ACCOUNT" exception.
        ///     If the password is not valid, or if the user don't exist then it will raise a "INVALID_PASSWORD" exception.
        ///     
        ///     Once the session created, we define the current session and the current user.
        ///     
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <exception cref="Error.DisabledAccount">Raised when the user/password is valid but the account has been disabled</exception>
        /// <exception cref="Error.InvalidPassword">Raised when the user don't exist or the password is not corresponding</exception>
        public static void ConnectUser(BaseController controller, User user, string password) {

            string            sqlProcedure;
            OracleParameter[] parameters;

            // Creating the function
            sqlProcedure    = @"PAK_AUTH.CONNECT_USER ( p_user_id       => :userId 
                                                      , p_user_email    => :userEmail 
                                                      , p_user_name     => :userName
                                                      , p_user_password => :password)";

            // Creating parameters
            parameters    = new OracleParameter[4];

            // Id
            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : user.id
                                                , parameterName: ":userId"
                                                , type         : OracleDbType.Int32);

            parameters[0].IsNullable = true;

            // Email
            parameters[1] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : user.email
                                                , parameterName: ":userEmail"
                                                , type         : OracleDbType.Varchar2);

            parameters[1].IsNullable = true;

            // Username
            parameters[2] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : user.username
                                                , parameterName: ":userName"
                                                , type         : OracleDbType.Varchar2);

            parameters[2].IsNullable = true;

            // Password
            parameters[3] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : password
                                                , parameterName: ":password"
                                                , type         : OracleDbType.Varchar2);

            // Trying to create the sesssion
            try {
                // Creating the session and storing the value
                controller.connection.ExecuteCode ( procedure : sqlProcedure
                                                       , parameters   : parameters);

            }
            catch (db.Error.DisabledAccount) {
                throw new Error.DisabledAccount();
            }
            catch (db.Error.InvalidPassword) {
                throw new Error.InvalidPassword();
            }

            // Define the user as the current one
            controller.session.user = user;

            // Make sure that we have the user ID before continuing
            if (user.id == null)
                user.findUserId(controller.connection);
        }

        /// <summary>
        ///     Create a server session.
        ///     This session will be used for all servers tasks, such as creating new session, loading parameters, etc...
        /// </summary>
        /// <returns>Newly created session</returns>
        public static Session CreateServerSession() {
            
            if (Session.serverSession != null)
                return null;

            Session.serverSession = new Session("0");

            return Session.serverSession;
        }

        /// <summary>
        ///     Disconnect the current user.
        ///     No error is raised if no user is connected.
        /// </summary>
        public static void DisconnectUser(BaseController controller) {

            string sqlProcedure;

            sqlProcedure = "PAK_AUTH.DISCONNECT_USER()";

            controller.connection.ExecuteCode ( procedure : sqlProcedure );

            controller.session.user = null;
        }

        /// <summary>
        ///     Check if the session ID correspond to an existing session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        private static bool ExistsSession(string sessionID) {
            if (sessionID == null)
                return false;

            return Session.sessions.ContainsKey(sessionID);
        }

        /// <summary>
        ///     Return the list of all sessions when we are in debug mode.
        ///     On non-debug mode, will return null.
        /// </summary>
        /// <returns></returns>
        public static Session[] GetSessions() {

            int i;
            Session[] list;

            if (!Debug.Debug.IsEnabled())
                return null;

            list = new Session[sessions.Count];
            i = 0;

            foreach(KeyValuePair<string, Session> entry in sessions) {
                list[i] = entry.Value;
                i++;
            }

            return list;
        }

        /// <summary>
        ///     Return the session corresponding to the ID
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        private static Session GetSession(string sessionID) {
            if (!Session.sessions.ContainsKey(sessionID))
                return null;

            return Session.sessions[sessionID];
        }

        public static void LoadSessions(db.AdminConnection connection) {

            SessionQueryResult[] dbItems;
            string               sqlQuery;

            sqlQuery = "SELECT SESSION_ID, USER_ID, SESSION_START_DATE FROM APP_OPENED_SESSION";

            dbItems = connection.Query<SessionQueryResult> ( sqlQuery   : sqlQuery );

            // Creating each sessions
            for (int d = 0; d < dbItems.Length; d++) {
                new Session(dbItems[d]);
            }

        }

        /// <summary>
        ///     Initialize the session of a controller.
        ///     If the controller already provide a session ID
        ///     then the corresponding session will be retrieve.
        ///     
        ///     Note that at this point, we don't check that session is still valid.
        ///     This will be done when creating the connection. If the session is no
        ///     longer valid, then a new session ID will be provided
        /// </summary>
        /// <param name="controller"></param>
        public static void SetSession(BaseController controller) {

            Session           session;
            string            sessionID;

            sessionID = controller.header.session_id;

            if (Session.ExistsSession(sessionID) && sessionID != Session.serverSession.sessionId)
                session = Session.GetSession(sessionID);
            else
                session = new Session(null);

            controller.SetSession(session);
        }

    }
}