using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {

    public class HttpSession {
        /******************** Attributes ********************/
        public Dictionary<string, HttpParameter> parameters;
        public HttpRequest                       request;

        /******************** Constructors ********************/
        public HttpSession(Http.BaseController controller) {
            
            request = new HttpRequest(controller.Request);

        }
    }

}