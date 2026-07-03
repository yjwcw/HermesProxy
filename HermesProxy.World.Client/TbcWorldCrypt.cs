using System.Linq;
using Framework.Cryptography;

namespace HermesProxy.World.Client;

public class TbcWorldCrypt : LegacyWorldCrypt
{
	public const uint CRYPTED_SEND_LEN = 6u;

	public const uint CRYPTED_RECV_LEN = 4u;

	private byte[] m_key;

	private byte m_send_i;

	private byte m_send_j;

	private byte m_recv_i;

	private byte m_recv_j;

	private bool m_isInitialized;

	public void Initialize(byte[] sessionKey)
	{
		HmacHash hmacHash = new HmacHash(new byte[16]
		{
			56, 167, 131, 21, 248, 146, 37, 48, 113, 152,
			103, 177, 140, 4, 226, 170
		});
		hmacHash.Finish(sessionKey, sessionKey.Count());
		m_key = hmacHash.Digest.ToArray();
		m_send_i = (m_send_j = (m_recv_i = (m_recv_j = 0)));
		m_isInitialized = true;
	}

	public void Decrypt(byte[] data, int len)
	{
		if ((long)len >= 4L)
		{
			byte b = 0;
			while ((uint)b < 4u)
			{
				m_recv_i %= (byte)m_key.Count();
				byte b2 = (byte)((data[b] - m_recv_j) ^ m_key[m_recv_i]);
				m_recv_i++;
				m_recv_j = data[b];
				data[b] = b2;
				b++;
			}
		}
	}

	public void Encrypt(byte[] data, int len)
	{
		if (m_isInitialized && (long)len >= 6L)
		{
			byte b = 0;
			while ((uint)b < 6u)
			{
				m_send_i %= (byte)m_key.Count();
				byte send_j = (byte)((data[b] ^ m_key[m_send_i]) + m_send_j);
				m_send_i++;
				data[b] = (m_send_j = send_j);
				b++;
			}
		}
	}
}
