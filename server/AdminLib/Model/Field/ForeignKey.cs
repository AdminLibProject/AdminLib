using System;
using AdminLib.Model.Model;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;

namespace AdminLib.Model.Field {
    public class ForeignKey<T> : Field.Field<T?>, IForeignKey
        where T: struct {

        /******************** Model ********************/
        public BaseField  refField { get; private set; }
        public AStructure refModel { get; private set; }
        public Type       refType  { get; private set; }

        /******************** Constructors ********************/
        public ForeignKey ( string dbColumn
                          , string apiName      = null
                          , string apiGroup     = null
                          , T?[]   choices      = null
                          , T?     defaultValue = null
                          , bool   primaryKey   = false
                          , bool?  nullable     = null
                          , bool?  unique       = null)
        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {
        }

        /******************** Methods ********************/

        public override OracleDbType GetDbType() {

            Type type;

            type = typeof(T);

            if (type == typeof(int))
                return IntegerField.dbType;

            else if (type == typeof(string))
                return CharField.dbType;

            throw new NotImplementedException();
        }

        public override object FromDbValue(object value) {
            return value;
        }

        private void DefineRefModel(AStructure refModel) {
            if (refModel == null)
                throw new Exception(this + " : Invalid model referenced");

            // Retreiving the primary key of the referenced model
            if (refModel.primaryKeys.Length != 1)
                throw new Exception(this + " : The model must have one and only one primary key");

            /*if (!(refModel.primaryKeys[0] is IntegerField))
                throw new Exception(this + " : The primary key of the referenced model must be an integer field");*/

            this.refModel  = refModel;
            this.refField = this.refModel.primaryKeys[0];
        }

        public BaseField GetRefField(){
            return this.refField;
        }

        public AStructure GetRefModel() {
            return this.refModel;
        }

        public override object GetValue(object instance, bool dbValue = false) {

            object value;

            if (!dbValue)
                return base.GetValue(instance, dbValue);

            value = base.GetValue(instance, false);

            return this.refField.GetValue(value, true);
        }

        /// <summary>
        ///     Initialize the foreign key.
        ///     The foreign key need to retreive the structure of
        ///     the referenced model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="attribute"></param>
        public override void Initialize(AStructure model) {
            base.Initialize(model);

            Type       refType;
            AStructure refModel;

            if (this.Virtual)
                throw new Exception("Virtual foreign key must be initialized with a ref model");

            // Finding referenced model
            if (this.Attribute.MemberType == MemberTypes.Field)
                refType = ((FieldInfo) this.Attribute).FieldType;
            else if (this.Attribute.MemberType == MemberTypes.Property)
                refType = ((PropertyInfo) this.Attribute).PropertyType;
            else
                throw new Exception("The field is neither a C# field nor property");

            this.refType = refType;

            refModel = ModelStructure.Get(this.refType);
            this.DefineRefModel(refModel);
        }

        public void Initialize(AStructure model, AStructure refModel) {

            // Referenced model should be provided only when the field is virtual
            if (!this.Virtual && refModel != null)
                throw new Exception("Providing the ref model is only valid for virtual fields");
            else if (!this.Virtual)
                this.Initialize(model);
            else if (refModel == null)
                throw new Exception("virtual foreign key must be initialized with a ref model");

            base.Initialize(model);

            this.DefineRefModel(refModel);   
        }

    }
}