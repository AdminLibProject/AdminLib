using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using db=AdminLib.Database;

namespace AdminLib.Auth {
    public class User {

        /******************** Static Attributes ********************/
        /// <summary>
        ///     List of all users.
        ///     The key correspond to the ID of the user
        /// </summary>
        private static Dictionary<int, User> users = new Dictionary<int,User>();
        
        /******************** Attributes ********************/

        /// <summary>
        ///     ID of the user
        /// </summary>
        public int? id { get; private set;}

        /// <summary>
        ///     Username of the user
        /// </summary>
        public string username { get; private set; }

        /// <summary>
        ///     Email of the user
        /// </summary>
        public string email {get; private set;}

        /******************** Constructor ********************/
        private User(int? id, string username = null, string email = null) {
            this.id        = id;
            this.username = username;
            this.email    = email;

            // The user is recorded only if it has an id
            // For a user object to get an id, it have to execute the function "findUserId"
            if (id != null && id != -1)
                users[id ?? -1] = this;
        }

        /******************** Static methods ********************/

        /// <summary>
        ///     Enable the current user
        /// </summary>
        public static void disableUser(db.AdminConnection connection, int id) {

            string sqlProcedure;
            OracleParameter[] parameters;

            sqlProcedure = "PAK_AUTH.DISABLE_USER(p_user_id => :id)";

            // Defining parameters
            parameters = new OracleParameter[1];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : id
                                                , parameterName: ":id"
                                                , type         : OracleDbType.Int32);

            // Executing query
            try {
                connection.ExecuteCode ( procedure : sqlProcedure
                                            , parameters   : parameters);
            }
            catch (db.Error.InvalidID) {
                throw new Error.InvalidUser();
            }
        }

        /// <summary>
        ///     Enable the current user
        /// </summary>
        public static void enableUser(db.AdminConnection connection, int id) {

            string sqlProcedure;
            OracleParameter[] parameters;

            sqlProcedure = "PAK_AUTH.ENABLE_USER(p_user_id => :id)";

            // Defining parameters
            parameters = new OracleParameter[1];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : id
                                                , parameterName: ":id"
                                                , type         : OracleDbType.Int32);

            // Executing query
            try {
                connection.ExecuteCode ( procedure : sqlProcedure
                                            , parameters   : parameters);
            }
            catch (db.Error.InvalidID) {
                throw new Error.InvalidUser();
            }
        }

        /// <summary>
        ///     Return the user corresponding to the id
        /// </summary>
        /// <param name="id">Id of the user to return</param>
        /// <returns>The user corresponding to the ID. If no user, then return null</returns>
        public static User get(int id) {
            if (!User.users.ContainsKey(id))
                return null;

            return User.users[id];
        }

        /// <summary>
        ///     Return the user corresponding to the ID.
        ///     If the ID don't correspond to an existing user, then
        ///     it will be first created.
        ///     No control is done to check if the user is real or not.
        /// </summary>
        /// <param name="id">Id of the user to return/create</param>
        /// <returns></returns>
        public static User getOrCreate(int? id, string email=null, string username=null) {

            if (users.ContainsKey(id ?? -1))
                return users[id ?? -1];

            return new User( id       : id
                           , email    : email
                           , username : username);
        }

        /// <summary>
        ///     Enable the current user
        /// </summary>
        public static string resetPassword(db.AdminConnection connection, int id) {

            string            newPassword;
            OracleParameter[] parameters;
            string            sqlFunction;

            sqlFunction = "PAK_AUTH.RESET_PASSWORD(p_user_id => :id)";

            // Defining parameters
            parameters = new OracleParameter[1];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : id
                                                , parameterName: ":id"
                                                , type         : OracleDbType.Int32);

            // Executing query
            try {
                newPassword = (string) connection.ExecuteFunction ( function : sqlFunction
                                                                  , parameters  : parameters);
            }
            catch (db.Error.InvalidID) {
                throw new Error.InvalidUser();
            }

            return newPassword;
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Change the password of the current user.
        /// </summary>
        /// <param name="oldPassword">Old password of the user</param>
        /// <param name="newPassword">New password of the user</param>
        public void changePassword (db.AdminConnection connection, string oldPassword, string newPassword) {

            string            sqlProcedure;
            OracleParameter[] parameters;

            sqlProcedure = @"PAK_AUTH.CHANGE_PASSWORD ( p_old_password => :old_password
                                                      , p_new_password => :new_password)";

            // Defining parameters
            parameters    = new OracleParameter[2];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : oldPassword
                                                , parameterName: ":old_password"
                                                , type         : OracleDbType.Varchar2);

            parameters[1] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : newPassword
                                                , parameterName: ":new_password"
                                                , type         : OracleDbType.Varchar2);

            try {
                connection.ExecuteCode ( procedure : sqlProcedure
                                            , parameters   : parameters);
            }
            catch (db.Error.InvalidData) {
                throw new Error.InvalidPasswordFormat();
            }
            catch (db.Error.InvalidID) {
                throw new Error.InvalidUser();
            }
            catch (db.Error.InvalidPassword) {
                throw new Error.InvalidPassword();
            }
            catch (db.Error.DisabledAccount) {
                throw new Error.DisabledAccount();
            }

        }

        /// <summary>
        ///     This function will search for the ID of the user
        ///     when the only information available is either the
        ///     email and/or the username.
        /// </summary>
        public void findUserId(db.AdminConnection connection) {

            string sqlFunction;
            OracleParameter[] parameters;

            sqlFunction = @"PAK_USER.GET_USER_ID ( p_user_email => :email
                                                 , p_user_name  => :username)";

            // Defining parameters
            parameters = new OracleParameter[2];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : this.email
                                                , parameterName: ":email"
                                                , type         : OracleDbType.Varchar2);

            parameters[1] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : this.username
                                                , parameterName: ":username"
                                                , type         : OracleDbType.Varchar2);

            // Executing query
            this.id =  connection.FunctionAsInt ( sqlFunction : sqlFunction
                                                , parameters  : parameters);

            User.users[this.id ?? -1] = this;
        }
    }
}