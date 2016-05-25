using System;
using System.Collections.Generic;
using AdminLib.Model.Model;
using AdminLib.Model.Field;
using AdminLib.Data.Query;
using AdminLib.Model.Query;
using AdminLib.Model;
using Field = AdminLib.Model.Field;

namespace AdminLib.Data.Adapter {

    using System.Data;
    using System.Collections;
    /// <summary>
    ///     This class is small, but very complexe.
    ///     Sorry for that. I try to add a maximum of comment, but still... don't discourage ;-)
    /// </summary>
    internal class SqlQuery {

        /******************** Attributes ********************/
        public  List<Path>                            fields          = new List<Path>();
        public  Filter                                filter;
        private Dictionary<string, FromTable>         fromTables;
        private Dictionary<string,SubQuery>           nextQueries     = new Dictionary<string,SubQuery>();
        public  AStructure                            model;
        private List<QueryParameter>                  listParameters  = new List<QueryParameter>();
        private List<OrderByElement>                  orderBy         = new List<OrderByElement>();
        private Dictionary<string, List<FieldFilter>> subQueryFilters = new Dictionary<string,List<FieldFilter>>();
        public string query {get; private set; }

        private List<SelectColumn>                    selectColumns;
        private Dictionary<string, SubQuery>          subQueries      = new Dictionary<string,SubQuery>();

        public QueryParameter[] parameters {
            get {
                return this.listParameters.ToArray();
            }
        }


        /******************** Class & Structures ********************/

        private class FromTable {
            /***** Attributes *****/
            public string             alias       { get; private set; }
            public bool               external    { get; set;         }
            public SelectColumn       leftColumn  { get; private set; }
            public List<SelectColumn> listColumns = new List<SelectColumn>();
            public AStructure         model       { get; private set; }
            public string             modelPath   { get; private set; }
            public SqlQuery           query       { get; private set; }

            /// <summary>
            ///     List of all columns to export.
            ///     Note that this property should be called once the query is finished to be builded.
            /// </summary>
            private SelectColumn[]     _exportedColumns;
            public SelectColumn[]     exportedColumns {
                get {

                    List<SelectColumn> list;

                    if (this._exportedColumns != null)
                        return this._exportedColumns;

                    list = new List<SelectColumn>();

                    foreach (SelectColumn column in this.listColumns) {

                        if (column.export)
                            list.Add(column);

                    }

                    this._exportedColumns = list.ToArray();

                    return this._exportedColumns;
                }

            }

            /***** Constructors *****/
            public FromTable(AStructure model, string modelPath, bool external, SqlQuery query) {
                this.external    = external;
                this.model       = model;
                this.modelPath   = modelPath;
                this.query       = query;

                this.alias       = "table_" + this.query.fromTables.Count;
            }

            public FromTable(AStructure model, string modelPath, bool external, SelectColumn leftColumn, SqlQuery query) {
                this.external   = external;
                this.leftColumn = leftColumn;
                this.model      = model;
                this.modelPath  = modelPath;
                this.query      = query;

                this.alias      = "table_" + this.query.fromTables.Count;
            }

            /***** Methods *****/

            /// <summary>
            ///     Add a column to the table.
            ///     The fucntion will check if the column is not already added
            /// </summary>
            /// <param name="column"></param>
            public void Add(SelectColumn column) {

                if (column.table != this)
                    throw new Exception("Invalid column");

                if (this.listColumns.Contains(column))
                    return;

                this.listColumns.Add(column);
            }

            public override string ToString() {
                return this.model.dbTable + ' ' + this.alias;
            }

            public string WhereClause() {

                string leftSide;
                string rightSide;

                if (this.leftColumn == null)
                    return "";

                leftSide = leftColumn.ToString(false);
                rightSide =  this.alias + '.' + this.model.primaryKeys[0].dbColumn;

                rightSide += this.external ? " (+)" : "";

                return leftSide + " = " + rightSide;
            }

            public string WhereClause(FieldFilter filter) {

                return filter.toSQL ( tableAlias : this.alias
                                    , parameters : this.query.listParameters);

            }

        }

        private class GroupBy {

            /***** Attributes *****/
            public BaseField field { get; private set; }
            public FromTable table { get; private set; }

        }

        private class OrderByElement : IComparable<OrderByElement> {

