﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Merge.Action
{
    public abstract class MergeAction
    {
        private readonly MergeObjectType _objectType;
        private readonly MergeActionType _actionType;
        private readonly string _description;

        public MergeAction(MergeObjectType objectType, MergeActionType actionType, string description)
        {
            _objectType = objectType;
            _actionType = actionType;
            _description = description;
        }

        public MergeObjectType ObjectType { get { return _objectType; } }
        public MergeActionType ActionType { get { return _actionType; } }
        public string Description { get { return _description; } }

        public abstract IEnumerable<string> ValidationErrors(IDbConnection connection);

        public bool IsValid(IDbConnection connection)
        {
            return !ValidationErrors(connection).Any();
        }

        public abstract IEnumerable<string> SqlCommands(IDbConnection connection);
    }
}