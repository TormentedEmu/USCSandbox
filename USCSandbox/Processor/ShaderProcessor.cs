﻿using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.Converter;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using AssetRipper.Primitives;
using AssetsTools.NET;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using System.Globalization;
using System.Text;
using USCSandbox.Extras;

namespace USCSandbox.Processor
{
    internal class ShaderProcessor
    {
        private AssetTypeValueField _shaderBf;
        private GPUPlatform _platformId;
        private UnityVersion _engVer;
        private StringBuilderIndented _sb;

        public ShaderProcessor(AssetTypeValueField shaderBf, UnityVersion engVer, GPUPlatform platformId)
        {
            _engVer = engVer;
            _shaderBf = shaderBf;
            _platformId = platformId;
            _sb = new StringBuilderIndented();
        }

        public string Process()
        {
            _sb.Clear();

            var parsedForm = _shaderBf["m_ParsedForm"];
            var name = parsedForm["m_Name"].AsString;
            var keywordNames = parsedForm["m_KeywordNames.Array"].Select(i => i.AsString).ToList();

            var platforms = _shaderBf["platforms.Array"].Select(i => i.AsInt).ToList();
            var offsets = _shaderBf["offsets.Array"];
            var compressedLengths = _shaderBf["compressedLengths.Array"];
            var decompressedLengths = _shaderBf["decompressedLengths.Array"];
            var compressedBlob = _shaderBf["compressedBlob.Array"].AsByteArray;

            var selectedIndex = platforms.IndexOf((int)_platformId);

            var selectedOffset = offsets[selectedIndex]["Array"][0].AsUInt;
            var selectedCompressedLength = compressedLengths[selectedIndex]["Array"][0].AsUInt;
            var selectedDecompressedLength = decompressedLengths[selectedIndex]["Array"][0].AsUInt;

            var decompressedBlob = new byte[selectedDecompressedLength];
            var lz4Decoder = new Lz4DecoderStream(new MemoryStream(compressedBlob));
            lz4Decoder.Read(decompressedBlob, 0, (int)selectedDecompressedLength);
            lz4Decoder.Dispose();

            var blobManager = new BlobManager(decompressedBlob, _engVer);
            for (var i = 0; i < blobManager.Entries.Count; i++)
            {
                var entryBytes = blobManager.GetRawEntry(i);
                File.WriteAllBytes($"dbg_entry_{i}.bin", entryBytes);
            }

            _sb.AppendLine($"Shader \"{name}\" {{");
            _sb.Indent();
            {
                WriteProperties(parsedForm["m_PropInfo"]);
                WriteSubShaders(blobManager, parsedForm);
            }
            _sb.Unindent();
            _sb.AppendLine("}");

            return _sb.ToString();
        }

