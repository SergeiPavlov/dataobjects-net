// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class CheckConstraintSqlComparer : SqlComparerBase<CheckConstraint>
  {
    public override IComparisonResult<CheckConstraint> Compare(CheckConstraint originalNode, CheckConstraint newNode, IEnumerable<ComparisonHintBase> hints)
    {
      return new CheckConstraintComparisonResult(originalNode, newNode);

    }

    public CheckConstraintSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}