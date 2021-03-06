﻿using Postulate.Orm.Attributes;
using Postulate.Orm.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Orm.Merge.Action
{
    public class MergeForeignKeyBase : MergeAction
    {
        protected readonly PropertyInfo _pi;
        protected readonly string _description;

        public MergeForeignKeyBase(PropertyInfo propertyInfo, MergeActionType actionType, string description, string sourceAction) : base(MergeObjectType.ForeignKey, actionType, description, sourceAction)
        {
            _pi = propertyInfo;
            _description = description;
        }

        public override IEnumerable<string> SqlCommands(IDbConnection connection)
        {
            foreach (var cmd in base.SqlCommands(connection)) yield return cmd;

            ForeignKeyAttribute fk = _pi.GetForeignKeyAttribute();
            string cascadeDelete = (fk.CascadeDelete) ? " ON DELETE CASCADE" : string.Empty;
            yield return
                $"ALTER TABLE {DbObject.SqlServerName(_pi.DeclaringType)} ADD CONSTRAINT [{_pi.ForeignKeyName()}] FOREIGN KEY (\r\n" +
                    $"\t[{_pi.SqlColumnName()}]\r\n" +
                $") REFERENCES {DbObject.SqlServerName(fk.PrimaryTableType)} (\r\n" +
                    $"\t[{fk.PrimaryTableType.IdentityColumnName()}]\r\n" +
                ")" + cascadeDelete;

            if (fk.CreateIndex && !connection.Exists("[sys].[indexes] WHERE [name]=@name", new { name = _pi.IndexName() }))
            {
                var obj = DbObject.FromType(_pi.DeclaringType);
                yield return $"CREATE INDEX [{_pi.IndexName()}] ON {obj} ([{_pi.SqlColumnName()}])";
            }
        }

        public override IEnumerable<string> ValidationErrors(IDbConnection connection)
        {
            return new string[] { };
        }

        public override string ToString()
        {
            return _description;
        }
    }
}