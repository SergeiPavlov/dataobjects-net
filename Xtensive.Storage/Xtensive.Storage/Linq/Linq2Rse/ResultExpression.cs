// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Linq.Expressions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  public sealed class ResultExpression : Expression
  {
    public RecordSet RecordSet { get; private set; }
    // TODO: => IsSingleResult
    public bool IsMultipleResults { get; private set; }
    public Func<RecordSet, object> Shaper { get; private set; }

    public int GetColumnIndex (ColumnInfo columnInfo)
    {
      throw new NotImplementedException();
    }


    // Constructors

    public ResultExpression(Type type, RecordSet recordSet, Func<RecordSet,object> shaper, bool isMultiple)
      : base(ExpressionType.Constant, type)
    {
      RecordSet = recordSet;
      Shaper = shaper;
      IsMultipleResults = isMultiple;
    }
  }
}