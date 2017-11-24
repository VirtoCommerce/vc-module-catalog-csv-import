using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvProductMap : ClassMap<CsvProduct>
    {
        public CsvProductMap(CsvProductMappingConfiguration mappingCfg)
        {
            //Dynamical map scalar product fields use by manual mapping information
            var index = 0;

            foreach (var mappingItem in mappingCfg.PropertyMaps.Where(x => !string.IsNullOrEmpty(x.CsvColumnName) || !string.IsNullOrEmpty(x.CustomValue)))
            {
                var propertyInfo = typeof(CsvProduct).GetProperty(mappingItem.EntityColumnName);
                if (propertyInfo != null)
                {
                    var newMap = MemberMap.CreateGeneric(typeof(CsvProduct), propertyInfo);

                    //var newMap = new CsvPropertyMap(propertyInfo);
                    newMap.Data.TypeConverterOptions.CultureInfo = CultureInfo.InvariantCulture;
                    newMap.Data.TypeConverterOptions.NumberStyle = NumberStyles.Any;
                    newMap.Data.TypeConverterOptions.BooleanTrueValues.AddRange(new List<string>() { "yes", "true" });
                    newMap.Data.TypeConverterOptions.BooleanFalseValues.AddRange(new List<string>() { "false", "no" });

                    newMap.Data.Index = ++index;

                    if (!string.IsNullOrEmpty(mappingItem.CsvColumnName))
                    {
                        //Map fields if mapping specified
                        newMap.Name(mappingItem.CsvColumnName);
                    }
                    //And default values if it specified
                    else if (mappingItem.CustomValue != null)
                    {
                        var typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        newMap.Data.ReadingConvertExpression = (Expression<Func<IReaderRow, object>>)(x => typeConverter.ConvertFromString(mappingItem.CustomValue));
                        //newMap.ConvertUsing(row => typeConverter.ConvertFromString(mappingItem.CustomValue));
                        newMap.Default(mappingItem.CustomValue);
                    }
                    MemberMaps.Add(newMap);
                }
            }

            //Map properties
            if (mappingCfg.PropertyCsvColumns != null && mappingCfg.PropertyCsvColumns.Any())
            {
                // Exporting multiple csv fields from the same property (which is a collection)
                foreach (var propertyCsvColumn in mappingCfg.PropertyCsvColumns)
                {
                    // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                    // to export multiple entries for the same property

                    var propertyValuesInfo = typeof(CsvProduct).GetProperty("PropertyValues");
                    var csvPropertyMap = MemberMap.CreateGeneric(typeof(CsvProduct), propertyValuesInfo);
                    csvPropertyMap.Name(propertyCsvColumn);

                    csvPropertyMap.Data.Index = ++index;

                    // create custom converter instance which will get the required record from the collection
                    csvPropertyMap.UsingExpression<ICollection<coreModel.PropertyValue>>(null, propValues =>
                         {
                             var multiValueProperty = propValues.Where(x => x.PropertyName == propertyCsvColumn).ToList();
                             if (multiValueProperty.Count == 1)
                             {
                                 var propValue = multiValueProperty.First();
                                 return propValue.Value?.ToString() ?? string.Empty;
                             }

                             if (multiValueProperty.Count > 1)
                             {
                                 var props = multiValueProperty.Where(x => x.Value != null).Select(x => x.Value.ToString());
                                 var result = string.Join(mappingCfg.Delimiter, props);
                                 return result;
                             }

                             return string.Empty;
                         });

                    MemberMaps.Add(csvPropertyMap);
                }

                var newPropInfo = typeof(CsvProduct).GetProperty("PropertyValues");
                var newPropMap = MemberMap.CreateGeneric(typeof(CsvProduct), newPropInfo);
                newPropMap.Data.ReadingConvertExpression =
                    (Expression<Func<IReaderRow, object>>)(x => mappingCfg.PropertyCsvColumns.Select(column => new coreModel.PropertyValue { PropertyName = column, Value = x.GetField<string>(column) }).ToList());
                newPropMap.UsingExpression<ICollection<coreModel.PropertyValue>>(null, null);

                newPropMap.Data.Index = ++index;

                MemberMaps.Add(newPropMap);
                newPropMap.Ignore(true);
            }

            //map line number
            var lineNumMeber = Map(m => m.LineNumber).ConvertUsing(row => row.Context.RawRow);
            lineNumMeber.Data.Index = ++index;
            lineNumMeber.Ignore(true);
        }
    }
}
