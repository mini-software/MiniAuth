namespace MiniAuth
{
    public class MiniAuthUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Emp_no { get; set; }
        public string Mail { get; set; }
        public bool Enable { get; set; }
        public string[] Roles { get; set; }
    }
}