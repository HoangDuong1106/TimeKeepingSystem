namespace DataAccess.InterfaceRepository { public interface IRequestRepository { Task<bool> SoftDeleteAsync(Guid id); } }