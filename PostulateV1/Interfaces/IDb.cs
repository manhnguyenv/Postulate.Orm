﻿using System.Data;

namespace Postulate.Orm.Interfaces
{
    /// <summary>
    /// Enables SchemaMerge to work without SqlDb&lt;Key&gt;
    /// </summary>
    public interface IDb
    {
        int Version { get; }

        IDbConnection GetConnection();

        string ConnectionName { get; }

        string MergeExcludeSchemas { get; }
        string MergeExcludeTables { get; }
    }
}