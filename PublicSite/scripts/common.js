$(document).ready(function() { 
	LoadAjaxSections();

  /*$("#icon-key-dropdown").mouseover(function(){
    adjustDropdownDirection($(this)[0]);
  });*/
});

$(document).ajaxStop(function() { 
  //console.log("ajaxstop firing");
	DetermineActiveSideNavPage();  
  AdjustDropdowns(); 
  CheckForQueryStringTab();
  CheckForQueryStringSection();
  SetupSiteSearch();
  EnableCommonAnalyticsEvents();
});

function SetupSiteSearch()
{
  
  // Copy text from custom input field over to hidden google site search field
  $("#site-search").keyup(function() {
    var textValue = $(this).val();
    $("#mobile-site-search").val(textValue);
    $('#gsc-i-id1').val(textValue);
  });

  // When custom search button is clicked, fire google search
  $("#search-btn").click(function(e){
    e.preventDefault();
    $(".gsc-search-button").click();
    TriggerSiteSearchGAEvent();
  });

  // If enter is pressed on input field, fire google search
  $("#site-search").on("keydown", function(e) {
    // Check if the pressed key is the Enter key (key code 13)
    if (e.which === 13) {
      e.preventDefault();
      $(".gsc-search-button").click();
      TriggerSiteSearchGAEvent();
    }
  });

  // Copy text from custom input field over to hidden google site search field
  $("#mobile-site-search").keyup(function() {
    var textValue = $(this).val();
    $("#site-search").val(textValue);
    $('#gsc-i-id1').val(textValue);
  });

  // When custom search button is clicked, fire google search
  $("#mobile-search-btn").click(function(e){
    e.preventDefault();
    $(".gsc-search-button").click();
    TriggerSiteSearchGAEvent();
  });

  // If enter is pressed on input field, fire google search
  $("#mobile-site-search").on("keydown", function(e) {
    // Check if the pressed key is the Enter key (key code 13)
    if (e.which === 13) {
      e.preventDefault();
      $(".gsc-search-button").click();
      TriggerSiteSearchGAEvent();
    }
  });
}

// Recalculate the dropdown direction when the window is resized
window.addEventListener('resize', function() {
  AdjustDropdowns();  
});

document.addEventListener('DOMContentLoaded', function () {
  //console.log('onload')

  window.addEventListener('scroll', function () {
    //console.log('scroll')
    var element = document.getElementById('header') // Replace 'myElement' with the ID of your target element
    var scrollPosition = window.pageYOffset || document.documentElement.scrollTop
    var scrollPoint = 200 // Adjust this value to set the desired scroll point
    var content = document.getElementById('content-container')
    //console.log('window.pageYOffset: ' + window.pageYOffset)
    //console.log('document.documentElement.scrollTop: ' + document.documentElement.scrollTop)
    if (scrollPosition > scrollPoint) {
      //console.log('sticky')
      element.classList.add('sticky')

      // Get the height of header
      var headerHeight = element.offsetHeight
      // Set the top padding of content
      content.style.paddingTop = headerHeight + 10 + 'px'
    } else {
      element.classList.remove('sticky')

      // Set the top padding of content
      content.style.paddingTop = '0px'
    }
  })
})

// Load HTML partials via AJAX
function LoadAjaxSections()
{
	var headerFileURL = "includes/inc_header.htm";
	var headerSection = $("#header");
	
	var footerFileURL = "includes/inc_footer.htm";
	var footerSection = $("#footer");

  var sidenavFileURL = "includes/inc_sidenav.htm";
	var sidenavSection = $("#related-links-nav");
	
	//Get the HTML from the specified file
	$.ajax({				//Header
		url: headerFileURL,
		type: 'get',
		async: true,
		dataType: "html",
		success: function(html) {
			headerSection.html(html);
		}
	});	

	$.ajax({				//Footer
		url: footerFileURL,
		type: 'get',
		async: true,
		dataType: "html",
		success: function(html) {
			footerSection.html(html);
		}
	});	

  // If the sidenav tag exists on the page, pull the sitenav into the page
  if($("#related-links-nav").length)
  {
    //console.log("related-links-nav found");
    $.ajax({				//Sidenav
      url: sidenavFileURL,
      type: 'get',
      async: true,
      dataType: "html",
      success: function(html) {
        sidenavSection.html(html);
      }
    });	
  }
}

function DetermineActiveSideNavPage()
{
  //console.log("DetermineActiveSideNavPage");
  var url = window.location.pathname; // Get the path of the current URL
  var fileName = url.substring(url.lastIndexOf('/') + 1); // Extract the file name from the path

  $("#related-links-nav ul li").each(function(){
    var href = $(this).children("a").attr("href");

    if(href == fileName)
    {
      $(this).addClass("selected");
    }
  });
}

function AdjustDropdowns()
{
  $("#main-nav>li ul.submenu").each(function(){
    //$(this).removeClass("dropdown-reverse");
    AdjustDropdownDirection($(this)[0]);
  });

  /*$("#icon-key-dropdown").each(function(){
    //$(this).removeClass("dropdown-reverse");
    AdjustDropdownDirection($(this)[0]);
  });*/
}

