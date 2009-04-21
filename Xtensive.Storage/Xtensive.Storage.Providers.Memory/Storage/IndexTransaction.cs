// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using System.Transactions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage.Providers.Memory
{
  [Serializable]
  public class IndexTransaction : Index.IndexTransaction
  {
    private TransactionState state;

    /// <inheritdoc/>
    public override void Commit()
    {
      if (state != TransactionState.Active)
        throw new InvalidOperationException();

      state = TransactionState.Committed;
    }

    /// <inheritdoc/>
    public override void Rollback()
    {
      state = TransactionState.RolledBack;
    }

    /// <inheritdoc/>
    public override TransactionState State
    {
      get { return state; }
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public IndexTransaction(Guid identifier, IsolationLevel isolationLevel)
    {
      Identifier = identifier;
      IsolationLevel = isolationLevel;
      state = TransactionState.Active;
    }
  }
}