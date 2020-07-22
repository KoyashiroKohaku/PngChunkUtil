using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KoyashiroKohaku.PngChunkUtil.Test
{
    [TestClass]
    public class PngTest
    {
        private static readonly byte[] _invalidImage = File.ReadAllBytes(@"Assets/invalid.jpg");
        private static readonly byte[] _validImage = File.ReadAllBytes(@"Assets/valid.png");

        private static IEnumerable<object[]> InvalidPngs => new object[][]
        {
            new object[] { Array.Empty<byte>() },
            new object[] { new byte[1] },
            new object[] { new byte[2] },
            new object[] { new byte[3] },
            new object[] { new byte[4] },
            new object[] { new byte[5] },
            new object[] { new byte[6] },
            new object[] { new byte[7] },
            new object[] { _invalidImage }
        };

        private static IEnumerable<object[]> ValidPngs => new object[][]
        {
            new object[] { _validImage }
        };

        private static IEnumerable<object[]> InvalidChunks => new object[][]
        {
            new object[] { Array.Empty<Chunk>() },
            new object[] { new Chunk[1] },
            new object[] { new Chunk[2] },
            new object[] { new Chunk[3] },
            new object[] { new Chunk[4] }
        };

        private static IEnumerable<object[]> ValidChunks => new object[][]
        {
            new object[] { Png.SplitIntoChunks(_validImage).ToArray() }
        };

        [TestMethod]
        [TestCategory(nameof(Png.SplitIntoChunks))]
        public void SplitIntoChunks_InputIsNull_ThrowArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Png.SplitIntoChunks(null));
        }

        [DataTestMethod]
        [DynamicData(nameof(InvalidPngs))]
        [TestCategory(nameof(Png.SplitIntoChunks))]
        public void SplitIntoChunks_InputIsInvalid_ThrowArgumentException(byte[] image)
        {
            Assert.ThrowsException<ArgumentException>(() => Png.SplitIntoChunks(image));
        }

        [DataTestMethod]
        [DynamicData(nameof(ValidPngs))]
        [TestCategory(nameof(Png.SplitIntoChunks))]
        public void SplitIntoChunks_InputIsValid_ReturnChunks(byte[] image)
        {
            var chunks = Png.SplitIntoChunks(image).ToArray();

            Assert.IsTrue(chunks.Any());
            Assert.IsTrue(chunks.All(c => c.IsValid()));
            Assert.AreEqual("IHDR", chunks.First().TypeString);
            Assert.IsTrue(chunks.Any(c => c.TypeString == "IDAT"));
            Assert.AreEqual("IEND", chunks.Last().TypeString);
        }

        [DataTestMethod]
        [TestCategory(nameof(Png.JoinToPng))]
        public void JoinToPng_InputIsNull_ThrowArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Png.JoinToPng(null));
        }

        [DataTestMethod]
        [DynamicData(nameof(InvalidChunks))]
        [TestCategory(nameof(Png.JoinToPng))]
        public void JoinToPng_InputIsInvalid_ThrowArgumentException(Chunk[] chunks)
        {
            Assert.ThrowsException<ArgumentException>(() => Png.JoinToPng(chunks));
        }

        [DataTestMethod]
        [DynamicData(nameof(ValidChunks))]
        [TestCategory(nameof(Png.JoinToPng))]
        public void JoinToPng_InputIsValid_ReturnPngArray(Chunk[] chunks)
        {
            var joinedPng = Png.JoinToPng(chunks);
            var resplittedChunks = Png.SplitIntoChunks(joinedPng).ToArray();

            Assert.AreEqual(chunks.Length, resplittedChunks.Length);

            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var resplittedChunk = resplittedChunks[i];

                Assert.IsTrue(chunk.Value.SequenceEqual(resplittedChunk.Value));
            }
        }
    }
}
