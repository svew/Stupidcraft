using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Test
{
    [SetUpFixture]
    public class CoreSetup
    {
        // The test framework will make calls to this class that will
        // initialize these members prior to running tests.  So we can get
        // away with "faking" them as non-nullable.
        private static IBlockRepository _blockRepository = null!;
        private static IItemRepository _itemRepository = null!;
        private static ICraftingRepository _craftingRepository = null!;

        public CoreSetup()
        {
        }

        // BlockProviderTest, WorldLighterTest, PhysicsEngineTest, and CraftingAreaTest depend upon
        // having some blocks and items available in their repositories.
        private class MockDiscover : IDiscover
        {
            public void DiscoverBlockProviders(IRegisterBlockProvider repository)
            {
                repository.RegisterBlockProvider(new GrassBlock());
                repository.RegisterBlockProvider(new DirtBlock());
                repository.RegisterBlockProvider(new StoneBlock());
                repository.RegisterBlockProvider(new AirBlock());
                repository.RegisterBlockProvider(new BedrockBlock());
                repository.RegisterBlockProvider(new LeavesBlock());
                repository.RegisterBlockProvider(new CobblestoneBlock());
            }

            public void DiscoverItemProviders(IRegisterItemProvider repository)
            {
                repository.RegisterItemProvider(new LavaBlock());  // Item ID 10
                repository.RegisterItemProvider(new SandBlock());  // Item ID 12
                repository.RegisterItemProvider(new StoneBlock()); // Item ID 1
                repository.RegisterItemProvider(new GrassBlock()); // Item ID 2
                repository.RegisterItemProvider(new DirtBlock());  // Item ID 3
                repository.RegisterItemProvider(new CobblestoneBlock());  // Item ID 4

                string xmlSnowBall = @"    <item>
      <id>332</id>
      <maximumstack>16</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Snowball</displayname>
        <icontexture>
          <x>14</x>
          <y>0</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
    </item>
";
                repository.RegisterItemProvider(new SnowballItem(GetTopNode(xmlSnowBall)));
            }

            private static XmlNode GetTopNode(string xml)
            {
                XmlDocument doc = new XmlDocument();
                using (StringReader sr = new StringReader(xml))
                using (XmlReader xmlr = XmlReader.Create(sr))
                    doc.Load(xmlr);

                return doc.FirstChild!;
            }

            public void DiscoverRecipes(IRegisterRecipe repository)
            {
                string[] recipeXML = new string[]
                {
                    // Sticks
                    @"<recipe>
      <pattern>
        <r>
          <c>
            <id>5</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>5</id>
            <count>1</count>
          </c>
        </r>
      </pattern>
      <output>
        <id>280</id>
        <count>4</count>
      </output>
    </recipe>
",
                    // Stone shovel
                    @"<recipe>
      <pattern>
        <r>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
        </r>
      </pattern>
      <output>
        <id>273</id>
        <count>1</count>
      </output>
    </recipe>
",
                    // Stone Hoe
                    @"<recipe>
      <pattern>
        <r>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
        </r>
      </pattern>
      <output>
        <id>291</id>
        <count>1</count>
      </output>
    </recipe>
",
                    // Stone PickAxe
                    @"<recipe>
      <pattern>
        <r>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
          <c>
            <id>4</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
        </r>
        <r>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
          <c>
            <id>280</id>
            <count>1</count>
          </c>
          <c>
            <id>-1</id>
            <count>1</count>
          </c>
        </r>
      </pattern>
      <output>
        <id>274</id>
        <count>1</count>
      </output>
    </recipe>"
                };

                for (int j = 0; j < recipeXML.Length; j ++)
                {
                    XmlDocument doc= new XmlDocument();
                    doc.LoadXml(recipeXML[j]);
                    XmlNode item = doc.DocumentElement!;

                    repository.RegisterRecipe(new CraftingRecipe(item));
                }
            }
        }

        public static IBlockRepository BlockRepository { get => _blockRepository; }
        public static IItemRepository ItemRepository { get => _itemRepository; }
        public static ICraftingRepository CraftingRepository { get => _craftingRepository; }

        [OneTimeSetUp]
        public void SetupRepositories()
        {
            IDiscover discover = new MockDiscover();
            _blockRepository = TrueCraft.Core.Logic.BlockRepository.Init(discover);
            _itemRepository = TrueCraft.Core.Logic.ItemRepository.Init(discover);
            _craftingRepository =  TrueCraft.Core.Logic.CraftingRepository.Init(discover);
        }
    }
}
