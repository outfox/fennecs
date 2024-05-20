// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    internal bool Submit(Batch operation)
    {
        if (Mode != WorldMode.Immediate)
        {
            _deferredOperations.Enqueue(new DeferredOperation(operation));
            return false;
        }

        Commit(operation);
        return true;
    }


    private void Commit(Batch operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var preAddSignature = archetype.Signature.Except(operation.Removals);

            if (operation.AddMode == Batch.AddConflict.SkipEntirely
                && _typeGraph.TryGetValue(preAddSignature, out var preAddArchetype)
                && preAddArchetype.Signature.Overlaps(operation.Additions)) continue;

            var newSignature = preAddSignature.Union(operation.Additions);
            var newArchetype = GetArchetype(newSignature);
            archetype.Migrate(newArchetype, operation.Additions, operation.BackFill);
        }
    }
}