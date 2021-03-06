﻿using Postulate.Orm.Attributes;
using Postulate.Orm.Extensions;
using ReflectionHelper;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Orm.Merge.Action
{
    public class AddColumn : MergeAction
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly DbObject _object;

        public AddColumn(PropertyInfo propertyInfo) : base(MergeObjectType.Column, MergeActionType.Create, $"Add column {propertyInfo.DeclaringType.Name}.{propertyInfo.Name}", nameof(AddColumn))
        {
            _propertyInfo = propertyInfo;
            _object = DbObject.FromType(_propertyInfo.ReflectedType);
        }

        public override IEnumerable<string> SqlCommands(IDbConnection connection)
        {
            foreach (var cmd in base.SqlCommands(connection)) yield return cmd;

            DefaultExpressionAttribute def = _propertyInfo.GetAttribute<DefaultExpressionAttribute>();
            if (def?.IsConstant ?? true)
            {
                yield return $"ALTER TABLE [{_object.Schema}].[{_object.Name}] ADD {_propertyInfo.SqlColumnSyntax()}";
            }
            else
            {
                yield return $"ALTER TABLE [{_object.Schema}].[{_object.Name}] ADD {_propertyInfo.SqlColumnSyntax(forceNull: true)}";

                yield return $"UPDATE [{_object.Schema}].[{_object.Name}] SET [{_propertyInfo.SqlColumnName()}]={def.Expression}";

                yield return $"ALTER TABLE [{_object.Schema}].[{_object.Name}] ALTER COLUMN {_propertyInfo.SqlColumnSyntax()}";
            }
        }

        public override IEnumerable<string> ValidationErrors(IDbConnection connection)
        {
            if (
                connection.TableExists(_object.Schema, _object.Name) &&
                !connection.IsTableEmpty(_object.Schema, _object.Name) &&
                !_propertyInfo.AllowSqlNull() &&
                !_propertyInfo.HasAttribute<DefaultExpressionAttribute>())
            {
                yield return "Adding a non-nullable column to a table with data requires a [DefaultExpression] attribute on the column.";
            }
        }

        public override string ToString()
        {
            return $"{_propertyInfo.ReflectedType.Name}.{_propertyInfo.Name}";
        }
    }
}