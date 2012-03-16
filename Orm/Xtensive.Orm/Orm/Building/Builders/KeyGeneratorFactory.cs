﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Internals.KeyGenerators;

namespace Xtensive.Orm.Building.Builders
{
  internal static class KeyGeneratorFactory
  {
    private static readonly Type[] SupportedTypes = WellKnown.SupportedNumericTypes
      .Concat(new[] {typeof (Guid), typeof (string)})
      .ToArray();

    public static bool IsSupported(Type valueType)
    {
      return SupportedTypes.Contains(valueType);
    }

    public static bool IsSequenceBacked(Type valueType)
    {
      return WellKnown.SupportedNumericTypes.Contains(valueType);
    }

    private static Type GetGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType))
        return typeof (StorageSequentalGenerator<>).MakeGenericType(valueType);

      if (valueType==typeof (Guid))
        return typeof (GuidGenerator);

      if (valueType==typeof (string))
        return typeof (StringGenerator);

      throw TypeNotSupported(valueType);
    }

    private static Type GetTemporaryGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType))
        return typeof (TemporarySequentalGenerator<>).MakeGenericType(valueType);

      if (valueType==typeof (Guid))
        return typeof (GuidGenerator);

      if (valueType==typeof (string))
        return typeof (StringGenerator);

      throw TypeNotSupported(valueType);
    }

    private static IEnumerable<ServiceRegistration> GetStandardRegistrations(string name, Type valueType)
    {
      yield return new ServiceRegistration(
        typeof (IKeyGenerator), name, GetGeneratorType(valueType), true);
      yield return new ServiceRegistration(
        typeof (ITemporaryKeyGenerator), name, GetTemporaryGeneratorType(valueType), true);
    }

    public static IEnumerable<ServiceRegistration> GetRegistrations(BuildingContext context)
    {
      var standardRegistrations = context.Model.Hierarchies.Select(h => h.Key)
        .Where(key => key.GeneratorKind==KeyGeneratorKind.Default && key.IsFirstAmongSimilarKeys)
        .SelectMany(key => GetStandardRegistrations(key.GeneratorName, key.SingleColumnType));

      var userRegistrations = context.Configuration.Types.KeyGenerators
        .SelectMany(ServiceRegistration.CreateAll)
        .ToList();

      return userRegistrations.Concat(standardRegistrations);
    }

    private static NotSupportedException TypeNotSupported(Type valueType)
    {
      return new NotSupportedException(String.Format(
        "Type '{0}' is not supported by standard key generators", valueType));
    }
  }
}