
using eCat.OrchidJet.Models;
using Newtonsoft.Json;
using RestSharp;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
//using System.Web.Script.Serialization;
using Microsoft.AspNetCore.Http;

namespace eCat.OrchidJet.Web.Api
{
    public class ApiService
    {
        //test
        private static IHttpContextAccessor _contextAccessor;

        public static HttpContext Current => _contextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        //test
        public string CallApi<T>(string resource, T content, Token tokenDetail)
        {
            RestClient restClient = new RestClient(ApiModel.apiBaseUrl);
            var request = new RestRequest(resource) { Method = Method.POST };
            request.Parameters.Clear();
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("application/json", JsonConvert.SerializeObject(content), ParameterType.RequestBody);
            request.AddHeader("Authorization", $"{tokenDetail.token_type} {tokenDetail.access_token}");
            request.AddHeader("Referer", string.Format("{0}://{1}", _contextAccessor.HttpContext.Request.Scheme, _contextAccessor.HttpContext.Request.Host));
            var readResult = restClient.Execute(request).Content;
            return readResult;
        }

        public List<string> GetMakes()
        {
            List<string> makes = new List<string>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                EntityRequestObject content = new EntityRequestObject()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    requestedPrimary = 1,
                    primaryValues = new List<string>() { }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getPrimaryEntityValuesByCascadingApplicatedPrimaries", content, tokenDetail);
                var entityresponse = JsonConvert.DeserializeObject<EntityResponseObject>(readresult);
                makes = entityresponse.response;
            }
            catch (Exception ex)
            {
                return makes;
            }
            return makes.Distinct().ToList();
        }

        public List<Tuple<string, string>> getModels(string make)
        {
            // this needs to display model> body > kmod years. model and body are configured in IDL but no kmod years available. We can try and work them out though.
            // we return a tuple<model, date>

            List<string> applicatedModels = new List<string>();
            List<Tuple<string, string>> returnModels = new List<Tuple<string, string>>();

            try
            {
                // get our actual applicated models - we can then add the dates on later
                var tokenDetail = getAuthenticationToken();
                var baseRequest = new Baserequest
                {
                    requestReference = 0,
                    requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                    system = "ecat"
                };

                EntityRequestObject requestModelContent = new EntityRequestObject()
                {
                    baseRequest = baseRequest,
                    indexReference = 1,
                    requestedPrimary = 2,
                    primaryValues = new List<string>() { make }
                };

                var modelResult = CallApi($"/{ApiModel.apiVersion}/entity/getPrimaryEntityValuesByCascadingApplicatedPrimaries", requestModelContent, tokenDetail);
                var modelResponse = JsonConvert.DeserializeObject<EntityResponseObject>(modelResult);
                applicatedModels = modelResponse.response;


                // get our delta auto parts formatted models by getting all entities by make
                EntityRequestObject content = new EntityRequestObject()
                {
                    baseRequest = baseRequest,
                    indexReference = 1,
                    requestedPrimary = 1,
                    primaryValues = new List<string>() { make }
                };

                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getEntitiesByCascadingPrimaries", content, tokenDetail);
                var model = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult);


                // loop through the applicated models and get the min/max years
                foreach (var applicatedModel in applicatedModels)
                {
                    List<int> sYear = new List<int>();
                    List<int> eYear = new List<int>();

                    // loop through the year
                    foreach (var entityDate in model.entities.Where(C => C.attributes.model == applicatedModel).Select(c => c.attributes.datefrom))
                    {
                        // split the date
                        var date = entityDate.Split(new string[] { "->" }, StringSplitOptions.None).Select(c => c.TrimStart().TrimEnd()).ToArray();
                        if (date.Count() == 2)
                        {
                            DateTime entityDateFrom = DateTime.MinValue;
                            DateTime entityDateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                            if(DateTime.TryParse(date[0], out entityDateFrom))
                                sYear.Add(entityDateFrom.Year);

                            if(DateTime.TryParse(string.IsNullOrEmpty(date[1]) ? entityDateTo.ToString() : date[1], out entityDateTo))
                                eYear.Add(entityDateTo.Year > DateTime.Now.Year ? DateTime.Now.Year : entityDateTo.Year);
                        }
                    }

                    returnModels.Add(new Tuple<string, string>(applicatedModel, $"{sYear.Min()}>{eYear.Max()}"));
                }
            }
            catch (Exception ex)
            {
                return returnModels;
            }

