using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Meca.ApplicationService.Services.HangFire
{
    public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public int Days { get; set; }
        public ProlongExpirationTimeAttribute(int days = 7)
        {
            Days = days;
        }
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(Days);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(Days);
        }
    }
}