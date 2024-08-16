using Microsoft.EntityFrameworkCore.ChangeTracking;
using SkillBridge_System_Prototype.Enums;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkillBridge_System_Prototype.Models;

namespace SkillBridge_System_Prototype.Util.Audit
{
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public AuditType AuditType { get; set; }
        public string AuditUser { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object>
               KeyValues
        { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>
               OldValues
        { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>
               NewValues
        { get; } = new Dictionary<string, object>();
        public List<string> ChangedColumns { get; } = new List<string>();

        public AuditEntry(EntityEntry entry, string auditUser)
        {
            Entry = entry;
            AuditUser = auditUser;
            SetChanges();
        }

        private void SetChanges()
        {
            TableName = Entry.Metadata.GetTableName();
            foreach (PropertyEntry property in Entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                string dbColumnName = property.Metadata.GetColumnName();

                if (property.Metadata.IsPrimaryKey())
                {
                    KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (Entry.State)
                {
                    case EntityState.Added:
                        NewValues[propertyName] = property.CurrentValue;
                        AuditType = AuditType.Create;
                        break;

                    case EntityState.Deleted:
                        OldValues[propertyName] = property.OriginalValue;
                        AuditType = AuditType.Delete;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            ChangedColumns.Add(dbColumnName);

                            OldValues[propertyName] = property.OriginalValue;
                            NewValues[propertyName] = property.CurrentValue;
                            AuditType = AuditType.Update;
                        }
                        break;
                }
            }
        }

        public SB_Audit ToAudit()
        {
            var audit = new SB_Audit();
            audit.Id = Guid.NewGuid();
            audit.AuditDateTimeUtc = DateTime.UtcNow;
            audit.AuditType = AuditType.ToString();
            audit.AuditUser = AuditUser;
            audit.TableName = TableName;
            audit.KeyValues = JsonConvert.SerializeObject(KeyValues);
            audit.OldValues = OldValues.Count == 0 ?
                              null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ?
                              null : JsonConvert.SerializeObject(NewValues);
            audit.ChangedColumns = ChangedColumns.Count == 0 ?
                                   null : JsonConvert.SerializeObject(ChangedColumns);

            return audit;
        }
    }
}
