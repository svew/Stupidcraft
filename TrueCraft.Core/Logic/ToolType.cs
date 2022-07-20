using System;

namespace TrueCraft.Core
{
    /// <summary>
    /// Specifies the type of the Tool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note; values in this enum must correspond to values in the
    /// toolKindType in the TrueCraft.xsd file.
    /// </para>
    /// </remarks>
    [Flags]
    public enum ToolType
    {
        None = 1,
        Pickaxe = 2,
        Axe = 4,
        Shovel = 8,
        Hoe = 16,
        Sword = 32,
        All = None | Pickaxe | Axe | Shovel | Hoe | Sword
    }
}
