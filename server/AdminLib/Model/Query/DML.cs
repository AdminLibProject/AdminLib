using AdminLib.Model.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AdminLib.Data.Store.Adapter;
using AdminLib.Data.Query;

namespace AdminLib.Model.Query
{
    internal static class DML {

        /******************** Methods ********************/

        /// <summary>
        ///     Reccord the instance into the database.
        ///     If the model is sequence based and the instance has no ID provided, then the function
        ///     will create a new ID using the sequence.
        ///     The newly created ID will be return.
        ///
        ///     If no ID has been created (e.g because the item is not sequence based), then null is returned.
        /// </summary>
        /// <returns>Newly created ID</returns>
        public static int? Create(Connection connection, AStructure model, object instance, string[] fields=null) {

            Field.BaseField[]     fieldsToCreate;
            bool                  createWithSequence;
            int?                  id;
            string                query;
            string                columnClause;
            string                fromClause;
            QueryParameter        parameter;
            string                parameterName;
            List<QueryParameter>  parameters;
            object                value;
            string                valueClause;

            columnClause = "";
            valueClause  = "";

            createWithSequence = false;
            id                 = null;

            // Checking if the item must be created with a sequence or not.
                
            // The model must have one and only one primary key
            if (model.sequenceBased) {
                id = model.GetId(instance, true);

                if (id == null)
                    createWithSequence = true;
            }

            // Retreiving the list of fields to create
            if (fields == null)
                fieldsToCreate = model.fields;
            else
                fieldsToCreate = model.GetFields(fields, false);

            parameters = new List<QueryParameter>();

            if (createWithSequence)
                id = DML.GetNextSequenceValue ( connection : connection
                                              , sequence   : model.IdField.sequence);

            // Building the columnClause and the valueClause.
            foreach (Field.BaseField field in fieldsToCreate) {

                if (field is Field.IMultipleValueField )
                    continue;

                columnClause += field.dbColumn + ",";
                parameterName = field.dbColumn;

                if (createWithSequence && field.primaryKey) {
                    value = id;
                    valueClause  += ":" + parameterName + ",";
                }
                else if (field.primaryKey && id == null) {
                    valueClause += "NVL(MAX(" + field.dbColumn + "), 0) + 1,";
                    value = null;
                }
                else {
                    value = field.GetValue(instance, true);
                    valueClause  += ":" + parameterName + ",";
                }

                parameter = new QueryParameter ( name  : parameterName
                                               , value : value);

                parameters.Add(parameter);
            }

            // Removing the last commas
            columnClause = columnClause.Substring(0, columnClause.Length - 1);
            valueClause   = valueClause.Substring(0, valueClause.Length - 1);

            if (!createWithSequence && id == null)
                fromClause = model.dbTable;
            else
                fromClause = "DUAL";

            query = "INSERT INTO " + model.dbTable
                    + " ( " + columnClause + ")"
                    + " SELECT " + valueClause
                    + "   FROM " + fromClause;

            connection.ExecuteDML ( query      : query
                                  , parameters : parameters.ToArray());

            return id;
        }

        /// <summary>
        ///     Delete in the database the reccord corresponding to the instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="model"></param>
        /// <param name="instance"></param>
        public static void Delete(Connection connection, AStructure model, object instance) {

            QueryParameter        parameter;
            string                parameterName;
            List<QueryParameter>  parameters;
            string                query;
            object                value;
            string                whereClause;

            parameters  = new List<QueryParameter>();
            whereClause = "";

            // Building the "WHERE" clause
            foreach(Field.BaseField field in model.primaryKeys) {
                
                value         = field.GetValue(instance, true);
                parameterName = field.dbColumn;

                if (value == null)
                    whereClause  += field.dbColumn + " IS NULL AND ";
                else
                    whereClause  += field.dbColumn + "= :" + parameterName + " AND ";

                parameter = new QueryParameter ( name  : parameterName
                                               , value : value);

                parameters.Add(parameter);
            }

            whereClause = whereClause.Substring(0, whereClause.Length - 5);

            query =   "DELETE FROM " + model.dbTable
                    + " WHERE " + whereClause;

            connection.ExecuteDML ( query      : query
                                  , parameters : parameters.ToArray());
        }

