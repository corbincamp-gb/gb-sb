using IntakeForm.Models;
using IntakeForm.Models.Data.Templates;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge.CMS.Intake.Data
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly IntakeFormContext _db;

        public TemplateRepository(IntakeFormContext db)
        {
            _db = db;
        }

        public async Task<DeserializedFormTemplate> GetCurrentFormTemplate(Enumerations.TemplateType templateType)
        {
            var today = DateTime.Today;

            var template = await _db.FormTemplates.Where(o => o.TemplateTypeID == (byte)templateType && o.RetiredDate >= today).FirstOrDefaultAsync();

            return JsonConvert.DeserializeObject<DeserializedFormTemplate>(template.SerializedFormTemplate);
        }

        public async Task<DeserializedFormTemplate> GetFormTemplate(int id)
        {
            var template = await _db.FormTemplates.Where(o => o.ID == id).FirstOrDefaultAsync();

            return JsonConvert.DeserializeObject<DeserializedFormTemplate>(template.SerializedFormTemplate);
        }

        public async Task<DeserializedFormTemplate> GetFormTemplateByFormID(int id)
        {
            var template = await _db.FormTemplates.Where(o => o.ID == _db.Forms.FirstOrDefault(f => f.ID == id).FormTemplateID).FirstOrDefaultAsync();

            return JsonConvert.DeserializeObject<DeserializedFormTemplate>(template.SerializedFormTemplate);

        }
    }
}
