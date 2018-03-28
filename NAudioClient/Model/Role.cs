using NAudioClient.Enumerations;

namespace NAudioClient.Model
{
    public class Role
    {
        #region Constructor

        /// <summary>
        ///     Initialize role with specific information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Role(string name, ApplicationRole value)
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
        public ApplicationRole Value { get; set; }

        #endregion
    }
}