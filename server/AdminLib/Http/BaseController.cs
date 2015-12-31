using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Oracle.ManagedDataAccess.Client;
using System.Web;
using DjangoSharp;
using DjangoSharp.Model;
using DjangoSharp.Query;
using db = AdminLib.Database;

using AdminLib.Model;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using AdminLib.Application;

namespace AdminLib.Http {
    public abstract class BaseController : ApiController {

        /******************** Attributes ********************/
        public Debug.Controller debug   {get; private set;}
        public Header           header  {get; private set;}
        public Auth.Session     session {get; private set;}

        public Auth.User       user {
            get {
                if (this.session == null)
                    return null;

                return this.session.user;
            }

        }

        public db.AdminConnection connection { get; private set; }

        /// <summary>
        ///     Return the list of files provided in the header payload.
        /// </summary>
        private Dictionary<string, HeaderFileEntry> _multipartFiles = null;
        public Dictionary<string, HeaderFileEntry> multipartFiles {
            
            get {

                HeaderFileEntry fileEntry;

                if (this._multipartFiles != null)
                    return this._multipartFiles;

                if (this.provider == null)
                    throw new Exception("\"provider\" not defined : The function \"RetreiveMultipartInformations\" must be called before");

                this._multipartFiles = new Dictionary<string, HeaderFileEntry>();

                foreach(MultipartFileData fileData in this.provider.FileData) {
                    fileEntry = new HeaderFileEntry(fileData);
                    this._multipartFiles[fileEntry.entryName] = fileEntry;
                }

                return this._multipartFiles;
            }

        }

        /// <summary>
        ///     Return the list of form data entries
        /// </summary>
        private Dictionary<string, string[]> _formData = null;
        public  Dictionary<string, string[]> formData {

            get {
                if (this._formData != null)
                    return this._formData;

                if (this.provider == null)
                    throw new Exception("\"provider\" not defined : The function \"RetreiveMultipartInformations\" must be called before");

                this._formData = new Dictionary<string, string[]>();
                
                foreach(string key in this.provider.FormData.AllKeys) {
                    this._formData[key] = this.provider.FormData.GetValues(key);
                }

                return this._formData;
            }

        }

        /// <summary>
        /// Return the list of all GET parameters in the request.
        /// The first time the attribute is called, it will calculate all parameters, save the list and return it.
        /// Next times, it will simply return the previously saved list.
        /// 
        /// This should parse queries such as :
        /// 
        /// ?id=1,2,3&code:like=code1*&fields=label&fields=id,code
        /// like :
        /// 
        ///     [ Parameter {name: "id"   , values:[1, 2, 3]}
        ///     , Parameter {name: "code" , operation: "like", values: ["code1*"]}
        ///     , Parameter {name: "field", values:["label", "id", "code"]}
        ///     
        /// Orders of parameters and values should be kept.
        /// 
        /// </summary>
        /// 
        private Dictionary<string, Parameter> _parameters = null;
        public  Dictionary<string, Parameter> parameters {
            get {
                if (this._parameters != null)
                    return this._parameters;

                string[]        fields;
                string          name;
                FilterOperator? filter;
                GroupOperator?  groupBy;
                Parameter       parameter;
                string          query;
                string[]        queryParameters;
                string[]        values;

                query = this.Request.RequestUri.Query;

                queryParameters = query.Split(new Char[] {'&'});

                this._parameters = new Dictionary<string, Parameter>();

                if (queryParameters.Length == 1)
                    if (queryParameters[0] == "?" || queryParameters[0] == "")
                        return this._parameters;

                // Parsing each parameters;
                for(int q=0; q<queryParameters.Length; q++) {

                    if (queryParameters[q] == "?")
                        continue;

                    fields = queryParameters[q].Split(new Char[] {'='}, 2);

                    if (fields.Length == 2)
                        values = fields[1].Split(new Char[] {','});
                    else
                        values = new String[0];

                    // Extracting name and operations
                    fields = fields[0].Split(new Char[] {':'});
                    name = fields[0];

                    if (q == 0)
                        name = name.Substring(1);

                    filter = null;
                    groupBy  = null;

                    // Extracting the operations
                    if (fields.Length > 1) {

                        if (fields.Length == 2) {

                            if (API.IsFilterOperator(fields[1]))
                                filter = API.GetFilterOperator(fields[1]);

                            else if (API.IsGroupOperator(fields[1]))
                                groupBy = API.GetGroupOperator(fields[1]);

                            else
                                throw new Exception("Invalid syntax");
                        }

                        else if (fields.Length == 3) {
                            if (!API.IsFilterOperator(fields[1]) || API.IsGroupOperator(fields[2]))
                                throw new Exception("Invalid syntax");

                            filter = API.GetFilterOperator(fields[1]);
                            groupBy  = API.GetGroupOperator(fields[2]);
                        }
                        else
                            throw new Exception("Invalid syntax");

                    }


                    // Url decoding each parameter
                    for (int v = 0; v < values.Length; v++) {
                        values[v] = HttpContext.Current.Server.UrlDecode(values[v]);
                    }
                    
                    if (filter == null && values != null)
                        filter = values.Length == 1 ? FilterOperator.equal : FilterOperator.inList;

                    // Creating parameter
                    // If the operation is not 
                    parameter = new Parameter () { name    = name
                                                 , filter  = filter
                                                 , groupBy = groupBy
                                                 , values  = values};

                    if (this._parameters.ContainsKey(name))
                        this._parameters[name].values.Concat(values);
                    else
                        this._parameters[name] = parameter;
                }

                return this._parameters;
            }
        }

