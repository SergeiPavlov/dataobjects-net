// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal static class ReferenceManager
  {
    private static readonly CascadeProcessor cascadeProcessor = new CascadeProcessor();
    private static readonly RestrictProcessor restrictProcessor = new RestrictProcessor();
    private static readonly ClearProcessor clearProcessor = new ClearProcessor();

    public static void ClearReferencesTo(Entity referencedObject, bool notify)
    {
      RemovalContext context = RemovalScope.Context ?? new RemovalContext(notify);

      using (context.Activate()) {
        context.RemovalQueue.Add(referencedObject);
        ApplyAction(referencedObject, ReferentialAction.Restrict);
        ApplyAction(referencedObject, ReferentialAction.Clear);
        ApplyAction(referencedObject, ReferentialAction.Cascade);
      }
    }

    private static void ApplyAction(Entity referencedObject, ReferentialAction action)
    {
      TypeInfo type = referencedObject.Type;
      ActionProcessor processor;
      switch(action) {
        case ReferentialAction.Clear:
          processor = clearProcessor;
          break;
        case ReferentialAction.Default:
          processor = restrictProcessor;
          break;
        case ReferentialAction.Cascade:
          processor = cascadeProcessor;
          break;
        default:
          throw new ArgumentOutOfRangeException("action");
      }

      IEnumerable<AssociationInfo> associations = type.GetAssociations().Where(a => a.OnRemove==action);

      foreach (AssociationInfo association in associations) {
        var referencingObjects = FindReferencingObjects(referencedObject, association);
        foreach (Entity referencingObject in referencingObjects)
          processor.Process(referencedObject, referencingObject, association);
      }
    }

    private static List<Entity> FindReferencingObjects(Entity referencedObject, AssociationInfo association)
    {
      FieldInfo field = association.ReferencingField;
      IndexInfo index = field.DeclaringType.Indexes.GetIndex(field.Name);
      RecordSet rs = index.ToRecordSet().Range(referencedObject.Key.Value, referencedObject.Key.Value);
      List<Entity> result = new List<Entity>();

      foreach (Entity referencingObject in rs.ToEntities(field.DeclaringType.UnderlyingType)) {
        if (RemovalScope.Context.RemovalQueue.Contains(referencingObject))
          continue;
        result.Add(referencingObject);
      }
      return result;
    }
  }
}