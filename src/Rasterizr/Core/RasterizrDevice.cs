using System.Collections.Generic;
using Nexus;
using Rasterizr.Core.InputAssembler;
using Rasterizr.Core.OutputMerger;
using Rasterizr.Core.Rasterizer;
using Rasterizr.Core.ShaderCore.GeometryShader;
using Rasterizr.Core.ShaderCore.PixelShader;
using Rasterizr.Core.ShaderCore.VertexShader;

namespace Rasterizr.Core
{
	public class RasterizrDevice
	{
		#region Fields

		private readonly List<object> _inputAssemblerOutputs;
		private readonly List<IVertexShaderOutput> _vertexShaderOutputs;
		private readonly List<IVertexShaderOutput> _geometryShaderOutputs;
		private readonly List<Fragment> _rasterizerOutputs;
		private readonly List<Pixel> _pixelShaderOutputs;

		#endregion

		#region Properties

		public InputAssemblerStage InputAssembler { get; private set; }
		public VertexShaderStage VertexShader { get; private set; }
		public GeometryShaderStage GeometryShader { get; private set; }
		public RasterizerStage Rasterizer { get; private set; }
		public PixelShaderStage PixelShader { get; private set; }
		public OutputMergerStage OutputMerger { get; private set; }

		#endregion

		#region Constructor

		public RasterizrDevice()
		{
			_inputAssemblerOutputs = new List<object>();
			_vertexShaderOutputs = new List<IVertexShaderOutput>();
			_geometryShaderOutputs = new List<IVertexShaderOutput>();
			_rasterizerOutputs = new List<Fragment>();
			_pixelShaderOutputs = new List<Pixel>();

			InputAssembler = new InputAssemblerStage();
			VertexShader = new VertexShaderStage();
			GeometryShader = new GeometryShaderStage();

			PixelShader = new PixelShaderStage();
			OutputMerger = new OutputMergerStage();

			Rasterizer = new RasterizerStage(VertexShader, PixelShader, OutputMerger);
		}

		#endregion

		public void ClearDepthBuffer(float depth)
		{
			OutputMerger.DepthBuffer.Clear(depth);
		}

		public void ClearRenderTarget(ColorF color)
		{
			OutputMerger.RenderTarget.Clear(color);
		}

		public void Draw()
		{
			_inputAssemblerOutputs.Clear();
			_vertexShaderOutputs.Clear();
			_geometryShaderOutputs.Clear();
			_rasterizerOutputs.Clear();
			_pixelShaderOutputs.Clear();

			InputAssembler.Run(_inputAssemblerOutputs);
			VertexShader.Run(_inputAssemblerOutputs, _vertexShaderOutputs);
			GeometryShader.Run(_vertexShaderOutputs, _geometryShaderOutputs);
			Rasterizer.Run(_geometryShaderOutputs, _rasterizerOutputs);
			PixelShader.Run(_rasterizerOutputs, _pixelShaderOutputs);
			OutputMerger.Run(_pixelShaderOutputs);
		}
	}
}