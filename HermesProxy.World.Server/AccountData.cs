namespace HermesProxy.World.Server;

/*
 * 账号数据
 */
public class AccountData
{
	public WowGuid128 Guid;  // Guid

	public long Timestamp;  // 时间戳

	public uint Type; //类型

	public uint UncompressedSize;  //未压缩大小
	 
	public byte[] CompressedData; //压缩数据
}