        private void WritePassBody(
            BlobManager blobManager,
            List<ShaderProgramBasket> baskets,
            int depth)
        {
            _sb.AppendLine("CGPROGRAM");

            var defineSb = new StringBuilder();
            var structSb = new StringBuilder();
            var cbufferSb = new StringBuilder();
            var texSb = new StringBuilder();
            var memeSb = new StringBuilder();
            var codeSb = new StringBuilder();
            var declaredCBufs = new HashSet<string>();
            defineSb.AppendLine(new string(' ', depth * 4));
            structSb.AppendLine(new string(' ', depth * 4));
            foreach (var basket in baskets)
            {
                var progInfo = basket.ProgramInfo;
                var subProgInfo = basket.SubProgramInfo;
                var index = basket.ParameterBlobIndex;

                var subProg = blobManager.GetShaderSubProgram((int)subProgInfo.BlobIndex);
                File.WriteAllBytes($"dbg_entry_data_{subProgInfo.BlobIndex}.bin", subProg.ProgramData);

                ShaderParams param;
                if (index != -1)
                {
                    param = blobManager.GetShaderParams(index);
                }
                else
                {
                    param = subProg.ShaderParams;
                }

                Console.WriteLine($"working on {basket.SubProgramInfo.BlobIndex}");

                param.CombineCommon(progInfo);

                var programType = subProg.GetProgramType(_engVer);
                var graphicApi = _platformId;

                defineSb.Append(new string(' ', depth * 4));
                defineSb.AppendLine(programType switch
                {
                    ShaderGpuProgramType.DX11VertexSM40 => "#pragma vertex vert",
                    ShaderGpuProgramType.DX11PixelSM40 => "#pragma fragment frag",
                    ShaderGpuProgramType.ConsoleVS => "#pragma vertex vert",
                    ShaderGpuProgramType.ConsoleFS => "#pragma fragment frag",
                    _ => $"// unknown shader type {programType}"
                });

                var keywords = subProg.GlobalKeywords.Concat(subProg.LocalKeywords);

                switch (programType)
                {
                    case ShaderGpuProgramType.DX11VertexSM40:
                    case ShaderGpuProgramType.DX11PixelSM40:
                    {
                        // DBG
                        // int ifDepth = 0;
                        // foreach (var inst in conv.DxShader!.Shdr.shaderInstructions)
                        // {
                        //     memeSb.Append(new string(' ', depth * 4));
                        //     memeSb.AppendLine("// " + DirectXDisassemblyHelper.DisassembleInstruction(inst, ref ifDepth));
                        // }
                        // ///

                        var conv = new USCShaderConverter();
                        conv.LoadDirectXCompiledShader(new MemoryStream(subProg.ProgramData), graphicApi, _engVer);
                        conv.ConvertDxShaderToUShaderProgram();
                        conv.ApplyMetadataToProgram(subProg, param, _engVer);

                        UShaderFunctionToHLSL hlslConverter = new UShaderFunctionToHLSL(conv.ShaderProgram!, depth);
                        structSb.Append(hlslConverter.WriteStruct());
                        structSb.AppendLine();

                        codeSb.Append(new string(' ', depth * 4));
                        codeSb.AppendLine("// Keywords: " + string.Join(", ", keywords));
                        codeSb.Append(hlslConverter.WriteFunction());

                        break;
                    }
                    case ShaderGpuProgramType.ConsoleVS:
                    case ShaderGpuProgramType.ConsoleFS:
                    {
                        var conv = new USCShaderConverter();
                        conv.LoadUnityNvnShader(new MemoryStream(subProg.ProgramData), graphicApi, _engVer);
                        conv.ConvertNvnShaderToUShaderProgram(programType);
                        conv.ApplyMetadataToProgram(subProg, param, _engVer);

                        UShaderFunctionToHLSL hlslConverter = new UShaderFunctionToHLSL(conv.ShaderProgram!, depth);
                        structSb.Append(hlslConverter.WriteStruct());
                        structSb.AppendLine();

                        codeSb.Append(new string(' ', depth * 4));
                        codeSb.AppendLine("// Keywords: " + string.Join(", ", keywords));
                        codeSb.Append(hlslConverter.WriteFunction());

                        break;
                    }
                }

                cbufferSb.Append(new string(' ', depth * 4));
                cbufferSb.AppendLine($"// CBs for {programType}");
                foreach (ConstantBuffer cbuffer in param.ConstantBuffers)
                {
                    //if (!UnityShaderConstants.BUILTIN_CBUFFER_NAMES.Contains(cbuffer.Name))
                    {
                        cbufferSb.Append(WritePassCBuffer(param, declaredCBufs, cbuffer, depth));
                    }
                }
                cbufferSb.AppendLine();

                texSb.Append(new string(' ', depth * 4));
                texSb.AppendLine($"// Textures for {programType}");
                texSb.Append(WritePassTextures(param, declaredCBufs, depth));
                texSb.AppendLine();
            }

            _sb.AppendNoIndent(defineSb.ToString());
            _sb.AppendNoIndent(structSb.ToString());
            _sb.AppendNoIndent(cbufferSb.ToString());
            _sb.AppendNoIndent(texSb.ToString());
            _sb.AppendNoIndent(memeSb.ToString());
            _sb.AppendNoIndent(codeSb.ToString());

            _sb.AppendLine("ENDCG");
            _sb.AppendLine("");
        }

