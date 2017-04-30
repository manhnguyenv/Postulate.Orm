﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Attributes
{
    /// <summary>
    /// Defines a WHERE clause expression that is appended to a query
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WhereAttribute : Attribute
    {
        private readonly string _expression;

        public WhereAttribute(string expression)
        {
            _expression = expression;
        }

        public string Expression {  get { return _expression; } }
    }
}