using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Program_Finder
{
    class DateHelper
    {
        public static DateTime TimeFromString(string strTime)
        {
            try
            {
                if (!strTime.Contains("-") && !strTime.Contains("/"))
                    return DateTime.ParseExact(strTime, "yyyyMMdd", CultureInfo.InvariantCulture);

                return DateTime.Parse(strTime);
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(strTime, ex);
                return DateTime.MinValue;
            }
        }
     
    }
}
