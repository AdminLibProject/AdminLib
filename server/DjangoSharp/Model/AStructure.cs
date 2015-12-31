using DjangoSharp.Field;
using System;
using System.Collections.Generic;
using DjangoSharp.Query;
using System.Collections;
using System.Reflection;

namespace DjangoSharp.Model {

    using ListField = Dictionary<string, BaseField>;
    using ListGroup = Dictionary<string, List<BaseField>>;

    public abstract class AStructure {

        /******************** Static Attribute ********************/
        private static Dictionary<string, AStructure> modelsByApiName  = new Dictionary<string, AStructure>();

        /******************** Attributes ********************/
        /// <summary>
        ///     Name of the model in API calls
        /// </summary>
        public string ApiName   {
            get {
                return this.meta.apiName;
            }
        }

        /// <summary>
        ///     Database table of the model
        /// </summary>
        public string    dbTable  {
            get {
                return this.meta.dbTable;
            }
        }

        /// <summary>
        ///     List of all fields in the model
        /// </summary>
        public ListField fieldsByApiName       = new ListField();

        /// <summary>
        ///     Return the list of all fields in the structure
        /// </summary>
        public BaseField[] fields {
            get {
                return this._fields.ToArray();
            }
        }
        private List<BaseField> _fields = new List<BaseField>();

        /// <summary>
        ///     List of all groups in the model
        /// </summary>
        public ListGroup groups            = new ListGroup();

        
        /// <summary>
        ///     Field corresponding to the ID of the model.
        ///     This value will be null except if the model has only one primary key 
        ///     and if this primary key is an integer field.
        /// </summary>
        public Field.IntegerField IdField { get; private set; }

        /// <summary>
        ///     All meta informations about the model :
        ///         table name
        ///         api name
        ///         ...
        /// </summary>
        public Meta      meta      { get; private set; }

        /// <summary>
        ///     Model type.
        ///         Example : Country
        /// </summary>
        public Type ModelType      { get; private set; }

        /// <summary>
        ///     List of model type.
        ///         Example :
        ///         
        ///             Model Type    : Country
        ///             ModelListType : List<Country>
        /// </summary>
        public Type ModelListType  { get; private set; }

        /// <summary>
        ///     Array of model type
        ///         Example : 
        ///             ModelType      : Country
        ///             ModelArrayType : Country[]
        /// </summary>
        public Type ModelArrayType { get; private set; }

        /// <summary>
        ///     List of primary keys
        /// </summary>
        public BaseField[] primaryKeys {
            get {
                return this._primaryKeys.ToArray();
            }
        }

        /// <summary>
        ///     Indicate if the model is sequence based or not.
        ///     A sequence based model is a model that has :
        ///         - only one primary key
        ///         - the primary key is a IntegerField
        ///         - the primary key has define a sequence.
        ///         - the meta is flaged like sequence based
        /// </summary>
        public bool sequenceBased { get; private set; }

        private List<BaseField> _primaryKeys = new List<BaseField>();

        /******************** Methods ********************/

        protected void AddField(BaseField field) {

            // Meta object must be defined becore adding any field
            if (this.meta == null)
                throw new Exception("Meta not defined");

            if (this._fields.Contains(field))
                throw new Exception("Field already added");

            // Checking that the API name is not already declared
            if (this.fieldsByApiName.ContainsKey(field.CompleteName))
                throw new Exception("The field API name \"" + field.CompleteName + "\" already used");

            // Adding the field to it's group
            if (field.ApiGroup != null) {
                if (!this.groups.ContainsKey(field.ApiGroup))
                    this.groups[field.ApiGroup] = new List<BaseField>();
            }

            // Checking that the dbColumn isn't already used.
            if (this.GetFieldByDbColumn(field.dbColumn) != null)
                throw new Exception("dbColumn \"" + field.dbColumn + "\" already added to the model");

            this._fields.Add(field);
            this.fieldsByApiName[field.CompleteName] = field;

            // Adding primary keys
            if (field.primaryKey)
                this._primaryKeys.Add(field);
        }

        public ICollection CreateArray(int size) {
            return (ICollection) Activator.CreateInstance(this.ModelArrayType, size);
        }

        /// <summary>
        ///     Create a new intance of the model.
        /// </summary>
        /// <returns></returns>
        public Object CreateInstance() {
            return Activator.CreateInstance(this.ModelType);
        }

        /// <summary>
        ///     Create a list of object model.
        ///     Example :
        ///         Model: <Model: Country>
        ///         Return :
        ///             List<Country>
        /// </summary>
        /// <returns></returns>
        public IList CreateList() {
            return (IList) Activator.CreateInstance(this.ModelListType);
        }

