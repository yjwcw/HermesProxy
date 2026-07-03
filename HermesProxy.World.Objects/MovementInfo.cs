using System;
using Framework.GameMath;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

public sealed class MovementInfo
{
	public const float DEFAULT_WALK_SPEED = 2.5f;

	public const float DEFAULT_RUN_SPEED = 7f;

	public const float DEFAULT_RUN_BACK_SPEED = 4.5f;

	public const float DEFAULT_SWIM_SPEED = 4.72222f;

	public const float DEFAULT_SWIM_BACK_SPEED = 2.5f;

	public const float DEFAULT_FLY_SPEED = 7f;

	public const float DEFAULT_FLY_BACK_SPEED = 4.5f;

	public const float DEFAULT_TURN_RATE = 3.141593f;

	public const float DEFAULT_PITCH_RATE = 3.141593f;

	public uint Flags;

	public uint FlagsExtra;

	public uint FlagsExtra2;

	public uint MoveTime;

	public float SwimPitch;

	public uint FallTime;

	public float JumpHorizontalSpeed;

	public float JumpVerticalSpeed;

	public float JumpCosAngle;

	public float JumpSinAngle;

	public float SplineElevation;

	public bool HasSplineData;

	public Vector3 Position;

	public float Orientation;

	public float CorpseOrientation;

	public WowGuid128 TransportGuid;

	public Vector3 TransportOffset;

	public float TransportOrientation;

	public uint TransportTime;

	public uint TransportTime2;

	public sbyte TransportSeat = -1;

	public Quaternion Rotation;

	public float WalkSpeed;

	public float RunSpeed;

	public float RunBackSpeed;

	public float SwimSpeed;

	public float SwimBackSpeed;

	public float FlightSpeed;

	public float FlightBackSpeed;

	public float TurnRate;

	public float PitchRate;

	public bool Hover;

	public float VehicleOrientation;

	public uint VehicleId;

	public uint TransportPathTimer;

	public MovementInfo CopyFromMe()
	{
		return new MovementInfo
		{
			Flags = Flags,
			FlagsExtra = FlagsExtra,
			SwimPitch = SwimPitch,
			FallTime = FallTime,
			JumpHorizontalSpeed = JumpHorizontalSpeed,
			JumpVerticalSpeed = JumpVerticalSpeed,
			JumpCosAngle = JumpCosAngle,
			JumpSinAngle = JumpSinAngle,
			SplineElevation = SplineElevation,
			HasSplineData = HasSplineData,
			Position = Position,
			Orientation = Orientation,
			CorpseOrientation = CorpseOrientation,
			TransportGuid = TransportGuid,
			TransportOffset = TransportOffset,
			TransportOrientation = TransportOrientation,
			TransportTime = TransportTime,
			TransportTime2 = TransportTime2,
			TransportSeat = TransportSeat,
			Rotation = Rotation,
			WalkSpeed = WalkSpeed,
			RunSpeed = RunSpeed,
			RunBackSpeed = RunBackSpeed,
			SwimSpeed = SwimSpeed,
			SwimBackSpeed = SwimBackSpeed,
			FlightSpeed = FlightSpeed,
			FlightBackSpeed = FlightBackSpeed,
			TurnRate = TurnRate,
			PitchRate = PitchRate,
			Hover = Hover,
			VehicleId = VehicleId,
			VehicleOrientation = VehicleOrientation,
			TransportPathTimer = TransportPathTimer
		};
	}

	public void SetMovementFlags(MovementFlagModern f)
	{
		Flags = (uint)f;
	}

	public void AddMovementFlag(MovementFlagModern f)
	{
		Flags |= (uint)f;
	}

	public void RemoveMovementFlag(MovementFlagModern f)
	{
		Flags &= (uint)(~f);
	}

	public bool HasMovementFlag(MovementFlagModern f)
	{
		return (Flags & (uint)f) != 0;
	}

