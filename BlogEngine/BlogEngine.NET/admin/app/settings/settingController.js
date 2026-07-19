angular.module('blogAdmin').controller('SettingsController', ["$rootScope", "$scope", "$location", "$log", "$http", "dataService", function ($rootScope, $scope, $location, $log, $http, dataService) {
    $scope.vm = {};
    $scope.settings = {};
    $scope.lookups = {};
    $scope.UserVars = UserVars;
    $scope.SiteVars = SiteVars;
    $scope.selfRegistrationInitialRole = {};
    $scope.ServerTime = moment(ServerTime).format("YYYY-MM-DD HH:mm");
    $scope.UtcTime = moment(UtcTime).format("YYYY-MM-DD HH:mm");
    $scope.moderationEnabled = 0;
    $scope.commentsProvider = 0;
    $scope.timeZoneOptions = [];

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/lookups')
        .then(function (response) {
            angular.copy(response.data, $scope.lookups);
            $scope.initializeGeoOptions();
            $scope.loadSettings();
        })
        .catch(function (error) {
            toastr.error($rootScope.lbl.errorLoadingSettings);
            spinOff();
        });
    }

    $scope.initializeGeoOptions = function () {
        // Initialize GEO optimization mode options
        $scope.geoOptimizationModeOptions = [
            { OptionName: 'Basic', OptionValue: 'Basic' },
            { OptionName: 'Advanced', OptionValue: 'Advanced' }
        ];

        // Initialize GEO metadata richness options
        $scope.geoMetadataRichnessOptions = [
            { OptionName: 'Minimal', OptionValue: 'Minimal' },
            { OptionName: 'Standard', OptionValue: 'Standard' },
            { OptionName: 'Rich', OptionValue: 'Rich' }
        ];
    }

    $scope.loadSettings = function () {
        dataService.getItems('/api/settings')
        .then(function (response) {
            var data = response.data;
            angular.copy(data, $scope.vm);
            $scope.settings = $scope.vm.Settings;
            $scope.timeZoneOptions = $scope.vm.TimeZones;

            // Initialize inLanguage options for Schema.org markup
            $scope.inLanguageOptions = $scope.vm.InLanguageOptions;

            $scope.selectedLanguage = selectedOption($scope.lookups.Cultures, $scope.settings.Culture);
            $scope.selectedDeskTheme = selectedOption($scope.lookups.InstalledThemes, $scope.settings.DesktopTheme);
            $scope.selfRegistrationInitialRole = selectedOption($scope.lookups.SelfRegisterRoles, $scope.settings.SelfRegistrationInitialRole);
            $scope.selFeedFormat = selectedOption($scope.vm.FeedOptions, $scope.settings.SyndicationFormat);
            $scope.selCloseDays = selectedOption($scope.vm.CloseDaysOptions, $scope.settings.DaysCommentsAreEnabled);
            $scope.selCommentsPerPage = selectedOption($scope.vm.CommentsPerPageOptions, $scope.settings.CommentsPerPage);
            $scope.selTimeZone = selectedOption($scope.timeZoneOptions, $scope.settings.TimeZoneId);
            $scope.selFacebookLanguage = selectedOption($scope.vm.FacebookLanguages, $scope.settings.FacebookLanguage);

            // Initialize GEO dropdown selections
            $scope.selectedGeoOptimizationMode = selectedOption($scope.geoOptimizationModeOptions, $scope.settings.GeoOptimizationMode || 'Basic');
            $scope.selectedGeoMetadataRichness = selectedOption($scope.geoMetadataRichnessOptions, $scope.settings.GeoMetadataRichness || 'Standard');
            $scope.selectedInLanguage = selectedOption($scope.inLanguageOptions, $scope.settings.InLanguage || 'en');
            $scope.geoImage = $scope.vm.GEOImage || '~/Custom/Themes/Default/img/default.png';

            $scope.setCommentProviders($scope.settings.CommentProvider);
            spinOff();
        })
        .catch(function (error) {
            toastr.error($rootScope.lbl.errorLoadingSettings);
            spinOff();
        });
    }

    $scope.save = function () {
        if (!$('#form').valid()) {
            return false;
        }
        $scope.settings.DesktopTheme = $scope.selectedDeskTheme.OptionValue;
        $scope.settings.Culture = $scope.selectedLanguage.OptionValue;
        if ($scope.selfRegistrationInitialRole) {
            $scope.settings.SelfRegistrationInitialRole = $scope.selfRegistrationInitialRole.OptionValue;
        }
        $scope.settings.SyndicationFormat = $scope.selFeedFormat.OptionValue;
        $scope.settings.DaysCommentsAreEnabled = $scope.selCloseDays.OptionValue;
        $scope.settings.CommentsPerPage = $scope.selCommentsPerPage.OptionValue;
        $scope.settings.TimeZoneId = $scope.selTimeZone.OptionValue;
        $scope.settings.FacebookLanguage = $scope.selFacebookLanguage.OptionValue;

        // Map GEO dropdown selections
        if ($scope.selectedGeoOptimizationMode) {
            $scope.settings.GeoOptimizationMode = $scope.selectedGeoOptimizationMode.OptionValue;
        }
        if ($scope.selectedGeoMetadataRichness) {
            $scope.settings.GeoMetadataRichness = $scope.selectedGeoMetadataRichness.OptionValue;
        }
        if ($scope.selectedInLanguage) {
            $scope.settings.InLanguage = $scope.selectedInLanguage.OptionValue;
        }

        $scope.settings.txtErrorTitle = $scope.txtErrorTitle;

        dataService.updateItem("/api/settings", $scope.settings)
        .then(function (response) {
            toastr.success($rootScope.lbl.settingsUpdated);
            $scope.load();
        })
        .catch(function () { toastr.error($rootScope.lbl.updateFailed); });
    }

    $scope.exportToXml = function () {
        location.href = SiteVars.ApplicationRelativeWebRoot + 'blogml.axd';
    }

    $scope.importClickOnce = function () {
        var url = 'https://blogengine.io/clickonce/blogimporter/blog.importer.application?url=';
        url += SiteVars.AbsoluteWebRoot + '&username=' + UserVars.Name;
        location.href = url;
    }

    $scope.uploadFile = function (files) {
        spinOn();
        var fd = new FormData();
        fd.append("file", files[0]);
        dataService.uploadFile("/api/upload?action=import", fd)
        .then(function (response) {
            toastr.success($rootScope.lbl.importedFromBlogML);
            spinOff();
        })
        .catch(function (response) {
            // Extract detailed error message from server response
            var errorMessage = $rootScope.lbl.importFailed;

            if (response && response.data) {
                if (typeof response.data === 'string' && response.data.length > 0) {
                    errorMessage = response.data;
                }
                else if (response.data.Message) {
                    errorMessage = response.data.Message;
                }
                else if (response.data.ExceptionMessage) {
                    errorMessage = response.data.ExceptionMessage;
                }
            }
            else if (response && response.statusText) {
                errorMessage = response.statusText;
            }

            toastr.error(errorMessage);
            spinOff();
        });
    }

    $scope.testEmail = function () {
        dataService.updateItem("/api/settings?action=testEmail", $scope.settings)
        .then(function (response) {
            var data = response.data;
            if (data) {
                toastr.error(data);
            }
            else {
                toastr.success($rootScope.lbl.completed);
            }
        })
        .catch(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.clearCache = function () {
        dataService.updateItem("/api/settings?action=clearCache", $scope.settings)
        .then(function (response) {
            var data = response.data;
            if (data) {
                toastr.error(data);
            }
            else {
                toastr.success($rootScope.lbl.completed);
            }
        })
        .catch(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.loadTheme = function () {
        var theme = $("#selDesktopTheme option:selected").text();
        window.location.assign("#/shared/package?id=" + theme);
    }

    $scope.setCommentProviders = function (provider) {
        if (provider == '0') {
            $("#dq-provider").hide();
            $("#fb-provider").hide();
        }
        if (provider == '1') {
            $("#be-provider").hide();
            $("#fb-provider").hide();
        }
        if (provider == '2') {
            $("#be-provider").hide();
            $("#dq-provider").hide();
        }
    }

    $scope.selectProvider = function (provider) {
        if (provider == 'be') {
            $("#dq-provider").fadeOut();
            $("#fb-provider").fadeOut();
            $("#be-provider").fadeIn();
        }
        if (provider == 'dq') {
            $("#be-provider").fadeOut();
            $("#fb-provider").fadeOut();
            $("#dq-provider").fadeIn();
        }
        if (provider == 'fb') {
            $("#be-provider").fadeOut();
            $("#dq-provider").fadeOut();
            $("#fb-provider").fadeIn();
        }
    }

    $(document).ready(function () {
        $('#form').validate({
            rules: {
                txtName: { required: true },
                txtTimeOffset: { required: true, number: true },
                txtPostsPerPage: { required: true, number: true },
                txtDescriptionCharacters: { required: true, number: true },
                txtDescriptionCharactersForPosts: { required: true, number: true },
                txtRemoteFileDownloadTimeout: { required: true, number: true },
                txtRemoteMaxFileSize: { required: true, number: true },
                txtFeedAuthor: { email: true },
                txtEndorsement: { url: true },
                txtAlternateFeedUrl: { url: true },
                txtpostsPerFeed: { number: true },
                txtEmail: { email: true },
                txtSmtpServerPort: { number: true },
                txtThemeCookieName: { required: true },
                // SEO/GEO validation rules
                txtSeoCanonicalDomain: { url: false },
                txtSeoDefaultImage: { url: false },
                txtSeoOrganizationLogo: { url: false }
            }
        });
    });

    $scope.load();

    $(document).ready(function () {
        bindCommon();
    });
}]);