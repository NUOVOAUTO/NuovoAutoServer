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
        public DateTimeOffset CreatedDateTime { get; set; } = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        public DateTimeOffset LastUpdatedDateTime { get; set; } = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        public Guid CreateUserId { get; set; }
        public Guid LastUpdatedUserId { get; set; }

        public virtual string PartitionKey { get; set; }

        public string RecordStatus { get; set; } = RecordStatusConstants.Active;

        public void OnCreated(Guid userId)
        {
            CreatedDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            LastUpdatedDateTime = CreatedDateTime;
            CreateUserId = userId;
            LastUpdatedUserId = userId;
        }

        public void OnCreated()
        {
            CreatedDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            LastUpdatedDateTime = CreatedDateTime;
            CreateUserId = Guid.Empty;
            LastUpdatedUserId = Guid.Empty;

        }

        public void OnChanged(Guid userId)
        {
            LastUpdatedDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            LastUpdatedUserId = userId;
        }

        public void OnChanged()
        {
            LastUpdatedDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            LastUpdatedUserId = Guid.Empty;
        }

        public void OnChanged(DomainModelBase domainModelBase)
        {
            CreatedDateTime = domainModelBase.CreatedDateTime;
            LastUpdatedDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            CreateUserId = domainModelBase.CreateUserId;
            LastUpdatedUserId = Guid.Empty;
        }

        public abstract string SetPartitionKey();
    }
}
