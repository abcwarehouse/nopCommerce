using System;
using Nop.Core;

namespace Nop.Plugin.Misc.AbcCore.Domain
{
    public class MickeyLandingPage : BaseEntity
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive()
        {
            var now = DateTime.Now;
            if (StartDate.HasValue && now < StartDate.Value)
                return false;
            if (EndDate.HasValue && now > EndDate.Value.AddDays(1))
                return false;
            return true;
        }

        public bool IsUpcoming() => StartDate.HasValue && DateTime.Now < StartDate.Value;

        public bool IsExpired() => EndDate.HasValue && DateTime.Now > EndDate.Value.AddDays(1);

        public string GetDateRangeDisplay()
        {
            if (!StartDate.HasValue && !EndDate.HasValue)
                return "Always active";
            if (!StartDate.HasValue)
                return $"Until {EndDate.Value:MMM d, yyyy}";
            if (!EndDate.HasValue)
                return $"From {StartDate.Value:MMM d, yyyy}";
            return $"{StartDate.Value:MMM d} – {EndDate.Value:MMM d, yyyy}";
        }
    }
}
