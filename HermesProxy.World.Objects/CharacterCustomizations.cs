using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Objects;

public static class CharacterCustomizations
{
	public static LegacyCustomizationOption GetLegacyCustomizationOption(uint option)
	{
		return option switch
		{
			9u => LegacyCustomizationOption.Skin, 
			10u => LegacyCustomizationOption.Face, 
			11u => LegacyCustomizationOption.HairStyle, 
			12u => LegacyCustomizationOption.HairColor, 
			13u => LegacyCustomizationOption.FacialHair, 
			14u => LegacyCustomizationOption.Skin, 
			15u => LegacyCustomizationOption.Face, 
			16u => LegacyCustomizationOption.HairStyle, 
			17u => LegacyCustomizationOption.HairColor, 
			18u => LegacyCustomizationOption.FacialHair, 
			19u => LegacyCustomizationOption.Skin, 
			20u => LegacyCustomizationOption.Face, 
			21u => LegacyCustomizationOption.HairStyle, 
			22u => LegacyCustomizationOption.HairColor, 
			23u => LegacyCustomizationOption.FacialHair, 
			25u => LegacyCustomizationOption.Skin, 
			26u => LegacyCustomizationOption.Face, 
			27u => LegacyCustomizationOption.HairStyle, 
			28u => LegacyCustomizationOption.HairColor, 
			29u => LegacyCustomizationOption.FacialHair, 
			30u => LegacyCustomizationOption.Skin, 
			31u => LegacyCustomizationOption.Face, 
			32u => LegacyCustomizationOption.HairStyle, 
			33u => LegacyCustomizationOption.HairColor, 
			34u => LegacyCustomizationOption.FacialHair, 
			35u => LegacyCustomizationOption.Skin, 
			36u => LegacyCustomizationOption.Face, 
			37u => LegacyCustomizationOption.HairStyle, 
			38u => LegacyCustomizationOption.HairColor, 
			39u => LegacyCustomizationOption.FacialHair, 
			40u => LegacyCustomizationOption.Skin, 
			41u => LegacyCustomizationOption.Face, 
			42u => LegacyCustomizationOption.HairStyle, 
			43u => LegacyCustomizationOption.HairColor, 
			44u => LegacyCustomizationOption.FacialHair, 
			49u => LegacyCustomizationOption.Skin, 
			50u => LegacyCustomizationOption.Face, 
			51u => LegacyCustomizationOption.HairStyle, 
			52u => LegacyCustomizationOption.HairColor, 
			53u => LegacyCustomizationOption.FacialHair, 
			58u => LegacyCustomizationOption.Skin, 
			59u => LegacyCustomizationOption.Face, 
			60u => LegacyCustomizationOption.HairStyle, 
			61u => LegacyCustomizationOption.HairColor, 
			62u => LegacyCustomizationOption.FacialHair, 
			63u => LegacyCustomizationOption.Skin, 
			64u => LegacyCustomizationOption.Face, 
			65u => LegacyCustomizationOption.HairStyle, 
			66u => LegacyCustomizationOption.HairColor, 
			67u => LegacyCustomizationOption.FacialHair, 
			68u => LegacyCustomizationOption.Skin, 
			71u => LegacyCustomizationOption.HairStyle, 
			72u => LegacyCustomizationOption.HairColor, 
			73u => LegacyCustomizationOption.FacialHair, 
			74u => LegacyCustomizationOption.Skin, 
			77u => LegacyCustomizationOption.HairStyle, 
			78u => LegacyCustomizationOption.HairColor, 
			79u => LegacyCustomizationOption.FacialHair, 
			80u => LegacyCustomizationOption.Skin, 
			81u => LegacyCustomizationOption.Face, 
			82u => LegacyCustomizationOption.HairStyle, 
			83u => LegacyCustomizationOption.HairColor, 
			84u => LegacyCustomizationOption.FacialHair, 
			85u => LegacyCustomizationOption.Skin, 
			86u => LegacyCustomizationOption.Face, 
			87u => LegacyCustomizationOption.HairStyle, 
			88u => LegacyCustomizationOption.HairColor, 
			89u => LegacyCustomizationOption.FacialHair, 
			90u => LegacyCustomizationOption.Skin, 
			91u => LegacyCustomizationOption.Face, 
			92u => LegacyCustomizationOption.HairStyle, 
			93u => LegacyCustomizationOption.HairColor, 
			94u => LegacyCustomizationOption.FacialHair, 
			95u => LegacyCustomizationOption.Skin, 
			96u => LegacyCustomizationOption.Face, 
			97u => LegacyCustomizationOption.HairStyle, 
			98u => LegacyCustomizationOption.HairColor, 
			99u => LegacyCustomizationOption.FacialHair, 
			100u => LegacyCustomizationOption.Skin, 
			102u => LegacyCustomizationOption.Face, 
			105u => LegacyCustomizationOption.Skin, 
			110u => LegacyCustomizationOption.Skin, 
			111u => LegacyCustomizationOption.Face, 
			112u => LegacyCustomizationOption.HairStyle, 
			113u => LegacyCustomizationOption.HairColor, 
			114u => LegacyCustomizationOption.FacialHair, 
			119u => LegacyCustomizationOption.Skin, 
			120u => LegacyCustomizationOption.Face, 
			121u => LegacyCustomizationOption.HairStyle, 
			122u => LegacyCustomizationOption.HairColor, 
			123u => LegacyCustomizationOption.FacialHair, 
			128u => LegacyCustomizationOption.Skin, 
			129u => LegacyCustomizationOption.Face, 
			130u => LegacyCustomizationOption.HairStyle, 
			131u => LegacyCustomizationOption.HairColor, 
			132u => LegacyCustomizationOption.FacialHair, 
			133u => LegacyCustomizationOption.Skin, 
			134u => LegacyCustomizationOption.Face, 
			135u => LegacyCustomizationOption.HairStyle, 
			136u => LegacyCustomizationOption.HairColor, 
			137u => LegacyCustomizationOption.FacialHair, 
			138u => LegacyCustomizationOption.Skin, 
			139u => LegacyCustomizationOption.Face, 
			140u => LegacyCustomizationOption.HairStyle, 
			141u => LegacyCustomizationOption.HairColor, 
			142u => LegacyCustomizationOption.Skin, 
			143u => LegacyCustomizationOption.Face, 
			144u => LegacyCustomizationOption.Skin, 
			145u => LegacyCustomizationOption.HairStyle, 
			146u => LegacyCustomizationOption.HairColor, 
			147u => LegacyCustomizationOption.Skin, 
			148u => LegacyCustomizationOption.HairStyle, 
			149u => LegacyCustomizationOption.HairColor, 
			150u => LegacyCustomizationOption.Skin, 
			151u => LegacyCustomizationOption.Face, 
			152u => LegacyCustomizationOption.HairStyle, 
			153u => LegacyCustomizationOption.HairColor, 
			154u => LegacyCustomizationOption.Skin, 
			155u => LegacyCustomizationOption.Face, 
			156u => LegacyCustomizationOption.Skin, 
			157u => LegacyCustomizationOption.Face, 
			158u => LegacyCustomizationOption.HairStyle, 
			159u => LegacyCustomizationOption.HairColor, 
			160u => LegacyCustomizationOption.Skin, 
			161u => LegacyCustomizationOption.Face, 
			176u => LegacyCustomizationOption.Skin, 
			177u => LegacyCustomizationOption.Face, 
			178u => LegacyCustomizationOption.HairStyle, 
			179u => LegacyCustomizationOption.HairColor, 
			180u => LegacyCustomizationOption.FacialHair, 
			181u => LegacyCustomizationOption.Skin, 
			182u => LegacyCustomizationOption.Face, 
			378u => LegacyCustomizationOption.Face, 
			379u => LegacyCustomizationOption.Face, 
			1000u => LegacyCustomizationOption.FacialHair, 
			1001u => LegacyCustomizationOption.HairStyle, 
			1002u => LegacyCustomizationOption.Face, 
			1003u => LegacyCustomizationOption.FacialHair, 
			1004u => LegacyCustomizationOption.Face, 
			1005u => LegacyCustomizationOption.FacialHair, 
			1006u => LegacyCustomizationOption.HairStyle, 
			1007u => LegacyCustomizationOption.FacialHair, 
			1008u => LegacyCustomizationOption.HairStyle, 
			1009u => LegacyCustomizationOption.HairStyle, 
			_ => LegacyCustomizationOption.None, 
		};
	}

