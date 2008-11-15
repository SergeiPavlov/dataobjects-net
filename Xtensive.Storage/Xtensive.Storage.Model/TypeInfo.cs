// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents an object describing any persistent type.
  /// </summary>
  [DebuggerDisplay("{underlyingType}")]
  [Serializable]
  public sealed class TypeInfo: MappingNode
  {
    private ColumnInfoCollection                            columns;
    private readonly FieldMap                               fieldMap;
    private readonly FieldInfoCollection                    fields;
    private readonly TypeIndexInfoCollection                indexes;
    private readonly NodeCollection<IndexInfo>              affectedIndexes;
    private readonly DomainModel                            model;
    private readonly TypeAttributes                         attributes;
    private ReadOnlyList<TypeInfo>                          ancestors;
    private ReadOnlyList<AssociationInfo>                   associations;
    private Type                                            underlyingType;
    private HierarchyInfo                                   hierarchy;
    private int                                             typeId;
    private TupleDescriptor                                 tupleDescriptor;

    /// <summary>
    /// Gets a value indicating whether this instance is entity.
    /// </summary>
    public bool IsEntity
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Entity) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is abstract entity.
    /// </summary>
    public bool IsAbstract
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Abstract) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is interface.
    /// </summary>
    public bool IsInterface
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Interface) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is structure.
    /// </summary>
    public bool IsStructure
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Structure) > 0; }
    }

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      [DebuggerStepThrough]
      get { return underlyingType; }
      set
      {
        this.EnsureNotLocked();
        underlyingType = value;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public TypeAttributes Attributes
    {
      [DebuggerStepThrough]
      get { return attributes; }
    }


    /// <summary>
    /// Gets the columns contained in this instance.
    /// </summary>
    public ColumnInfoCollection Columns
    {
      [DebuggerStepThrough]
      get { return columns; }
    }

    /// <summary>
    /// Gets the indexes for this instance.
    /// </summary>
    public TypeIndexInfoCollection Indexes
    {
      [DebuggerStepThrough]
      get { return indexes; }
    }

    public NodeCollection<IndexInfo> AffectedIndexes
    {
      [DebuggerStepThrough]
      get { return affectedIndexes; }
    }

    /// <summary>
    /// Gets the fields contained in this instance.
    /// </summary>
    public FieldInfoCollection Fields
    {
      [DebuggerStepThrough]
      get { return fields; }
    }

    /// <summary>
    /// Gets the field map for implemented interfaces.
    /// </summary>
    public FieldMap FieldMap
    {
      [DebuggerStepThrough]
      get { return fieldMap; }
    }

    /// <summary>
    /// Gets the <see cref="DomainModel"/> this instance belongs to.
    /// </summary>
    public DomainModel Model
    {
      [DebuggerStepThrough]
      get { return model; }
    }

    /// <summary>
    /// Gets or sets the hierarchy.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      [DebuggerStepThrough]
      get { return hierarchy; }
      set
      {
        this.EnsureNotLocked();
        hierarchy = value;
      }
    }

    /// <summary>
    /// Gets or sets the type id.
    /// </summary>
    /// <value></value>
    public int TypeId
    {
      [DebuggerStepThrough]
      get { return typeId; }
      [DebuggerStepThrough]
      set
      {
        this.EnsureNotLocked();
        typeId = value;
      }
    }

    /// <summary>
    /// Gets the tuple descriptor.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor
    {
      [DebuggerStepThrough]
      get { return tupleDescriptor; }
    }

    /// <summary>
    /// Gets the persistent type prototype.
    /// </summary>
    public Tuple Prototype { get; private set; }

    /// <summary>
    /// Gets the primary key injector transform.
    /// </summary>
    public MapTransform InjectPrimaryKeyTransform { get; private set; }

    /// <summary>
    /// Gets the primary key injector.
    /// </summary>
    public Func<Tuple, Tuple> InjectPrimaryKey { get; private set; }

    /// <summary>
    /// Gets the direct descendants of this instance.
    /// </summary>
    public IEnumerable<TypeInfo> GetDescendants()
    {
      return GetDescendants(false);
    }

    /// <summary>
    /// Gets descendants of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and nested descendants will be returned.</param>
    /// <returns></returns>
    public IEnumerable<TypeInfo> GetDescendants(bool recursive)
    {
      return model.Types.FindDescendants(this, recursive);
    }

    /// <summary>
    /// Gets the direct persistent interfaces this instance implements.
    /// </summary>
    public IEnumerable<TypeInfo> GetInterfaces()
    {
      return model.Types.FindInterfaces(this);
    }

    /// <summary>
    /// Gets the persistent interfaces this instance implements.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implemented interfaces will be returned.</param>
    public IEnumerable<TypeInfo> GetInterfaces(bool recursive)
    {
      return model.Types.FindInterfaces(this, true);
    }

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    public IEnumerable<TypeInfo> GetImplementors()
    {
      return model.Types.FindImplementors(this);
    }

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implementors will be returned.</param>
    public IEnumerable<TypeInfo> GetImplementors(bool recursive)
    {
      return model.Types.FindImplementors(this, recursive);
    }

    /// <summary>
    /// Gets the ancestor.
    /// </summary>
    /// <returns>The ancestor</returns>
    public TypeInfo GetAncestor()
    {
      return model.Types.FindAncestor(this);
    }

    /// <summary>
    /// Gets the ancestors recursively. Root-to-inheritor order.
    /// </summary>
    /// <returns>The ancestor</returns>
    public IList<TypeInfo> GetAncestors()
    {
      if (IsLocked)
        return ancestors;

      var result = new List<TypeInfo>();
      var ancestor = model.Types.FindAncestor(this);
      // TODO: Refactor
      while (ancestor!=null) {
        result.Add(ancestor);
        ancestor = model.Types.FindAncestor(ancestor);
      }
      result.Reverse();
      return result;
    }

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    /// <returns>The hierarchy root.</returns>
    public TypeInfo GetRoot()
    {
      return model.Types.FindRoot(this);
    }

    /// <summary>
    /// Gets the associations this instance is participating in.
    /// </summary>
    public IList<AssociationInfo> GetAssociations()
    {
      if (IsLocked)
        return associations;

      return model.Associations.Find(this).ToList();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      ancestors = new ReadOnlyList<TypeInfo>(GetAncestors());
      associations = new ReadOnlyList<AssociationInfo>(GetAssociations());
      base.Lock(recursive);
      if (recursive) {
        affectedIndexes.Lock(true);
        indexes.Lock(true);
        columns.Lock(true);
        fieldMap.Lock(true);
      }
      if (IsInterface) {
        if (recursive)
          fields.Lock(true);
        return;
      }
      var orderedColumns = columns.OrderBy(c => c.Field.MappingInfo.Offset).ToList();
      columns = new ColumnInfoCollection();
      columns.AddRange(orderedColumns);
      columns.Lock(true);
      tupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
      fields.Lock(true);

      if (IsEntity || IsStructure) {
        // Building nullable map
        var nullableMap = new BitArray(TupleDescriptor.Count);
        int i = 0;
        foreach (var column in Columns)
          nullableMap[i++] = column.IsNullable;

        // Building prototype
        Prototype = Tuple.Create(TupleDescriptor);
        Prototype.Initialize(nullableMap);
        if (IsEntity) {
          var typeIdField = Fields.Where(f => f.IsTypeId).First();
          Prototype.SetValue(typeIdField.MappingInfo.Offset, TypeId);

          // Building primary key injector
          var fieldCount = TupleDescriptor.Count;
          var keyFieldCount = Hierarchy.KeyTupleDescriptor.Count;
          var keyFieldMap = new Pair<int, int>[fieldCount];
          for (i = 0; i < fieldCount; i++)
            keyFieldMap[i] = new Pair<int, int>((i < keyFieldCount) ? 0 : 1, i);
          InjectPrimaryKeyTransform = new MapTransform(true, TupleDescriptor, keyFieldMap);
          InjectPrimaryKey = keyValue => InjectPrimaryKeyTransform.Apply(TupleTransformType.TransformedTuple, keyValue, Prototype);
        }
        Prototype = Prototype.ToFastReadOnly();
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="typeAttributes">The type attributes.</param>
    public TypeInfo(DomainModel model, TypeAttributes typeAttributes)
    {
      this.model = model;
      attributes = typeAttributes;
      columns = new ColumnInfoCollection();
      fields = new FieldInfoCollection();
      fieldMap = IsEntity ? new FieldMap() : FieldMap.Empty;
      indexes = new TypeIndexInfoCollection();
      affectedIndexes = new NodeCollection<IndexInfo>();
    }
  }
}