namespace ApplicantManagement.Domain.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Region { get; set; }
        public bool IsActive { get; set; } = true;
    }
}