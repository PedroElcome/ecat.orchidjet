using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace eCat.OrchidJet.Web.Api
{
    public class ApiModel
    {
        public static string apiBaseUrl = ConfigurationManager.AppSettings["elcomeWebApi:url"].ToString();

        public static string apiVersion = ConfigurationManager.AppSettings["elcomeWebApi:version"].ToString();
    }

    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long expires_in { get; set; }
    }

    public class Baserequest
    {
        public int requestReference { get; set; }
        public string requestPostedTime { get; set; }
        public string system { get; set; }
    }

    public class Baseresponse
    {
        public Baserequest baseRequest { get; set; }
        public string responseTime { get; set; }
        public int resultCode { get; set; }
        public string resultText { get; set; }
    }

    public class EntityRequestObject
    {
        public Baserequest baseRequest { get; set; }
        public int indexReference { get; set; }
        public int requestedPrimary { get; set; }
        public int requestedSecondary { get; set; }
        public List<string> primaryValues { get; set; }
        public Filterrequest filterRequest { get; set; }
    }

    public class Filterrequest
    {
        public string[] brands { get; set; }
        public int[] productTypeReferences { get; set; }
        public int[] categoryReferences { get; set; }
    }

    public class EntityResponseObject
    {
        public EntityResponseObject()
        {
            response = new List<string>();
        }
        public Baseresponse baseResponse { get; set; }
        public List<string> response { get; set; }
    }

    public class VrmRequest
    {
        public Baserequest baseRequest { get; set; }
        public int indexReference { get; set; }
        public string account { get; set; }
        public string userName { get; set; }
        public string vrm { get; set; }
    }


    public class VrmResponse
    {
        public Baseresponse baseResponse { get; set; }
        public Dictionary<string, string> vrmDetails { get; set; }
    }


    public class EntityByExternalIdsRequest
    {
        public Baserequest baseRequest { get; set; }
        public int indexReference { get; set; }
        public List<ReferenceEntityTextRef> references { get; set; }
    }
    public class ReferenceEntityTextRef
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class EntityVehicleResponeObject
    {
        public Baseresponse baseResponse { get; set; }
        public List<Entity> entities { get; set; }
    }

    public class Entity
    {
        public int indexReference { get; set; }
        public string provider { get; set; }
        public int internalReference { get; set; }
        public string externalReference { get; set; }
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public string kw { get; set; }
        public string ps { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string enginesize { get; set; }
        public string datefrom { get; set; }
        public string vehicletype { get; set; }
        public string submodel { get; set; }
        public string mark { get; set; }
        public string trim { get; set; }
        public string enginenumberofcylinders { get; set; }
        public string enginecylinderlayout { get; set; }
        public string enginecamshafttype { get; set; }
        public string enginenumberofvalves { get; set; }
        public string exactenginesize { get; set; }
        public string dateto { get; set; }
        public string bodytype { get; set; }
        public string bhp { get; set; }
        public string drivetype { get; set; }
        public string fueltype { get; set; }
        public string import { get; set; }
        public string enginesizeknownas { get; set; }
        public string enginecode { get; set; }
        public string v8_doors { get; set; }
        public string transmissiontype { get; set; }
        public object greatbritain { get; set; }
        public object classiccar { get; set; }
    }




    public class ApplicationRequest
    {
        public Baserequest baseRequest { get; set; }
        public int indexReference { get; set; }
        public long internalEntityReference { get; set; }
        public Searchproducts searchProducts { get; set; }
    }

    

    public class Searchproducts
    {
        public string partNumber { get; set; }
        public string barcode { get; set; }
        public string externalReference { get; set; }
        public string sku { get; set; }
        public string brand { get; set; }
        public int productReference { get; set; }
        public int productTypeReference { get; set; }
        public string searchOption { get; set; }
    }

    public class ApplicationResponse
    {
        public Baseresponse baseResponse { get; set; }
        public List<Application> applications { get; set; }
        public List<Product> products { get; set; }
        public List<Entity> entities { get; set; }
    }

    public class Application
    {
        public long applicationReference { get; set; }
        public int internalEntityReference { get; set; }
        public long productReference { get; set; }
        public string productBrand { get; set; }
        public bool productIsParent { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int axSort { get; set; }
        public bool publishable { get; set; }
        public DateTime publishFrom { get; set; }
        public Dictionary<string, string> criteria { get; set; }
    }

      public class Product
    {
        public long productReference { get; set; }
        public int categoryReference { get; set; }
        public string categoryName { get; set; }
        public int productTypeReference { get; set; }
        public string productTypeName { get; set; }
        public string partNumber { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public Visibility visibility { get; set; }
        public bool isKitOrAccessory { get; set; }
        public object kitOrAccessory { get; set; }
        public int quantity { get; set; }
        public string brand { get; set; }
        public bool isParent { get; set; }
        public bool isChild { get; set; }
        public bool isClone { get; set; }
        public int parentProductReference { get; set; }
        public List<Childproduct> childProducts { get; set; }
        public Dictionary<string, string> criteria { get; set; }
        public List<Kitcontent> kitContents { get; set; }
        public List<Xreference> xReferences { get; set; }
        public object[] supercedes { get; set; }
        public object[] supercededBy { get; set; }
        public int sortOrder { get; set; }
    }

    public class Visibility
    {
        public bool automate { get; set; }
        public bool ecat { get; set; }
    }


    public class Kitcontent
    {
        public int productReference { get; set; }
        public string partNumber { get; set; }
        public string kitOrAccessory { get; set; }
        public int quantity { get; set; }
        public int sortOrder { get; set; }
    }

    public class Childproduct
    {
        public string brand { get; set; }
        public long productReference { get; set; }
        public string partNumber { get; set; }
        public string relationship { get; set; }
        public int parentProductReference { get; set; }
    }

    public class Xreference
    {
        public string xRef { get; set; }
        public string company { get; set; }
        public object price { get; set; }
        public string info { get; set; }
        public string xRefSearch { get; set; }
        public string xRefType { get; set; }
    }

    public class ProductRequest
    {
        public Baserequest baseRequest { get; set; }
        public List<Searchproduct> searchProducts { get; set; }
    }

    public class Searchproduct
    {
        public string partNumber { get; set; }
        public string barcode { get; set; }
        public string externalReference { get; set; }
        public string sku { get; set; }
        public string brand { get; set; }
        public int productReference { get; set; }
        public int productTypeReference { get; set; }
        public string crossReference { get; set; }
        public string searchOption { get; set; }
    }
    public class ProductResponse
    {
        public Baseresponse baseResponse { get; set; }
        public List<Product> products { get; set; }

    }

    public class EntitiesByPartNumberRequest
    {
        public Baserequest baseRequest { get; set; }
        public int indexReference { get; set; }
        public string partNumber { get; set; }
        public string brand { get; set; }
        public int productTypeReference { get; set; }
        public string searchOption { get; set; }
    }


    public class SimpleRequest
    {
        public Baserequest baseRequest { get; set; }
    }

    public class ProductCategoryResponse
    {
        public Baseresponse baseResponse { get; set; }
        public List<ProductCategory> categories { get; set; }
    }

    public class ProductCategory
    {
        public long categoryReference { get; set; }
        public string categoryName { get; set; }
    }

    public class ProductTypeRequest
    {
        public Baserequest baseRequest { get; set; }
        public string productCategory { get; set; }
        public long productCategoryReference { get; set; }
        public bool includeAvailableCriteria { get; set; }
    }

    public class ProductTypesResponse
    {
        public Baseresponse baseResponse { get; set; }
        public List<ProductType> productTypes { get; set; }
    }

    public class ProductType
    {
        public long categoryReference { get; set; }
        public string categoryName { get; set; }
        public long productTypeReference { get; set; }
        public string productTypeName { get; set; }
        public string productTypeDescription { get; set; }
        public string externalProductTypeReference { get; set; }
        public string externalProductTypeName { get; set; }
        public long sortOrder { get; set; }
        public List<Criteria> criteria { get; set; }
    }

    public class Criteria
    {
        public string type { get; set; }
        public long criteriaReference { get; set; }
        public long groupReference { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
        public string externalName { get; set; }
        public string valueType { get; set; }
        public string value { get; set; }
        public string reference { get; set; }
    }
    public class ProductRequestByProductType
    {
        public Baserequest baseRequest { get; set; }
        public string productType { get; set; }
        public long productTypeReference { get; set; }
        public List<string> brands { get; set; }
    }

    public class AssetRequest
    {
        public Baserequest baseRequest { get; set; }
        public int brandID { get; set; }
        public List<Searchproductasset> searchProductAssets { get; set; }
    }

    public class Searchproductasset
    {
        public int assetID { get; set; }
        public string externalAssetID { get; set; }
        public string partNumber { get; set; }
        public string brand { get; set; }
        public string searchOption { get; set; }
        public string assetType { get; set; }
        public string mediaType { get; set; }
        public string exportSet { get; set; }
    }

    public class AssetResponse
    {
        public Baseresponse baseResponse { get; set; }
        public List<AssetDescriptor> assetDescriptors { get; set; }

    }

    public class AssetDescriptor
    {
        public string partNumber { get; set; }
        public string brandName { get; set; }
        public string assetType { get; set; }
        public string mediaType { get; set; }
        public string assetURL { get; set; }
        public List<string> tags { get; set; }
        public Value values { get; set; }
        public Mapping mappings { get; set; }
    }

    public class Mapping
    {
        public string tecdoc { get; set; }
    }
    public class Value
    {
        public string name { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string assetID { get; set; }
        public string externalAssetID { get; set; }
        public string order { get; set; }
        public string width { get; set; }
        public string height { get; set; }
    }
}