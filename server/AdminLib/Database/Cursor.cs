using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace AdminLib.Database {
    public class Cursor<QueryStructure> : BaseCursor
        where QueryStructure : new() {

        /******************** Attributes ********************/
        private   bool              opened;
        private   OracleDataReader  reader;
        private   DataTable         schemaTable;

        /******************** Constructors ********************/
        public Cursor(AdminConnection connection, string query, OracleParameter[] parameters) : base(connection, query, parameters) { }
        public Cursor() : base() { }

        /******************** Static Methods ********************/

        /// <summary>
        ///     Return the cursor corresponding to the ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Cursor<QueryStructure> getCursor(string sessionID, int id) {

            return (Cursor<QueryStructure>) BaseCursor.GetCursor ( sessionID : sessionID
                                                                 , id        : id);
        }

        /******************** Methods ********************/
        /// <summary>
        ///     Close the cursor.
        /// </summary>
        public override void Close() {
            this.opened = false;
            this.reader.Close();
            this.reader.Dispose();
            this.reader = null;
            this.connection.unregisterCursor(this);

            base.Close();
        }

        public override void Dispose() {
            this.Close();
            this.parameters = null;
        }

        /// <summary>
        ///     The [count] rows from the cursor
        /// </summary>
        /// <param name="count">Number of rows to fetch</param>
        /// <returns></returns>
        public QueryStructure[] Fetch(int count = 100) {

            bool                 hasRows; // Use to know if there is still remaining rows to fetch
            QueryStructure[]     items;
            int                  nb;
            QueryStructure       row;
            List<QueryStructure> rows;

            if (!this.opened)
                this.Open();

            nb = 0;

            rows = new List<QueryStructure>();

            while (true){

                hasRows = this.reader.Read();

                if (!hasRows || nb >= count)
                    break;

                row = this.toRow();
                rows.Add(row);
                nb++;
            }

            items = rows.ToArray();
            this.UpdateLastAccessDate();

            if (!hasRows)
                this.Close();

            return items;
        }

        /// <summary>
        ///     Indicate if there is still reamaining rows to fetch
        /// </summary>
        /// <returns></returns>
        public bool HasRows() {

            bool hasRows; 

            hasRows = this.reader.HasRows;

            this.UpdateLastAccessDate();
            return hasRows;
        }

        /// <summary>
        ///     Indicate if the cursor is open (true) or not (false)
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen () {
            if (!this.opened)
                return false;

            return !this.reader.IsClosed;
        }

        /// <summary>
        ///     Open the cursor
        /// </summary>
        public override void Open() {

            OracleCommand command;

            this.connection.registerCursor(this);

            // Creating the command
            command = this.connection.getCommand(this.query);

            // Adding parameters
            foreach(OracleParameter parameter in this.parameters) {
                command.Parameters.Add(parameter);
            }

            this.reader      = command.ExecuteReader();
            this.schemaTable = this.reader.GetSchemaTable();
            this.opened      = true;

            base.Open();
            this.UpdateLastAccessDate();
        }

        /// <summary>
        ///     Convert the current reader value to a Row object.
        /// </summary>
        /// <returns></returns>
        private QueryStructure toRow() {

            DataColumn     columnDataType;
            string         columnName;
            QueryStructure instance;
            Type           typeRow;
            Type           typeInt;
            Type           typeIntNullable;
            PropertyInfo   property;

            instance        = new QueryStructure();
            typeRow         = typeof(QueryStructure);
            columnDataType  = this.schemaTable.Columns["DataType"];
            typeInt         = typeof(int);
            typeIntNullable = typeof(int?);

            foreach(DataRow queryColumn in this.schemaTable.Rows) {

                columnName = queryColumn["ColumnName"].ToString();

                property = typeRow.GetProperty(columnName);

                if (this.reader[columnName] is System.Decimal && property.PropertyType == typeInt)
                    property.SetValue(instance, Convert.ToInt32(this.reader[columnName]), null);

                else if (this.reader[columnName] is System.Decimal && property.PropertyType == typeIntNullable)
                    property.SetValue(instance, this.reader[columnName] == null ? (int?) null : Convert.ToInt32(this.reader[columnName]), null);

                else 
                    property.SetValue(instance, this.reader[columnName], null);
            }

            return instance;
        }
    }
}