using System;
using System.IO;
using BinarySerializer;
using NLog;
using System.Text;
using ILogger = BinarySerializer.ILogger;

namespace RayCarrot.Ray1Editor
{
    public class EditorContext : Context
    {
        public EditorContext(string basePath, bool noLog = false) : base(basePath, new EditorSettings(), noLog ? null : new EditorSerializerLog(), null, new EditorLogger()) { }

        public class EditorSettings : ISerializerSettings
        {
            public Encoding DefaultStringEncoding => Encoding.GetEncoding(437);
            public bool CreateBackupOnWrite => R1EServices.App.UserData.Serializer_CreateBackupOnWrite;
            public bool SavePointersForRelocation => false;
            public bool IgnoreCacheOnRead => false;
            public PointerSize? LoggingPointerSize => PointerSize.Pointer32;
        }

        public class EditorLogger : ILogger
        {
            private static readonly Logger Logger = LogManager.GetLogger("SerializerContext");

            public void Log(object log) => Logger.Log(LogLevel.Info, log);
            public void LogWarning(object log) => Logger.Log(LogLevel.Warn, log);
            public void LogError(object log) => Logger.Log(LogLevel.Error, log);
        }

        public class EditorSerializerLog : ISerializerLog
        {
            private static bool _hasBeenCreated;
            public bool IsEnabled => R1EServices.App.UserData.Serializer_EnableLog;

            private StreamWriter _logWriter;

            protected StreamWriter LogWriter => _logWriter ??= GetFile();

            public string OverrideLogPath { get; set; }
            public string LogFile => OverrideLogPath ?? R1EServices.App.Path_SerializerLogFile;

            public StreamWriter GetFile()
            {
                var w = new StreamWriter(File.Open(LogFile, _hasBeenCreated ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                _hasBeenCreated = true;
                return w;
            }

            public void Log(object obj)
            {
                if (IsEnabled)
                    LogWriter.WriteLine(obj != null ? obj.ToString() : String.Empty);
            }

            public void Dispose()
            {
                _logWriter?.Dispose();
                _logWriter = null;
            }
        }
    }
}