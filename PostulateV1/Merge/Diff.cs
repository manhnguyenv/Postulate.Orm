﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Merge
{
    public abstract class Diff
    {
        private readonly MergeObjectType _objectType;
        private readonly MergeActionType _actionType;
        private readonly string _description;

        public Diff(MergeObjectType objectType, MergeActionType actionType, string description)
        {
            _objectType = objectType;
            _actionType = actionType;
            _description = description;
        }

        public MergeObjectType ObjectType { get { return _objectType; } }
        public MergeActionType ActionType { get { return _actionType; } }

        public abstract IEnumerable<string> ValidationErrors();

        public bool IsValid()
        {
            return !ValidationErrors().Any();
        }

        public abstract IEnumerable<string> SqlCommands();
    }
}
