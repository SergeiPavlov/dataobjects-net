// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class SequenceComparisonResult : NodeComparisonResult,
    IComparisonResult<Sequence>
  {
    /// <inheritdoc/>
    public Sequence NewValue
    {
      get { return (Sequence) base.NewValue; }
    }

    /// <inheritdoc/>
    public Sequence OriginalValue
    {
      get { return (Sequence) base.OriginalValue; }
    }

    public SequenceComparisonResult(Sequence originalValue, Sequence newValue)
      : base(originalValue, newValue)
    {
    }
  }
}