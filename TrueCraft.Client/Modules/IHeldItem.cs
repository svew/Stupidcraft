using System;
using TrueCraft.API;

namespace TrueCraft.Client.Modules
{
    public interface IHeldItem
    {
        ItemStack HeldItem { get; set; }
    }
}
