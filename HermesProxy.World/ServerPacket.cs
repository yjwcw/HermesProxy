using Framework;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

public abstract class ServerPacket
{
	private byte[] buffer;

	private ConnectionType connectionType;

	protected WorldPacket _worldPacket;

	protected ServerPacket(Opcode universalOpcode)
	{
		connectionType = ConnectionType.Realm;
		uint currentOpcode = ModernVersion.GetCurrentOpcode(universalOpcode);
		_worldPacket = new WorldPacket(currentOpcode);
	}

	protected ServerPacket(Opcode universalOpcode, ConnectionType type = ConnectionType.Realm)
	{
		connectionType = type;
		uint currentOpcode = ModernVersion.GetCurrentOpcode(universalOpcode);
		_worldPacket = new WorldPacket(currentOpcode);
	}

	public void Clear()
	{
		_worldPacket.Clear();
		buffer = null;
	}

	public uint GetOpcode()
	{
		return _worldPacket.GetOpcode();
	}

	public Opcode GetUniversalOpcode()
	{
		return ModernVersion.GetUniversalOpcode(GetOpcode());
	}

	public byte[] GetData()
	{
		return buffer;
	}

	public void LogPacket(ref SniffFile sniffFile)
	{
		if (Settings.PacketsLog)
		{
			if (sniffFile == null)
			{
				sniffFile = new SniffFile("modern", (ushort)Settings.ClientBuild);
				sniffFile.WriteHeader();
			}
			sniffFile.WritePacket(GetOpcode(), isFromClient: false, GetData());
		}
	}

	public abstract void Write();

	public void WritePacketData()
	{
		if (buffer == null)
		{
			Write();
			buffer = _worldPacket.GetData();
			_worldPacket.Dispose();
		}
	}

	public ConnectionType GetConnection()
	{
		return connectionType;
	}
}
