using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Models;

namespace UI.Services
{
    public interface ISessionExecutionService
    {
        void UpdateServerParamsFromSettings();
        Feedback GetCurrentFeedback();
        bool SessionIsSet();
        IamSession GetIamSession();
        Task<Session> GetStudentActiveSession();
        bool SessionHasFeedback();
    }
}
