namespace CLSF_Compare.Models
{
    public class BOIRate
    {
        public string SERIES_CODE { get; set; }
        public string FREQ { get; set; }
        public string BASE_CURRENCY { get; set; }
        public string COUNTER_CURRENCY { get; set; }
        public string UNIT_MEASURE { get; set; }
        public string DATA_TYPE { get; set; }
        public string DATA_SOURCE { get; set; }
        public string TIME_COLLECT { get; set; }
        public string CONF_STATUS { get; set; }
        public string PUB_WEBSITE { get; set; }
        public int UNIT_MULT { get; set; }
        public string COMMENTS { get; set; }
        public DateTime TIME_PERIOD { get; set; }
        public decimal OBS_VALUE { get; set; }
    }
}
