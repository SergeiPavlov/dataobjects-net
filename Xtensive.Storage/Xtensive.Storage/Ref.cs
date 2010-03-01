// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Typed reference to <see cref="Entity"/>.
  /// </summary>
  /// <typeparam name="T">The type of referenced object (<see cref="Value"/> property).</typeparam>
  [Serializable]
  [DebuggerDisplay("Key = {Key}")]
  public struct Ref<T> : 
    IEquatable<Ref<T>>,
    IEquatable<Key>,
    ISerializable
    where T : class, IEntity
  {
    private readonly Key key;

    /// <summary>
    /// Gets the key of the referenced entity.
    /// </summary>
    public Key Key {
      get { return key; }
    }

    /// <summary>
    /// Gets the referenced entity (resolves the reference).
    /// </summary>
    public T Value {
      get { return Query.Single<T>(key); }
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(Ref<T> other)
    {
      return other.Key==Key;
    }

    /// <inheritdoc/>
    public bool Equals(Key other)
    {
      return other==Key;
    }

    /// <inheritdoc/>
    public override bool Equals(object other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (other.GetType()==typeof (Ref<T>))
        return Equals((Ref<T>) other);
      var otherKey = other as Key;
      if (otherKey!=null)
        return Equals(otherKey);
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (Key!=null ? Key.GetHashCode() : 0);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.RefFormat,
        typeof (T).GetShortName(),
        key==null ? Strings.Null : key.ToString());
    }

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="Key"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="key">Key of the entity to provide typed reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Ref<T>(Key key)
    {
      return new Ref<T>(key);
    }

    /// <summary>
    /// Implicit conversion of <see cref="IEntity"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="entity">The entity to provide typed reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Ref<T>(T entity)
    {
      return new Ref<T>(entity);
    }

    /// <summary>
    /// Implicit conversion of <see cref="Ref{T}"/> to <see cref="Key"/>.
    /// </summary>
    /// <param name="reference">The typed reference to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Key(Ref<T> reference)
    {
      return reference.Key;
    }

    /// <summary>
    /// Implicit conversion of <see cref="Ref{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reference">The typed reference to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator T(Ref<T> reference)
    {
      return reference.Value;
    }

    #endregion


    // Constructors

    private Ref(Key key)
    {
      this.key = key;
    }

    private Ref(T entity)
    {
      if (entity==null)
        key = null;
      else
        key = entity.Key;
    }

    #region ISerializable members

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    private Ref(SerializationInfo info, StreamingContext context)
    {
      key = Key.Parse(info.GetString("Key"));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Key", key.Format());
    }

    #endregion
  }
}