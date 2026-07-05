angular.module('blogAdmin').controller('ProfileController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.user = {};
    $scope.noAvatar = SiteVars.ApplicationRelativeWebRoot + "Content/images/blog/noavatar.jpg";
    $scope.photo = $scope.noAvatar;
    $scope.UserVars = UserVars;

    $scope.focusInput = false;
    $scope.customFields = [];
    $scope.editItem = {};

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/users/' + UserVars.Name)
        .then(function (response) {
            angular.copy(response.data, $scope.user);
            $scope.loadCustom();
            $scope.setPhoto();
            spinOff();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.errorLoadingUser);
            spinOff();
        });
    }

    $scope.save = function () {
        spinOn();
        dataService.updateItem("/api/users/saveprofile/item", $scope.user)
        .then(function (response) {
            toastr.success($rootScope.lbl.userUpdatedShort);
            if ($scope.customFields && $scope.customFields.length > 0) {
                $scope.updateCustom();
            }
            $scope.load();
            spinOff();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.updateFailed);
            spinOff();
        });
    }

    $scope.removePicture = function () {
        $scope.user.Profile.PhotoUrl = "";
        $scope.save();
    }

    $scope.changePicture = function (files) {
        var fd = new FormData();
        fd.append("file", files[0]);

        dataService.uploadFile("/api/upload?action=profile", fd)
        .then(function (response) {
            $scope.user.Profile.PhotoUrl = response.data;
            $scope.save();
        })
        .catch(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.setPhoto = function () {
        if ($scope.user.Profile.PhotoUrl) {
            $scope.photo = SiteVars.RelativeWebRoot + "image.axd?picture=/avatars/" +
                $scope.user.Profile.PhotoUrl + "&" + new Date().getTime();
        }
        else {
            $scope.photo = $scope.noAvatar;
        }
    }

    $scope.load();

    /* Custom fields */

    $scope.showCustom = function () {
        $("#modal-custom").modal();
        $scope.focusInput = true;
    }

    $scope.loadCustom = function () {
        $scope.customFields = [];

        dataService.getItems('/api/customfields', { filter: 'CustomType == "PROFILE"' })
        .then(function (response) {
            angular.copy(response.data, $scope.customFields);
        })
        .catch(function () {
            toastr.error($rootScope.lbl.errorLoadingCustomFields);
        });
    }

    $scope.saveCustom = function () {
        var customField = {
            "CustomType": "PROFILE",
            "Key": $("#txtKey").val(),
            "Value": $("#txtValue").val()
        };
        if (customField.Key === '') {
            toastr.error("Custom key is required");
            return false;
        }
        dataService.addItem("/api/customfields", customField)
        .then(function (response) {
            toastr.success('New item added');
            $scope.load();
            $("#modal-custom").modal('hide');
        })
        .catch(function () {
            toastr.error($rootScope.lbl.updateFailed);
            $("#modal-custom").modal('hide');
        });
    }

    $scope.updateCustom = function () {
        dataService.updateItem("/api/customfields", $scope.customFields)
        .then(function (response) {
            spinOff();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.updateFailed);
            spinOff();
        });
    }

    $scope.deleteCustom = function (key, objId) {
        var customField = {
            "CustomType": "PROFILE",
            "Key": key,
            "ObjectId": objId
        };
        spinOn();
        dataService.deleteItem("/api/customfields", customField)
        .then(function (response) {
            toastr.success("Item deleted");
            spinOff();
            $scope.load();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.couldNotDeleteItem);
            spinOff();
        });
    }
    $(document).ready(function () {
        bindCommon();
    });

}]);