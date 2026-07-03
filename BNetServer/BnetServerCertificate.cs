using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace BNetServer;
/*
 *  Bnet 服务证书
 */
public static class BnetServerCertificate
{
	private const string BNET_SERVER_CERT_RESOURCE = "HermesProxy.BNetServer.pfx";

	public static X509Certificate2 Certificate { get; }

	static BnetServerCertificate()
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HermesProxy.BNetServer.pfx");
		if (stream == null)
		{
			throw new Exception("Resource not found: 'HermesProxy.BNetServer.pfx'");
		}
		MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		Certificate = new X509Certificate2(memoryStream.ToArray());
	}
}
