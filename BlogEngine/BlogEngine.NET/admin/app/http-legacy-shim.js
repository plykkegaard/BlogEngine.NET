// AngularJS 1.6+ compatibility shim
// Restores the deprecated .success() and .error() promise methods
angular.module('blogAdmin').config(['$provide', function($provide) {
    $provide.decorator('$http', ['$delegate', function($delegate) {
        // Helper to add legacy methods to a promise
        function addLegacyMethods(promise) {
            promise.success = function(fn) {
                promise.then(function(response) {
                    fn(response.data, response.status, response.headers, response.config);
                });
                return promise;
            };

            promise.error = function(fn) {
                promise.then(null, function(response) {
                    fn(response.data, response.status, response.headers, response.config);
                });
                return promise;
            };

            return promise;
        }

        // Decorate each HTTP method
        var methods = ['get', 'delete', 'head', 'jsonp', 'post', 'put', 'patch'];

        methods.forEach(function(method) {
            var original = $delegate[method];
            $delegate[method] = function() {
                return addLegacyMethods(original.apply($delegate, arguments));
            };
        });

        return $delegate;
    }]);
}]);

console.log('HTTP legacy shim loaded');
