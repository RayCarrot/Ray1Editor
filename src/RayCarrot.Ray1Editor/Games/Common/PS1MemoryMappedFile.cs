using BinarySerializer;

namespace RayCarrot.Ray1Editor
{
    public class PS1MemoryMappedFile : MemoryMappedFile
    {
        public PS1MemoryMappedFile(Context context, string filePath, uint baseAddress, InvalidPointerMode currentInvalidPointerMode, Endian endianness = Endian.Little, long fileLength = 0) : base(context, filePath, baseAddress, endianness, fileLength)
        {
            CurrentInvalidPointerMode = currentInvalidPointerMode;
        }

        public InvalidPointerMode CurrentInvalidPointerMode { get; }

        private bool CheckIfDevPointer(uint serializedValue, Pointer anchor = null)
        {
            var anchorOffset = anchor?.AbsoluteOffset ?? 0;
            var offset = serializedValue + anchorOffset;
            offset ^= 0xFFFFFFFF;
            return offset is >= 0x80000000 and < 0x807FFFFF;
        }
        public override bool AllowInvalidPointer(long serializedValue, Pointer anchor = null)
        {
            return CurrentInvalidPointerMode switch
            {
                InvalidPointerMode.DevPointerXOR => CheckIfDevPointer((uint)serializedValue, anchor: anchor),
                InvalidPointerMode.Allow => true,
                _ => true
            };
        }

        public enum InvalidPointerMode
        {
            DevPointerXOR,
            Allow
        }
    }
}