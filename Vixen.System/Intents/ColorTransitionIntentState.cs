﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Vixen.Intents.Interpolators;
using Vixen.Sys;

namespace Vixen.Intents {
	class ColorTransitionIntentState : Dispatchable<ColorTransitionIntentState>, IIntentState<Color> {
		private ColorTransitionIntent _intent;
		private ColorInterpolator _interpolator;

		public ColorTransitionIntentState(ColorTransitionIntent intent, TimeSpan intentRelativeTime) {
			_intent = intent;
			RelativeTime = intentRelativeTime;
			_interpolator = new ColorInterpolator();
			FilterStates = new List<IFilterState>();
		}

		public TimeSpan RelativeTime { get; private set; }

		public List<IFilterState> FilterStates { get; private set; }

		public Color GetValue() {
			Color value;
			_interpolator.Interpolate(RelativeTime, _intent.TimeSpan, _intent.StartValue, _intent.EndValue, out value);
			return value;
		}

		public IIntentState Clone() {
			ColorTransitionIntentState newState = new ColorTransitionIntentState(_intent, RelativeTime);
			newState.FilterStates.AddRange(FilterStates);
			return newState;
		}
	}
}
