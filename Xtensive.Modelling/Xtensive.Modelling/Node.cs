// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Threading;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Validation;

namespace Xtensive.Modelling
{
  /// <summary>
  /// An abstract base class for model node.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public abstract class Node : LockableBase,
    INode,
    IDeserializationCallback
  {
    /// <summary>
    /// Path delimiter character.
    /// </summary>
    public static readonly char PathDelimiter = '/';
    /// <summary>
    /// Path escape character.
    /// </summary>
    public static readonly char PathEscape = '\\';

    [NonSerialized]
    private static ThreadSafeDictionary<Type, ReadOnlyDictionary<string, PropertyAccessor>> cachedPropertyAccessors = 
      ThreadSafeDictionary<Type, ReadOnlyDictionary<string, PropertyAccessor>>.Create(new object());
    [NonSerialized]
    private Node model;
    [NonSerialized]
    private string cachedPath;
    [NonSerialized]
    private Nesting nesting;
    [NonSerialized]
    private ReadOnlyDictionary<string, PropertyAccessor> propertyAccessors;
    internal Node parent;
    private string name;
    private NodeState state;
    private int index;

    #region Properties

    /// <inheritdoc/>
    [Property(IgnoreInComparison = true)]
    [SystemProperty]
    public Node Parent {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        if (value==Parent)
          return;
        NodeCollection collection = null;
        if (Nesting.IsNestedToCollection)
          collection = (NodeCollection) Nesting.PropertyGetter(value);
        Move(value, Name, collection==null ? 0 : collection.Count);
      }
    }

    /// <inheritdoc/>
    public Node Model {
      [DebuggerStepThrough]
      get { return model; }
    }

