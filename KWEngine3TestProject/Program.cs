namespace KWEngine3TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (GameWindow gw = new GameWindow())
            {
                gw.Run();
            }
        }
    }
}