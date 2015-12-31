using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DjangoSharp.Field;
using DjangoSharp.Model;

namespace DjangoSharp.Query {
    /// <summary>
    ///     Represent a path of a field;
    ///     For example, the path of :
    ///         
    ///         country.centers.centerGroups.label
    ///         
    ///     is :
    ///         [<Field: Country.centers>, <Field: Center.centerGroups>, <Field: centerGroup.label>]
    /// 
    ///     Note that if a field belong to a group, the group don't appear in the path.
    ///     Example :
    ///     
    ///         country.centers.contact.manager.phone
    ///         
    ///     is :
    ///         [<Field: Country.centers>, <Field: Center.manager_phone>]
    /// 
    /// </summary>
    public class Path {

        /******************** Attributes ********************/

        /// <summary>
        /// Indicate if the field must be exported when queried or not
        /// </summary>
        public bool export { get; private set; }

        /// <summary>
        ///     Final field to wich lead the path
        ///     For example :
        /// 
        ///         api path : country.centers.centerGroups.label
        ///         field    : <Field: centerGroup.label>
        /// 
        /// </summary>
        public BaseField field {
            get {
                if (this.listPath == null)
                    return null;

                if (this.listPath.Count == 0)
                    return null;

                return this.listPath[this.listPath.Count - 1];
            }
        }

        /// <summary>
        ///     Group by operator.
        ///     The group by operator is always on the last field;
        /// </summary>
        public GroupOperator? groupBy { get; private set; }

        /// <summary>
        ///     Return the number of fields in the path
        /// </summary>
        public int Length {
            get {
                if (this.listPath == null)
                    return 0;

                return this.listPath.Count;
            }
        }

        private List<BaseField>  listPath;

        /// <summary>
        ///     Indicate if the field should tbe ordered or not.
        /// </summary>
        public OrderByDirection? orderBy;
        public int?     orderIndex;

        /// <summary>
        ///     Model path of the field.
        ///     Example :
        ///         country.center.centerGroup
        /// </summary>
        public string modelPath { get; private set; }

        /// <summary>
        ///     Return the path to the parent field.
        ///     Example :
        ///     
        ///         path       : {job/instance}.creator.profile.firstName
        ///         parentPath : {job/instance}.creator.profile
        ///         
        /// </summary>
        
        private Path _parent;
        public Path parent {

            get {

                if (this.path.Length <= 1)
                    return null;

                if (this._parent != null)
                    return this._parent;

                this._parent = new Path(this.rootModel, this.parentPath );

                return this._parent;
            }
        }

        /// <summary>
        ///     Return the string of the parent path.
        /// </summary>
        private string parentPath {

            get {

                int i;
                string parentPath;

                if (this.listPath.Count <= 1)
                    return null;

                parentPath = "";

                i = 0;

                while (i < this.listPath.Count - 1) {
                    parentPath += this.listPath[i].CompleteName + '.';
                    i++;
                }

                return parentPath.Substring(0, parentPath.Length -  1);
            }

        }

        /// <summary>
        ///     List of all intermediate fields.
        ///     
        ///     For example, the path of :
        ///         
        ///         country.centers.centerGroups.label
        ///         
        ///     is :
        ///         [<Field: Country.centers>, <Field: Center.centerGroups>, <Field: centerGroup.label>]
        /// </summary>
        public BaseField[] path {
            get {

                if (this.listPath == null)
                    return null;

                return this.listPath.ToArray();
            }
        }

        /// <summary>
        ///     String representation of the path from the rootModel to the field
        ///     Example :
        ///         Root model : <Model: country>
        ///         FieldPath : country.centers.centerGroups.label
        /// </summary>
        public string pathString { get; private set; }

        public BaseField previousField {
            get {
                if (this.path.Length <= 1)
                    return null;

                return this.path[this.path.Length - 2];
            }
        }

        /// <summary>
        /// Indicate if the field must be retrieve from the database or not.
        /// A not retrieve field is for example a field that is used for an Order By
        /// </summary>
        public bool retrieve;

        /// <summary>
        ///     Root model of the path
        /// </summary>
        public AStructure rootModel;