            public int                 orderNumber {get; private set; }
            public AdminLib.Model.OrderByDirection orderBy     {get; private set; }
            public SelectColumn        column      {get; private set; }

            public OrderByElement(Path fieldPath, SelectColumn column) {
                this.orderBy     = fieldPath.orderBy ?? AdminLib.Model.OrderByDirection.asc;
                this.orderNumber = fieldPath.orderIndex ?? -1;
                this.column      = column;
            }

            public int CompareTo(OrderByElement other) {
                return this.orderNumber.CompareTo(other.orderNumber);
            }

            public override string ToString() {
                return this.column.ToString(false) + ' ' + (this.orderBy == AdminLib.Model.OrderByDirection.asc ? "ASC" : "DESC");
            }

        }

        private class SelectColumn {

            /***** Attributes *****/
            public string         alias          { get; private set; }
            public bool           export         = true;
            public bool           subQueryUsage  = false;
            public BaseField      field          { get; private set; }
            public GroupOperator? groupBy        { get; private set; }

            private Dictionary<string, object> queryInstances = new Dictionary<string,object>(); // key: id; value: instance
            public  List<string> subQueryValues               = new List<string>();

            public FromTable table              { get; private set; }

            public string    path {
                get {
                    return this.table.modelPath + "." + this.field.CompleteName;
                }
            }

            /***** Constructors *****/
            
            public SelectColumn(BaseField field, FromTable table, GroupOperator? groupBy=null, bool export=true) {
                this.field   = field;
                this.table   = table;
                this.groupBy = groupBy;
                this.table.Add(this);
                this.alias   = this.table.alias + "_" + this.table.listColumns.Count;
                this.export  = export;
            }

            /***** Method *****/
            public override bool Equals(object obj) {
                SelectColumn selectColumn;

                selectColumn = (SelectColumn) obj;

                return selectColumn.field == this.field && selectColumn.table == this.table;
            }
            
            public override int GetHashCode() {
                return (this.field.ToString() + " - " + this.table.alias).GetHashCode();
            }

            public object GetInstance(object id) {
                return this.queryInstances[id.ToString()];
            }

            /// <summary>
            ///     Set the value of the column field of the intance.
            ///     If the column is used for sub-queries, then we keep the value for later
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            public void SetValue(object instance, object value) {

                this.field.SetValue(instance, value);

                if (this.subQueryUsage) {
                    this.queryInstances[value.ToString()] = instance;
                    this.subQueryValues.Add(value.ToString());
                }
            }

            public override string ToString() {
                return this.ToString(true);
            }

            public string ToString(bool addAlias) {
                return this.table.alias + '.' + this.field.dbColumn + (addAlias ? " " + this.alias : "");
            }

        }

        private class SubQuery {

            /***** Attributes *****/
            public  BaseField             sourceField { get; private set; }
            private List<Path>       fields  = new List<Path>();
            private List<FieldFilter>     filters = new List<FieldFilter>();
            public  string                modelPath   { get; private set; }
            public  AStructure            rootModel   { get; private set; }
            public  SelectColumn          source      { get; private set; }
            public  SqlQuery              sqlQuery    { get; private set; }
            public  BaseField             field       { get; private set; }
            public  AStructure            parentModel { get; private set; }
            private Field.ManyToManyField manyToManyField;
            private bool                  exportRef;
            private BaseField             filterField;

            /***** Constructors *****/

            /// <summary>
            /// 
            ///     The entry field is the field in the parent model
            ///     For example :
            ///         <Field: {Country}.centers>
            ///         
            ///     The Source is the SelectColumn corresponding to
            ///     the primary key in the parent model.
            ///     For example :
            ///         <Field: {Country}.id>
            ///         
            ///     With theses wto informations, we can start a query
            ///     such as :
            ///         SELECT {fields}
            ///           FROM {rootModel}
            ///          WHERE {entryField} IN ({source.subQueryValues})
            ///     
            /// 
            /// </summary>
            /// <param name="entryField">Field in the parent model</param>
            /// <param name="source"></param>
            public SubQuery(BaseField sourceField, SelectColumn source, string modelPath) {

                Path fieldPath;

                this.sourceField = sourceField;
                this.source      = source;
                this.modelPath   = modelPath;

                if (this.sourceField is Field.ManyToManyField)  {
                    this.manyToManyField = (Field.ManyToManyField) this.sourceField;
                    this.field       = this.manyToManyField.midRefField;
                    this.rootModel   = this.field.model;
                    this.parentModel = ((IRefField) this.field).GetRefModel();

                    // TODO : not optimised : we add a foreign key and not the primary key

                    fieldPath = new Path ( rootModel : this.rootModel
                                         , path      : this.manyToManyField.midField.CompleteName);

                    this.filterField = this.manyToManyField.midField;

                    this.fields.Add(fieldPath);

                }
                else {
                    this.manyToManyField = null;
                    this.field       = ((IRefField) this.sourceField).GetRefField();
                    this.rootModel   = (ModelStructure) ((IRefField) this.sourceField).GetRefModel();
                    this.parentModel = sourceField.model;
                    this.filterField = this.field;
                }
                    
            }


