// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public class KeyColumnRef: ColumnInfoRef
  {
    private Direction direction;

    /// <summary>
    /// Gets or sets the column direction.
    /// </summary>
    [Property]
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
    /// <exception cref="IntegrityException">Invalid <see cref="Direction"/> value 
    /// (<see cref="Core.Direction.None"/>).</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (direction==Direction.None)
          ea.Execute(() => {
            throw new IntegrityException(Strings.ExInvalidDirectionValue, Path);
          });
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<KeyColumnRef, IndexInfo, KeyColumnRefCollection>(this, "KeyColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public KeyColumnRef(IndexInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public KeyColumnRef(IndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
      Direction = Direction.Positive;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="direction">The direction.</param>
    public KeyColumnRef(IndexInfo parent, ColumnInfo column, Direction direction)
      : base(parent, column)
    {
      Direction = direction;
    }
  }
}