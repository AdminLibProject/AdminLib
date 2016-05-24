using System.Collections.Generic;
using AdminLib.Data.Query;
using AdminLib.Data.Query.Exception;

namespace AdminLib.App.Auth {
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
        public static void disableUser(Connection connection, int id) {

            string procedure;
            QueryParameter[] parameters;

            procedure = "PAK_AUTH.DISABLE_USER";

            // Defining parameters
            parameters = new QueryParameter[1];

            parameters[0] = new QueryParameter ( name  : "p_user_id"
                                               , value : id);

            // Executing query
            try {
                connection.ExecuteProcedure ( procedure  : procedure
                                            , parameters : parameters);
            }
            catch (InvalidID) {
                throw new Exception.InvalidUser();
            }
        }

        /// <summary>
        ///     Enable the current user
        /// </summary>
        public static void enableUser(Connection connection, int id) {

            string           procedure;
            QueryParameter[] parameters;

            procedure = "PAK_AUTH.ENABLE_USER";

            // Defining parameters
            parameters = new QueryParameter[1];

            parameters[0] = new QueryParameter ( value : id
                                               , name  : "p_user_id");

            // Executing query
            try {
                connection.ExecuteProcedure ( procedure : procedure
                                            , parameters   : parameters);
            }
            catch (InvalidID) {
                throw new Exception.InvalidUser();
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
        public static string resetPassword(Connection connection, int id) {

            string           newPassword;
            QueryParameter[] parameters;
            string           function;

            function = "PAK_AUTH.RESET_PASSWORD";

            // Defining parameters
            parameters = new QueryParameter[1];

            parameters[0] = new QueryParameter ( name  : "p_user_id"
                                               , value : id);

            // Executing query
            try {
                newPassword = connection.ExecuteFunction ( function : function
                                                         , parameters  : parameters);
            }
            catch (InvalidID) {
                throw new Exception.InvalidUser();
            }

            return newPassword;
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Change the password of the current user.
        /// </summary>
        /// <param name="oldPassword">Old password of the user</param>
        /// <param name="newPassword">New password of the user</param>
        public void changePassword ( Connection connection
                                   , string     oldPassword
                                   , string     newPassword) {

            string           procedure;
            QueryParameter[] parameters;

            procedure = "PAK_AUTH.CHANGE_PASSWORD";

            // Defining parameters
            parameters    = new QueryParameter[2];

            parameters[0] = new QueryParameter ( name  : "p_old_password"
                                               , value : oldPassword);

            parameters[1] = new QueryParameter ( name  : "p_new_password"
                                               , value : newPassword);

            try {
                connection.ExecuteProcedure ( procedure  : procedure
                                            , parameters : parameters);
            }
            catch (InvalidData) {
                throw new Exception.InvalidPasswordFormat();
            }
            catch (InvalidID) {
                throw new Exception.InvalidUser();
            }
            catch (App.QueryException.InvalidPassword) {
                throw new Exception.InvalidPassword();
            }
            catch (App.QueryException.DisabledAccount) {
                throw new Exception.DisabledAccount();
            }

        }

        /// <summary>
        ///     This function will search for the ID of the user
        ///     when the only information available is either the
        ///     email and/or the username.
        /// </summary>
        public void findUserId(Connection connection) {

            string function;
            QueryParameter[] parameters;

            function = "PAK_USER.GET_USER_ID";

            // Defining parameters
            parameters = new QueryParameter[2];

            parameters[0] = new QueryParameter ( name  : "p_user_email"
                                               , value : this.email);

            parameters[1] = new QueryParameter ( name  : "p_user_name"
                                               , value : this.username);

            // Executing query
            this.id =  connection.ExecuteFunction<int> ( function   : function
                                                       , parameters : parameters);

            User.users[this.id ?? -1] = this;
        }
    }
}