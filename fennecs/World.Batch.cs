// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    internal bool Submit(Batch batch, string callerLine, int callerFile)
    {
        if (Mode != WorldMode.Immediate)
        {
            _deferredOperations.Enqueue(new(batch, callerLine, callerFile));
            return false;
        }

        Commit(batch);
        
        return true;
    }


    private void Commit(Batch operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var preAddSignature = archetype.Signature.Remove(operation.Removals);
            var destinationSignature = preAddSignature.Union(operation.Additions);
            var destination = GetOrCreateArchetype(destinationSignature);
            archetype.Migrate(destination, operation.Additions, operation.BackFill, operation.AddMode);
        }
    }
}