            /***** Methods *****/
            /// <summary>
            ///     Add a field corresponding to the given path
            /// </summary>
            /// <param name="path"></param>
            public void Add(Path path) {

                string pathString;

                // If the subquery is for a many-to-many field
                // we have to "relocate" the field path to start from the
                // mid table
                if (this.manyToManyField != null) {

                    if (((IRefField) this.field).GetRefModel() != path.rootModel)
                        throw new Exception("Invalid root model");

                    pathString = this.field.CompleteName + '.' + path.pathString;
                    path = new Path ( rootModel : this.rootModel
                                    , path      : pathString);
                }

                // Checking that the root model is the same
                if (this.rootModel != path.rootModel)
                    throw new Exception("Invalid root model");

                if (this.fields.Contains(path))
                    return;

                this.fields.Add(path);
            }

            /// <summary>
            ///     Add a filter to the sub-query
            /// </summary>
            /// <param name="filter"></param>
            public void Add(FieldFilter filter) {

                string pathString;

                if (this.manyToManyField != null) {
                    pathString = Path.AddPath(this.field.CompleteName, filter.path.pathString);

                    filter = new FieldFilter ( rootModel : this.rootModel
                                             , path      : pathString
                                             , type      : filter.type
                                             , value     : filter.GetValues());

                }

                this.filters.Add(filter);
            }

            /// <summary>
            ///     Build the SQL query object from the given fields
            /// </summary>
            /// <returns></returns>
            private SqlQuery BuildSqlQuery() {
                
                FieldFilter       entryFilter;
                List<string>      fields;
                Filter            filter;
                OrderBy           orderBy;
                List<OrderBy>     sorting;
                string            refField;

                if (this.sqlQuery != null)
                    throw new Exception("SQL Query already builded");

                // Adding fields and order by columns
                fields  = new List<string>();
                sorting = new List<OrderBy>();

                foreach(Path fieldPath in this.fields) {

                    if (fieldPath.retrieve)
                        fields.Add(fieldPath.pathString);

                    if (fieldPath.orderBy != null) {

                        orderBy = new OrderBy ( field     : fieldPath.pathString
                                              , direction : fieldPath.orderBy ?? default(OrderByDirection) );

                        sorting.Add(orderBy);
                    }

                }

                refField = this.filterField.CompleteName + "." + ((IRefField) this.field).GetRefField().CompleteName;

                // Adding the referenced field
                if (!fields.Contains(refField)) {
                    fields.Add(refField);
                    this.exportRef = false;
                }
                else
                    this.exportRef = true;

                // Adding filters
                filter = new Filter(this.filters.ToArray());

                // Adding entry values
                entryFilter = new FieldFilter ( rootModel : this.rootModel
                                              , path      : this.filterField.CompleteName + "." + ((IRefField) this.field).GetRefField().CompleteName
                                              , type      : FilterOperator.inList
                                              , value     : this.source.subQueryValues.ToArray());

                filter.Add(entryFilter);

                this.sqlQuery = new SqlQuery ( model   : this.rootModel
                                             , fields  : fields.ToArray()
                                             , filter  : filter
                                             , sorting : sorting.ToArray());

                return this.sqlQuery;
            }

