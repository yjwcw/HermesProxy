using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class FeatureSystemStatusGlueScreen : ServerPacket
{
	public bool BpayStoreAvailable;

	public bool BpayStoreDisabledByParentalControls;

	public bool CharUndeleteEnabled;

	public bool BpayStoreEnabled;

	public bool CommerceSystemEnabled;

	public bool Unk14;

	public bool WillKickFromWorld;

	public bool IsExpansionPreorderInStore;

	public bool KioskModeEnabled;

	public bool CompetitiveModeEnabled;

	public bool TrialBoostEnabled;

	public bool TokenBalanceEnabled;

	public bool LiveRegionCharacterListEnabled;

	public bool LiveRegionCharacterCopyEnabled;

	public bool LiveRegionAccountCopyEnabled;

	public bool LiveRegionKeyBindingsCopyEnabled;

	public bool Unknown901CheckoutRelated;

	public EuropaTicketConfig EuropaTicketSystemStatus;

	public List<int> LiveRegionCharacterCopySourceRegions = new List<int>();

	public uint TokenPollTimeSeconds;

	public long TokenBalanceAmount;

	public int MaxCharactersPerRealm;

	public uint BpayStoreProductDeliveryDelay;

	public int ActiveCharacterUpgradeBoostType;

	public int ActiveClassTrialBoostType;

	public int MinimumExpansionLevel;

	public int MaximumExpansionLevel;

	public int ActiveSeason;

	public List<GameRuleValuePair> GameRuleValues = new List<GameRuleValuePair>();

	public short MaxPlayerNameQueriesPerPacket;

	public short PlayerNameQueryTelemetryInterval;

	public uint KioskSessionMinutes;

	public FeatureSystemStatusGlueScreen()
		: base(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(BpayStoreEnabled);
		_worldPacket.WriteBit(BpayStoreAvailable);
		_worldPacket.WriteBit(BpayStoreDisabledByParentalControls);
		_worldPacket.WriteBit(CharUndeleteEnabled);
		_worldPacket.WriteBit(CommerceSystemEnabled);
		_worldPacket.WriteBit(Unk14);
		_worldPacket.WriteBit(WillKickFromWorld);
		_worldPacket.WriteBit(IsExpansionPreorderInStore);
		_worldPacket.WriteBit(KioskModeEnabled);
		_worldPacket.WriteBit(CompetitiveModeEnabled);
		_worldPacket.WriteBit(TrialBoostEnabled);
		_worldPacket.WriteBit(TokenBalanceEnabled);
		_worldPacket.WriteBit(LiveRegionCharacterListEnabled);
		_worldPacket.WriteBit(LiveRegionCharacterCopyEnabled);
		_worldPacket.WriteBit(LiveRegionAccountCopyEnabled);
		_worldPacket.WriteBit(LiveRegionKeyBindingsCopyEnabled);
		_worldPacket.WriteBit(Unknown901CheckoutRelated);
		_worldPacket.WriteBit(EuropaTicketSystemStatus != null);
		_worldPacket.FlushBits();
		if (EuropaTicketSystemStatus != null)
		{
			EuropaTicketSystemStatus.Write(_worldPacket);
		}
		_worldPacket.WriteUInt32(TokenPollTimeSeconds);
		_worldPacket.WriteUInt32(KioskSessionMinutes);
		_worldPacket.WriteInt64(TokenBalanceAmount);
		_worldPacket.WriteInt32(MaxCharactersPerRealm);
		_worldPacket.WriteInt32(LiveRegionCharacterCopySourceRegions.Count);
		_worldPacket.WriteUInt32(BpayStoreProductDeliveryDelay);
		_worldPacket.WriteInt32(ActiveCharacterUpgradeBoostType);
		_worldPacket.WriteInt32(ActiveClassTrialBoostType);
		_worldPacket.WriteInt32(MinimumExpansionLevel);
		_worldPacket.WriteInt32(MaximumExpansionLevel);
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			_worldPacket.WriteInt32(ActiveSeason);
			_worldPacket.WriteInt32(GameRuleValues.Count);
			if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
			{
				_worldPacket.WriteInt16(MaxPlayerNameQueriesPerPacket);
			}
			if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
			{
				_worldPacket.WriteInt16(PlayerNameQueryTelemetryInterval);
			}
		}
		foreach (int liveRegionCharacterCopySourceRegion in LiveRegionCharacterCopySourceRegions)
		{
			_worldPacket.WriteInt32(liveRegionCharacterCopySourceRegion);
		}
		if (!ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
		{
			return;
		}
		foreach (GameRuleValuePair gameRuleValue in GameRuleValues)
		{
			gameRuleValue.Write(_worldPacket);
		}
	}
}
