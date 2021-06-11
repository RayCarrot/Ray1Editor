using System.Text;
using BinarySerializer;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A wrapper for serializable file data to be used in the editor. This allows additional data to be appended to the file.
    /// </summary>
    /// <typeparam name="T">The file data type</typeparam>
    public class SerializableEditorFile<T> : BinarySerializable
        where T : BinarySerializable, new()
    {
        public const string Magic = "EDIT";

        public T FileData { get; set; }
        public int EditorVersion { get; set; } = 1;
        public string Message { get; set; } = $"Ray1Editor v. {AppViewModel.Instance.CurrentAppVersion}";
        public int RelocatedStructsCount { get; set; }
        public RelocatedStruct[] RelocatedStructs { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            var hasRead = FileData != null;

            FileData = s.SerializeObject<T>(FileData, name: nameof(FileData));

            if (s.CurrentFileOffset >= s.CurrentLength && !hasRead)
                return;

            var footerOffset = s.CurrentPointer.FileOffset;

            EditorVersion = s.Serialize<int>(EditorVersion, name: nameof(EditorVersion));

            if (EditorVersion != 1)
            {
                s.LogWarning($"Unknown editor version {EditorVersion}");
                return;
            }

            Message = s.SerializeString(Message, encoding: Encoding.UTF8, name: nameof(Message));
            RelocatedStructsCount = s.Serialize<int>(RelocatedStructsCount, name: nameof(RelocatedStructsCount));
            RelocatedStructs = s.SerializeObjectArray<RelocatedStruct>(RelocatedStructs, RelocatedStructsCount, name: nameof(RelocatedStructs));

            // End with the footer offset and magic. This way the footer can be read without first parsing the file.
            s.Serialize<long>(footerOffset, name: nameof(footerOffset));
            var magic = s.SerializeString(Magic, 4, name: nameof(Magic));

            if (Magic != magic)
                s.LogWarning($"Unknown magic identifier {magic}");
        }

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
}