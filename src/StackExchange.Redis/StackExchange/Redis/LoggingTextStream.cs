using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace StackExchange.Redis
{
#if LOGOUTPUT
    sealed class LoggingTextStream : Stream
    {
        [Conditional("VERBOSE")]
        void Trace(string value, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine(value, this.category + ":" + caller);
        }
        [Conditional("VERBOSE")]
        void Trace(char value, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine(value, this.category + ":" + caller);
        }

        private readonly Stream stream, echo;
        private readonly string category;
        public LoggingTextStream(Stream stream, string category, Stream echo)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrWhiteSpace(category)) category = GetType().Name;
            this.stream = stream;
            this.category = category;
            this.echo = echo;
        }
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            asyncBuffer = buffer;
            asyncOffset = offset;
            asyncCount = count;
            return stream.BeginRead(buffer, offset, count, callback, state);
        }
        private volatile byte[] asyncBuffer;
        private volatile int asyncOffset, asyncCount;
        public override int EndRead(IAsyncResult asyncResult)
        {
            int bytes = stream.EndRead(asyncResult);
            if (bytes <= 0)
            {
                Trace("<EOF>");
            }
            else
            {
                Trace(Encoding.UTF8.GetString(asyncBuffer, asyncOffset, asyncCount));
            }
            return bytes;
        }
        public override bool CanRead { get {  return stream.CanRead; } }
        public override bool CanSeek { get {  return stream.CanSeek; } }
        public override bool CanWrite { get { return stream.CanWrite; } }
        public override bool CanTimeout { get { return stream.CanTimeout; } }
        public override long Length { get { return stream.Length; } }
        public override long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }
        public override int ReadTimeout
        {
            get { return stream.ReadTimeout; }
            set { stream.ReadTimeout = value; }
        }
        public override int WriteTimeout
        {
            get { return stream.WriteTimeout; }
            set { stream.WriteTimeout = value; }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
                if (echo != null) echo.Flush();
            }
            base.Dispose(disposing);
        }
        public override void Close()
        {
            Trace("Close");
            stream.Close();
            if (echo != null) echo.Close();
            base.Close();
        }
        public override void Flush()
        {
            Trace("Flush");
            stream.Flush();
            if (echo != null) echo.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }
        public override void WriteByte(byte value)
        {
            Trace((char)value);
            stream.WriteByte(value);
            if (echo != null) echo.WriteByte(value);
        }
        public override int ReadByte()
        {
            int value = stream.ReadByte();
            if(value < 0)
            {
                Trace("<EOF>");
            } else
            {
                Trace((char)value);
            }
            return value;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytes = stream.Read(buffer, offset, count);
            if(bytes <= 0)
            {
                Trace("<EOF>");
            }
            else
            {
                Trace(Encoding.UTF8.GetString(buffer, offset, bytes));
            }
            return bytes;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count != 0)
            {
                Trace(Encoding.UTF8.GetString(buffer, offset, count));
            }
            stream.Write(buffer, offset, count);
            if (echo != null) echo.Write(buffer, offset, count);
        }
    }
#endif
}
