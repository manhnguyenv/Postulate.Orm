﻿using Postulate.Orm.Abstract;
using Postulate.Orm.Attributes;
using Postulate.Orm.Extensions;
using Postulate.Orm.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Orm.ModelMerge.Actions
{
	public class AddColumn : MergeAction
	{
		private readonly PropertyInfo _propertyInfo;
		private readonly TableInfo _tableInfo;
		private readonly Type _modelType;

		public AddColumn(SqlSyntax scriptGen, PropertyInfo propertyInfo) : base(scriptGen, ObjectType.Column, ActionType.Create, $"{propertyInfo.QualifiedName()}")
		{
			_propertyInfo = propertyInfo;
			_modelType = propertyInfo.ReflectedType;
			_tableInfo = Syntax.GetTableInfoFromType(_modelType);
		}

		public override IEnumerable<string> ValidationErrors(IDbConnection connection)
		{
			var modelType = _propertyInfo.ReflectedType;

			if (
				Syntax.TableExists(connection, modelType) &&
				!Syntax.IsTableEmpty(connection, modelType) &&
				!_propertyInfo.AllowSqlNull() &&
				!_propertyInfo.HasAttribute<DefaultExpressionAttribute>() &&
				!IsIdentityColumn(_propertyInfo))
			{
				yield return "Adding a non-nullable column to a table with data requires a [DefaultExpression] attribute on the column, or it must be the custom identity column for the table.";
			}
		}

		public static bool IsIdentityColumn(PropertyInfo propertyInfo)
		{
			IdentityColumnAttribute attr;
			if (propertyInfo.DeclaringType.HasAttribute(out attr))
			{
				return propertyInfo.Name.Equals(attr.ColumnName);
			}
			return false;
		}

		public override IEnumerable<string> SqlCommands(IDbConnection connection)
		{
			DefaultExpressionAttribute def = _propertyInfo.GetAttribute<DefaultExpressionAttribute>();
			if (def?.IsConstant ?? true)
			{
				yield return Syntax.ColumnAddStatement(_tableInfo, _propertyInfo);

				if (IsIdentityColumn(_propertyInfo))
				{
					yield return Syntax.UniqueKeyStatement(_propertyInfo);
				}
			}
			else
			{
				yield return Syntax.ColumnAddStatement(_tableInfo, _propertyInfo, forceNull: true);
				yield return Syntax.UpdateColumnWithExpressionStatement(_tableInfo, _propertyInfo, def.Expression);
				yield return Syntax.ColumnAlterStatement(_tableInfo, _propertyInfo);
			}
		}
	}
}