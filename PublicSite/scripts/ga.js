/* Google Analytics tracking code */

var liveGA4TrackingID = '';
var devGA4TrackingID = '';


var hostname = document.location.hostname;
//console.log("hostname: " + hostname);

if (hostname !== "") {// only execute GA code if server isn't localhost, prevents calls from cool2go local instances
	//new GA code that works with site search
	var _gaq = _gaq || [];
	var tempGA4ID = '';
	
	if(hostname == 'skillbridge.osd.mil')
	{
		tempGA4ID = liveGA4TrackingID;
	}
	else if(hostname == '')	// Dev site tracking domain or ip
	{
		tempGA4ID = devGA4TrackingID;
	}

	// GA4
	(function() {
		var ga = document.createElement('script');
		ga.type = 'text/javascript';
		ga.async = true;
		ga.src = 'https://www.googletagmanager.com/gtag/js?id=' + tempGA4ID;
		var s = document.getElementsByTagName('script')[0];
		s.parentNode.insertBefore(ga, s);
	})();
	
	window.dataLayer = window.dataLayer || [];
	function gtag(){dataLayer.push(arguments);}
	gtag('js', new Date());

	gtag('config', tempGA4ID);	
}