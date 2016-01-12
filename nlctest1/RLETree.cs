using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nlctest1 {
    class RLETree : Chunk {
        private RandomTree tree = new RandomTree();
        const int RUN_LENGTH_SHIFT = 7;

        public RLETree() {
            // 64^3 = 262 144 = 00000000 00000100 00000000 00000000 = 0000 0000000 0010000 0000000 0000000
            // 10000000 10000000 00010000
            var data = new byte[32] {
                (1<<7)+RUN_LENGTH_SHIFT, 1<<7, 1<<4, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
            }; // fill air
            tree.Insert(0 /* offset */, data);
        }

        public RLETree(byte[] buffer, int offset) {
            var off = 0;
            var len = 0;
            var data = new byte[0];

            foreach(var run in RLEEncode(buffer, offset)) {
                var newlen = run.Item1;
                var newdata = run.Item2.ToArray();

                if(data.Length+newdata.Length<=32) {
                    data = data.Concat(newdata).ToArray();
                    len += newlen;
                } else {
                    tree.Insert(off, data);
                    off += len;
                    data = newdata;
                    len = newlen;
                }
            }
            tree.Insert(off, data);

        }


        public override byte[] toByteArray() {
            throw new NotImplementedException();
            //return new byte[1];
        }

        public override uint getBlock(int x, int y, int z) {
            var ind = index(x, y, z);
            var node = tree.FindGreatestNotGreater(ind);
            var offset = node.key;
            var runLen = 0;

            using(var reader = new BinaryReader7(new MemoryStream(node.data, 0, node.data.Length))) {
                while(true) {
                    if(runLen == 0) {
                        runLen = reader.Read7BitEncodedInt() - RUN_LENGTH_SHIFT;
                    } else if(runLen>0) {
                        var runId = reader.ReadUInt32();
                        // blocks in range [offset...offset+runLen) is runId
                        offset = offset + runLen;
                        if(ind < offset) {
                            return runId;
                        }
                        runLen = 0;
                    } else if(runLen<0) {
                        var blockId = reader.ReadUInt32();
                        if(offset==ind) {
                            return blockId;
                        }
                        runLen++;
                        offset++;
                    }
                }
            }
        }

        public override void setBlock(int x, int y, int z, uint block) {
            var ind = index(x, y, z);
            var node = tree.FindGreatestNotGreater(ind);
            var offset = node.key;

            var runLen = 0;
            var runLenWrited = false;

            MemoryStream output = null;
            using (var reader = new BinaryReader7(new MemoryStream(node.data, 0, node.data.Length))) {
                using(var writer = new BinaryWriter7(output = new MemoryStream())) {
                    while (reader.BaseStream.Position != reader.BaseStream.Length) {
                        if (runLen == 0) {
                            runLen = reader.Read7BitEncodedInt() - RUN_LENGTH_SHIFT;
                            runLenWrited = false;
                        } else if (runLen > 0) {
                            var runId = reader.ReadUInt32();
                            // blocks in range [offset...offset+runLen) is runId
                            offset = offset + runLen;
                            if (ind < offset) {
                                writer.Write7BitEncodedInt(ind - (offset - runLen) + RUN_LENGTH_SHIFT);
                                writer.Write(runId);
                                writer.Write7BitEncodedInt(1 + RUN_LENGTH_SHIFT);
                                writer.Write(block);
                                if (offset - ind -1 > 0) {
                                    writer.Write7BitEncodedInt(offset - ind -1 + RUN_LENGTH_SHIFT);
                                    writer.Write(runId);
                                }
                            } else {
                                writer.Write7BitEncodedInt(runLen + RUN_LENGTH_SHIFT);
                                writer.Write(runId);
                            }
                            runLen = 0;
                        } else if (runLen < 0) {
                            var blockId = reader.ReadUInt32();

                            if(!runLenWrited) {
                                writer.Write7BitEncodedInt(runLen + RUN_LENGTH_SHIFT);
                                runLenWrited = true;
                            }
                            if (offset == ind) {
                                writer.Write(block);
                            } else {
                                writer.Write(blockId);
                            }
                            runLen++;
                            offset++;
                        }
                    }
                }
            }

            var newdata = output.ToArray();
            node.data = output.ToArray();
        }


        private IEnumerable<Tuple<int, MemoryStream>> RLEEncode(byte[] data, int offset) {
            // Этот метод может казаться страшным,
            // но если долго на него смотреть, то можно увидеть конечный автомат
            // очень долго

            using(var reader = new BinaryReader7(new MemoryStream(data, offset, 1024 * 1024))) {
                var tempIds = new uint[RUN_LENGTH_SHIFT];
                int runLen = 0;
                uint runId = 0;
                while(reader.BaseStream.Position != reader.BaseStream.Length) {
                    var id = SwapEndian(reader.ReadUInt32());
                    if(runLen == 0) {
                        runId = id;
                        runLen++;
                    } else if(runLen < 0) {
                        if(id == tempIds[-runLen]) {
                            runLen++;
                            yield return RLEEncodeAddRun(runLen, tempIds);
                            runLen = 2;
                            runId = id;
                        } else {
                            if(runLen == -(RUN_LENGTH_SHIFT - 1)) {
                                yield return RLEEncodeAddRun(runLen, tempIds);
                                runLen = 1;
                                runId = id;
                            } else {
                                runLen--;
                                tempIds[-runLen] = id;
                            }
                        }
                    } else if(runLen == 1 && runId != id) {
                        runLen = -2;
                        tempIds[1] = runId;
                        tempIds[2] = id;
                    } else if(runLen > 0) {
                        if(id == runId) {
                            runLen++;
                        } else {
                            yield return RLEEncodeAddRun(runLen, runId);
                            runLen = 1;
                            runId = id;
                        }
                    }
                }

                if (runLen > 0) {
                    yield return RLEEncodeAddRun(runLen, runId);
                } else if(runLen<0) {
                    yield return RLEEncodeAddRun(runLen, tempIds);
                }
            }
        }

        private static Tuple<int, MemoryStream> RLEEncodeAddRun(int runLen, uint runId) {
            var output = new MemoryStream();
            using (var writer = new BinaryWriter7(output)) {
                writer.Write7BitEncodedInt(RUN_LENGTH_SHIFT + runLen);
                writer.Write(runId);
            }
            return new Tuple<int, MemoryStream>(runLen, output);
        }

        private static Tuple<int, MemoryStream> RLEEncodeAddRun(int runLen, uint[] runIds) {
            var output = new MemoryStream();
            using (var writer = new BinaryWriter7(output)) {
                writer.Write7BitEncodedInt(RUN_LENGTH_SHIFT + (runLen == -1 ? 1 : runLen));
                for (var i = -1; i >= runLen; i--) {
                    writer.Write(runIds[-i]);
                }
            }
            return new Tuple<int, MemoryStream>(Math.Abs(runLen), output);
        }

        private static uint SwapEndian(uint n) {
            var temp = BitConverter.GetBytes(n);
            Array.Reverse(temp);
            return BitConverter.ToUInt32(temp, 0);
        }

    }
}
