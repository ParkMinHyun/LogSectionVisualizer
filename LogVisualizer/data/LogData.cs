using System;

namespace LogVisualizer {
    public class LogData
    {
        public string startLog;
        public string endLog;
        public Double interval;

        public LogData(string startLog)
        {
            this.startLog = startLog;
        }

        public bool isValid()
        {
            return (startLog != null) && (endLog != null);
        }

        public void calculateInterval()
        {
            var startDateTime = getDateTime(startLog);
            var endtDateTime = getDateTime(endLog);

            interval = Convert.ToDouble((endtDateTime - startDateTime).TotalMilliseconds);
        }

        public DateTime getDateTime(string time)
        {
            var startYYMM = time.Split(' ')[0].Split('-');
            var startHHMM = time.Split(' ')[1].Split(':');
            var startSSFF = time.Split(' ')[1].Split(':')[2].Split('.');

            return new DateTime(
                2022,
                Convert.ToInt32(startYYMM[0]),
                Convert.ToInt32(startYYMM[1]),
                Convert.ToInt32(startHHMM[0]),
                Convert.ToInt32(startHHMM[1]),
                Convert.ToInt32(startSSFF[0]),
                Convert.ToInt32(startSSFF[1]));
        }
    }
}
