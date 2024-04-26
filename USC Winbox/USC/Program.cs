using AssetsTools.NET;
using AssetsTools.NET.Extra;
using USCSandbox.Processor;
using UnityVersion = AssetRipper.Primitives.UnityVersion;
using NLog;

namespace USCSandbox
{
    internal class ConsoleProgram
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void ConsoleMain(string[] args)
        {
            // null "C:\Users\nesquack\Documents\GitIssueWs\Data2g\Data2\Resources\unity_builtin_extra" 7 Switch

            var bundlePath = args[0];
            var assetsFileName = args[1];
            var shaderPathId = -1L;
            if (args.Length > 2)
                shaderPathId = long.Parse(args[2]);
            var shaderPlatform = args.Length > 3 ? Enum.Parse<GPUPlatform>(args[3]) : GPUPlatform.d3d11;

            AssetsManager manager = new AssetsManager();
            AssetsFileInstance afileInst;
            AssetsFile afile;
            UnityVersion ver;
            int shaderTypeId;
            Dictionary<long, string> files = [];

            if (!string.IsNullOrEmpty(bundlePath))
            {
                var bundleFile = manager.LoadBundleFile(bundlePath, true);
                afileInst = manager.LoadAssetsFileFromBundle(bundleFile, assetsFileName);
                if (afileInst == null)
                {
                    _logger.Error($"Bundle did not contain asset file name: {assetsFileName}\nListing available file names...");

                    foreach (var file in bundleFile.file.GetAllFileNames())
                    {
                        _logger.Info($"File name in bundle: {file}");
                    }

                    return;
                }

                afile = afileInst.file;
                manager.LoadClassPackage("classdata.tpk");
                manager.LoadClassDatabaseFromPackage(bundleFile.file.Header.EngineVersion);
                ver = UnityVersion.Parse(bundleFile.file.Header.EngineVersion);
                shaderTypeId = manager.ClassPackage.GetClassDatabase(ver.ToString()).FindAssetClassByName("Shader").ClassId;

                var ggm = manager.LoadAssetsFileFromBundle(bundleFile, "globalgamemanagers");
                if (ggm != null)
                {
                    //manager.LoadClassDatabaseFromPackage(ggm.file.Metadata.UnityVersion);
                    var rsrcInfo = ggm.file.GetAssetsOfType(AssetClassID.ResourceManager)[0];
                    var rsrcBf = manager.GetBaseField(ggm, rsrcInfo);
                    var m_Container = rsrcBf["m_Container.Array"];
                    foreach (var data in m_Container.Children)
                    {
                        var name = data[0].AsString;
                        var pathId = data[1]["m_PathID"].AsLong;
                        files[pathId] = name;
                        //Console.WriteLine($"in resources.assets, pathid {pathId} = {name}");
                    }
                }
            }
            else
            {
                afileInst = manager.LoadAssetsFile(assetsFileName);
                if (afileInst == null)
                {
                    _logger.Error($"Asset file name does not exist in bundle: {assetsFileName}");
                    return;
                }

                afile = afileInst.file;
                manager.LoadClassPackage("classdata.tpk");
                manager.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);
                ver = UnityVersion.Parse(afile.Metadata.UnityVersion);
                shaderTypeId = manager.ClassPackage.GetClassDatabase(ver.ToString()).FindAssetClassByName("Shader").ClassId;
            }

            var shaders = afileInst.file.GetAssetsOfType(shaderTypeId);
            _logger.Info($"Shaders found: {shaders.Count}");

            //int unnamedCount = 0;
            foreach (var shader in shaders)
            {
                if (shaderPathId != -1 && shader.PathId != shaderPathId)
                    continue;

                var shaderBf = manager.GetExtAsset(afileInst, 0, shader.PathId).baseField;
                if (shaderBf == null)
                {
                    _logger.Info("Shader asset not found.");
                    return;
                }

                var shaderProcessor = new ShaderProcessor(shaderBf, ver, shaderPlatform);
                bool fileNameExists = files.TryGetValue(shader.PathId, out string? name);
                _logger.Info($"Printing shader Path: {(ulong)shader.PathId} Name: {shaderProcessor.Name}");
                string shaderText = string.Empty;// shaderProcessor.Process();

                try
                {
                    shaderText = shaderProcessor.Process();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception caught: {ex.Message}");
                    continue;
                }

                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Shaders", Path.GetDirectoryName(shaderProcessor.Name)!));
                File.WriteAllText($"{Path.Combine(Application.StartupPath, "Shaders", shaderProcessor.Name)}.shader", shaderText);
                _logger.Info($"{shaderProcessor.Name} decompiled");
                /*
                if (fileNameExists)
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Shaders", Path.GetDirectoryName(shaderProcessor.Name)!));
                    File.WriteAllText($"{Path.Combine(Application.StartupPath, "Shaders", shaderProcessor.Name)}.shader", shaderText);
                    _logger.Info($"{shaderProcessor.Name} decompiled");
                }
                else
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "out", "unnamed"));
                    File.WriteAllText($"{Path.Combine(Environment.CurrentDirectory, "out", "unnamed", $"unnamed {unnamedCount}")}.shader", shaderText);
                    _logger.Info($"Unnamed {unnamedCount} decompiled");
                    unnamedCount++;
                }*/
            }

            _logger.Info($"End of ConsoleMain");
        }
    }
}