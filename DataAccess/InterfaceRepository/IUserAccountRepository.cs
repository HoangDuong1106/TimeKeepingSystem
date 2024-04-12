using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository
{

    public interface IUserAccountRepository
    {
        Task<List<UserAccount>> GetMembers();
        Task<UserAccount> LoginMember(string email, string password);
        Task ChangePassword(Guid AccountID, string password);
        Task DeleteMember(Guid m);
        Task UpdateMember(UserAccount m);
        Task AddMember(UserAccount m);
        Task<UserAccount> GetProfile(Guid TblAccountID);

        Task<List<Employee>> GetEmployees();

        Task DeleteEmployee(Guid m);
        Task UpdateEmployee(Employee m);
        Task AddEmployee(Employee m);

        Task<object> GetDepartments();

        Task DeleteDepartment(Guid m);
        Task UpdateDepartment(Team m);
        Task AddDepartment(Team m);


        Task<object> GetHolidays();

        Task DeleteHoliday(Guid m);
        Task UpdateHoliday(Holiday m);
        Task AddHolidayt(Holiday m);

        Task<object> GetWorkTrackSettings();

        Task DeleteWorkTrackSetting(Guid m);
        Task UpdateWorkTrackSetting(WorkTrackSetting m);
        Task AddWorkTrackSetting(WorkTrackSetting m);

        Task<object> GetWorkTimeSettings();

        Task DeleteWorkTimeSetting(Guid m);
        Task UpdateWorkTimeSetting(WorkTimeSetting m);
        Task AddWorkTimeSetting(WorkTimeSetting m);
        Task AddWorkDateSetting(WorkDateSetting m);

        Task<object> GetRiskPerFormanceSettings();

        Task DeleteRiskPerFormanceSetting(Guid m);
        Task UpdateRiskPerFormanceSetting(RiskPerformanceSetting m);
        Task AddRiskPerFormanceSetting(RiskPerformanceSetting m);

        Task<List<LeaveSetting>> GetLeaveSettings();

        Task DeleteLeaveSetting(Guid m);
        Task UpdateLeaveSetting(LeaveSetting m);
        Task AddLeaveSetting(LeaveSetting m);

        Task<object> GetRiskPerFormanceSettingByDepartment(Guid Id);
        Task<object> GetWorkDateSettingByDepartment(Guid Id);
        Task<object> GetWorkTimeSettingByDepartment(Guid Id);
        Task<object> GetLeaveSettingByDepartment(Guid Id);
        Task<object> GetAllWorkTimeSetting();
    }
}