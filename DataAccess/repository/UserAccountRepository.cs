using BusinessObject.DTO;
using DataAccess.DAO;
using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class UserAccountRepository : IUserAccountRepository
    {
        public Task<List<UserAccount>> GetMembers() => AccountDAO.GetMembers();
        public Task<UserAccount> LoginMember(string email, string password) => AccountDAO.Login(email, password);
        public Task DeleteMember(Guid m) => AccountDAO.DeleteUserAccount(m);
        public Task ChangePassword(Guid UserAccountID, string password) => AccountDAO.ChangePassword(UserAccountID, password);
        public Task AddMember(UserAccount m) => AccountDAO.AddUserAccount(m);
        public Task UpdateMember(UserAccount m) => AccountDAO.UpdateUserAccount(m);
        public Task<UserAccount> GetProfile(Guid TblAccountID) => AccountDAO.GetProfile(TblAccountID);


        public Task<List<Employee>> GetEmployees() => EmployeeDAO.GetEmployees();
        public Task DeleteEmployee(Guid m) => EmployeeDAO.DeleteEmployee(m);
        public Task AddEmployee(Employee m) => EmployeeDAO.AddEmployee(m);
        public Task UpdateEmployee(Employee m) => EmployeeDAO.UpdateEmployee(m);

        public Task<object> GetDepartments() => DepartmentDAO.GetDepartments();
        public Task DeleteDepartment(Guid m) => DepartmentDAO.DeleteDepartment(m);
        public Task AddDepartment(Team m) => DepartmentDAO.AddDepartment(m);
        public Task UpdateDepartment(Team m) => DepartmentDAO.UpdateDepartment(m);


        public Task<object> GetHolidays() => HolidayDAO.GetHolidays();
        public Task DeleteHoliday(Guid m) => HolidayDAO.DeleteHoliday(m);
        public Task AddHolidayt(DepartmentHoliday m) => HolidayDAO.AddHoliday(m);
        public Task UpdateHoliday(DepartmentHoliday m) => HolidayDAO.UpdateHoliday(m);

        public Task<object> GetWorkTrackSettings() => WorkTrackSettingDAO.GetWorkTrackSettings();
        public Task DeleteWorkTrackSetting(Guid m) => WorkTrackSettingDAO.DeleteWorkTrackSetting(m);
        public Task UpdateWorkTrackSetting(WorkTrackSetting m) => WorkTrackSettingDAO.UpdateWorkTrackSetting(m);
        public Task AddWorkTrackSetting(WorkTrackSetting m) => WorkTrackSettingDAO.AddWorkTrackSetting(m);

        public Task<object> GetWorkTimeSettings() => WorkTimeSettingDAO.GetWorkTimeSettings();

        public Task DeleteWorkTimeSetting(Guid m) => WorkTimeSettingDAO.DeleteWorkTimeSetting(m);
        public Task UpdateWorkTimeSetting(WorkTimeSetting m) => WorkTimeSettingDAO.UpdateWorkTimeSetting(m);
        public Task AddWorkTimeSetting(WorkTimeSetting m) => WorkTimeSettingDAO.AddWorkTimeSetting(m);
        public Task AddWorkDateSetting(WorkDateSetting m) => WorkTimeSettingDAO.AddWorkDateSetting(m);

        public Task<object> GetRiskPerFormanceSettings() => RiskPerformanceSettingDAO.GetRiskPerformanceSettings();

        public Task DeleteRiskPerFormanceSetting(Guid m) => RiskPerformanceSettingDAO.DeleteRiskPerformanceSetting(m);
        public Task UpdateRiskPerFormanceSetting(RiskPerformanceSetting m) => RiskPerformanceSettingDAO.UpdateRiskPerformanceSetting(m);
        public Task AddRiskPerFormanceSetting(RiskPerformanceSetting m) => RiskPerformanceSettingDAO.AddRiskPerformanceSetting(m);
        public Task<List<LeaveSetting>> GetLeaveSettings() => LeaveSettingDAO.GetLeaveSettings();
        public Task DeleteLeaveSetting(Guid m) => LeaveSettingDAO.DeleteLeaveSetting(m);
        public Task UpdateLeaveSetting(LeaveSetting m) => LeaveSettingDAO.UpdateLeaveSetting(m);
        public Task AddLeaveSetting(LeaveSetting m) => LeaveSettingDAO.AddLeaveSetting(m);

        public Task<object> GetLeaveSettingByDepartment(Guid Id) => WorkTrackSettingDAO.GetLeaveSettingByDeparment(Id);

        public Task<object> GetRiskPerFormanceSettingByDepartment(Guid Id) => WorkTrackSettingDAO.GetRiskSettingByDeparment(Id);
        public Task<object> GetWorkDateSettingByDepartment(Guid Id) => WorkTrackSettingDAO.GetWorkDateSettingByDeparment(Id);
        public Task<object> GetWorkTimeSettingByDepartment(Guid Id) => WorkTrackSettingDAO.GetWorkTimeSettingByDeparment(Id);
        public Task<object> GetAllWorkTimeSetting() => WorkTimeSettingDAO.GetWorkTimeSettings();

    }
}
