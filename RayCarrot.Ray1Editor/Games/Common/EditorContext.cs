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
            public bool CreateBackupOnWrite => false;
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
            public bool IsEnabled => AppViewModel.Instance.UserData.EnableSerializerLog;

            private StreamWriter _logWriter;

            protected StreamWriter LogWriter => _logWriter ??= GetFile();

            public string OverrideLogPath { get; set; }
            public string LogFile => OverrideLogPath ?? AppViewModel.Instance.Path_SerializerLogFile;

            public StreamWriter GetFile()
            {
                return new StreamWriter(File.Open(LogFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
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