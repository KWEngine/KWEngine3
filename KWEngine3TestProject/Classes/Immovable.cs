using KWEngine3.GameObjects;

namespace KWEngine3TestProject.Classes
{
    public class Immovable : GameObject
    {
        public override void Act()
        {
            Console.WriteLine(LookAtVector);
            Console.WriteLine(LookAtVectorLocalRight);
            Console.WriteLine(LookAtVectorLocalUp);
            Console.WriteLine(  "-----");
        }
    }
}
