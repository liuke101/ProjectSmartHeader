#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace Best.HTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public sealed class DtlsRequest
    {
        private readonly long m_recordSeq;
        private readonly byte[] m_message;
        private readonly ClientHello m_clientHello;

        internal DtlsRequest(long recordSeq, byte[] message, ClientHello clientHello)
        {
            this.m_recordSeq = recordSeq;
            this.m_message = message;
            this.m_clientHello = clientHello;
        }

        internal ClientHello ClientHello
        {
            get { return m_clientHello; }
        }

        internal byte[] Message
        {
            get { return m_message; }
        }

        internal int MessageSeq
        {
            get { return TlsUtilities.ReadUint16(m_message, 4); }
        }

        internal long RecordSeq
        {
            get { return m_recordSeq; }
        }
    }
}
#pragma warning restore
#endif
