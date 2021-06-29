using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public static class CsvReaderExtension
    {
        public static string Delimiter { get; set; } = ";";
        public static string InnerDelimiter { get; set; } = "__";
        public static IList<PropertyValue> GetPropertiesByColumn(this IReaderRow reader, string columnName)
        {
            var result = new List<PropertyValue>();
            var columnValue = reader.GetField<string>(columnName);
            foreach(var propertyValue in GetPropertyValue(columnName, columnValue))
            {
                result.Add(propertyValue);
            }
            return result;
        }
        private static IEnumerable<PropertyValue> GetPropertyValue(string columnName, string columnValue)
        {
            foreach (var value in columnValue.Trim().Split(Delimiter))
            {
                var multilanguage = value.Contains(InnerDelimiter);
                var splitedValues = value.Split(InnerDelimiter);
                var languageCode = splitedValues.First();
                var propertyValue = multilanguage ? splitedValues.Last() : value;
                yield return new PropertyValue()
                {
                    PropertyName = columnName,
                    Value = propertyValue,
                    LanguageCode = multilanguage ? languageCode : string.Empty
                };
            }
            yield break;
        }
    }
}
