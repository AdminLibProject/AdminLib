using AdminLib.Model.Model;
using AdminLib.Model.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AdminLib.Model.Field {

    public abstract class BaseField : IField {

        /******************** Attributes ********************/
        /// <summary>
        ///     Name of the group in wich the field will belong in the JSON object.
        /// </summary>
        public string ApiGroup   { get; private set; }

        /// <summary>
        ///     Name of the field on API call and in JSON objects
        /// </summary>
        public string ApiName    { get; private set; }

        /// <summary>
        ///     Name of the attribute in the "Model" object to wich the field is linked.
        /// </summary>
        public MemberInfo Attribute  { get; private set; }

        private PropertyInfo AttributeAsProperty;
        private FieldInfo    AttributeAsField;

        /// <summary>
        ///     Type of the attribute :
        /// </summary>
        public Type AttributeType { get; private set; }

        /// <summary>
        ///     Concatanation of the apiGroup and the apiName.
        ///     Example :
        ///         apiName      : phone
        ///         apiGroup     : contact.manager
        ///         completeName : contact.manager.phone
        /// </summary>
        public string CompleteName { get; private set; }

        public Array tmp;

        /// <summary>
        ///     Database column on wich the field is based on.
        /// </summary>
        public string dbColumn   { get; protected set; }

        /// <summary>
        ///     Indicate if the field is already initialized (true) or not (false).
        /// </summary>
        public bool  Initialized {get; private set; }

        /// <summary>
        ///     Structure in wich belong the field
        /// </summary>
        public AStructure model { get; private set; }

        public Path  path  { get; private set; }

        /// <summary>
        ///     If true, then the field is the primary key of his model.
        /// </summary>
        public bool   primaryKey { get; private set; }

        /// <summary>
        ///     Indicate the primary key index of the field in the model.
        /// </summary>
        public int?   primaryKeyIndex { get; private set; }

        /// <summary>
        ///     If true, then the field accept NULL values.
        /// </summary>
        public bool   nullable   { get; private set; }

        /// <summary>
        ///     If true, then the field have an unique value.
        /// </summary>
        public bool   unique     { get; private set; }

        /// <summary>
        ///     Indicate if the field is a virtual field or not.
        ///     A virtual field is a field who don't have an attribute.
        ///     For now, they are usefull only in VirtualStructure.
        /// </summary>
        public bool Virtual     { get; private set; }

        /******************** Exception ********************/
        public class InvalidField : Exception {
            public InvalidField(string message) : base(message) {}
        }

        /******************** Exception ********************/
        public class InvalidValue : Exception {
            public InvalidValue(string message) : base(message) {}
        }

        /******************** Constructors ********************/
        public BaseField ( string dbColumn
                         , string apiName    = null
                         , string apiGroup   = null
                         , bool   primaryKey = false
                         , bool?  nullable   = null
                         , bool?  unique     = null)
        {
            this.ApiName      = apiName;
            this.ApiGroup     = apiGroup;
            this.dbColumn     = dbColumn;
            this.nullable     = nullable ?? !this.primaryKey;
            this.primaryKey   = primaryKey;
            this.unique       = unique ?? this.primaryKey;
            this.Initialized  = false;
            this.Virtual      = true;

            if (this.primaryKey && !this.unique)
                throw new InvalidField("Primary key must be unique");

            if (this.ApiName != null)
                this.CompleteName = (this.ApiGroup != null ? this.ApiGroup + "." : "") + this.ApiName;

        }

        public BaseField() { }

        /******************** Methods ********************/

        public virtual object FromDbValue(object value) {
            return value;
        }

        /// <summary>
        ///     Convert the value to the database value.
        ///     The received value is never null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string ToDbValue(object value) {
            return value.ToString();
        }

        public void DefineAttribute(MemberInfo attribute) {

            // You can't define the attribute if the field is already initialized
            if (this.Initialized)
                throw new Exception("Field already initialized");

            // You can't redefined the attribute
            if (this.Attribute != null)
                throw new Exception("Member already defined");

            this.Attribute = attribute;
            this.Virtual   = false;

            if (this.ApiName == null) {
                this.ApiName      = this.Attribute.Name;
            }
                
            this.CompleteName = (this.ApiGroup != null ? this.ApiGroup + "." : "") + this.ApiName;
            
            if (this.Attribute.MemberType == MemberTypes.Field) {
                this.AttributeAsField = (FieldInfo) this.Attribute;
                this.AttributeType = this.AttributeAsField.FieldType;
            }
            else {
                this.AttributeAsProperty = (PropertyInfo) this.Attribute;
                this.AttributeType = this.AttributeAsProperty.PropertyType;
            }
        }

        public string GetDbColumn() {
            return this.dbColumn;
        }

        /// <summary>
        ///     Return the value of the field in the instance.
        ///     If the field is virtual, the instance is expected to be a Dictionary<string, Object> object.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dbValue">
        /// If true, then the db value will be returned. This is specially usefull for foreign key when you want to retreive the ID number and not the referenced instance
        /// </param>
        /// <returns></returns>
        public virtual object GetValue(Object instance, bool dbValue=false) {

            object                     value;
            Dictionary<string, Object> virtualInstance;

            if (this.Virtual) {
                virtualInstance = (Dictionary<string, Object>) instance;

                if (!virtualInstance.ContainsKey(this.CompleteName))
                    value = null;
                else
                    value = virtualInstance[this.CompleteName];
            }
            else if (instance == null)
                value = null;
            else if (this.AttributeAsField != null)
                value = this.AttributeAsField.GetValue(instance);
            else
                value = this.AttributeAsProperty.GetValue(instance);

            if (dbValue && value != null)
                value = this.ToDbValue(value);

            return value;
        }

        /// <summary>
        ///     Define the name of the attribute of the field
        /// </summary>
        /// <param name="attribute"></param>
        public virtual void Initialize(AStructure model) {

            if (this.Initialized)
                throw new Exception("The field is already initialized");

            this.model       = model;
            this.Initialized = true;
            this.path        = new Path ( rootModel : this.model
                                             , path      : this.CompleteName);

            if (! BaseField.IsValidApiName(this.ApiName))
                throw new Exception("Invalid field name");
        }

        /// <summary>
        ///     Set the field value of the instance.
        ///     For virtual fields, the instance should be a Dictionary<string, Object> object.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        public void SetValue(Object instance, Object value) {

            Dictionary<string, Object> virtualInstance;
            value = this.FromDbValue(value);

            if (value is DBNull)
                value = null;

            if (this.Virtual) {

                virtualInstance = (Dictionary<string, Object>) instance;
                virtualInstance[this.CompleteName] = value;

            }
            else {
                if (this.AttributeAsField != null /* && this.AttributeAsField is DBNull*/ )
                    this.AttributeAsField.SetValue(instance, value);
                else
                    this.AttributeAsProperty.SetValue(instance, value);
            }

        }

        public override string ToString() {
            return "<Field : {" + this.model.ApiName + "}." + this.CompleteName + ">";
        }

        /// <summary>
        ///     Function used to validate a model field property.
        ///     An example of validation is to check that the property is an int if the field is an IntegerField
        /// </summary>
        public virtual void Validate() { }
        public abstract DbType GetDbType();

        /******************** Static methods ********************/
        public static bool IsValidApiName(string name) {
            if (name.IndexOf(':') != -1)
                return false;

            return true;
        }

        public static GroupOperator? GetGroupByOperator(string name) {

            if (name.IndexOf(':') == -1)
                return null;

            return GroupOperator_extension.Get(name.Split(new char[1]{':'}, 2)[1]);
        }
    }
}