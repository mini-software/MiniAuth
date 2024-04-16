namespace MiniAuth.IdentityAuth.Models
{
    internal class ResponseVo
    {
        public bool ok { get; set; } = true;
        public int code { get; set; } = 200;
        public string message { get; set; } = "";
        public object data { get; set; }
    }
}