        private MultipartFormDataStreamProvider provider;

        /******************** Structures ********************/
        public struct EmptyResponse : IAdminQueryResult {
            public Debug.Debug debug   { get; set; }
            public string      message { get; set; }
        }

        public class Error<T> : IAdminQueryResult
            where T: Exception {

            /***** Attributes *****/
            public Debug.Debug             debug     { get; set; }
            public string                  message   { get; set; }
            public string                  exception { get; set; }
            public string                  type      { get; set; }

            /***** Constructors *****/
            public Error (T exception) {
                this.exception = exception.GetType().FullName;
                this.message   = exception.Message;
                this.type      = "Error";
            }

            public Error(OracleException exception) {
                this.exception = typeof(T).FullName;
            }

        }

        /// <summary>
        ///     Summarize informations about a file provided in the header of the request
        /// </summary>
        public class HeaderFileEntry {

            public string entryName     {get; private set; }
            public string localFileName {get; private set; }
            public string filename      {get; private set; }
            public string mimeType      {get; private set; }

            public HeaderFileEntry(MultipartFileData fileData) {

                this.entryName     = fileData.Headers.ContentDisposition.Name;
                this.localFileName = fileData.LocalFileName;
                this.filename      = fileData.Headers.ContentDisposition.FileName;
                this.mimeType      = fileData.Headers.ContentType.MediaType;
                
                this.entryName = this.entryName.Substring(1, this.entryName.Length - 2);
                this.filename  = this.filename.Substring(1, this.filename.Length - 2);

            }

        }

        public class QueryError : Error<db.Error.QueryException>  {

            public db.Error.Code?                      code             { get; private set; }
            public string                              query            { get; private set; }
            public db.Error.QueryException.Parameter[] parameters       { get; private set; }
            public ExceptionInformation                informations     { get; private set; }

            public QueryError(db.Error.QueryException exception) : base(exception) {

                this.query            = exception.query;
                this.parameters       = exception.parameters;
                this.code             = exception.code;
                this.type             = "DatabaseException";

                this.informations     = new ExceptionInformation (exception.exception);
            }

            public class ExceptionInformation {

                public int    number  {get; private set; }
                public string message {get; private set; }