            // return this back
            return returnModels;
        }

        public List<string> getLitre(string make, string model)
        {
            List<string> litres = new List<string>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                EntityRequestObject content = new EntityRequestObject()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    requestedPrimary = 3,
                    primaryValues = new List<string>() { make, model }
                };

                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getPrimaryEntityValuesByCascadingApplicatedPrimaries", content, tokenDetail);
                var entityresponse = JsonConvert.DeserializeObject<EntityResponseObject>(readresult);
                litres = entityresponse.response;
                
            }
            catch (Exception ex)
            {
                return litres;
            }
            return litres.Distinct().OrderBy(c => c).ToList();
        }

        public List<string> getYear(string make, string model,string litre)
        {
            var tokenDetail = getAuthenticationToken();

            List<string> year = new List<string>();
            EntityRequestObject content = new EntityRequestObject()
            {
                baseRequest = new Baserequest
                {
                    requestReference = 0,
                    requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                    system = "ecat"
                },
                indexReference = 1,
                requestedPrimary = 4,
                primaryValues = new List<string>() { make, model,litre }
            };

            var readResult = CallApi<EntityRequestObject>($"/{ApiModel.apiVersion}/entity/getPrimaryEntityValuesBySparsePrimaries", content, tokenDetail);
            var entityresponse = JsonConvert.DeserializeObject<EntityResponseObject>(readResult);

            year.AddRange(entityresponse.response);

            var alldates = new List<string>();
            var allDatesDateFormat = new List<DateTime>();
            foreach (var date in entityresponse.response)
            {
                var dates = date.Split(new string[] { "->" }, StringSplitOptions.None);
                foreach (var item in dates)
                {
                    DateTime datetoadd;
                    if (DateTime.TryParse(item, out datetoadd))
                        allDatesDateFormat.Add(datetoadd);
                }
            }

            if (allDatesDateFormat.Count > 0)
            {
                var minYear = allDatesDateFormat.Min().Year;
                var maxYear = allDatesDateFormat.Max().Year;
                for (var i = Convert.ToInt32(minYear); i <= Convert.ToInt32(maxYear); i++)
                {
                    var date = i.ToString();
                    alldates.Add(date);
                }
            }
            return alldates.Distinct().ToList();
        }

        public List<Vehicle> getVehicles(string make, string model, string litre, int year)
        {
            List<Entity> respondedVehicles = new List<Entity>();
            List<Vehicle> vechilelist = new List<Vehicle>();
            try
            {
                var tokenDetail = getAuthenticationToken();

                EntityRequestObject content = new EntityRequestObject()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    requestedPrimary = 2,
                    primaryValues = new List<string>() { make, model }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getEntitiesByCascadingPrimaries", content, tokenDetail);
                //respondedVehicles = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult).entities;
                var entities = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult);

                // now filter the entity reponse by the year/litre selected. This will be the proper list of entities
                DateTime dateSelected = new DateTime(year, 1, 1);

                foreach (var entity in entities.entities)
                {
                    // check years for this entity
                    if (checkYear(year, entity))
                    {
                        // check if the litre matches
                        if (entity?.attributes?.enginesize?.ToLower() == litre.ToLower() )
                        {
                            // add the entity
                            respondedVehicles.Add(entity);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return vechilelist;
            }
            return GetVehiclefromResponse(respondedVehicles);
        }

        public List<Vehicle> getVehiclesByEngineCode(string enginecode)
        {
            List<Entity> respondedVehicles = new List<Entity>();
            List<Vehicle> vechilelist = new List<Vehicle>();
            try
            {
                var tokenDetail = getAuthenticationToken();

                EntityRequestObject content = new EntityRequestObject()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    requestedPrimary = 5,
                    primaryValues = new List<string>() { enginecode }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getEntitiesBySinglePrimary", content, tokenDetail);
                //respondedVehicles = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult).entities;
                 respondedVehicles = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult).entities;

            }
            catch (Exception ex)
            {
                return vechilelist;
            }
            return GetVehiclefromResponse(respondedVehicles);
        }

        public Dictionary<string, string> getVehiclesByVrm(string Vrm)
        {
            var response = new Dictionary<string, string>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                VrmRequest content = new VrmRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    account = "orchidjet",
                    userName = "ecat-orchidjet",
                    vrm = Vrm
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getVehicleByVRM", content, tokenDetail);
                var entityresponse = JsonConvert.DeserializeObject<VrmResponse>(readresult);
                response = entityresponse.vrmDetails;
            }
            catch (Exception ex)
            {
                return response;
            }
            return response;
        }

        public List<Vehicle> getVehicleByExternalId(string extId)
        {
            List<Vehicle> vechilelist = new List<Vehicle>();
            List<Entity> respondedVehicles = new List<Entity>();
            List<ReferenceEntityTextRef> externalid = new List<ReferenceEntityTextRef>();
            externalid.Add(new ReferenceEntityTextRef { name = "", value = extId });
            try
            {
                var tokenDetail = getAuthenticationToken();
                EntityByExternalIdsRequest content = new EntityByExternalIdsRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    references = externalid
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/entity/getEntitiesByExternalReferences", content, tokenDetail);
                respondedVehicles = JsonConvert.DeserializeObject<EntityVehicleResponeObject>(readresult).entities;
            }
            catch (Exception ex)
            {
                return vechilelist;
            }
            return GetVehiclefromResponse(respondedVehicles);
        }

        public ApplicationResponse getApplByInternalEntityRef(long intId)
        {
            ApplicationResponse applicationResponse = new ApplicationResponse();
            try
            {
                var tokenDetail = getAuthenticationToken();
                ApplicationRequest content = new ApplicationRequest
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    indexReference = 1,
                    internalEntityReference = intId,
                    searchProducts = new Searchproducts
                    {
                        partNumber = "",
                        barcode = "",
                        externalReference = "",
                        brand = "",
                        productReference = 0,
                        productTypeReference = 0,
                        searchOption = "",
                        sku = ""
                    }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/application/getApplicationsByInternalEntityReference", content, tokenDetail);
                applicationResponse = JsonConvert.DeserializeObject<ApplicationResponse>(readresult);
                return applicationResponse;
            }
            catch (Exception ex)
            {
                return applicationResponse;
            }
        }

        internal List<Product> getProductsByPartNumbers(string partNumber)
        {
            List<Product> product = new List<Product>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                ProductRequest content = new ProductRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    searchProducts = new List<Searchproduct>{
                        new Searchproduct
                        {
                            partNumber = partNumber,
                            barcode = "",
                            externalReference = "",
                            brand = "",
                            productReference = 0,
                            productTypeReference = 0,
                            searchOption = ""
                        }
                    }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/product/getProductsByPartNumbers", content, tokenDetail);
                product = JsonConvert.DeserializeObject<ProductResponse>(readresult).products;
            }
            catch (Exception ex)
            {
                return product;
            }
            return product;
        }

        public List<Product> getProductsByCrossReference(string CrossRef)
        {
            List<Product> products = new List<Product>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                ProductRequest content = new ProductRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    searchProducts = new List<Searchproduct>
                    {
                        new Searchproduct
                        {
                            partNumber = "",
                            barcode = "",
                            externalReference = "",
                            brand = "",
                            productReference = 0,
                            crossReference=CrossRef,
                            productTypeReference = 0,
                            searchOption = "startswith"
                        }
                    }
                };
                //var searializer = new JavaScriptSerializer();
                //searializer.MaxJsonLength = Int32.MaxValue;
                var readresult = CallApi($"/{ApiModel.apiVersion}/product/getProductsByCrossReference", content, tokenDetail);
                products = JsonConvert.DeserializeObject<ProductResponse>(readresult).products.Where(x => x.visibility.ecat == true).ToList();
            }
            catch (Exception ex)
            {
                return products;
            }
            return products;
        }

        public List<Product> getProductsByPartNumbersFilter(string partnumber)
        {
            List<Product> products = new List<Product>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                ProductRequest content = new ProductRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    searchProducts = new List<Searchproduct>
                    {
                        new Searchproduct
                        {
                            partNumber = partnumber,
                            barcode = "",
                            externalReference = "",
                            brand = "",
                            productReference = 0,
                            crossReference="",
                            productTypeReference = 0,
                            searchOption = "startswith"
                        }
                    }
                };
                //var searializer = new JavaScriptSerializer();
                //searializer.MaxJsonLength = Int32.MaxValue;
                var readresult = CallApi($"/{ApiModel.apiVersion}/product/getProductsByPartNumbers", content, tokenDetail);
                products = JsonConvert.DeserializeObject<ProductResponse>(readresult).products.Where(x => x.visibility.ecat == true).ToList();
            }
            catch (Exception ex)
            {
                return products;
            }
            return products;
        }

        public List<ProductCategory> getProductCategories()
        {
            List<ProductCategory> categories = new List<ProductCategory>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                SimpleRequest content = new SimpleRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    }

                };
                //var searializer = new JavaScriptSerializer();
                //searializer.MaxJsonLength = Int32.MaxValue;
                var readresult = CallApi($"/{ApiModel.apiVersion}/product/getProductCategories", content, tokenDetail);
                categories = JsonConvert.DeserializeObject<ProductCategoryResponse>(readresult).categories;
            }
            catch (Exception ex)
            {
                return categories;
            }
            return categories;
        }

        public List<ProductType> getProductTypesByCategory(string productCategoryName)
        {
            List<ProductType> productTypes = new List<ProductType>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                ProductTypeRequest content = new ProductTypeRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    includeAvailableCriteria = true,
                    productCategory = productCategoryName,
                    productCategoryReference = 0
                };
                //var searializer = new JavaScriptSerializer();
                //searializer.MaxJsonLength = Int32.MaxValue;
                var readresult = CallApi($"/{ApiModel.apiVersion}/product/getProductTypesByCategory", content, tokenDetail);
                productTypes = JsonConvert.DeserializeObject<ProductTypesResponse>(readresult).productTypes;
            }
            catch (Exception ex)
            {
                return productTypes;
            }
            return productTypes;
        }

        internal List<AssetDescriptor> getProductAssets(string partNumber)
        {
            List<AssetDescriptor> assets = new List<AssetDescriptor>();
            try
            {
                var tokenDetail = getAuthenticationToken();
                AssetRequest content = new AssetRequest()
                {
                    baseRequest = new Baserequest
                    {
                        requestReference = 0,
                        requestPostedTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        system = "ecat"
                    },
                    brandID = int.Parse(ConfigurationManager.AppSettings["asset:BrandID"].ToString()),
                    searchProductAssets = new List<Searchproductasset>
                    {
                            new Searchproductasset
                            {
                                assetID=0,
                                externalAssetID="",
                                partNumber = partNumber,
                                brand="",
                                searchOption="",
                                assetType="",
                                mediaType="",
                                exportSet=ConfigurationManager.AppSettings["asset:exportSetPdf"]
                            },
                            new Searchproductasset
                            {
                                assetID=0,
                                externalAssetID="",
                                partNumber = partNumber,
                                brand="",
                                searchOption="",
                                assetType="",
                                mediaType="",
                                exportSet=ConfigurationManager.AppSettings["asset:exportSet"]
                            }
                    }
                };
                var readresult = CallApi($"/{ApiModel.apiVersion}/asset/getProductAssets", content, tokenDetail);
                assets = JsonConvert.DeserializeObject<AssetResponse>(readresult).assetDescriptors.ToList();
                    //.Where(c => c.tags.Contains("destination-ecat")).ToList();
            }
            catch (Exception ex)
            {
                return assets;
            }
            return assets;
        }

        public List<Vehicle> GetVehiclefromResponse(List<Entity> respondedVehicles)
        {
            List<Vehicle> vechilelist = new List<Vehicle>();
            foreach (var vehi in respondedVehicles)
            {
                Vehicle vehic;
                try
                {
                    vehic = new Vehicle
                    {
                        Enginecode = vehi.attributes.enginecode == null ? "" : vehi.attributes.enginecode,
                        Body = vehi.attributes.bodytype == null ? "" : vehi.attributes.bodytype,
                        Bhp = vehi.attributes.bhp == null ? "" : vehi.attributes.bhp,
                        CC = vehi.attributes.exactenginesize == null ? "" : vehi.attributes.exactenginesize,
                        Valves = vehi.attributes.enginenumberofvalves == null ? "" : vehi.attributes.enginenumberofvalves,
                        DateFrom = vehi.attributes.datefrom == null ? "" : vehi.attributes.datefrom,
                        DateTo = vehi.attributes.dateto == null ? "" : vehi.attributes.dateto,
                        EngineSize = vehi.attributes.enginesize == null ? "" : vehi.attributes.enginesize,
                        Mark = vehi.attributes.mark == null ? "" : vehi.attributes.mark,
                        FuelType = vehi.attributes.fueltype == null ? "" : vehi.attributes.fueltype,
                        Make = vehi.attributes.make == null ? "" : vehi.attributes.make,
                        Model = vehi.attributes.model == null ? "" : vehi.attributes.model.Replace(vehi.attributes.bodytype==null?"": vehi.attributes.bodytype, ""),
                        IntId = vehi.internalReference.ToString(),
                        ExtId = vehi.externalReference,
                        Cam = vehi.attributes.enginecamshafttype == null ? "" : vehi.attributes.enginecamshafttype,

                        Trim = vehi.attributes.trim == null ? "" : vehi.attributes.trim,
                        VehicleType = vehi.attributes.vehicletype == null ? "" : vehi.attributes.vehicletype,
                        NumberOfCylinder = vehi.attributes.enginenumberofcylinders == null ? "" : vehi.attributes.enginenumberofcylinders,
                        DriveType = vehi.attributes.drivetype == null ? "" : vehi.attributes.drivetype,
                        TransmissionType = vehi.attributes.transmissiontype == null ? "" : vehi.attributes.transmissiontype,
                        Kw = vehi.attributes.kw == null ? "" : vehi.attributes.kw,
                        HP = vehi.attributes.ps
                    };
                    vehic.DateRange = vehic.DateFrom;
                    var dates = vehic.DateFrom.Split(new string[] { "->" }, StringSplitOptions.None);
                    DateTime datetoadd;
                    DateTime.TryParse(dates[0], out datetoadd);
                    vehic.fromYearint = datetoadd.Year.ToString();
                    vehic.fromMonthint = datetoadd.Month.ToString();
                    DateTime datetoaddend;
                    DateTime.TryParse(vehic.DateTo, out datetoaddend);
                    if (vehic.DateTo != "")
                    {
                        vehic.toYearint = datetoaddend.Year.ToString();
                        vehic.toMonthint = datetoaddend.Month.ToString();
                    }
                }
                catch (Exception ex)
                {
                    vehic = new Vehicle();
                }
                vechilelist.Add(vehic);
            }
            var lst = vechilelist.GroupBy(c => new
            {
                c.Make,
                c.Model,
                c.Mark,
                c.FuelType,
                c.Enginecode,
                c.CC,
                c.EngineSize,
                c.Body,
                c.Valves,
                c.Bhp,
                c.Cam,
                c.fromMonthint,
                c.fromYearint,
                c.toMonthint,
                c.toYearint,
                c.Kw,
                c.HP,
                c.DateRange,
                c.TransmissionType,
                c.Trim
            }
            ).Select(grp =>
            new Vehicle
            {
                Make = grp.Key.Make,
                Model = grp.Key.Model,
                Mark = grp.Key.Mark,
                FuelType = grp.Key.FuelType,
                Enginecode = grp.Key.Enginecode,
                CC = grp.Key.CC,
                EngineSize = grp.Key.EngineSize,
                Body = grp.Key.Body,
                Valves = grp.Key.Valves,
                Bhp = grp.Key.Bhp,
                Cam = grp.Key.Cam,
                fromMonthint = grp.Key.fromMonthint,
                fromYearint = grp.Key.fromYearint,
                toMonthint = grp.Key.toMonthint,
                toYearint = grp.Key.toYearint,
                IntId = grp.ToList().FirstOrDefault().IntId,
                Kw = grp.Key.Kw,
                HP = grp.Key.HP,
                DateRange = grp.Key.DateRange,
                TransmissionType = grp.Key.TransmissionType,
                Trim=grp.Key.Trim
            }
            ).ToList();
            lst = lst.OrderBy(c => c.Make).ThenBy(c => c.Model).ThenBy(c => c.EngineSize).ThenBy(c => c.Trim).ThenBy(c => c.DateRange).ThenBy(c => c.CC).ThenBy(c => c.FuelType).ThenBy(c => c.Kw).ThenBy(c => c.HP).ToList();
            return lst;
        }

        private Token getAuthenticationToken(Boolean forceTokenRefresh = false)
        {
            var token = new Token();
            
            if (MemoryCache.Default.Get("identityToken") != null && !forceTokenRefresh)
            {
                Token identityToken = (Token)MemoryCache.Default.Get("identityToken");

                return identityToken;
            }
            else
            {
                try
                {
                    string apiClientId = ConfigurationManager.AppSettings["identity:id"].ToString();
                    string apiSecret = ConfigurationManager.AppSettings["identity:key"].ToString();
                    string apiAccessUrl = ConfigurationManager.AppSettings["identity:url"].ToString();

                    RestClient restClient = new RestClient(apiAccessUrl);
                    var request = new RestRequest("/token") { Method = Method.POST };
                    request.Parameters.Clear();
                    request.RequestFormat = DataFormat.Json;
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    request.AddParameter("application/x-www-form-urlencoded", $"client_id={apiClientId}&client_secret={apiSecret}&grant_type={"client_credentials"}", ParameterType.RequestBody);
                    var response = restClient.Execute(request);

                    if (response.IsSuccessful)
                    {
                        var readResult = response.Content;
                        var identityToken = JsonConvert.DeserializeObject<Token>(readResult);

                        if (identityToken != null)
                        {
                            string accessToken = identityToken.access_token; //?? throw new Exception();


                            int seconds = 3600; // 1 hour
                            long tokenExpires = identityToken.expires_in > seconds ? (identityToken.expires_in - seconds) : identityToken.expires_in; 

                            MemoryCache.Default.Remove("identityToken");
                            MemoryCache.Default.Add("identityToken", identityToken, DateTimeOffset.FromUnixTimeSeconds(tokenExpires));//#debug UNIXTIMESECONDS??
                            return identityToken;
                        }
                        else

                            return token;
                    }
                    else

                        return token;

                }
                catch (Exception)
                {
                    return token;
                }
            }
        }

        //this checks if an entity is valid based on a given year (i.e. if i give a year does it fit within the dates on the entity)
        private Boolean checkYear(int year, Entity entity)
        {
            bool yearPassed = false;
            var date = entity.attributes.datefrom.Split(new string[] { "->" }, StringSplitOptions.None).Select(c => c.TrimStart().TrimEnd()).ToArray();

            // if this count is not 2 something has gone wrong
            if (date.Count() == 2)
            {
                DateTime tempStartDate = DateTime.MinValue;
                DateTime tempEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                DateTime.TryParse(string.IsNullOrEmpty(date[0]) ? tempStartDate.ToString() : date[0], out tempStartDate);
                DateTime.TryParse(string.IsNullOrEmpty(date[1]) ? tempEndDate.ToString() : date[1], out tempEndDate);

                if ((tempStartDate <= new DateTime(year, 12, 31) && tempEndDate >= new DateTime(year, 1, 1)))
                    return true;
            }

            return yearPassed;
        }

    }
}