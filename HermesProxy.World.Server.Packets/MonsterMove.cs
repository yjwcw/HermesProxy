using System.Collections.Generic;
using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class MonsterMove : ServerPacket
{
	public WowGuid128 MoverGUID;

	public ServerSideMovement MoveSpline;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PackedDeltas = new List<Vector3>();

	public MonsterMove(WowGuid128 guid, ServerSideMovement moveSpline)
		: base(Opcode.SMSG_ON_MONSTER_MOVE, ConnectionType.Instance)
	{
		if (moveSpline.SplineFlags.HasFlag(SplineFlagModern.UncompressedPath))
		{
			if (!moveSpline.SplineFlags.HasFlag(SplineFlagModern.Cyclic))
			{
				foreach (Vector3 splinePoint in moveSpline.SplinePoints)
				{
					Points.Add(splinePoint);
				}
				if (moveSpline.EndPosition != Vector3.Zero)
				{
					Points.Add(moveSpline.EndPosition);
				}
			}
			else
			{
				if (moveSpline.EndPosition != Vector3.Zero)
				{
					Points.Add(moveSpline.EndPosition);
				}
				foreach (Vector3 splinePoint2 in moveSpline.SplinePoints)
				{
					Points.Add(splinePoint2);
				}
			}
		}
		else if (moveSpline.EndPosition != Vector3.Zero)
		{
			Points.Add(moveSpline.EndPosition);
			if (moveSpline.SplinePoints.Count > 0)
			{
				Vector3 vector = (moveSpline.StartPosition + moveSpline.EndPosition) / 2f;
				for (int i = 0; i < moveSpline.SplinePoints.Count; i++)
				{
					PackedDeltas.Add(vector - moveSpline.SplinePoints[i]);
				}
			}
		}
		MoverGUID = guid;
		MoveSpline = moveSpline;
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(MoverGUID);
		_worldPacket.WriteVector3(MoveSpline.StartPosition);
		_worldPacket.WriteUInt32(MoveSpline.SplineId);
		_worldPacket.WriteVector3(Vector3.Zero);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBits((Points.Count == 0) ? 2 : 0, 3);
		_worldPacket.WriteUInt32((uint)MoveSpline.SplineFlags);
		_worldPacket.WriteInt32(0);
		_worldPacket.WriteUInt32(MoveSpline.SplineTimeFull);
		_worldPacket.WriteUInt32(0u);
		_worldPacket.WriteUInt8(MoveSpline.SplineMode);
		_worldPacket.WritePackedGuid128((MoveSpline.TransportGuid != null) ? MoveSpline.TransportGuid : WowGuid128.Empty);
		_worldPacket.WriteInt8(MoveSpline.TransportSeat);
		_worldPacket.WriteBits((byte)MoveSpline.SplineType, 2);
		_worldPacket.WriteBits(Points.Count, 16);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBits(PackedDeltas.Count, 16);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.WriteBit(bit: false);
		_worldPacket.FlushBits();
		switch (MoveSpline.SplineType)
		{
		case SplineTypeModern.FacingSpot:
			_worldPacket.WriteVector3(MoveSpline.FinalFacingSpot);
			break;
		case SplineTypeModern.FacingTarget:
			_worldPacket.WriteFloat(MoveSpline.FinalOrientation);
			_worldPacket.WritePackedGuid128(MoveSpline.FinalFacingGuid);
			break;
		case SplineTypeModern.FacingAngle:
			_worldPacket.WriteFloat(MoveSpline.FinalOrientation);
			break;
		}
		foreach (Vector3 point in Points)
		{
			_worldPacket.WriteVector3(point);
		}
		foreach (Vector3 packedDelta in PackedDeltas)
		{
			_worldPacket.WritePackXYZ(packedDelta);
		}
	}
}