                public ExceptionInformation(OracleException exception) {
                    this.number  = exception.HResult;
                    this.message = exception.Message;
                }

            }

        }

        public static class EHeaders {
            public const string session_id        = "Session-Id";
            public const string connection_id     = "Connection-Id";
            public const string commitTransaction = "Auto-Commit";
            public const string keepAlive         = "Keep-Connection-Alive";
        }

        /// <summary>
        ///     Correspond to an HTTP parameter
        /// </summary>
        public class Parameter {
            public string          name;
            public FilterOperator? filter;
            public GroupOperator?  groupBy;
            public string[]        values;

            public FieldFilter toFieldFilter(ModelStructure rootModel) {
                
                if (this.filter == null)
                    throw new Exception("Parameter is not a filter");

                return new FieldFilter ( rootModel : rootModel
                                       , path      : this.GetName()
                                       , type      : this.filter ?? default(FilterOperator)
                                       , value     : this.values);
            }

            public string GetName(bool includeGroupBy = true) {

                if (includeGroupBy) {
                    if (this.groupBy == null)
                        return this.name;
                    else
                        return this.name + ':' + this.groupBy;
                }

                return this.name;
            }

        }

        public class Header {

            public BaseController controller {get; private set; }
            private HttpRequestHeaders httpHeaders;

            /// <summary>
            ///     Indicate if the header ask for auto commit or no.
            ///     The default value is true if no value provided in the header.
            /// </summary>
            private bool? _commitTransaction;
            public bool commitTransaction {

                get {
                    string commitTransaction;

                    if (this._commitTransaction != null)
                        return (bool) this._commitTransaction;

                    commitTransaction = this.GetHeaderValue(EHeaders.commitTransaction);

                    this._commitTransaction = commitTransaction == "true" || commitTransaction == null;

                    return (bool) this._commitTransaction;
                }
            }

            /// <summary>
            ///     Indicate if connection should be keep
            /// </summary>
            public bool keepAlive {
                get {
                    string keepAlive;
                    keepAlive = this.GetHeaderValue(EHeaders.keepAlive);
                    return keepAlive == "true";
                }
            }

            /// <summary>
            ///     Return the connection ID provided by the header
            /// </summary>
            public string connection_id {
                get {
                    return this.GetHeaderValue(EHeaders.connection_id);
                }
            }

            /// <summary>
            ///     Return the session ID provided by the header
            /// </summary>
            private string _session_id;
            public string session_id {
                get {
                    if (this._session_id != null)
                        return this._session_id;

                    this._session_id = this.GetHeaderValue(EHeaders.session_id);

                    return this._session_id;
                }
            }

            /******************** Constructors ********************/
            public Header(BaseController controller) {
                this.controller  = controller;
                this.httpHeaders = this.controller.Request.Headers;
            }

            /******************** Methods ********************/

            /// <summary>
            ///     Return the value of the given http parameter name
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetHeaderValue(string name) {
                string value;
            
                if (!this.httpHeaders.Contains(name))
                    return null;

                value = this.httpHeaders.GetValues(name).FirstOrDefault();

                return value == "" ? null : value;
            }

        };

        /******************** Static classes ********************/
        public static class Headers {
        }

        /******************** Methods ********************/

        public virtual void FinalizeResponse(HttpResponseMessage response) { }

        public virtual string[] GetFields(ModelStructure model) {

            List<string> fields;
            string[]     fieldsName;

            if (!this.parameters.ContainsKey("fields"))
                return new string[0];

            fieldsName = this.parameters["fields"].values;
            fields     = new List<string>();

            foreach(string field in fieldsName) {

                if (!model.IsValidPath(field))
                    continue;

                fields.Add(field);
            }

            return fields.ToArray();
        }

