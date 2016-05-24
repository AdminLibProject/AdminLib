using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace AdminLib.Debug {
    /// <summary>
    ///     HTTP Request details
    /// </summary>
    public class HttpRequest {

        public Dictionary<string, string[]> headers;
        public string method;
        public string query;

        public HttpRequest (HttpRequestMessage request) {

            string[] header;

            this.query      = request.RequestUri.Query;
            this.method     = request.Method.ToString();
            this.headers    = new Dictionary<string,string[]>();

            foreach (KeyValuePair<string, IEnumerable<string>> entry in request.Headers){
                header = entry.Value.ToArray();
                this.headers.Add(entry.Key, header);
            }
        }
    }
}