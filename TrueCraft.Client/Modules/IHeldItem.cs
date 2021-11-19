using System;
using TrueCraft.Core;

namespace TrueCraft.Client.Modules
{
    public interface IHeldItem
    {
        ItemStack HeldItem { get; set; }
    }
}
