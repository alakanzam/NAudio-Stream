using SoundClient.Enumeration;

namespace SoundClient.Model
{
    public class RoleModel
    {
        #region Properties

        public string Name { get; set; }

        public AppRole Value { get; set; }

        #endregion

        #region Constructor

        public RoleModel(string name, AppRole value)
        {
            Name = name;
            Value = value;
        }

        #endregion
    }
}