	public static byte GetLegacyCustomizationChoice(uint value)
	{
		return value switch
		{
			17160u => 0, 
			17161u => 1, 
			17162u => 2, 
			17163u => 3, 
			17164u => 4, 
			17165u => 5, 
			17166u => 6, 
			17167u => 7, 
			17168u => 8, 
			17169u => 9, 
			17170u => 10, 
			17171u => 11, 
			17172u => 0, 
			17173u => 1, 
			17174u => 2, 
			17175u => 3, 
			17176u => 4, 
			17177u => 5, 
			17178u => 6, 
			17179u => 7, 
			17180u => 8, 
			17181u => 9, 
			17182u => 10, 
			17183u => 11, 
			17184u => 0, 
			17185u => 1, 
			17186u => 2, 
			17187u => 3, 
			17188u => 4, 
			17189u => 5, 
			17190u => 6, 
			17191u => 7, 
			17192u => 8, 
			17193u => 9, 
			17194u => 10, 
			17195u => 11, 
			17196u => 0, 
			17197u => 1, 
			17198u => 2, 
			17199u => 3, 
			17200u => 4, 
			17201u => 5, 
			17202u => 6, 
			17203u => 7, 
			17204u => 8, 
			17205u => 9, 
			17206u => 0, 
			17207u => 1, 
			17208u => 2, 
			17209u => 3, 
			17210u => 4, 
			17211u => 5, 
			17212u => 6, 
			17213u => 7, 
			17214u => 8, 
			17215u => 0, 
			17216u => 1, 
			17217u => 2, 
			17218u => 3, 
			17219u => 4, 
			17220u => 5, 
			17221u => 6, 
			17222u => 7, 
			17223u => 8, 
			17224u => 9, 
			17225u => 10, 
			17226u => 11, 
			17227u => 0, 
			17228u => 1, 
			17229u => 2, 
			17230u => 3, 
			17231u => 4, 
			17232u => 5, 
			17233u => 6, 
			17234u => 7, 
			17235u => 8, 
			17236u => 9, 
			17237u => 10, 
			17238u => 11, 
			17239u => 12, 
			17240u => 13, 
			17241u => 14, 
			17242u => 0, 
			17243u => 1, 
			17244u => 2, 
			17245u => 3, 
			17246u => 4, 
			17247u => 5, 
			17248u => 6, 
			17249u => 7, 
			17250u => 8, 
			17251u => 9, 
			17252u => 10, 
			17253u => 11, 
			17254u => 12, 
			17255u => 13, 
			17256u => 14, 
			17257u => 15, 
			17258u => 16, 
			17259u => 17, 
			17260u => 18, 
			17261u => 0, 
			17262u => 1, 
			17263u => 2, 
			17264u => 3, 
			17265u => 4, 
			17266u => 5, 
			17267u => 6, 
			17268u => 7, 
			17269u => 8, 
			17270u => 9, 
			17271u => 0, 
			17272u => 1, 
			17273u => 2, 
			17274u => 3, 
			17275u => 4, 
			17276u => 5, 
			17277u => 6, 
			17278u => 0, 
			17279u => 1, 
			17280u => 2, 
			17281u => 3, 
			17282u => 4, 
			17283u => 5, 
			17284u => 6, 
			17285u => 7, 
			17286u => 8, 
			17287u => 9, 
			17288u => 10, 
			17289u => 11, 
			17290u => 12, 
			17291u => 13, 
			17292u => 14, 
			17293u => 0, 
			17294u => 1, 
			17295u => 2, 
			17296u => 3, 
			17297u => 4, 
			17298u => 5, 
			17299u => 6, 
			17300u => 7, 
			17301u => 8, 
			17302u => 0, 
			17303u => 1, 
			17304u => 2, 
			17305u => 3, 
			17306u => 4, 
			17307u => 5, 
			17308u => 6, 
			17309u => 0, 
			17310u => 1, 
			17311u => 2, 
			17312u => 3, 
			17313u => 4, 
			17314u => 5, 
			17315u => 6, 
			17316u => 7, 
			17317u => 0, 
			17318u => 1, 
			17319u => 2, 
			17320u => 3, 
			17321u => 4, 
			17322u => 5, 
			17323u => 6, 
			17324u => 7, 
			17325u => 8, 
			17326u => 9, 
			17327u => 10, 
			17328u => 0, 
			17329u => 1, 
			17330u => 2, 
			17331u => 3, 
			17332u => 4, 
			17333u => 5, 
			17334u => 6, 
			17335u => 7, 
			17336u => 8, 
			17337u => 9, 
			17338u => 10, 
			17339u => 0, 
			17340u => 1, 
			17341u => 2, 
			17342u => 3, 
			17343u => 4, 
			17344u => 5, 
			17345u => 6, 
			17346u => 7, 
			17347u => 8, 
			17348u => 0, 
			17349u => 1, 
			17350u => 2, 
			17351u => 3, 
			17352u => 4, 
			17353u => 5, 
			17354u => 6, 
			17355u => 7, 
			17356u => 0, 
			17357u => 1, 
			17358u => 2, 
			17359u => 3, 
			17360u => 4, 
			17361u => 5, 
			17362u => 6, 
			17363u => 7, 
			17364u => 0, 
			17365u => 1, 
			17366u => 2, 
			17367u => 3, 
			17368u => 4, 
			17369u => 5, 
			17370u => 6, 
			17371u => 0, 
			17372u => 1, 
			17373u => 2, 
			17374u => 3, 
			17375u => 4, 
			17376u => 5, 
			17377u => 6, 
			17378u => 7, 
			17379u => 8, 
			17380u => 9, 
			17381u => 10, 
			17382u => 11, 
			17383u => 12, 
			17384u => 13, 
			17385u => 14, 
			17386u => 15, 
			17387u => 16, 
			17388u => 17, 
			17389u => 18, 
			17390u => 0, 
			17391u => 1, 
			17392u => 2, 
			17393u => 3, 
			17394u => 4, 
			17395u => 5, 
			17396u => 6, 
			17397u => 7, 
			17398u => 8, 
			17399u => 9, 
			17400u => 0, 
			17401u => 1, 
			17402u => 2, 
			17403u => 3, 
			17404u => 4, 
			17405u => 5, 
			17406u => 6, 
			17407u => 7, 
			17408u => 8, 
			17409u => 9, 
			17410u => 10, 
			17411u => 0, 
			17412u => 1, 
			17413u => 2, 
			17414u => 3, 
			17415u => 4, 
			17416u => 5, 
			17417u => 6, 
			17418u => 7, 
			17419u => 8, 
			17420u => 9, 
			17421u => 0, 
			17422u => 1, 
			17423u => 2, 
			17424u => 3, 
			17425u => 4, 
			17426u => 5, 
			17427u => 6, 
			17428u => 7, 
			17429u => 8, 
			17430u => 9, 
			17431u => 10, 
			17432u => 0, 
			17433u => 1, 
			17434u => 2, 
			17435u => 3, 
			17436u => 4, 
			17437u => 5, 
			17438u => 6, 
			17439u => 7, 
			17440u => 8, 
			17441u => 9, 
			17442u => 10, 
			17443u => 0, 
			17444u => 1, 
			17445u => 2, 
			17446u => 3, 
			17447u => 4, 
			17448u => 5, 
			17449u => 6, 
			17450u => 7, 
			17451u => 8, 
			17452u => 9, 
			17453u => 0, 
			17454u => 1, 
			17455u => 2, 
			17456u => 3, 
			17457u => 4, 
			17458u => 5, 
			17459u => 6, 
			17460u => 7, 
			17461u => 8, 
			17462u => 9, 
			17463u => 10, 
			17464u => 11, 
			17465u => 12, 
			17466u => 13, 
			17467u => 0, 
			17468u => 1, 
			17469u => 2, 
			17470u => 3, 
			17471u => 4, 
			17472u => 5, 
			17473u => 6, 
			17474u => 7, 
			17475u => 8, 
			17476u => 9, 
			17477u => 0, 
			17478u => 1, 
			17479u => 2, 
			17480u => 3, 
			17481u => 4, 
			17482u => 5, 
			17483u => 0, 
			17484u => 1, 
			17485u => 2, 
			17486u => 3, 
			17487u => 4, 
			17488u => 5, 
			17489u => 6, 
			17490u => 7, 
			17491u => 8, 
			17492u => 0, 
			17493u => 1, 
			17494u => 2, 
			17495u => 3, 
			17496u => 4, 
			17497u => 5, 
			17498u => 6, 
			17499u => 7, 
			17500u => 8, 
			17501u => 0, 
			17502u => 1, 
			17503u => 2, 
			17504u => 3, 
			17505u => 4, 
			17506u => 5, 
			17507u => 6, 
			17508u => 0, 
			17509u => 1, 
			17510u => 2, 
			17511u => 3, 
			17512u => 4, 
			17513u => 5, 
			17514u => 6, 
			17515u => 7, 
			17516u => 0, 
			17517u => 1, 
			17518u => 2, 
			17519u => 3, 
			17520u => 4, 
			17521u => 5, 
			17522u => 0, 
			17523u => 1, 
			17524u => 2, 
			17525u => 3, 
			17526u => 4, 
			17527u => 5, 
			17528u => 6, 
			17529u => 7, 
			17530u => 8, 
			17531u => 0, 
			17532u => 1, 
			17533u => 2, 
			17534u => 3, 
			17535u => 4, 
			17536u => 5, 
			17537u => 6, 
			17538u => 7, 
			17539u => 8, 
			17540u => 0, 
			17541u => 1, 
			17542u => 2, 
			17543u => 3, 
			17544u => 4, 
			17545u => 5, 
			17546u => 6, 
			17547u => 0, 
			17548u => 1, 
			17549u => 2, 
			17550u => 3, 
			17551u => 4, 
			17552u => 5, 
			17553u => 6, 
			17554u => 7, 
			17555u => 0, 
			17556u => 1, 
			17557u => 2, 
			17558u => 3, 
			17559u => 4, 
			17560u => 5, 
			17561u => 6, 
			17562u => 7, 
			17563u => 8, 
			17564u => 9, 
			17565u => 0, 
			17566u => 1, 
			17567u => 2, 
			17568u => 3, 
			17569u => 4, 
			17570u => 5, 
			17571u => 0, 
			17572u => 1, 
			17573u => 2, 
			17574u => 3, 
			17575u => 4, 
			17576u => 5, 
			17577u => 6, 
			17578u => 7, 
			17579u => 8, 
			17580u => 9, 
			17581u => 0, 
			17582u => 1, 
			17583u => 2, 
			17584u => 3, 
			17585u => 4, 
			17586u => 5, 
			17587u => 6, 
			17588u => 7, 
			17589u => 8, 
			17590u => 9, 
			17591u => 0, 
			17592u => 1, 
			17593u => 2, 
			17594u => 3, 
			17595u => 4, 
			17596u => 5, 
			17597u => 6, 
			17598u => 7, 
			17599u => 8, 
			17600u => 9, 
			17601u => 0, 
			17602u => 1, 
			17603u => 2, 
			17604u => 3, 
			17605u => 4, 
			17606u => 5, 
			17607u => 6, 
			17608u => 7, 
			17609u => 8, 
			17610u => 9, 
			17611u => 10, 
			17612u => 11, 
			17613u => 12, 
			17614u => 13, 
			17615u => 14, 
			17616u => 15, 
			17617u => 16, 
			17618u => 0, 
			17619u => 1, 
			17620u => 2, 
			17621u => 3, 
			17622u => 4, 
			17623u => 5, 
			17624u => 0, 
			17625u => 1, 
			17626u => 2, 
			17627u => 3, 
			17628u => 4, 
			17629u => 5, 
			17630u => 6, 
			17631u => 7, 
			17632u => 8, 
			17633u => 9, 
			17634u => 0, 
			17635u => 1, 
			17636u => 2, 
			17637u => 3, 
			17638u => 4, 
			17639u => 5, 
			17640u => 6, 
			17641u => 7, 
			17642u => 8, 
			17643u => 9, 
			17644u => 0, 
			17645u => 1, 
			17646u => 2, 
			17647u => 3, 
			17648u => 4, 
			17649u => 5, 
			17650u => 6, 
			17651u => 7, 
			17652u => 8, 
			17653u => 9, 
			17654u => 0, 
			17655u => 1, 
			17656u => 2, 
			17657u => 3, 
			17658u => 4, 
			17659u => 5, 
			17660u => 6, 
			17661u => 7, 
			17662u => 0, 
			17663u => 1, 
			17664u => 2, 
			17665u => 3, 
			17666u => 4, 
			17667u => 5, 
			17668u => 6, 
			17669u => 7, 
			17670u => 8, 
			17671u => 9, 
			17672u => 10, 
			17673u => 11, 
			17674u => 12, 
			17675u => 13, 
			17676u => 14, 
			17677u => 15, 
			17678u => 16, 
			17679u => 17, 
			17680u => 18, 
			17681u => 0, 
			17682u => 1, 
			17683u => 2, 
			17684u => 3, 
			17685u => 4, 
			17686u => 0, 
			17687u => 1, 
			17688u => 2, 
			17689u => 3, 
			17690u => 4, 
			17691u => 5, 
			17692u => 6, 
			17693u => 7, 
			17694u => 0, 
			17695u => 1, 
			17696u => 2, 
			17697u => 0, 
			17698u => 1, 
			17699u => 2, 
			17700u => 3, 
			17701u => 4, 
			17702u => 5, 
			17703u => 6, 
			17704u => 0, 
			17705u => 1, 
			17706u => 2, 
			17707u => 3, 
			17708u => 4, 
			17709u => 5, 
			17710u => 6, 
			17711u => 7, 
			17712u => 8, 
			17713u => 9, 
			17714u => 10, 
			17715u => 0, 
			17716u => 1, 
			17717u => 2, 
			17718u => 3, 
			17719u => 0, 
			17720u => 1, 
			17721u => 2, 
			17722u => 3, 
			17723u => 4, 
			17724u => 5, 
			17725u => 6, 
			17726u => 0, 
			17727u => 1, 
			17728u => 2, 
			17729u => 0, 
			17730u => 1, 
			17731u => 2, 
			17732u => 3, 
			17733u => 4, 
			17734u => 0, 
			17735u => 1, 
			17736u => 2, 
			17737u => 3, 
			17738u => 4, 
			17739u => 5, 
			17740u => 6, 
			17741u => 0, 
			17742u => 1, 
			17743u => 2, 
			17744u => 3, 
			17745u => 4, 
			17746u => 5, 
			17747u => 6, 
			17748u => 0, 
			17749u => 1, 
			17750u => 2, 
			17751u => 3, 
			17752u => 4, 
			17753u => 5, 
			17754u => 6, 
			17755u => 0, 
			17756u => 1, 
			17757u => 2, 
			17758u => 3, 
			17759u => 4, 
			17760u => 5, 
			17761u => 6, 
			17762u => 7, 
			17763u => 8, 
			17764u => 0, 
			17765u => 1, 
			17766u => 2, 
			17767u => 3, 
			17768u => 4, 
			17769u => 5, 
			17770u => 6, 
			17771u => 7, 
			17772u => 0, 
			17773u => 1, 
			17774u => 2, 
			17775u => 3, 
			17776u => 4, 
			17777u => 5, 
			17778u => 6, 
			17779u => 0, 
			17780u => 1, 
			17781u => 2, 
			17782u => 3, 
			17783u => 4, 
			17784u => 5, 
			17785u => 6, 
			17786u => 0, 
			17787u => 1, 
			17788u => 2, 
			17789u => 3, 
			17790u => 4, 
			17791u => 5, 
			17792u => 6, 
			17793u => 0, 
			17794u => 1, 
			17795u => 2, 
			17796u => 3, 
			17797u => 4, 
			17798u => 5, 
			17799u => 6, 
			17800u => 7, 
			17801u => 8, 
			17802u => 0, 
			17803u => 1, 
			17804u => 2, 
			17805u => 3, 
			17806u => 4, 
			17807u => 5, 
			17808u => 6, 
			17809u => 0, 
			17810u => 1, 
			17811u => 2, 
			17812u => 3, 
			17813u => 4, 
			17814u => 5, 
			17815u => 6, 
			17816u => 7, 
			17817u => 8, 
			17818u => 9, 
			17819u => 10, 
			17820u => 11, 
			17821u => 12, 
			17822u => 13, 
			17823u => 14, 
			17824u => 0, 
			17825u => 1, 
			17826u => 2, 
			17827u => 3, 
			17828u => 4, 
			17829u => 0, 
			17830u => 1, 
			17831u => 2, 
			17832u => 3, 
			17833u => 4, 
			17834u => 5, 
			17835u => 0, 
			17836u => 1, 
			17837u => 2, 
			17838u => 3, 
			17839u => 4, 
			17840u => 5, 
			17841u => 6, 
			17842u => 7, 
			17843u => 8, 
			17844u => 9, 
			17845u => 0, 
			17846u => 1, 
			17847u => 2, 
			17848u => 3, 
			17849u => 4, 
			17850u => 5, 
			17851u => 6, 
			17852u => 7, 
			17853u => 8, 
			17854u => 9, 
			17855u => 10, 
			17856u => 0, 
			17857u => 1, 
			17858u => 2, 
			17859u => 3, 
			17860u => 4, 
			17861u => 5, 
			17862u => 6, 
			17863u => 7, 
			17864u => 8, 
			17865u => 9, 
			17866u => 10, 
			17867u => 11, 
			17868u => 12, 
			17869u => 13, 
			17870u => 14, 
			17871u => 0, 
			17872u => 1, 
			17873u => 2, 
			17874u => 3, 
			17875u => 4, 
			17876u => 5, 
			17877u => 0, 
			17878u => 1, 
			17879u => 2, 
			17880u => 3, 
			17881u => 4, 
			17882u => 0, 
			17883u => 1, 
			17884u => 2, 
			17885u => 3, 
			17886u => 4, 
			17887u => 5, 
			17888u => 6, 
			17889u => 7, 
			17890u => 8, 
			17891u => 9, 
			17892u => 0, 
			17893u => 1, 
			17894u => 2, 
			17895u => 3, 
			17896u => 4, 
			17897u => 5, 
			17898u => 0, 
			17899u => 1, 
			17900u => 2, 
			17901u => 0, 
			17902u => 1, 
			17903u => 0, 
			17904u => 1, 
			17905u => 2, 
			17906u => 0, 
			17907u => 1, 
			17908u => 2, 
			17909u => 3, 
			17910u => 4, 
			17911u => 5, 
			17912u => 6, 
			17913u => 7, 
			17914u => 8, 
			17915u => 9, 
			17916u => 10, 
			17917u => 11, 
			17918u => 12, 
			17919u => 13, 
			17920u => 14, 
			17921u => 15, 
			17922u => 0, 
			17923u => 1, 
			17924u => 2, 
			17925u => 3, 
			17926u => 4, 
			17927u => 5, 
			17928u => 6, 
			17929u => 7, 
			17930u => 8, 
			17931u => 9, 
			17932u => 0, 
			17933u => 1, 
			17934u => 2, 
			17935u => 3, 
			17936u => 4, 
			17937u => 5, 
			17938u => 6, 
			17939u => 7, 
			17940u => 8, 
			17941u => 9, 
			17942u => 10, 
			17943u => 0, 
			17944u => 1, 
			17945u => 2, 
			17946u => 3, 
			17947u => 4, 
			17948u => 5, 
			17949u => 6, 
			17950u => 7, 
			17951u => 8, 
			17952u => 9, 
			17953u => 0, 
			17954u => 1, 
			17955u => 2, 
			17956u => 3, 
			17957u => 4, 
			17958u => 5, 
			17959u => 6, 
			17960u => 7, 
			17961u => 8, 
			17962u => 9, 
			17963u => 0, 
			17964u => 1, 
			17965u => 2, 
			17966u => 3, 
			17967u => 4, 
			17968u => 5, 
			17969u => 6, 
			17970u => 7, 
			17971u => 8, 
			17972u => 9, 
			17973u => 10, 
			17974u => 11, 
			17975u => 12, 
			17976u => 13, 
			17977u => 14, 
			17978u => 15, 
			17979u => 0, 
			17980u => 1, 
			17981u => 2, 
			17982u => 3, 
			17983u => 4, 
			17984u => 5, 
			17985u => 6, 
			17986u => 7, 
			17987u => 8, 
			17988u => 9, 
			17989u => 0, 
			17990u => 1, 
			17991u => 2, 
			17992u => 3, 
			17993u => 4, 
			17994u => 5, 
			17995u => 6, 
			17996u => 7, 
			17997u => 8, 
			17998u => 9, 
			17999u => 10, 
			18000u => 11, 
			18001u => 12, 
			18002u => 13, 
			18004u => 0, 
			18005u => 1, 
			18006u => 2, 
			18007u => 3, 
			18008u => 4, 
			18009u => 5, 
			18010u => 6, 
			18011u => 7, 
			18012u => 8, 
			18013u => 9, 
			18014u => 0, 
			18015u => 1, 
			18016u => 2, 
			18017u => 3, 
			18018u => 4, 
			18019u => 5, 
			18020u => 6, 
			18021u => 7, 
			18022u => 8, 
			18023u => 9, 
			18024u => 10, 
			18025u => 0, 
			18026u => 1, 
			18027u => 2, 
			18028u => 3, 
			18029u => 4, 
			18030u => 5, 
			18031u => 6, 
			18032u => 7, 
			18033u => 8, 
			18034u => 9, 
			18035u => 10, 
			18036u => 11, 
			18037u => 12, 
			18038u => 13, 
			18039u => 0, 
			18040u => 1, 
			18041u => 2, 
			18042u => 3, 
			18043u => 4, 
			18044u => 5, 
			18045u => 6, 
			18046u => 7, 
			18047u => 8, 
			18048u => 9, 
			18049u => 0, 
			18050u => 1, 
			18051u => 2, 
			18052u => 3, 
			18053u => 4, 
			18054u => 5, 
			18055u => 6, 
			18056u => 7, 
			18057u => 8, 
			18058u => 0, 
			18059u => 1, 
			18060u => 2, 
			18061u => 3, 
			18062u => 4, 
			18063u => 5, 
			18064u => 6, 
			18065u => 0, 
			18066u => 1, 
			18067u => 2, 
			18068u => 3, 
			18069u => 4, 
			18070u => 5, 
			18071u => 6, 
			18072u => 7, 
			18073u => 0, 
			18074u => 1, 
			18075u => 2, 
			18076u => 3, 
			18077u => 4, 
			18078u => 5, 
			18079u => 6, 
			18080u => 7, 
			18081u => 8, 
			18082u => 9, 
			18083u => 10, 
			18084u => 11, 
			18085u => 0, 
			18086u => 1, 
			18087u => 2, 
			18088u => 3, 
			18089u => 4, 
			18090u => 5, 
			18091u => 6, 
			18092u => 7, 
			18093u => 8, 
			18094u => 9, 
			18095u => 0, 
			18096u => 1, 
			18097u => 2, 
			18098u => 3, 
			18099u => 4, 
			18100u => 5, 
			18101u => 6, 
			18102u => 7, 
			18103u => 8, 
			18104u => 9, 
			18105u => 10, 
			18106u => 0, 
			18107u => 1, 
			18108u => 2, 
			18109u => 3, 
			18110u => 4, 
			18111u => 5, 
			18112u => 6, 
			18113u => 0, 
			18114u => 1, 
			18115u => 2, 
			18116u => 3, 
			18117u => 4, 
			18118u => 5, 
			18119u => 6, 
			18120u => 0, 
			18121u => 1, 
			18122u => 2, 
			18123u => 0, 
			18124u => 0, 
			18125u => 0, 
			18126u => 0, 
			18127u => 0, 
			18128u => 0, 
			18129u => 0, 
			18130u => 0, 
			18131u => 1, 
			18132u => 2, 
			18133u => 3, 
			18134u => 4, 
			18135u => 0, 
			18136u => 0, 
			18137u => 0, 
			18138u => 0, 
			18139u => 0, 
			18140u => 1, 
			18141u => 2, 
			18142u => 3, 
			18143u => 4, 
			18144u => 0, 
			18145u => 0, 
			18146u => 0, 
			18147u => 0, 
			18148u => 0, 
			18149u => 1, 
			18150u => 2, 
			18151u => 3, 
			18152u => 4, 
			18153u => 5, 
			18154u => 0, 
			18155u => 0, 
			18156u => 1, 
			18157u => 2, 
			18158u => 0, 
			18159u => 1, 
			18160u => 2, 
			18161u => 3, 
			18162u => 4, 
			18163u => 5, 
			18164u => 6, 
			18165u => 7, 
			18166u => 8, 
			18167u => 9, 
			18168u => 0, 
			18169u => 0, 
			18170u => 0, 
			18171u => 0, 
			18172u => 1, 
			18173u => 2, 
			18174u => 3, 
			18175u => 4, 
			18176u => 5, 
			18177u => 0, 
			18178u => 0, 
			18179u => 0, 
			18180u => 0, 
			18181u => 0, 
			18182u => 0, 
			18183u => 0, 
			18184u => 0, 
			18185u => 1, 
			18186u => 2, 
			18187u => 3, 
			18188u => 4, 
			18189u => 5, 
			18190u => 0, 
			18191u => 1, 
			18192u => 2, 
			18193u => 3, 
			18194u => 4, 
			18195u => 0, 
			18196u => 1, 
			18197u => 2, 
			18198u => 3, 
			18199u => 4, 
			18200u => 5, 
			18201u => 0, 
			18202u => 1, 
			18203u => 2, 
			18204u => 3, 
			18205u => 4, 
			18206u => 5, 
			18207u => 6, 
			18208u => 7, 
			18209u => 8, 
			18210u => 9, 
			18211u => 0, 
			18212u => 1, 
			18213u => 2, 
			18214u => 3, 
			18215u => 4, 
			18216u => 5, 
			18217u => 6, 
			18218u => 7, 
			18219u => 8, 
			18220u => 9, 
			18221u => 10, 
			18222u => 0, 
			18223u => 0, 
			18224u => 0, 
			_ => 0, 
		};
	}

