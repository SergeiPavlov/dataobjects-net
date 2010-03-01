// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Describes a set of key-version pairs used to validate versions.
  /// </summary>
  [Serializable]
  public sealed class VersionSet : ICountable<KeyValuePair<Key, VersionInfo>>
  {
    private Dictionary<Ref<Entity>, VersionInfo> versions = 
      new Dictionary<Ref<Entity>, VersionInfo>();

    /// <inheritdoc/>
    public long Count {
      get { return versions.Count; }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.VersionInfo"/> by the specified key.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    public VersionInfo this[Key key] {
      get {
        return Get(key);
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.VersionInfo"/> for the specified 
    /// <paramref name="entity"/>.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    /// <param name="entity">The entity to get associated <see cref="VersionInfo"/> for.</param>
    /// <returns>Associated <see cref="VersionInfo"/>, if found;
    /// otherwise, <see cref="VersionInfo.Void"/>.</returns>
    public VersionInfo Get(Entity entity)
    {
      return Get(entity!=null ? entity.Key : null);
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.VersionInfo"/> by the specified key.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    /// <param name="key">The key to get associated <see cref="VersionInfo"/> for.</param>
    /// <returns>Associated <see cref="VersionInfo"/>, if found;
    /// otherwise, <see cref="VersionInfo.Void"/>.</returns>
    public VersionInfo Get(Key key)
    {
      VersionInfo result;
      if (versions.TryGetValue(key, out result))
        return result;
      else
        return VersionInfo.Void;
    }

    /// <summary>
    /// Determines whether this set contains the key of the specified entity.
    /// </summary>
    /// <param name="entity">The entity to check the key for containment.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Entity entity)
    {
      return Contains(entity!=null ? entity.Key : null);
    }

    /// <summary>
    /// Determines whether this set contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for containment.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      if (key==null)
        return false;
      return versions.ContainsKey(key);
    }

    #region Validate methods

    /// <summary>
    /// Validates version of the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to validate version for.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Entity entity)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      return Validate(entity.Key, entity.VersionInfo);
    }

    /// <summary>
    /// Validates version of the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to validate version for.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Entity entity, bool throwOnFailure)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      return Validate(entity.Key, entity.VersionInfo, throwOnFailure);
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Key key, VersionInfo version)
    {
      var expectedVersion = Get(key);
      if (expectedVersion.IsVoid)
        return true;
      else
        return expectedVersion==version;
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <param name="throwOnFailure">Indicates whether <see cref="InvalidOperationException"/>
    /// must be thrown on validation failure.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Version conflict is detected.</exception>
    public bool Validate(Key key, VersionInfo version, bool throwOnFailure)
    {
      var result = Validate(key, version);
      if (throwOnFailure && !result)
        throw new InvalidOperationException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      return result;
    }

    #endregion

    #region Add, Remove, Clear methods

    /// <summary>
    /// Adds key and <see cref="VersionInfo"/> pair
    /// of the specified <paramref name="entity"/> to this set.
    /// </summary>
    /// <param name="entity">The entity to add version of.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing
    /// key-version pair or not, if it exists.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Add(Entity entity, bool overwrite)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      var key = entity.Key;
      if (!Contains(key)) {
        versions.Add(key, entity.VersionInfo);
        return true;
      }
      else if (overwrite) {
        versions[key] = entity.VersionInfo;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Adds the specified key and <see cref="VersionInfo"/> pair to this set.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The version related to this key.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing
    /// key-version pair or not, if it exists.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Add(Key key, VersionInfo value, bool overwrite)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (!Contains(key)) {
        versions.Add(key, value);
        return true;
      }
      else if (overwrite) {
        versions[key] = value;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Removed the key and <see cref="VersionInfo"/> pair 
    /// of the specified <paramref name="entity"/> from this set.
    /// </summary>
    /// <param name="entity">The entity to remove the key-version pair of.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Remove(Entity entity)
    {
      return Remove(entity.Key);
    }

    /// <summary>
    /// Removed the specified key and <see cref="VersionInfo"/> pair from this set.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Remove(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (Contains(key)) {
        versions.Remove(key);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Clears this set.
    /// </summary>
    public void Clear()
    {
      versions.Clear();
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Key, VersionInfo>> GetEnumerator()
    {
      foreach (var pair in versions)
        yield return new KeyValuePair<Key, VersionInfo>(pair.Key, pair.Value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public VersionSet()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(params Entity[] source)
    {
      foreach (var entity in source)
        Add(entity, true);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(IEnumerable<Entity> source)
    {
      foreach (var entity in source)
        Add(entity, true);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(IEnumerable<KeyValuePair<Key, VersionInfo>> source)
    {
      foreach (var pair in source)
        Add(pair.Key, pair.Value, true);
    }
  }
}