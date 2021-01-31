﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sync.Behaviour
{
    public static class Util
    {
        public static Dictionary<MethodId, List<CallBehaviourBuilder>> SortByMethod(List<ActionBehaviourBuilder> allBehaviours)
        {
            Dictionary<MethodId, List<CallBehaviourBuilder>> applicableBehaviours =
                new Dictionary<MethodId, List<CallBehaviourBuilder>>();
            
            IEnumerable<MethodId> patchedMethods = allBehaviours
                .SelectMany(b => b.CallBehaviours.Keys).Distinct();
            foreach (MethodId patchedMethod in patchedMethods)
            {
                var callBehaviours = allBehaviours
                    .Where(a => a.CallBehaviours.ContainsKey(patchedMethod))
                    .Select(a => a.CallBehaviours[patchedMethod]);
                applicableBehaviours[patchedMethod] = callBehaviours.ToList();
            }

            return applicableBehaviours;
        }
        
        public static Dictionary<FieldId, List<FieldActionBehaviourBuilder>> SortByField(List<ActionBehaviourBuilder> allBehaviours)
        {
            Dictionary<FieldId, List<FieldActionBehaviourBuilder>> applicableBehaviours =
                new Dictionary<FieldId, List<FieldActionBehaviourBuilder>>();
            
            IEnumerable<FieldId> patchedFields = allBehaviours
                .SelectMany(b => b.FieldChangeAction.Keys).Distinct();
            foreach (FieldId patchedField in patchedFields)
            {
                var fieldBehaviours = allBehaviours
                    .Where(a => a.FieldChangeAction.ContainsKey(patchedField))
                    .Select(a => a.FieldChangeAction[patchedField]);
                applicableBehaviours[patchedField] = fieldBehaviours.ToList();
            }

            return applicableBehaviours;
        }
    }
}