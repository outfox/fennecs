// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    internal bool Submit(Batch batch)
    {
        if (Mode != WorldMode.Immediate)
        {
            _deferredOperations.Enqueue(new(batch));
            return false;
        }

        Commit(batch);
        
        return true;
    }


    private void Commit(Batch operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var preAddSignature = archetype.Signature.Except(operation.Removals);
            var destinationSignature = preAddSignature.Union(operation.Additions);
            var destination = GetArchetype(destinationSignature);
            archetype.Migrate(destination, operation.Additions, operation.BackFill, operation.AddMode);
        }
    }
}