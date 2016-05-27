using System;
using System.Net.Http;
using AdminLib.Model.Model;
using AdminLib.Model.Query;
using AdminLib.Model;
using AdminLib.Data.Adapter;
using AdminLib.Model.Interface;
using AdminLib.Data.Query.Exception;

namespace AdminLib.Http
{

    public class ModelController<Model> : BaseController
        where Model: IModel, IQueryResult, new() {

        /****************** Attributes ******************/
        public static ModelStructure model {

            get {
                if (ModelController<Model>._model != null)
                    return ModelController<Model>._model;

                ModelController<Model>._model = ModelStructure.Get(typeof(Model));

                return ModelController<Model>._model;
            }
        
        }

        private static ModelStructure _model;

        /****************** Methods ******************/

        /// <summary>
        ///     Add an element to the model.
        ///     Example : adding a center to a center group
        ///     
        ///         Model : CenterGroup
        ///         SubModel : Center
        ///         id       : (ID of a center group)
        ///         element  : <center 1: Ile de France>
        ///         path     : null
        /// 
        /// </summary>
        /// <typeparam name="SubModel"></typeparam>
        /// <param name="id">ID of the parent element</param>
        /// <param name="element"></param>
        /// <param name="path">Path of the field to use for adding. This is usefull when there is several fields linked to the same model</param>
        /// <returns></returns>
        public HttpResponseMessage Add<SubModel>(int id, SubModel element, string path=null) {

            Model item;

            item = this.QueryItem(id: id);

            if (item == null)
                return this.NotFound();

            item.Add ( connection : this.connection
                     , item       : element
                     , path       : path);

            return this.Ok();
        }

        /// <summary>
        ///     Create a new item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Create(Model item) {

            object primaryKey;

            try {
                item.Create ( connection : this.connection);
            }
            catch (QueryException exception) {
                return this.InternalServerError(exception);
            }
            catch (Exception exception) {
                return this.InternalServerError(exception);
            }

            // If the query ask to return some fields, then we have to check that the primary key is compatible.
            // A compatible primary key is a IntegerField

            if (this.GetField().Length == 0)
                return this.response(item);

            if (this.GetModel().primaryKeys.Length != 1)
                return this.InternalServerError("The primary key is not usable for returning an object");

            if (!(this.GetModel().primaryKeys[0] is AdminLib.Model.Field.IntegerField))
                return this.InternalServerError("The primary key is not usable for returning an object");

            primaryKey = this.GetModel().GetPrimaryKeyValue(item);

            if (primaryKey == null)
                return this.InternalServerError("No primary key provided inside the object");

            return this.GetItem((int) primaryKey);
        }

        /// <summary>
        ///     Delete a item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Delete(int id) {

            Model item;

            item = this.QueryItem ( id: id
                                  , extraFields : new string[1] {this.GetModel().primaryKeys[0].ApiName});

            if (item == null)
                return this.NotFound();

            try {
                item.Delete(connection : this.connection);
            }
            catch (QueryException exception) {
                return this.InternalServerError(exception);
            }
            catch (Exception exception) {
                return this.InternalServerError(exception);
            }
            

