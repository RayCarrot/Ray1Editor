using BinarySerializer;

namespace RayCarrot.Ray1Editor
{
    public class RelocatedStruct : BinarySerializable
    {
        public Pointer OriginalPointer { get; set; }
        public Pointer NewPointer { get; set; }
        public uint DataSize { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            OriginalPointer = s.SerializePointer(OriginalPointer, name: nameof(OriginalPointer));
            NewPointer = s.SerializePointer(NewPointer, name: nameof(NewPointer));
            DataSize = s.Serialize<uint>(DataSize, name: nameof(DataSize));
        }
    }
}