            public void Execute(SQLAdapter adapter) {

                object                    resultID;
                string                    parentObjectId;
                object[]                  results;
                Dictionary<string, IList> instancesList;

                this.BuildSqlQuery();

                results = this.sqlQuery.ExecuteQuery(adapter : adapter);

                instancesList = new Dictionary<string, IList>();

                if (this.manyToManyField != null) {

                    BaseField  midField;
                    BaseField  midRefField;
                    Object     midInstance;
                    Object     midRefInstance;
                    AStructure refModel;
                    AStructure model;

                    midField    = this.manyToManyField.midField;
                    midRefField = this.manyToManyField.midRefField;
                    refModel    = this.manyToManyField.refModel;
                    model       = this.manyToManyField.model;

                    foreach (Object result in results) {

                        midInstance    = midField.GetValue(result);

                        midRefInstance = midRefField.GetValue(result);

                        parentObjectId = (string) model.GetPrimaryKeyValue(midInstance).ToString();

                        if (!instancesList.ContainsKey(parentObjectId))
                            instancesList[parentObjectId] = refModel.CreateList();

                        instancesList[parentObjectId].Add(midRefInstance);
                    }

                    // Adding the list to each parent object
                    foreach(KeyValuePair<string, IList> entry in instancesList) {
                        this.sourceField.SetValue ( this.source.GetInstance(entry.Key)
                                                  , refModel.ToArray(entry.Value));
                    }

                }
                else
                {
                    // Creating the list for each parent object
                    foreach (Object result in results) {

                        resultID = this.field.GetValue(result);

                        /* if (!this.exportRef)
                            this.field.SetValue(result, null);*/

                        parentObjectId = this.parentModel.GetPrimaryKeyValue(resultID).ToString();

                        if (!instancesList.ContainsKey(parentObjectId)) {
                            instancesList[parentObjectId] = ((ModelStructure) this.rootModel).CreateList();
                        }

                        instancesList[parentObjectId].Add(result);
                    }

                    // Adding the list to each parent object
                    foreach(KeyValuePair<string, IList> entry in instancesList) {
                        this.sourceField.SetValue ( this.source.GetInstance(entry.Key)
                                                  , this.rootModel.ToArray(entry.Value));
                    }

                }                
            }

            public override string ToString()
            {
                return "<SubQuery: " + this.modelPath + '>';
            }
        }

