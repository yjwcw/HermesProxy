using System;
using System.IO;
using System.Threading;

namespace HermesProxy.World;

public class SniffFile
{
	private BinaryWriter _fileWriter;

	private ushort _gameVersion;

	private Mutex _mutex = new Mutex();

	public SniffFile(string fileName, ushort build)
	{
		string text = "PacketsLog";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = fileName + "_" + build + "_" + Time.UnixTime + ".pkt";
		string text3 = Path.Combine(text, text2);
		_fileWriter = new BinaryWriter(File.Open(text3, FileMode.Create));
		_gameVersion = build;
	}

	public void WriteHeader()
	{
		_fileWriter.Write('P');
		_fileWriter.Write('K');
		_fileWriter.Write('T');
		ushort num = 513;
		_fileWriter.Write(num);
		_fileWriter.Write(_gameVersion);
		for (int i = 0; i < 40; i++)
		{
			byte b = 0;
			_fileWriter.Write(b);
		}
	}

	public void WritePacket(uint opcode, bool isFromClient, byte[] data)
	{
		_mutex.WaitOne();
		byte b = (byte)((!isFromClient) ? byte.MaxValue : 0);
		_fileWriter.Write(b);
		uint num = (uint)Time.UnixTime;
		_fileWriter.Write(num);
		_fileWriter.Write(Environment.TickCount);
		if (isFromClient)
		{
			uint num2 = (uint)(data.Length - 2 + 4);
			_fileWriter.Write(num2);
			_fileWriter.Write(opcode);
			for (int i = 2; i < data.Length; i++)
			{
				_fileWriter.Write(data[i]);
			}
		}
		else
		{
			uint num3 = (uint)(data.Length + 2);
			_fileWriter.Write(num3);
			ushort num4 = (ushort)opcode;
			_fileWriter.Write(num4);
			_fileWriter.Write(data);
		}
		_mutex.ReleaseMutex();
	}

	public void CloseFile()
	{
		_fileWriter.Close();
	}
}
