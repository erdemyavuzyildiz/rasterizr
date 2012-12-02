﻿using Rasterizr.Diagnostics;

namespace Rasterizr.Pipeline.OutputMerger
{
	public class DepthStencilState : DeviceChild
	{
		private readonly DepthStencilStateDescription _description;

		public DepthStencilStateDescription Description
		{
			get { return _description; }
		}

		public DepthStencilState(Device device, DepthStencilStateDescription description)
			: base(device)
		{
			device.Loggers.BeginOperation(OperationType.DepthStencilStateCreate, description);
			_description = description;
		}

		internal bool DepthTestPasses(float newDepth, float currentDepth)
		{
			if (!Description.IsDepthEnabled)
				return true;

			return ComparisonUtility.DoComparison(Description.DepthComparison,
				newDepth, currentDepth);
		}
	}
}