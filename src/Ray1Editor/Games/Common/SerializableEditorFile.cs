using System;
using System.Collections.Generic;
using System.Text;
using BinarySerializer;

namespace Ray1Editor;

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
    public string Message { get; set; } = $"Ray1Editor v. {R1EServices.App.CurrentAppVersion}";
    public int RelocatedStructsCount { get; set; }
    public RelocatedStruct[] RelocatedStructs { get; set; }

    /// <summary>
    /// Optional serialization actions to perform at specific offsets
    /// </summary>
    public Dictionary<Pointer, Action<SerializerObject>> AdditionalSerializationActions { get; set; } = new Dictionary<Pointer, Action<SerializerObject>>();

    public override void SerializeImpl(SerializerObject s)
    {
        var hasRead = FileData != null;

        // Read the file data
        FileData = s.SerializeObject<T>(FileData, name: nameof(FileData));

        // If we're at the end of the file we return. No footer has been added.
        if (s.CurrentFileOffset >= s.CurrentLength && !hasRead)
            return;

        // Get the start of the footer data
        var footerOffset = s.CurrentPointer.FileOffset;

        // Check the magic footer header to make sure we're reading an editor footer
        var magic = s.SerializeString(Magic, 4, name: nameof(Magic));

        // If the magic is not correct we return
        if (Magic != magic)
        {
            s.LogWarning($"Incorrect magic identifier {magic}");
            return;
        }

        // Get the editor version
        EditorVersion = s.Serialize<int>(EditorVersion, name: nameof(EditorVersion));

        // If not 1 we don't read the data. It's most likely from a later version.
        if (EditorVersion != 1)
        {
            s.LogWarning($"Unknown editor version {EditorVersion}");

            // Set to 1 again so it's correct when we write the data
            EditorVersion = 1;

            return;
        }

        // Write the message. This is irrelevant when reading and is only used for users to more easily be able to identify
        // files which have been edited using the editor. We could also append other info here such as the edit date, username, description etc.
        Message = s.SerializeString(Message, encoding: Encoding.UTF8, name: nameof(Message));
            
        // Keep a table of relocated data structs. This is useful for files we need to repack or files where we append data to the end.
        RelocatedStructsCount = s.Serialize<int>(RelocatedStructsCount, name: nameof(RelocatedStructsCount));
        RelocatedStructs = s.SerializeObjectArray<RelocatedStruct>(RelocatedStructs, RelocatedStructsCount, name: nameof(RelocatedStructs));

        // End with the footer offset and magic. This way the footer can be read without first parsing the file.
        s.Serialize<long>(footerOffset, name: nameof(footerOffset));
        magic = s.SerializeString(Magic, 4, name: nameof(Magic));

        if (Magic != magic)
            s.LogWarning($"Unknown magic identifier {magic}");

        // Perform additional serialization actions
        if (AdditionalSerializationActions != null)
        {
            foreach (var data in AdditionalSerializationActions)
                s.DoAt(data.Key, () => data.Value(s));
        }
    }
}