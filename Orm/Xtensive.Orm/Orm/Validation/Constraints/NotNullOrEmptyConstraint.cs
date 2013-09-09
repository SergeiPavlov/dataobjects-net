// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that property value is not 
  /// <see langword="null" /> or <see cref="string.Empty"/>.
  /// </summary>
  public sealed class NotNullOrEmptyConstraint : PropertyValidator
  {
    public override void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      base.Configure(domain, type, field);

      if (field.ValueType!=typeof (string))
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, typeof (string).FullName));
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      var value = (string) fieldValue;
      return !string.IsNullOrEmpty(value) ? Success() : Error(fieldValue, Strings.ValueShouldNotBeEmpty);
    }

    public override IPropertyValidator CreateNew()
    {
      return new NotNullOrEmptyConstraint {
        IsImmediate = IsImmediate,
      };
    }
  }
}