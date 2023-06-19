using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TcMenu.CoreSdk.Protocol
{

    public class SystemClock
    {

        virtual public long SystemMillis() 
        { 
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return (long)ts.TotalMilliseconds;
        }

        virtual public DateTime FromSecondsUTC(long seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
        }
    }

    /// <summary>
    /// A correlation ID that allows events sent from the client or server to be linked via
    /// this ID. Calling the constructor with no parameters creates a new correlation. These
    /// are only unique for a time frame of hours to days. They should not be used for any
    /// purpose requiring persistence that could extend beyond that.
    /// </summary>
    public class CorrelationId
    {
        public static readonly CorrelationId EMPTY_CORRELATION = new CorrelationId("0");
        private static readonly int COUNTER_MODULO = 1000000;
        private static int COUNTER = 0;

        private readonly long _correlation;

        /// <summary>
        /// Create a correlation ID from a text string
        /// </summary>
        /// <param name="correlationText">the correlation string</param>
        public CorrelationId(string correlationText)
        {
            _correlation = long.Parse(correlationText, System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        /// <summary>
        /// Create a correlation ID using a specific system clock, see the above system clock for default
        /// </summary>
        /// <param name="clock">the clock</param>
        public CorrelationId(SystemClock clock)
        {
            var counterPart = Interlocked.Increment(ref COUNTER) % COUNTER_MODULO;
            var timePart = clock.SystemMillis() % (int.MaxValue - COUNTER_MODULO);
            _correlation =  timePart + counterPart;
        }

        public override bool Equals(object obj)
        {
            var id = obj as CorrelationId;
            return id != null && _correlation == id._correlation;
        }

        public override int GetHashCode()
        {
            return -771638009 + _correlation.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of the ID
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0:X08}", _correlation);
        }

        /// <summary>
        /// Return the underlying long of the ID
        /// </summary>
        /// <returns></returns>
        public long GetUnderlyingCorrelation()
        {
            return _correlation;
        }

        /// <summary>
        /// clear down the counter part of the correlation ID.
        /// </summary>
        public static void ResetCounter()
        {
            Interlocked.Exchange(ref COUNTER, 0);
        }
    }
}
