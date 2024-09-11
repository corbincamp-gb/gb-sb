using Newtonsoft.Json;

namespace Taku.Core.Command
{
    public interface ISerializeObjectCommand : IRenderingCommand
    {
        void Execute(object inObject, out string output);
    }

    public class SerializeObjectCommand : ISerializeObjectCommand
    {
        public void Execute(object inObject, out string output)
        {
            output = JsonConvert.SerializeObject(inObject);
        }
    }
}