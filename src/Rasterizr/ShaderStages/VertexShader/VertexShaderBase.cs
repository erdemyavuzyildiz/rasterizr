using System;
namespace Rasterizr.ShaderStages.VertexShader
{
	public abstract class VertexShaderBase<TVertexShaderInput, TVertexShaderOutput> : ShaderBase, IVertexShader
		where TVertexShaderOutput : IVertexShaderOutput, new()
	{
		public Type VertexShaderInputType
		{
			get { return typeof(TVertexShaderInput); }
		}

		public Type VertexShaderOutputType
		{
			get { return typeof(TVertexShaderOutput); }
		}

		public abstract TVertexShaderOutput Execute(TVertexShaderInput vertexShaderInput);

		public IVertexShaderOutput Execute(object vertexShaderInput)
		{
			// Cast VertexShaderInput to TVertexShaderInput.
			var input = (TVertexShaderInput) vertexShaderInput;

			// Execute VertexShader.
			TVertexShaderOutput output = Execute(input);

			// Convert TVertexShaderOutput to VertexShaderOutput.
			return output;
		}
	}
}