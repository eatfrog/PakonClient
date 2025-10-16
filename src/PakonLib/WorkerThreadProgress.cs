using System;
using TLXLib;

namespace PakonLib
{
    public readonly struct WorkerThreadProgress : IEquatable<WorkerThreadProgress>
    {
        private readonly WORKER_THREAD_PROGRESS_000 _value;

        private WorkerThreadProgress(WORKER_THREAD_PROGRESS_000 value)
        {
            _value = value;
        }

        public static WorkerThreadProgress Initialize => FromInterop(WORKER_THREAD_PROGRESS_000.WTP_Initialize);

        public static WorkerThreadProgress ProgressComplete => FromInterop(WORKER_THREAD_PROGRESS_000.WTP_ProgressComplete);

        public int RawValue => (int)_value;

        public string Name => _value.ToString();

        public string DisplayName => FormatName(Name);

        public bool IsDefined => Enum.IsDefined(typeof(WORKER_THREAD_PROGRESS_000), _value);

        public static WorkerThreadProgress FromInterop(WORKER_THREAD_PROGRESS_000 value) => new WorkerThreadProgress(value);

        public WORKER_THREAD_PROGRESS_000 ToInterop() => _value;

        public static WorkerThreadProgress FromValue(int value) => new WorkerThreadProgress((WORKER_THREAD_PROGRESS_000)value);

        public static bool TryFromValue(int value, out WorkerThreadProgress result)
        {
            if (Enum.IsDefined(typeof(WORKER_THREAD_PROGRESS_000), value))
            {
                result = new WorkerThreadProgress((WORKER_THREAD_PROGRESS_000)value);
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryParse(string name, out WorkerThreadProgress result)
        {
            if (Enum.TryParse(name, out WORKER_THREAD_PROGRESS_000 parsed))
            {
                result = new WorkerThreadProgress(parsed);
                return true;
            }

            result = default;
            return false;
        }

        public static WorkerThreadProgress Parse(string name)
        {
            if (!TryParse(name, out var result))
            {
                throw new ArgumentException("Unknown worker thread progress value.", nameof(name));
            }

            return result;
        }

        public override string ToString() => DisplayName;

        public bool Equals(WorkerThreadProgress other) => _value == other._value;

        public override bool Equals(object obj) => obj is WorkerThreadProgress other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(WorkerThreadProgress left, WorkerThreadProgress right) => left.Equals(right);

        public static bool operator !=(WorkerThreadProgress left, WorkerThreadProgress right) => !left.Equals(right);

        private static string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            int prefixSeparator = name.IndexOf('_');
            if (prefixSeparator >= 0 && prefixSeparator + 1 < name.Length)
            {
                name = name.Substring(prefixSeparator + 1);
            }

            return name.Replace('_', ' ');
        }
    }
}