        /// <summary>
        ///     Define the meta object of the model.
        ///     The function must be launched only once.
        /// </summary>
        /// <param name="meta"></param>
        protected void DefineMeta(Meta meta) {

            if (this.meta != null)
                throw new Exception("Meta already defined");

            if (AStructure.modelsByApiName.ContainsKey(meta.apiName))
                throw new Exception("A model with the API name \"" + meta.apiName + "\" has already been declared");

            this.meta = meta;
            this.meta.DefineModel(this);

            // Adding the model
            AStructure.modelsByApiName[this.meta.apiName] = this;
        }

        /// <summary>
        ///     Define the type of the model
        /// </summary>
        /// <param name="modelType"></param>
        protected void DefineType(Type modelType) {
            this.ModelType      = modelType;
            this.ModelListType  = typeof(List<>).MakeGenericType(new Type[] { this.ModelType });
            this.ModelArrayType = this.ModelType.MakeArrayType();
        }

        /// <summary>
        ///     Return the field of the given name
        /// </summary>
        /// <param name="name">Complete API name of the field</param>
        /// <returns></returns>
        public BaseField GetField(string name) {

            if (name.IndexOf(':') != -1) {
                name = name.Split(':')[0];
            }

            if (!this.fieldsByApiName.ContainsKey(name))
                return null;

            return this.fieldsByApiName[name];
        }

        /// <summary>
        ///     Return the field that correspond to the given dbColumn.
        ///     Note : ListField don't have a dbColumn (dbColumn=null).
        ///     If the function is called with dbColumn = null, then
        ///     NULL will be returned.
        /// </summary>
        /// <param name="dbColumn"></param>
        /// <returns></returns>
        public BaseField GetFieldByDbColumn(string dbColumn) {

            if (dbColumn == null)
                return null;

            foreach (BaseField field in this.fields) {
                if (field.dbColumn == dbColumn)
                    return field;
            }

            return null;
        }

        /// <summary>
        ///     Return all the fields of the given type
        /// </summary>
        /// <typeparam name="T">Type of fields to return</typeparam>
        /// <returns></returns>
        public T[] GetFields<T>()
            where T : BaseField {

            List<T> fields = new List<T>();

            foreach(BaseField field in this._fields) {

                if (!(field is T))
                    continue;

                fields.Add((T) field);
            }

            return fields.ToArray();
        }

        /// <summary>
        ///     Return the list of fields objects corresponding to the given api names
        /// </summary>
        /// <param name="fields">List of api names of fields to return.</param>
        /// <param name="silent">If true, then if the field don't exists, it will be replace by NULL. Otherwise it will raise an exception</param>
        /// <returns></returns>
        public BaseField[] GetFields(string[] fields, bool silent=false) {

            BaseField       field;
            List<BaseField> listFields;

            if (fields == null)
                return new BaseField[0];

            listFields = new List<BaseField>();

            foreach (string name in fields) {
                field = this.GetField(name);

                if (!silent && field == null)
                    throw new Exception("The field \"" + name + "\" don't exists");

                listFields.Add(field);
            }

            return listFields.ToArray();

        }

        /// <summary>
        ///     Return all the fields belonging to the given group
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BaseField[] GetGroup(string name) {
            if (!this.groups.ContainsKey(name))
                return null;

            return this.groups[name].ToArray();;
        }

        
        public int? GetId(object instance, bool dbValue=false) {

            if (this.IdField == null)
                throw new Exception("The model don't have an ID field");

            return (int?) this.IdField.GetValue ( instance : instance
                                                , dbValue  : dbValue);
        }

        /// <summary>
        ///     Return the value of the primary key.
        ///     Note that not test are made : if there is no primary key, an exception will be thrown.
        ///     If there is more than one primary key fields, the first declared field will be used
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object GetPrimaryKeyValue(object value) {
            return this.primaryKeys[0].GetValue(value);
        }

