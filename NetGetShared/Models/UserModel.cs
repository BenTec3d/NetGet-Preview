using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGetShared
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is UserModel model &&
                   Name == model.Name;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
