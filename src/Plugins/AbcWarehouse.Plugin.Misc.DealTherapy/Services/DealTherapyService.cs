using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.DealTherapy.Domain;
using Nop.Data;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Services
{
    public class DealTherapyService : IDealTherapyService
    {
        private readonly IRepository<DealTherapySubmission> _submissionRepository;
        private readonly IRepository<DealTherapyAnswer> _answerRepository;

        public DealTherapyService(
            IRepository<DealTherapySubmission> submissionRepository,
            IRepository<DealTherapyAnswer> answerRepository)
        {
            _submissionRepository = submissionRepository;
            _answerRepository = answerRepository;
        }

        public async Task<int> SaveSubmissionAsync(int? customerId, string email, IList<(string questionKey, string answerValue)> answers)
        {
            var submission = new DealTherapySubmission
            {
                CustomerId = customerId,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                CreatedOnUtc = DateTime.UtcNow
            };

            await _submissionRepository.InsertAsync(submission);

            foreach (var (questionKey, answerValue) in answers)
            {
                await _answerRepository.InsertAsync(new DealTherapyAnswer
                {
                    SubmissionId = submission.Id,
                    QuestionKey = questionKey,
                    AnswerValue = answerValue
                });
            }

            return submission.Id;
        }

        public async Task<DealTherapySubmission> GetSubmissionAsync(int id)
        {
            return await _submissionRepository.GetByIdAsync(id);
        }

        public async Task<IList<DealTherapyAnswer>> GetAnswersForSubmissionAsync(int submissionId)
        {
            return await _answerRepository.GetAllAsync(q => q.Where(a => a.SubmissionId == submissionId));
        }
    }
}
