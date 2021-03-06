﻿using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Postulate.Orm.Merge.Action
{
    public abstract class MergeAction
    {
        private readonly MergeObjectType _objectType;
        private readonly MergeActionType _actionType;
        private readonly string _description;
        private readonly string _sourceAction;

        public MergeAction(MergeObjectType objectType, MergeActionType actionType, string description, string sourceAction)
        {
            _objectType = objectType;
            _actionType = actionType;
            _description = description;
            _sourceAction = sourceAction;
        }

        public MergeObjectType ObjectType { get { return _objectType; } }
        public MergeActionType ActionType { get { return _actionType; } }
        public string Description { get { return _description; } }
        public string SourceAction { get { return _sourceAction; } }

        public abstract IEnumerable<string> ValidationErrors(IDbConnection connection);

        public bool IsValid(IDbConnection connection)
        {
            return !ValidationErrors(connection).Any();
        }

        public virtual IEnumerable<string> SqlCommands(IDbConnection connection)
        {
            yield return $"-- {Description} ({SourceAction})";
        }

        public override string ToString()
        {
            return Description;
        }
    }
}