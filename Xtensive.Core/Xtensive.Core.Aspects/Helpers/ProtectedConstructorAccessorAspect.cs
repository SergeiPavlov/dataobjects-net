// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.02

using System;
using System.Reflection;
using System.Linq;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Helpers.Internals;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Protected constructor accessors aspect - provides an accessor (delegate)
  /// for the specified protected constructor of a type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  [Serializable]
  public sealed class ProtectedConstructorAccessorAspect : CompoundAspect
  {

    /// <summary>
    /// Gets the protected constructor argument types.
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <summary>
    /// Gets or sets the target type.
    /// </summary>
    public Type TargetType { get; private set; }

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      TargetType = (Type)element;
      var surrogateType = AspectHelper.GetSurrogateType(TargetType.Module);
      collection.AddAspect(surrogateType, new ImplementProtectedConstructorAccessorAspect(this));
    }

   /* /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      AspectHelper.AddStandardRequirements(requirements);
      return requirements;
    }*/

    /*/// <summary>
    /// Applies this aspect to the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to apply the aspect to.</param>
    /// <param name="parameterTypes">Types of constructor parameters.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ProtectedConstructorAccessorAspect ApplyOnce(Type type, params Type[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(parameterTypes, "parameterTypes");

      return AppliedAspectSet.Add(
        string.Format("{0}({1})", type.FullName, parameterTypes.Select(t => t.FullName).ToCommaDelimitedString()),
        () => new ProtectedConstructorAccessorAspect(parameterTypes));
    }*/


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parameterTypes"><see cref="ParameterTypes"/> property value.</param>
    public ProtectedConstructorAccessorAspect(params Type[] parameterTypes)
    {
      ParameterTypes = parameterTypes;
    }
  }
}
