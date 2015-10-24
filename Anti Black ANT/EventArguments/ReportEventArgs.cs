using System;

namespace EventArguments
{
    /// <summary>
    /// Reporter Event Arguments
    /// </summary>
    public class ReportEventArgs : EventArgs
    {
        private string source;
        private string message;
        private Exception exception;
        private bool isException;
        private DateTime occurrenceUtcTime;

        public ReportEventArgs(string sourceName, string messageData)
        {
            occurrenceUtcTime = DateTime.UtcNow;
            message = messageData;
            source = sourceName;
            this.isException = false;
        }

        public ReportEventArgs(string sourceName, Exception exp)
        {
            this.occurrenceUtcTime = DateTime.UtcNow;
            this.message = exp.Message;
            this.source = sourceName;
            this.isException = true;
            this.exception = exp;
        }

        /// <summary>
        /// Properties of message variable
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Properties of source variable
        /// </summary>
        public string Source
        {
            get { return source; }
        }

        /// <summary>
        /// Properties of occurrenceTime variable
        /// </summary>
        public DateTime OccurrenceUtcTime
        {
            get { return occurrenceUtcTime; }
        }

        /// <summary>
        /// Get Occurrence Time in Iran Standard Time format's
        /// </summary>
        public DateTime OccurrenceTime_IST
        {
            get
            {
                try
                {
                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                    DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(OccurrenceUtcTime, cstZone);
                    return cstTime;
                }
                catch (TimeZoneNotFoundException)
                {
                    System.Windows.Forms.MessageBox.Show("The registry does not define the Central Standard Time zone.");
                }
                catch (InvalidTimeZoneException)
                {
                    System.Windows.Forms.MessageBox.Show("Registry data on the Central STandard Time zone has been corrupted.");
                }

                return OccurrenceUtcTime;
            }
        }

        /// <summary>
        /// Properties of exception variable
        /// </summary>
        public Exception ReportException
        {
            get { return exception; }
        }

        /// <summary>
        /// Is this Report a exception handler or not this is just a report?
        /// </summary>
        public bool IsException
        { get { return isException; } }
    }
}
