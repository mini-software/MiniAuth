using System;

namespace MiniAuth.Helpers
{
    internal static class IdHelper
    {
        static SnowflakeIdGenerator snowflakeIdGenerator = new SnowflakeIdGenerator(1);
        public static string NewId()
        {
            return snowflakeIdGenerator.GenerateId().ToString();
        }

        public class SnowflakeIdGenerator
        {
            private const long Epoch = 1609459200000; // January 1, 2021 midnight UTC in milliseconds  
            private const int WorkerIdBits = 5;
            private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);
            private const int SequenceBits = 12;
            private const int MaxSequence = -1 ^ (-1 << SequenceBits);
            private const long TimestampShift = WorkerIdBits + SequenceBits;
            private const long WorkerIdShift = SequenceBits;

            private long _workerId;
            private long _sequence = 0;
            private long _lastTimestamp = -1;

            public SnowflakeIdGenerator(long workerId)
            {
                if (workerId < 0 || workerId > MaxWorkerId)
                {
                    throw new ArgumentException($"Worker ID can't be greater than {MaxWorkerId} or less than 0.");
                }

                _workerId = workerId;
            }

            public long GenerateId()
            {
                lock (this)
                {
                    var timestamp = TimeGen();

                    if (_lastTimestamp == timestamp)
                    {
                        _sequence = (_sequence + 1) & MaxSequence;
                        if (_sequence == 0)
                        {
                            timestamp = NextMillis(_lastTimestamp);
                        }
                    }
                    else
                    {
                        _sequence = 0;
                    }

                    _lastTimestamp = timestamp;

                    return ((timestamp - Epoch) << (int)TimestampShift) | (_workerId << (int)WorkerIdShift) | _sequence;
                }
            }

            private long TimeGen()
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            private long NextMillis(long lastTimestamp)
            {
                var timestamp = TimeGen();
                while (timestamp <= lastTimestamp)
                {
                    timestamp = TimeGen();
                }
                return timestamp;
            }
        }
    }
}
