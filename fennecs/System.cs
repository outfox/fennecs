// SPDX-License-Identifier: MIT

namespace fennecs;

public interface ISystem
{
    void Run(World world);
}

public sealed class SystemGroup : List<ISystem>
{
    
    public void Run(World world)
    {
        foreach (var system in this)
        {
            system.Run(world);
        }
    }
}