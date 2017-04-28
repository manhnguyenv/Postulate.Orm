﻿using Postulate.Abstract;
using Postulate.Attributes;
using Postulate.Extensions;
using Postulate.Merge.Action;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Merge
{
    public partial class SchemaMerge<TDb>
    {
        /// <summary>
        /// Creates tables and columns that exist in the model but not in the schema
        /// </summary>
        private IEnumerable<MergeAction> CreateTablesAndColumns(IDbConnection connection)
        {
            /* I do tables and columns together because I need to know what tables are created so I don't mistake
               the columns in those tables as standalone additions. Previously, I used an instance variable
               to record created tables, but I would like to avoid that going forward */

            List<MergeAction> results = new List<MergeAction>();

            var schemaTables = GetSchemaTables(connection);
            var addTables = _modelTypes.Where(t => !schemaTables.Any(st => st.Equals(t)));
            results.AddRange(addTables.Select(t => new CreateTable(t)));

            var schemaColumns = GetSchemaColumns(connection);            
            var addColumns = _modelTypes.Where(t => !results.OfType<CreateTable>().Any(ct => ct.Equals(t)))
                .SelectMany(t => t.GetProperties()
                    .Where(pi =>
                        !schemaColumns.Any(cr => cr.Equals(pi)) &&
                        !connection.ColumnExists(t.GetSchema(), t.GetTableName(), pi.SqlColumnName()) &&
                        pi.CanWrite &&
                        IsSupportedType(pi.PropertyType) &&                        
                        !pi.Name.ToLower().Equals(nameof(Record<int>.Id).ToLower()) &&
                        !pi.HasAttribute<NotMappedAttribute>()));

            // empty tables may be dropped and rebuilt with new columns
            var rebuildTables = addColumns
                .GroupBy(pi => pi.GetDbObject(connection))
                .Where(obj => connection.IsTableEmpty(obj.Key.Schema, obj.Key.Name));
            foreach (var tbl in rebuildTables)
            {
                results.Add(new DropTable(tbl.Key, $"Empty table being re-created to add new columns: {string.Join(", ", tbl.Select(pi => pi.SqlColumnName()))}"));
                results.Add(new CreateTable(tbl.Key.ModelType));
            }

            // tables with data may only have columns added
            var alterColumns = addColumns
                .GroupBy(pi => pi.GetDbObject(connection))
                .Where(obj => !connection.IsTableEmpty(obj.Key.Schema, obj.Key.Name))
                .SelectMany(tbl => tbl);                       

            // adding primary key columns requires containing PKs to be rebuilt
            var pkColumns = alterColumns.Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());
            var pkTables = pkColumns.GroupBy(item => DbObject.FromType(item.DeclaringType));
            foreach (var pk in pkTables)
            {
                results.Add(new DropPrimaryKey(pk.Key));
            }

            results.AddRange(alterColumns.Select(pi => new AddColumn(pi)));

            foreach (var pk in pkTables)
            {
                results.Add(new CreatePrimaryKey(pk.Key));
            }

            var foreignKeys = _modelTypes.SelectMany(t => t.GetModelForeignKeys().Where(pi => !connection.ForeignKeyExists(pi)));
            results.AddRange(foreignKeys.Select(pi => new CreateForeignKey(pi)));

            return results;
        }        
    }    
}