        /// <summary>
        ///     Initialize the structure
        /// </summary>
        public virtual void Initialize() {

            bool sequenceBased;

            foreach (BaseField field in this.fields) {

                // ListField will be initialized later : they need all foreign key of all models to be initialized first
                if (field is IListField)
                    continue;

                field.Initialize ( model : this);
            }

            this.IdField = null;

            if (this.primaryKeys.Length == 1) {

                if (this.primaryKeys[0] is Field.IntegerField)
                    this.IdField = (Field.IntegerField) this.primaryKeys[0];

            }

            // Determining if the model is sequence based.
            if (this.IdField == null)
                sequenceBased = false;
            else
                sequenceBased = this.IdField.sequence != null;

            if (sequenceBased && (this.meta.sequenceBased ?? true))
                this.sequenceBased = true;
            else if (!sequenceBased && this.meta.sequenceBased == true)
                throw new Exception("The model can't be sequence based (the ID field probably have no sequence)");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitializeListField() {

            foreach (BaseField field in this.fields) {
                if (!(field is IListField))
                    continue;

                field.Initialize(model: this);
            }

        }

        /// <summary>
        ///     Indicate if the given name correspond to a field (true) or not (false).
        ///     The name should be the complete name of the field.
        /// </summary>
        /// <param name="name">Complete API name of the field</param>
        /// <returns></returns>
        public bool IsField(string name) {
            return this.fieldsByApiName.ContainsKey(name);
        }

        /// <summary>
        ///     Indicate if the given name correspond to an exesting group in the model (true)
        ///     or not (false).
        ///     The name should be the complete API name of the group, meaning the group name 
        ///     and it's parent name.
        ///     For example :
        ///         Model : <Model: Center>
        ///         Group : contacts
        ///         Return : true
        ///         
        ///         Model : <Model: Center>
        ///         Group : contacts.manager
        ///         Return : true
        ///         
        ///         Model : <Model: Center>
        ///         Group : manager
        ///         Return : false
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsGroup(string name) {
            return this.groups.ContainsKey(name);
        }

        /// <summary>
        ///     Indicate if the given path is valid for the current model
        ///     Example :
        ///         Model  : Country
        ///         Path   : centers.name
        ///         Return : false
        ///         
        ///         Model  : Country
        ///         Path   : centers.label
        ///         Return : true
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsValidPath(string path) {
            return Path.IsValidPath ( rootModel : this
                                         , path      : path);
        }

        public Object QueryItem ( IConnection       connection
                                , int               id
                                , string[]          fields
                                , Filter filter  = null) {

            Object[]    items;
            FieldFilter pkFilter;

            if (this.primaryKeys.Length > 1)
                throw new Exception("Function not callable for models who has more than one primary key");

            pkFilter = new FieldFilter ( rootModel : this
                                       , path      : this.primaryKeys[0].CompleteName
                                       , type      : FilterOperator.equal
                                       , value     : id);

            items = this.QueryItems ( connection : connection
                                    , fields     : fields
                                    , filter     : new Filter(pkFilter)
                                    , orderBy    : null);

            if (items.Length == 0)
                return null;

            return items[0];
        }

        public Object QueryItem ( IConnection       connection
                                , object            model
                                , string[]          fields
                                , Filter filter=null) {

            Object[]    items;
            FieldFilter pkFilter;

            filter = filter ?? new Filter();

            foreach (BaseField field in this.primaryKeys) {

                pkFilter = new FieldFilter ( rootModel : this
                                           , path      : field.CompleteName
                                           , type      : FilterOperator.equal
                                           , value     : (string) field.GetValue(model));

                filter.Add(pkFilter);
            }

            items = this.QueryItems ( connection : connection
                                    , fields     : fields
                                    , filter     : filter);

            if (items.Length == 0)
                return null;

            // This normaly never happen
            if (items.Length > 1)
                throw new Exception("More than one item found");

            return items[0];
        }

        public Object[] QueryItems ( IConnection connection
                                   , string[]    fields
                                   , Filter      filter
                                   , OrderBy[]   orderBy = null) {

            SqlQuery sqlQuery;

            sqlQuery = new SqlQuery ( model   : this
                                    , fields  : fields
                                    , filter  : filter
                                    , sorting : orderBy);

            Object[] items = sqlQuery.ExecuteQuery(connection);

            return items;
        }


        public ICollection ToArray(IList list) {
            MethodInfo methodInfo;
            methodInfo = this.ModelListType.GetMethod("ToArray");

            return (ICollection) methodInfo.Invoke(list, null);
        }

        /******************** Static Attributes ********************/

        /// <summary>
        ///     Return the model structure corresponding to the given API Name
        ///     
        ///     Example :
        ///         AStructure.Get("country");
        /// 
        /// </summary>
        /// <param name="apiName">API name of the model</param>
        /// <returns></returns>
        public static AStructure Get(string apiName) {
            if (!AStructure.modelsByApiName.ContainsKey(apiName))
                return null;

            return AStructure.modelsByApiName[apiName];
        }
    }
}