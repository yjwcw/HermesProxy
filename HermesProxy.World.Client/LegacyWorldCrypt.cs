namespace HermesProxy.World.Client;

public interface LegacyWorldCrypt
{
	void Initialize(byte[] sessionKey);

	void Decrypt(byte[] data, int len);

	void Encrypt(byte[] data, int len);
}