        /// <summary>
        ///     Build and return a filter object based on the GET parameters
        /// </summary>
        /// <param name="rootModel"></param>
        /// <returns></returns>
        public virtual Filter GetFilter(ModelStructure rootModel) {

            Filter queryFilters;

            queryFilters = new Filter();

            // Looping on all parameters
            foreach(KeyValuePair<string, Parameter> entry in this.parameters) {

                if (!rootModel.IsValidPath(entry.Key))
                    continue;

                // Adding the parameter
                queryFilters.Add(entry.Value.toFieldFilter(rootModel));
            }

            return queryFilters;

        }

        /// <summary>
        /// Return the list of order by fields that are expected by the HTTP Request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual OrderBy[] GetOrderByFields(ModelStructure model) {

            OrderBy       orderBy;
            List<OrderBy> listOrderBy;
            string[]      fieldsName;

            if (!this.parameters.ContainsKey("orderBy"))
                return new OrderBy[0];

            fieldsName  = this.parameters["orderBy"].values;
            listOrderBy = new List<OrderBy>();

            foreach(string field in fieldsName) {

                if (!model.IsValidPath(field))
                    continue;

                orderBy = new OrderBy (field : field);

                listOrderBy.Add(orderBy);
            }

            return listOrderBy.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns></returns>
        public virtual string[] GetParameterValues(string name) {

            string[]     values;

            if (!this.parameters.ContainsKey(name))
                return new string[0];

            values = this.parameters[name].values;

            return values.ToArray();
        }

        /// <summary>
        /// </summary>
        protected override void Initialize(HttpControllerContext controllerContext){

            this.debug = new Debug.Controller(this);

            this.debug.startTimer("AdminHttpController");
            this.debug.startTimer("AdminHttpController.Initialize");

            base.Initialize(controllerContext);

            this.header = new Header(this);

            Auth.Session.SetSession(this);

            this.connection = this.session.GetConnection ( connectionID : this.header.connection_id
                                                         , autoCommit   : this.header.commitTransaction
                                                         , keepAlive    : this.header.keepAlive);

            this.debug.stopTimer("AdminHttpController.Initialize");
        }

        // Internal Server Error

        private HttpResponseMessage InternalServerError(db.Error.QueryException exception) {

            this.debug.Log ( message : "InternalServerError"
                           , level   : Debug.Log.Level.error
                           , error   : exception);

            return this.response ( statusCode : HttpStatusCode.InternalServerError
                                 , data       : new QueryError(exception));
        }

        private HttpResponseMessage InternalServerError(OracleException exception) {

            this.debug.Log ( message : "InternalServerError"
                           , level   : Debug.Log.Level.error
                           , error   : exception);

            return this.response ( statusCode : HttpStatusCode.InternalServerError
                                 , data       : new Error<OracleException>(exception));
        }

        protected new HttpResponseMessage InternalServerError(Exception exception=null) {

            if (exception is db.Error.QueryException)
                return this.InternalServerError((db.Error.QueryException) exception);
            else if (exception is OracleException)
                return this.InternalServerError((OracleException) exception);

            this.debug.Log ( message : "InternalServerError"
                           , level   : Debug.Log.Level.error
                           , error   : exception);

            return this.response ( statusCode : HttpStatusCode.InternalServerError
                                 , data       : new Error<Exception>(exception));
        }

        protected HttpResponseMessage InternalServerError(string message) {
            Exception exception;
            exception = new Exception(message : message);
            return this.InternalServerError(exception : exception);
        }

        protected new HttpResponseMessage BadRequest(string message = "") {
            this.debug.Log ( message : "BadRequest : " + message
                           , level   : Debug.Log.Level.error);

            return this.response ( statusCode : HttpStatusCode.BadRequest);
        }

        protected virtual HttpResponseMessage response<T>(T[] list, HttpStatusCode statusCode=HttpStatusCode.OK, string message=null) {

            ModelList<T> data;

            data = new ModelList<T>(list);

            return this.response ( data       : data
                                 , statusCode : statusCode
                                 , message    : message);

        }

