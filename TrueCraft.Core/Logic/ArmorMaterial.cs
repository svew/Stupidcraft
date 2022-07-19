using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.Core
{
    /// <summary>
    /// Enumerates the materials armor can be crafted from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the values in this enum must correspond to values in the
    /// armorMaterialType in the TrueCraft.xsd.
    /// </para>
    /// </remarks>
    public enum ArmorMaterial
    {
        /// <summary>
        /// The armor is made of leather.
        /// </summary>
        Leather,

        /// <summary>
        /// The armor is made of chain (fire).
        /// </summary>
        Chain,

        /// <summary>
        /// The armor is made of iron ingots.
        /// </summary>
        Iron,

        /// <summary>
        /// The armor is made of gold ingots.
        /// </summary>
        Gold,

        /// <summary>
        /// The armor is made of diamonds.
        /// </summary>
        Diamond
    }
}
