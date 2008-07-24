// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Provides consistency validation for see <see cref="IValidationAware"/> implementors.
  /// </summary>
  public abstract class ValidationContextBase: Context<ValidationScope>
  {
    private bool isConsistent = true;
    private int  activationCount;
    private HashSet<Pair<IValidationAware, Action>> registry;

    /// <inheritdoc/>
    protected override ValidationScope CreateActiveScope()
    {
      return new ValidationScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return ValidationScope.CurrentContext == this; }
    }

    /// <summary>
    /// Gets the value indicating whether this context is in inconsistent state.
    /// </summary>
    public bool IsConsistent {
      get {
        return isConsistent;
      }
      private set {
        if (value==isConsistent)
          return;
        isConsistent = value;
        if (value)
          LeaveInconsistentRegion();
        else
          EnterInconsistentRegion();
      }
    }

    /// <summary>
    /// Creates the "inconsistent region" - the code region, in which <see cref="Validate"/> method
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <returns>
    /// <see cref="IDisposable"/> object, which disposal will identify the end of the region.
    /// <see langowrd="Null"/>, if <see cref="IsConsistent"/> is <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The beginning of the region is the place where this method is called.
    /// </para>
    /// <para>
    /// The end of the region is the place where returned <see cref="IDisposable"/> object is disposed.
    /// The validation of all queued to-validate objects will be performed during disposal.
    /// </para>
    /// </remarks>
    public IDisposable InconsistentRegion()
    {
      if (!IsConsistent)
        return null;
      IsConsistent = false;
      return new Disposable<ValidationContextBase>(this, (disposing, context) => context.IsConsistent = true);
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public bool Validate(IValidationAware target, Action validationDelegate, ValidationMode mode)
    {      
      if (!target.IsCompatibleWith(this))
        throw new ArgumentException(Strings.ExObjectAndContextAreIncompatible, "target");

      bool immedate = mode==ValidationMode.Immediate || IsConsistent;

      if (immedate)
        if (validationDelegate==null)
          target.OnValidate();
        else
          validationDelegate.Invoke();
      else
        EnqueueValidate(target, validationDelegate);        

      return immedate;
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="validationDelegate">The delegate partially validating object.
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public bool Validate(IValidationAware target, Action validationDelegate)
    {
      return Validate(target, validationDelegate, ValidationMode.Default);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>        
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public bool Validate(IValidationAware target, ValidationMode mode)
    {
      return Validate(target, null, mode);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/> using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>            
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public bool Validate(IValidationAware target)
    {
      return Validate(target, null, ValidationMode.Default);
    }


    #region Protected methods (to override, if necessary)    

    /// <summary>
    /// Enqueues the object for delayed partial validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    /// <param name="validationDelegate">The validation delegate partially validating the <paramref name="target"/>.
    /// If <see langword="null" />, whole object should be validated.
    /// </param>    
    protected virtual void EnqueueValidate(IValidationAware target, Action validationDelegate)
    {
      registry.Add(new Pair<IValidationAware, Action>(target, validationDelegate));
    }

    /// <summary>
    /// Enqueues the object for delayed validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>    
    protected virtual void EnqueueValidate(IValidationAware target)
    {
      EnqueueValidate(target, null);
    }

    /// <summary>
    /// Enters the inconsistent region.
    /// </summary>
    protected virtual void EnterInconsistentRegion()
    {
      registry = new HashSet<Pair<IValidationAware, Action>>();
    }

    /// <summary>
    /// Leaves the inconsistent region.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    protected virtual void LeaveInconsistentRegion()
    {      
      IList<Exception> exceptions = null;
      try {
        foreach (var pair in registry) {
          try {           
            if (pair.Second==null)
              pair.First.Validate();
            else
              if (!registry.Contains(new Pair<IValidationAware, Action>(pair.First, null)))
                pair.Second.Invoke();
          }
          catch (Exception e) {
            if (exceptions==null)
              exceptions = new List<Exception>();
            exceptions.Add(e);
          }
        }
      }
      finally {
        registry = null;
        if (exceptions!=null && exceptions.Count > 0)
          throw new AggregateException(exceptions);
      }
    }

    /// <summary>
    /// Called on context activation.
    /// </summary>
    /// <param name="scope">The scope activating this context.</param>
    protected internal virtual void OnActivate(ValidationScope scope)
    {
      activationCount++;
    }

    /// <summary>
    /// Called on context deactivation.
    /// </summary>
    /// <param name="scope">The scope deactivating this context.</param>
    protected internal void OnDeactivate(ValidationScope scope)
    {
      activationCount--;
      if (activationCount==0 && !IsConsistent)
        LeaveInconsistentRegion();
    }

    #endregion
  }
}
