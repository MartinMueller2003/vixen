﻿namespace Vixen.Intents.Interpolators {
	class FloatInterpolator : Interpolator<float> {
		protected override float InterpolateValue(float startValue, float endValue, float percent) {
			return startValue + (endValue - startValue) * percent;
		}
	}
}
