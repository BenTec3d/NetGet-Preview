using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGetShared
{
    public class GroupModel
    {
        public string Name { get; set; }
        public List<ContentModel> Content { get; set; }

        public GroupModel()
        {
            Content = new List<ContentModel>();
        }
        public GroupModel(string name)
        {
            Name = name;
            Content = new List<ContentModel>();
        }

        public override bool Equals(object? obj)
        {
            return obj is GroupModel model &&
                   Name == model.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