	public static void ConvertModernCustomizationsToLegacy(List<ChrCustomizationChoice> customizations, out byte skin, out byte face, out byte hairStyle, out byte hairColor, out byte facialHair)
	{
		skin = 0;
		face = 0;
		hairStyle = 0;
		hairColor = 0;
		facialHair = 0;
		foreach (ChrCustomizationChoice customization in customizations)
		{
			LegacyCustomizationOption legacyCustomizationOption = GetLegacyCustomizationOption(customization.ChrCustomizationOptionID);
			byte legacyCustomizationChoice = GetLegacyCustomizationChoice(customization.ChrCustomizationChoiceID);
			switch (legacyCustomizationOption)
			{
			case LegacyCustomizationOption.Skin:
				skin = legacyCustomizationChoice;
				break;
			case LegacyCustomizationOption.Face:
				face = legacyCustomizationChoice;
				break;
			case LegacyCustomizationOption.HairStyle:
				hairStyle = legacyCustomizationChoice;
				break;
			case LegacyCustomizationOption.HairColor:
				hairColor = legacyCustomizationChoice;
				break;
			case LegacyCustomizationOption.FacialHair:
				facialHair = legacyCustomizationChoice;
				break;
			}
		}
	}

	public static uint GetModernCustomizationOption(Race raceId, Gender gender, LegacyCustomizationOption option)
	{
		switch (raceId)
		{
		case Race.Human:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 9u;
				case LegacyCustomizationOption.Face:
					return 10u;
				case LegacyCustomizationOption.HairStyle:
					return 11u;
				case LegacyCustomizationOption.HairColor:
					return 12u;
				case LegacyCustomizationOption.FacialHair:
					return 13u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 14u;
				case LegacyCustomizationOption.Face:
					return 15u;
				case LegacyCustomizationOption.HairStyle:
					return 16u;
				case LegacyCustomizationOption.HairColor:
					return 17u;
				case LegacyCustomizationOption.FacialHair:
					return 18u;
				}
			}
			return 0u;
		case Race.Orc:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 19u;
				case LegacyCustomizationOption.Face:
					return 20u;
				case LegacyCustomizationOption.HairStyle:
					return 21u;
				case LegacyCustomizationOption.HairColor:
					return 22u;
				case LegacyCustomizationOption.FacialHair:
					return 23u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 25u;
				case LegacyCustomizationOption.Face:
					return 26u;
				case LegacyCustomizationOption.HairStyle:
					return 27u;
				case LegacyCustomizationOption.HairColor:
					return 28u;
				case LegacyCustomizationOption.FacialHair:
					return 29u;
				}
			}
			return 0u;
		case Race.Dwarf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 30u;
				case LegacyCustomizationOption.Face:
					return 31u;
				case LegacyCustomizationOption.HairStyle:
					return 32u;
				case LegacyCustomizationOption.HairColor:
					return 33u;
				case LegacyCustomizationOption.FacialHair:
					return 34u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 35u;
				case LegacyCustomizationOption.Face:
					return 36u;
				case LegacyCustomizationOption.HairStyle:
					return 37u;
				case LegacyCustomizationOption.HairColor:
					return 38u;
				case LegacyCustomizationOption.FacialHair:
					return 39u;
				}
			}
			return 0u;
		case Race.NightElf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 40u;
				case LegacyCustomizationOption.Face:
					return 41u;
				case LegacyCustomizationOption.HairStyle:
					return 42u;
				case LegacyCustomizationOption.HairColor:
					return 43u;
				case LegacyCustomizationOption.FacialHair:
					return 44u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 49u;
				case LegacyCustomizationOption.Face:
					return 50u;
				case LegacyCustomizationOption.HairStyle:
					return 51u;
				case LegacyCustomizationOption.HairColor:
					return 52u;
				case LegacyCustomizationOption.FacialHair:
					return 53u;
				}
			}
			return 0u;
		case Race.Undead:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 58u;
				case LegacyCustomizationOption.Face:
					return 59u;
				case LegacyCustomizationOption.HairStyle:
					return 60u;
				case LegacyCustomizationOption.HairColor:
					return 61u;
				case LegacyCustomizationOption.FacialHair:
					return 62u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 63u;
				case LegacyCustomizationOption.Face:
					return 64u;
				case LegacyCustomizationOption.HairStyle:
					return 65u;
				case LegacyCustomizationOption.HairColor:
					return 66u;
				case LegacyCustomizationOption.FacialHair:
					return 67u;
				}
			}
			return 0u;
		case Race.Tauren:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 68u;
				case LegacyCustomizationOption.HairStyle:
					return 71u;
				case LegacyCustomizationOption.HairColor:
					return 72u;
				case LegacyCustomizationOption.FacialHair:
					return 73u;
				case LegacyCustomizationOption.Face:
					return 378u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 74u;
				case LegacyCustomizationOption.HairStyle:
					return 77u;
				case LegacyCustomizationOption.HairColor:
					return 78u;
				case LegacyCustomizationOption.FacialHair:
					return 79u;
				case LegacyCustomizationOption.Face:
					return 379u;
				}
			}
			return 0u;
		case Race.Gnome:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 80u;
				case LegacyCustomizationOption.Face:
					return 81u;
				case LegacyCustomizationOption.HairStyle:
					return 82u;
				case LegacyCustomizationOption.HairColor:
					return 83u;
				case LegacyCustomizationOption.FacialHair:
					return 84u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 85u;
				case LegacyCustomizationOption.Face:
					return 86u;
				case LegacyCustomizationOption.HairStyle:
					return 87u;
				case LegacyCustomizationOption.HairColor:
					return 88u;
				case LegacyCustomizationOption.FacialHair:
					return 89u;
				}
			}
			return 0u;
		case Race.Troll:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 90u;
				case LegacyCustomizationOption.Face:
					return 91u;
				case LegacyCustomizationOption.HairStyle:
					return 92u;
				case LegacyCustomizationOption.HairColor:
					return 93u;
				case LegacyCustomizationOption.FacialHair:
					return 94u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 95u;
				case LegacyCustomizationOption.Face:
					return 96u;
				case LegacyCustomizationOption.HairStyle:
					return 97u;
				case LegacyCustomizationOption.HairColor:
					return 98u;
				case LegacyCustomizationOption.FacialHair:
					return 99u;
				}
			}
			return 0u;
		case Race.Goblin:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 100u;
				case LegacyCustomizationOption.Face:
					return 102u;
				}
			}
			else if (option == LegacyCustomizationOption.Skin)
			{
				return 105u;
			}
			return 0u;
		case Race.BloodElf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 110u;
				case LegacyCustomizationOption.Face:
					return 111u;
				case LegacyCustomizationOption.HairStyle:
					return 112u;
				case LegacyCustomizationOption.HairColor:
					return 113u;
				case LegacyCustomizationOption.FacialHair:
					return 114u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 119u;
				case LegacyCustomizationOption.Face:
					return 120u;
				case LegacyCustomizationOption.HairStyle:
					return 121u;
				case LegacyCustomizationOption.HairColor:
					return 122u;
				case LegacyCustomizationOption.FacialHair:
					return 123u;
				}
			}
			return 0u;
		case Race.Draenei:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 128u;
				case LegacyCustomizationOption.Face:
					return 129u;
				case LegacyCustomizationOption.HairStyle:
					return 130u;
				case LegacyCustomizationOption.HairColor:
					return 131u;
				case LegacyCustomizationOption.FacialHair:
					return 132u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 133u;
				case LegacyCustomizationOption.Face:
					return 134u;
				case LegacyCustomizationOption.HairStyle:
					return 135u;
				case LegacyCustomizationOption.HairColor:
					return 136u;
				case LegacyCustomizationOption.FacialHair:
					return 137u;
				}
			}
			return 0u;
		case Race.FelOrc:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 138u;
				case LegacyCustomizationOption.Face:
					return 139u;
				case LegacyCustomizationOption.HairStyle:
					return 140u;
				case LegacyCustomizationOption.HairColor:
					return 141u;
				case LegacyCustomizationOption.FacialHair:
					return 1000u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 142u;
				case LegacyCustomizationOption.Face:
					return 143u;
				case LegacyCustomizationOption.HairStyle:
					return 1001u;
				}
			}
			return 0u;
		case Race.Naga:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 144u;
				case LegacyCustomizationOption.HairStyle:
					return 145u;
				case LegacyCustomizationOption.HairColor:
					return 146u;
				case LegacyCustomizationOption.Face:
					return 1002u;
				case LegacyCustomizationOption.FacialHair:
					return 1003u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 147u;
				case LegacyCustomizationOption.HairStyle:
					return 148u;
				case LegacyCustomizationOption.HairColor:
					return 149u;
				case LegacyCustomizationOption.Face:
					return 1004u;
				case LegacyCustomizationOption.FacialHair:
					return 1005u;
				}
			}
			return 0u;
		case Race.Broken:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 150u;
				case LegacyCustomizationOption.Face:
					return 151u;
				case LegacyCustomizationOption.HairStyle:
					return 152u;
				case LegacyCustomizationOption.HairColor:
					return 153u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 154u;
				case LegacyCustomizationOption.Face:
					return 155u;
				case LegacyCustomizationOption.HairStyle:
					return 1006u;
				}
			}
			return 0u;
		case Race.Skeleton:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 156u;
				case LegacyCustomizationOption.Face:
					return 157u;
				case LegacyCustomizationOption.HairStyle:
					return 158u;
				case LegacyCustomizationOption.HairColor:
					return 159u;
				case LegacyCustomizationOption.FacialHair:
					return 1007u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 160u;
				case LegacyCustomizationOption.Face:
					return 161u;
				case LegacyCustomizationOption.HairStyle:
					return 1008u;
				}
			}
			return 0u;
		case Race.ForestTroll:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 176u;
				case LegacyCustomizationOption.Face:
					return 177u;
				case LegacyCustomizationOption.HairStyle:
					return 178u;
				case LegacyCustomizationOption.HairColor:
					return 179u;
				case LegacyCustomizationOption.FacialHair:
					return 180u;
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return 181u;
				case LegacyCustomizationOption.Face:
					return 182u;
				case LegacyCustomizationOption.HairStyle:
					return 1009u;
				}
			}
			return 0u;
		default:
			return 0u;
		}
	}

	public static uint GetModernCustomizationChoice(Race raceId, Gender gender, LegacyCustomizationOption option, byte value)
	{
		switch (raceId)
		{
		case Race.Human:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17160u, 
						1 => 17161u, 
						2 => 17162u, 
						3 => 17163u, 
						4 => 17164u, 
						5 => 17165u, 
						6 => 17166u, 
						7 => 17167u, 
						8 => 17168u, 
						9 => 17169u, 
						10 => 17170u, 
						11 => 17171u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17172u, 
						1 => 17173u, 
						2 => 17174u, 
						3 => 17175u, 
						4 => 17176u, 
						5 => 17177u, 
						6 => 17178u, 
						7 => 17179u, 
						8 => 17180u, 
						9 => 17181u, 
						10 => 17182u, 
						11 => 17183u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17184u, 
						1 => 17185u, 
						2 => 17186u, 
						3 => 17187u, 
						4 => 17188u, 
						5 => 17189u, 
						6 => 17190u, 
						7 => 17191u, 
						8 => 17192u, 
						9 => 17193u, 
						10 => 17194u, 
						11 => 17195u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17196u, 
						1 => 17197u, 
						2 => 17198u, 
						3 => 17199u, 
						4 => 17200u, 
						5 => 17201u, 
						6 => 17202u, 
						7 => 17203u, 
						8 => 17204u, 
						9 => 17205u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17206u, 
						1 => 17207u, 
						2 => 17208u, 
						3 => 17209u, 
						4 => 17210u, 
						5 => 17211u, 
						6 => 17212u, 
						7 => 17213u, 
						8 => 17214u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17215u, 
						1 => 17216u, 
						2 => 17217u, 
						3 => 17218u, 
						4 => 17219u, 
						5 => 17220u, 
						6 => 17221u, 
						7 => 17222u, 
						8 => 17223u, 
						9 => 17224u, 
						10 => 17225u, 
						11 => 17226u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17227u, 
						1 => 17228u, 
						2 => 17229u, 
						3 => 17230u, 
						4 => 17231u, 
						5 => 17232u, 
						6 => 17233u, 
						7 => 17234u, 
						8 => 17235u, 
						9 => 17236u, 
						10 => 17237u, 
						11 => 17238u, 
						12 => 17239u, 
						13 => 17240u, 
						14 => 17241u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17242u, 
						1 => 17243u, 
						2 => 17244u, 
						3 => 17245u, 
						4 => 17246u, 
						5 => 17247u, 
						6 => 17248u, 
						7 => 17249u, 
						8 => 17250u, 
						9 => 17251u, 
						10 => 17252u, 
						11 => 17253u, 
						12 => 17254u, 
						13 => 17255u, 
						14 => 17256u, 
						15 => 17257u, 
						16 => 17258u, 
						17 => 17259u, 
						18 => 17260u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17261u, 
						1 => 17262u, 
						2 => 17263u, 
						3 => 17264u, 
						4 => 17265u, 
						5 => 17266u, 
						6 => 17267u, 
						7 => 17268u, 
						8 => 17269u, 
						9 => 17270u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17271u, 
						1 => 17272u, 
						2 => 17273u, 
						3 => 17274u, 
						4 => 17275u, 
						5 => 17276u, 
						6 => 17277u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Orc:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17278u, 
						1 => 17279u, 
						2 => 17280u, 
						3 => 17281u, 
						4 => 17282u, 
						5 => 17283u, 
						6 => 17284u, 
						7 => 17285u, 
						8 => 17286u, 
						9 => 17287u, 
						10 => 17288u, 
						11 => 17289u, 
						12 => 17290u, 
						13 => 17291u, 
						14 => 17292u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17293u, 
						1 => 17294u, 
						2 => 17295u, 
						3 => 17296u, 
						4 => 17297u, 
						5 => 17298u, 
						6 => 17299u, 
						7 => 17300u, 
						8 => 17301u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17302u, 
						1 => 17303u, 
						2 => 17304u, 
						3 => 17305u, 
						4 => 17306u, 
						5 => 17307u, 
						6 => 17308u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17309u, 
						1 => 17310u, 
						2 => 17311u, 
						3 => 17312u, 
						4 => 17313u, 
						5 => 17314u, 
						6 => 17315u, 
						7 => 17316u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17317u, 
						1 => 17318u, 
						2 => 17319u, 
						3 => 17320u, 
						4 => 17321u, 
						5 => 17322u, 
						6 => 17323u, 
						7 => 17324u, 
						8 => 17325u, 
						9 => 17326u, 
						10 => 17327u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17328u, 
						1 => 17329u, 
						2 => 17330u, 
						3 => 17331u, 
						4 => 17332u, 
						5 => 17333u, 
						6 => 17334u, 
						7 => 17335u, 
						8 => 17336u, 
						9 => 17337u, 
						10 => 17338u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17339u, 
						1 => 17340u, 
						2 => 17341u, 
						3 => 17342u, 
						4 => 17343u, 
						5 => 17344u, 
						6 => 17345u, 
						7 => 17346u, 
						8 => 17347u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17348u, 
						1 => 17349u, 
						2 => 17350u, 
						3 => 17351u, 
						4 => 17352u, 
						5 => 17353u, 
						6 => 17354u, 
						7 => 17355u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17356u, 
						1 => 17357u, 
						2 => 17358u, 
						3 => 17359u, 
						4 => 17360u, 
						5 => 17361u, 
						6 => 17362u, 
						7 => 17363u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17364u, 
						1 => 17365u, 
						2 => 17366u, 
						3 => 17367u, 
						4 => 17368u, 
						5 => 17369u, 
						6 => 17370u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Dwarf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17371u, 
						1 => 17372u, 
						2 => 17373u, 
						3 => 17374u, 
						4 => 17375u, 
						5 => 17376u, 
						6 => 17377u, 
						7 => 17378u, 
						8 => 17379u, 
						9 => 17380u, 
						10 => 17381u, 
						11 => 17382u, 
						12 => 17383u, 
						13 => 17384u, 
						14 => 17385u, 
						15 => 17386u, 
						16 => 17387u, 
						17 => 17388u, 
						18 => 17389u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17390u, 
						1 => 17391u, 
						2 => 17392u, 
						3 => 17393u, 
						4 => 17394u, 
						5 => 17395u, 
						6 => 17396u, 
						7 => 17397u, 
						8 => 17398u, 
						9 => 17399u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17400u, 
						1 => 17401u, 
						2 => 17402u, 
						3 => 17403u, 
						4 => 17404u, 
						5 => 17405u, 
						6 => 17406u, 
						7 => 17407u, 
						8 => 17408u, 
						9 => 17409u, 
						10 => 17410u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17411u, 
						1 => 17412u, 
						2 => 17413u, 
						3 => 17414u, 
						4 => 17415u, 
						5 => 17416u, 
						6 => 17417u, 
						7 => 17418u, 
						8 => 17419u, 
						9 => 17420u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17421u, 
						1 => 17422u, 
						2 => 17423u, 
						3 => 17424u, 
						4 => 17425u, 
						5 => 17426u, 
						6 => 17427u, 
						7 => 17428u, 
						8 => 17429u, 
						9 => 17430u, 
						10 => 17431u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17432u, 
						1 => 17433u, 
						2 => 17434u, 
						3 => 17435u, 
						4 => 17436u, 
						5 => 17437u, 
						6 => 17438u, 
						7 => 17439u, 
						8 => 17440u, 
						9 => 17441u, 
						10 => 17442u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17443u, 
						1 => 17444u, 
						2 => 17445u, 
						3 => 17446u, 
						4 => 17447u, 
						5 => 17448u, 
						6 => 17449u, 
						7 => 17450u, 
						8 => 17451u, 
						9 => 17452u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17453u, 
						1 => 17454u, 
						2 => 17455u, 
						3 => 17456u, 
						4 => 17457u, 
						5 => 17458u, 
						6 => 17459u, 
						7 => 17460u, 
						8 => 17461u, 
						9 => 17462u, 
						10 => 17463u, 
						11 => 17464u, 
						12 => 17465u, 
						13 => 17466u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17467u, 
						1 => 17468u, 
						2 => 17469u, 
						3 => 17470u, 
						4 => 17471u, 
						5 => 17472u, 
						6 => 17473u, 
						7 => 17474u, 
						8 => 17475u, 
						9 => 17476u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17477u, 
						1 => 17478u, 
						2 => 17479u, 
						3 => 17480u, 
						4 => 17481u, 
						5 => 17482u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.NightElf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17483u, 
						1 => 17484u, 
						2 => 17485u, 
						3 => 17486u, 
						4 => 17487u, 
						5 => 17488u, 
						6 => 17489u, 
						7 => 17490u, 
						8 => 17491u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17492u, 
						1 => 17493u, 
						2 => 17494u, 
						3 => 17495u, 
						4 => 17496u, 
						5 => 17497u, 
						6 => 17498u, 
						7 => 17499u, 
						8 => 17500u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17501u, 
						1 => 17502u, 
						2 => 17503u, 
						3 => 17504u, 
						4 => 17505u, 
						5 => 17506u, 
						6 => 17507u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17508u, 
						1 => 17509u, 
						2 => 17510u, 
						3 => 17511u, 
						4 => 17512u, 
						5 => 17513u, 
						6 => 17514u, 
						7 => 17515u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17516u, 
						1 => 17517u, 
						2 => 17518u, 
						3 => 17519u, 
						4 => 17520u, 
						5 => 17521u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17522u, 
						1 => 17523u, 
						2 => 17524u, 
						3 => 17525u, 
						4 => 17526u, 
						5 => 17527u, 
						6 => 17528u, 
						7 => 17529u, 
						8 => 17530u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17531u, 
						1 => 17532u, 
						2 => 17533u, 
						3 => 17534u, 
						4 => 17535u, 
						5 => 17536u, 
						6 => 17537u, 
						7 => 17538u, 
						8 => 17539u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17540u, 
						1 => 17541u, 
						2 => 17542u, 
						3 => 17543u, 
						4 => 17544u, 
						5 => 17545u, 
						6 => 17546u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17547u, 
						1 => 17548u, 
						2 => 17549u, 
						3 => 17550u, 
						4 => 17551u, 
						5 => 17552u, 
						6 => 17553u, 
						7 => 17554u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17555u, 
						1 => 17556u, 
						2 => 17557u, 
						3 => 17558u, 
						4 => 17559u, 
						5 => 17560u, 
						6 => 17561u, 
						7 => 17562u, 
						8 => 17563u, 
						9 => 17564u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Undead:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17565u, 
						1 => 17566u, 
						2 => 17567u, 
						3 => 17568u, 
						4 => 17569u, 
						5 => 17570u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17571u, 
						1 => 17572u, 
						2 => 17573u, 
						3 => 17574u, 
						4 => 17575u, 
						5 => 17576u, 
						6 => 17577u, 
						7 => 17578u, 
						8 => 17579u, 
						9 => 17580u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17581u, 
						1 => 17582u, 
						2 => 17583u, 
						3 => 17584u, 
						4 => 17585u, 
						5 => 17586u, 
						6 => 17587u, 
						7 => 17588u, 
						8 => 17589u, 
						9 => 17590u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17591u, 
						1 => 17592u, 
						2 => 17593u, 
						3 => 17594u, 
						4 => 17595u, 
						5 => 17596u, 
						6 => 17597u, 
						7 => 17598u, 
						8 => 17599u, 
						9 => 17600u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17601u, 
						1 => 17602u, 
						2 => 17603u, 
						3 => 17604u, 
						4 => 17605u, 
						5 => 17606u, 
						6 => 17607u, 
						7 => 17608u, 
						8 => 17609u, 
						9 => 17610u, 
						10 => 17611u, 
						11 => 17612u, 
						12 => 17613u, 
						13 => 17614u, 
						14 => 17615u, 
						15 => 17616u, 
						16 => 17617u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17618u, 
						1 => 17619u, 
						2 => 17620u, 
						3 => 17621u, 
						4 => 17622u, 
						5 => 17623u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17624u, 
						1 => 17625u, 
						2 => 17626u, 
						3 => 17627u, 
						4 => 17628u, 
						5 => 17629u, 
						6 => 17630u, 
						7 => 17631u, 
						8 => 17632u, 
						9 => 17633u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17634u, 
						1 => 17635u, 
						2 => 17636u, 
						3 => 17637u, 
						4 => 17638u, 
						5 => 17639u, 
						6 => 17640u, 
						7 => 17641u, 
						8 => 17642u, 
						9 => 17643u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17644u, 
						1 => 17645u, 
						2 => 17646u, 
						3 => 17647u, 
						4 => 17648u, 
						5 => 17649u, 
						6 => 17650u, 
						7 => 17651u, 
						8 => 17652u, 
						9 => 17653u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17654u, 
						1 => 17655u, 
						2 => 17656u, 
						3 => 17657u, 
						4 => 17658u, 
						5 => 17659u, 
						6 => 17660u, 
						7 => 17661u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Tauren:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17662u, 
						1 => 17663u, 
						2 => 17664u, 
						3 => 17665u, 
						4 => 17666u, 
						5 => 17667u, 
						6 => 17668u, 
						7 => 17669u, 
						8 => 17670u, 
						9 => 17671u, 
						10 => 17672u, 
						11 => 17673u, 
						12 => 17674u, 
						13 => 17675u, 
						14 => 17676u, 
						15 => 17677u, 
						16 => 17678u, 
						17 => 17679u, 
						18 => 17680u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17681u, 
						1 => 17682u, 
						2 => 17683u, 
						3 => 17684u, 
						4 => 17685u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17686u, 
						1 => 17687u, 
						2 => 17688u, 
						3 => 17689u, 
						4 => 17690u, 
						5 => 17691u, 
						6 => 17692u, 
						7 => 17693u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17694u, 
						1 => 17695u, 
						2 => 17696u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17697u, 
						1 => 17698u, 
						2 => 17699u, 
						3 => 17700u, 
						4 => 17701u, 
						5 => 17702u, 
						6 => 17703u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17704u, 
						1 => 17705u, 
						2 => 17706u, 
						3 => 17707u, 
						4 => 17708u, 
						5 => 17709u, 
						6 => 17710u, 
						7 => 17711u, 
						8 => 17712u, 
						9 => 17713u, 
						10 => 17714u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17715u, 
						1 => 17716u, 
						2 => 17717u, 
						3 => 17718u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17719u, 
						1 => 17720u, 
						2 => 17721u, 
						3 => 17722u, 
						4 => 17723u, 
						5 => 17724u, 
						6 => 17725u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17726u, 
						1 => 17727u, 
						2 => 17728u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17729u, 
						1 => 17730u, 
						2 => 17731u, 
						3 => 17732u, 
						4 => 17733u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Gnome:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17734u, 
						1 => 17735u, 
						2 => 17736u, 
						3 => 17737u, 
						4 => 17738u, 
						5 => 17739u, 
						6 => 17740u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17741u, 
						1 => 17742u, 
						2 => 17743u, 
						3 => 17744u, 
						4 => 17745u, 
						5 => 17746u, 
						6 => 17747u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17748u, 
						1 => 17749u, 
						2 => 17750u, 
						3 => 17751u, 
						4 => 17752u, 
						5 => 17753u, 
						6 => 17754u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17755u, 
						1 => 17756u, 
						2 => 17757u, 
						3 => 17758u, 
						4 => 17759u, 
						5 => 17760u, 
						6 => 17761u, 
						7 => 17762u, 
						8 => 17763u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17764u, 
						1 => 17765u, 
						2 => 17766u, 
						3 => 17767u, 
						4 => 17768u, 
						5 => 17769u, 
						6 => 17770u, 
						7 => 17771u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17772u, 
						1 => 17773u, 
						2 => 17774u, 
						3 => 17775u, 
						4 => 17776u, 
						5 => 17777u, 
						6 => 17778u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17779u, 
						1 => 17780u, 
						2 => 17781u, 
						3 => 17782u, 
						4 => 17783u, 
						5 => 17784u, 
						6 => 17785u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17786u, 
						1 => 17787u, 
						2 => 17788u, 
						3 => 17789u, 
						4 => 17790u, 
						5 => 17791u, 
						6 => 17792u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17793u, 
						1 => 17794u, 
						2 => 17795u, 
						3 => 17796u, 
						4 => 17797u, 
						5 => 17798u, 
						6 => 17799u, 
						7 => 17800u, 
						8 => 17801u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17802u, 
						1 => 17803u, 
						2 => 17804u, 
						3 => 17805u, 
						4 => 17806u, 
						5 => 17807u, 
						6 => 17808u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Troll:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17809u, 
						1 => 17810u, 
						2 => 17811u, 
						3 => 17812u, 
						4 => 17813u, 
						5 => 17814u, 
						6 => 17815u, 
						7 => 17816u, 
						8 => 17817u, 
						9 => 17818u, 
						10 => 17819u, 
						11 => 17820u, 
						12 => 17821u, 
						13 => 17822u, 
						14 => 17823u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17824u, 
						1 => 17825u, 
						2 => 17826u, 
						3 => 17827u, 
						4 => 17828u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17829u, 
						1 => 17830u, 
						2 => 17831u, 
						3 => 17832u, 
						4 => 17833u, 
						5 => 17834u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17835u, 
						1 => 17836u, 
						2 => 17837u, 
						3 => 17838u, 
						4 => 17839u, 
						5 => 17840u, 
						6 => 17841u, 
						7 => 17842u, 
						8 => 17843u, 
						9 => 17844u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17845u, 
						1 => 17846u, 
						2 => 17847u, 
						3 => 17848u, 
						4 => 17849u, 
						5 => 17850u, 
						6 => 17851u, 
						7 => 17852u, 
						8 => 17853u, 
						9 => 17854u, 
						10 => 17855u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17856u, 
						1 => 17857u, 
						2 => 17858u, 
						3 => 17859u, 
						4 => 17860u, 
						5 => 17861u, 
						6 => 17862u, 
						7 => 17863u, 
						8 => 17864u, 
						9 => 17865u, 
						10 => 17866u, 
						11 => 17867u, 
						12 => 17868u, 
						13 => 17869u, 
						14 => 17870u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17871u, 
						1 => 17872u, 
						2 => 17873u, 
						3 => 17874u, 
						4 => 17875u, 
						5 => 17876u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17877u, 
						1 => 17878u, 
						2 => 17879u, 
						3 => 17880u, 
						4 => 17881u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17882u, 
						1 => 17883u, 
						2 => 17884u, 
						3 => 17885u, 
						4 => 17886u, 
						5 => 17887u, 
						6 => 17888u, 
						7 => 17889u, 
						8 => 17890u, 
						9 => 17891u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17892u, 
						1 => 17893u, 
						2 => 17894u, 
						3 => 17895u, 
						4 => 17896u, 
						5 => 17897u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.BloodElf:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17906u, 
						1 => 17907u, 
						2 => 17908u, 
						3 => 17909u, 
						4 => 17910u, 
						5 => 17911u, 
						6 => 17912u, 
						7 => 17913u, 
						8 => 17914u, 
						9 => 17915u, 
						10 => 17916u, 
						11 => 17917u, 
						12 => 17918u, 
						13 => 17919u, 
						14 => 17920u, 
						15 => 17921u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17922u, 
						1 => 17923u, 
						2 => 17924u, 
						3 => 17925u, 
						4 => 17926u, 
						5 => 17927u, 
						6 => 17928u, 
						7 => 17929u, 
						8 => 17930u, 
						9 => 17931u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17932u, 
						1 => 17933u, 
						2 => 17934u, 
						3 => 17935u, 
						4 => 17936u, 
						5 => 17937u, 
						6 => 17938u, 
						7 => 17939u, 
						8 => 17940u, 
						9 => 17941u, 
						10 => 17942u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 17943u, 
						1 => 17944u, 
						2 => 17945u, 
						3 => 17946u, 
						4 => 17947u, 
						5 => 17948u, 
						6 => 17949u, 
						7 => 17950u, 
						8 => 17951u, 
						9 => 17952u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 17953u, 
						1 => 17954u, 
						2 => 17955u, 
						3 => 17956u, 
						4 => 17957u, 
						5 => 17958u, 
						6 => 17959u, 
						7 => 17960u, 
						8 => 17961u, 
						9 => 17962u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 17963u, 
						1 => 17964u, 
						2 => 17965u, 
						3 => 17966u, 
						4 => 17967u, 
						5 => 17968u, 
						6 => 17969u, 
						7 => 17970u, 
						8 => 17971u, 
						9 => 17972u, 
						10 => 17973u, 
						11 => 17974u, 
						12 => 17975u, 
						13 => 17976u, 
						14 => 17977u, 
						15 => 17978u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 17979u, 
						1 => 17980u, 
						2 => 17981u, 
						3 => 17982u, 
						4 => 17983u, 
						5 => 17984u, 
						6 => 17985u, 
						7 => 17986u, 
						8 => 17987u, 
						9 => 17988u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 17989u, 
						1 => 17990u, 
						2 => 17991u, 
						3 => 17992u, 
						4 => 17993u, 
						5 => 17994u, 
						6 => 17995u, 
						7 => 17996u, 
						8 => 17997u, 
						9 => 17998u, 
						10 => 17999u, 
						11 => 18000u, 
						12 => 18001u, 
						13 => 18002u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 18004u, 
						1 => 18005u, 
						2 => 18006u, 
						3 => 18007u, 
						4 => 18008u, 
						5 => 18009u, 
						6 => 18010u, 
						7 => 18011u, 
						8 => 18012u, 
						9 => 18013u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 18014u, 
						1 => 18015u, 
						2 => 18016u, 
						3 => 18017u, 
						4 => 18018u, 
						5 => 18019u, 
						6 => 18020u, 
						7 => 18021u, 
						8 => 18022u, 
						9 => 18023u, 
						10 => 18024u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		case Race.Draenei:
			if (gender == Gender.Male)
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 18025u, 
						1 => 18026u, 
						2 => 18027u, 
						3 => 18028u, 
						4 => 18029u, 
						5 => 18030u, 
						6 => 18031u, 
						7 => 18032u, 
						8 => 18033u, 
						9 => 18034u, 
						10 => 18035u, 
						11 => 18036u, 
						12 => 18037u, 
						13 => 18038u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 18039u, 
						1 => 18040u, 
						2 => 18041u, 
						3 => 18042u, 
						4 => 18043u, 
						5 => 18044u, 
						6 => 18045u, 
						7 => 18046u, 
						8 => 18047u, 
						9 => 18048u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 18049u, 
						1 => 18050u, 
						2 => 18051u, 
						3 => 18052u, 
						4 => 18053u, 
						5 => 18054u, 
						6 => 18055u, 
						7 => 18056u, 
						8 => 18057u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 18058u, 
						1 => 18059u, 
						2 => 18060u, 
						3 => 18061u, 
						4 => 18062u, 
						5 => 18063u, 
						6 => 18064u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 18065u, 
						1 => 18066u, 
						2 => 18067u, 
						3 => 18068u, 
						4 => 18069u, 
						5 => 18070u, 
						6 => 18071u, 
						7 => 18072u, 
						_ => 0u, 
					};
				}
			}
			else
			{
				switch (option)
				{
				case LegacyCustomizationOption.Skin:
					return value switch
					{
						0 => 18073u, 
						1 => 18074u, 
						2 => 18075u, 
						3 => 18076u, 
						4 => 18077u, 
						5 => 18078u, 
						6 => 18079u, 
						7 => 18080u, 
						8 => 18081u, 
						9 => 18082u, 
						10 => 18083u, 
						11 => 18084u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.Face:
					return value switch
					{
						0 => 18085u, 
						1 => 18086u, 
						2 => 18087u, 
						3 => 18088u, 
						4 => 18089u, 
						5 => 18090u, 
						6 => 18091u, 
						7 => 18092u, 
						8 => 18093u, 
						9 => 18094u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairStyle:
					return value switch
					{
						0 => 18095u, 
						1 => 18096u, 
						2 => 18097u, 
						3 => 18098u, 
						4 => 18099u, 
						5 => 18100u, 
						6 => 18101u, 
						7 => 18102u, 
						8 => 18103u, 
						9 => 18104u, 
						10 => 18105u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.HairColor:
					return value switch
					{
						0 => 18106u, 
						1 => 18107u, 
						2 => 18108u, 
						3 => 18109u, 
						4 => 18110u, 
						5 => 18111u, 
						6 => 18112u, 
						_ => 0u, 
					};
				case LegacyCustomizationOption.FacialHair:
					return value switch
					{
						0 => 18113u, 
						1 => 18114u, 
						2 => 18115u, 
						3 => 18116u, 
						4 => 18117u, 
						5 => 18118u, 
						6 => 18119u, 
						_ => 0u, 
					};
				}
			}
			return 0u;
		default:
			return 0u;
		}
	}

	public static Array<ChrCustomizationChoice> ConvertLegacyCustomizationsToModern(Race raceId, Gender gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair)
	{
		return new Array<ChrCustomizationChoice>(5)
		{
			[0] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.Skin), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.Skin, skin)),
			[1] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.Face), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.Face, face)),
			[2] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.HairStyle), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.HairStyle, hairStyle)),
			[3] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.HairColor), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.HairColor, hairColor)),
			[4] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.FacialHair), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.FacialHair, facialHair))
		};
	}
}
