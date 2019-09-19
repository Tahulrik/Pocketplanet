using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Cache;


public class GetServerTime
{

	public static DateTime GetNistTime()
	{
		int oneHourInMiliseconds = 3600000;
		DateTime dateTime = DateTime.MinValue;

		HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
		request.Method = "GET";
		request.Accept = "text/html, application/xhtml+xml, */*";
		request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
		request.ContentType = "application/x-www-form-urlencoded";
		request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
		HttpWebResponse response = (HttpWebResponse)request.GetResponse();
		if (response.StatusCode == HttpStatusCode.OK)
		{
			StreamReader stream = new StreamReader(response.GetResponseStream());
			string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
			string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
			double milliseconds = Convert.ToInt64(time) / 1000.0;
			dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds + oneHourInMiliseconds);
		}

		return dateTime;
	}
}


