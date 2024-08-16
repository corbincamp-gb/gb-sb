﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SkillBridge_System_Prototype.Models
{
    public class PendingProgramModel
    {
        public int Id { get; set; }  
        public int Program_Id { get; set; }
        public bool Is_Active { get; set; }
        public string Program_Name { get; set; }
        public string Organization_Name { get; set; }
        public int Organization_Id { get; set; }
        public string Mou_Link { get; set; }       // URL link to actual MOU packet
        public bool Online { get; set; }
        public string Program_Duration { get; set; }
        public string Delivery_Method { get; set; }
        public string Opportunity_Type { get; set; }
        public string? SerializedTrainingPlan { get; set; }
        public bool IsAddition { get; set; }

        public PendingProgramModel() { }

        public PendingProgramModel(SB_PendingProgramChange model)
        {
            Id = model.Id;
            Program_Id = model.Program_Id;
            Is_Active = model.Is_Active;
            Program_Name = model.Program_Name;
            Organization_Name = model.Organization_Name;
            Organization_Id = model.Organization_Id;
            Mou_Link = model.Mou_Link;
            Online = model.Online;
            Program_Duration = Util.Global.GlobalFunctions.GetProgramDuration(model.Program_Duration);
            Delivery_Method = Util.Global.GlobalFunctions.GetDeliveryMethod(model.Delivery_Method);
            Opportunity_Type = model.Opportunity_Type;
            SerializedTrainingPlan = model.SerializedTrainingPlan;
            IsAddition = false;
        }

        public PendingProgramModel(SB_PendingProgramAddition model)
        {
            Id = model.Id;
            Program_Id = model.Program_Id;
            Is_Active = model.Is_Active;
            Program_Name = model.Program_Name;
            Organization_Name = model.Organization_Name;
            Organization_Id = model.Organization_Id;
            Mou_Link = model.Mou_Link;
            Online = model.Online;
            Program_Duration = Util.Global.GlobalFunctions.GetProgramDuration(model.Program_Duration);
            if (!String.IsNullOrWhiteSpace(model.Delivery_Method))
            {
                var deliveryMethod = 0;
                if (int.TryParse(model.Delivery_Method, out deliveryMethod))
                {
                    Delivery_Method = Util.Global.GlobalFunctions.GetDeliveryMethod(deliveryMethod);
                }
            }
            Opportunity_Type = model.Opportunity_Type;
            SerializedTrainingPlan = model.SerializedTrainingPlan;
            IsAddition = true;
        }

    }
}
