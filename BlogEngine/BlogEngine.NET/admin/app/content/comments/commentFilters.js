
angular.module('blogAdmin').controller('CommentFilterController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.items = [];
    $scope.editItem = {};
    $scope.modalTitle = $rootScope.lbl.add;
    $scope.focusInput = false;

    $scope.load = function (callback) {
        dataService.getItems('/api/commentfilter', { take: 0, skip: 0 })
        .then(function (response) {
            angular.copy(response.data, $scope.items);
            gridInit($scope, $filter);
            $('#txtFilter').val('');
            $('#txtFilter').focus();
            spinOff();
            callback;
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
        });
    }

    $scope.addFilter = function () {
        if ($('#txtFilter').val().length == 0) {
            toastr.error($rootScope.lbl.required);
            return false;
        }
        $scope.editItem.Action = $('#ddAction').val();
        $scope.editItem.Subject = $('#ddSubject').val();
        $scope.editItem.Operation = $('#ddOperator').val();
        $scope.editItem.Filter = $('#txtFilter').val();

        spinOn();
        dataService.addItem("/api/commentfilter", $scope.editItem)
        .then(function (response) {
            toastr.success($rootScope.lbl.completed);
            $scope.load();
            spinOff();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
            spinOff();
        });
    }

    $scope.deleteAll = function (itemsChecked) {
        if (itemsChecked) {
            spinOn();
            dataService.updateItem("/api/commentfilter/deleteall/foo", $scope.editItem)
            .then(function (response) {
                toastr.success($rootScope.lbl.completed);
                $scope.load();
                spinOff();
            })
            .catch(function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
        }
    }

    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/commentfilter/processchecked/", action, $scope, dataService);
        }
    }

    $scope.load();

    $(document).ready(function () {
        bindCommon();
    });
}]);