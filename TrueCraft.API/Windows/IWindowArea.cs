using System;

namespace TrueCraft.API.Windows
{
    public interface IWindowArea : IDisposable
    {
        event EventHandler<WindowChangeEventArgs> WindowChange;

        int StartIndex { get; }
        int Length { get; }
        int Width { get; }
        int Height { get; }
        ItemStack[] Items { get; }

        ItemStack this[int index] { get; set; }

        void CopyTo(IWindowArea area);
        int MoveOrMergeItem(int index, ItemStack item, IWindowArea from);
    }
}
