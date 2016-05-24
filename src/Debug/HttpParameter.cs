using AdminLib.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {
    /// <summary>
    ///     Contains an Http Parameter that will appear in the debug
    /// </summary>
    public class HttpParameter {
        public string   name;
        public string   operation;
        public string[] values;

        public static HttpParameter fromController (BaseController.Parameter parameter) {

            return new HttpParameter { name         = parameter.name
                                        , operation = parameter.filter.ToString()
                                        , values    = parameter.values};
        }

        /// <summary>
        ///     Convert parameters defined in the controllers to debug parameters
        /// </summary>
        /// <param name="controllerParameters"></param>
        /// <returns></returns>
        public static Dictionary <string, HttpParameter> fromController(Dictionary<string, BaseController.Parameter> controllerParameters) {

            Dictionary <string, HttpParameter> parameters;

            parameters = new Dictionary<string,HttpParameter>();

            foreach (KeyValuePair<string, BaseController.Parameter> entry in controllerParameters) {
                parameters[entry.Key] = fromController(entry.Value);
            }

            return parameters;
        }
    }
}