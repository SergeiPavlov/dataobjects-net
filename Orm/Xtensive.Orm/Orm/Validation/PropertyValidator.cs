// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Standard base type for <see cref="IPropertyValidator"/> implementation.
  /// </summary>
  public abstract class PropertyValidator : Attribute, IPropertyValidator
  {
    private bool isImmediate;

    /// <summary>
    /// Gets or sets value indicating if current validator is immediate.
    /// </summary>
    public bool IsImmediate
    {
      get { return isImmediate; }
      set
      {
        if (Domain!=null)
          throw Exceptions.ObjectIsReadOnly(null);

        isImmediate = value;
      }
    }

    /// <summary>
    /// Gets domain this instance is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets type this instance is bound to.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Gets field this instance is bound to.
    /// </summary>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Configures this instance.
    /// </summary>
    /// <param name="domain">A domain this validator is bound to.</param>
    /// <param name="type">A type this validator is bound to.</param>
    /// <param name="field">A persitent field this validator is bound to.</param>
    public virtual void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      if (Domain!=null)
        throw Exceptions.ObjectIsReadOnly(null);

      Domain = domain;
      Type = type;
      Field = field;
    }

    /// <summary>
    /// Validates specified object considering new value of a persistent field.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    /// <param name="fieldValue">Persistent field value.</param>
    public abstract ValidationResult Validate(Entity target, object fieldValue);

    /// <summary>
    /// Creates new unconfigured <see cref="IPropertyValidator"/> instance
    /// with the same parameters.
    /// </summary>
    public abstract IPropertyValidator CreateNew();

    /// <summary>
    /// Constructs successful validation result.
    /// </summary>
    /// <returns>Constructed result.</returns>
    protected ValidationResult Success()
    {
      return ValidationResult.Success;
    }

    /// <summary>
    /// Constructs validation error result.
    /// </summary>
    /// <param name="value">Validated value.</param>
    /// <param name="errorMessage">Validatio error message.</param>
    /// <returns>Constructed result.</returns>
    protected ValidationResult Error(object value, string errorMessage)
    {
      return new ValidationResult(true, Field, value, errorMessage);
    }

    /// <summary>
    /// Throws configuration error with specified message.
    /// </summary>
    /// <param name="message">Configuration error message.</param>
    protected void ThrowConfigurationError(string message, Exception innerException = null)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(message, "message");

      var exceptionMessage = string.Format(
        Strings.ExValidatorXConfigurationFailedOnTypeYFieldZWithMessageA,
        GetType().Name, Type, Field, message);

      var exception = innerException==null
        ? new DomainBuilderException(exceptionMessage)
        : new DomainBuilderException(message, innerException);

      throw exception;
    }
  }
}