        private string WritePassCBuffer(
            ShaderParams shaderParams, HashSet<string> declaredCBufs,
            ConstantBuffer? cbuffer, int depth)
        {
            StringBuilder sb = new StringBuilder();
            if (cbuffer != null)
            {
                bool nonGlobalCbuffer = cbuffer.Name != "$Globals";
                int cbufferIndex = shaderParams.ConstantBuffers.IndexOf(cbuffer);

                if (nonGlobalCbuffer)
                {
                    sb.Append(new string(' ', depth * 4)); // todo: new stringbuilder
                    sb.AppendLine($"CBUFFER_START({cbuffer.Name}) // {cbufferIndex}");
                    depth++;
                }

                char[] chars = new char[] { 'x', 'y', 'z', 'w' };
                List<ConstantBufferParameter> allParams = cbuffer.CBParams;
                foreach (ConstantBufferParameter param in allParams)
                {
                    string typeName = DXShaderNamingUtils.GetConstantBufferParamTypeName(param);
                    string name = param.ParamName;

                    // skip things like unity_MatrixVP if they show up in $Globals
                    if (UnityShaderConstants.INCLUDED_UNITY_PROP_NAMES.Contains(name))
                    {
                        continue;
                    }

                    if (!declaredCBufs.Contains(name))
                    {
                        if (param.ArraySize > 0)
                        {
                            sb.Append(new string(' ', depth * 4));
                            sb.AppendLine($"{typeName} {name}[{param.ArraySize}]; // {param.Index} (starting at cb{cbufferIndex}[{param.Index / 16}].{chars[param.Index % 16 / 4]})");
                        }
                        else
                        {
                            sb.Append(new string(' ', depth * 4));
                            sb.AppendLine($"{typeName} {name}; // {param.Index} (starting at cb{cbufferIndex}[{param.Index / 16}].{chars[param.Index % 16 / 4]})");
                        }
                        declaredCBufs.Add(name);
                    }
                }

                if (nonGlobalCbuffer)
                {
                    depth--;
                    sb.Append(new string(' ', depth * 4));
                    sb.AppendLine("CBUFFER_END");
                }
            }
            return sb.ToString();
        }

