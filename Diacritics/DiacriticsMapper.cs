﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Diacritics.AccentMappings;

namespace Diacritics
{
    public class DiacriticsMapper : IDiacriticsMapper
    {
        #region DiacriticsMapper.Current
        private static Lazy<IDiacriticsMapper> implementation;

        static DiacriticsMapper()
        {
            SetDefaultMapper(CreateDefaultDiacriticsMapper);
        }

        public static IDiacriticsMapper Current => implementation.Value;

        public static void SetDefaultMapper(Func<IDiacriticsMapper> factory)
        {
            implementation = new Lazy<IDiacriticsMapper>(factory, LazyThreadSafetyMode.PublicationOnly);
        }

        private static IDiacriticsMapper CreateDefaultDiacriticsMapper()
        {
            return new DefaultDiacriticsMapper();
        }
        #endregion

        private Dictionary<char, string> diacriticsMapping;

        public DiacriticsMapper(params IAccentMapping[] mappings)
        {
            this.UpdateMappings(mappings);
        }

        private void UpdateMappings(IAccentMapping[] mappings)
        {
            this.diacriticsMapping = new Dictionary<char, string>();
            var all = new List<KeyValuePair<char, string>>();

            if (mappings != null)
            {
                foreach (var accentMapping in mappings)
                {
                    var map = accentMapping.Mapping;
                    all.AddRange(map);
                }
            }

            // Group keys so that duplicates are eliminated
            this.diacriticsMapping = all
                .GroupBy(x => x.Key)
                .ToDictionary(k => k.Key, v => v.First().Value);
        }

        public IEnumerator<KeyValuePair<char, string>> GetEnumerator()
        {
            return this.diacriticsMapping.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string RemoveDiacritics(string input, DiacriticsOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var result = new StringBuilder(input.Length);
            var inputLowerCase = input.ToLowerInvariant();

            for (var currentIndex = 0; currentIndex < input.Length; currentIndex++)
            {
                var characterLowerCase = inputLowerCase[currentIndex];
                if (this.diacriticsMapping.TryGetValue(characterLowerCase, out var diacriticRemovedChar))
                {
                    // If the diacritic character from the input is an uppercase letter,
                    // we also want to have the non-diacritic character to be an uppercase letter.
                    if (char.IsUpper(input[currentIndex]))
                    {
                        diacriticRemovedChar = diacriticRemovedChar.ToUpper();
                    }

                    result.Append(diacriticRemovedChar);
                }
                else
                {
                    result.Append(input[currentIndex]);
                }
            }

            return result.ToString();
        }

        public bool HasDiacritics(string source, DiacriticsOptions options = null)
        {
            return source != this.RemoveDiacritics(source, options);
        }
    }
}