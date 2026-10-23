namespace Web_SoSwim_Version1._0.Models.Domain
{
    public class SwimmerVitals
    {
        public int HeartRateBpm { get; set; } = 78;
        
        /// <summary>
        /// نسبة احتمالية عدم الاستقرار والخطر من 0 إلى 100%
        /// تُقاس من حساسات الحركة وضربات القلب
        /// لا يسمح لولي الأمر بطلب النجدة إلا إذا كانت 50% فأكثر
        /// </summary>
        public int InstabilityRiskPercentage { get; set; } = 15;

        /// <summary>
        /// مؤشر اتزان الحركة (مستقر، مضطرب، فاقد للسيطرة)
        /// </summary>
        public string MovementPattern { get; set; } = "منتظم وطبيعي";

        public double WaterTemperatureCelsius { get; set; } = 24.5;

        public double DepthMeters { get; set; } = 1.2;

        public double DistanceFromShoreMeters { get; set; } = 25.0;

        public double Latitude { get; set; } = 31.2001;

        public double Longitude { get; set; } = 29.9187;

        public int BatteryPercentage { get; set; } = 92;

        public DateTime LastSignalTimeUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// هل تم تفعيل زر الطوارئ يدوياً من جهاز السوار نفسه بواسطة السباح؟
        /// </summary>
        public bool IsEmergencyButtonPressed { get; set; } = false;
    }
}
