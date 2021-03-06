// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public abstract class KeyColumnRef<TParent>: ColumnInfoRef<TParent>
    where TParent: IndexInfo
  {
    private Direction direction;

    /// <summary>
    /// Gets or sets the column direction.
    /// </summary>
    [Property(Priority = -1000)]
    public Direction Direction {
      get { return direction; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Direction", value)) {
          direction = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Invalid <see cref="Direction"/> value 
    /// (<see cref="Xtensive.Core.Direction.None"/>).</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (direction==Direction.None) {
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidDirectionValue, Path);
          });
        }
        ea.Complete();
      }
    }


    // Constructors

    /// <inheritdoc/>
    public KeyColumnRef(TParent parent)
      : base(parent)
    {
    }

    public KeyColumnRef(TParent parent, ColumnInfo column)
      : base(parent, column)
    {
      Direction = Direction.Positive;
    }

    public KeyColumnRef(TParent parent, ColumnInfo column, Direction direction)
      : base(parent, column)
    {
      Direction = direction;
    }
  }
}