// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.19

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Internals
{
  /// <summary>
  /// Used to describe tuples, which length is longer then
  /// <see cref="MaxGeneratedTupleLength.Value"/>.
  /// </summary>
  [DataContract]
  [Serializable]
  public sealed class JoinedTuple : RegularTuple
  {
    /// <summary>
    /// Always equals to <see cref="MaxGeneratedTupleLength.Value"/>.
    /// </summary>
    public const int FirstCount = MaxGeneratedTupleLength.Value;

    /// <summary>
    /// The first tuple.
    /// </summary>
    [DataMember]
    public readonly RegularTuple First;

    /// <summary>
    /// The second tuple.
    /// </summary>
    [DataMember]
    public readonly RegularTuple Second;

    /// <inheritdoc/>
    public override int Count
    {
      get { return descriptor.Count; }
    }

    /// <inheritdoc/>
    public override Tuple CreateNew()
    {
      return new JoinedTuple(descriptor, (RegularTuple) First.CreateNew(), (RegularTuple) Second.CreateNew());
    }

    /// <inheritdoc/>
    public override Tuple Clone()
    {
      return new JoinedTuple(this);
    }

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return fieldIndex < FirstCount 
        ? First.GetFieldState(fieldIndex) 
        : Second.GetFieldState(fieldIndex - FirstCount);
    }

    /// <inheritdoc/>
    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      if (fieldIndex < FirstCount)
        First.SetFieldState(fieldIndex, fieldState);
      else
        Second.SetFieldState(fieldIndex - FirstCount, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      return fieldIndex < FirstCount
        ? First.GetValue(fieldIndex, out fieldState)
        : Second.GetValue(fieldIndex - FirstCount, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (fieldIndex < FirstCount)
        First.SetValue(fieldIndex, fieldValue);
      else
        Second.SetValue(fieldIndex - FirstCount, fieldValue);
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(First.Descriptor.Concat(Second.Descriptor));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="first">The first tuple.</param>
    /// <param name="second">The second tuple.</param>
    public JoinedTuple(TupleDescriptor descriptor, RegularTuple first, RegularTuple second)
      : base(descriptor)
    {
      First = first;
      Second = second;
    }

    private JoinedTuple(JoinedTuple template)
      : base(template.descriptor)
    {
      First = template.First;
      Second = template.Second;
    }
  }
}