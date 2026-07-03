using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class CreateObjectData
{
	public ObjectType ObjectType;

	public MovementInfo MoveInfo;

	public ServerSideMovement MoveSpline;

	public bool NoBirthAnim;

	public bool EnablePortals;

	public bool PlayHoverAnim;

	public bool ThisIsYou;

	public WowGuid128 AutoAttackVictim;
}
