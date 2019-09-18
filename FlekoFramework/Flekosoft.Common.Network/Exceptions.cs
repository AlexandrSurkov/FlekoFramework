using System;
using System.Runtime.Serialization;

namespace Flekosoft.Common.Network
{
    [Serializable]
    public abstract class NetworkException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        protected NetworkException()
        {
        }

        protected NetworkException(string message)
            : base(message)
        {
        }

        protected NetworkException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NetworkException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }


    [Serializable]
    public class HandshakeException : NetworkException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public HandshakeException()
        {
        }

        // ReSharper disable UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public HandshakeException(string message)
            // ReSharper restore UnusedMember.Global
            : base(message)
        {
        }

        // ReSharper disable UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public HandshakeException(string message, Exception inner)
            // ReSharper restore UnusedMember.Global
            : base(message, inner)
        {
        }

        protected HandshakeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class NotConnectedException : NetworkException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotConnectedException()
        {
        }

        // ReSharper disable UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public NotConnectedException(string message)
            // ReSharper restore UnusedMember.Global
            : base(message)
        {
        }

        // ReSharper disable UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public NotConnectedException(string message, Exception inner)
            // ReSharper restore UnusedMember.Global
            : base(message, inner)
        {
        }

        protected NotConnectedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class NetworkWriteException : NetworkException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        // ReSharper disable UnusedMember.Global
        public NetworkWriteException()
        // ReSharper restore UnusedMember.Global
        {
        }

        public NetworkWriteException(string message)
            : base(message)
        {
        }

        // ReSharper disable UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public NetworkWriteException(string message, Exception inner)
            // ReSharper restore UnusedMember.Global
            : base(message, inner)
        {
        }

        protected NetworkWriteException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
