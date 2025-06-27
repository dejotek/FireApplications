namespace FireApplications.Models
{
    public class RoleOption
    {
        public string Name { get; }
        public bool IsSelected { get; set; }
        public RoleOption(string name) => Name = name;
    }
}