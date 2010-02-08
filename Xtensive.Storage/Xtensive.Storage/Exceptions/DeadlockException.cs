// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// An exception that is thrown when deadlock is detected by RDBMS.
  /// </summary>
  [Serializable]
  public class DeadlockException : ReprocessableException
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public DeadlockException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DeadlockException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected DeadlockException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }    
  }
}