using AdminLib.Model.Model;
using AdminLib.Model.Query;
using System;
using AdminLib.Data.Store;
using AdminLib.Model.Interface;
using AdminLib.Data.Query;

namespace AdminLib.Model {
    public abstract class Model<Self> : IQueryResult
        where Self : Model<Self> {

        /******************** Static attributes ********************/
        public static ModelStructure structure;

        /******************** Attributes ********************/
        public Debug.Debug  debug   { get; set;}
        public string       message { get; set; }

        /******************** Fields ********************/

        private AStructure model {
            get{
                return Model<Self>.structure;
            }
        }

        /******************** Static Methods ********************/

        public static FieldFilter CreateFilter(string field, FilterOperator type, string value) {

            return new FieldFilter ( rootModel : structure
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, string[] value) {

            return new FieldFilter ( rootModel : structure
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, int value) {

            return new FieldFilter ( rootModel : structure
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, int[] value) {

            return new FieldFilter ( rootModel : structure
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static ModelStructure Initialize() {
            Model<Self>.structure = new ModelStructure(typeof(Self));

            if (Model<Self>.structure.primaryKeys.Length != 1)
                throw new Exception("The model have an incorrect number of primary keys");

            return Model<Self>.structure;
        }

        public static ModelStructure GetModelStructure() {
            return structure;
        }

        public static Self QueryItem(Connection connection, int id, string[] fields) {

            return (Self) Model<Self>.QueryItem ( connection : connection
                                                      , id         : id
                                                      , fields     : fields);

        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="connection">Connection to use</param>
        /// <param name="filter">List of filters</param>
        /// <param name="fields">API Name of the fields to return</param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public static Self[] QueryItems ( Connection connection
                                        , Filter     filter
                                        , string[]   fields
                                        , OrderBy[]  orderBy) {

            Object[] items;
            Self[]   results;

            items = Model<Self>.structure.QueryItems ( connection : connection
                                                     , filter     : filter
                                                     , fields     : fields
                                                     , orderBy    : orderBy);

            results = new Self[items.Length];

            items.CopyTo(results, 0);

            return results;
        }

        /******************** Methods ********************/
        public virtual void Add<Model>(Connection connection, Model model, string path=null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        ///     Create the instance into the database.
        ///     Note that if the model is sequence based and no ID has been provided
        ///     then the function will create a new ID (based on the sequence) and use
        ///     it for the database reccord.
        ///     The function will then update the ID of the object with the calculated one.
        ///     The function will NOT create subelements such as foreign key objects or
        ///     list fields.
        ///
        ///     Example :
        ///         
        ///         country : {id:null, code:'F'}
        ///         country.id; // null
        ///         country.Create(connection);
        ///         country.id; // 1
        ///         
        ///         country = {id:2, code:'G'}
        ///         country.id; // 2
        ///         country.Create(connection);
        ///         country.id; // 2
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="fields"></param>
        public virtual void Create(Connection connection, string[] fields=null) {

            int? id;

            id = connection.Create ( model      : this.model
                                   , instance   : this
                                   , fields     : fields);

            if (this.model.sequenceBased && id != null)
                this.model.IdField.SetValue ( instance : this
                                            , value    : id);
        }

        public virtual void Delete(Connection connection) {
            connection.Delete ( model      : Model<Self>.structure
                              , instance   : this);
        }

        /// <summary>
        ///     This function is to remove one item from the instance.
        ///     For example :
        ///     
        /// 
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <param name="connection"></param>
        /// <param name="model"></param>
        /// <param name="path"></param>
        public virtual void Remove<Model>(Connection connection, Model model, string path=null) {
            throw new NotImplementedException();
        }

        public virtual void Update(Connection connection, string[] fields=null, string[] emptyFields=null) {

            connection.Update ( model       : Model<Self>.structure
                              , instance    : this
                              , fields      : fields
                              , emptyFields : emptyFields);

        }
    }
}