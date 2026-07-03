namespace HermesProxy.World.Enums;

public static class Weather
{
	public static WeatherState ConvertWeatherTypeToWeatherState(WeatherType type, float grade)
	{
		switch (type)
		{
		case WeatherType.Fine:
			return WeatherState.Fine;
		case WeatherType.Rain:
			if (grade <= 0.25f)
			{
				return WeatherState.Drizzle;
			}
			if (grade <= 0.3f)
			{
				return WeatherState.LightRain;
			}
			if (grade <= 0.6f)
			{
				return WeatherState.MediumRain;
			}
			return WeatherState.HeavyRain;
		case WeatherType.Snow:
			if (grade <= 0.3f)
			{
				return WeatherState.LightSnow;
			}
			if (grade <= 0.6f)
			{
				return WeatherState.MediumSnow;
			}
			return WeatherState.HeavySnow;
		case WeatherType.Storm:
			if (grade <= 0.3f)
			{
				return WeatherState.LightSandstorm;
			}
			if (grade <= 0.6f)
			{
				return WeatherState.MediumSandstorm;
			}
			return WeatherState.HeavySandstorm;
		default:
			return WeatherState.Fine;
		}
	}
}
