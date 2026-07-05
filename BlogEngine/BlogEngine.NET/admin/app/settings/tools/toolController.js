angular.module('blogAdmin').controller('ToolsController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.items = [];
    $scope.security = $rootScope.security;
    $scope.userIdentity = {};

    $scope.load = function () {
        if (!$scope.security.canManageUsers == true) {
            window.location.replace("../Account/Login.aspx");
        }
        spinOn();
        dataService.getItems('/api/tools/check1')
        .then(function (response) {
            var data = response.data;
            $scope.userIdentity = data.replace(/["']/g, "");
            spinOff();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.errorLoadingUsers);
            spinOff();
        });
    }

    $scope.check = function () {
        if (!$scope.security.canManageUsers == true) {
            window.location.replace("../Account/Login.aspx");
        }
        $("#msgList").empty();

        dataService.getItems('/api/tools/trust')
            .then(function (response) { $scope.addMsg(response.data); })
            .catch(function (data) { toastr.error($rootScope.lbl.Error); });

        dataService.getItems('/api/tools/data')
            .then(function (response) { $scope.addMsg(response.data); })
            .catch(function (data) { toastr.error($rootScope.lbl.Error); });

        dataService.getItems('/api/tools/root')
            .then(function (response) { $scope.addMsg(response.data); })
            .catch(function (data) { toastr.error($rootScope.lbl.Error); });

        dataService.getItems('/api/tools/Custom')
            .then(function (response) { $scope.addMsg(response.data); })
            .catch(function () { toastr.error($rootScope.lbl.Error); });
    }

    $scope.addMsg = function (data) {
        if (data.success === true) {
            $("#msgList").append('<div class="alert alert-dismissable alert-success"><span><i class="fa fa-check-circle"></i> ' + data.msg + '</span></div>');
        }
        else {
            $("#msgList").append('<div class="alert alert-dismissable alert-danger"><span><i class="fa fa-exclamation-triangle"></i> ' + data.msg + '</span></div>');
        }
    }

    $scope.load();
}]);