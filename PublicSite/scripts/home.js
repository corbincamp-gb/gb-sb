var imageWidth, imageHeight;

function adjustOverlaySizes() {
    var image = document.querySelector('.carousel-item.active .slide-image');
    
    var overlay = image.nextElementSibling;
    overlay.style.width = image.clientWidth + 'px';
    overlay.style.height = image.clientHeight + 'px';

    imageWidth = image.clientWidth + 'px';
    imageHeight = image.clientHeight + 'px';
}

function updateOverlays()
{
    var images = document.querySelectorAll('.slide-image');

    images.forEach(function(image) {
        var overlay = image.nextElementSibling;
        overlay.style.width = imageWidth;
        overlay.style.height = imageHeight;
    });
}



window.addEventListener('load', adjustOverlaySizes);
window.addEventListener('resize', adjustOverlaySizes);

$('#mainCarousel').on('slide.bs.carousel', function () {
    updateOverlays();
})

$('#mainCarousel').on('slid.bs.carousel', function () {
    updateOverlays();
})

$(document).ajaxStop(function() { 
    EnableHomeAnalyticsEvents();
});

// Google Analytics Event Tracking
function EnableHomeAnalyticsEvents()
{
    "use strict";

    // Top carousel
    $(".slide-content a").click(function(e){
        var slidetext = $(this).parent().find("p").text();
        var text = $(this).text();
        gtag('event', 'Homepage Image Carousel Button', {
			event_category: 'Homepage',
            slide_text: slidetext,
            button_text: text,
			page_url: window.location.href
		  });
    });

    // Image Carousel previous arrow
    $("#mainCarousel .carousel-control-prev").click(function(e){
        gtag('event', 'Homepage Image Carousel Previous Arrow', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // Image Carousel next arrow
    $("#mainCarousel .carousel-control-next").click(function(e){
        gtag('event', 'Homepage Image Carousel Next Arrow', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // Image Carousel previous arrow mobile
    $("#mainCarousel .carousel-control-prev-mobile").click(function(e){
        gtag('event', 'Homepage Image Carousel Previous Arrow Mobile', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // Image Carousel next arrow mobile
    $("#mainCarousel .carousel-control-next-mobile").click(function(e){
        gtag('event', 'Homepage Image Carousel Next Arrow Mobile', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // 3 Column user links
    $("#user-cta-section a").click(function(e){
        var usertype = $(this).parent().parent().parent().find("h2").text();
        var text = $(this).text();
        gtag('event', 'Homepage Usertype CTA Link', {
			event_category: 'Homepage',
            user_type: usertype,
            text: text,
			page_url: window.location.href
		  });
    });

    // Map section link
    $("#find-opportunities-content a").click(function(e){
        gtag('event', 'Homepage Find Opportunities Map Button', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // Testimonials Carousel previous arrow
    $("#testimonialsCarousel .carousel-control-prev").click(function(e){
        gtag('event', 'Homepage Testimonials Carousel Previous Arrow', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    // Testimonials Carousel next arrow
    $("#testimonialsCarousel .carousel-control-next").click(function(e){
        gtag('event', 'Homepage Testimonials Carousel Next Arrow', {
			event_category: 'Homepage',
			page_url: window.location.href
		  });
    });

    
}