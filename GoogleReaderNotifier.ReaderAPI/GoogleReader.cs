using System;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;

namespace GoogleReaderNotifier.ReaderAPI
{
	/// <summary>
	/// Summary description for GoogleReader.
	/// </summary>
	public class GoogleReader
	{
		#region Private variables

		private CookieCollection _Cookies = new CookieCollection();
		private CookieContainer _cookiesContainer = new CookieContainer();
		private bool _loggedIn = false;
		private int _totalCount = 0;
		private string _tagCount = "";
	
		#endregion 
		
		public int TotalCount
		{
			get{return _totalCount;}
			set{_totalCount = value;}
		}

		public string TagCount
		{
			get{return _tagCount;}
			set{_tagCount = value;}
		}

		#region Application Code

		// https://www.google.com/reader/atom/user/-/state/com.google/reading-list // get full feed of items
		public string GetDetailedCount(string username, string password, string filters)
		{
			//verify login
			if(!_loggedIn)
			{
				string loginResult = this.Login(username, password);
				if(loginResult != string.Empty)
					return loginResult;
			}

			//get the counts from google
			XmlDocument xdoc = this.GetUnreadCounts();

			
			string detailedcount = "";

			// if filters are set, then don't check the regular unread items
			if(filters.Trim().Length == 0)
			{
				this._totalCount = this.CountAllFeeds(xdoc);
			}
			else
			{
				this._totalCount = this.CountFilteredFeeds(xdoc, filters, ref detailedcount);
			}
			
			detailedcount = this._totalCount.ToString() + " unread items" + Environment.NewLine + detailedcount;
			this.TagCount = detailedcount;

			return detailedcount;
		}

        string auth_head_string;    // authorization string, got from Login() "Auth=" field.
		public string Login(string username, string password)
		{
            auth_head_string = null;
			HttpWebRequest req = CreateRequest("https://www.google.com/accounts/ClientLogin");
			PostLoginForm(req, String.Format(
                "accountType=HOSTED_OR_GOOGLE&Email={0}&Passwd={1}&service=reader&source=GRN_mod&continue=https://www.google.com/reader",
                Uri.EscapeDataString(username),
                Uri.EscapeDataString(password)));

			var resString = GetResponseString(req);
			if(resString.IndexOf("SID=") != -1)
			{
				foreach(var line in resString.Split('\n'))
				{
					if(line.StartsWith("SID="))
						_Cookies.Add(new Cookie("SID", line.Substring(4)));
                    if (line.StartsWith("LSID="))
                        _Cookies.Add(new Cookie("LSID", line.Substring(5)));
                    if (line.StartsWith("Auth="))
                        auth_head_string = line.Substring(5);
                    if (line.StartsWith("HSID="))
                        _Cookies.Add(new Cookie("HSID", line.Substring(5)));
                }

				_loggedIn = true;
				return string.Empty;
			}
			else
			{
				return "AUTH_ERROR";
			}
		}

		private XmlDocument GetUnreadCounts()
		{
			string url = "https://www.google.com/reader/api/0/unread-count?all=true";
			string theXml = GetResponseString(CreateRequest(url));

			XmlDocument xdoc = new XmlDocument();
			xdoc.LoadXml(theXml);

			return xdoc;
		}

		private int CountAllFeeds(XmlDocument xdoc)
		{
			int totalFeeds = 0;
			foreach(XmlNode node in xdoc.SelectNodes("//object/string[contains(.,'feed/http')]"))
			{
				int thenumber = Convert.ToInt32(node.ParentNode.SelectSingleNode("number").InnerText);
				totalFeeds += thenumber;
			}
			return totalFeeds;
		}

		private int CountFilteredFeeds(XmlDocument xdoc, string filters, ref string filterBreakdown)
		{
			// filters have been set, check here for just the tagged items.
			string[] filterlist = filters.Split(" ".ToCharArray());
			int totalFeeds = 0;

			foreach(XmlNode node in xdoc.SelectNodes("//object/string[contains(.,'/label/') and contains(.,'user/')]"))
			{
				string thelabel = node.InnerText.Substring(node.InnerText.LastIndexOf("/")+1);  //user/10477630455154158284/label/food

				foreach(string thefilter in filterlist)
				{
					if(thefilter == thelabel)
					{
						int thenumber = Convert.ToInt32(node.ParentNode.SelectSingleNode("number").InnerText);
						totalFeeds += thenumber;
						if(thenumber > 0)
						{
							filterBreakdown += thenumber.ToString() + " in " + thelabel + Environment.NewLine;
						}					
					}
				}
			}
			return totalFeeds;
		}
		#endregion 

		#region HTTP Functions

		private string GetResponseString(HttpWebRequest req)
		{
			string responseString = null;
			try
			{
				HttpWebResponse res = (HttpWebResponse)req.GetResponse();
				res.Cookies = req.CookieContainer.GetCookies(req.RequestUri);
				_Cookies.Add(res.Cookies);
				
				using (StreamReader read = new StreamReader(res.GetResponseStream()))
				{
					responseString = read.ReadToEnd();
				}
				res.Close();
				//_currentHTML = responseString;
				
			}
			catch (Exception ex)
			{
				string exc = ex.ToString();
			}
			return responseString;
		}

		private void PostLoginForm(HttpWebRequest req, string p)
		{
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			//req.Referer = _currentURL;
			byte[] b = Encoding.UTF8.GetBytes(p);
			req.ContentLength = b.Length;
			using (Stream s = req.GetRequestStream())
			{
				s.Write(b, 0, b.Length);
				s.Close();
			}
		}

		private HttpWebRequest CreateRequest(string url)
		{
			HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
			WebProxy defaultProxy = WebProxy.GetDefaultProxy();

			_cookiesContainer.Add(new Uri(url, UriKind.Absolute), _Cookies);

            // add request header: Authorization field, GoogleLogin auth=[login auth string]...
            if(!string.IsNullOrEmpty(auth_head_string))
                req.Headers.Add("Authorization", "GoogleLogin auth=" + auth_head_string);

			req.Proxy = defaultProxy; 
			//req.UserAgent = _user.UserAgent;
			req.CookieContainer = _cookiesContainer;
			//req.Referer = _currentURL; // set the referring url properly to appear as a regular browser
			//_currentURL = url; // set the current url so the next request will have the right referring url ( might not work for sub pages )
			return req;
		}

		#endregion 

	}
}
