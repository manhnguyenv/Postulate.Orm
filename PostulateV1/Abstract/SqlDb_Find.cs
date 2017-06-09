﻿using Postulate.Orm.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Orm.Abstract
{
    public abstract partial class SqlDb<TKey> : IDb
    {
        public TRecord Find<TRecord>(IDbConnection connection, TKey id) where TRecord : Record<TKey>
        {
            var row = ExecuteFind<TRecord>(connection, id);
            return FindInner<TRecord>(connection, row);
        }

        public TRecord Find<TRecord>(TKey id) where TRecord : Record<TKey>
        {
            using (IDbConnection cn = GetConnection())
            {
                cn.Open();
                return Find<TRecord>(cn, id);
            }
        }

        public TRecord FindWhere<TRecord>(IDbConnection connection, string critieria, object parameters) where TRecord : Record<TKey>
        {
            var row = ExecuteFindWhere<TRecord>(connection, critieria, parameters);
            return FindInner(connection, row);
        }

        public TRecord FindUserProfile<TRecord>() where TRecord : Record<TKey>, IUserProfile
        {
            using (IDbConnection cn = GetConnection())
            {
                cn.Open();
                return FindUserProfile<TRecord>(cn);
            }
        }

        public TRecord FindUserProfile<TRecord>(IDbConnection connection) where TRecord : Record<TKey>, IUserProfile
        {
            return ExecuteFindWhere<TRecord>(connection, "[UserName]=@name", new { name = UserName });
        }

        public TRecord FindWhere<TRecord>(string criteria, object parameters) where TRecord : Record<TKey>
        {
            using (IDbConnection cn = GetConnection())
            {
                cn.Open();
                return FindWhere<TRecord>(cn, criteria, parameters);
            }
        }

        public bool ExistsWhere<TRecord>(string criteria, object parameters) where TRecord : Record<TKey>
        {
            using (IDbConnection cn = GetConnection())
            {
                cn.Open();
                return ExistsWhere<TRecord>(cn, criteria, parameters);
            }
        }

        public bool ExistsWhere<TRecord>(IDbConnection connection, string criteria, object parameters) where TRecord : Record<TKey>
        {
            TRecord record = FindWhere<TRecord>(connection, criteria, parameters);
            return (record != null);
        }
    }
}