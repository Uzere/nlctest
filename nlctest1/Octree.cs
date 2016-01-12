using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// эта штука вообще не работает
namespace nlctest1 {
    class Octree {
        const int chunkSize = 64;
        const uint NOT_A_LEAF = 0xFFFFFF00;
        const uint NOT_LOADED = 0xFFFFFF01;
        const int baseTreeSize = 100000; // 1..299593 blocks
        const int maxTreeSize = 299593; // in blocks
        const int sizeOfNotLastLevels = 1 + 8 + 64 + 512 + 4096 + 32768; // in blocks

        protected uint[] tree;

        public Octree() {
            throw new NotImplementedException();
            /*tree = new uint[baseTreeSize];
            tree[0] = 0; // set the whole chunk to null-blocks*/
        }

        public Octree(byte[] buffer, int start, int len) {
            throw new NotImplementedException();
            /*tree = new uint[maxTreeSize];
            for(var i = 0; i < sizeOfNotLastLevels; i++) {
                tree[i] = NOT_A_LEAF;
            }

            for(var by = 0; by < chunkSize; by++) {
                for(var bz = 0; bz < chunkSize; bz++) {
                    for(var bx = 0; bx < chunkSize; bx++) {
                        tree[sizeOfNotLastLevels + chunkCoordsToZCurve(bx, by, bz)] = 
                            buffer[start + chunkCoordsToIndex(bx, by, bz)];
                    }
                }
            }

            compactTree();*/
        }

        int chunkCoordsToIndex(int x, int y, int z) {
            return y * chunkSize * chunkSize + z * chunkSize + x;
        }

        int chunkCoordsToZCurve(int x, int y, int z) {
            x = (x | (x << 16)) & 0x030000FF;
            x = (x | (x << 8)) & 0x0300F00F;
            x = (x | (x << 4)) & 0x030C30C3;
            x = (x | (x << 2)) & 0x09249249;

            y = (y | (y << 16)) & 0x030000FF;
            y = (y | (y << 8)) & 0x0300F00F;
            y = (y | (y << 4)) & 0x030C30C3;
            y = (y | (y << 2)) & 0x09249249;

            z = (z | (z << 16)) & 0x030000FF;
            z = (z | (z << 8)) & 0x0300F00F;
            z = (z | (z << 4)) & 0x030C30C3;
            z = (z | (z << 2)) & 0x09249249;

            return x | (y << 1) | (z << 2);
        }

        delegate int LevelOffset(int l);
        private void compactTree() {
            var levelSize = new int[7];
            LevelOffset levelOffset = (l) => { var s = 0; for(var i = 0; i < l; i++) { s += levelSize[i]; }; return s; };
            var level = 0;
            levelSize[0] = 1;
            var index = 0;
            while(true) {
                if(index == levelOffset(level + 1)) {
                    level++;
                }
                if(level > 6) {
                    return;
                }

                if(tree[index]==NOT_A_LEAF) {
                    levelSize[level + 1] += 8;
                }

                index++;

            }
        }

        byte[] toByteArray() {
            return new byte[64 * 64 * 64 * 4];////
        }

        uint getBlock(int x, int y, int z) {
            return 0;////
        }

        void setBlock(int x, int y, int z, uint block) {

        }

        bool softSetBlock(int x, int y, int z, uint block) {
            return false;////
        }
    }
}
