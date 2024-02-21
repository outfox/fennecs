// SPDX-License-Identifier: MIT

namespace fennecs;

public struct EntityMeta(Entity entity, int tableId, int row)
{
    public Entity Entity = entity;
    public int TableId = tableId;
    public int Row = row;

    public void Clear()
    {
        Entity = Entity.None;
        TableId = 0;
        Row = 0;
    }
}