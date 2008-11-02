// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=Xtensive.Storage.Internals.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// The underlying data of the <see cref="Storage.Entity"/>.
  /// </summary>
  public sealed class EntityState : Tuple
  {
    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; internal set; }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return Key.Type; }
    }

    /// <summary>
    /// Gets the values as <see cref="DifferentialTuple"/>.
    /// </summary>
    public DifferentialTuple Data { get; private set; }

    /// <summary>
    /// Gets the transaction the state belongs to.
    /// </summary>
    public Transaction Transaction { get; private set; }
    
    /// <summary>
    /// Gets the the persistence state.
    /// </summary>
    public PersistenceState PersistenceState { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Entity"/> associated with this state.
    /// </summary>
    public Entity Entity { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    public bool IsRemoved {
      get { return Data==null; }
    }

    /// <summary>
    /// Determines whether the field with the specified offset is fetched.
    /// </summary>
    /// <param name="offset">The offset of the field.</param>
    public bool IsFetched(int offset)
    {
      return PersistenceState==PersistenceState.New || IsAvailable(offset);
    }

    /// <summary>
    /// Updates the entity state to the most current one.
    /// </summary>
    /// <param name="tuple">The state change tuple, or a new state tuple. 
    /// If <see langword="null" />, the entity is considered as removed.</param>
    /// <param name="transaction">The transaction.</param>
    public void Update(Tuple tuple, Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      if (Transaction.State.IsActive()) {
        if (tuple==null)
          Data = null;
        else
          Data.Origin.MergeWith(tuple);
      }
      else {
        Transaction = transaction;
        Data = tuple==null ? null : new DifferentialTuple(tuple.ToRegular());
      }
    }

    #region EnsureXxx methods

    /// <summary>
    /// Ensures the data belongs to the current <see cref="Transaction"/> and resents the data if not.
    /// </summary>
    public void EnsureIsActual()
    {
      if (!Transaction.State.IsActive())
        Fetcher.Fetch(Key);
    }

    /// <summary>
    /// Ensures the <see cref="Entity"/> property has been set,
    /// i.e. <see cref="Xtensive.Storage.Entity"/> is associated
    /// with this state.
    /// </summary>
    public void EnsureHasEntity()
    {
      if (Entity!=null)
        return;
      var entity = Activator.CreateEntity(Type.UnderlyingType, this);
      entity.Initialize();
      Entity = entity;
    }

    /// <summary>
    /// Ensures the entity is not removed and its data is actual.
    /// Call this method before getting or setting values.
    /// </summary>
    public void EnsureNotRemoved()
    {
      EnsureIsActual();
      if (IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    #endregion

    #region Tuple implementation

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return Data.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return Data.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      Data.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return Data.Descriptor; }
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Key = '{0}', Values = {1}, State = {2}", Key, Data.ToRegular(), PersistenceState);
    }


    // Constructors

    internal EntityState(Key key, DifferentialTuple tuple, Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      Key = key;
      Data = tuple;
      Transaction = transaction;
    }
  }
}