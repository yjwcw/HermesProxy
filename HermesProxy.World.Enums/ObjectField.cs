namespace HermesProxy.World.Enums;

public enum ObjectField
{
	[UpdateField(UpdateFieldType.Guid)]
	OBJECT_FIELD_GUID,
	[UpdateField(UpdateFieldType.Uint)]
	OBJECT_FIELD_TYPE,
	[UpdateField(UpdateFieldType.Uint)]
	OBJECT_FIELD_ENTRY,
	[UpdateField(UpdateFieldType.Float)]
	OBJECT_FIELD_SCALE_X,
	OBJECT_FIELD_PADDING,
	OBJECT_DYNAMIC_FLAGS,
	OBJECT_END,
	OBJECT_FIELD_DATA
}
