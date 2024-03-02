namespace BusinessObject.DTO
{
    public class DepartmentDTO
    {
        public Guid? Id { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? WorkTrackId { get; set; }
        public bool? IsDeleted { get; set; }
        public string? Name { get; set; }
    }
}