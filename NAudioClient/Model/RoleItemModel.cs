using NAudioClient.Enumerations;

namespace NAudioClient.Model
{
    public class RoleItemModel
    {
        #region Constructor

        /// <summary>
        ///     Initialize role with specific information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public RoleItemModel(string name, ClientRole value)
        {
            Name = name;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Name of role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Value of role.
        /// </summary>
        public ClientRole Value { get; set; }

        #endregion
    }
}