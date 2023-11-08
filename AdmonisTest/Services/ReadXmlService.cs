using AdmonisTest.Admonis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace AdmonisTest.Services
{
    public static class ReadXmlService
    {
        static string XML_FILE_PATH = "..\\..\\Xmls\\Product.xml";
        static string JSON_PATH = "..\\..\\Outputs\\";

        /// <summary>
        /// Reads and parses an XML file, creating a dictionary of AdmonisProducts and a dictionary of
        /// product option IDs and their parent IDs. The method then generates a JSON file containing
        /// the parsed data and saves it.
        /// </summary>
        public static void ReadXml()
        {
            // Create an instance of XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            Dictionary<string, AdmonisProduct> products = new Dictionary<string, AdmonisProduct>();
            Dictionary<string, string> productOptionIdAndhisParent = new Dictionary<string, string>();

            try
            {
                // Load the XML file into the XmlDocument
                xmlDoc.Load(XML_FILE_PATH);
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;

                foreach (XmlNode node in childNodes)
                {
                    if (node.Name.Equals("product"))
                    {
                        if (node.LastChild.Name.Equals("variations"))
                        {
                            // The "variants" element is present as a child of parentNode, which is an AdmonisProduct.
                            string parentId = CreateAdmonisProductAndInsertToProductDic(node, products);
                            InsertVariantsToproductOptionDic(parentId, node.LastChild, productOptionIdAndhisParent);
                        }
                        else
                        {
                            // The "variants" element is not present as a child of parentNode, which is an AdmonisProductOption.
                            CreateAdmonisProductOptionAndUpdateProductDic(node, products, productOptionIdAndhisParent);
                        }
                    }
                }

                // Generate a JSON file from the parsed data and save it
                PrintProductsToJsonFile(products.Values.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Recursively inserts product option IDs and their corresponding parent IDs into a dictionary
        /// by parsing a given XML node and its child nodes for product variants and variation groups.
        /// </summary>
        /// <param name="parentId">The ID of the parent product option or group to associate with the child options.</param>
        /// <param name="variantsChild">The XML node representing product variants and variation groups.</param>
        /// <param name="productOptionIdAndhisParent">A dictionary to store product option IDs and their parent IDs.</param>
        private static void InsertVariantsToproductOptionDic(string parentId, XmlNode variantsChild, Dictionary<string, string> productOptionIdAndhisParent)
        {
            foreach (XmlNode currentNode in variantsChild.ChildNodes)
            {
                if (currentNode.Name.Equals("variants"))
                {
                    foreach (XmlNode currentChildNode in currentNode.ChildNodes)
                    {
                        XmlAttribute productIdAttribute = currentChildNode.Attributes["product-id"];
                        if (productIdAttribute != null)
                        {
                            string productId = productIdAttribute.Value;
                            productOptionIdAndhisParent.Add(productId, parentId);
                        }
                    }
                }
                if (currentNode.Name.Equals("variation-groups"))
                {
                    foreach (XmlNode currentChildNode in currentNode.ChildNodes)
                    {
                        XmlAttribute productIdAttribute = currentChildNode.Attributes["product-id"];
                        if (productIdAttribute != null)
                        {
                            string productId = productIdAttribute.Value;
                            productOptionIdAndhisParent.Add(productId, parentId);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Parses the XML node representing a product option, extracts product option details
        /// such as color and size, and associates it with the corresponding parent product in the dictionaries.
        /// </summary>
        /// <param name="xmlNode">The XML node representing the product option.</param>
        /// <param name="products">A dictionary of AdmonisProducts indexed by their unique IDs.</param>
        /// <param name="productOptionIdAndhisParent">
        /// A dictionary mapping product option IDs to their parent product IDs.
        /// </param>
        private static void CreateAdmonisProductOptionAndUpdateProductDic(XmlNode xmlNode, Dictionary<string, AdmonisProduct> products, Dictionary<string, string> productOptionIdAndhisParent)
        {
            string productId = "", optionColor = "", optionSize = "";

            // Extract the product ID attribute from the XML node
            XmlAttribute productIdAttribute = xmlNode.Attributes["product-id"];
            if (productIdAttribute != null)
            {
                productId = productIdAttribute.Value;
            }

            foreach (XmlNode currentNode in xmlNode.ChildNodes)
            {
                if (currentNode.Name.Equals("custom-attributes"))
                {
                    foreach (XmlNode currentChildNode in currentNode.ChildNodes)
                    {
                        // Extract the attribute ID for color and size
                        XmlAttribute currentAttribute = currentChildNode.Attributes["attribute-id"];
                        if (currentAttribute != null && currentAttribute.Value.Contains("f54ProductColor"))
                        {
                            optionColor = currentChildNode.InnerText;
                        }
                        else if (currentAttribute != null && currentAttribute.Value.Contains("f54ProductSize"))
                        {
                            optionSize = currentChildNode.InnerText;
                        }

                        // If both color and size have been found, exit the loop
                        if (IsExists(optionColor) && IsExists(optionSize))
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            // Create an AdmonisProductOption with the extracted color and size
            AdmonisProductOption productOption = new AdmonisProductOption(optionColor, optionSize);

            string parentId;
            productOptionIdAndhisParent.TryGetValue(productId, out parentId);

            if (string.IsNullOrEmpty(parentId))
            {
                // Handle the case where there is no parent product for the product option
                // throw new Exception($"There is no ParentProduct to productOption id :{productId}");
                Console.WriteLine($"There is no ParentProduct to productOption id :{productId}");
                return;
            }

            // Get the parent product from the products dictionary and associate the product option with it
            AdmonisProduct parentProduct;
            products.TryGetValue(parentId, out parentProduct);

            if (parentProduct != null)
            {
                parentProduct.Options.Add(productOption);
                productOptionIdAndhisParent.Remove(productId);
            }
        }


        /// <summary>
        /// Parses the XML node representing a product, extracts its details such as name, descriptions,
        /// brand, and video link, and creates an AdmonisProduct object. It inserts the product into
        /// a dictionary of AdmonisProducts.
        /// </summary>
        /// <param name="xmlNode">The XML node representing the product.</param>
        /// <param name="products">A dictionary of AdmonisProducts indexed by their unique IDs.</param>
        /// <returns>The unique ID of the newly created AdmonisProduct.</returns>
        private static string CreateAdmonisProductAndInsertToProductDic(XmlNode xmlNode, Dictionary<string, AdmonisProduct> products)
        {
            string productId = "", name = "", shortDescription = "", longDescription = "", brand = "", videoLink = "";

            // Extract the product ID attribute from the XML node
            XmlAttribute productIdAttribute = xmlNode.Attributes["product-id"];
            if (productIdAttribute != null)
            {
                productId = productIdAttribute.Value;
            }

            foreach (XmlNode currentNode in xmlNode.ChildNodes)
            {
                // Extract and assign details based on the current node's name
                if (currentNode.Name.Equals("display-name"))
                {
                    name = GetFirstChildText(currentNode);
                }
                else if (currentNode.Name.Equals("short-description"))
                {
                    shortDescription = GetFirstChildText(currentNode);
                }
                else if (currentNode.Name.Equals("long-description"))
                {
                    longDescription = GetFirstChildText(currentNode);
                }
                else if (currentNode.Name.Equals("brand"))
                {
                    brand = GetFirstChildText(currentNode);
                }
                else if (currentNode.Name.Equals("custom-attributes"))
                {
                    foreach (XmlNode currentChildNode in currentNode.ChildNodes)
                    {
                        // Extract the video link attribute
                        XmlAttribute currentAttribute = currentChildNode.Attributes["attribute-id"];
                        if (currentAttribute != null && currentAttribute.Value.Contains("f54ProductVideo"))
                        {
                            videoLink = currentChildNode.InnerText;
                            break;
                        }
                    }
                }

                // If all necessary details have been found, exit the loop
                if (IsExists(name) && IsExists(shortDescription) && IsExists(longDescription) && IsExists(brand) && IsExists(videoLink))
                {
                    break;
                }
            }

            // Create an AdmonisProduct with the extracted details and insert it into the dictionary
            AdmonisProduct product = new AdmonisProduct(name, shortDescription, longDescription, brand, videoLink);
            products.Add(productId, product);

            return productId;
        }

        private static void PrintProductsToJsonFile(List<AdmonisProduct> admonisProducts)
        {
            // Specify the file path where you want to save the JSON
            string jsonFilePath = JSON_PATH + $"products_{DateTime.Now:yyyyMMddHHmm}.json";

            // Serialize the list to JSON and write it to the file
            using (var stream = new FileStream(jsonFilePath, FileMode.Create))
            {
                JsonSerializer.Serialize(stream, admonisProducts, new JsonSerializerOptions
                {
                    WriteIndented = true, // Make the JSON human-readable.
                });
            }

            Console.WriteLine($"Data has been serialized to {jsonFilePath}");
        }

        private static bool IsExists(string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        private static string GetFirstChildText(XmlNode xmlNode)
        {
            if (xmlNode.ChildNodes.Count > 0)
            {
                return xmlNode.FirstChild.InnerText;
            }
            return "";
        }
    }
}
;