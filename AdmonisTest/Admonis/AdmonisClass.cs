using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AdmonisTest.Admonis
{
    [XmlRoot("product", Namespace = "http://www.demandware.com/xml/impex/catalog/2006-10-31")]
    public class AdmonisProduct
    {
        public int CustomerID { get; set; }

        [XmlAttribute("DisplayName")]
        public string Name { get; set; }

        [XmlAttribute("short-description")]
        public string Description { get; set; }

        [XmlAttribute("long-description")]
        public string DescriptionLong { get; set; }
        public string WarrantyPeriod { get; set; }
        public string WarrantyBy { get; set; }
        public string Makat { get; set; }
        public string Model { get; set; }
        public decimal Price_Cost_Customer { get; set; }
        public decimal Price_Cost { get; set; }
        public decimal Price_Market { get; set; }
        public decimal Price_Publish { get; set; }

        [XmlAttribute("brand")]
        public string Brand { get; set; }
        public string Volume { get; set; }
        public string ClassificationID { get; set; }
        public string SubClass { get; set; }
        public string PlatformCategoryID { get; set; }
        public string VolumeType { get; set; }
        public string VideoLink { get; set; }
        public string StorageLocation { get; set; }
        public string UPC { get; set; }
        public string StatusID { get; set; }
        public string StatusComments { get; set; }
        public string Package { get; set; }

        public List<AdmonisProductOption> Options { get; set; } = new List<AdmonisProductOption>();


        public AdmonisProduct(string name, string description, string descriptionLong, string brand, string videoLink)
        {
            Name = name;
            Description = description;
            DescriptionLong = descriptionLong;
            Brand = brand;
            VideoLink = videoLink;
        }
    }

    public class AdmonisProductOption
    {
        public string optionSugName1 { get; set; }
        public string optionSugName1Title { get; set; }
        public string optionSugName2 { get; set; }
        public string optionSugName2Title { get; set; }
        public string ProductMakat { get; set; }
        public string optionMakat { get; set; }
        public string optionName { get; set; }
        public string optionModel { get; set; }
        public decimal optionPrice_Cost_Customer { get; set; }
        public decimal optionPrice_Cost { get; set; }
        public decimal optionPrice_Publish { get; set; }
        public decimal optionPrice_Market { get; set; }
        public string optionstorageLocation { get; set; }
        public string optionupc { get; set; }

        public AdmonisProductOption(string color, string size)
        {
            optionSugName2 = color;
            optionName = size;
            optionSugName1 = "צבע";
            optionSugName1Title = "בחר צבע";
            optionSugName2Title = "בחר מידה";
        }
    }
}
