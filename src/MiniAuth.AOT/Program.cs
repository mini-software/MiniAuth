namespace MiniAuth.AOT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);
            var app = builder.Build();
            app.UseMiniAuth();
            app.MapGet("/", () => "Hello MiniAuth! Please view https://github.com/mini-software/MiniAuth");
            app.Run();
        }
    }
}
