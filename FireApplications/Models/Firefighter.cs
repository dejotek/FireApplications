using System.Collections.ObjectModel;

namespace FireApplications.Models
{
    public class Firefighter
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public ObservableCollection<string> Roles { get; set; } = new();

        // Scal role w łańcuch dla DataGrid
        public string RolesString => string.Join(", ", Roles);
    }
}