var OpenWindowPlugin = {
    openWindow: function(link)
    {
    	var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
        	window.open(url, "_blank", "location=no,menubar=no,resizable=no,scrollbars=no,status=no,titlebar=no,toolbar=no,width=600,height=300");
        	document.onmouseup = null;
        }
    },
	setCookie: function(cname1, cvalue1, exdays) {
		var cname = Pointer_stringify(cname1);
		var cvalue = Pointer_stringify(cvalue1);
		var d = new Date();
		d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
		var expires = "expires="+d.toUTCString();
		document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
		console.log("log =" + cname + "-" + cvalue + "-" + exdays);
		console.log("cookie = " + document.cookie);
	},

	getCookie: function(cname) {
		var name = cname + "=";
		var ca = document.cookie.split(';');
		for(var i = 0; i < ca.length; i++) {
			var c = ca[i];
			while (c.charAt(0) == ' ') {
				c = c.substring(1);
			}
			if (c.indexOf(name) == 0) {
				return c.substring(name.length, c.length);
			}
		}
		return "";
	},reload:function(){
location.reload();

},    openNewTabWindow: function(link)
    {
    	var url = Pointer_stringify(link);
		if(url.includes("reload")) {			
			location.reload();
		} else {			
			document.onmouseup = function()
			{
				window.open(url);				
				document.onmouseup = null;
			}
		}
    }
};

mergeInto(LibraryManager.library, OpenWindowPlugin);