using System;
using System.Collections.Generic;
using System.Xml;
using TrueCraft.Core.Windows;

namespace TrueCraft.Core.Logic
{
    public class CraftingPattern : IEquatable<CraftingPattern>
    {
        // TODO: These need to be immutable.
        private ItemStack[,] _pattern;

        #region Construction
        public static CraftingPattern GetCraftingPattern(XmlNode pattern)
        {
            int xmax = 3, ymax = 3;
            ItemStack[,] items = new ItemStack[xmax, ymax];
            int x = 0, y = 0;

            for (x = 0; x < xmax; x++)
                for (y = 0; y < ymax; y++)
                    items[x, y] = ItemStack.EmptyStack;

            y = 0;
            foreach(XmlNode row in pattern.ChildNodes)
            {
                x = 0;
                foreach (XmlNode itemNode in row.ChildNodes)
                {
                    items[x, y] = new ItemStack(itemNode);
                    x++;
                }
                y++;
            }

            return GetCraftingPattern(items);
        }

        public CraftingPattern(ItemStack[,] items, int xmin, int xmax, int ymin, int ymax)
        {
            _pattern = new ItemStack[xmax - xmin + 1, ymax - ymin + 1];
            for (int x = xmin; x <= xmax; x++)
                for (int y = ymin; y <= ymax; y++)
                    _pattern[x - xmin, y - ymin] = new ItemStack(items[x, y].ID, 1, items[x, y].Metadata);
        }

        public static CraftingPattern GetCraftingPattern(ItemStack[,] items)
        {
            int xul = items.GetLength(0);
            int yul = items.GetLength(1);

            if (xul < 1 || xul > 3 || yul < 1 || yul > 3)
                throw new ArgumentException();

            // Find the smallest non-empty column index
            int xmin = 0;
            bool blank;
            do
            {
                blank = true;
                for (int y = 0; y < yul && blank; y++)
                    blank = (ItemStack.EmptyStack == items[xmin, y]);
                if (blank)
                    xmin++;
            } while (blank && xmin < xul);
            if (xmin == xul)
                return null;

            // Find the largest non-empty column index.
            int xmax = xul - 1;
            do
            {
                blank = true;
                for (int y = yul - 1; y >= 0 && blank; y--)
                    blank = (ItemStack.EmptyStack == items[xmax, y]);
                if (blank)
                    xmax--;
            } while (blank);

            // Find the smallest non-empty row index.
            int ymin = 0;
            do
            {
                blank = true;
                for (int x = xmin; x <= xmax && blank; x++)
                    blank = (ItemStack.EmptyStack == items[x, ymin]);
                if (blank)
                    ymin++;
            } while (blank);

            // Find the largest non-empty row index.
            int ymax = yul - 1;
            do
            {
                blank = true;
                for (int x = xmax; x >= xmin && blank; x--)
                    blank = (ItemStack.EmptyStack == items[x, ymax]);
                if (blank)
                    ymax--;
            } while (blank);

            return new CraftingPattern(items, xmin, xmax, ymin, ymax);

        }

        private CraftingPattern(ICraftingArea area, int xmin, int xmax, int ymin, int ymax)
        {
            _pattern = new ItemStack[xmax - xmin + 1, ymax - ymin + 1];

            for (int x = xmin; x <= xmax; x++)
                for (int y = ymin; y <= ymax; y++)
                {
                    ItemStack item = area.GetItemStack(x, y);
                    _pattern[x - xmin, y - ymin] = new ItemStack(item.ID, 1, item.Metadata);
                }
        }

        public static CraftingPattern GetCraftingPattern(ICraftingArea area)
        {
            // Find x & y extents of the crafting inputs.
            int xmin, xmax;
            int ymin, ymax;
            bool blank;

            // Find the smallest non-empty column index
            xmin = 0;
            do
            {
                blank = true;
                for (int y = 0; y < area.Height && blank; y++)
                    blank = (ItemStack.EmptyStack == area.GetItemStack(xmin, y));
                if (blank)
                    xmin++;
            } while (blank && xmin < area.Width);
            if (xmin == area.Width)
                return null;

            // Find the largest non-empty column index.
            xmax = area.Width - 1;
            do
            {
                blank = true;
                for (int y = area.Height - 1; y >= 0 && blank; y--)
                    blank = (ItemStack.EmptyStack == area.GetItemStack(xmax, y));
                if (blank)
                    xmax--;
            } while (blank);

            // Find the smallest non-empty row index.
            ymin = 0;
            do
            {
                blank = true;
                for (int x = xmin; x <= xmax && blank; x++)
                    blank = (ItemStack.EmptyStack == area.GetItemStack(x, ymin));
                if (blank)
                    ymin++;
            } while (blank);

            // Find the largest non-empty row index.
            ymax = area.Height - 1;
            do
            {
                blank = true;
                for (int x = xmax; x >= xmin && blank; x--)
                    blank = (ItemStack.EmptyStack == area.GetItemStack(x, ymax));
                if (blank)
                    ymax--;
            } while (blank);

            return new CraftingPattern(area, xmin, xmax, ymin, ymax);
        }
        #endregion  // Construction

        public int Width { get => _pattern.GetLength(0); }

        public int Height { get => _pattern.GetLength(1); }

        public ItemStack this[int x, int y] { get => _pattern[x, y]; }

        #region object overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as CraftingPattern);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int xul = _pattern.GetLength(0);
                int yul = _pattern.GetLength(1);
                int rv = 3 * xul;
                rv += 5 * yul;

                for (int x = 0; x < xul; x++)
                    for (int y = 0; y < yul; y++)
                        rv ^= _pattern[x, y].GetHashCode();

                return rv;
            }
        }
        #endregion

        #region IEquatable<CraftingPattern> & related
        public bool Equals(CraftingPattern other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            if (_pattern.GetLength(0) != other._pattern.GetLength(0) ||
                _pattern.GetLength(1) != other._pattern.GetLength(1))
                return false;

            // NOTE: the equality of ItemStacks includes metadata.
            //     This may or may not be a problem.
            for (int x = 0, xul = _pattern.GetLength(0); x < xul; x++)
                for (int y = 0, yul = _pattern.GetLength(1); y < yul; y++)
                    if (_pattern[x, y] != other._pattern[x, y])
                        return false;

            return true;
        }

        public static bool operator==(CraftingPattern l, CraftingPattern r)
        {
            if (!object.ReferenceEquals(l, null))
            {
                if (!object.ReferenceEquals(r, null))
                    return l.Equals(r);
                else
                    return false;
            }
            else
            {
                if (!object.ReferenceEquals(r, null))
                    return false;
                else
                    return true;
            }
        }

        public static bool operator!=(CraftingPattern l, CraftingPattern r)
        {
            return !(l == r);
        }
        #endregion
    }
}
