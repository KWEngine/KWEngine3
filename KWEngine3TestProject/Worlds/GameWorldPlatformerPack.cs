using KWEngine3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldPlatformerPack : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            DirectoryInfo di = new DirectoryInfo("./Models/PlatformerPack");
            foreach(FileInfo fi in di.GetFiles())
            {
                KWEngine.LoadModel(fi.Name.Substring(0, fi.Name.LastIndexOf('.')), "./Models/PlatformerPack/" + fi.Name);
            }


        }
    }
}
