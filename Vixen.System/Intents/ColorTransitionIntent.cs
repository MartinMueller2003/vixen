﻿using System;
using System.Drawing;
using Vixen.Intents.Interpolators;
using Vixen.Sys;

namespace Vixen.Intents {
	public class ColorTransitionIntent : TransitionIntent<Color> {
		public ColorTransitionIntent(Color startValue, Color endValue, TimeSpan timeSpan)
			: base(startValue, endValue, timeSpan, new ColorInterpolator()) {
		}

		public override IIntentState CreateIntentState(TimeSpan intentRelativeTime) {
			return new ColorTransitionIntentState(this, intentRelativeTime);
		}
	}
}
