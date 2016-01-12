using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlctest1 {
    abstract class Chunk {
        public const int chunkSize = 64;

        public abstract byte[] toByteArray();

        public abstract uint getBlock(int x, int y, int z);

        public abstract void setBlock(int x, int y, int z, uint block);

        protected static int index(int x, int y, int z) {
            return chunkSize * chunkSize * y + chunkSize * z + x;
        }
    }
}
