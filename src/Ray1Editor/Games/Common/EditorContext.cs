using System;
using System.IO;
using BinarySerializer;
using NLog;
using System.Text;
using LogLevel = BinarySerializer.LogLevel;

namespace Ray1Editor;

public class EditorContext : Context
{
    public EditorContext(string basePath, bool noLog = false) 
        : base(basePath, new EditorSettings(), noLog ? null : new EditorSerializerLogger(), null, new EditorLogger()) { }

    public class EditorSettings : ISerializerSettings
    {
        public Encoding DefaultStringEncoding => Encoding.GetEncoding(437);
        public Endian DefaultEndianness => Endian.Little;
        public bool CreateBackupOnWrite => R1EServices.App.UserData.Serializer_CreateBackupOnWrite;
        public bool SavePointersForRelocation => false;
        public bool IgnoreCacheOnRead => false;
        public bool LogAlignIfNotNull => false;
        public PointerSize? LoggingPointerSize => PointerSize.Pointer32;
    }

    public class EditorLogger : ISystemLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Log(LogLevel logLevel, object log, params object[] args)
        {
            NLog.LogLevel nlogLevel = logLevel switch
            {
                LogLevel.Trace => NLog.LogLevel.Trace,
                LogLevel.Debug => NLog.LogLevel.Debug,
                LogLevel.Info => NLog.LogLevel.Info,
                LogLevel.Warning => NLog.LogLevel.Warn,
                LogLevel.Error => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info,
            };

            Logger.Log(nlogLevel, () => String.Format($"BinarySerializer: {log}", args));
        }
    }

    public class EditorSerializerLogger : ISerializerLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static bool _hasBeenCreated;

        private bool _isInvalid;

        public bool IsEnabled => !_isInvalid && R1EServices.App.UserData.Serializer_EnableLog;

        private StreamWriter _logWriter;

        protected StreamWriter LogWriter => _logWriter ??= GetFile();

        public string LogFile => R1EServices.App.Path_SerializerLogFile;

        public StreamWriter GetFile()
        {
            var mode = _hasBeenCreated ? FileMode.Append : FileMode.Create;

            try
            {
                StreamWriter w = new(File.Open(LogFile, mode, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                _hasBeenCreated = true;
                return w;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Opening serializer log file with mode {0}", mode);
                _isInvalid = true;
                return null;
            }
        }

        public void Log(object obj)
        {
            if (IsEnabled)
                LogWriter?.WriteLine(obj != null ? obj.ToString() : String.Empty);
        }

        public void Dispose()
        {
            _logWriter?.Dispose();
            _logWriter = null;
        }
    }
}