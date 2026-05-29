using System.Collections.Generic;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.DealTherapy.Domain;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Services
{
    public interface IDealTherapyService
    {
        Task<int> SaveSubmissionAsync(int? customerId, string email, IList<(string questionKey, string answerValue)> answers);
        Task<DealTherapySubmission> GetSubmissionAsync(int id);
        Task<IList<DealTherapyAnswer>> GetAnswersForSubmissionAsync(int submissionId);
    }
}
