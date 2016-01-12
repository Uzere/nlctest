using fNbt;
using System;
using System.Collections.Generic;
using System.IO;

namespace nlctest1 {
    class Program {
        static byte[] chunks = new byte[32 * 1024 * 1024];
        static string saveFolder = "..\\..\\..\\";
        static string region = "..\\..\\..\\r.-2.1.mca";

        static void Main(string[] args) {
            // получение тестовых данных
            GetData(region, saveFolder);

            if (args.Length == 2) {
                EvalScript(args[0], args[1]);
            } else {
                Console.WriteLine("Usage: nlctest1 inputFilename outputFilename");
            }
        }

        // обработка входного файла
        static void EvalScript(string inputFilename, string outputFilename) {
            RLETree chunk = null;
            var results = new List<string>();

            string[] lines;
            try {
                lines = File.ReadAllLines(inputFilename);
            } catch (FileNotFoundException) {
                ScriptFail(outputFilename, "Input file does not exist");
                return;
            }

            try {
                foreach (var line in lines) {
                    var tokens = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if(tokens.Length==0) {
                        continue;
                    }

                    switch (tokens[0]) {
                        case "load":
                            chunk = new RLETree(chunks, int.Parse(tokens[1]) * 1024 * 1024);
                            break;
                        case "get":
                            var x = int.Parse(tokens[1]);
                            var y = int.Parse(tokens[2]);
                            var z = int.Parse(tokens[3]);

                            var id = chunk.getBlock(x, y, z);
                            results.Add(id.ToString());
                            break;
                        case "set":
                            x = int.Parse(tokens[1]);
                            y = int.Parse(tokens[2]);
                            z = int.Parse(tokens[3]);
                            id = uint.Parse(tokens[4]);

                            chunk.setBlock(x, y, z, id);
                            break;
                        default:
                            ScriptFail(outputFilename, "Syntax error: unknown command");
                            return;
                    }
                }
            } catch (IndexOutOfRangeException) {
                ScriptFail(outputFilename, "Syntax error: wrong argument count");
                return;
            } catch (NullReferenceException) {
                ScriptFail(outputFilename, "Runtime error: no chunk loaded");
                return;
            } catch (FormatException) {
                ScriptFail(outputFilename, "Syntax error: wrong argument");
                return;
            } catch (OverflowException) {
                ScriptFail(outputFilename, "Syntax error: wrong argument");
                return;
            } catch (Exception e) {
                ScriptFail(outputFilename, "Unknown error: " + e.ToString());
                return;
            }

            File.WriteAllLines(outputFilename, results.ToArray());
            // длинный метод, на самом деле
        }

        static private void ScriptFail(string outputFilename, string reason) {
            File.WriteAllText(outputFilename, reason);
        }

        // меняет endianness значения
        static uint SwapEndian(uint n) {
            var temp = BitConverter.GetBytes(n);
            Array.Reverse(temp);
            return BitConverter.ToUInt32(temp, 0);
        }

        // страшная вещь
        static void GetData(string from, string to) {
            // эту функцию не следует вообще смотреть
            // страшный, одноразовый, write-only код, задача которого 1 раз распарсить mca файл и самоуничтожиться
            // однако, последняя функция не сработала, поэтому он всё ещё здесь
            var file = File.ReadAllBytes(from);
            for (var chx = 0; chx < 16; chx++) {
                for (var chz = 0; chz < 24; chz++) {
                    var index = SwapEndian(BitConverter.ToUInt32(file, 4 * (32 * chz + chx)));

                    var size = index & 0xFF;
                    var offset = (index >> 8) * 4096; // тут даже есть куча магических констант
                    //Console.WriteLine("{0} {1} {2} {3}", chx, chz, offset, size);
                    //Console.SetCursorPosition(chx, chz);
                    //Console.WriteLine("{0}", size);

                    if (offset != 0 && size != 0) {
                        var nbt = new NbtFile();
                        var length = SwapEndian(BitConverter.ToUInt32(file, (int)offset));
                        nbt.LoadFromBuffer(file, (int)offset + 5, (int)length, NbtCompression.AutoDetect);

                        if (chz < 16) {
                            continue;
                        }

                        var sections = nbt.RootTag.Get<NbtCompound>("Level").Get<NbtList>("Sections");
                        foreach (var section in sections) {
                            //Console.Write("{0}", ((NbtCompound)section).Get<NbtByte>("Y").Value // о, отладочный вывод
                            var sy = ((NbtCompound)section).Get<NbtByte>("Y").Value;
                            var blocks = ((NbtCompound)section).Get<NbtByteArray>("Blocks").Value;
                            for (var by = 0; by < 16; by++) {
                                for (var bz = 0; bz < 16; bz++) {
                                    for (var bx = 0; bx < 16; bx++) {
                                        var gx = chx * 16 + bx;
                                        var gy = sy * 16 + by;
                                        var gz = (chz - 16) * 16 + bz;
                                        chunks[
                                            ((gy / 64) * 4 * 2 + (gz / 64) * 4 + (gx / 64)) * 64 * 64 * 64 * 4 // непонятные формулы
                                            + (64 * 64 * (gy % 64) + 64 * (gz % 64) + (gx % 64)) * 4 // неведомые константы
                                            + 3
                                            ] = blocks[by * 16 * 16 + bz * 16 + bx];
                                        //if(gy==0)Console.WriteLine("{0} {1} {2}", gx, gy, gz); // мм, комментарии с кодом
                                        /* if(gy==0 && gx<32 && gz<32) {
                                             Console.SetCursorPosition(gx, gz);
                                             Console.WriteLine("{0}", blocks[by * 16 * 16 + bz * 16 + bx]%10);
                                         }*/
                                        if (gy == 0 && blocks[by * 16 * 16 + bz * 16 + bx] != 7) {
                                            //Console.WriteLine("{0} {1} {2}", gx, gy, gz);
                                        }
                                    }
                                }

                            }
                        }

                    }

                }
            }

            File.WriteAllBytes(to + "1.bin", chunks);
        }
    }

}
