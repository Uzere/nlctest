using System.IO;

namespace nlctest1 {

    public class BinaryReader7 : BinaryReader {
        public BinaryReader7(Stream stream) : base(stream) { }
        public new int Read7BitEncodedInt() {
            return base.Read7BitEncodedInt();
        }
    }
}