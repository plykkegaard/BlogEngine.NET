/*!
 * Native Cookie Manager - Replacement for jQuery.Cookie
 * Provides backward-compatible API using native browser cookie APIs
 * No external dependencies beyond jQuery for API surface
 */
(function ($) {
    'use strict';

    // Helper function to encode cookie values
    function encode(s) {
        return config.raw ? s : encodeURIComponent(s);
    }

    // Helper function to decode cookie values
    function decode(s) {
        return config.raw ? s : decodeURIComponent(s);
    }

    // Helper function to stringify cookie value
    function stringifyCookieValue(value) {
        return encode(config.json ? JSON.stringify(value) : String(value));
    }

    // Helper function to parse cookie value
    function parseCookieValue(s) {
        // Handle quoted cookies (RFC2068 format)
        if (s.indexOf('"') === 0) {
            s = s.slice(1, -1).replace(/\\"/g, '"').replace(/\\\\/g, '\\');
        }

        try {
            // Replace server-side written pluses with spaces
            s = decodeURIComponent(s.replace(/\+/g, ' '));
            return config.json ? JSON.parse(s) : s;
        } catch (e) {
            // Ignore invalid cookies
            return undefined;
        }
    }

    // Main cookie function
    var config = $.cookie = function (key, value, options) {

        // Write cookie
        if (arguments.length > 1 && !$.isFunction(value)) {
            options = $.extend({}, config.defaults, options);

            // Handle numeric expires as days
            if (typeof options.expires === 'number') {
                var days = options.expires;
                var t = options.expires = new Date();
                t.setTime(+t + days * 864e+5);
            }

            // Build cookie string
            return (document.cookie = [
                encode(key), '=', stringifyCookieValue(value),
                options.expires ? '; expires=' + options.expires.toUTCString() : '',
                options.path ? '; path=' + options.path : '',
                options.domain ? '; domain=' + options.domain : '',
                options.secure ? '; secure' : ''
            ].join(''));
        }

        // Read cookie(s)
        var result = key ? undefined : {};
        var cookies = document.cookie ? document.cookie.split('; ') : [];

        for (var i = 0, l = cookies.length; i < l; i++) {
            var parts = cookies[i].split('=');
            var name = decode(parts.shift());
            var cookie = parts.join('=');

            if (key && key === name) {
                // If second argument (value) is a function, it's a converter
                result = config.raw ? cookie : parseCookieValue(cookie);
                if ($.isFunction(value)) {
                    result = value(result);
                }
                break;
            }

            // Store all cookies when no key specified
            if (!key) {
                var parsed = config.raw ? cookie : parseCookieValue(cookie);
                if (parsed !== undefined) {
                    result[name] = parsed;
                }
            }
        }

        return result;
    };

    // Default configuration
    config.defaults = {};

    // Remove cookie function
    $.removeCookie = function (key, options) {
        if ($.cookie(key) === undefined) {
            return false;
        }

        // Set cookie with past expiration date to delete it
        $.cookie(key, '', $.extend({}, options, { expires: -1 }));
        return !$.cookie(key);
    };

})(jQuery);
