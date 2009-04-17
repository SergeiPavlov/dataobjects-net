// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Table.
  /// </summary>
  [Serializable]
  public class TableInfo : NodeBase<StorageInfo>
  {
    private PrimaryIndexInfo primaryIndex;

    /// <summary>
    /// Gets columns.
    /// </summary>
    [Property]
    public ColumnInfoCollection Columns { get; private set; }

    /// <summary>
    /// Gets or sets the primary index.
    /// </summary>
    [Property]
    public PrimaryIndexInfo PrimaryIndex {
      get { return primaryIndex; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("PrimaryIndex", value)) {
          primaryIndex = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets secondary indexes.
    /// </summary>
    [Property]
    public SecondaryIndexInfoCollection SecondaryIndexes { get; private set; }

    /// <summary>
    /// Gets foreign keys.
    /// </summary>
    [Property]
    public ForeignKeyCollection ForeignKeys { get; private set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<TableInfo, StorageInfo, TableInfoCollection>(this, "Tables");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Columns==null)
        Columns = new ColumnInfoCollection(this);
      if (SecondaryIndexes == null)
        SecondaryIndexes = new SecondaryIndexInfoCollection(this);
      if (ForeignKeys == null)
        ForeignKeys = new ForeignKeyCollection(this);
    }


    // Constructors

    /// <inheritdoc/>
    public TableInfo(StorageInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}