using System;
using NUnit.Framework;
using TrueCraft.Core.Lighting;

namespace TrueCraft.Core.Test.Lighting
{
    public class TestBool3D
    {
        public TestBool3D()
        {
        }

        [Test]
        public void ctor_true()
        {
            int xsize = 30;
            int ysize = 40;
            int zsize = 50;
            Bool3D tst = new Bool3D(xsize, ysize, zsize, true);

            for (int x = 0; x < xsize; x++)
                for (int y = 0; y < ysize; y++)
                    for (int z = 0; z < zsize; z++)
                        Assert.True(tst[x, y, z]);
        }

        [Test]
        public void ctor_false()
        {
            int xsize = 20;
            int ysize = 30;
            int zsize = 35;
            Bool3D tst = new Bool3D(xsize, ysize, zsize, false);

            for (int x = 0; x < xsize; x++)
                for (int y = 0; y < ysize; y++)
                    for (int z = 0; z < zsize; z++)
                        Assert.False(tst[x, y, z]);
        }

        [Test]
        public void ctor_size()
        {
            int xsize = 30;
            int ysize = 40;
            int zsize = 50;
            Bool3D tst = new Bool3D(xsize, ysize, zsize, true);

            Assert.AreEqual(xsize, tst.XSize);
            Assert.AreEqual(ysize, tst.YSize);
            Assert.AreEqual(zsize, tst.ZSize);
        }

        [Test]
        public void Indexer_Throws()
        {
            int xsize = 15;
            int ysize = 25;
            int zsize = 35;

            Bool3D tst = new Bool3D(xsize, ysize, zsize, false);

            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[-1, 0, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[0, -1, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[0, 0, -1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[xsize, 0, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[0, ysize, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { bool b = tst[0, 0, zsize]; });

            Assert.DoesNotThrow(() => { bool b = tst[0, 0, 0]; });
            Assert.DoesNotThrow(() => { bool b = tst[xsize - 1, ysize - 1, zsize - 1]; });
        }

        [Test]
        public void Indexer()
        {
            int xsize = 13;
            int ysize = 17;
            int zsize = 19;
            Random rnd = new Random(123);

            // Test that a bit is successfully set and retrieved.
            // No other bits are altered.
            for (int j = 0; j < 50; j++)
            {
                Bool3D tst = new Bool3D(xsize, ysize, zsize, false);
                int x1 = rnd.Next(xsize);
                int y1 = rnd.Next(ysize);
                int z1 = rnd.Next(zsize);
                tst[x1, y1, z1] = true;
                for (int x = 0; x < xsize; x++)
                    for (int y = 0; y < ysize; y++)
                        for (int z = 0; z < zsize; z++)
                            Assert.AreEqual(x == x1 && y == y1 && z == z1, tst[x, y, z]);
            }

            // Test that a bit is successfully cleared.
            // No other bits are altered.
            for (int j = 0; j < 50; j++)
            {
                Bool3D tst = new Bool3D(xsize, ysize, zsize, true);
                int x1 = rnd.Next(xsize);
                int y1 = rnd.Next(ysize);
                int z1 = rnd.Next(zsize);
                tst[x1, y1, z1] = false;
                for (int x = 0; x < xsize; x++)
                    for (int y = 0; y < ysize; y++)
                        for (int z = 0; z < zsize; z++)
                            Assert.AreEqual(x != x1 || y != y1 || z != z1, tst[x, y, z]);
            }
        }
    }
}