        /// <summary>
        ///     OK response with data
        /// </summary>
        /// <returns></returns>
        protected virtual HttpResponseMessage response<T> (T data, HttpStatusCode statusCode=HttpStatusCode.OK, string message=null)
            where T: Model.IAdminQueryResult {

            HttpResponseMessage response;
            Model.IAdminQueryResult queryResult;

            this.debug.startTimer("AdminHttpController.response");

            if (data != null)
                queryResult = data;
            else
                queryResult = new EmptyResponse();

            // Adding the message
            if (message != null) {
                if (queryResult.message == null)
                    queryResult.message = message;
                else
                    queryResult.message += " - " + message;
            }


            // Adding debug informations to the data
            try {
                queryResult.debug = new Debug.Debug(this);
            }
            catch (Exception) { }
            

            // Creating response
            response = this.Request.CreateResponse ( statusCode : statusCode
                                                   , value      : queryResult);

            // Adding the session ID to the header
            response.Headers.Add(EHeaders.session_id, this.session.sessionId);

            this.FinalizeResponse(response);

            if (!this.header.keepAlive)
                this.connection.Close();

            // Cleaning cursors
            db.BaseCursor.Clean();

            debug.stopTimer("AdminHttpController.response");
            debug.stopTimer("AdminHttpController");

            return response;
        }

        /// <summary>
        ///     OK response with data
        /// </summary>
        /// <returns></returns>
        protected HttpResponseMessage response(string message=null, HttpStatusCode statusCode=HttpStatusCode.OK) {
            return this.response ( data      : new EmptyResponse()
                                 , statusCode: statusCode
                                 , message   : message);
        }

        protected HttpResponseMessage NotFound(string message = null) {
            return this.response ( message    : message
                                 , statusCode : HttpStatusCode.NotFound);
        }

        protected HttpResponseMessage Ok(string message = null) {
            return this.response ( message    : message
                                 , statusCode : HttpStatusCode.OK);
        }

        protected HttpResponseMessage Ok<T>(T data, string message = null)
        where T: Model.IAdminQueryResult {

            return this.response ( data       : data
                                 , statusCode : HttpStatusCode.OK
                                 , message    : message);
        }

        protected HttpResponseMessage Ok<T>(T[] list, string message = null) {
            return this.response ( list       : list
                                 , statusCode : HttpStatusCode.OK
                                 , message    : message);
        }

        /// <summary>
        ///     Retrieve all files send as multipart
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RetrieveMultipartInformations() {
            await RetrieveMultipartInformations(uploadPath : Config.defaultUploadPath);
        }

        /// <summary>
        ///     Retrieve all files send as multipart
        /// </summary>
        /// <param name="uploadPath">Root path in wich files will be saved</param>
        /// <returns></returns>
        private async Task RetrieveMultipartInformations (string uploadPath) {

            if (this.provider != null)
                return;

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent()) {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            Stream reqStream = Request.Content.ReadAsStreamAsync().Result;
            if (reqStream.CanSeek) {
                reqStream.Position = 0;
            }

            this.provider = new MultipartFormDataStreamProvider(uploadPath);

            // Read the form data.
            await Request.Content.ReadAsMultipartAsync(this.provider);
        }

        protected HttpResponseMessage Forbidden(string message = null) {
            return this.response ( message    : message
                                 , statusCode : HttpStatusCode.Forbidden);
        }
        
        protected HttpResponseMessage Unauthorized(string message = null) {
            return this.response ( message    : message
                                 , statusCode : HttpStatusCode.Unauthorized);
        }

        public void SetConnection(db.AdminConnection connection) {
            if (this.connection != null)
                throw new Exception("The controller has already a connection defined");

            this.connection = connection;
        }

        /// <summary>
        ///     Define the session controller.
        ///     This function should be executed only once.
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(Auth.Session session) {
            
            if (this.session != null)
                throw new Exception("The controller has already a session defined");

            this.session = session;
        }
    }
}