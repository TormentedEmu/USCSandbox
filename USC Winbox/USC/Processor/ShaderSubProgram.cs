using AssetRipper.Primitives;
using AssetsTools.NET;
using NLog;

namespace USCSandbox.Processor
{
    public class ShaderSubProgram
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public int ProgramType;
        public int StatsALU;
        public int StatsTEX;
        public int StatsFlow;
        public int StatsTempRegister;

        public List<string> GlobalKeywords;
        public List<string> LocalKeywords;

        public byte[] ProgramData;
        public ParserBindChannels BindChannels;

        public ShaderParams ShaderParams;

        public ShaderSubProgram(AssetsFileReader r, UnityVersion version)
        {
            //_logger.Info($"r position: {r.Position}");
            var hasStatsTempRegister = version.GreaterThanOrEquals(5, 5);
            var hasLocalKeywords = version.LessThan(2021, 2) && version.GreaterThanOrEquals(2019, 1);

            var blobVersion = r.ReadInt32();
            ProgramType = r.ReadInt32();
            StatsALU = r.ReadInt32();
            StatsTEX = r.ReadInt32();
            StatsFlow = r.ReadInt32();
            if (hasStatsTempRegister)
            {
                StatsTempRegister = r.ReadInt32();
            }

            var globalKeywordCount = r.ReadInt32();
            //_logger.Info($"globalKeywordCount: {globalKeywordCount}");

            var remainingBytes = r.BaseStream.Length - r.Position;
            var globalKeywordCountTotalSize = globalKeywordCount * sizeof(int);

            if (globalKeywordCountTotalSize > (remainingBytes))
            {
                _logger.Error($"Global keyword remainingBytes: {remainingBytes}");
                return;
            }

            GlobalKeywords = new List<string>(globalKeywordCount);
            for (var i = 0; i < globalKeywordCount; i++)
            {
                GlobalKeywords.Add(r.ReadCountStringInt32());
                r.Align();
            }
            if (hasLocalKeywords)
            {
                var localKeywordCount = r.ReadInt32();
                _logger.Info($"localKeywordCount: {localKeywordCount}");

                remainingBytes = r.BaseStream.Length - r.Position;
                var localKeywordCountTotalSize = localKeywordCount * sizeof(int);

                if (localKeywordCountTotalSize > (remainingBytes))
                {
                    _logger.Error($"Local remainingBytes: {remainingBytes}");
                    return;
                }

                LocalKeywords = new List<string>(localKeywordCount);
                for (var i = 0; i < localKeywordCount; i++)
                {
                    LocalKeywords.Add(r.ReadCountStringInt32());
                    r.Align();
                }
            }
            else
            {
                LocalKeywords = new List<string>(0);
            }

            if (r.Position >= r.BaseStream.Length)
            {
                _logger.Error("End of stream.");
                return;
            }

            var programDataSize = r.ReadInt32();
            ProgramData = r.ReadBytes(programDataSize);
            r.Align();

            BindChannels = new ParserBindChannels(r);

            if (version.LessThan(2021))
            {
                ShaderParams = new ShaderParams(r, version, false);
            }
        }

        public ShaderGpuProgramType GetProgramType(UnityVersion version)
        {
            if (version.GreaterThanOrEquals(5, 5))
            {
                return ((ShaderGpuProgramType55)ProgramType).ToGpuProgramType();
            }
            else
            {
                return ((ShaderGpuProgramType53)ProgramType).ToGpuProgramType();
            }
        }
    }
}