function AdjustDropdownDirection(dropdownElement) {
  var dropdownRect = dropdownElement.getBoundingClientRect();
  var viewportWidth = window.innerWidth;
  var overflowOffset = dropdownRect.right - viewportWidth;

  if (overflowOffset > 0) {
    dropdownElement.style.transform = 'translateX(-' + overflowOffset + 'px)';
  } else {
    dropdownElement.style.transform = '';
  }
}

function CheckForQueryStringTab()
{
    if(getParameterByName("tab") != null)
    {
        var tabName = getParameterByName("tab");
        $("#page-tabs li button").each(function(){
            var button = $(this);
            var target = $(this).attr("data-target").toString();
            var adjustedTarget = target.split("-tab")[0];
            
            adjustedTarget = adjustedTarget.substring(1);

            if(adjustedTarget === tabName)
            {
              button.click();
            }
        });
    }
}

function CheckForQueryStringSection()
{
    if(getParameterByName("section") != null)
    {
        var sectionNum = getParameterByName("section");
        var index = 0

        $(".accordian-item-btn").each(function(){
            var button = $(this);
            index++;

            if(index == sectionNum)
            {
              button.click();
              scrollToElement(button);
            }
        });
    }
}

function scrollToElement(element) {
  var headerOffset = 215;
  $('html, body').animate({
    scrollTop: element.offset().top - headerOffset
  }, 500); // Adjust the animation speed (in milliseconds) to your preference
}

function getParameterByName(name) 
{
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
} 

function TriggerSiteSearchGAEvent()
{
    var searchTerm = $('#gsc-i-id1').val();

    gtag('event', 'Site Search', {
      event_category: 'Search',
      search_term: searchTerm,
      page_url: window.location.href
    });
}


// Google Analytics Event Tracking
function EnableCommonAnalyticsEvents()
{
    "use strict";
    
    // Logo
    $("#logo-link").click(function(e){
      gtag('event', 'Logo Link', {
        event_category: 'Header',
        page_url: window.location.href
      });
    });

    // Header Partner CMS link
    $("#top-nav-partner-link").click(function(e){
      gtag('event', 'Partner CMS Link', {
        event_category: 'Header',
        page_url: window.location.href
      });
    });

    // Header locations link
    $("#top-nav-locations-link").click(function(e){
      gtag('event', 'Locations Link', {
        event_category: 'Header',
        page_url: window.location.href
      });
    });

    // Header main navigation
    $("#main-nav-container a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Main Navigation Link', {
        event_category: 'Header',
        text: text,
        page_url: window.location.href
      });
    });

    // Header mobile partner CMS link
    $("#mobile-cms-login-link").click(function(e){
      gtag('event', 'Mobile Partner CMS Link', {
        event_category: 'Header',
        page_url: window.location.href
      });
    });

    // Header mobile locations link
    $("#mobile-locations-link").click(function(e){
      gtag('event', 'Mobile Locations Link', {
        event_category: 'Header',
        page_url: window.location.href
      });
    });

    // Header mobile main navigation
    $("#mobile-main-nav-container a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Mobile Main Navigation Link', {
        event_category: 'Header',
        text: text,
        page_url: window.location.href
      });
    });

    // Footer image links
    $("a.footer-image-link").click(function(e){
      var text = $(this).find("img").attr("alt");
      gtag('event', 'Footer Image Link', {
        event_category: 'Footer',
        text: text,
        page_url: window.location.href
      });
    });

    // Footer text links
    $("#footer-links-container a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Footer Link', {
        event_category: 'Footer',
        text: text,
        page_url: window.location.href
      });
    });

    // Legal and Admin Link
    $("#copyright a").click(function(e){
      gtag('event', 'Legal and Admin Link', {
        event_category: 'Footer',
        page_url: window.location.href
      });
    });

    // Related Links Sidenav Link
    $("#related-links-nav ul li a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Related Links Link', {
        event_category: 'Side-Navigation',
        text: text,
        page_url: window.location.href
      });
    });

    // Jump Links
    $("#jump-links ul li a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Jump Link', {
        event_category: 'Content',
        text: text,
        page_url: window.location.href
      });
    });

    // Expandable Areas
    $(".accordian-item-btn").click(function(e){
      var text = $(this).text();
      gtag('event', 'Expandable Section', {
        event_category: 'Content',
        text: text,
        page_url: window.location.href
      });
    });

    // Content-Embedded Links
    $("#content-container a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Content Link', {
        event_category: 'Content',
        text: text,
        page_url: window.location.href
      });
    });

    // Legal/Admin Side Links
    $("#legal-admin-links-nav a").click(function(e){
      var text = $(this).text();
      gtag('event', 'Legal/Admin Side Link', {
        event_category: 'Content',
        text: text,
        page_url: window.location.href
      });
    });
    
}