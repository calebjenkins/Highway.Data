using System;
using System.Linq.Expressions;

namespace Highway.Data.PrebuiltCommands
{
    public class Merge<T> : AdvancedCommand
    {
        private string baseSqlStatement = @" 
MERGE %%SCHEMA%%.%%TABLE%% WITH (HOLDLOCK) AS t
USING (SELECT @ID AS ID) AS new_t
      ON %%UNIQUE%%
WHEN MATCHED THEN
    UPDATE
            SET f.UpdateSpid = @@SPID,
            UpdateTime = SYSDATETIME()
WHEN NOT MATCHED THEN
    INSERT
      (
            ID,
            InsertSpid,
            InsertTime
      )
    VALUES
      (
            new_foo.ID,
            @@SPID,
            SYSDATETIME()
      );
       ";

        public Merge(T item, Func<T, object> uniqueMapping)
        {
            ContextQuery = c =>
            {
                c.ExecuteSqlCommand(GetSqlStatement(c, item, uniqueMapping));
            };
        }

        private string GetSqlStatement(IDataContext context, T item, Func<T, object> uniqueMapping)
        {
            var mappingDetail = context.GetMappingFor<T>();

        }
    }
}