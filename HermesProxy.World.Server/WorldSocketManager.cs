using System.Net.Sockets;
using Framework;
using Framework.Logging;
using Framework.Networking;

namespace HermesProxy.World.Server;

public class WorldSocketManager : SocketManager<WorldSocket>
{
	private AsyncAcceptor _instanceAcceptor;

	private int _socketSendBufferSize;

	private bool _tcpNoDelay;

	public override bool StartNetwork(string bindIp, int realmPort, int threadCount = 1)
	{
		_tcpNoDelay = true;
		_socketSendBufferSize = -1;
		if (!base.StartNetwork(bindIp, realmPort, threadCount))
		{
			return false;
		}
		_instanceAcceptor = new AsyncAcceptor();
		if (!_instanceAcceptor.Start(bindIp, Settings.InstancePort))
		{
			Log.Print(LogType.Error, "StartNetwork failed to start instance AsyncAcceptor", "StartNetwork", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocketManager.cs");
			return false;
		}
		_instanceAcceptor.AsyncAcceptSocket(OnSocketOpen);
		return true;
	}

	public override void StopNetwork()
	{
		_instanceAcceptor.Close();
		base.StopNetwork();
		_instanceAcceptor = null;
	}

	public override void OnSocketOpen(Socket sock)
	{
		Log.Print(LogType.Network, "Instance socket open.", "OnSocketOpen", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocketManager.cs");
		try
		{
			if (_socketSendBufferSize >= 0)
			{
				sock.SendBufferSize = _socketSendBufferSize;
			}
			sock.NoDelay = _tcpNoDelay;
		}
		catch (SocketException ex)
		{
			Log.Print(LogType.Error, ((object)ex).ToString(), "OnSocketOpen", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocketManager.cs");
			return;
		}
		base.OnSocketOpen(sock);
	}
}
