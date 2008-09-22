// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.02

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// Base class for one-to-one tuple transformations.
  /// </summary>
  [Serializable]
  public abstract class WrappingTransformTupleBase: TransformedTuple
  {
    private readonly Tuple origin;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      [DebuggerStepThrough]
      get { return origin.Descriptor; }
    }

    /// <inheritdoc />
    public override int Count {
      [DebuggerStepThrough]
      get { return origin.Count; }
    }

    /// <inheritdoc/>
    public override object[] Arguments {
      [DebuggerStepThrough]
      get { return new object[] {origin}; }
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return origin.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      return origin.GetValueOrDefault<T>(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return origin.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc />
    public override void SetValue<T>(int fieldIndex, T fieldValue)
    {
      origin.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      origin.SetValue(fieldIndex, fieldValue);
    }

    #endregion

    /// <inheritdoc/>
    public sealed override bool Equals(Tuple other)
    {
      return origin.Equals(other);
    }

    /// <inheritdoc/>
    public sealed override int GetHashCode()
    {
      return origin.GetHashCode();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="tuple">Tuple to provide the wrapper for.</param>
    protected WrappingTransformTupleBase(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      origin = tuple;
    }
  }
}