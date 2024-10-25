//using Newtonsoft.Json;

using Newtonsoft.Json;
using System.Text.Json;

namespace Taku.Core.Command
{
    public class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToUpper();
        }
    }

    public interface ISerializeObjectCommand : IRenderingCommand
    {
        void Execute(object inObject, out string output);
        void Execute(object inObject, out string output, bool allCaps);
    }

    public class SerializeObjectCommand : ISerializeObjectCommand
    {
        private readonly JsonSerializerOptions _options = new(){WriteIndented = false};

        public void Execute(object inObject, out string output)
        {
            output = System.Text.Json.JsonSerializer.Serialize(inObject, _options);
        }

        public void Execute(object inObject, out string output, bool allCaps)
        {
            if (allCaps)
            {
                _options.PropertyNamingPolicy = new UpperCaseNamingPolicy();
            }

            Execute(inObject, out var result);
            output = result;
        }
    }
}