	public void ReadMovementInfoLegacy(WorldPacket packet, GameSessionData gameState)
	{
		bool flag;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			MovementFlagWotLK movementFlagWotLK = (MovementFlagWotLK)(Flags = packet.ReadUInt32());
			FlagsExtra = packet.ReadUInt16();
			flag = movementFlagWotLK.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying) || FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching);
		}
		else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			MovementFlagTBC movementFlagTBC = (MovementFlagTBC)packet.ReadUInt32();
			Flags = (uint)movementFlagTBC.CastFlags<MovementFlagWotLK>();
			FlagsExtra = packet.ReadUInt8();
			flag = movementFlagTBC.HasAnyFlag(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2);
		}
		else
		{
			MovementFlagVanilla movementFlagVanilla = (MovementFlagVanilla)packet.ReadUInt32();
			Flags = (uint)movementFlagVanilla.CastFlags<MovementFlagWotLK>();
			flag = movementFlagVanilla.HasAnyFlag(MovementFlagVanilla.Swimming);
			Hover = movementFlagVanilla.HasAnyFlag(MovementFlagVanilla.FixedZ);
		}
		MoveTime = packet.ReadUInt32();
		Position = packet.ReadVector3();
		Orientation = packet.ReadFloat();
		if (Flags.HasAnyFlag(MovementFlagWotLK.OnTransport))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
			{
				TransportGuid = packet.ReadPackedGuid().To128(gameState);
			}
			else
			{
				TransportGuid = packet.ReadGuid().To128(gameState);
			}
			TransportOffset = packet.ReadVector3();
			TransportOrientation = packet.ReadFloat();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				TransportTime = packet.ReadUInt32();
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				TransportSeat = packet.ReadInt8();
			}
			if (FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
			{
				TransportTime2 = packet.ReadUInt32();
			}
		}
		if (flag)
		{
			SwimPitch = packet.ReadFloat();
		}
		FallTime = packet.ReadUInt32();
		if (Flags.HasAnyFlag(MovementFlagWotLK.Falling))
		{
			JumpVerticalSpeed = packet.ReadFloat();
			JumpSinAngle = packet.ReadFloat();
			JumpCosAngle = packet.ReadFloat();
			JumpHorizontalSpeed = packet.ReadFloat();
		}
		if (Flags.HasAnyFlag(MovementFlagWotLK.SplineElevation))
		{
			SplineElevation = packet.ReadFloat();
		}
	}

	public void WriteMovementInfoLegacy(WorldPacket data)
	{
		uint num = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? ((uint)((MovementFlagModern)Flags).CastFlags<MovementFlagWotLK>()) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? ((uint)((MovementFlagModern)Flags).CastFlags<MovementFlagVanilla>()) : ((uint)((MovementFlagModern)Flags).CastFlags<MovementFlagTBC>())));
		if (TransportGuid != null)
		{
			num = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? (num | 0x200u) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? (num | 0x2000000u) : (num | 0x200u)));
		}
		data.WriteUInt32(num);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			data.WriteUInt16((ushort)FlagsExtra);
		}
		else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			data.WriteUInt8((byte)FlagsExtra);
		}
		data.WriteUInt32(MoveTime);
		data.WriteVector3(Position);
		data.WriteFloat(Orientation);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? num.HasAnyFlag(MovementFlagWotLK.OnTransport) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? num.HasAnyFlag(MovementFlagVanilla.OnTransport) : num.HasAnyFlag(MovementFlagTBC.OnTransport)))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
			{
				data.WritePackedGuid(TransportGuid.To64());
			}
			else
			{
				data.WriteGuid(TransportGuid.To64());
			}
			data.WriteVector3(TransportOffset);
			data.WriteFloat(TransportOrientation);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				data.WriteUInt32(TransportTime);
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				data.WriteInt8(TransportSeat);
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) && FlagsExtra.HasAnyFlag(MovementFlagExtra.InterpolateMove))
			{
				data.WriteUInt32(TransportTime2);
			}
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? (num.HasAnyFlag(MovementFlagWotLK.Swimming | MovementFlagWotLK.Flying) || FlagsExtra.HasAnyFlag(MovementFlagExtra.AlwaysAllowPitching)) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? num.HasAnyFlag(MovementFlagVanilla.Swimming) : num.HasAnyFlag(MovementFlagTBC.Swimming | MovementFlagTBC.Flying2)))
		{
			data.WriteFloat(SwimPitch);
		}
		data.WriteUInt32(FallTime);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? num.HasAnyFlag(MovementFlagWotLK.Falling) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? num.HasAnyFlag(MovementFlagVanilla.Falling) : num.HasAnyFlag(MovementFlagTBC.Falling)))
		{
			data.WriteFloat(JumpVerticalSpeed);
			data.WriteFloat(JumpSinAngle);
			data.WriteFloat(JumpCosAngle);
			data.WriteFloat(JumpHorizontalSpeed);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? num.HasAnyFlag(MovementFlagWotLK.SplineElevation) : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? num.HasAnyFlag(MovementFlagVanilla.SplineElevation) : num.HasAnyFlag(MovementFlagTBC.SplineElevation)))
		{
			data.WriteFloat(SplineElevation);
		}
	}

	public void ReadMovementInfoModern(WorldPacket data)
	{
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			Flags = data.ReadUInt32();
			FlagsExtra = data.ReadUInt32();
			FlagsExtra2 = data.ReadUInt32();
		}
		MoveTime = data.ReadUInt32();
		Position = data.ReadVector3();
		Orientation = data.ReadFloat();
		SwimPitch = data.ReadFloat();
		SplineElevation = data.ReadFloat();
		uint num = data.ReadUInt32();
		data.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			data.ReadPackedGuid128();
		}
		if (!ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			Flags = data.ReadBits<uint>(30);
			FlagsExtra = data.ReadBits<uint>(18);
		}
		bool num3 = data.HasBit();
		bool flag = data.HasBit();
		data.HasBit();
		data.ReadBit();
		data.ReadBit();
		bool flag2 = ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3) && data.HasBit();
		if (num3)
		{
			ReadTransportInfoModern(data);
		}
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3) && flag2)
		{
			data.ReadPackedGuid128();
			data.ReadVector3();
			data.ReadUInt32();
		}
		if (flag)
		{
			FallTime = data.ReadUInt32();
			JumpVerticalSpeed = data.ReadFloat();
			if (data.HasBit())
			{
				JumpSinAngle = data.ReadFloat();
				JumpCosAngle = data.ReadFloat();
				JumpHorizontalSpeed = data.ReadFloat();
			}
		}
	}

	public void ReadTransportInfoModern(WorldPacket data)
	{
		TransportGuid = data.ReadPackedGuid128();
		TransportOffset = data.ReadVector3();
		TransportOrientation = data.ReadFloat();
		TransportSeat = data.ReadInt8();
		TransportTime = data.ReadUInt32();
		bool num = data.HasBit();
		bool flag = data.HasBit();
		if (num)
		{
			TransportTime2 = data.ReadUInt32();
		}
		if (flag)
		{
			VehicleId = data.ReadUInt32();
		}
	}

	public void WriteMovementInfoModern(WorldPacket data, WowGuid128 guid)
	{
		bool flag = Flags.HasAnyFlag(MovementFlagModern.Falling | MovementFlagModern.FallingFar);
		bool flag2 = flag || FallTime != 0;
		data.WritePackedGuid128(guid);
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			data.WriteUInt32(Flags);
			data.WriteUInt32(FlagsExtra);
			data.WriteUInt32(FlagsExtra2);
		}
		data.WriteUInt32(MoveTime);
		data.WriteFloat(Position.X);
		data.WriteFloat(Position.Y);
		data.WriteFloat(Position.Z);
		data.WriteFloat(Orientation);
		data.WriteFloat(SwimPitch);
		data.WriteFloat(SplineElevation);
		data.WriteUInt32(0u);
		data.WriteUInt32(0u);
		if (!ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			data.WriteBits(Flags, 30);
			data.WriteBits(FlagsExtra, 18);
		}
		data.WriteBit(TransportGuid != null);
		data.WriteBit(flag2);
		data.WriteBit(HasSplineData);
		data.WriteBit(bit: false);
		data.WriteBit(bit: false);
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			data.WriteBit(bit: false);
		}
		data.FlushBits();
		if (TransportGuid != null)
		{
			WriteTransportInfoModern(data);
		}
		if (flag2)
		{
			data.WriteUInt32(FallTime);
			data.WriteFloat(JumpVerticalSpeed);
			data.WriteBit(flag);
			data.FlushBits();
			if (flag)
			{
				data.WriteFloat(JumpSinAngle);
				data.WriteFloat(JumpCosAngle);
				data.WriteFloat(JumpHorizontalSpeed);
			}
		}
	}

	public void WriteTransportInfoModern(WorldPacket data)
	{
		bool flag = false;
		bool flag2 = VehicleId != 0;
		data.WritePackedGuid128(TransportGuid);
		data.WriteFloat(TransportOffset.X);
		data.WriteFloat(TransportOffset.Y);
		data.WriteFloat(TransportOffset.Z);
		data.WriteFloat(TransportOrientation);
		data.WriteInt8(TransportSeat);
		data.WriteUInt32(TransportTime);
		data.WriteBit(flag);
		data.WriteBit(flag2);
		data.FlushBits();
		if (flag)
		{
			data.WriteUInt32(0u);
		}
		if (flag2)
		{
			data.WriteUInt32(VehicleId);
		}
	}

	public static void ClampOrientation(ref float orientation)
	{
		while (orientation < 0f)
		{
			orientation += (float)Math.PI * 2f;
		}
		while (orientation > (float)Math.PI * 2f)
		{
			orientation -= (float)Math.PI * 2f;
		}
	}

	public void ValidateMovementInfo()
	{
		ClampOrientation(ref Orientation);
		ClampOrientation(ref TransportOrientation);
		Action<bool, MovementFlagModern> obj = delegate(bool check, MovementFlagModern maskToRemove)
		{
			if (check)
			{
				Log.Print(LogType.Error, $"Violation of MovementFlags found ({check}). MovementFlags: {Flags}, MovementFlags2: {FlagsExtra}. Mask {maskToRemove} will be removed.", "ValidateMovementInfo", "D:\\a\\HermesProxy\\HermesProxy\\World\\Objects\\MovementInfo.cs");
				RemoveMovementFlag(maskToRemove);
			}
		};
		obj(HasMovementFlag(MovementFlagModern.Root) && HasMovementFlag(MovementFlagModern.MaskMoving), MovementFlagModern.MaskMoving);
		obj(HasMovementFlag(MovementFlagModern.Ascending) && HasMovementFlag(MovementFlagModern.Descending), MovementFlagModern.Ascending | MovementFlagModern.Descending);
		obj(HasMovementFlag(MovementFlagModern.TurnLeft) && HasMovementFlag(MovementFlagModern.TurnRight), MovementFlagModern.TurnLeft | MovementFlagModern.TurnRight);
		obj(HasMovementFlag(MovementFlagModern.StrafeLeft) && HasMovementFlag(MovementFlagModern.StrafeRight), MovementFlagModern.StrafeLeft | MovementFlagModern.StrafeRight);
		obj(HasMovementFlag(MovementFlagModern.PitchUp) && HasMovementFlag(MovementFlagModern.PitchDown), MovementFlagModern.PitchUp | MovementFlagModern.PitchDown);
		obj(HasMovementFlag(MovementFlagModern.Forward) && HasMovementFlag(MovementFlagModern.Backward), MovementFlagModern.Forward | MovementFlagModern.Backward);
		obj(HasMovementFlag(MovementFlagModern.DisableGravity | MovementFlagModern.CanFly) && HasMovementFlag(MovementFlagModern.Falling), MovementFlagModern.Falling);
		obj(HasMovementFlag(MovementFlagModern.SplineElevation) && MathFunctions.fuzzyEq(SplineElevation, 0f), MovementFlagModern.SplineElevation);
		if (MathFunctions.fuzzyNe(SplineElevation, 0f))
		{
			AddMovementFlag(MovementFlagModern.SplineElevation);
		}
	}
}
