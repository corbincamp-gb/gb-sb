using IntakeForm.Models;
using IntakeForm.Models.Data.Templates;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Intake.Data
{
    public interface ITemplateRepository
    {
        Task<DeserializedFormTemplate> GetCurrentFormTemplate(Enumerations.TemplateType templateType);
        Task<DeserializedFormTemplate> GetFormTemplate(int id);
        Task<DeserializedFormTemplate> GetFormTemplateByFormID(int id);
    }
}