        private string WritePassTextures(
            ShaderParams shaderParams, HashSet<string> declaredCBufs, int depth)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TextureParameter param in shaderParams.TextureParameters)
            {
                string name = param.Name;
                if (!declaredCBufs.Contains(name) && !UnityShaderConstants.BUILTIN_TEXTURE_NAMES.Contains(name))
                {
                    sb.Append(new string(' ', depth * 4));
                    switch (param.Dim)
                    {
                        case 2:
                            sb.AppendLine($"sampler2D {name}; // {param.Index}");
                            break;
                        case 3:
                            sb.AppendLine($"sampler3D {name}; // {param.Index}");
                            break;
                        case 4:
                            sb.AppendLine($"samplerCUBE {name}; // {param.Index}");
                            break;
                        case 5:
                            sb.AppendLine($"UNITY_DECLARE_TEX2DARRAY({name}); // {param.Index}");
                            break;
                        case 6:
                            sb.AppendLine($"UNITY_DECLARE_TEXCUBEARRAY({name}); // {param.Index}");
                            break;
                        default:
                            sb.AppendLine($"sampler2D {name}; // {param.Index} // Unsure of real type ({param.Dim})");
                            break;
                    }
                    declaredCBufs.Add(name);
                }
            }
            return sb.ToString();
        }

        private void WriteProperties(AssetTypeValueField propInfo)
        {
            _sb.AppendLine("Properties {");
            _sb.Indent();
            var props = propInfo["m_Props.Array"];
            foreach (var prop in props)
            {
                _sb.Append("");

                var attributes = prop["m_Attributes.Array"];
                foreach (var attribute in attributes)
                {
                    _sb.AppendNoIndent($"[{attribute.AsString}] ");
                }

                var flags = (SerializedPropertyFlag)prop["m_Flags"].AsUInt;
                if (flags.HasFlag(SerializedPropertyFlag.HideInInspector))
                    _sb.AppendNoIndent("[HideInInspector] ");
                if (flags.HasFlag(SerializedPropertyFlag.PerRendererData))
                    _sb.AppendNoIndent("[PerRendererData] ");
                if (flags.HasFlag(SerializedPropertyFlag.NoScaleOffset))
                    _sb.AppendNoIndent("[NoScaleOffset] ");
                if (flags.HasFlag(SerializedPropertyFlag.Normal))
                    _sb.AppendNoIndent("[Normal] ");
                if (flags.HasFlag(SerializedPropertyFlag.HDR))
                    _sb.AppendNoIndent("[HDR] ");
                if (flags.HasFlag(SerializedPropertyFlag.Gamma))
                    _sb.AppendNoIndent("[Gamma] ");
                // more?

                var name = prop["m_Name"].AsString;
                var description = prop["m_Description"].AsString;
                var type = (SerializedPropertyType)prop["m_Type"].AsInt;
                var defValues = new string[]
                {
                    prop["m_DefValue[0]"].AsFloat.ToString(CultureInfo.InvariantCulture),
                    prop["m_DefValue[1]"].AsFloat.ToString(CultureInfo.InvariantCulture),
                    prop["m_DefValue[2]"].AsFloat.ToString(CultureInfo.InvariantCulture),
                    prop["m_DefValue[3]"].AsFloat.ToString(CultureInfo.InvariantCulture)
                };
                var defTextureName = prop["m_DefTexture.m_DefaultName"].AsString;
                var defTextureDim = prop["m_DefTexture.m_TexDim"].AsInt;

                var typeName = type switch
                {
                    SerializedPropertyType.Color => "Color",
                    SerializedPropertyType.Vector => "Vector",
                    SerializedPropertyType.Float => "Float",
                    SerializedPropertyType.Range => $"Range({defValues[0]}, {defValues[1]})",
                    SerializedPropertyType.Texture => defTextureDim switch
                    {
                        1 => "any",
                        2 => "2D",
                        3 => "3D",
                        4 => "Cube",
                        5 => "2DArray",
                        6 => "CubeArray",
                        _ => throw new NotSupportedException("Bad texture dim")
                    },
                    SerializedPropertyType.Int => "Int",
                    _ => throw new NotSupportedException("Bad property type")
                };

                var value = type switch
                {
                    SerializedPropertyType.Color or
                    SerializedPropertyType.Vector => $"({defValues[0]}, {defValues[1]}, {defValues[2]}, {defValues[3]})",
                    SerializedPropertyType.Float or
                    SerializedPropertyType.Range or
                    SerializedPropertyType.Int => defValues[0],
                    SerializedPropertyType.Texture => $"\"{defTextureName}\" {{}}",
                    _ => throw new NotSupportedException("Bad property type")
                };

                _sb.AppendNoIndent($"{name} (\"{description}\", {typeName}) = {value}\n");
            }
            _sb.Unindent();
            _sb.AppendLine("}");
        }

        private void WriteSubShaders(BlobManager blobManager, AssetTypeValueField parsedForm)
        {
            var subshaders = parsedForm["m_SubShaders.Array"];
            foreach (var subshader in subshaders)
            {
                _sb.AppendLine("SubShader {");
                _sb.Indent();
                {
                    var tags = subshader["m_Tags"]["tags.Array"];
                    if (tags.Children.Count > 0)
                    {
                        _sb.AppendLine("Tags {");
                        _sb.Indent();
                        {
                            foreach (var tag in tags)
                            {
                                _sb.AppendLine($"\"{tag["first"].AsString}\"=\"{tag["second"].AsString}\"");
                            }
                        }
                        _sb.Unindent();
                        _sb.AppendLine("}");
                    }

                    var lod = subshader["m_LOD"].AsInt;
                    if (lod != 0)
                    {
                        _sb.AppendLine($"LOD {lod}");
                    }

                    WritePasses(blobManager, subshader);
                }
                _sb.Unindent();
                _sb.AppendLine("}");
            }
        }

        private void WritePasses(BlobManager blobManager, AssetTypeValueField subshader)
        {
            var passes = subshader["m_Passes.Array"];
            foreach (var pass in passes)
            {
                _sb.AppendLine("Pass {");
                _sb.Indent();
                {
                    WritePassState(pass["m_State"]);

                    var nameTable = pass["m_NameIndices.Array"]
                        .ToDictionary(ni => ni["second"].AsInt, ni => ni["first"].AsString);

                    var vertInfo = new SerializedProgramInfo(pass["progVertex"], nameTable);
                    var fragInfo = new SerializedProgramInfo(pass["progFragment"], nameTable);

                    var vertProgInfos = vertInfo.GetForPlatform((int)GetVertexProgramForPlatform(_platformId));
                    var fragProgInfos = fragInfo.GetForPlatform((int)GetFragmentProgramForPlatform(_platformId));

                    if (vertProgInfos.Count != fragProgInfos.Count && fragProgInfos.Count > 0)
                    {
                        throw new Exception("Vert and frag program count should be the same");
                    }

                    // we should hopefully only have one of each type, but just in case...
                    // todo: cleanup
                    if (vertProgInfos.Count > 0 && fragProgInfos.Count > 0)
                    {
                        for (var i = 0; i < vertProgInfos.Count; i++)
                        {
                            List<ShaderProgramBasket> baskets;
                            if (vertInfo.ParameterBlobIndices.Count > 0 && fragInfo.ParameterBlobIndices.Count > 0)
                            {
                                baskets = new List<ShaderProgramBasket>
                                {
                                    new ShaderProgramBasket(vertInfo, vertProgInfos[i], (int)vertInfo.ParameterBlobIndices[i]),
                                    new ShaderProgramBasket(fragInfo, fragProgInfos[i], (int)fragInfo.ParameterBlobIndices[i])
                                };
                            }
                            else
                            {
                                baskets = new List<ShaderProgramBasket>
                                {
                                    new ShaderProgramBasket(vertInfo, vertProgInfos[i], -1),
                                    new ShaderProgramBasket(fragInfo, fragProgInfos[i], -1)
                                };
                            }
                            WritePassBody(blobManager, baskets, _sb.GetIndent());
                        }
                    }
                    else if (vertProgInfos.Count > 0 && fragProgInfos.Count == 0)
                    {
                        for (var i = 0; i < vertProgInfos.Count; i++)
                        {
                            List<ShaderProgramBasket> baskets;
                            if (vertInfo.ParameterBlobIndices.Count > 0)
                            {
                                baskets = new List<ShaderProgramBasket>
                                {
                                    new ShaderProgramBasket(vertInfo, vertProgInfos[i], (int)vertInfo.ParameterBlobIndices[i]),
                                };
                            }
                            else
                            {
                                baskets = new List<ShaderProgramBasket>
                                {
                                    new ShaderProgramBasket(vertInfo, vertProgInfos[i], -1),
                                };
                            }
                            WritePassBody(blobManager, baskets, _sb.GetIndent());
                        }
                    }
                }
                _sb.Unindent();
                _sb.AppendLine("}");
            }
        }

        private void WritePassState(AssetTypeValueField state)
        {
            var name = state["m_Name"].AsString;
            _sb.AppendLine($"Name \"{name}\"");

            var lod = state["m_LOD"].AsInt;
            if (lod != 0)
            {
                _sb.AppendLine($"LOD {lod}");
            }

            var rtSeparateBlend = state["rtSeparateBlend"].AsBool;
            if (rtSeparateBlend)
            {
                for (var i = 0; i < 8; i++)
                {
                    WritePassRtBlend(state[$"rtBlend{i}"], i);
                }
            }
            else
            {
                WritePassRtBlend(state["rtBlend0"], -1);
            }

            var alphaToMask = state["alphaToMask.val"].AsFloat;
            var zClip = (ZClip)(int)state["zClip.val"].AsFloat;
            var zTest = (ZTest)(int)state["zTest.val"].AsFloat;
            var zWrite = (ZWrite)(int)state["zWrite.val"].AsFloat;
            var culling = (CullMode)(int)state["culling.val"].AsFloat;
            var offsetFactor = state["offsetFactor.val"].AsFloat;
            var offsetUnits = state["offsetUnits.val"].AsFloat;
            var stencilRef = state["stencilRef.val"].AsFloat;
            var stencilReadMask = state["stencilReadMask.val"].AsFloat;
            var stencilWriteMask = state["stencilWriteMask.val"].AsFloat;
            var stencilOp = state["stencilOp"];
            var stencilOpFront = state["stencilOpFront"];
            var stencilOpBack = state["stencilOpBack"];

            var lighting = state["lighting"].AsBool;

            if (alphaToMask > 0f)
            {
                _sb.AppendLine("AlphaToMask On");
            }
            if (zClip == ZClip.On)
            {
                _sb.AppendLine("ZClip On");
            }
            if (zTest != ZTest.None && zTest != ZTest.LEqual)
            {
                _sb.AppendLine($"ZTest {zTest}");
            }
            if (zWrite == ZWrite.On)
            {
                _sb.AppendLine("ZWrite On");
            }
            if (culling != CullMode.Back)
            {
                _sb.AppendLine($"Cull {culling}");
            }
            if (offsetFactor != 0f || offsetUnits != 0f)
            {
                _sb.AppendLine($"Offset {offsetFactor}, {offsetUnits}");
            }

            if (lighting)
            {
                _sb.AppendLine("Lighting On");
            }

            var tags = state["m_Tags"]["tags.Array"];
            if (tags.Children.Count > 0)
            {
                _sb.AppendLine("Tags {");
                _sb.Indent();
                {
                    foreach (var tag in tags)
                    {
                        _sb.AppendLine($"\"{tag["first"].AsString}\"=\"{tag["second"].AsString}\"");
                    }
                }
                _sb.Unindent();
                _sb.AppendLine("}");
            }
        }

        private void WritePassRtBlend(AssetTypeValueField rtBlend, int index)
        {
            var srcBlend = (BlendMode)(int)rtBlend["srcBlend.val"].AsFloat;
            var destBlend = (BlendMode)(int)rtBlend["destBlend.val"].AsFloat;
            var srcBlendAlpha = (BlendMode)(int)rtBlend["srcBlendAlpha.val"].AsFloat;
            var destBlendAlpha = (BlendMode)(int)rtBlend["destBlendAlpha.val"].AsFloat;
            var blendOp = (BlendOp)(int)rtBlend["blendOp.val"].AsFloat;
            var blendOpAlpha = (BlendOp)(int)rtBlend["blendOpAlpha.val"].AsFloat;
            var colMask = (ColorWriteMask)(int)rtBlend["colMask.val"].AsFloat;

            if (srcBlend != BlendMode.One || destBlend != BlendMode.Zero || srcBlendAlpha != BlendMode.One || destBlendAlpha != BlendMode.Zero)
            {
                _sb.Append("");
                _sb.AppendNoIndent("Blend ");
                if (index != -1)
                {
                    _sb.AppendNoIndent($"{index} ");
                }
                _sb.AppendNoIndent($"{srcBlend} {destBlend}");
                if (srcBlendAlpha != BlendMode.One || destBlendAlpha != BlendMode.Zero)
                {
                    _sb.AppendNoIndent($", {srcBlendAlpha} {destBlendAlpha}");
                }
                _sb.AppendNoIndent("\n");
            }

            if (blendOp != BlendOp.Add || blendOpAlpha != BlendOp.Add)
            {
                _sb.Append("");
                _sb.AppendNoIndent("BlendOp ");
                if (index != -1)
                {
                    _sb.AppendNoIndent($"{index} ");
                }
                _sb.AppendNoIndent($"{blendOp}");
                if (blendOpAlpha != BlendOp.Add)
                {
                    _sb.AppendNoIndent($", {blendOpAlpha}");
                }
                _sb.AppendNoIndent("\n");
            }

            if (colMask != ColorWriteMask.All)
            {
                _sb.Append("");
                _sb.AppendNoIndent("ColorMask ");
                if (colMask == ColorWriteMask.None)
                {
                    _sb.AppendNoIndent("0");
                }
                else
                {
                    if (colMask == ColorWriteMask.Red)
                    {
                        _sb.AppendNoIndent("R");
                    }
                    else if (colMask == ColorWriteMask.Green)
                    {
                        _sb.AppendNoIndent("G");
                    }
                    else if (colMask == ColorWriteMask.Blue)
                    {
                        _sb.AppendNoIndent("B");
                    }
                    else if (colMask == ColorWriteMask.Alpha)
                    {
                        _sb.AppendNoIndent("A");
                    }
                }
                if (index != -1)
                {
                    _sb.AppendNoIndent($" {index}"); // -1 check needed?
                }
                _sb.AppendNoIndent("\n");
            }
        }

        // todo: move
        private ShaderGpuProgramType GetVertexProgramForPlatform(GPUPlatform gpuPlatform)
        {
            return gpuPlatform switch
            {
                GPUPlatform.d3d11 => ShaderGpuProgramType.DX11VertexSM40,
                GPUPlatform.Switch => ShaderGpuProgramType.Console,
            };
        }

        private ShaderGpuProgramType GetFragmentProgramForPlatform(GPUPlatform gpuPlatform)
        {
            return gpuPlatform switch
            {
                GPUPlatform.d3d11 => ShaderGpuProgramType.DX11PixelSM40,
                GPUPlatform.Switch => ShaderGpuProgramType.Console,
            };
        }
    }
}