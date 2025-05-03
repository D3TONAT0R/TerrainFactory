namespace TerrainFactory.Util
{
	public struct Range
	{
		public float Min;
		public float Max;

		public float Extent => Max - Min;

		public Range(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public float Lerp(float t)
		{
			return MathUtils.Lerp(Min, Max, t);
		}

		public float LerpClamped(float t)
		{
			return MathUtils.Clamp01(Lerp(t));
		}

		public float InverseLerp(float value)
		{
			return MathUtils.InverseLerp(Min, Max, value);
		}

		public float InverseLerpClamped(float value)
		{
			return MathUtils.Clamp01(InverseLerp(value));
		}
	}
}