using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Highway.Data.EntityFramework;

namespace Highway.Data
{
    public class Merge<T> : AdvancedCommand
    {
        private readonly T _item;
        private readonly Func<T, object> _uniqueMapping;
        private string baseSqlStatement = @" 
MERGE %%SCHEMA%%.%%TABLE%% WITH (HOLDLOCK) AS t
USING (SELECT %%UNIQUECOLUMNS%%) AS new_t
      ON %%UNIQUE%%
WHEN MATCHED THEN
    UPDATE
            SET %%UPDATE%%
WHEN NOT MATCHED THEN
    INSERT
      (
            %%COLUMNS%%
      )
    VALUES
      (
            %%VALUES%%
      );
       ";
        private string _sqlStatement;

        public Merge(T item, Func<T, object> uniqueMapping)
        {
            _item = item;
            _uniqueMapping = uniqueMapping;
            ContextQuery = c =>
            {
                var sqlMerge = GetSqlStatement(c, item, uniqueMapping);
                c.ExecuteSqlCommand(sqlMerge.Statement, sqlMerge.Parameters.ToArray());
            };
        }

        private SqlMerge GetSqlStatement(IDataContext context, T item, Func<T, object> uniqueMapping)
        {
            var mappingDetail = context.GetMappingFor<T>();
            var sqlMerge = new SqlMerge();
            var uniqueObject = uniqueMapping(item);
            var constraintProperties = uniqueObject.GetType().GetProperties();
            var uniqueColumns = new List<string>();
            var constraints = new List<string>();
            var columnValues = new List<ColumnValueParameterTuple>();
            foreach (var propertyInfo in constraintProperties)
            {
                var propertyDetail = mappingDetail.Properties.Single(x => x.Property.Name == propertyInfo.Name);
                object propertyValue = item.GetPropertyValue(propertyInfo.Name);
                var columnValue = GetColumnValue(columnValues, propertyDetail, propertyValue);
                uniqueColumns.Add(string.Format("{0} as {1}", columnValue.ParameterName, propertyDetail.ColumnName));
                constraints.Add(string.Format("t.{0} = new_t.{0}", propertyDetail.ColumnName));
            }
            var sql = baseSqlStatement.Replace("%%SCHEMA%%", mappingDetail.Schema);
            sql = sql.Replace("%%TABLE%%", mappingDetail.Table);
            sql = sql.Replace("%%UNIQUECOLUMNS%%", string.Join(",", uniqueColumns));
            sql = sql.Replace("%%UNIQUE%%", string.Join(" and ", constraints));
            
            var updateStatements = new List<string>();
            
            foreach (var propertyDetail in mappingDetail.Properties)
            {
                if (!propertyDetail.IsGenerated)
                {
                    object propertyValue = item.GetPropertyValue(propertyDetail.Property.Name);
                    var columnValue = GetColumnValue(columnValues, propertyDetail, propertyValue);
                    updateStatements.Add(string.Format("{0} = {1}", propertyDetail.ColumnName, columnValue.ParameterName));
                    
                }
            }

            sql = sql.Replace("%%UPDATE%%", string.Join(",", updateStatements));
            var columnsToInsert = columnValues.Where(x => !x.Excluded);
            sql = sql.Replace("%%COLUMNS%%", string.Join(",", columnsToInsert.Select(x => x.ColumnName)));
            sql = sql.Replace("%%VALUES%%", string.Join(",", columnsToInsert.Select(x => x.ParameterName)));
            sqlMerge.Statement = sql;
            sqlMerge.Parameters = columnValues.Select(x => new SqlParameter(x.ParameterName, x.PropertyValue));
            return sqlMerge;
        }

        private static ColumnValueParameterTuple GetColumnValue(List<ColumnValueParameterTuple> columnValues, PropertyDetail propertyDetail, object propertyValue)
        {
            var columnValue = columnValues.SingleOrDefault(x => x.ColumnName == propertyDetail.ColumnName);
            if (columnValue == null)
            {
                var paramName = string.Format("@p_{0}", columnValues.Count() + 1);
                columnValue = new ColumnValueParameterTuple(propertyDetail.ColumnName, propertyValue, paramName,
                    propertyDetail.IsGenerated);
                columnValues.Add(columnValue);
            }
            return columnValue;
        }

        public string OutputQuery(IDataContext context)
        {
            return GetSqlStatement(context, _item, _uniqueMapping).ToString();
        }
    }

    internal class SqlMerge
    {
        public SqlMerge()
        {
            Parameters = new List<DbParameter>();
        }
        public string Statement { get; set; }
        public IEnumerable<DbParameter> Parameters { get; set; }

        public override string ToString()
        {
            return string.Format("Statement: {0}, Parameters: {1}", Statement, String.Join(",", Parameters.Select(x => string.Format("{0}:{1}", x.ParameterName, x.Value))));
        }
    }

    internal class ColumnValueParameterTuple
    {
        public ColumnValueParameterTuple(string columnName, object propertyValue, string paramName, bool excluded = false)
        {
            ColumnName = columnName;
            PropertyValue = propertyValue;
            ParameterName = paramName;
            Excluded = excluded;
        }

        public string ColumnName { get; private set; }
        public object PropertyValue { get; private set; }
        public string ParameterName { get; private set; }
        public bool IsNull { get; private set; }
        public bool Excluded { get; set; }
    }
}