angular.module('blogAdmin').controller('CustomPluginsController', ["$rootScope", "$scope", "$location", "$filter", "dataService", function ($rootScope, $scope, $location, $filter, dataService) {
    $scope.items = [];
    $scope.customFields = [];
    $scope.editId = "";
    $scope.package = {};
    $scope.activeTheme = ActiveTheme;
    $scope.IsPrimary = $rootScope.SiteVars.IsPrimary === "True";
    $scope.fltr = 'extensions';
    $scope.order = 'DownloadCount desc';
    $scope.sortingOrder = 'DownloadCount';
    $scope.reverse = true;
    $scope.selectedRating = 0;
    $scope.author = UserVars.Name;

    console.log("CustomPluginsController initialized");
    console.log("Current path:", $location.path());
    console.log("IsPrimary:", $scope.IsPrimary);

    if ($location.path().indexOf("/custom/plugins/gallery") === 0) {
        console.log("Gallery context detected");
        $scope.fltr = 'all';
        $scope.galleryFilter = 'extensions';
    }

    $scope.load = function () {
        console.log("Loading packages...");
        spinOn();
        dataService.getItems('/api/packages', { take: 0, skip: 0, filter: $scope.fltr, order: 'LastUpdated desc' })
        .then(function (response) { var data = response.data;
            console.log("Packages loaded successfully. Count:", data.length);
            angular.copy(data, $scope.items);
            gridInit($scope, $filter);
            if ($scope.galleryFilter) {
                $scope.setFilter();
            }
            var pkgId = getFromQueryString('pkgId');
            if (pkgId !== null) {
                $scope.query = pkgId;
                $scope.search();
            }
            spinOff();
        })
        .catch(function (error) {
            console.error("Failed to load packages:", error);
            toastr.error($rootScope.lbl.errorLoadingPackages);
            spinOff();
        });
    }

    $scope.showPluginInfo = function (id) {
        dataService.getItems('/api/packages/' + id)
        .then(function (response) { var data = response.data;
            angular.copy(data, $scope.package);
            $scope.selectedRating = $scope.package.Rating;

            $scope.extEditSrc = SiteVars.RelativeWebRoot + "admin/Extensions/Settings.aspx?ext=" + $scope.id + "&enb=False";
            if ($scope.package.SettingsUrl) {
                $scope.extEditSrc = $scope.package.SettingsUrl.replace("~/", SiteVars.RelativeWebRoot);
            }
            $scope.removeEmptyReviews();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.errorLoadingPackages);
        });
        $("#modal-info").modal();
    }

    $scope.showPluginSettings = function (id) {
        $scope.editId = id;
        $scope.extEditSrc = SiteVars.RelativeWebRoot + "admin/Extensions/Settings.aspx?ext=" + id + "&enb=False";

        for (var i = 0, len = $scope.items.length; i < len; i++) {
            if ($scope.items[i].Id === id) {
                angular.copy($scope.items[i], $scope.package);

                if ($scope.package) {
                    if ($scope.package.SettingsUrl) {
                        $scope.extEditSrc = $scope.package.SettingsUrl.replace("~/", SiteVars.RelativeWebRoot);
                    }
                }
            }
        }
        $("#modal-settings").modal();
    }

    $scope.setFilter = function () {
        if ($scope.galleryFilter === 'extensions') {
            $scope.gridFilter('PackageType', 'Extension', 'pub');
        }
        if ($scope.galleryFilter === 'themes') {
            $scope.gridFilter('PackageType', 'Theme', 'dft');
        }
    }

    $scope.checkStar = function (item, rating) {
        if (item === rating) {
            return true;
        }
        return false;
    }

    $scope.setRating = function (rating) {
        $scope.selectedRating = rating;
    }

    $scope.submitRating = function () {
        var author = $("#txtAuthor").val().length > 0 ? $("#txtAuthor").val() : $scope.author;
        var review = { "Name": author, "Rating": $scope.selectedRating, "Body": $("#txtReview").val() };

        dataService.updateItem("/api/packages/rate/" + $scope.package.Extra.Id, review)
        .then(function (response) { var data = response.data;
            //if (data != null) {
            //    data = JSON.parse(data);
            //}
            if (data.length === 0) {
                toastr.success($rootScope.lbl.completed);
            }
            else {
                toastr.error(data);
            }
            $("#modal-info").modal("hide");
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
            $("#modal-info").modal("hide");
        });
    }

    $scope.setPriority = function (upDown) {
        if (upDown === 'up') {
            $scope.package.Priority++;
        }
        else {
            if ($scope.package.Priority > 0) {
                $scope.package.Priority--;
            }
        }
        $scope.save();
    }

    $scope.sortBy = function (ord) {
        $scope.sortingOrder = ord;
        $scope.reverse = true;
        $scope.load();
    }

    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/packages/processchecked/", action, $scope, dataService);
        }
    }

    $scope.removeEmptyReviews = function () {
        if ($scope.package.Extra !== null && $scope.package.Extra.Reviews !== null) {
            var reviews = [];
            for (var i = 0; i < $scope.package.Extra.Reviews.length; i++) {
                var review = $scope.package.Extra.Reviews[i];
                if (review.Body.length > 0) {
                    reviews.push(review);
                }
            }
            $scope.package.Extra.Reviews = reviews;
        }
    }

    $scope.installPackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/install/" + pkgId, pkgId)
        .then(function (response) { var data = response.data;
            toastr.success($rootScope.lbl.completed);
            $scope.load();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
            spinOff();
        });
    }

    $scope.uninstallPackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/uninstall/" + pkgId, pkgId)
        .then(function (response) { var data = response.data;
            toastr.success($rootScope.lbl.completed);
            $scope.load();
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
            spinOff();
        });
    }

    $scope.upgradePackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/uninstall/" + pkgId, pkgId)
        .then(function (response) { var data = response.data;
            $scope.installPackage(pkgId);
        })
        .catch(function () {
            toastr.error($rootScope.lbl.failed);
            spinOff();
        });
    }

    $scope.goToGallery = function () {
        console.log("Navigating to gallery...");
        $location.path('/custom/plugins/gallery');
    }

    $scope.load();

    $(document).ready(function () {
        bindCommon();
    });
}]);