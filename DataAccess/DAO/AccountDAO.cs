using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace DataAccess.DAO
{
    public class AccountDAO
    {
        private static AccountDAO instance = null;
        private static readonly object instanceLock = new object();
        private AccountDAO() { }
        public static AccountDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AccountDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<List<UserAccount>> GetMembers()
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var members = await context.UserAccounts
                        .Include(u => u.Role)
                              .Include(u => u.Employee).ThenInclude(u => u.Department)
                        .ToListAsync();

                    return members;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        public static string GenerateHashedPassword(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);

            byte[] hashedPasswordBytes;
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];
                Buffer.BlockCopy(passwordBytes, 0, passwordWithSaltBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, passwordWithSaltBytes, passwordBytes.Length, saltBytes.Length);

                hashedPasswordBytes = sha256.ComputeHash(passwordWithSaltBytes);
            }

            return Convert.ToBase64String(hashedPasswordBytes);
        }
        public static async Task<UserAccount> GetProfile(Guid TblAccountID)
        {
            try
            {
                using (var context = new MyDbContext())
                {

                    IEnumerable<UserAccount> accounts = await GetMembers();
                    UserAccount DiscountCode = accounts.SingleOrDefault(mb => mb.ID == TblAccountID);
                    return DiscountCode;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<UserAccount> Login(string email, string password)
        {

            IEnumerable<UserAccount> members = await GetMembers();
            UserAccount member = members.SingleOrDefault(mb => mb.Username.ToLower().Equals(email.ToLower()) && GenerateHashedPassword(password, mb.SaltPassword) == mb.PasswordHash);
            return member;
        }
        public static async Task AddUserAccount(UserAccount m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p1 = await context.UserAccounts.FirstOrDefaultAsync(c => c.Username.ToLower().Equals(m.Username.ToLower()));
                    var p2 = await context.UserAccounts.FirstOrDefaultAsync(c => c.ID.Equals(m.ID));
                    if (p1 == null)
                    {
                        if (p2 == null)
                        {
                            context.UserAccounts.Add(m);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            throw new Exception("Id is Exits");
                        }
                    }
                    else
                    {
                        throw new Exception("Email is Exist");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateUserAccount(UserAccount m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.UserAccounts.Local.FirstOrDefault(e => e.ID == m.ID);
                    if (existingEntity != null)
                    {
                        // Nếu đã được theo dõi, cập nhật trực tiếp trên đối tượng đó
                        context.Entry(existingEntity).CurrentValues.SetValues(m);
                    }
                    else
                    {
                        // Nếu chưa được theo dõi, đính kèm và đánh dấu là sửa đổi
                        context.Attach(m);
                        context.Entry(m).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task DeleteUserAccount(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.UserAccounts.FirstOrDefaultAsync(c => c.ID == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.UserAccounts.Remove(member);
                        await context.SaveChangesAsync();
                    }



                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }



        public static async Task ChangePassword(Guid UserAccountID, string password)
        {
            try
            {
                var user = new UserAccount() { ID = UserAccountID, PasswordHash = password };
                using (var db = new MyDbContext())
                {
                    db.UserAccounts.Attach(user);
                    db.Entry(user).Property(x => x.PasswordHash).IsModified = true;
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    } 
    }
