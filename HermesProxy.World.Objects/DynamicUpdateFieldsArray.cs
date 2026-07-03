using System;
using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

public class DynamicUpdateFieldsArray
{
	private uint ValuesCount;

	private UpdateTypeModern m_updateType;

	private UpdateMask m_updateMask;

	private ByteBuffer m_fieldBuffer;

	public DynamicUpdateFieldsArray(uint size, UpdateTypeModern updateType)
	{
		ValuesCount = size;
		m_updateType = updateType;
		m_updateMask = new UpdateMask(size);
		m_fieldBuffer = new ByteBuffer();
	}

	public void WriteToPacket(ByteBuffer buffer)
	{
		m_updateMask.AppendToPacket(buffer);
		buffer.WriteBytes(m_fieldBuffer);
	}

	public void SetUpdateField(int index, uint[] values, DynamicFieldChangeType changeType)
	{
		ByteBuffer byteBuffer = new ByteBuffer();
		m_updateMask.SetBit(index);
		DynamicUpdateMask dynamicUpdateMask = new DynamicUpdateMask((uint)values.Length);
		dynamicUpdateMask.EncodeDynamicFieldChangeType(changeType, m_updateType);
		if (m_updateType == UpdateTypeModern.Values && changeType == DynamicFieldChangeType.ValueAndSizeChanged)
		{
			dynamicUpdateMask.ValueCount = values.Length;
			dynamicUpdateMask.SetCount(values.Length);
		}
		for (int i = 0; i < values.Length; i++)
		{
			dynamicUpdateMask.SetBit(i);
			byteBuffer.WriteUInt32(values[i]);
		}
		dynamicUpdateMask.AppendToPacket(m_fieldBuffer);
		m_fieldBuffer.WriteBytes(byteBuffer);
	}

	public void SetUpdateField<T>(object index, T value, DynamicFieldChangeType changeType) where T : new()
	{
		if (value is int signedValue)
		{
			uint[] array = new uint[1];
			UpdateValues updateValues = default(UpdateValues);
			updateValues.SignedValue = signedValue;
			array[0] = updateValues.UnsignedValue;
			SetUpdateField((int)index, array, changeType);
			return;
		}
		if (value is uint num)
		{
			SetUpdateField(values: new uint[1] { num }, index: (int)index, changeType: changeType);
			return;
		}
		if (value is float floatValue)
		{
			uint[] array2 = new uint[1];
			UpdateValues updateValues2 = default(UpdateValues);
			updateValues2.FloatValue = floatValue;
			array2[0] = updateValues2.UnsignedValue;
			SetUpdateField((int)index, array2, changeType);
			return;
		}
		if (value is ulong x)
		{
			SetUpdateField(values: new uint[2]
			{
				MathFunctions.Pair64_LoPart(x),
				MathFunctions.Pair64_HiPart(x)
			}, index: (int)index, changeType: changeType);
			return;
		}
		if (!(value is WowGuid128 wowGuid))
		{
			throw new Exception("Unhandled type " + typeof(T).ToString() + " in SetUpdateField!");
		}
		SetUpdateField(values: new uint[4]
		{
			MathFunctions.Pair64_LoPart(wowGuid.GetLowValue()),
			MathFunctions.Pair64_HiPart(wowGuid.GetLowValue()),
			MathFunctions.Pair64_LoPart(wowGuid.GetHighValue()),
			MathFunctions.Pair64_HiPart(wowGuid.GetHighValue())
		}, index: (int)index, changeType: changeType);
	}
}
