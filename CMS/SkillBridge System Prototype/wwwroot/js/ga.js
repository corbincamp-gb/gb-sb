/* Google Analytics tracking code */

var devSiteTrackingID = 'UA-197841733-1';			//https://skillbridgesystemprototype20210414022041.azurewebsites.net/ (azure test environnment)
var liveSiteTrackingID = 'UA-197841733-2';			//skillbridge.org

var hostname = document.location.hostname;
//console.log("hostname: " + hostname);

if (hostname !== "") {// only execute GA code if server isn't localhost, prevents calls from cool2go local instances
	//new GA code that works with site search
	var _gaq = _gaq || [];
	var tempTrackingID = '';

	if (hostname == 'skillbridge.org' || hostname == 'www.skillbridge.org')	// Live site tracking
	{
		tempTrackingID = liveSiteTrackingID;
	}

	else if (hostname == 'skillbridge-cms-test.azurewebsites.us')	// azure test environnment
	{
		tempTrackingID = devSiteTrackingID;
	}

	_gaq.push(['_setAccount', tempTrackingID]);
	_gaq.push(['_trackPageview']);

	(function () {
		var ga = document.createElement('script');
		ga.type = 'text/javascript';
		ga.async = true;
		ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
		var s = document.getElementsByTagName('script')[0];
		s.parentNode.insertBefore(ga, s);
	})();
}