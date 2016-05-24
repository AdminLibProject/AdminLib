using System;
using AdminLib.Model.Model;
using System.Reflection;
using System.Data;

namespace AdminLib.Model.Field
{
    /// <summary>
    ///     A ManyToMany field is used when two tables are linked by a intermediate table.
    /// </summary>
    /// <typeparam name="T">Model linked</typeparam>
    public class ManyToManyField : BaseField, IMultipleValueField {

        /* ManyToMany field create a midModel : a virtual model referencing the midTable.
         * 
         * 
         * 
         * 
         * 
         */

        /******************** Attributes ********************/

        /// <summary>
        ///     DB name of the SELF column in the middle table
        /// </summary>
        public string midColumn          { get; private set; }

        /// <summary>
        ///     Field in the middle table
        /// </summary>
        public ForeignKey midField       { get; private set; }

        public VirtualStructure midModel { get; private set; }

        /// <summary>
        ///     DB name of the referenced column in the middle table
        /// </summary>
        public string midRefColumn       { get; private set; }

        /// <summary>
        ///     Field of the referenced column in the middle table
        /// </summary>
        public ForeignKey midRefField    { get; private set; }

        /// <summary>
        ///     DB name of the middle table
        /// </summary>
        public string midTableName       { get; private set; }

        /// <summary>
        ///     Column referenced in the ref model
        /// </summary>
        public BaseField refColumn       { get; private set; }

        /// <summary>
        ///     Complete API name of the referenced column
        /// </summary>
        public string refColumnName      { get; private set; }

        public AStructure refModel       { get; private set; }

        public Type       refType        { get; private set; }

        public BaseField selfColumn      { get; private set; }

        /// <summary>
        ///     Complete API name of the self based column.
        /// </summary>
        public string selfColumnName     { get; private set; }

        /******************** Constructors ********************/
        /// <summary>
        /// 
        ///     The field will use the intermediate table "midTable" to look for entries in the referenced model.
        ///     A example is better than explanation, this is how the parameters are used
        ///     
        ///         (self is the current model)
        ///     
        ///         SELECT *
        ///           FROM {self}
        ///              , {T}
        ///              , {midTable}
        ///          WHERE {self}.{field}         = {midTable}.{midColumn}     (+)
        ///            AND {T}.{ref_column}       = {midTable}.{midRefColumn}  (+)
        ///            AND {midTable}.{midColumn} = {midTable}.{midRefColumn}  (+)
        /// 
        ///     <example>
        ///     
        ///     Model : Center
        /// 
        ///         Fields :
        ///             - id           : IntegerField(primary_key=true, db_column="CNTR_ID")
        ///             - centerGroups : ManyToMany<CenterGroup>(table:"CENTER_GROUPING")
        /// 
        ///         Table : CENTER
        ///             - CNTR_ID NUMBER
        ///         
        ///     Model : CenterGroup
        ///         Fields :
        ///             - id      : IntegerField(primary_key=true, db_column="CNTP_ID")
        ///             - centers : ManyToMany<CenterGroup>(table:"CENTER_GROUPING")
        /// 
        ///         Table : CENTER_GROUP
        ///             - CNTP_ID NUMBER
        ///             
        ///     Intermediate table : 
        ///         Table : CENTER_GROUPING
        ///             - CNTG_CNTP_ID NUMBER
        ///             - CNTG_CNTR_ID NUMBER
        /// 
        ///     </example>
        /// 
        /// </summary>
        /// <param name="field">Default : the primary key field. Complete API name of the column in the self model</param>
        /// <param name="midTable">DB name of the middle table</param>
        /// <param name="midColumn">Default : dbColumn. DB name of the column in the middle table</param>
        /// <param name="midRefColumn">Default: refColumn. Complete API name of the referenced column</param>
        /// <param name="refColumn">Default: Primary key of the T model</param>
        public ManyToManyField ( string midTable
                               , string apiName        = null
                               , string apiGroup       = null
                               , string field          = null
                               , string midColumn      = null
                               , string midRefColumn   = null
                               , bool?  nullable       = null
                               , bool   primaryKey     = false
                               , string refColumn      = null
                               , bool?  unique         = null)
        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , dbColumn     : null
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {
            this.refColumnName  = refColumn;
            this.midTableName   = midTable;
            this.midRefColumn   = midRefColumn;
            this.midColumn      = midColumn;
            this.selfColumnName = field;
        }

        /******************** Methods ********************/

        public override object FromDbValue(object value) {
            return value;
        }

        public override DbType GetDbType() {
            return DbType.Int32;
        }

        public BaseField GetRefField() {
            return this.refColumn;
        }

        public AStructure GetRefModel() {
            return this.refModel;
        }

        public override void Initialize(AStructure model) {

            Type refType;
            base.Initialize(model);

            // Finding referenced model
            if (this.Attribute.MemberType == MemberTypes.Field)
                refType = ((FieldInfo) this.Attribute).FieldType;
            else if (this.Attribute.MemberType == MemberTypes.Property)
                refType = ((PropertyInfo) this.Attribute).PropertyType;
            else
                throw new Exception("The field is neither a C# field nor property");

            // The type of a ManyToMany field must be an array
            if (!refType.IsArray)
                throw new Exception("The ManyToMany field \"" + this + "\" is not an Array");

            this.refType = refType;

            this.refModel = ModelStructure.Get(refType.GetElementType());

            if (this.refModel == null)
                throw new Exception("Invalid model referenced");

            // Property : selfColumn
            if (this.selfColumnName == null) {
                if (this.model.primaryKeys.Length != 1)
                    throw new Exception("Invalid primary keys number");

                this.selfColumn     = this.model.primaryKeys[0];
                this.selfColumnName = this.selfColumn.CompleteName;
            }
            else {
                this.selfColumn = this.model.GetField(this.selfColumnName);

                if (this.selfColumn == null)
                    throw new Exception("\"dbColumn\" not found");
            }

            this.dbColumn       = this.selfColumn.dbColumn;

            // Property : refColumn
            if (this.refColumnName == null) {

                if (this.refModel.primaryKeys.Length != 1)
                    throw new Exception("Invalid primary keys number in the referenced model");

                this.refColumn     = this.refModel.primaryKeys[0];
                this.refColumnName = this.refColumn.CompleteName;
            }
            else {

                this.refColumn = this.refModel.GetField(this.refColumnName);

                if (this.refColumn == null)
                    throw new Exception("\"refColumn\" not found");
            }

            // Property : midColumn
            if (this.midColumn == null)
                this.midColumn = this.dbColumn;

            if (this.midRefColumn == null)
                this.midRefColumn = this.refColumn.dbColumn;

            // Building mid fields

            this.midModel = VirtualStructure.GetOrCreate(this.midTableName);

            this.midField = new ForeignKey ( dbColumn : this.midColumn
                                           , apiName  : VirtualStructure.CalcFieldApiName(this.model.ApiName));

            this.midModel.AddField(this.midField);

            this.midField.Initialize ( model    : this.midModel
                                     , refModel : this.model);

            this.midRefField = new ForeignKey ( dbColumn : this.midRefColumn
                                              , apiName  : VirtualStructure.CalcFieldApiName(this.refModel.ApiName));

            this.midModel.AddField(this.midRefField);

            this.midRefField.Initialize ( model    : this.midModel
                                        , refModel : this.refModel);

            this.midModel.Initialize();
        }
    }
}