using DataBaseBusiness.ModelHistory;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VideoGuard.ApiModels;

namespace VxClient.Models
{
    public class HistAlarmHub : Hub
    {
        //參考的代碼而已
        //private readonly IMSContext _context; 
        //public NotiHub(IMSContext context)
        //{
        //    _context = context;
        //}

        //public async Task NotifyPlate(string plate, string image64)
        //{
        //    await Clients.All.SendAsync("Plate", plate, image64);
        //}
    }

    /// <summary>
    /// 警報業務
    /// </summary>
    public class HistAlarmBusiness
    {
        /// <summary>
        /// AddNew 警報任務
        /// </summary>
        /// <param name="histAlarm"></param>
        /// <returns></returns>
        public async Task<bool> AddHistAlarm(HistAlarm histAlarm)
        {
            try
            {
                using HistoryContext historyContext = new HistoryContext();

                await historyContext.HistAlarm.AddAsync(histAlarm);
                bool result = historyContext.SaveChangesAsync().GetAwaiter().GetResult() > 0;
                return result;
            }
            catch (Exception ex)
            {
                Common.CommonBase.OperateDateLoger($"[FUNC::HistAlarmBusiness::AddHistAlarm] [{ex.Message}]");
                return false;
            }
        }
    }
}
