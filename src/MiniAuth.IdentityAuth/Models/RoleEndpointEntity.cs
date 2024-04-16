namespace MiniAuth.IdentityAuth.Models
{
    public class RoleEndpointEntity
    {
        public string Id { get; set; }
        public string Type { get; set; } = "system";
        public string Name { get; set; }
        public string Route { get; set; }
        public string[] Methods { get; set; }
        public bool Enable { get; set; }
        public string[] Roles { get; set; }
        public bool RedirectToLoginPage { get; set; }
    }
}
