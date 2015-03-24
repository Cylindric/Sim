using System.IO;

namespace Sim
{
    class ResourceController
    {
        public static string GetDataFile(string name)
        {
            return Path.Combine("Resources", "Data", "name");
        }
    }
}
