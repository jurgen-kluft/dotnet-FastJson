using System.Text;
using Xunit;

public class JsonReaderTests2
{
    public class PipelineState
    {
        public Dictionary<string, StageState> Stages { get; set; } = new();
    }

    public class StageState
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, ProcessState> Processes { get; set; } = new();
    }

    public class ProcessState
    {
        public string Name { get; set; } = string.Empty;
        public string ProcessDescriptorHash { get; set; } = string.Empty;
        public string Commandline { get; set; } = string.Empty;
        public List<string> UsedVars { get; set; } = new();
        public Dictionary<string, FileState> Files { get; set; } = new();
    }

    public class FileState
    {
        public bool IsOutput { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long LastWriteTime { get; set; }
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();
        public string ContentHashStr { get; set; } = string.Empty;
    }

    public static class PipelineStateJsonWriter
    {
        public static string WritePipelineState(FJson.Writer writer, PipelineState state)
        {
            writer.Begin();
            writer.BeginArray("Stages");
            {
                for (var i = 0; i < state.Stages.Keys.Count; ++i)
                {
                    writer.BeginObject(string.Empty);
                    {
                        var key = state.Stages.Keys.ElementAt(i);
                        var value = state.Stages[key];
                        WriteStageState(writer, value);
                    }
                    writer.EndObject(i == (state.Stages.Keys.Count - 1));
                }
            }
            writer.EndArray(true);
            return writer.End();
        }

        public static void WriteStageState(FJson.Writer writer, StageState state)
        {
            writer.WriteField("Name", state.Name);
            writer.BeginArray("Processes");
            {
                for (var i = 0; i < state.Processes.Keys.Count; ++i)
                {
                    writer.BeginObject(string.Empty);
                    {
                        var key = state.Processes.Keys.ElementAt(i);
                        var value = state.Processes[key];
                        WriteProcessState(writer, value);
                    }
                    writer.EndObject(i == (state.Processes.Keys.Count - 1));
                }
            }
            writer.EndArray(true);
        }

        public static void WriteProcessState(FJson.Writer writer, ProcessState state)
        {
            writer.WriteField("Name", state.Name);
            writer.WriteField("CmdLine", state.Commandline);
            writer.WriteField("DepHash", state.ProcessDescriptorHash);

            writer.BeginArray("Files");
            {
                for (var i = 0; i < state.Files.Keys.Count; ++i)
                {
                    var key = state.Files.Keys.ElementAt(i);
                    writer.BeginObject(string.Empty);
                    {
                        var value = state.Files[key];
                        WriteFileState(writer, value);
                    }
                    writer.EndObject(i == (state.Files.Keys.Count - 1));
                }
            }
            writer.EndArray();

            writer.BeginArray("UsedVars");
            {
                for (var i = 0; i < state.UsedVars.Count; ++i)
                {
                    var element = state.UsedVars[i];
                    writer.WriteElement(element, i == (state.UsedVars.Count - 1));
                }
            }
            writer.EndArray(true);
        }

        public static void WriteFileState(FJson.Writer writer, FileState state)
        {
            writer.WriteField("IsOutput", state.IsOutput);
            writer.WriteField("FilePath", state.FilePath);
            writer.WriteField("LastWriteTime", state.LastWriteTime);
            writer.WriteField("ContentHash", state.ContentHashStr, true);
        }
    }

    public static class PipelineStateJsonReader
    {
        public static PipelineState ReadPipelineState(FJson.Reader json)
        {
            var ps = new PipelineState();
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;

                if (json.IsFieldName(key, "Stages"))
                {
                    while (json.Read(out  key, out  value))
                    {
                        if (json.IsArrayEnd(key, value))
                            break;
                        var ss = ReadStageState(json);
                        ps.Stages.Add(ss.Name, ss);
                    }
                }
            }
            return ps;
        }

        private static StageState ReadStageState(FJson.Reader json)
        {
            var ss = new StageState();
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;

                if (json.IsFieldName(key, "Name"))
                {
                    ss.Name = json.ParseString(value);
                }
                else if (json.IsFieldName(key, "Processes"))
                {
                    while (json.Read(out  key, out  value))
                    {
                        if (json.IsArrayEnd(key, value))
                            break;
                        var ps = ReadProcessState(json);
                        ss.Processes.Add(ps.Name, ps);
                    }
                }
            }

            return ss;
        }

        private static ProcessState ReadProcessState(FJson.Reader json)
        {
            var ps = new ProcessState();
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;

                if (json.IsFieldName(key, "Name"))
                {
                    ps.Name = json.ParseString(value);
                }
                else if (json.IsFieldName(key, "DepHash"))
                {
                    ps.ProcessDescriptorHash = json.ParseString(value);
                }
                else if (json.IsFieldName(key, "CmdLine"))
                {
                    ps.Commandline = json.ParseString(value);
                }
                else if (json.IsFieldName(key, "UsedVars"))
                {
                    while (json.Read(out key, out value))
                    {
                        if (json.IsArrayEnd(key, value))
                            break;
                        ps.UsedVars.Add(json.ParseString(value));
                    }
                }
                else if (json.IsFieldName(key, "Files"))
                {
                    while (json.Read(out  key, out  value))
                    {
                        if (json.IsArrayEnd(key, value))
                            break;
                        var fs = ReadFileState(json);
                        ps.Files.Add(fs.FilePath, fs);
                    }
                }
            }

            return ps;
        }

        private static FileState ReadFileState(FJson.Reader json)
        {
            var fs = new FileState();
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;

                if (json.IsFieldName(key, "IsOutput"))
                {
                    fs.IsOutput = json.ParseBool(value);
                }
                else if (json.IsFieldName(key, "FilePath"))
                {
                    fs.FilePath = json.ParseString(value);
                }
                else if (json.IsFieldName(key, "LastWriteTime"))
                {
                    fs.LastWriteTime = json.ParseLong(value);
                }
                else if (json.IsFieldName(key, "ContentHash"))
                {
                    fs.ContentHashStr = json.ParseString(value);
                    //fs.ContentHash = Hashing.Utils.AsString(fs.ContentHashStr);
                }
            }

            return fs;
        }
    }

    [Fact]
    public void TestRead()
    {
        var myPipelineState = new PipelineState();

        var stageA = new StageState() { Name = "stageA" };
        var stageB = new StageState() { Name = "stageB" };

        var processA = new ProcessState() { Name = "A", Commandline = "-x -f", ProcessDescriptorHash = "asfsafsadfdsd" };
        processA.UsedVars.Add("size=0");
        processA.UsedVars.Add("border=100");
        var processB = new ProcessState() { Name = "B", Commandline = "-x -f", ProcessDescriptorHash = "oupaisudfiuso" };
        processB.UsedVars.Add("size=33");
        processB.UsedVars.Add("border=999");

        var file1 = new FileState() { IsOutput = false, FilePath = "c:\\input\\file1.png", ContentHashStr = "LJSKLFOAUSPDIFUIUOSKJF", ContentHash = new byte[32], LastWriteTime = 99};
        var file2 = new FileState() { IsOutput = true, FilePath = "c:\\output\\file1.png", ContentHashStr = "SPDIFUIUOSKJFLJSKLFOAU", ContentHash = new byte[32], LastWriteTime = 100};
        var file3 = new FileState() { IsOutput = true, FilePath = "c:\\cache\\file1.png", ContentHashStr = "UIUOSKJFLJSKLFOAUSPDIF", ContentHash = new byte[32], LastWriteTime = 101};

        processA.Files.Add(file1.FilePath, file1);
        processA.Files.Add(file2.FilePath, file2);
        processA.Files.Add(file3.FilePath, file3);

        processB.Files.Add(file1.FilePath, file1);
        processB.Files.Add(file2.FilePath, file2);
        processB.Files.Add(file3.FilePath, file3);

        stageA.Processes.Add(processA.Name, processA);
        stageA.Processes.Add(processB.Name, processB);

        stageB.Processes.Add(processA.Name, processA);
        stageB.Processes.Add(processB.Name, processB);

        myPipelineState.Stages.Add(stageA.Name, stageA);
        myPipelineState.Stages.Add(stageB.Name, stageB);

        var jsonWriter = new FJson.Writer();
        string jsonString = PipelineStateJsonWriter.WritePipelineState(jsonWriter, myPipelineState);

        var jsonReader = new FJson.Reader();
        if (jsonReader.Begin(jsonString))
        {
            var pipelineState = PipelineStateJsonReader.ReadPipelineState(jsonReader);
        }
    }
}
