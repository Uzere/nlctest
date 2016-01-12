using System.IO;

namespace nlctest1 {
    public class BinaryWriter7 : BinaryWriter {
        public BinaryWriter7(Stream stream) : base(stream) { }
        public new void Write7BitEncodedInt(int i) {
            base.Write7BitEncodedInt(i);
        }
    }
}
