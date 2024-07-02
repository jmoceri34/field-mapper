namespace FieldMapperForDotNet
{
    /// <summary>
    /// These are the options used in the configuration for the field mapper. Use them to customize the behavior of the field mapper.
    /// </summary>
    public class FieldMapperConfigurationOptions
    {
        /// <summary>
        /// This will DeEntitize the content by stripping all html and other relevant characters. If you're pulling plain-text, you can set this to false most likely. If you're pulling html strings, you may want it true.
        /// </summary>
        public bool DeEntitizeContent { get; set; }
    }
}