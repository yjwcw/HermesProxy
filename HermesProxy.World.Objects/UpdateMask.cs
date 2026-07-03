using System.Collections;
using Framework.IO;

namespace HermesProxy.World.Objects;

public class UpdateMask
{
	private uint _fieldCount;

	protected uint _blockCount;

	protected BitArray _mask;

	public UpdateMask(uint valuesCount = 0u)
	{
		_fieldCount = valuesCount;
		_blockCount = (valuesCount + 32 - 1) / 32;
		_mask = new BitArray((int)valuesCount, false);
	}

	public void SetCount(int valuesCount)
	{
		_fieldCount = (uint)valuesCount;
		_blockCount = (uint)(valuesCount + 32 - 1) / 32u;
		_mask = new BitArray(valuesCount, false);
	}

	public uint GetCount()
	{
		return _fieldCount;
	}

	public virtual void AppendToPacket(ByteBuffer data)
	{
		data.WriteUInt8((byte)_blockCount);
		byte[] array = new byte[_blockCount << 2];
		_mask.CopyTo(array, 0);
		data.WriteBytes(array);
	}

	public bool GetBit(int index)
	{
		return _mask.Get(index);
	}

	public void SetBit(int index)
	{
		_mask.Set(index, true);
	}

	private void UnsetBit(int index)
	{
		_mask.Set(index, false);
	}

	public void Clear()
	{
		_mask.SetAll(false);
	}
}
