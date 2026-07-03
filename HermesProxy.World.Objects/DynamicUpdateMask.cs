using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

public class DynamicUpdateMask : UpdateMask
{
	public uint DynamicFieldChangeType;

	public int? ValueCount;

	public DynamicUpdateMask(uint valuesCount)
		: base(valuesCount)
	{
	}

	public void EncodeDynamicFieldChangeType(DynamicFieldChangeType changeType, UpdateTypeModern updateType)
	{
		DynamicFieldChangeType = (uint)(_blockCount | ((uint)(changeType & HermesProxy.World.Objects.DynamicFieldChangeType.ValueAndSizeChanged) * ((int)(3 - updateType) / 3)));
	}

	public override void AppendToPacket(ByteBuffer data)
	{
		data.WriteUInt16((ushort)DynamicFieldChangeType);
		if (ValueCount.HasValue)
		{
			data.WriteInt32(ValueCount.Value);
		}
		byte[] array = new byte[_blockCount << 2];
		_mask.CopyTo(array, 0);
		data.WriteBytes(array);
	}
}
