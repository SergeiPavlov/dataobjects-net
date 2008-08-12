// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// Base class for any transformed tuples.
  /// </summary>
  [Serializable]
  public abstract class TransformedTuple : Tuple
  {
    /// <summary>
    /// Gets the transform used to produce this instance.
    /// </summary>
    public abstract ITupleTransform Transform { get; }

    /// <summary>
    /// Gets a list of arguments used in <see cref="ITupleTransform.Apply"/> method
    /// to produce this tuple.
    /// <see langword="Null"/> means arguments are unknown an this stage.
    /// </summary>
    public abstract object[] Arguments { get; }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      [DebuggerStepThrough]
      get { return Transform.Descriptor; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.TransformedTupleFormat, 
        base.ToString(), 
        Transform, 
        Arguments.ToCommaDelimitedString());
    }
  }
}