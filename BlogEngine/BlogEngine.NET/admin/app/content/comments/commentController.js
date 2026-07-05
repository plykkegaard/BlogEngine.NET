angular.module('blogAdmin').controller('CommentListController', ["$rootScope", "$scope", "$location", "$filter", "$log", "dataService", function ($rootScope, $scope, $location, $filter, $log, dataService) {
    $scope.vm = {};
    $scope.items = [];
    $scope.filter = ($location.search()).fltr;
    $scope.sortingOrder = 'DateCreated';
    $scope.reverse = true;
    $scope.commentsPage = true;
    $scope.focusInput = false;

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/comments')
        .then(function (response) {
            var data = response.data;
            angular.copy(data, $scope.vm);
            $scope.items = $scope.vm.Items;
            gridInit($scope, $filter);

            if ($scope.filter == 'apr') {
                $scope.gridFilter('IsApproved', true, 'apr');
            }
            if ($scope.filter == 'pnd') {
                $scope.gridFilter('IsPending', true, 'pnd');
            }
            if ($scope.filter == 'spm') {
                $scope.gridFilter('IsSpam', true, 'spm');
            }
            spinOff();
        })
        .catch(function (response) {
            toastr.error($rootScope.lbl.failed);
            spinOff();
        });
    }

    $scope.showEditForm = function (id) {
        $scope.vm.SelectedItem = findInArray($scope.items, 'Id', id);
        dataService.getItems("/api/comments/" + id)
        .then(function (response) {
            var data = response.data;
            angular.copy(data, $scope.vm.Detail);
            $("#modal-comment-edit").modal();
            $scope.focusInput = true;
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
        });
    }

    $scope.reply = function (parentId, postId) {
        var comment = {
            "ParentId": parentId,
            "PostId": postId,
            "Content": $scope.commentReply.text
        }
        dataService.addItem("/api/comments", comment)
        .then(function (response) {
            toastr.success($rootScope.lbl.commentUpdated);
            $scope.load();
            $("#modal-comment-edit").modal('hide');
        })
        .catch(function () {
            toastr.error($rootScope.lbl.updateFailed);
            $("#modal-comment-edit").modal('hide');
        });
    }

    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/comments/processchecked/", action, $scope, dataService);
        }
	}

	$scope.deleteAll = function () {
		if ($scope.filter) {
			spinOn();
			var url = "/api/comments/DeleteAll/spam";

			if ($scope.filter === "pnd") {
				url = "/api/comments/DeleteAll/pending";
			}
			dataService.updateItem(url, { item: $scope.item })
			.then(function (response) {
				toastr.success($rootScope.lbl.commentsDeleted);
				$scope.load();
				spinOff();
			})
			.catch(function () { toastr.error($rootScope.lbl.failed); spinOff(); });
		}
	}

    $(document).ready(function () {
        bindCommon();
    });

    $scope.load();
}]);