    /// <inheritdoc/>
    [Property(IgnoreInComparison = true)]
    [SystemProperty]
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
      [DebuggerStepThrough]
      set {
        Move(Parent, value, Index);
      }
    }

    /// <inheritdoc/>
    public string EscapedName
    {
      [DebuggerStepThrough]
      get { return new[] {Name}.RevertibleJoin(PathEscape, PathDelimiter); }
    }

    /// <inheritdoc/>
    public NodeState State {
      get { return state; }
    }

    /// <inheritdoc/>
    [Property(IgnoreInComparison = true)]
    [SystemProperty]
    public int Index
    {
      [DebuggerStepThrough]
      get { return index; }
      [DebuggerStepThrough]
      set {
        Move(Parent, Name, value);
      }
    }

    /// <inheritdoc/>
    public Nesting Nesting {
      [DebuggerStepThrough]
      get { return nesting; }
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, PropertyAccessor> PropertyAccessors {
      [DebuggerStepThrough]
      get { return propertyAccessors; }
    }

    /// <inheritdoc/>
    public object GetProperty(string propertyName)
    {
      return PropertyAccessors[propertyName].Getter.Invoke(this);
    }

    /// <inheritdoc/>
    public void SetProperty(string propertyName, object value)
    {
      PropertyAccessors[propertyName].Setter.Invoke(this, value);
    }

    /// <inheritdoc/>
    public IPathNode GetNestedProperty(string propertyName)
    {
      var getter = PropertyAccessors[propertyName].Getter;
      if (getter==null)
        return null;
      var value = getter.Invoke(this) as IPathNode;
      if (value==null)
        return null;
      if (value is NodeCollection)
        return value;
      var node = value as Node;
      if (node!=null && node.Parent==this)
        return value;
      return null;
    }

    /// <inheritdoc/>
    public string Path {
      //[DebuggerStepThrough]
      get {
        if (cachedPath!=null)
          return cachedPath;
        if (Parent==null)
          return string.Empty;
        else {
          string parentPath = Parent.Path;
          if (parentPath.Length!=0)
            parentPath += PathDelimiter;
          return string.Concat(
            parentPath, 
            Nesting.EscapedPropertyName,
            Nesting.IsNestedToCollection ? PathDelimiter.ToString() : string.Empty,
            Nesting.IsNestedToCollection ? EscapedName : string.Empty);
        }
      }
    }

    #endregion

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Move(Node newParent, string newName, int newIndex)
    {
      if (State==NodeState.Removed)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
      if (newParent==Parent && newName==Name && newIndex==Index)
        return;
      if (this is IUnnamedNode)
        newName = newIndex.ToString();
      ValidateMove(newParent, newName, newIndex);
      if (State==NodeState.Initializing) {
        parent = newParent;
        name = newName;
        index = newIndex;
        UpdateModel();
        OnPropertyChanged("Parent");
        OnPropertyChanged("Model");
        OnPropertyChanged("Name");
        OnPropertyChanged("EscapedName");
        OnPropertyChanged("Index");
        using (var scope = LogAction()) {
          scope.Action = new CreateNodeAction()
            {
              Path = newParent==null ? string.Empty : newParent.Path,
              Type = GetType(),
              Name = newName,
              Index = newIndex
            };
          PerformCreate();
          scope.Commit();
        }
      }
      else {
        using (var scope = LogAction()) {
          scope.Action = new MoveNodeAction()
            {
              Path = Path,
              Parent = newParent==parent ? null : newParent.Path,
              Name = newName==name ? null : newName,
              Index = newIndex==index ? (int?)null : newIndex
            };
          PerformMove(newParent, newName, newIndex);
          scope.Commit();
        }
      }
      OnPropertyChanged("State");
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Invalid node state.</exception>
    public void Remove()
    {
      EnsureIsEditable();
      ValidateRemove();
      using (var scope = LogAction(new RemoveNodeAction() { Path = Path })) {
        PerformRemove();
        scope.Commit();
      }
      OnPropertyChanged("State");
    }

    /// <inheritdoc/>
    public IPathNode Resolve(string path)
    {
      if (path.IsNullOrEmpty())
        return this;
      var parts = path.RevertibleSplitFirstAndTail(Node.PathEscape, Node.PathDelimiter);
      var next = (IPathNode) GetProperty(parts.First);
      if (parts.Second==null)
        return next;
      return next.Resolve(parts.Second);
    }

    /// <inheritdoc/>
    public void Validate()
    {
      using (ValidationScope.Open()) {
        using (var ea = new ExceptionAggregator()) {
          if (ValidationContext.Current.IsValidated(this))
            return;
          ValidateState();
          foreach (var pair in PropertyAccessors) {
            if (!pair.Value.HasGetter)
              continue;
            var nested = GetNestedProperty(pair.Key);
            if (nested!=null) {
              ea.Execute(x => x.Validate(), nested);
              continue;
            }
            var value = GetProperty(pair.Key);
            if (value!=null) {
              var pathNode = value as Node;
              if (pathNode!=null)
                ea.Execute(x => x.ValidateState(), pathNode);
            }
          }
        }
      }
    }

    #region ValidateXxx methods

    /// <summary>
    /// Validates the <see cref="Move"/> method arguments.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    /// <exception cref="ArgumentException">Item already exists.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is out of range, 
    /// or <paramref name="newParent"/> belongs to a different <see cref="Model"/>.</exception>
    /// <exception cref="InvalidOperationException">newName!=newIndex for <see cref="IUnnamedNode"/>.</exception>
    protected virtual void ValidateMove(Node newParent, string newName, int newIndex)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if (this is IModel) {
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, "newIndex");
        return;
      }

      // Validating parent model
      ArgumentValidator.EnsureArgumentNotNull(newParent, "newParent");
      ArgumentValidator.EnsureArgumentIs<Node>(newParent, "newParent");
      var model = Model;
      if (model!=null) {
        var newModel = newParent.Model;
        if (model!=newModel)
          throw new ArgumentOutOfRangeException("newParent.Model");
      }

      if (this is IUnnamedNode) {
        // Validation for unnamed nodes
        if (newName!=newIndex.ToString())
          throw Exceptions.InternalError("newName!=newIndex for IUnnamedNode!", Log.Instance);
      }
      else if (!Nesting.IsNestedToCollection)
        // Validation parent property nesting
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, 0, "newIndex");
      else {
        // Validation parent collection nesting
        var collection = (NodeCollection) Nesting.PropertyGetter(newParent);
        ArgumentValidator.EnsureArgumentIsInRange(newIndex, 0, collection.Count - (newParent==Parent ? 1 : 0), "newIndex");
        Node node;
        if (!collection.TryGetValue(newName, out node))
          return;
        if (node!=this)
          throw new ArgumentException(String.Format(
            Strings.ExItemWithNameXAlreadyExists, newName), newName);
      }
    }

    /// <summary>
    /// Validates the <see cref="Remove"/> method call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Model object cannot be removed.</exception>
    protected virtual void ValidateRemove()
    {
      if (this is IModel)
        throw new InvalidOperationException(Strings.ExModelObjectCannotBeRemoved);
    }

    /// <summary>
    /// Validates the state (i.e. checks everything except nested properties).
    /// </summary>
    protected virtual void ValidateState()
    {
      EnsureIsLive();
    }

    #endregion

    #region PerformXxx methods

    /// <summary>
    /// Actually performs construction operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Target object already exists.</exception>
    protected virtual void PerformCreate()
    {
      if (Parent!=null) {
        if (!Nesting.IsNestedToCollection) {
          if (Nesting.PropertyValue!=null)
            throw new InvalidOperationException(string.Format(
              Strings.ExTargetObjectExistsX, Nesting.PropertyValue));
          Nesting.PropertyValue = this;
        }
        else {
          var collection = (NodeCollection) Nesting.PropertyValue;
          collection.Add(this);
        }
      }
      state = NodeState.Live;
    }

    /// <summary>
    /// Actually performs <see cref="Move"/> operation.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    /// <exception cref="InvalidOperationException">Target object already exists.</exception>
    protected virtual void PerformMove(Node newParent, string newName, int newIndex)
    {
      bool bUnnamed = this is IUnnamedNode;
      if (newParent!=parent) {
        // Parent is changed
        if (!Nesting.IsNestedToCollection) {
          Nesting.PropertySetter(parent, null);
          var existingNode = Nesting.PropertyGetter(newParent);
          if (existingNode!=null)
            throw new InvalidOperationException(string.Format(
              Strings.ExTargetObjectExistsX, existingNode));
          Nesting.PropertySetter(newParent, this);
        }
        else {
          var oldCollection = (NodeCollection) Nesting.PropertyGetter(parent);
          var newCollection = (NodeCollection) Nesting.PropertyGetter(newParent);
          for (int i = index + 1; i < oldCollection.Count; i++)
            oldCollection[i].EnsureIsEditable();
          for (int i = newIndex; i < newCollection.Count; i++)
            newCollection[i].EnsureIsEditable();
          if (bUnnamed)
            oldCollection.RemoveName(this);
          for (int i = index + 1; i < oldCollection.Count; i++)
            oldCollection[i].PerformShift(-1);
          for (int i = newCollection.Count-1; i>=newIndex; i--)
            newCollection[i].PerformShift(1);
          oldCollection.Remove(this);
          index = newIndex;
          if (bUnnamed)
            name = newName;
          newCollection.Add(this);
        }
      }
      else {
        // Parent isn't changed
        if (newIndex!=index) {
          var collection = (NodeCollection) Nesting.PropertyGetter(newParent);
          int minIndex, maxIndex, shift;
          if (newIndex < index) {
            minIndex = newIndex;
            maxIndex = index - 1;
            shift = 1;
          }
          else {
            minIndex = index + 1;
            maxIndex = newIndex;
            shift = -1;
          }
          if (bUnnamed)
            collection.RemoveName(this);
          for (int i = minIndex; i <= maxIndex; i++)
            collection[i].EnsureIsEditable();
          if (shift<0)
            for (int i = minIndex; i <= maxIndex; i++)
              collection[i].PerformShift(shift);
          else
            for (int i = maxIndex; i >= maxIndex; i--)
              collection[i].PerformShift(shift);
          collection.Move(this, newIndex);
          name = newName;
          index = newIndex;
          if (bUnnamed)
            collection.AddName(this);
          // collection.CheckIntegrity();
        }
      }
      parent = (Node) newParent;
      name = newName;
      index = newIndex;
      UpdateModel();
    }

    /// <summary>
    /// Performs "shift" operation 
    /// (induced by <see cref="Move"/> operation of another node).
    /// </summary>
    /// <param name="offset">Shift offset.</param>
    protected virtual void PerformShift(int offset)
    {
      bool bUnnamed = this is IUnnamedNode;
      string newName = name;
      var newIndex = index + offset;
      if (bUnnamed) {
        newName = newIndex.ToString();
        if (Nesting.IsNestedToCollection) {
          var collection = ((NodeCollection) Nesting.PropertyValue);
          if (collection!=null) {
            collection.RemoveName(this);
            name = newName;
            index = newIndex;
            collection.AddName(this);
          }
        }
      }
      else {
        index = newIndex;
      }
    }

    /// <summary>
    /// Actually performs <see cref="Remove"/> operation.
    /// </summary>
    protected virtual void PerformRemove()
    {
      state = NodeState.Removed;
      if (!Nesting.IsNestedToCollection)
        Nesting.PropertyValue = null;
      else {
        var collection = (NodeCollection) Nesting.PropertyValue;
        collection.Remove(this);
        for (int i = 1; i < collection.Count; i++)
          collection[i].EnsureIsEditable();
        for (int i = 1; i < collection.Count; i++)
          collection[i].PerformShift(-1);
      }
      Lock(true);
    }

    #endregion

    #region LogXxx methods

    /// <summary>
    /// Logs the property change.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyValue">The property value.</param>
    /// <returns></returns>
    protected ActionScope LogPropertyChange(string propertyName, object propertyValue)
    {
      var scope = LogAction();
      var action = new PropertyChangeAction {Path = Path};
      action.Properties.Add(propertyName, PathNodeReference.Get(propertyValue));
      scope.Action = action;
      return scope;
    }
    
    /// <summary>
    /// Begins registration of a new action.
    /// </summary>
    /// <param name="action">The action to register.</param>
    /// <returns>
    /// <see cref="ActionScope"/> object allowing to describe it.
    /// </returns>
    protected ActionScope LogAction(NodeAction action)
    {
      var scope = LogAction();
      scope.Action = action;
      return scope;
    }

    /// <summary>
    /// Begins registration of a new action.
    /// </summary>
    /// <returns>
    /// <see cref="ActionScope"/> object allowing to describe it.
    /// </returns>
    protected ActionScope LogAction()
    {
      var model = (IModel) Model;
      if (model==null)
        return new ActionScope();
      var actions = model.Actions;
      if (actions==null)
        return new ActionScope();
      return actions.LogAction();
    }

    #endregion

    #region EnsureXxxx methods

    /// <summary>
    /// Ensures the node <see cref="State"/> is <see cref="NodeState.Live"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsLive()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
    }

    /// <summary>
    /// Ensures the node <see cref="State"/> is <see cref="NodeState.Live"/> and
    /// node isn't <see cref="Lock"/>ed.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="State"/> is invalid.</exception>
    protected void EnsureIsEditable()
    {
      if (State!=NodeState.Live)
        throw new InvalidOperationException(Strings.ExInvalidNodeState);
      this.EnsureNotLocked();
    }

    #endregion

    #region IDifferentiable<...> methods

    /// <inheritdoc/>
    public Difference GetDifferenceWith(object target, string propertyName, bool swap)
    {
      var difference = CreateDifference((Node) target, propertyName, swap);
      using (difference.Activate()) {
        if ((difference = BuildDifference())!=null)
          return difference;
      }
      return null;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Creates the difference object for the current node.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="propertyName">Name of the property that is being compared now (if any).</param>
    /// <param name="swap">Indicates whether source (this instance)
    /// and target are swapped.</param>
    /// <returns>Newly created <see cref="NodeDifference"/>
    /// object or its ancestor.</returns>
    protected virtual Difference CreateDifference(Node target, string propertyName, bool swap)
    {
      if (swap)
        return new NodeDifference(propertyName, target, this);
      else
        return new NodeDifference(propertyName, this, target);
    }

    /// <summary>
    /// Builds the difference.
    /// </summary>
    /// <exception cref="InvalidOperationException">Both Source and Target are null.</exception>
    protected virtual Difference BuildDifference()
    {
      var difference = (NodeDifference) Difference.Current;
      var propertyName = difference.PropertyName;
      var source = difference.Source;
      var target = difference.Target;

      // Filling MovementInfo
      var mi = new MovementInfo();
      if (source==null) {
        if (target==null)
          throw new InvalidOperationException(Strings.ExBothSourceAndTargetAreNull);
        if (!propertyName.IsNullOrEmpty() && target.Nesting.PropertyName==propertyName)
          mi.IsCreated = true;
        else
          return CreatePropertyValueDifference();
      }
      else if (target==null) {
        if (!propertyName.IsNullOrEmpty() && source.Nesting.PropertyName==propertyName)
          mi.IsRemoved = true;
        else
          return CreatePropertyValueDifference();
      }
      else if (!propertyName.IsNullOrEmpty() && target.Nesting.PropertyName==propertyName) {
        return CreatePropertyValueDifference();
      }
      else {
        if (!(source is IUnnamedNode) && Name!=target.Name) {
          var renameHint = HintSet.Current.GetHint<RenameHint>(source);
          if (renameHint==null || renameHint.TargetPath!=target.Path)
            mi.IsNameChanged = true;
        }
        mi.IsIndexChanged = source.index != target.Index;
        var collection = target.Nesting.PropertyValue as NodeCollection;
        if (collection!=null && (collection is IUnorderedNodeCollection))
          mi.IsIndexChanged = false;
        var nd = difference.GetNearestParent<NodeDifference>();
        if (nd!=null) {
          if (source.Parent!=nd.Source || target.Parent!=nd.Target)
            mi.IsParentChanged = true;
          var ndmi = nd.MovementInfo;
          if (ndmi!=null)
            mi.IsAnyParentChanged = ndmi.IsAnyParentChanged | mi.IsParentChanged;
        }
      }
      difference.MovementInfo = mi;

      // Comparing properties
      if (!mi.IsRemoved) {
        foreach (var pair in PropertyAccessors) {
          var newPropertyName = pair.Key;
          var accessor = pair.Value;
          var sa = accessor.PropertyInfo.GetAttribute<SystemPropertyAttribute>(
            AttributeSearchOptions.InheritNone);
          if (sa!=null)
            continue;
          var pa = accessor.PropertyInfo.GetAttribute<PropertyAttribute>(
            AttributeSearchOptions.InheritNone);
          if (pa!=null && pa.IgnoreInComparison)
            continue;

          object sv = (source==null || !accessor.HasGetter) ?
            GetDefaultPropertyValue(newPropertyName) : accessor.Getter(source);
          object tv = (target==null || !accessor.HasGetter) ?
            GetDefaultPropertyValue(newPropertyName) : accessor.Getter(target);
          if (sv==null) {
            if (tv==null)
              continue; // Both are null
            var dtv = tv as IDifferentiable;
            if (dtv!=null) {
              var d = dtv.GetDifferenceWith(sv, newPropertyName, true);
              if (d!=null) 
                difference.PropertyChanges.Add(newPropertyName, d);
            }
            else
              difference.PropertyChanges.Add(newPropertyName,
                new PropertyValueDifference(newPropertyName, sv, tv));
            continue;
          }
          var dsv = sv as IDifferentiable;
          if (dsv!=null) {
            var d = dsv.GetDifferenceWith(tv, newPropertyName, false);
            if (d!=null) 
              difference.PropertyChanges.Add(newPropertyName, d);
          }
          else if (!Equals(sv, tv))
            difference.PropertyChanges.Add(newPropertyName,
              new PropertyValueDifference(newPropertyName, sv, tv));
        }
      }

      return (!mi.IsUnchanged || difference.PropertyChanges.Count > 0) ? difference : null;
    }

    /// <summary>
    /// Creates the property value difference object describing current difference.
    /// </summary>
    /// <returns>New <see cref="PropertyValueDifference"/> object.</returns>
    protected static Difference CreatePropertyValueDifference()
    {
      var difference = (NodeDifference) Difference.Current;
      using (new ComparisonScope(difference.Parent)) {
        return new PropertyValueDifference(difference.PropertyName, 
          difference.Source, difference.Target);
      }
    }

    /// <summary>
    /// Gets the default property value.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Default property value.</returns>
    protected virtual object GetDefaultPropertyValue(string propertyName)
    {
      return PropertyAccessors[propertyName].Default;
    }

    #endregion

    #region INotifyPropertyChanged methods

    /// <inheritdoc/>
    [field : NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    protected virtual void OnPropertyChanged(string name)
    {
      if (PropertyChanged!=null)
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Does all the dirty job to change the property of this node.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name">Name of the property.</param>
    /// <param name="value">New value of the property.</param>
    /// <param name="setter">Property setter delegate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> belongs to a different <see cref="Model"/>.</exception>
    protected void ChangeProperty<T>(string name, T value, Action<Node,T> setter)
    {
      EnsureIsEditable();
      var pathNode = value as IPathNode;
      var model = Model;
      if (pathNode!=null && model!=null && pathNode.Model!=model)
        throw new ArgumentOutOfRangeException(Strings.ExPropertyValueMustBelongToTheSameModel, "value");
      using (var scope = !PropertyAccessors.ContainsKey(name) ? null : LogPropertyChange(name, value)) {
        setter.Invoke(this, value);
        OnPropertyChanged(name);
        if (scope!=null)
          scope.Commit();
      }
    }

    #endregion

    #region ILockable methods

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      foreach (var pair in PropertyAccessors) {
        var nested = GetNestedProperty(pair.Key);
        if (nested!=null)
          nested.Lock();
      }
      cachedPath = Path;
    }

    #endregion

    #region Private \ internal methods

    private void UpdateModel()
    {
      var p = Parent;
      if (p==null)
        model = (Node) (this as IModel);
      else
        model = p.Model;
    }

    private static ReadOnlyDictionary<string, PropertyAccessor> GetPropertyAccessors(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return cachedPropertyAccessors.GetValue(type,
        (_type) => {
          var d = new Dictionary<string, PropertyAccessor>();
          if (_type!=typeof(object))
            foreach (var pair in GetPropertyAccessors(_type.BaseType))
              d.Add(pair.Key, pair.Value);
          foreach (var p in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (p.GetAttribute<PropertyAttribute>(AttributeSearchOptions.InheritNone)!=null)
              d.Add(p.Name, new PropertyAccessor(p));
          }
          return new ReadOnlyDictionary<string, PropertyAccessor>(d, false);
        });
    }

    #endregion

    #region To override

    /// <summary>
    /// Creates <see cref="Nesting"/> object describing how this node is nested.
    /// </summary>
    /// <returns>New <see cref="Nesting"/> object.</returns>
    protected abstract Nesting CreateNesting();

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="CreateNesting"/> has returned <see langword="null" />.</exception>
    protected virtual void Initialize()
    {
      nesting = CreateNesting();
      if (nesting==null)
        throw new InvalidOperationException(Strings.ExNoNesting);
      propertyAccessors = GetPropertyAccessors(GetType());
    }

    #endregion

    #region Dump, ToString

    /// <inheritdoc/>
    public virtual void Dump()
    {
      string prefix = string.Empty;
      if (Nesting.IsNestedToCollection && !(Nesting.PropertyValue is IUnorderedNodeCollection))
        prefix = string.Format("{0}: ", Index);
      Log.Info("{0}{1} \"{2}\"", prefix, GetType().GetShortName(), this);
      using (new LogIndentScope()) {
        // Validation errors
        Exception error = null;
        try { 
          ValidateState();
        }
        catch (Exception e) {
          error = e;
        }
        // Basic properties
        if (error!=null)
          Log.Info("+Error = {0}", error);
        if (State!=NodeState.Live)
          Log.Info("+State = {0}", State);
        // Everything else
        foreach (var pair in propertyAccessors) {
          string propertyName = pair.Key;
          var accessor = pair.Value;
          if (accessor.PropertyInfo.GetAttribute<SystemPropertyAttribute>(
            AttributeSearchOptions.InheritNone)!=null)
            continue;
          var propertyValue = accessor.HasGetter ? GetProperty(propertyName) : null;
          if (Equals(propertyValue, accessor.Default))
            continue;
          var propertyType = 
            (propertyValue==null ? accessor.PropertyInfo.PropertyType : propertyValue.GetType())
            .GetShortName();
          var nested = GetNestedProperty(propertyName);
          if (nested!=null) {
            var collection = nested as NodeCollection;
            if (collection!=null && collection.Count!=0)
              Log.Info("+{0} ({1}):", propertyName, collection.Count);
            else
              Log.Info("+{0}:", propertyName);
            using (new LogIndentScope())
              nested.Dump();
          }
          else
            Log.Info("+{0} = {1} ({2})", propertyName, propertyValue, propertyType);
        }
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var m = Model;
      string fullName = Path;
      if (m!=null)
        fullName = string.Concat(m.EscapedName, PathDelimiter, fullName);
      if (!Nesting.IsNestedToCollection && !(this is IModel))
        fullName = string.Format(Strings.NodeInfoFormat, fullName, Name);
      return fullName;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    /// <param name="index">Initial <see cref="Index"/> property value.</param>
    protected Node(Node parent, string name, int index)
    {
      if (!(this is IModel))
        ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      if (!(this is IUnnamedNode))
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Initialize();
      Move(parent, name, index);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent"><see cref="Parent"/> property value.</param>
    /// <param name="name">Initial <see cref="Name"/> property value.</param>
    protected Node(Node parent, string name)
    {
      if (!(this is IModel))
        ArgumentValidator.EnsureArgumentNotNull(parent, "parent");
      if (!(this is IUnnamedNode))
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Initialize();
      if (!Nesting.IsNestedToCollection)
        Move(parent, name, 0);
      else
        Move(parent, name, ((NodeCollection) Nesting.PropertyGetter(parent)).Count);
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      if (nesting!=null)
        return; // Protects from multiple calls
      Initialize();
      var p = Parent as IDeserializationCallback;
      if (p!=null)
        p.OnDeserialization(sender);
      UpdateModel();
      if (IsLocked) {
        cachedPath = Path;
      }
    }
  }
}