﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer.Ray1;

namespace Ray1Editor.Rayman1;

public class R1_EventDefinition
{
    // This is the GeneralEventInfoData class from Ray1Map

    #region Constructor

    public R1_EventDefinition(string name, string[] codeNames,
        ushort type, string typeName,
        byte etat, byte subEtat,
        string des, string eta,
        World[] worlds, Engine[] engines,
        byte offsetBx, byte offsetBy, byte offsetHy,
        byte followSprite, uint hitPoints, byte hitSprite, bool followEnabled,
        string[] connectedEvents,
        ushort[] labelOffsets, byte[] commands)
    {
        Name = name;
        CodeNames = codeNames;
        Type = type;
        TypeName = typeName;
        Etat = etat;
        SubEtat = subEtat;
        DES = des;
        ETA = eta;
        Worlds = worlds;
        Engines = engines;
        OffsetBX = offsetBx;
        OffsetBY = offsetBy;
        OffsetHY = offsetHy;
        FollowSprite = followSprite;
        HitPoints = hitPoints;
        HitSprite = hitSprite;
        FollowEnabled = followEnabled;
        ConnectedEvents = connectedEvents;
        LabelOffsets = labelOffsets;
        Commands = commands;
    }

    #endregion

    #region Public Properties

    public string Name { get; }
    public string[] CodeNames { get; }

    public ushort Type { get; }
    public string TypeName { get; }

    public byte Etat { get; }
    public byte SubEtat { get; }

    public string DES { get; }
    public string ETA { get; }

    public World[] Worlds { get; }
    public Engine[] Engines { get; }

    public byte OffsetBX { get; }
    public byte OffsetBY { get; }
    public byte OffsetHY { get; }

    public byte FollowSprite { get; }
    public uint HitPoints { get; }
    public byte HitSprite { get; }
    public bool FollowEnabled { get; }

    public string[] ConnectedEvents { get; }

    public ushort[] LabelOffsets { get; }
    public byte[] Commands { get; }

    #endregion

    #region Enums

    public enum Engine
    {
        R1,
        EDU,
        KIT
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Reads the event info data from a .csv file
    /// </summary>
    /// <param name="fileStream">The file stream to read from</param>
    /// <param name="sort">Indicates if the items should be sorted</param>
    /// <returns>The read data</returns>
    public static IEnumerable<R1_EventDefinition> ReadCSV(Stream fileStream, bool sort = true)
    {
        // Use a reader
        using var reader = new StreamReader(fileStream);

        // Create the output
        var output = new List<R1_EventDefinition>();

        // Skip header
        reader.ReadLine();

        // Read every line
        while (!reader.EndOfStream)
        {
            // Read the line
            var line = reader.ReadLine()?.Split(',');

            // Make sure we read something
            if (line == null)
                break;

            // Keep track of the value index
            var index = 0;

            try
            {
                // Helper methods for parsing values
                string nextValue() => line[index++];
                bool nextBoolValue() => Boolean.Parse(line[index++]);
                ushort nextUShortValue() => UInt16.Parse(nextValue());
                uint nextUIntValue() => UInt32.Parse(nextValue());
                byte nextByteValue() => Byte.Parse(nextValue());
                ushort[] next16ArrayValue() => nextValue().Split('-').Where(x => !String.IsNullOrWhiteSpace(x)).Select(UInt16.Parse).ToArray();
                IEnumerable<int> next32ArrayValue() => nextValue().Split('-').Select(Int32.Parse);
                byte[] next8ArrayValue() => nextValue().Split('-').Where(x => !String.IsNullOrWhiteSpace(x)).Select(Byte.Parse).ToArray();
                string[] nextStringArrayValue() => nextValue().Split('-').ToArray();

                // Add the item to the output
                output.Add(new R1_EventDefinition(name: nextValue(), codeNames: nextStringArrayValue(),
                    type: nextUShortValue(), typeName: nextValue(),
                    etat: nextByteValue(), subEtat: nextByteValue(),
                    des: nextValue(), eta: nextValue(),
                    worlds: next32ArrayValue().Select(x => (World)x).ToArray(), engines: nextStringArrayValue().Select(x => (Engine)Enum.Parse(typeof(Engine), x)).ToArray(),
                    offsetBx: nextByteValue(), offsetBy: nextByteValue(), offsetHy: nextByteValue(),
                    followSprite: nextByteValue(), hitPoints: nextUIntValue(), hitSprite: nextByteValue(), followEnabled: nextBoolValue(),
                    connectedEvents: nextStringArrayValue(),
                    labelOffsets: next16ArrayValue(), commands: next8ArrayValue()));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse event info. Index: {index}, items: {String.Join(" - ", line)}", ex);
            }
        }

        // Return the output
        return sort ? output.OrderBy(x => x.Name).ThenBy(x => x.Type) : output;
    }

    /// <summary>
    /// Writes the event info data to a .csv file
    /// </summary>
    /// <param name="fileStream">The file stream to write to</param>
    /// <param name="eventInfoDatas">The data to write</param>
    /// <param name="sort">Indicates if the items should be sorted</param>
    public static void WriteCSV(Stream fileStream, IEnumerable<R1_EventDefinition> eventInfoDatas, bool sort = true)
    {
        using var writer = new StreamWriter(fileStream);

        // Helper method for writing a new line
        void WriteLine(params object[] values)
        {
            foreach (var value in values)
            {
                var toWrite = value?.ToString();

                const char separator = '-';

                if (value is IDictionary dict)
                {
                    toWrite = dict.Values.Cast<object>().Aggregate(String.Empty, (current, o) => current + $"{separator}{o}");

                    if (toWrite.Length > 1)
                        toWrite = toWrite.Remove(0, 1);

                    if (toWrite.All(x => x == separator))
                        toWrite = String.Empty;
                }
                else if (value is IEnumerable enu and not string)
                {
                    toWrite = enu.Cast<object>().Aggregate(String.Empty, (current, o) => current + $"{separator}{o}");

                    if (toWrite.Length > 1)
                        toWrite = toWrite.Remove(0, 1);

                    if (toWrite.All(x => x == separator))
                        toWrite = String.Empty;
                }

                toWrite = toWrite?.Replace(",", " _");

                writer.Write($"{toWrite},");
            }

            writer.Flush();

            fileStream.Position--;

            writer.Write(Environment.NewLine);
        }

        // Write header
        WriteLine("Name", "CodeName",
            "Type", "TypeName",
            "Etat", "SubEtat",
            "DES", "ETA",
            "Worlds", "Engines",
            "OffsetBX", "OffsetBY", "OffsetHY",
            "FollowSprite", "HitPoints", "HitSprite", "FollowEnabled",
            "ConnectedEvents",
            "LabelOffsets", "Commands");

        // Get the enumerable
        var collection = sort ? eventInfoDatas.OrderBy(x => x.Type).ThenBy(x => x.Etat).ThenBy(x => x.SubEtat).ThenBy(x => x.HitPoints).ThenBy(x => x.Commands?.Length ?? 0) : eventInfoDatas;

        // Write every item on a new line
        foreach (var e in collection)
        {
            WriteLine(e.Name, e.CodeNames,
                e.Type, e.TypeName,
                e.Etat, e.SubEtat,
                e.DES, e.ETA,
                e.Worlds.Select(x => (int)x), e.Engines.Select(x => x.ToString()),
                e.OffsetBX, e.OffsetBY, e.OffsetHY,
                e.FollowSprite, e.HitPoints, e.HitSprite, e.FollowEnabled,
                e.ConnectedEvents,
                e.LabelOffsets, e.Commands);
        }
    }

    #endregion
}