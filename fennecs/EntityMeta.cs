// SPDX-License-Identifier: MIT

namespace fennecs;

public struct EntityMeta(Identity identity, int tableId, int row)
{
    public Identity Identity = identity;
    public int TableId = tableId;
    public int Row = row;

    public void Clear()
    {
        Identity = Identity.None;
        TableId = 0;
        Row = 0;
    }
}