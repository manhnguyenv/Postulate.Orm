﻿using Postulate.Orm.Abstract;
using Postulate.Orm.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Orm.Merge.Actions
{
    public class AlterColumn : MergeAction
    {
        public AlterColumn(SqlSyntax scriptGen, PropertyInfo propertyInfo) : base(scriptGen, ObjectType.Column, ActionType.Alter, $"{propertyInfo.QualifiedName()}")
        {
        }

        public override IEnumerable<string> SqlCommands(IDbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}