        /******************** Constructors ********************/
        /// <summary>
        ///     Complete path of the field from the root model (not included) :
        ///     
        ///     Example :
        ///         rootModel : <Model: Country>
        ///         path      : id
        ///         
        ///         rootModel : <Model: Country>
        ///         path      : centers.contacts.manager.phone
        ///         
        /// </summary>
        /// <param name="rootModel"></param>
        /// <param name="path"></param>
        /// <param name="retrieve"></param>
        /// <param name="export"></param>
        public Path(AStructure rootModel, string path, bool retrieve = true, bool? export = null, OrderByDirection? orderBy=null) {

            AStructure currentModel;
            BaseField  field;
            string     fieldGroup;
            string     fieldName;
            string[]   fieldsName;
            bool       isLast;

            this.retrieve   = retrieve;
            this.rootModel  = rootModel;
            this.pathString = path;
            this.listPath   = new List<BaseField>();
            this.modelPath  = this.rootModel.ApiName;

            this.export     = export == null ? this.retrieve : export ?? true;

            if (this.pathString != "") {
                fieldsName      = path.Split(new Char[1] {'.'});
                fieldGroup      = null;
                currentModel    = rootModel;

                for (int fn = 0; fn < fieldsName.Length; fn++) {

                    isLast = fn == fieldsName.Length -1;

                    fieldName = (fieldGroup != null? fieldGroup + "." : "" ) + fieldsName[fn];

                    // If the field name correspond to a group, then we add the field name at the end of
                    // the group name and go to the next field.
                    // We do to build the complete name of the field.
                    if (currentModel.IsGroup(fieldName)) {
                        fieldGroup = fieldName;
                        continue;
                    }
                    else
                        fieldGroup = null;

                    field = currentModel.GetField(fieldName);

                    if (field == null)
                        throw new Exception("Field not found");

                    this.listPath.Add(field);

                    this.groupBy = BaseField.GetGroupByOperator(fieldName);

                    if (this.groupBy != null && !isLast)
                        throw new Exception("The group by operator must be on the last field");

                    // If the field is not a ForeignKey, a ListField, a OneToOneField or a ManyToManyField
                    // Then it should be the last field
                    if (!(field is IRefField) && !isLast)
                        throw new Exception("The field \"" + fieldName + "\" can't have sub elements");

                    if (!isLast) {
                        currentModel = ((IRefField) field).GetRefModel();
                        this.modelPath += '.' + field.CompleteName;
                    }
                    
                }
            }
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Clone the object.
        ///     If a field is provided, then the first element of the path
        ///     will start from the given field (excluded)
        ///     Note that in this case, the rootModel will change.
        ///     For example :
        ///         Path : <path: {Country} centers.label>
        ///         Return :
        ///             <path: {Center}.label>
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public Path Clone(BaseField from) {

            Path       newPath;
            string          pathString;
            List<BaseField> path;
            bool            start;
            AStructure      rootModel;

            from      = from ?? this.field;
            rootModel = from.model;
            start     = false;

            path = new List<BaseField>();

            foreach (BaseField field in this.path) {

                if (!start && field != from)
                    continue;

                /* 
                 * We want to start to add field only AFTER
                 * finding the from field.
                 */
                if (!start) {
                    start = true;
                    continue;
                }

                path.Add(field);
                start = true;
            }

            if (path.Count == 0)
                return null;

            pathString = Path.GetApiPath(path.ToArray());

            newPath = new Path ( rootModel : path[0].model
                                    , path      : pathString
                                    , retrieve  : this.retrieve
                                    , export    : this.export);

            newPath.orderBy    = this.orderBy;
            newPath.orderIndex = this.orderIndex;

            return newPath;
        }

        public override bool Equals(object obj) {
            if (!(obj is Path))
                return false;

            return this.pathString == ((Path) obj).pathString;
        }

        /// <summary>
        ///     Return a FieldPath object corresponding to a field of the referenced model.
        ///     For example :
        ///     
        ///         Model :
        ///             Country
        ///                 - {int}    id
        ///                 - {Center} centers
        ///                 - {string} label
        ///                 
        ///             Center :
        ///                 - {int}    id
        ///                 - {string} label
        ///                     
        /// 
        ///         this  : <Path: {Country}.centers
        ///         field : <Field: {Center}.label
        ///         
        ///         Return :
        ///             <Path: {Country}.centers.label>
        ///         
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public Path GetChildPath(BaseField field) {

            IRefField refField;

            if (!(this.field is IRefField))
                throw new Exception("The field can't have children");

            refField = (IRefField) this.field;
            
            if (refField.GetRefModel() != field.model)
                throw new Exception("The field is not a child field");

            return new Path ( rootModel : this.rootModel
                                 , path      : this.pathString + '.' + field.CompleteName);

        }

        /// <summary>
        ///     Return the first field in the path that is of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BaseField GetFirst<T>() 
            where T : IField {

            foreach(BaseField field in this.path) {
                if (field is T)
                    return field;
            }

            return null;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return "<Path: {" + this.rootModel.ApiName + "} " + this.pathString + '>';
        }

        /******************** Static Methods ********************/

        public static string AddPath(string root, string element) {
            return root + "." + element;
        }

        public static string AddPath(string root, BaseField element) {
            return root + "." + element.CompleteName;
        }

        public static string AddPath(AStructure root, string element) {
            return root.ApiName + "." + element;
        }

        /// <summary>
        ///     Return a FieldPath object of a sibling fields.
        /// </summary>
        /// <param name="fieldPath"></param>
        /// <param name="sibling">Sibling field</param>
        /// <returns></returns>
        public static Path GetSiblingPath(Path fieldPath, BaseField sibling) {

            BaseField[] path;

            if (sibling.model != fieldPath.field.model)
                throw new Exception("The sibling field don\'t belong to the same model");

            path = fieldPath.path;

            path[path.Length - 1] = sibling;

            return new Path ( rootModel : fieldPath.rootModel
                                 , path      : Path.GetApiPath(path));
        }

        /// <summary>
        ///     Build the API path string of the given path.
        ///     
        ///     Example :
        ///         fields    : [<Field: Country.centers>, <Field: Center.centerGroups>, <Field: CenterGroup.label>]
        ///         
        ///         Return :
        ///             country.centers.centerGroups.label
        ///             
        ///         
        ///         fields: [<Field: Country.centers>, <Field: Center.contacts.manager.phone>]
        ///         
        ///         Return :
        ///             country.centers.contacts.manager.phone
        /// </summary>
        /// <param name="fields">List of fields in the path</param>
        /// <returns></returns>
        public static string GetApiPath(BaseField[] fields) {

            string apiPath;

            if (fields == null)
                return null;

            if (fields.Length == 0)
                return "";

            apiPath = "";

            foreach (BaseField field in fields) {
                apiPath += field.CompleteName + '.';
            }

            apiPath = apiPath.Remove(apiPath.Length - 1);

            return apiPath;
        }

        public static bool IsValidPath ( AStructure rootModel
                                       , string     path)
        {

            try {
                new Path ( rootModel : rootModel
                              , path      : path);

                return true;
            }
            catch(Exception) {
                return false;
            }

        }
    }
}