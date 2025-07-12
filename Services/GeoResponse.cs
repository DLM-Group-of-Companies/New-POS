namespace NLI_POS.Services
{
    public class GeoResponse
    {
        public string Query { get; set; } // IP
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Timezone { get; set; }
    }

}
