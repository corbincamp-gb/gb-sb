using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Enums
{
    public enum ProgramType
    {
        [Display(Name = "Department of Labor (DOL) Registered Apprenticeship Program")]
        DOLRegisteredApprenticeshipProgram = 0,
        [Display(Name = "DOL Registered Pre-Apprenticeship Program")]
        DOLRegisteredPreApprenticeshipProgram = 1,
        [Display(Name = "Industry Recognized (Non-DOL-Registered) Pre-Apprenticeship Program")]
        IndustryRecognizedPreApprenticeshipProgram = 2,
        [Display(Name = "Industry Recognized (Non-DOL-Registered) Apprenticeship Program (IRAP)")]
        IndustryRecognizedApprenticeshipProgram = 3,
        [Display(Name = "Internship Program")]
        InternshipProgram = 4,
        [Display(Name = "Employment Skills Training Program")]
        EmploymentSkillsTrainingProgram = 5,
        [Display(Name = "Job Training Program")]
        JobTrainingProgram = 6
    }
}