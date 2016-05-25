using System.Data;
using AdminLib.Data.Query;
using AdminLib.Model.Model;
using AdminLib.Model.Query;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace AdminLib.Data.Adapter {

    public abstract class Adapter {

        /******************** Attrributes & Fields ********************/
        protected AdapterConfiguration adapterConfiguration;

        /******************** Static Attrributes & Fields ********************/
        private static Dictionary<string, ICreator> adapterCreators = new Dictionary<string, ICreator>();

        /******************** Classes & Structures ********************/
        [AttributeUsage ( AttributeTargets.Class
                        , AllowMultiple = false)
        ]
        public class AdapterDeclaration : Attribute {

            /***** Attributes *****/
            private string name;

            /***** Static attributes *****/
            internal List<AdapterDeclaration> declarations = new List<AdapterDeclaration>();

            /***** Constructors *****/
            public AdapterDeclaration(string name) {
                this.name = name;
            }

            /***** Static methods *****/
            internal void FindAllDeclarations() {

            }
        }

        public interface ICreator {
            Adapter GetNewAdapter ( AdapterConfiguration configuration
                                  , bool                 autoCommit);

            string name {get; }
        }

        /******************** Constructors ********************/
        public Adapter (AdapterConfiguration configuration) {
            this.adapterConfiguration = configuration;
            this.Initialize();
        }

        public Adapter (string configuration) {
            AdapterConfiguration adapterConfiguration;
            adapterConfiguration = AdapterConfiguration.GetAdapterConfiguration(configuration);

            this.adapterConfiguration = adapterConfiguration;
            this.Initialize();
        }

        /******************** Static Methods ********************/
        protected static void DeclareAdapter(ICreator creator) {
            if (Adapter.adapterCreators.ContainsKey(creator.name))
                throw new Exception("Creator already declared");

            Adapter.adapterCreators[creator.name] = creator;
        }

        public static Adapter GetAdapter ( bool   autoCommit
                                         , string configuration)  {

            Adapter              adapter;
            AdapterConfiguration adapterConfiguration;
            ICreator             creator;

            adapterConfiguration = AdapterConfiguration.GetAdapterConfiguration(configuration);

            creator = adapterConfiguration.adapterCreator;

            adapter = creator.GetNewAdapter ( autoCommit    : autoCommit
                                            , configuration : adapterConfiguration);

            return adapter;
        }

        /******************** Methods ********************/

        protected abstract void Initialize();

        // TODO : remove default parameter values : this interface is not meant to be used by "final" users.

        ConnectionState state {get; }

        public abstract DataTable QueryDataTable ( string           query
                                                 , QueryParameter[] parameters = null
                                                 , bool?            bindByName = null);

        public abstract int? Create(AStructure model, object instance, string[] fields=null);
        public abstract void Delete(AStructure model, object instance);

        public abstract void Update ( AStructure model
                                    , object     instance
                                    , string[]   fields      = null
                                    , string[]   emptyFields = null);

        /// <summary>
        /// Close the connection.
        /// 
        /// The closing will fail if there is remaining curors opened.
        /// 
        /// </summary>
        /// <param name="force">
        /// If true, then the closing WILL NOT fail if there is remaining cursors.
        /// </param>
        /// <param name="commitTransactions">
        /// If true, then all transactions will be commited before closing.
        /// </param>
        /// <returns>
        /// Indicate if the closing was successful (true) or not (false).
        /// The closing will be unsuccessful if force=false and there is still remaining cursors.
        /// </returns>
        public abstract bool Close ( bool force = false
                                   , bool? commitTransactions = null);

        public abstract void Commit();

        public abstract void ExecuteDML ( string           query
                                        , QueryParameter[] parameters = null
                                        , bool?            bindByName = null
                                        , bool?            commit     = null);

        public abstract T ExecuteFunction<T> ( string           function
                                             , QueryParameter[] parameters = null
                                             , bool?            bindByName = null
                                             , bool?            commit     = null);

        public abstract void ExecuteProcedure ( string           procedure
                                              , QueryParameter[] parameters = null
                                              , bool?            bindByName = null
                                              , bool?            commit     = null);

        public abstract void ExecuteCode ( string           code
                                         , QueryParameter[] parameters = null
                                         , bool?            bindByName = null
                                         , bool?            commit     = null);

        /// <summary>
        ///     Execute the given query.
        ///     All rows are fetched in one single time. If the query return 1.000.000 rows, all of them will be retreived
        /// </summary>
        /// <typeparam name="Row">Return type of each row</typeparam>
        /// <param name="sqlQuery">Query to execute</param>
        /// <param name="connection">Connection to use for execution</param>
        /// <param name="parameters">Parameters of the query</param>
        /// <returns></returns>
        public Row[] Query<Row> ( string           query
                                , QueryParameter[] parameters
                                , bool?            bindByName = null)
            where Row: new() {

            object            instance;
            int               nbRows;
            PropertyInfo      property;
            DataTable         queryTable;
            Row[]             results;
            DataRow           row;
            Type              typeInt;
            Type              typeIntNullable;
            Type              typeT;

            queryTable = this.QueryDataTable ( query      : query
                                             , parameters : parameters
                                             , bindByName : bindByName);

            nbRows = queryTable.Rows.Count;

            // Building the array of result type
            results = new Row[nbRows];

            typeT           = typeof(Row);
            typeInt         = typeof(int);
            typeIntNullable = typeof(int?);

            // Populating the result array
            for(int q=0; q < nbRows; q++) {

                row = queryTable.Rows[q];

                instance = new Row();

                foreach(DataColumn column in queryTable.Columns) {
                    property = typeT.GetProperty(column.ColumnName);

                    if (row[column] is System.DBNull)
                        continue;

                    if (row[column] is System.Decimal && property.PropertyType == typeInt)
                        property.SetValue(instance, Convert.ToInt32(row[column]), null);

                    else if (row[column] is System.Decimal && property.PropertyType == typeIntNullable)
                        property.SetValue(instance, row[column] == null ? (int?) null : Convert.ToInt32(row[column]), null);

                    else
                        property.SetValue(instance, row[column], null);
                }

                results[q] = (Row) instance;
            }

            return results;
        }

        public abstract object[] QueryItems ( AStructure model
                                            , Filter     filter
                                            , string[]   fields
                                            , OrderBy[]  orderBy);

        public abstract void RegisterCursor (BaseCursor cursor);

        public abstract void UnregisterCursor(BaseCursor cursor);

        public abstract void Rollback();
    }
}
