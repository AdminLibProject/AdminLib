using System;
using System.Collections.Generic;
using db = AdminLib.Database;

namespace AdminLib.App {
    public class ApplicationParameter {

        /******************** Static Attributes ********************/
        private static Dictionary<string, Dictionary<string, ApplicationParameter>> parameters = new Dictionary<string, Dictionary<string, ApplicationParameter>>();

        // Max duration time of a session (in hours)
        public static int sessionMaxDuration {
            get {
                return Convert.ToInt32(getParameter(applicationGroup.SessionMaxDuration)._value);
            }
        }

        /******************** Attributes ********************/

        private string _group;
        public string group {
            get {
                return _group;
            }
        }

        private string _name;
        public string name {
            get {
                return this._name;
            }
        }
        

        private string _value;
        public string value {
            get {
                return this._value;
            }
        }

        /******************** Enum ********************/
        public enum applicationGroup {
            SessionMaxDuration = 0
        }

        /******************** Structures ********************/
        public struct QueryStructure {

            public string PARAM_GROUP {get; set;}
            public string PARAM_NAME  {get; set;}
            public string PARAM_VALUE {get; set;}
        }

        /******************** Constructor ********************/
        private ApplicationParameter() { }

        /******************** Static methods ********************/

        /// <summary>
        ///     Return the list of all parameters.
        ///     This function is usable only on debug mode (debug.isEnabled() == true).
        ///     If no debug mode, then return null.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, ApplicationParameter>> getParametersOnDebugOnly() {
            if (!Debug.Debug.IsEnabled())
                return null;

            return ApplicationParameter.parameters;
        }

        /// <summary>
        ///     Return the given parameter
        /// </summary>
        /// <param name="group"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static ApplicationParameter getParameter(string group, string parameter) {

            Dictionary<string, ApplicationParameter> groupParameter;

            if (!ApplicationParameter.parameters.ContainsKey(group))
                throw new Error.ParameterNotFound();

            groupParameter = ApplicationParameter.parameters[group];

            if (!parameter.Contains(parameter))
                throw new Error.ParameterNotFound();

            return parameters[group][parameter];
        }

        /// <summary>
        ///     Return the given parameter
        /// </summary>
        /// <param name="group"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <exception cref="AdminApp.Error.ParameterNotFound">The parameter don't exists</exception>
        public static ApplicationParameter getParameter(applicationGroup parameter) {
            return getParameter("application", parameter.ToString());
        }

        /// <summary>
        ///     Load all parameters from the database
        /// </summary>
        public static void loadParameters(db.Connection connection) {

            QueryStructure       dbParameter;
            QueryStructure[]     dbParameters;
            ApplicationParameter parameter;
            string               sqlQuery;

            sqlQuery     = "SELECT PARAM_GROUP, PARAM_NAME, PARAM_VALUE FROM APP_PARAMETER";
            dbParameters = connection.Query<QueryStructure> ( query : sqlQuery );

            for (int i = 0; i < dbParameters.Length; i++) {

                dbParameter = dbParameters[0];

                parameter = new ApplicationParameter { _group = dbParameter.PARAM_GROUP
                                                     , _name  = dbParameter.PARAM_NAME
                                                     , _value = dbParameter.PARAM_VALUE };

                if (!ApplicationParameter.parameters.ContainsKey(parameter._group))
                    ApplicationParameter.parameters[parameter._group] = new Dictionary<string, ApplicationParameter>();

                ApplicationParameter.parameters[parameter._group][parameter._name] = parameter;
            }


        }
        

    }
}