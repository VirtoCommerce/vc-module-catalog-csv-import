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
            if (columnValue.Contains(Delimiter))
            {
                foreach (var value in columnValue.Trim().Split(Delimiter))
                {
                    if (value.Contains(InnerDelimiter))
                    {
                        var splitedValues = value.Split(InnerDelimiter);
                        var languageCode = splitedValues.First();
                        var propertyValue = splitedValues.Last();
                        result.Add(new PropertyValue()
                        {
                            PropertyName = columnName,
                            Value = propertyValue,
                            LanguageCode = languageCode
                        });
                    }
                    else
                    {
                        result.Add(new PropertyValue()
                        {
                            PropertyName = columnName,
                            Value = value.Clone()
                        });
                    }
                }
            }
            else
            {
                result.Add(new PropertyValue
                {
                    PropertyName = columnName,
                    Value = columnValue
                });
            }
            return result;
        }
    }
}
