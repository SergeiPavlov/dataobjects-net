// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class AssociationInfo : Node
  {
    private Multiplicity multiplicity;
    private AssociationInfo reversed;
    private TypeInfo underlyingType;
    private bool isMaster = true;

    /// <summary>
    /// Gets the referencing type.
    /// </summary>
    public TypeInfo ReferencingType
    {
      get { return ReferencingField.DeclaringType; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is paired.
    /// </summary>
    public bool IsPaired
    {
      get { return reversed!=null; }
    }

    /// <summary>
    /// Gets master association.
    /// </summary>
    /// <remarks>
    /// If association is master, returns it. Otherwise returns paired association.
    /// </remarks>
    public AssociationInfo Master
    {
      get
      {
        if (isMaster) 
          return this;
        if (reversed==null || !reversed.isMaster) 
          throw new InvalidOperationException(String.Format(Strings.ExUnableToFindMasterAssociation, Name));
        return reversed;
      }
    }

    /// <summary>
    /// Gets the referencing field.
    /// </summary>
    public FieldInfo ReferencingField { get; private set; }

    /// <summary>
    /// Gets the referenced type.
    /// </summary>
    public TypeInfo ReferencedType { get; private set; }

    /// <summary>
    /// Gets the persistent type that represents this association.
    /// </summary>
    public TypeInfo UnderlyingType
    {
      get { return underlyingType; }
      set
      {
        this.EnsureNotLocked();
        underlyingType = value;
      }
    }

    /// <summary>
    /// Gets the association multiplicity.
    /// </summary>
    public Multiplicity Multiplicity
    {
      get { return multiplicity; }
      set
      {
        this.EnsureNotLocked();
        multiplicity = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is master association.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is master association; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMaster
    {
      get { return isMaster; }
      set
      {
        this.EnsureNotLocked();
        isMaster = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="ReferentialAction"/> that will be applied on <see cref="ReferencedType"/> object removal.
    /// </summary>
    public ReferentialAction OnRemove { get; private set; }

    /// <summary>
    /// Gets or sets the reversed paired <see cref="AssociationInfo"/> for this instance.
    /// </summary>
    public AssociationInfo Reversed
    {
      get { return reversed; }
      set
      {
        this.EnsureNotLocked();
        reversed = value;
      }
    }

    /// <summary>
    /// Gets the underlying index for this instance.
    /// </summary>
    /// <value>The underlying index.</value>
    public IndexInfo UnderlyingIndex { get; private set; }

    /// <summary>
    /// Gets the foreign key extraction transform.
    /// </summary>
    public SegmentTransform ExtractForeignKeyTransform { get; private set; }

    /// <summary>
    /// Gets the foreign key extraction delegate.
    /// </summary>
    public Func<Tuple, Tuple> ExtractForeignKey { get; private set; }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!ReferencingType.IsEntity)
        return;
      switch (Multiplicity) {
      case Multiplicity.OneToZero:
      case Multiplicity.OneToOne:
      case Multiplicity.OneToMany:
        UnderlyingIndex = ReferencingType.Indexes.GetIndex(ReferencingField.Name);
        ExtractForeignKeyTransform = ReferencingField.ExtractValueTransform;
        ExtractForeignKey = ReferencingField.ExtractValue;
        break;
      case Multiplicity.ManyToOne:
        UnderlyingIndex = Reversed.ReferencingType.Indexes.GetIndex(Reversed.ReferencingField.Name);
        break;
      case Multiplicity.ManyToZero:
      case Multiplicity.ManyToMany:
        UnderlyingIndex = underlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(IsMaster ? 1 : 0).First();
        break;
      }
      if (ExtractForeignKey == null) {
        Segment<int> foreignKeySegment = new Segment<int>(ReferencingType.Hierarchy.MappingInfo.Length, ReferencedType.Hierarchy.MappingInfo.Length);
        ExtractForeignKeyTransform = new SegmentTransform(true, UnderlyingIndex.TupleDescriptor, foreignKeySegment);
        ExtractForeignKey = tuple => ExtractForeignKeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="referencingField">The referencing field.</param>
    /// <param name="referencedType">The referenced type.</param>
    /// <param name="multiplicity">The association multiplicity.</param>
    /// <param name="onRemove">The <see cref="ReferentialAction"/> that will be applied on <see cref="ReferencedType"/> object removal.</param>
    public AssociationInfo(FieldInfo referencingField, TypeInfo referencedType, Multiplicity multiplicity, ReferentialAction onRemove)
    {
      ReferencingField = referencingField;
      ReferencedType = referencedType;
      Multiplicity = multiplicity;
      OnRemove = onRemove;
      referencingField.Association = this;
    }
  }
}