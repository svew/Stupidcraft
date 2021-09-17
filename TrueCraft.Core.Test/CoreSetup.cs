using System;
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
        public CoreSetup()
        {
        }

        // BlockProviderTest, WorldLighterTest and PhysicsEngineTest depend upon
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
            }

            public void DiscoverItemProviders(IRegisterItemProvider repository)
            {
                repository.RegisterItemProvider(new LavaBlock());  // Item ID 10
                repository.RegisterItemProvider(new SandBlock());  // Item ID 12
                repository.RegisterItemProvider(new StoneBlock()); // Item ID 1
                repository.RegisterItemProvider(new GrassBlock()); // Item ID 2
                repository.RegisterItemProvider(new DirtBlock());  // Item ID 3
                repository.RegisterItemProvider(new SnowballItem());
            }

            public void DiscoverRecipes(IRegisterRecipe repository)
            {
                XmlDocument doc= new XmlDocument();
                doc.LoadXml(@"<recipe>
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
");
                XmlNode sticks = doc.DocumentElement;

                repository.RegisterRecipe(new CraftingRecipe(sticks));
            }
        }

        [OneTimeSetUp]
        public void SetupRepositories()
        {
            IDiscover discover = new MockDiscover();
            BlockRepository.Init(discover);
            ItemRepository.Init(discover);
            CraftingRepository.Init(discover);
        }
    }
}
