﻿using SlimShader;

namespace Rasterizr.Pipeline.Rasterizer
{
	internal struct Fragment
	{
		public int X;
		public int Y;
		public FragmentQuadLocation QuadLocation;
		public Samples Samples;
		public Number4[] Data;

		public Fragment(int x, int y, FragmentQuadLocation quadLocation)
		{
			X = x;
			Y = y;
			QuadLocation = quadLocation;
			Samples = new Samples();
			Data = null;
		}
	}
}