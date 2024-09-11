using System.Threading.Tasks;
using IntakeForm.Models;
using IntakeForm.Models.Data.Templates;
using Taku.Core;

namespace SkillBridge.CMS.Areas.Intake.Data
{
    public interface ITemplateRepository :IRepository
    {
        Task<DeserializedFormTemplate> GetCurrentFormTemplate(Enumerations.TemplateType templateType);
        Task<DeserializedFormTemplate> GetFormTemplate(int id);
        Task<DeserializedFormTemplate> GetFormTemplateByFormID(int id);
    }
}
