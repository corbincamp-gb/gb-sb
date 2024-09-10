using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Skillbridge.Business.Model.Db;
using Taku.Core.Enums;
using RelationalEntityTypeExtensions = Microsoft.EntityFrameworkCore.RelationalEntityTypeExtensions;
using RelationalPropertyExtensions = Microsoft.EntityFrameworkCore.RelationalPropertyExtensions;

namespace Skillbridge.Business.Util.Audit
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
            TableName = RelationalEntityTypeExtensions.GetTableName();
            foreach (PropertyEntry property in Entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                string dbColumnName = RelationalPropertyExtensions.GetColumnName();

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

        public AuditModel ToAudit()
        {
            var audit = new AuditModel
            {
                Id = Guid.NewGuid(),
                AuditDateTimeUtc = DateTime.UtcNow,
                AuditType = AuditType.ToString(),
                AuditUser = AuditUser,
                TableName = TableName,
                KeyValues = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                ChangedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns)
            };

            return audit;
        }
    }
}
