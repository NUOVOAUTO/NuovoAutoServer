using NuovoAutoServer.Model.Constants;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model
{
    public abstract class DomainModelBase
    {
        public DateTime CreatedDateTime { get; set; }= DateTime.Now;
        public DateTime LastUpdatedDateTime { get; set; } = DateTime.Now;
        public Guid CreateUserId { get; set; }
        public Guid LastUpdatedUserId{ get; set; }

        public virtual string PartitionKey { get; set; }

        public string RecordStatus { get; set; } = RecordStatusConstants.Active;

        public void OnCreated(Guid userId)
        {
            CreatedDateTime = DateTime.UtcNow;
            LastUpdatedDateTime = CreatedDateTime;
            CreateUserId = userId;
            LastUpdatedUserId = userId;
        }

        public void OnCreated()
        {
            CreatedDateTime = DateTime.UtcNow;
            LastUpdatedDateTime = CreatedDateTime;
            CreateUserId = Guid.Empty;
            LastUpdatedUserId = Guid.Empty;

        }

        public void OnChanged(Guid userId)
        {
            LastUpdatedDateTime = DateTime.UtcNow;
            LastUpdatedUserId = userId;
        }

        public void OnChanged()
        {
            LastUpdatedDateTime = DateTime.UtcNow;
            LastUpdatedUserId = Guid.Empty;
        }

        public void OnChanged(DomainModelBase domainModelBase)
        {
            CreatedDateTime = domainModelBase.CreatedDateTime;
            LastUpdatedDateTime = DateTime.UtcNow;

            CreateUserId = domainModelBase.CreateUserId;
            LastUpdatedUserId = Guid.Empty;
        }

        public abstract string SetPartitionKey();
    }
}
