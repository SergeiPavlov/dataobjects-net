// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Service provider contract.
  /// </summary>
  public interface IServiceProvider
  {
    /// <summary>
    /// Gets the service of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <returns>The service of specified type.</returns>
    T GetService<T>()
      where T : class;
  }
}