using System.Text;
using SkillBridge.Business.Model;
using Taku.Core;
using Taku.Core.Command;

namespace SkillBridge.Business.Command
{
    public interface IRenderDropDownJsFileCommand : IRenderingCommand
    {
        void Execute(IDropDownData data, out string result);
    }

    public class RenderDropDownJsFileCommand(ISerializeObjectCommand serializeObjectCommand)
        : IRenderDropDownJsFileCommand
    {
        public void Execute(IDropDownData data, out string result)
        {
            var dict = new Dictionary<string, IEnumerable<string>>
            {
                { "program", data.Programs },
                { "service", data.MilitaryBranches },
                { "duration", data.Durations },
                { "delivery", data.Deliveries },
                { "location", data.Locations },
                { "family", data.OccupationAreas },
                { "parentOrg", data.Organizations }
            };
            
            var sb = new StringBuilder();
            sb.AppendLine($"// Created {DateTime.Now.ToString("G")}");
            foreach (var dd in dict.Keys)
            {
                serializeObjectCommand.Execute(dict[dd], out var arrString);
                sb.AppendLine($"const {dd}Dropdown = {arrString}");
            }
            serializeObjectCommand.Execute(data.RelatedOrganizations, out var relatedOrgs);
            sb.AppendLine($"var relatedOrgs = {relatedOrgs}");

            result = sb.ToString();
        }
    }
}
