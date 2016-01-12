using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlctest1 {
    // чанк на основе массива, никакого rocket science
    class PlainChunk : Chunk {
        private uint[] blocks;

        PlainChunk() {
            blocks = new uint[chunkSize * chunkSize * chunkSize];
        }

        public override byte[] toByteArray() {
            byte[] result = new byte[blocks.Length * sizeof(int)];
            Buffer.BlockCopy(blocks, 0, result, 0, result.Length);
            return result;
        }

        public override uint getBlock(int x, int y, int z) {
            return blocks[index(x, y, z)];
        }

        public override void setBlock(int x, int y, int z, uint block) {
            blocks[index(x, y, z)] = block;
        }
    }
}