        /// <summary>
        ///     Return the NEXTVAL of the given sequence.
        /// </summary>
        /// <param name="connection">Connection to use to make the SQL query</param>
        /// <param name="sequence">Sequence to use</param>
        /// <returns></returns>
        public static int GetNextSequenceValue(Connection connection, string sequence) {

            string    query;
            DataTable data;

            query = "SELECT TO_NUMBER(" + sequence + ".NEXTVAL) FROM DUAL";

            data = connection.QueryDataTable(query : query);

            return Convert.ToInt32(data.Rows[0][0]);
        }

        /// <summary>
        ///     Update the corresponding row in the database.
        /// </summary>
        /// <param name="connection">Connection to use</param>
        /// <param name="model">Model corresponding to the instance</param>
        /// <param name="instance">Instance that will be updated in the database</param>
        /// <param name="fields">Fields to update. If null, then all fields will be updated. NULL fields will not be updated</param>
        /// <param name="emptyFields">All given fields will be emptied in the database</param>
        public static void Update ( Connection connection
                                  , AStructure model
                                  , object     instance
                                  , string[]   fields      = null
                                  , string[]   emptyFields = null) {

            Field.BaseField[]    fieldsToUpdate;
            Field.BaseField[]    fieldsToEmpty;
            QueryParameter       parameter;
            string               parameterName;
            List<QueryParameter> parameters;
            string               query;
            string               setClause;
            object               value;
            string               whereClause;

            whereClause = "";
            setClause   = "";

            parameters = new List<QueryParameter>();

            // Retreiving the list of fields to update
            if (fields == null)
                fieldsToUpdate = model.fields;
            else {
                fieldsToUpdate = model.GetFields(fields, false);
            }                

            // Retreiving the list of fields to empty
            fieldsToEmpty = model.GetFields(emptyFields, false);

            // Retreiving all updatable fields
            // All non-list fields are updatable

            // Building the "WHERE" and "SET" clause.
            foreach(Field.BaseField field in fieldsToUpdate) {

                parameterName = field.dbColumn;
                value         = field.GetValue(instance, true);

                if (field.primaryKey) {
                    // Primary key can't be updated
                    if (value == null)
                        whereClause += field.dbColumn + " IS NULL AND ";
                    else
                        whereClause += field.dbColumn + "=:" + parameterName + " AND ";
                }
                else {
                    if (fieldsToEmpty.Contains(field))
                        continue;
                    else if (value != null)
                        // COLUMN_NAME = NVL(:COLUMN_NAME, COLUMN_NAME)
                        // The column must not be emptied if the value is null.
                        setClause += field.dbColumn + "=NVL(:" + parameterName + "," + field.dbColumn + "),";
                }

                parameter = new QueryParameter ( name  : parameterName
                                               , value : value);

                parameters.Add(parameter);
            }

            // Adding the "SET" clause for columns to empty
            foreach (Field.BaseField field in fieldsToEmpty) {

                if (field is Field.ListField)
                    throw new Exception("Field " + field + " is a list field and can't be emptied");

                if (!field.nullable)
                    throw new Exception("Field " + field + " is not nullable");

                setClause += field.dbColumn + "=NULL,";
            }

            // Removing the " AND " tail
            whereClause = whereClause.Substring(0, whereClause.Length - 5);

            // Removing the "," tail.
            setClause   = setClause.Substring(0, setClause.Length -1);

            // Building the query
            query = "UPDATE "   + model.dbTable
                    + " SET "   + setClause
                    + " WHERE " + whereClause;

            connection.ExecuteDML ( query      : query
                                  , parameters : parameters.ToArray());

        }

    }
}