            return this.Ok();
        }

        /// <summary>
        ///     Return the list of fields defined in the request
        /// </summary>
        /// <returns></returns>
        public string[] GetField() {
            return base.GetFields(this.GetModel());
        }

        /// <summary>
        ///     Return the list of filters defined in the request
        /// </summary>
        /// <returns></returns>
        public Filter GetFilter() {
            return base.GetFilter(this.GetModel());
        }

        public OrderBy[] GetOrderByFields(){
            return base.GetOrderByFields(this.GetModel());
        }

        /// <summary>
        ///     Return the item corresponding to the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage GetItem(int id) {
            
            Model item;
            /*
            try { */
                item = this.QueryItem(id : id); /*
            }
            catch (Exception exception) {
                return this.InternalServerError(exception);
            }*/

            if (item == null)
                return this.NotFound();
            else
                return this.response(data : (IQueryResult) item);
        }

        public virtual HttpResponseMessage GetItems() {
            return this.GetItems(fields:null);
        }

        /// <summary>
        ///     Return the list of items corresponding to the 
        /// </summary>
        /// <returns></returns>
        public virtual HttpResponseMessage GetItems(string[] fields=null) {

            Object[]         items;

            try {
                items     = this.QueryItems();
            }
            catch (AdminLib.Data.Query.Exception.QueryException exception) {
                return this.InternalServerError(exception);
            }
            catch (Exception exception) {
                return this.InternalServerError(exception);
            }

            return this.response(list : items);
        }

        public ModelStructure GetModel() {
            return ModelController<Model>.model;
        }

        /// <summary>
        ///     Query an item with the given ID. Will return the item with the requested fields
        ///     Extra filters could have been provided (mainly useful for sub-queries).
        /// </summary>
        /// <param name="id">ID of the item to retreive</param>
        /// <param name="extraFields">Extra fields to query</param>
        /// <returns></returns>
        public Model QueryItem(int id, string[] extraFields=null) {

            string[] fields;
            string[] allFields;

            fields      = this.GetField();

            if (extraFields != null) {
                extraFields = extraFields ?? new string[0];

                allFields = new string[fields.Length + extraFields.Length];

                fields.CopyTo(allFields, 0);
                extraFields.CopyTo(allFields, extraFields.Length - 1);
            }
            else
                allFields = fields;

            return (Model) this.GetModel().QueryItem ( connection : this.connection
                                                     , id         : id
                                                     , fields     : allFields
                                                     , filter     : this.GetFilter());

        }

        /// <summary>
        ///     Query an item 
        /// </summary>
        /// <returns></returns>
        public object[] QueryItems() {

            object[] items;

            items = this.GetModel().QueryItems ( connection : this.connection
                                               , fields     : this.GetField()
                                               , filter     : this.GetFilter()
                                               , orderBy    : this.GetOrderByFields());

            return items;
        }

        public object[] QueryItems( Filter    filter
                                  , string[]  fields
                                  , OrderBy[] orderBy) {

            object[] items;

            items = this.GetModel().QueryItems ( connection : this.connection
                                               , fields     : fields
                                               , filter     : filter
                                               , orderBy    : orderBy);

            return items;

        }

        /// <summary>
        ///     Remove an element to the model.
        ///     Example : removing a center to a center group
        ///     
        ///         Model : CenterGroup
        ///         SubModel : Center
        ///         id       : (ID of a center group)
        ///         element  : <center 1: Ile de France>
        ///         path     : null
        /// 
        /// </summary>
        /// <typeparam name="SubModel"></typeparam>
        /// <param name="id">ID of the parent element</param>
        /// <param name="element"></param>
        /// <param name="path">Path of the field to use for adding. This is usefull when there is several fields linked to the same model</param>
        /// <returns></returns>
        public HttpResponseMessage Remove<SubModel>(int id, SubModel element, string path=null) {

            Model item;

            item = this.QueryItem(id: id);

            if (item == null)
                return this.NotFound();

            item.Remove ( connection : this.connection
                        , item       : element
                        , path       : path);

            return this.Ok();
        }

        /// <summary>
        ///     Update an item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage Update(int id, Model item) {

            string[] emptyFields;
            int?     itemID;

            emptyFields = this.GetParameterValues("emptyFields");

            itemID = (int?) this.GetModel().GetPrimaryKeyValue(item);

            // If the item has an id, we check that this ID is consistant with the provided ID of the update
            // If the item has no ID, we add the provided ID to it
            if (itemID != null) {
                if (itemID != id )
                    return this.InternalServerError();
            }
            else {
                this.GetModel().primaryKeys[0].SetValue(item, id);
            }

            try {
                item.Update ( connection  : connection
                            , emptyFields : emptyFields);
            }
            catch (QueryException exception) {
                return this.InternalServerError(exception);
            }
            catch (Exception exception) {
                return this.InternalServerError(exception);
            }

            return this.GetItem(id : id);
        }

        /******************** Static methods ********************/
        public static FieldFilter CreateFilter(string field, FilterOperator type, string value) {

            return new FieldFilter ( rootModel : ModelController<Model>.model
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, string[] value) {

            return new FieldFilter ( rootModel : ModelController<Model>.model
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, int value) {

            return new FieldFilter ( rootModel : ModelController<Model>.model
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

        public static FieldFilter CreateFilter(string field, FilterOperator type, int[] value) {

            return new FieldFilter ( rootModel : ModelController<Model>.model
                                   , path      : field
                                   , type      : type
                                   , value     : value);

        }

    }

}