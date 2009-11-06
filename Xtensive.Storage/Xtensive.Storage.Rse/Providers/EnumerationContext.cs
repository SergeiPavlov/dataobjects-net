// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// The single enumeration attempt context for the <see cref="ExecutableProvider"/>.
  /// </summary>
  [Serializable]
  public abstract class EnumerationContext: Context<EnumerationScope>
  {
    private const string DefaultName = "Default";
    private readonly Dictionary<Pair<object, string>, object> cache = new Dictionary<Pair<object, string>, object>();

    /// <summary>
    /// Gets or sets the global temporary data.
    /// </summary>
    public abstract GlobalTemporaryData GlobalTemporaryData { get; }

    /// <summary>
    /// Gets or sets the transaction temporary data.
    /// </summary>
    public abstract TransactionTemporaryData TransactionTemporaryData { get; }

    /// <summary>
    /// Gets the current <see cref="EnumerationContext"/>.
    /// </summary>
    public static EnumerationContext Current {
      get { return EnumerationScope.CurrentContext; }
    }

    /// <summary>
    /// Factory method. Creates new <see cref="EnumerationContext"/>.
    /// </summary>
    public abstract EnumerationContext CreateNew();
    
    /// <summary>
    /// Caches the value in the current <see cref="EnumerationContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    public void SetValue<T>(object key, T value)
      where T: class
    {
      SetValue(key, DefaultName, value);
    }

    /// <summary>
    /// Gets the cached value from the current <see cref="EnumerationContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>Cached value with the specified key;
    /// <see langword="null"/>, if no cached value is found, or it is already expired.</returns>
    public T GetValue<T>(object key)
      where T: class
    {
      return GetValue<T>(key, DefaultName);
    }

    internal void SetValue<T>(object key, string name, T value)
      where T: class
    {
      cache[new Pair<object, string>(key, name)] = value;
    }

    internal T GetValue<T>(object key, string name)
      where T: class
    {
      object result;
      if (cache.TryGetValue(new Pair<object, string>(key, name), out result))
        return result as T;
      return null;
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return EnumerationScope.CurrentContext==this; }
    }

    #endregion

    #region EnsureXxx methods

    /// <summary>
    /// Ensures the context is active.
    /// </summary>
    /// <exception cref="InvalidOperationException">Context is not active.</exception>
    public void EnsureIsActive()
    {
      if (EnumerationScope.CurrentContext!=this)
        throw new InvalidOperationException(string.Format(Strings.ExXMustBeActive, 
          GetType().GetShortName()));
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected EnumerationContext()
    {
    }
  }
}