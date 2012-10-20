using System;
using SlimShader.IO;
using SlimShader.ObjectModel;
using SlimShader.ObjectModel.Tokens;
using SlimShader.Parser.Opcodes.Declarations;

namespace SlimShader.Parser
{

	public class ShaderProgramParser : BytecodeParser<ShaderProgram>
	{
		public ShaderProgramParser(BytecodeReader reader)
			: base(reader)
		{
			
		}

		public override ShaderProgram Parse()
		{
			var headerReader = Reader.CopyAtCurrentPosition();
			var program = new ShaderProgram();

			// Version Token (VerTok)
			// [07:00] minor version number (0-255)
			// [15:08] major version number (0-255)
			// [31:16] D3D10_SB_TOKENIZED_PROGRAM_TYPE
			uint versionToken = headerReader.ReadUInt32();
			program.Version = new ShaderVersion
			{
				MinorVersion = versionToken.DecodeValue<byte>(0, 3),
				MajorVersion = versionToken.DecodeValue<byte>(4, 7),
				ProgramType = versionToken.DecodeValue<ProgramType>(16, 31)
			};

			// Length Token (LenTok)
			// Always follows VerTok
			// [31:00] Unsigned integer count of number of DWORDs in program code, including version and length tokens.
			// So the minimum value is 0x00000002 (if an empty program is ever valid).
			program.Length = headerReader.ReadUInt32();

			while (!Reader.EndOfBuffer)
			{
				// Opcode Format (OpcodeToken0)
				//
				// [10:00] D3D10_SB_OPCODE_TYPE
				// if( [10:00] == D3D10_SB_OPCODE_CUSTOMDATA )
				// {
				//    Token starts a custom-data block.  See "Custom-Data Block Format".
				// }
				// else // standard opcode token
				// {
				//    [23:11] Opcode-Specific Controls
				//    [30:24] Instruction length in DWORDs including the opcode token.
				//    [31]    0 normally. 1 if extended operand definition, meaning next DWORD
				//            contains extended opcode token.
				// }
				var opcodeHeaderReader = Reader.CopyAtCurrentPosition();
				var opcodeToken0 = opcodeHeaderReader.ReadUInt32();
				var opcodeHeader = new OpcodeHeader
				{
					OpcodeType = opcodeToken0.DecodeValue<OpcodeType>(0, 10),
					Length = opcodeToken0.DecodeValue(24, 30),
					IsExtended = (opcodeToken0.DecodeValue(31, 31) == 1)
				};

				OpcodeToken opcodeToken = null;
				switch (opcodeHeader.OpcodeType)
				{
					case OpcodeType.DclGlobalFlags:
						opcodeToken = new GlobalFlagsDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclResource:
						opcodeToken = new ResourceDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclSampler:
						opcodeToken = new SamplerDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclInput:
					case OpcodeType.DclInputSgv:
					case OpcodeType.DclInputSiv:
					case OpcodeType.DclInputPs:
					case OpcodeType.DclInputPsSgv:
					case OpcodeType.DclInputPsSiv:
						opcodeToken = new InputRegisterDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclOutput :
					case OpcodeType.DclOutputSgv :
					case OpcodeType.DclOutputSiv :
						opcodeToken = new OutputRegisterDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclIndexRange :
						opcodeToken = new IndexingRangeDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclTemps :
						opcodeToken = new TempRegisterDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclIndexableTemp:
						opcodeToken = new IndexableTempRegisterDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclConstantBuffer :
						opcodeToken = new ConstantBufferDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.CustomData :
					{
						// Custom-Data Block Format
						//
						// DWORD 0 (CustomDataDescTok):
						// [10:00] == D3D10_SB_OPCODE_CUSTOMDATA
						// [31:11] == D3D10_SB_CUSTOMDATA_CLASS
						//
						// DWORD 1: 
						//          32-bit unsigned integer count of number
						//          of DWORDs in custom-data block,
						//          including DWORD 0 and DWORD 1.
						//          So the minimum value is 0x00000002,
						//          meaning empty custom-data.
						//
						// Layout of custom-data contents, for the various meta-data classes,
						// not defined in this file.
						var customDataClass = opcodeToken0.DecodeValue<CustomDataClass>(11, 31);
						switch (customDataClass)
						{
							case CustomDataClass.DclImmediateConstantBuffer :
								opcodeToken = new ImmediateConstantBufferDeclarationParser(Reader).Parse();
								break;
							case CustomDataClass.ShaderMessage :
								opcodeToken = new ShaderMessageDeclarationParser(Reader).Parse();
								break;
							default :
								throw new ArgumentOutOfRangeException();
						}
						break;
					}
					case OpcodeType.DclGsInputPrimitive :
						opcodeToken = new GeometryShaderInputPrimitiveDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclGsOutputPrimitiveTopology :
						opcodeToken = new GeometryShaderOutputPrimitiveTopologyDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclMaxOutputVertexCount :
						opcodeToken = new GeometryShaderMaxOutputVertexCountDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclGsInstanceCount:
						opcodeToken = new GeometryShaderInstanceCountDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclInputControlPointCount :
					case OpcodeType.DclOutputControlPointCount :
						opcodeToken = new ControlPointCountDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclTessDomain :
						opcodeToken = new TessellatorDomainDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclTessPartitioning:
						opcodeToken = new TessellatorPartitioningDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclTessOutputPrimitive:
						opcodeToken = new TessellatorOutputPrimitiveDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclHsMaxTessFactor:
						opcodeToken = new HullShaderMaxTessFactorDeclarationParser(Reader).Parse();
						break;
					case OpcodeType.DclHsForkPhaseInstanceCount:
						opcodeToken = new HullShaderForkPhaseInstanceCountDeclarationParser(Reader).Parse();
						break;
					default:
						Reader.ReadUInt32();
						if (opcodeHeader.IsExtended)
							Reader.ReadUInt32();
						break;
				}

				if (opcodeToken != null)
				{
					opcodeToken.Header = opcodeHeader;
					program.Tokens.Add(opcodeToken);
				}
			}

			return program;
		}
	}
}