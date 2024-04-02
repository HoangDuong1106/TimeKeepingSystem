using DataAccess;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;
using DataAccess.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BusinessObject.Model;
using DataAccess.service;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services

var configuration = builder.Configuration;
var secretKey = configuration["Appsettings:SecretKey"];
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
builder.Services.AddDbContext<MyDbContext>(options =>
//options.UseSqlServer(configuration.GetConnectionString("local")));
options.UseSqlServer(configuration.GetConnectionString("online")));
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeKeeping", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod()
     .AllowAnyHeader());
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddSwaggerGen();
// Register repository classes
builder.Services.AddScoped<IRepository<AttendanceStatus>, AttendanceStatusRepository>();
builder.Services.AddScoped<IRepository<Team>, TeamRepository>();
builder.Services.AddScoped<IRepository<DepartmentHoliday>, DepartmentHolidayRepository>();
builder.Services.AddScoped<IRepository<DepartmentHolidayException>, DepartmentHolidayExceptionRepository>();
builder.Services.AddScoped<IRepository<Employee>, EmployeeRepository>();
builder.Services.AddScoped<IRepository<LeaveSetting>, LeaveSettingRepository>();
builder.Services.AddScoped<IRepository<LeaveType>, LeaveTypeRepository>();
builder.Services.AddScoped<IRepository<Request>, RequestRepository>();
builder.Services.AddScoped<IRepository<RequestLeave>, RequestLeaveRepository>();
builder.Services.AddScoped<IRepository<RequestOverTime>, RequestOverTimeRepository>();
builder.Services.AddScoped<IRepository<RequestWorkTime>, RequestWorkTimeRepository>();
builder.Services.AddScoped<IRepository<RiskPerformanceEmployee>, RiskPerformanceEmployeeRepository>();
builder.Services.AddScoped<IRepository<RiskPerformanceSetting>, RiskPerformanceSettingRepository>();
builder.Services.AddScoped<IRepository<WorkDateSetting>, WorkDateSettingRepository>();
builder.Services.AddScoped<IRepository<WorkingStatus>, WorkingStatusRepository>();
builder.Services.AddScoped<IRepository<WorkPermissionSetting>, WorkPermissionSettingRepository>();
builder.Services.AddScoped<IRepository<Workslot>, WorkslotRepository>();
builder.Services.AddScoped<IRepository<WorkslotEmployee>, WorkslotEmployeeRepository>();
builder.Services.AddScoped<IRepository<WorkTimeSetting>, WorkTimeSettingRepository>();
builder.Services.AddScoped<IRepository<WorkTrackSetting>, WorkTrackSettingRepository>();
builder.Services.AddScoped<IRepository<Wifi>, WifiRepository>();

builder.Services.AddScoped<IAttendanceStatusRepository, AttendanceStatusRepository>();
builder.Services.AddScoped<IDepartmentHolidayExceptionRepository, DepartmentHolidayExceptionRepository>();
builder.Services.AddScoped<IDepartmentHolidayRepository, DepartmentHolidayRepository>();
builder.Services.AddScoped<IDepartmentRepository, TeamRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveSettingRepository, LeaveSettingRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<IRequestLeaveRepository, RequestLeaveRepository>();
builder.Services.AddScoped<IRequestOverTimeRepository, RequestOverTimeRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestWorkTimeRepository, RequestWorkTimeRepository>();
builder.Services.AddScoped<IRiskPerformanceEmployeeRepository, RiskPerformanceEmployeeRepository>();
builder.Services.AddScoped<IRiskPerformanceSettingRepository, RiskPerformanceSettingRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IWorkDateSettingRepository, WorkDateSettingRepository>();
builder.Services.AddScoped<IWorkingStatusRepository, WorkingStatusRepository>();
builder.Services.AddScoped<IWorkPermissionSettingRepository, WorkPermissionSettingRepository>();
builder.Services.AddScoped<IWorkslotEmployeeRepository, WorkslotEmployeeRepository>();
builder.Services.AddScoped<IWorkslotRepository, WorkslotRepository>();
builder.Services.AddScoped<IWorkTimeSettingRepository, WorkTimeSettingRepository>();
builder.Services.AddScoped<IWorkTrackSettingRepository, WorkTrackSettingRepository>();
builder.Services.AddScoped<IWifiRepository, WifiRepository>();


builder.Services.AddScoped<IAttendanceStatusService, AttendanceStatusService>();
builder.Services.AddScoped<IDepartmentHolidayExceptionService, DepartmentHolidayExceptionService>();
builder.Services.AddScoped<IDepartmentHolidayService, DepartmentHolidayService>();
builder.Services.AddScoped<IDepartmentService, TeamService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveSettingService, LeaveSettingService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<IRequestLeaveService, RequestLeaveService>();
builder.Services.AddScoped<IRequestOverTimeService, RequestOverTimeService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IRequestWorkTimeService, RequestWorkTimeService>();
builder.Services.AddScoped<IRiskPerformanceEmployeeService, RiskPerformanceEmployeeService>();
builder.Services.AddScoped<IRiskPerformanceSettingService, RiskPerformanceSettingService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IWorkDateSettingService, WorkDateSettingService>();
builder.Services.AddScoped<IWorkingStatusService, WorkingStatusService>();
builder.Services.AddScoped<IWorkPermissionSettingService, WorkPermissionSettingService>();
builder.Services.AddScoped<IWorkslotEmployeeService, WorkslotEmployeeService>();
builder.Services.AddScoped<IWorkslotService, WorkslotService>();
builder.Services.AddScoped<IWorkTimeSettingService, WorkTimeSettingService>();
builder.Services.AddScoped<IWorkTrackSettingService, WorkTrackSettingService>();
builder.Services.AddScoped<IWifiService, WifiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.

}

app.UseRouting();
app.UseAuthentication();


// Thêm middleware UseAuthorization() ở đây
app.UseAuthorization();
app.UseCors(builder => builder
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed((host) => true)
              .AllowCredentials().WithOrigins("https://localhost:5001",
                                              "https://localhost:3000"
                                              )
          );

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseSwagger();

// Enable middleware to serve Swagger UI (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
});
var swaggerUiUrl = "/swagger/index.html";
app.Run(context =>
{
    context.Response.Redirect(swaggerUiUrl);
    return Task.CompletedTask;
});
app.UseHttpsRedirection();




app.MapControllers();
app.Run();