﻿using System;

namespace UFE3D
{
	public class DelayedAction
	{
		public Action action;
		public int steps;

		public DelayedAction(Action action, int steps)
		{
			this.action = action;
			this.steps = steps;
		}
	}
}