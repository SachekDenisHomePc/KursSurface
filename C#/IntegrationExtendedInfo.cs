using System;
using System.Collections.Generic;

namespace KursSurface
{
    public class IntegrationExtendedInfo
    {
        public double IntegrationResult { get; set; }
        public Dictionary<int, TimeSpan> ThreadsTime { get; set; }
    }
}