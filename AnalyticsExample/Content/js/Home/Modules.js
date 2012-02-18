/// <reference path="bootstrap.js" />
/// <reference path="jquery.sparkline.js" />

$(function() {
	var refreshUrl = "/widgets/refresh";
	function load(url, data, contentHolder, postLoad) {
		$.ajax({
			type: 'get',
			url: url,
			data: data,
			success: function(html) {
				contentHolder.empty().hide();
				contentHolder.html(html);
				contentHolder.slideDown();

				if (postLoad)
					postLoad.call(this);
			}
		});			
	}
	function config(url, widget, postLoad) {
		var content = $("div.content", widget);
		load(url, {}, content, postLoad);

		widget.on('click', "a[rel='refresh']", function(e) {
			e.preventDefault();
			content.slideUp(function() {
				load(refreshUrl, {url:url}, content, postLoad);
			});
		});
	}
	
	var analyticsWidget = $("#AnalyticsSummaryWidget");
	if (analyticsWidget.size()) {
		config('/widgets/AnalyticsSummary', analyticsWidget, function() {
			$('.ga_visits', analyticsWidget).sparkline(ga_visits, { type:'line', width:'100%', height:'75px', lineColor:'#999', fillColor:'#eee', spotColor:false, minSpotColor:false, maxSpotColor:false, chartRangeMin:0 });
		});
	}	
});