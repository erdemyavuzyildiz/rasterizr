﻿namespace SlimShader.ObjectModel
{
	public enum ResourceDimension
	{
		RESOURCE_DIMENSION_UNKNOWN = 0,
		RESOURCE_DIMENSION_BUFFER = 1,
		RESOURCE_DIMENSION_TEXTURE1D = 2,
		RESOURCE_DIMENSION_TEXTURE2D = 3,
		RESOURCE_DIMENSION_TEXTURE2DMS = 4,
		RESOURCE_DIMENSION_TEXTURE3D = 5,
		RESOURCE_DIMENSION_TEXTURECUBE = 6,
		RESOURCE_DIMENSION_TEXTURE1DARRAY = 7,
		RESOURCE_DIMENSION_TEXTURE2DARRAY = 8,
		RESOURCE_DIMENSION_TEXTURE2DMSARRAY = 9,
		RESOURCE_DIMENSION_TEXTURECUBEARRAY = 10,
		RESOURCE_DIMENSION_RAW_BUFFER = 11,
		RESOURCE_DIMENSION_STRUCTURED_BUFFER = 12,
	}
}