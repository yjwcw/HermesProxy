namespace HermesProxy.World.Enums;

internal enum LegacyGmTicketResponse : uint
{
	TicketDoesNotExist = 0u,
	AlreadyExist = 1u,
	CreateSuccess = 2u,
	CreateError = 3u,
	UpdateSuccess = 4u,
	UpdateError = 5u,
	TicketDeleted = 9u
}
