namespace FieldMapperForDotNet
{
    /// <summary>
    /// Use this class to customize the behavior of the field mapper.
    /// </summary>
    public class FieldMapperConfiguration
    {
        /// <summary>
        /// You can modify the options to change how the field mapper works when running <see cref="FieldMapper.Get(string, System.Collections.Generic.IEnumerable{string})"/>
        /// </summary>
        public readonly FieldMapperConfigurationOptions options;

        /// <summary>
        /// By default initalizes the options with <see cref="FieldMapperConfigurationOptions.DeEntitizeContent"/> as true
        /// </summary>
        public FieldMapperConfiguration()
        {
            options = new FieldMapperConfigurationOptions
            {
                DeEntitizeContent = true
            };
        }
    }
}