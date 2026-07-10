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


    private static void Commit(Batch operation)
    {
        operation.Aspect.Commit(operation);
    }
}