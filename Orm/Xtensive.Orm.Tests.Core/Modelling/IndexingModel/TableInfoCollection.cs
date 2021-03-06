// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="TableInfo"/> instances.
  /// </summary>
  [Serializable]
  public sealed class TableInfoCollection: NodeCollectionBase<TableInfo, StorageInfo>,
    IUnorderedNodeCollection
  {
    // Constructors

    public TableInfoCollection(StorageInfo storage)
      : base(storage, "Tables")
    {
    }
  }
}