﻿using System;

namespace Postulate.Orm.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public class DefaultExpressionAttribute : Attribute
	{
		private readonly string _columnName;
		private readonly string _expression;

		public DefaultExpressionAttribute(string expression)
		{
			_expression = expression;
		}

		public DefaultExpressionAttribute(string columnName, string expression)
		{
			_columnName = columnName;
			_expression = expression;
		}

		public string Expression { get { return _expression; } }

		public string ColumnName { get { return _columnName; } }

		public bool IsConstant { get; set; }
	}
}