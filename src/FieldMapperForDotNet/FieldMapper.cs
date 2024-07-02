using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FieldMapperForDotNet
{
    /// <summary>
    /// This is the main class used for mapping fields
    /// </summary>
    public class FieldMapper
    {
        /// <summary>
        /// The configuration holds options that can change the behavior of the tool, such as choosing whether to DeEntitize the content first.
        /// </summary>
        private readonly FieldMapperConfiguration configuration;

        /// <summary>
        /// By default it uses <see cref="FieldMapperConfiguration"/>
        /// </summary>
        public FieldMapper(): this(new FieldMapperConfiguration()) { }

        /// <summary>
        /// You can pass in your own <see cref="FieldMapperConfiguration"/>
        /// </summary>
        /// <param name="configuration"></param>
        public FieldMapper(FieldMapperConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Use this to see what the mappings should look like before they're mapped to values.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="mappings">The mappings to apply to the content.</param>
        /// <returns></returns>
        public string PreviewContent(string content, IEnumerable<string> mappings)
        {
            // whether or not to decode encoded characters and strip html in the content, by default is true
            if (configuration.options.DeEntitizeContent)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            }

            // replace all line breaks with spaces
            content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");

            // so we can create a single line break between mappings, this helps with parsing
            SeparateMappingsByLineBreaks();

            return content;

            void SeparateMappingsByLineBreaks()
            {
                // for each mapping
                foreach (var searchMapping in mappings)
                {
                    // get the very first occurrence of it in the content
                    var startIndex = GetIndexOfKey(content, mappings, searchMapping);
                    var nextLocation = int.MaxValue;

                    // if we haven't added a line break yet, and found a mapping
                    if (!content.Contains(Environment.NewLine) && startIndex != -1)
                    {
                        // insert it right before it
                        content = content.Insert(startIndex, Environment.NewLine);
                    }

                    // check through the other mappings
                    foreach (var key in mappings.Where(k => k != searchMapping))
                    {
                        // searching past the mapping found
                        var loc = content.IndexOf(key, startIndex + searchMapping.Length);

                        // if we found the mapping, and it's the earliest one yet
                        if (loc != -1 && loc < nextLocation)
                        {
                            // set it
                            nextLocation = loc;
                        }
                    }

                    // if we ended up finding the next location
                    if (nextLocation != int.MaxValue)
                    {
                        // insert a line break before it
                        content = content.Insert(nextLocation, Environment.NewLine);
                    }
                }
            }

            int GetIndexOfKey(string content, IEnumerable<string> mappings, string searchKey)
            {
                var nestedKey = false;

                // get the keys we're not currently searching for
                var nonSearchedKeys = mappings.Where(k => k != searchKey);

                // check the non searched keys to see if they contain the actual search key
                foreach (var key in nonSearchedKeys)
                {
                    if (key.Contains(searchKey))
                    {
                        // if it does then we found a nested key
                        nestedKey = true;
                    }
                }

                // if we found a nested key
                if (nestedKey)
                {
                    var tempContent = content;

                    // get an ordered list of mappings by length with the largest first
                    var orderedKeys = mappings.OrderByDescending(m => m.Length).ToList();
                    for (var i = 0; i < mappings.Count(); i++)
                    {
                        // convert the keys to uppercase to separate them, this requires the mappings to not be uppercase beforehand
                        tempContent = tempContent.Replace(orderedKeys[i], orderedKeys[i].ToUpperInvariant());
                    }

                    // find the keys larger than the search key from the non searched ones
                    var nonSearchedLargerKeys = nonSearchedKeys.Where(k => k.Length > searchKey.Length);

                    foreach (var key in nonSearchedLargerKeys)
                    {
                        // convert all of those to lower
                        tempContent = tempContent.Replace(key.ToUpperInvariant(), key.ToLowerInvariant());
                    }

                    // to isolate the search key
                    return tempContent.IndexOf(searchKey.ToUpperInvariant());
                }
                else
                {
                    // otherwise find the first occurrence of it in the content
                    return content.IndexOf(searchKey);
                }
            }
        }

        /// <summary>
        /// This is the main method for getting values out of a string with mappings.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="mappings">The mappings.</param>
        /// <returns></returns>
        public IDictionary<string, string> Get(string content, IList<string> mappings)
        {
            // validate content and mappings and throw and error if it fails
            Validate();

            // when we preview the content, we apply config-specific logic, remove all line breaks and re-add them between the mappings
            content = PreviewContent(content, mappings);

            var result = new Dictionary<string, string>();

            // use a string reader to parse the content and search for mappings
            using (var reader = new StringReader(content))
            {
                var line = reader.ReadLine()?.Trim();

                while (line != null)
                {
                    // for each line check to see if any mappings are on it
                    for (var i = 0; i < mappings.Count(); i++)
                    {
                        var mapping = mappings[i];

                        // we found a mapping on a line and the dictionary doesn't contain the mapping
                        if (line.Contains(mapping) && line.IndexOf(mapping) == 0 && !result.ContainsKey(mapping))
                        {
                            // since mappings are per line get everything after it
                            var value = line.Substring(line.IndexOf(mapping) + mapping.Length).Trim();

                            // add it
                            result.Add(mapping, value);
                            
                            // mappings are separated by line breaks above so once you found it don't check anymore
                            break;
                        }
                    }

                    // get the next line
                    line = reader.ReadLine();
                }
            }

            return result;

            void Validate()
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new ArgumentException("Content cannot be null or empty.", nameof(content));
                }

                if (mappings == null || mappings.Count() == 0)
                {
                    throw new ArgumentException("Mappings cannot be null or empty.", nameof(mappings));
                }
                
                if (mappings.Any(m => string.IsNullOrWhiteSpace(m)))
                {
                    throw new ArgumentException("Mappings cannot contain any empty values.", nameof(mappings));
                }

                if (mappings.Distinct().Count() != mappings.Count())
                {
                    throw new ArgumentException("Duplicate mappings found. Please make sure they are all unique.");
                }
            }
        }
    }
}