        /******************** Constructors ********************/
        /// <summary>
        ///     Build the SQL query.
        ///         1. Adding all asked fields. if not fields are requested,
        ///            then all fields will be added, including listfields, many-to-many
        ///            fields and foreign key. However, their own listfields/m2m/fk fields
        ///            will not be added.
        ///         2. Initialization
        ///            
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields">Complete API name of each fields to return</param>
        /// <param name="filter"></param>
        public SqlQuery ( AStructure model
                        , string[]   fields
                        , Filter     filter
                        , OrderBy[]  sorting) {

            Dictionary<string, Path> dictFields;
            Path                     fieldPath;
            List<string>             listFields;
            OrderBy                  orderBy;

            this.filter = filter;
            this.model  = model;

            fields      = fields ?? new string[0];
            dictFields  = new Dictionary<string,Path>();

            sorting = sorting ?? new OrderBy[0];

            // If no fields asked, we add all fields of the model, including ListFields
            if (fields.Length == 0) {
                listFields = new List<string>();

                foreach (BaseField modelField in model.fields) {
                    listFields.Add(modelField.CompleteName);
                }

                fields = listFields.ToArray();
            }

            // Field to return
            foreach (string fieldApiPath in fields) {
                fieldPath = new Path ( rootModel : this.model
                                          , path      : fieldApiPath
                                          , export    : true
                                          , retrieve  : true);

                // Don't add twice the same field
                if (this.fields.Contains(fieldPath))
                    continue;

                dictFields[fieldPath.pathString] = fieldPath;

                this.fields.Add(fieldPath);
            }

            // Handling orderby
            for(int orderIndex=0; orderIndex < sorting.Length; orderIndex++) {

                orderBy = sorting[orderIndex];

                fieldPath = new Path ( rootModel : this.model
                                     , path      : orderBy.field
                                     , export    : false
                                     , retrieve  : false );

                if (dictFields.ContainsKey(fieldPath.pathString)) {
                    fieldPath = dictFields[fieldPath.pathString];
                }

                fieldPath.orderBy    = orderBy.direction;
                fieldPath.orderIndex = orderIndex;

                if (!dictFields.ContainsKey(fieldPath.pathString)) {
                    this.fields.Add(fieldPath);
                }
            }

            this.Initialize();
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Adding fitlers that will be used for sub-queries.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="modelPath"></param>
        public void AddFilterToSubQuery(BaseField field, FieldFilter filter, string modelPath) {

            modelPath = Path.AddPath(modelPath, field);

            if (!this.subQueryFilters.ContainsKey(modelPath))
                this.subQueryFilters[modelPath] = new List<FieldFilter>();

            // Relocating the filter from the root model
            // For example, if the filter was :
            //      root model : Country
            //      filter : <Filter: {Country}.centers.code
            //  Then, we want a new filter who will be :
            //      <Filter: {Center}.code>
            filter = filter.Clone(field);

            this.subQueryFilters[modelPath].Add(filter);
        }

        /// <summary>
        ///     Add a column to the list of selected columns
        ///     The function
        /// </summary>
        /// <param name="selectColumn"></param>
        private void AddSelect(SelectColumn selectColumn) {

            if (this.selectColumns.Contains(selectColumn))
                return;

            this.selectColumns.Add(selectColumn);
        }

        private void AddOrderBy(Path path, SelectColumn column) {

            OrderByElement orderBy;

            orderBy = new OrderByElement ( fieldPath : path
                                  , column    : column);

            this.orderBy.Add(orderBy);
        }

        /// <summary>
        ///     This function is call to add all tables necessary to
        ///     query the given field.
        ///     It will add all models in the path to the tables.
        ///     The function will return true if the field
        ///     can be added to the selected fields or not.
        ///     
        ///     Example :
        ///         path : user.profile.country.label
        ///         Added models :
        ///             <Model: User>
        ///             <Model: Profile>
        ///             <Model: Country>
        ///             
        ///     If one of the intermediate field is a IMultipleValueField
        ///     (such as ManyToManyField), then we will create a sub-query.
        ///         
        /// </summary>
        /// <param name="path">Path of the field we want to retreive</param>
        /// <param name="external">Indicate if intermediate table will be added with the "external" attribute or not</param>
        /// <returns></returns>
        private bool AddTable(bool external, Path path=null, FieldFilter filter = null) {

            BaseField    field;
            string       modelPath;
            SelectColumn selectColumn;
            string       tableAlias;
            string       previousModelPath;
            BaseField    previousField;
            SubQuery     subQuery;
            Path    subPath;
            bool         isLast;

            if (filter != null)
                path = filter.path;

            /* The model path represent the path to each model.
             * A path to a model will contain all intermediate models, fields and group
             * between the current model and the root model.
             */
            modelPath          = this.model.ApiName;
            previousField      = null;
            previousModelPath  = "";

            // Adding all intermediate tables of the field
            // Note that the first field is always a field of the root model.
            for(int f=0; f < path.Length; f++) {

                field      = path.path[f];
                isLast     = f == path.Length -1;
                tableAlias = "table_" + this.fromTables.Count;

                if (field is IMultipleValueField || (isLast && path.groupBy != null)) {
                    // Creating a sub-query to handle the multiple-value

                    subQuery = this.GetOrCreateSubQuery ( basePath  : modelPath
                                                        , entryField : field);

                    /* Adding the "global" field to the sub query. The path of the field will start
                     * from the modelPath :
                     * For example, if the function has been call with the given path :
                     *  path : <Path: {Country}.centers.label>
                     *  
                     * Then the path added to the sub query will be :
                     *  path : <Path: {Center}.label>
                     * 
                     */

                    /* If the MultipleValueField is the lat field in the path,
                     * then it is expected to retreive ALL fields of the referenced
                     * model, except the MultipleValueField.
                     * 
                     * Example :
                     * 
                     *      Root model : <Model: Country>
                     *      Path       : <Path: {Country}.centers>
                     * 
                     */
                    if (isLast) {
                        foreach(BaseField subField in ((IMultipleValueField) field).GetRefModel().fields) {
                            if (subField is IMultipleValueField)
                                continue;

                            subQuery.Add(path : subField.path);
                        }
                    }
                    else {
                        subQuery.Add(path : path.Clone(from: field));
                        subPath = path.Clone(from: field);
                    }
                        
                    if (filter != null)
                        this.AddFilterToSubQuery( field     : field
                                                , filter    : filter
                                                , modelPath : modelPath);

                    return false;
                }
                else {
                    if (this.fromTables.ContainsKey(modelPath))
                        this.fromTables[modelPath].external = external;
                    else {
                        selectColumn = new SelectColumn ( field   : previousField
                                                        , table   : this.fromTables[previousModelPath]
                                                        , groupBy : isLast ? path.groupBy : null );

                        // If the model hasn't been already encountered
                        // we add it to the dict table
                        fromTables[modelPath] = new FromTable ( model      : field.model
                                                              , modelPath  : modelPath
                                                              , leftColumn : selectColumn
                                                              , external   : external
                                                              , query      : this);
                    }
                }

                previousField      = field;
                previousModelPath = modelPath;
                modelPath         += '.' + field.CompleteName;
            }

            return true;
        }

        /// <summary>
        ///     Executing the query.
        ///     The query has been builded during the creation of the SqlQuery instance.
        ///     It's executed using the given adapter and the result is a DataTable.
        ///     The query will convert each row of the datatable to an instance of the model.
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public Object[] ExecuteQuery (SQLAdapter adapter) {

            DataColumn                 column;
            Dictionary<string, Object> currentResult;
            FromTable                  currentTable;
            DataTable                  dataTable;
            int                        nbRows;
            Object                     instance;
            FromTable                  parentTable;
            Object                     parentInstance;
            List<Object>               results;
            SelectColumn               selectColumn;
            SubQuery                   subQuery;
            List<FieldFilter>          subQueryFilters;
            FromTable                  table;
            Object                     value;

            dataTable = adapter.QueryDataTable ( query      : this.query
                                               , parameters : this.parameters);

            nbRows = dataTable.Rows.Count;

            if (nbRows == 0)
                return new Object[0];

            results = new List<object>();

            /*
             * For each row of the datatable we create a new instance of the Model.
             * In the same time, we create also all "sub-models".
             * For example, if the queried fields where :
             *      Root : {Center}
             *      Fields :
             *          - <Path: {Center}.label>
             *          - <Path: {Center}.country.label>
             *          
             * We will create both an Center instance and a Country instance.
             * The instances will be saved in the currentResult dictionary.
             * Each key of this dictionnary will be the ModelPath of the instance.
             * 
             * The next part, it's to add each models and sub models to it's parent.
             * In our example, we have to add the Country model as a value of the "country"
             * field :
             * 
             *          Center.country = Country;
             *  
             *  Last, set the value of each fields.
             *  The column in the DataTable are in the same order than the list this.selectColumns.
             *  
             *  For each column in this.selectColumns, we retreive it's instance using it's model path :
             *  The SelectColumn whose field' path is <Path: {Center}.country.label> will have it's "modelPath"
             *  variable equal to "center.country". In this case, we retreive the Country instance previously
             *  created.
             * 
             */

            foreach (DataRow row in dataTable.Rows) {

                currentResult = new Dictionary<string, Object>();

                // Creating an instance for all models that have been queried
                foreach(KeyValuePair<string, FromTable> entry in this.fromTables) {

                    table = entry.Value;

                    // Creating the model
                    currentResult[table.modelPath] = table.model.CreateInstance();
                }

                // Adding the model to it's parent.
                foreach (KeyValuePair<string, FromTable> entry in this.fromTables) {

                    table = entry.Value;

                    // Root model will not have a left column.
                    if (table.leftColumn == null || table.listColumns.Count == 0)
                        continue;

                    instance       = currentResult[table.modelPath];
                    parentTable    = table.leftColumn.table;
                    parentInstance = currentResult[parentTable.modelPath];
                    
                    table.leftColumn.field.SetValue(parentInstance, instance);
                }


                for (int c = 0; c < dataTable.Columns.Count; c++) {

                    /* If the field is a ForeignKey,
                     * then the value is an instance of the referenced type
                     * Example :
                     *      If field is <Field: {Center}.country>
                     *      Then the value must be an instance of Country
                     *      In the instance of Country, we will add the value of the column
                     */

                    selectColumn = this.selectColumns[c];
                    column       = dataTable.Columns[c];
                    currentTable = selectColumn.table;
                    instance     = currentResult[currentTable.modelPath];
                    value        = row[column];

                    selectColumn.SetValue(instance, value);
                }

                results.Add(currentResult[this.model.ApiName]);
            }

            // Executing sub queries
            foreach (KeyValuePair<string, SubQuery> entry in this.subQueries) {

                subQuery        = entry.Value;
                subQueryFilters = this.subQueryFilters.ContainsKey(subQuery.modelPath)
                                        ? this.subQueryFilters[subQuery.modelPath]
                                        : new List<FieldFilter>();

                foreach(FieldFilter filter in subQueryFilters) {
                    subQuery.Add(filter);
                }

                subQuery.Execute(adapter);
            }

            return results.ToArray();
        }

        /// <summary>
        ///     Initialize the query.
        ///     Mainly, the function will build the SQL query string.
        /// </summary>
        private void Initialize() {

            bool         addColumn;
            string       fromClause;
            string       modelPath;
            string       orderByClause;
            AStructure   refModel;
            string       selectClause;
            SelectColumn selectColumn;
            FromTable    table;
            string       where;
            string       whereClause;

            this.fromTables    = new Dictionary<string,FromTable>();
            this.selectColumns = new List<SelectColumn>();

            this.fromTables[this.model.ApiName] = new FromTable ( model     : this.model
                                                                , modelPath : this.model.ApiName
                                                                , external  : false
                                                                , query     : this);

            // Each fields have a different path :
            //  country.code
            //  country.centers.label
            //  country.centers.centerGroups.code
            //

            /* Creating the list of columns to select
             * For each fields :
             *  We determine the path of the field by looping on it's path.
             *  All intermediate models will be added to the "tables" dict with it's alias and it's modelPath.
             * 
             * A model path is a string concatanation of all model name between the root model and the field model.
             *  Example :
             *      Root model     : country
             *      Field API name : country.centers.centerGroups.label
             *      modelPath      : country.centers.centerGroups
             * 
             * The {tables} variable ensure us that we do not call a table twice. For example, the fields :
             *  
             *      country.center.label
             *      country.center.code
             *      
             *  have the same model path (country.center), so only one entry in the {tables} variable.
             *  
             *  For each field in the model path, we look the type of field. If it's a MultipleValueField
             *  we add it to the "externalField". We didn't add it earlier because we want to retreive the ID
             *  necessary to retreive it latter.
             *
             *  Example :
             *      If all field queried are :
             *      
             *      user.id
             *      user.email
             *      user.profile.grants.grantor
             *      
             *  Here, "profile" object is not needed. However we will want to retreive all grants objects : so, we
             *  still need to retreive the "profile.id" object, even if it's not requested at first.
             * 
             */
            foreach (Path path in this.fields) {

                modelPath     = this.model.ApiName;
                addColumn     = false;

                // Adding all intermediate tables of the field
                addColumn = this.AddTable ( path     : path
                                          , external : true);

                /*
                // If the field is not expected to be retrieve (e.g: for order by)
                // the we do not continue
                if (!path.retrieve)
                    continue;*/

                if (addColumn) {

                    if (path.previousField != null)
                        modelPath = Path.AddPath(modelPath, path.parent.pathString);

                    // Note that at this point, field is not a MultipleValueField.

                    // Adding the field.
                    if (path.field is IForeignKey) {

                        // If the field is a foreign key, we have to add all fields of the referenced model,
                        // but the model Foreignkeys and MultipleValues fields are not added
                        // For example :
                        //
                        //      api name : country.centers
                        //
                        //      Added fields :
                        //          <Field: center.id>
                        //          <Field: center.code>
                        //          <Field: center.label>
                        //
                        //      But not :
                        //          <Field: center.centerGroups>
                        //

                        refModel   = ((IForeignKey) path.field).GetRefModel();
                        modelPath += '.' + path.field.CompleteName;

                        // Adding the referenced model to the list of tables
                        if (!this.fromTables.ContainsKey(modelPath))
                            this.AddTable ( path     : path.GetChildPath(refModel.primaryKeys[0])
                                          , external : true);

                        foreach (BaseField refField in refModel.fields) {

                            if (refField is IMultipleValueField || refField is IForeignKey)
                                continue;

                            selectColumn = this.GetSelectColumn ( create        : true
                                                                , defaultExport : path.export
                                                                , field         : refField
                                                                , table         : this.fromTables[modelPath]);

                            if (path.retrieve)
                                this.AddSelect(selectColumn);

                            // If the path is expected to be ordered by, then we use only the primary key to order by
                            // For example:
                            // If we want to order by center.country
                            // then we order by center.country.id
                            if (path.orderBy != null && refField.primaryKey)
                                this.AddOrderBy(path, selectColumn);

                        }
                    }
                    else {

                        selectColumn = this.GetSelectColumn ( create        : true
                                                            , defaultExport : path.export
                                                            , field         : path.field
                                                            , table         : this.fromTables[modelPath]);

                        if (path.retrieve)
                            this.AddSelect(selectColumn);

                        if (path.orderBy != null)
                            this.AddOrderBy(path, selectColumn);

                    }
                }
            }

            // Adding intermediate tables needed for filters
            // All tables will be marked as not-external tables
            foreach (FieldFilter filter in this.filter.filters) {
                this.AddTable ( filter   : filter
                              , external : false);
            }

            /* 
             * 
             * Building SQL Query
             * 
             */

            // Select clause
            selectClause = "SELECT ";

            if (this.selectColumns.Count > 0) {

                foreach (SelectColumn column in this.selectColumns) {
                    selectClause += " " + column + ", ";
                }

                selectClause = selectClause.Remove(selectClause.Length - 2);
            }
            else {
                selectClause += "NULL";
            }

            // From clause
            fromClause = "FROM ";

            foreach (KeyValuePair<string, FromTable> entry in this.fromTables) {
                fromClause += entry.Value + ", ";
            }
            
            fromClause = fromClause.Remove(fromClause.Length - 2);
            
            // Where clause
            whereClause = "WHERE 1 = 1";

                // Joining tables
            foreach (KeyValuePair<string, FromTable> entry in this.fromTables) {
                table = entry.Value;
                where = table.WhereClause();

                if (where != "")
                    whereClause += " AND " + table.WhereClause();
            }

                // Filtering tables
            foreach (FieldFilter filter in this.filter.filters) {

                if (!this.fromTables.ContainsKey(filter.path.modelPath))
                    continue;

                table = this.fromTables[filter.path.modelPath];

                whereClause += " AND " + table.WhereClause(filter);
            }

            // Order By clause

            if (this.orderBy.Count > 0) {
                orderByClause = "ORDER BY ";

                this.orderBy.Sort();

                foreach (OrderByElement orderBy in this.orderBy) {
                    orderByClause += orderBy.ToString() + ',';
                }

                orderByClause = orderByClause.Substring(0, orderByClause.Length - 1);
            }
            else
                orderByClause = "";

            // Sql query
            this.query = selectClause + " " + fromClause + " " + whereClause + " " + orderByClause;
        }

        private SubQuery GetOrCreateSubQuery(string basePath, BaseField entryField) {

            string       modelPath;
            SubQuery     subQuery;
            SelectColumn selectColumn;
            string       subQueryFilterModelPath;

            modelPath = Path.AddPath(basePath, entryField);

            if (this.subQueries.ContainsKey(modelPath))
                return this.subQueries[modelPath];

            // Retreiving the primary key
            selectColumn = this.GetSelectColumn ( field         : entryField.model.primaryKeys[0]
                                                , table         : this.fromTables[basePath]
                                                , create        : true
                                                , defaultExport : false);

            selectColumn.subQueryUsage = true;

            subQueryFilterModelPath = Path.AddPath(this.model.ApiName, basePath);

            subQuery = new SubQuery ( sourceField : entryField
                                    , source      : selectColumn
                                    , modelPath   : modelPath);

            this.subQueries[modelPath] = subQuery;

            return subQuery;
        }

        private SelectColumn GetSelectColumn(BaseField field, FromTable table, bool create=false, bool defaultExport=false) {

            SelectColumn column;

            foreach (SelectColumn selectColumn in this.selectColumns) {

                if (selectColumn.field == field && selectColumn.table == table)
                    return selectColumn;
                
            }

            if (create) {
                column = new SelectColumn ( field  : field
                                          , table  : table
                                          , export : defaultExport);

                this.selectColumns.Add(column);

                return column;
            }

            return null;
        }

        public override string ToString() {
            return "<SQL Query: " + this.model.ApiName + ">";
        }

    }
}