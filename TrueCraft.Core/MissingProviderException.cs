using System;

namespace TrueCraft.Core
{
    /// <summary>
    /// Throw an Exception of this Type if a provider class could not be found
    /// </summary>
    public class MissingProviderException : ApplicationException
    {
        /// <summary>
        /// Constructs the MissingProviderException
        /// </summary>
        /// <param name="missingTypeName">The fully qualified name of the missing provider class.</param>
        public MissingProviderException(string missingTypeName) :
            base($"The provider of type \"{missingTypeName}\" could not be found.")
        {
        }
    }
}
