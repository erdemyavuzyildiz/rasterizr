using System;
using SlimShader.IO;
using SlimShader.ObjectModel;
using SlimShader.ObjectModel.Tokens;

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
			var program = new ShaderProgram();

			// Version Token (VerTok)
			// [07:00] minor version number (0-255)
			// [15:08] major version number (0-255)
			// [31:16] D3D10_SB_TOKENIZED_PROGRAM_TYPE
			uint versionToken = Reader.ReadUInt32();
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
			program.Length = Reader.ReadUInt32();

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

				OpcodeToken opcodeToken;
				if (opcodeHeader.OpcodeType == OpcodeType.CustomData)
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
						case CustomDataClass.DclImmediateConstantBuffer:
							opcodeToken = ImmediateConstantBufferDeclarationToken.Parse(Reader);
							break;
						case CustomDataClass.ShaderMessage:
							opcodeToken = ShaderMessageDeclarationToken.Parse(Reader);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				else if (opcodeHeader.OpcodeType.IsDeclaration())
				{
					switch (opcodeHeader.OpcodeType)
					{
						case OpcodeType.DclGlobalFlags:
							opcodeToken = GlobalFlagsDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclResource:
							opcodeToken = ResourceDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclSampler:
							opcodeToken = SamplerDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclInput:
						case OpcodeType.DclInputSgv:
						case OpcodeType.DclInputSiv:
						case OpcodeType.DclInputPs:
						case OpcodeType.DclInputPsSgv:
						case OpcodeType.DclInputPsSiv:
							opcodeToken = InputRegisterDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclOutput:
						case OpcodeType.DclOutputSgv:
						case OpcodeType.DclOutputSiv:
							opcodeToken = OutputRegisterDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclIndexRange:
							opcodeToken = IndexingRangeDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclTemps:
							opcodeToken = TempRegisterDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclIndexableTemp:
							opcodeToken = IndexableTempRegisterDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclConstantBuffer:
							opcodeToken = ConstantBufferDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclGsInputPrimitive:
							opcodeToken = GeometryShaderInputPrimitiveDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclGsOutputPrimitiveTopology:
							opcodeToken = GeometryShaderOutputPrimitiveTopologyDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclMaxOutputVertexCount:
							opcodeToken = GeometryShaderMaxOutputVertexCountDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclGsInstanceCount:
							opcodeToken = GeometryShaderInstanceCountDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclInputControlPointCount:
						case OpcodeType.DclOutputControlPointCount:
							opcodeToken = ControlPointCountDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclTessDomain:
							opcodeToken = TessellatorDomainDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclTessPartitioning:
							opcodeToken = TessellatorPartitioningDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclTessOutputPrimitive:
							opcodeToken = TessellatorOutputPrimitiveDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclHsMaxTessFactor:
							opcodeToken = HullShaderMaxTessFactorDeclarationToken.Parse(Reader);
							break;
						case OpcodeType.DclHsForkPhaseInstanceCount:
							opcodeToken = HullShaderForkPhaseInstanceCountDeclarationToken.Parse(Reader);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				else // Not custom data or declaration, so must be instruction.
				{
					var instructionToken = new InstructionToken();

					instructionToken.Saturate = (opcodeToken0.DecodeValue(13, 13) == 1);
					instructionToken.TestBoolean = opcodeToken0.DecodeValue<InstructionTestBoolean>(18, 18);

					// Advance to next token.
					var instructionEnd = Reader.CurrentPosition + (opcodeHeader.Length * sizeof(uint));
					Reader.ReadUInt32();

					bool extended = opcodeHeader.IsExtended;
					while (extended)
					{
						uint extendedToken = Reader.ReadUInt32();
						var extendedType = extendedToken.DecodeValue<InstructionTokenExtendedType>(0, 6);
						instructionToken.ExtendedTypes.Add(extendedType);
						extended = (extendedToken.DecodeValue(31, 31) == 1);

						switch (extendedType)
						{
							case InstructionTokenExtendedType.SampleControls:
								instructionToken.SampleOffsets[0] = extendedToken.DecodeValue(09, 12);
								instructionToken.SampleOffsets[1] = extendedToken.DecodeValue(13, 16);
								instructionToken.SampleOffsets[2] = extendedToken.DecodeValue(17, 20);
								break;
							case InstructionTokenExtendedType.ResourceDim:
								instructionToken.ResourceTarget = extendedToken.DecodeValue<byte>(6, 10);
								break;
							case InstructionTokenExtendedType.ResourceReturnType:
								instructionToken.ResourceReturnTypes[0] = extendedToken.DecodeValue<byte>(06, 09);
								instructionToken.ResourceReturnTypes[1] = extendedToken.DecodeValue<byte>(10, 12);
								instructionToken.ResourceReturnTypes[2] = extendedToken.DecodeValue<byte>(13, 16);
								instructionToken.ResourceReturnTypes[3] = extendedToken.DecodeValue<byte>(17, 20);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}

					if (opcodeHeader.OpcodeType == OpcodeType.InterfaceCall)
					{
						// TODO
						Reader.ReadUInt32();
					}

					while (Reader.CurrentPosition < instructionEnd)
					{
						instructionToken.Operands.Add(new OperandParser(Reader,
							opcodeHeader.OpcodeType.IsIntegralTypeInstruction()).Parse());
					}

					opcodeToken = instructionToken;
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