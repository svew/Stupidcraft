using System;

namespace TrueCraft.Core.Utility
{
    /// <summary>
    /// A utility class for caching small numbers of items by their metadata.
    /// </summary>
    /// <typeparam name="T">The type of item to be cached.</typeparam>
    public class CacheEntry<T>
    {
        private readonly T _cachedItem;
        private readonly short _metadata;
        private CacheEntry<T>? _next;

        /// <summary>
        /// Constructs a new CacheEntry object.
        /// </summary>
        /// <param name="cachedItem">The item to be cached.</param>
        /// <param name="metadata">The metadata to be used to retrieve the cached item.</param>
        public CacheEntry(T cachedItem, short metadata)
        {
            _cachedItem = cachedItem;
            _metadata = metadata;
            _next = null;
        }

        /// <summary>
        /// Gets the Value cached in this CacheEntry
        /// </summary>
        public T Value { get => _cachedItem; }

        /// <summary>
        /// Gets the metadata that identifies this CacheEntry.
        /// </summary>
        public short Metadata { get => _metadata; }

        /// <summary>
        /// Gets the next entry in the Cache.
        /// </summary>
        public CacheEntry<T>? Next { get => _next; }

        /// <summary>
        /// Appends the item to be cached to this cache.
        /// </summary>
        /// <param name="cachedItem">The item to be cached.</param>
        /// <param name="metadata">The metadata to be used to retrieve the item.</param>
        public void Append(T cachedItem, short metadata)
        {
            CacheEntry<T> last = this;
            while (last._next != null)
                last = last._next;

            last._next = new CacheEntry<T>(cachedItem, metadata);
        }

        /// <summary>
        /// Finds the CacheEntry within this list with matching Metadata.
        /// </summary>
        /// <param name="metadata">The metadata to match</param>
        /// <returns>The matching CacheEntry or this if there is no match.</returns>
        /// <remarks>
        /// Call this method on the head of the list.  If no matching entry
        /// is found, the this object will be returned as a default.
        /// </remarks>
        public CacheEntry<T> Find(short metadata)
        {
            CacheEntry<T>? rv = this;
            while (rv._metadata != metadata && rv._next != null)
                rv = rv._next;
            if (rv is null || rv._metadata != metadata)
                return this;
            else
                return rv;
        }
    }
}
