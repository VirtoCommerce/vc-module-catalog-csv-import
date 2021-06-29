using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public static class CsvReaderExtension
    {
        public static string Delimiter { get; set; } = ";";
        public static string InnerDelimiter { get; set; } = "__";
        public static IEnumerable<PropertyValue> GetPropertiesByColumn(this IReaderRow reader, string columnName)
        {
            var columnValue = reader.GetField<string>(columnName);
            foreach (var value in columnValue.Trim().Split(Delimiter))
            {
                var multilanguage = value.Contains(InnerDelimiter);
                var splitedValues = value.Split(InnerDelimiter);
                var languageCode = splitedValues.First();
                var propertyValue = multilanguage ? splitedValues.Last() : value;
                yield return new PropertyValue
                {
                    PropertyName = columnName,
                    Value = propertyValue,
                    LanguageCode = multilanguage ? languageCode : string.Empty
                };
            }
        }
    }
}
