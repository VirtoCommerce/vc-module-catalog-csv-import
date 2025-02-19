# CSV Catalog Export and Import Module

[![CI status](https://github.com/VirtoCommerce/vc-module-catalog-csv-export-import/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-catalog-csv-export-import/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-catalog-csv-export-import&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-catalog-csv-export-import) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-catalog-csv-export-import&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-catalog-csv-export-import) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-catalog-csv-export-import&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-catalog-csv-export-import) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-catalog-csv-export-import&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-catalog-csv-export-import)

The CSV Catalog Export and Import module is a module for Virto Commerce platform that allows to export and import catalog data in CSV format. By leveraging this module, category managers can efficiently manage large sets of catalog data and ensure consistency across systems.

## Key features

* **Export catalog data to CSV**: Extract product and category information into CSV format for easy editing and sharing.
* **Import catalog data from CSV**: Bring updated or new catalog data into the platform from CSV files.
* **Dynamic properties support**: Export and import dynamic properties seamlessly.
* **Multiple product images**: Manage image URLs and groups during export and import.
* **Configurable property mapping: Define custom mappings for CSV columns during import.
* **Customizable column delimiter**: Choose a delimiter for CSV files to suit your needs.
* **Create new dictionary values**: Optionally allow the creation of new dictionary values during import (default: disabled).
* **Bulk update ready**: Simplify bulk updates by exporting data, modifying it in bulk, and importing the updated data back.

## Scenarios

### Export products

1. Open the Catalog module, select either categories or products and click `Export` button.

![Step 1 - Initiate Export](docs/media/csv-export-01.png)

2. Select column delimiter, fulfillment center and pricelist and click `Export` button.

![Step 2 - Configure Export Options](docs/media/csv-export-02.png)

3. Wait for the export to complete and download the CSV file.

![Step 3 - Complete Export](docs/media/csv-export-03.png)

4. Open CSV file in Excel or any other editor.

![Step 4 - View results](docs/media/csv-export-04.png)

### Import products

1. Open the Catalog module, select category and click 'Import' button.

![Step 1 - Initiate Import](docs/media/csv-import-01.png)

2. Select CSV column delimiter, upload CSV file and click 'Map columns' button.

![Step 2 - Prepare for Import](docs/media/csv-import-02.png)

3. Run the import process.

## Settings

The module includes configurable settings to enhance user control:

* **Dictionary value creation**: Enable or disable the ability to create new dictionary values during import.
* **Output file name**: Customize the name of the exported file.

![Settings](docs/media/csv-import-export-settings.png)

## Documentation

* [Catalog CSV Export and Import module user documentation](https://docs.virtocommerce.org/platform/user-guide/catalog-csv-export-import/overview/)
* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.CatalogCsvImportModule)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-catalog-csv-export-import/)


## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-catalog-csv-import/releases)

## License
Copyright (c) Virto Solutions LTD.  All rights reserved.

This software is licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at http://virtocommerce.com/opensourcelicense.

Unless required by the applicable law or agreed to in written form, the software
distributed under the License is provided on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.

