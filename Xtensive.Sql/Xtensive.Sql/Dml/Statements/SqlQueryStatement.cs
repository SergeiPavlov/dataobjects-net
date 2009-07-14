// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Base class for DML statements.
  /// </summary>
  [Serializable]
  public abstract class SqlQueryStatement: SqlStatement
  {
    private int limit;
    private int offset;
    private IList<SqlHint> hints;

    /// <summary>
    /// Gets or sets the limit.
    /// </summary>
    /// <value>The limit.</value>
    public int Limit
    {
      get { return limit; }
      set {
        if (value<0)
          throw new ArgumentOutOfRangeException("value");
        limit = value;
      }
    }

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <value>The offset.</value>
    public int Offset
    {
      get { return offset; }
      set {
        if (value<0)
          throw new ArgumentOutOfRangeException("value");
        offset = value;
      }
    }

    /// <summary>
    /// Gets the collection of join hints.
    /// </summary>
    /// <value>The collection of join hints.</value>
    public IList<SqlHint> Hints
    {
      get {
        if (hints==null)
          hints = new Collection<SqlHint>();
        return hints;
      }
    }


    // Constructors

    protected SqlQueryStatement(SqlNodeType nodeType) : base(nodeType)
    {
    }
  }
}
