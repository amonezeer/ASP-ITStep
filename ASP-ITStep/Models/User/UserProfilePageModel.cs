namespace ASP_ITStep.Models.User
{
    public class UserProfilePageModel
    {
        public bool? IsPersonal { get; set; }
        public String? Name { get; set; } = null!;
        public String? Email { get; set; } = null!;
        public DateTime? Birthdate { get; set; }
        public DateTime? RegisteredAt { get; set; }
}
}
